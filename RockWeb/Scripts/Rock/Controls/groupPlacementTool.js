(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    /** JS helper for the groupPlacement block */
    Rock.controls.groupPlacementTool = (function () {
        var exports = {
            /** initializes the JavasSript for the groupPlacement tool */
            initialize: function (options) {
                if (!options.id) {
                    throw 'id is required';
                }

                var self = this;

                var $control = $('#' + options.id);

                if ($control.length == 0) {
                    return;
                }

                var $blockInstance = $control.closest('.block-instance')[0];
                self.$groupPlacementTool = $control;
                self.$registrantList = $('.js-group-placement-registrant-list', $control);
                self.$groupList = $('.js-placement-groups');
                self.registrationTemplatePlacementId = $('.js-registration-template-placement-id', self.$groupPlacementTool).val()

                // initialize dragula
                var containers = [];

                // add the registrant list as a dragula container
                containers.push($control.find('.js-group-placement-registrant-container')[0]);

                // add all the placement group's group roles as dragula containers
                var targets = $control.find('.js-group-role-container').toArray();

                $.each(targets, function (i) {
                    containers.push(targets[i]);
                });

                self.registrantListDrake = dragula(containers, {
                    isContainer: function (el) {
                        return false;
                    },
                    moves: function (el, source, handle, sibling) {
                        return true;
                    },
                    copy: function (el, source) {
                        return source.classList.contains('js-group-placement-registrant-container');
                    },
                    accepts: function (el, target, source, sibling) {
                        $(el).data('allow-drop', true);
                        return true;
                    },
                    invalid: function (el, handle) {
                        // ignore drag if they are clicking on the actions menu of a registrant
                        var isMenu = $(el).closest('.js-registrant-actions').length;
                        return isMenu;
                    },
                    ignoreInputTextSelection: true,

                    mirrorContainer: $blockInstance
                })
                    .on('drag', function (el) {
                        $('body').addClass('state-drag');
                    })
                    .on('dragend', function (el) {
                        $('body').removeClass('state-drag');
                    })
                    .on('drop', function (el, target, source, sibling) {
                        if (source == target) {
                            // don't do anything if a person is dragged around within the same occurrence
                            return;
                        }

                        if (target == null) {
                            // don't do anything if a person is dragged into an invalid container
                            return;
                        }

                        if ($(el).data('allow-drop') == false) {
                            // move the el back to the source container
                            $(el).detach().appendTo($(source));
                            return;
                        }

                        var $draggedItem = $(el);

                        if ($draggedItem.attr('data-has-placement-error')) {
                            // if a registrant got a placement error when dragged into group role, remove the dragged item
                            $draggedItem.remove();
                            return;
                        }

                        if ($draggedItem.hasClass('js-group-member')) {
                            // if a group member is dragged outside of its div, remove it from the assign placement
                            self.removeGroupMember($draggedItem);
                            $draggedItem.remove();
                            return;
                        }

                        var $draggedRegistrant = $draggedItem;

                        var $groupRoleMembers = $(target).closest('.js-group-role-members');
                        var $placementGroup = $groupRoleMembers.closest('.js-placement-group');

                        var registrantId = $draggedRegistrant.attr('data-registrant-id');
                        var groupId = $placementGroup.find('.js-placement-group-id').val();

                        var groupTypeRoleId = $groupRoleMembers.find('.js-grouptyperole-id').val()
                        var personId = $draggedRegistrant.attr('data-person-id');

                        var groupMember = {
                            IsSystem: false,
                            GroupId: groupId,
                            PersonId: personId,
                            GroupRoleId: groupTypeRoleId,
                            GroupMemberStatus: 1
                        }

                        var canPlaceRegistrantUrl = Rock.settings.get('baseUrl') + 'api/RegistrationTemplatePlacements/CanPlaceRegistrant';
                        canPlaceRegistrantUrl += '?registrantId=' + registrantId + '&registrationTemplatePlacementId=' + self.registrationTemplatePlacementId + '&groupId=' + groupId;

                        // first do a GET to CanPlaceRegistrant to see if the registrant is allowed to be placed into a placement group due to AllowMultiple rules
                        $.ajax({
                            method: "GET",
                            url: canPlaceRegistrantUrl
                        }).done(function () {

                            // if CanPlaceRegistrant returns true, go ahead and add them to the group
                            var addGroupMemberUrl = Rock.settings.get('baseUrl') + 'api/GroupMembers';
                            $.ajax({
                                method: "POST",
                                url: addGroupMemberUrl,
                                data: groupMember
                            }).done(function () {
                                self.populateGroupRoleMembers($groupRoleMembers);
                            }).fail(function (jqXHR) {
                                $draggedItem.attr('data-has-placement-error', true);
                                $draggedItem.attr('registrant-placement-error-message', jqXHR.responseJSON.Message);
                                $draggedItem.addClass('alert alert-warning');
                                $draggedItem.html(jqXHR.responseJSON.Message);
                            });

                        }).fail(function (jqXHR) {
                            $draggedItem.attr('data-has-placement-error', true);
                            $draggedItem.attr('registrant-placement-error-message', jqXHR.responseJSON.Message);
                            $draggedItem.addClass('alert alert-warning');
                            $draggedItem.html(jqXHR.responseJSON.Message);
                        });





                self.trimSourceContainer();
            });

        this.trimSourceContainer();
        this.initializeEventHandlers();

        self.populateRegistrants(self.$registrantList);

        self.populateAllGroupRoleMembers();
    },
        /** trims the source container if it just has whitespace, so that the :empty css selector works */
        trimSourceContainer: function () {
            // if js-scheduler-source-container just has whitespace in it, trim it so that the :empty css selector works
            var $sourceContainer = $('.js-scheduler-source-container');
            if (($.trim($sourceContainer.html()) == "")) {
                $sourceContainer.html("");
            }
        },
    /** Removes the groupMember and repopulates the UI */
    removeGroupMember: function ($groupMember) {
        debugger
        var self = this;

        var groupMemberId = $groupMember.attr('data-groupmember-id');

        var groupMembersURI = Rock.settings.get('baseUrl') + 'api/GroupMembers';

        $.ajax({
            method: "DELETE",
            url: groupMembersURI + '/' + groupMemberId
        }).done(function (deleteResult) {

            var $groupRole = $groupMember.closest('.js-group-role-members')
            self.populateGroupRoleMembers($groupRole);
        }).fail(function (a, b, c) {
            console.log('fail');
        });
    },
    /** populates the placed registrants for all the occurrence group-role divs */
    populateAllGroupRoleMembers: function () {
        var self = this;
        var groupRoleEls = $(".js-group-role-members", self.$groupPlacementTool).toArray();
        $.each(groupRoleEls, function (i) {
            var $groupRole = $(groupRoleEls[i]);
            self.populateGroupRoleMembers($groupRole);
        });
    },
    /** populates the group role members for the group role div */
    populateGroupRoleMembers: function ($groupRole) {
        var getGroupMembersUrl = Rock.settings.get('baseUrl') + 'api/GroupMembers';
        var $placementGroup = $groupRole.closest('.js-placement-group');
        var $groupRoleContainer = $groupRole.find('.js-group-role-container');

        var groupId = $placementGroup.find('.js-placement-group-id').val();
        var groupTypeRoleId = $groupRole.find('.js-grouptyperole-id').val();
        var groupMemberFilter = '$filter='
        groupMemberFilter += 'GroupId eq ' + groupId;
        groupMemberFilter += ' and GroupRoleId eq ' + groupTypeRoleId;

        var self = this;
        $.get(getGroupMembersUrl + '?' + groupMemberFilter + '&$expand=Person', function (groupMembers) {
            $groupRoleContainer.html('');

            $.each(groupMembers, function (i) {
                var groupMember = groupMembers[i];

                var $groupMemberDiv = $('.js-group-member-template').find('.js-group-member').clone();
                self.populateGroupRoleDiv($groupMemberDiv, groupMember);
                $groupRoleContainer.append($groupMemberDiv);
            });

        });
    },
    populateGroupRoleDiv: function ($groupRoleDiv, groupMember) {
        $groupRoleDiv.attr('data-groupmember-id', groupMember.Id);
        $groupRoleDiv.attr('data-person-id', groupMember.Id);
        $groupRoleDiv.attr('data-person-gender', groupMember.Person.Gender);
        $groupRoleDiv.find('.js-groupmember-name').text(groupMember.Person.NickName + ' ' + groupMember.Person.LastName);
    },
    /** populates the registrant list with available registrants */
    populateRegistrants: function ($registrantList) {
        var self = this;
        var $registrantContainer = $('.js-group-placement-registrant-container', $registrantList);
        var getGroupPlacementRegistrantsUrl = Rock.settings.get('baseUrl') + 'api/RegistrationRegistrants/GetGroupPlacementRegistrants';
        var getGroupPlacementRegistrantsParameters = {
            RegistrationTemplateId: Number($('.js-registration-template-id', self.$groupPlacementTool).val()),
            RegistrationInstanceId: Number($('.js-registration-instance-id', self.$groupPlacementTool).val()),
            RegistrationTemplatePlacementId: self.registrationTemplatePlacementId,
            IncludeFees: $('.js-options-include-fees', self.$groupPlacementTool).val(),
            DataFilterId: Number($('.js-options-datafilter-id', self.$groupPlacementTool).val()),
        };

        if ($('.js-registration-template-instance-id-list', self.$groupPlacementTool).val() != '') {
            getGroupPlacementRegistrantsParameters.RegistrationTemplateInstanceIds = JSON.parse($('.js-registration-template-instance-id-list', self.$groupPlacementTool).val());
        }

        if ($('.js-options-displayed-attribute-ids', self.$groupPlacementTool).val() != '') {
            getGroupPlacementRegistrantsParameters.DisplayedAttributeIds = JSON.parse($('.js-options-displayed-attribute-ids', self.$groupPlacementTool).val());
        }

        var $loadingNotification = self.$groupPlacementTool.find('.js-loading-notification');

        $registrantContainer.html(' ');
        $loadingNotification.fadeIn();

        $.ajax({
            method: "POST",
            url: getGroupPlacementRegistrantsUrl,
            data: getGroupPlacementRegistrantsParameters
        }).done(function (registrants) {
            var registrantContainerParent = $registrantContainer.parent();

            // temporarily detach $registrantContainer to speed up adding the registrantdivs
            $registrantContainer.detach();
            $registrantContainer.html('');
            var $registrantTemplate = $('.js-registrant-template').find('.js-registrant');
            for (var i = 0; i < registrants.length; i++) {
                var registrant = registrants[i];
                var $registrantDiv = $registrantTemplate.clone();
                self.populateRegistrantDiv($registrantDiv, registrant);
                $registrantContainer.append($registrantDiv);
            }

            registrantContainerParent.append($registrantContainer);

            setTimeout(function () {
                $loadingNotification.hide();
            }, 0)

        }).fail(function (a, b, c) {
            console.log('fail:' + a.responseText);
            $loadingNotification.hide();
        });

    },
    /**  populates the registrant element */
    populateRegistrantDiv: function ($registrantDiv, registrant) {

        $registrantDiv.attr('data-person-id', registrant.PersonId);
        $registrantDiv.attr('data-person-gender', registrant.PersonGender);
        $registrantDiv.attr('data-registrant-id', registrant.RegistrantId);

        $registrantDiv.find('.js-registrant-name').text(registrant.PersonName);
        $registrantDiv.find('.js-registrant-registrationinstance-name').text(registrant.RegistrationInstanceName);

        var $feesDiv = $registrantDiv.find('.js-registrant-fees-container');
        var $feesDl = $('<dl></dl>');
        for (var fee in registrant.Fees) {
            $feesDl.append('<dt>' + fee + ' </dt><dd>' + registrant.Fees[fee] + '</dd>');
        }
        $feesDiv.append($feesDl);

        var $attributesDiv = $registrantDiv.find('.js-registrant-attributes-container');
        var $attributesDl = $('<dl></dl>');
        for (var displayedAttributeValue in registrant.DisplayedAttributeValues) {
            $attributesDl.append('<dt>' + displayedAttributeValue + ' </dt><dd>' + registrant.DisplayedAttributeValues[displayedAttributeValue] + '</dd>');
        }
        $attributesDiv.append($attributesDl);
    },
    /**  */
    initializeEventHandlers: function () {
        var self = this;

        self.$groupPlacementTool.on('click', '.js-remove-group-member, .js-unlink-group, .js-delete-group', function (a, b, c) {
            debugger
            var $groupMember = $(this).closest('.js-group-member');
            var $placementGroup = $(this).closest('.js-placement-group');
            if ($(this).hasClass('js-remove-group-member')) {
                self.removeGroupMember($groupMember)
            }
            else if ($(this).hasClass('js-unlink-group')) {
                //scheduledPersonUrl = Rock.settings.get('baseUrl') + 'api/Attendances/ScheduledPersonDecline';
            }
            else if ($(this).hasClass('js-delete-group')) {
                //scheduledPersonUrl = Rock.settings.get('baseUrl') + 'api/Attendances/ScheduledPersonPending';
            }
            else {
                return;
            }

        });

        // add autoscroll capabilities during dragging
        $(window).mousemove(function (e) {
            if (self.registrantListDrake.dragging) {
                // editor scrollbar
                // automatically scroll the editor (inner scrollbar) if the mouse gets within 10% of the top or 10% of the bottom while dragging
                var $editorScrollWindow = $(window);
                var editorScrollHeight = window.innerHeight;
                var editorScrollLevel = $editorScrollWindow.scrollTop()
                var editorMouseY = e.clientY;
                var editorMousePositionProportion = editorMouseY / editorScrollHeight;
                if (editorMousePositionProportion > .90) {
                    editorScrollLevel += 20;
                    $editorScrollWindow.scrollTop(editorScrollLevel);
                }
                else if (editorMousePositionProportion < .10 && editorScrollLevel != 0) {
                    editorScrollLevel -= 20;
                    $editorScrollWindow.scrollTop(editorScrollLevel);
                }
            }
        });

    }
};

return exports;
    }());
}(jQuery));
