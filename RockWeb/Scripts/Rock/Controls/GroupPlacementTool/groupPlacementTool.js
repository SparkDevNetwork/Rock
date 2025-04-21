(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    /** JS helper for the groupPlacement block */
    Rock.controls.groupPlacementTool = (function () {
        /// <summary>
        /// s this instance.
        /// </summary>
        /// <returns></returns>
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
                self.registrationTemplatePlacementId = parseInt($('.js-registration-template-placement-id', self.$groupPlacementTool).val()) || null;
                self.allowMultiplePlacements = $('.js-registration-template-placement-allow-multiple-placements', self.$groupPlacementTool).val() == 'true';
                self.registrationInstanceId = parseInt($('.js-registration-instance-id', self.$groupPlacementTool).val()) || null;
                self.groupMemberDetailUrl = $('.js-group-member-detail-url', self.$groupPlacementTool).val();
                self.groupDetailUrl = $('.js-group-detail-url', self.$groupPlacementTool).val();
                self.$groupMemberTemplate = $('.js-group-member-template', self.$groupPlacementTool).find('.js-group-member');
                self.$toggleRegistrantDetails = $('.js-toggle-registrant-details', self.$groupPlacementTool);
                self.blockId = $('.js-block-id', self.$groupPlacementTool).val();

                if (self.groupDetailUrl == '') {
                    // no url, so remove the Edit button(s)
                    $('.js-edit-group', self.$groupPlacementTool).remove();
                } else {
                    $('.js-edit-group', self.$groupPlacementTool).each(function (i, e) {
                        var groupId = $(e).closest('.js-placement-group').find('.js-placement-group-id').val();
                        $(e).attr('href', self.groupDetailUrl + '?GroupId=' + groupId);
                    });
                }

                if (self.groupMemberDetailUrl == '') {
                    // no group member url, so remove the edit button from the registrationTemplatePlacementId

                    $('.js-edit-group-member', self.$groupPlacementTool).remove();
                }

                if (self.registrationTemplatePlacementId == null) {
                    // if registrationTemplatePlacementId wasn't selected yet, return
                    return;
                }

                self.showRegistrantInstanceName = $('.js-registration-template-show-instance-name', self.$groupPlacementTool).val() == 'true';

                if (self.registrationInstanceId) {
                    // if group placement is in specific instance mode, then never show instance name in the registrant details
                    self.showRegistrantInstanceName = false;
                }

                self.showAllRegistrantDetails = false;
                self.highlightGenders = $('.js-options-highlight-genders', self.$groupPlacementTool).val() == 'true';
                self.hideFullGroups = $('.js-options-hide-full-groups', self.$groupPlacementTool).val() == 'true';
                self.displayRegistrantAttributes = $('.js-options-display-registrant-attributes', self.$groupPlacementTool).val() == 'true';
                self.applyRegistrantFilter = $('.js-options-apply-registrant-filters', self.$groupPlacementTool).val() == 'true';

                // initialize dragula
                var containers = [];

                // add the registrant list as a dragula container
                containers.push($control.find('.js-group-placement-registrant-container')[0]);

                // add all the placement group's group roles as dragula containers
                var groupRoleDropZones = $control.find('.js-group-role-container').toArray();

                $.each(groupRoleDropZones, function (i) {
                    containers.push(groupRoleDropZones[i]);
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

                        if (target.classList.contains('js-group-role-container') && source.classList.contains('js-group-role-container')) {
                            // don't let a group member get dragged from one group role to another
                            $(el).data('allow-drop', false);
                            return false;
                        }
                        else {
                            $(el).data('allow-drop', true);
                            return true;
                        }
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
                            var $groupMember = $draggedItem;
                            var $groupRoleMembers = $(source).closest('.js-group-role-members');
                            self.removeGroupMember($groupMember, $groupRoleMembers);
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

                        /*
                           GroupTypeId is purposely assigned default integer value to bypass the validation check in apiController. 
                           However GroupMember PreSave Hook of SaveChanges takes care by assigning the correct value to GroupTypeId. 
                        */

                        var groupMember = {
                            IsSystem: false,
                            GroupId: groupId,
                            GroupTypeId: 0,
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
                                self.showPlaceRegistrantError($groupRoleMembers, jqXHR, $draggedRegistrant);
                            });

                        }).fail(function (jqXHR) {
                            self.showPlaceRegistrantError($groupRoleMembers, jqXHR, $draggedRegistrant);
                        });
                    });

                this.initializeEventHandlers();

                var getBlockUserPreferenceUrl = Rock.settings.get('baseUrl') + 'api/People/GetBlockUserPreference?blockId=' + self.blockId + '&userPreferenceKey=expandRegistrantDetails';

                $.ajax({
                    method: "GET",
                    url: getBlockUserPreferenceUrl
                }).done(function (expandDetails) {
                    // if this user does not yet have this Block user preference defined, default to true
                    if (!expandDetails) {
                        expandDetails = 'true';
                    }
                    self.populateRegistrants(self.$registrantList, expandDetails == 'true');
                });

                self.populateAllGroupRoleMembers();
            },
            /** checks to see if there are any visible registrants, and shows a message if there aren't */
            checkVisibleRegistrants: function () {
                var self = this;

                var $noRegistrantsDiv = $('.js-no-registrants-div', self.$groupPlacementTool)
                var $sourceContainer = $('.js-group-placement-registrant-container ');
                if ($sourceContainer.height() == 0) {
                    $sourceContainer.addClass("empty");
                }
                else {
                    $sourceContainer.removeClass("empty");
                }
            },
            /** Shows or Hides the registrants for the specified personId, and placementGroupRegistrationInstanceId (if it is a instance specific placementgroup)
             *  this is only needed when AllowMultiplePlacements = false
             */
            setRegistrantVisibility: function (personId, placementGroupRegistrationInstanceId, setVisible) {
                var self = this;
                var registrantSelector = '[data-person-id=' + personId + ']';
                if (placementGroupRegistrationInstanceId != '') {
                    registrantSelector += '[data-registrant-registrationinstanceid=' + placementGroupRegistrationInstanceId + ']';
                }

                if (setVisible) {
                    // if the group member has been removed from the group, and the registrant is hidden due to 'allow multiple placements=false', we can show it again since they are no longer in a group
                    var $registrantsToShow = self.$registrantList.find(registrantSelector);
                    $registrantsToShow.attr('allow-search', 'true');
                    $registrantsToShow.show();
                } else {
                    // hide any registrants (person) that are already placed
                    var $registrantsToHide = self.$registrantList.find(registrantSelector);
                    $registrantsToHide.attr('allow-search', 'false');
                    $registrantsToHide.hide();
                }
            },
            /** Removes the groupMember and repopulates the UI */
            removeGroupMember: function ($groupMember, $groupRoleMembers) {
                var self = this;
                var groupMemberId = $groupMember.attr('data-groupmember-id');
                var groupMembersURI = Rock.settings.get('baseUrl') + 'api/GroupMembers';
                var personId = $groupMember.attr('data-person-id');
                var placementGroupRegistrationInstanceId = $groupMember.attr('data-placementgroup-registrationinstanceid');

                $.ajax({
                    method: "DELETE",
                    url: groupMembersURI + '/' + groupMemberId
                }).done(function (deleteResult) {
                    // if the group member has been removed from the group, and the registrant is hidden due to 'allow multiple placements=false', we can show it again since they are no longer in a group
                    self.setRegistrantVisibility(personId, placementGroupRegistrationInstanceId, true);

                    self.populateGroupRoleMembers($groupRoleMembers);
                    self.checkVisibleRegistrants();
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
            populateGroupRoleMembers: function ($groupRoleMembers) {
                var self = this;

                // hide any alerts that are showing
                $('.js-alert', self.$groupPlacementTool).hide();

                var getGroupMembersUrl = Rock.settings.get('baseUrl') + 'api/GroupMembers/GetGroupPlacementGroupMembers';
                var $placementGroup = $groupRoleMembers.closest('.js-placement-group');

                // If this placement group is just for a specific registration instance, this will be the instance id
                // If this is a shared group, placementGroupRegistrationInstanceId will be null.
                var placementGroupRegistrationInstanceId = $placementGroup.find('.js-placement-group-registrationinstanceid').val();

                var $groupRoleContainer = $groupRoleMembers.find('.js-group-role-container');

                var groupId = $placementGroup.find('.js-placement-group-id').val();
                var groupTypeRoleId = $groupRoleMembers.find('.js-grouptyperole-id').val();
                var groupMemberAttributeIds = $('.js-options-displayed-groupmember-attribute-ids', self.$groupPlacementTool).val();


                var getGroupPlacementGroupMembersParameters = {
                    GroupId: groupId,
                    GroupRoleId: groupTypeRoleId,
                    BlockId: self.blockId,
                    RegistrantId: parseInt($('.js-registrant-id', self.$groupPlacementTool).val()) || null,
                    RegistrationTemplateId: parseInt($('.js-registration-template-id', self.$groupPlacementTool).val()) || null,
                    RegistrationInstanceId: parseInt($('.js-registration-instance-id', self.$groupPlacementTool).val()) || null,
                    RegistrationTemplatePlacementId: self.registrationTemplatePlacementId,
                    IncludeFees: $('.js-options-include-fees', self.$groupPlacementTool).val(),
                    RegistrantPersonDataViewFilterId: parseInt($('.js-options-registrant-person-dataviewfilter-id', self.$groupPlacementTool).val()) || null,
                    FilterFeeId: parseInt($('.js-options-filter-fee-id', self.$groupPlacementTool).val()) || null,
                    DisplayRegistrantAttributes: self.displayRegistrantAttributes,
                    ApplyRegistrantFilter: self.applyRegistrantFilter
                };

                if ($('.js-options-displayed-registrant-attribute-ids', self.$groupPlacementTool).val() != '') {
                    getGroupPlacementGroupMembersParameters.DisplayedRegistrantAttributeIds = JSON.parse($('.js-options-displayed-registrant-attribute-ids', self.$groupPlacementTool).val());
                }

                if (groupMemberAttributeIds) {
                    getGroupPlacementGroupMembersParameters.DisplayedGroupMemberAttributeIds = JSON.parse(groupMemberAttributeIds);
                }

                if ($('.js-options-filter-fee-item-ids', self.$groupPlacementTool).val() != '') {
                    getGroupPlacementGroupMembersParameters.FilterFeeOptionIds = JSON.parse($('.js-options-filter-fee-item-ids', self.$groupPlacementTool).val());
                }

                $.ajax({
                    method: "POST",
                    url: getGroupMembersUrl,
                    data: getGroupPlacementGroupMembersParameters
                }).done(function (groupMembers) {

                    $groupRoleContainer.html('');

                    $.each(groupMembers, function (i) {
                        var groupMember = groupMembers[i];

                        var $groupMemberDiv = self.$groupMemberTemplate.clone();
                        self.populateGroupMember($groupMemberDiv, groupMember, placementGroupRegistrationInstanceId);
                        $groupRoleContainer.append($groupMemberDiv);
                    });

                    self.checkVisibleRegistrants();

                    var groupCapacity = parseInt($('.js-placement-capacity', $placementGroup).val()) || null;

                    var $groupCapacityLabel = $('.js-placement-capacity-label', $placementGroup);

                    if (groupCapacity) {
                        var groupMemberCount = $('.js-group-member', $placementGroup).length;
                        $groupCapacityLabel.text(groupMemberCount + ' / ' + groupCapacity);
                        var groupCapacityPercent = (groupMemberCount / groupCapacity) * 100;
                        if (groupCapacityPercent > 100) {
                            $groupCapacityLabel.attr('data-status', 'over-capacity').attr('title', 'Group Over Capacity');
                        } else if (groupCapacityPercent == 100) {
                            $groupCapacityLabel.attr('data-status', 'at-capacity').attr('title', 'Group At Capacity');
                        } else if (groupCapacityPercent > 80) {
                            $groupCapacityLabel.attr('data-status', 'near-capacity').attr('title', (groupCapacity - groupMemberCount) + ' Spots Remaining');
                        } else {
                            $groupCapacityLabel.attr('data-status', 'under-capacity').attr('title', (groupCapacity - groupMemberCount) + ' Spots Remaining');
                        }

                        if (self.hideFullGroups && groupMemberCount >= groupCapacity) {
                            var $group = $groupRoleContainer.closest('.js-placement-group');
                            $group.hide();
                        }

                    } else {
                        $groupCapacityLabel.attr('data-status', 'none');
                    }

                    var pageAnchor = location.hash;
                    if (pageAnchor) {
                        var pageAnchorPersonId = pageAnchor.replace('#PersonId_', '');

                        if (pageAnchorPersonId && $('.js-group-member[data-person-id=' + pageAnchorPersonId + ']', $groupRoleContainer).length) {

                            // Since the group members load after the page is loaded, we have to manually navigate to the anchor
                            // first change hash to something else to trigger the browser to renavigate to personid anchor
                            setTimeout(function () {
                                location.hash = "#other";
                                location.hash = '#PersonId_' + pageAnchorPersonId;
                            }, 0)
                        }
                    }

                    var groupRoleMaxMembers = parseInt($('.js-grouptyperole-max-members', $groupRoleMembers).val()) || null;
                    var $groupRoleMaxMembersLabel = $('.js-grouptyperole-max-members-label', $groupRoleMembers);
                    if (groupRoleMaxMembers) {
                        var groupRoleMemberCount = groupMembers.length;
                        $groupRoleMaxMembersLabel.text(groupRoleMemberCount + ' / ' + groupRoleMaxMembers);
                        if (groupRoleMemberCount > groupRoleMaxMembers) {
                            $groupRoleMaxMembersLabel.attr('data-status', 'over-capacity').attr('title', 'Group role over capacity.');
                        } else if (groupRoleMemberCount == groupRoleMaxMembers) {
                            $groupRoleMaxMembersLabel.attr('data-status', 'at-capacity').attr('title', 'Group role at capacity.');
                        } else {
                            $groupRoleMaxMembersLabel.attr('data-status', 'under-capacity').attr('title', 'Group role under capacity.');
                        }
                    }
                    else {
                        $groupRoleMaxMembersLabel.attr('data-status', 'none');
                    }
                });
            },
            /**
             * Populates the group member div with the groupMember data
             * @param {any} $groupMemberDiv
             * @param {any} groupMember
             * @param {any} placementGroupRegistrationInstanceId
             */
            populateGroupMember: function ($groupMemberDiv, groupMember, placementGroupRegistrationInstanceId) {
                var self = this;
                $groupMemberDiv.attr('data-groupmember-id', groupMember.Id);
                $groupMemberDiv.attr('data-person-id', groupMember.PersonId);

                // If this placement group is just for a specific registration instance, this will be the instance id
                // If this is a shared group, placementGroupRegistrationInstanceId will be null.
                $groupMemberDiv.attr('data-placementgroup-registrationinstanceid', placementGroupRegistrationInstanceId);

                $groupMemberDiv.find('.js-person-id-anchor').prop('name', 'PersonId_' + groupMember.PersonId);
                if (self.highlightGenders) {
                    $groupMemberDiv.attr('data-person-gender', groupMember.PersonGender);
                }
                $groupMemberDiv.find('.js-groupmember-name').text(groupMember.PersonName);
                var $editGroupMemberButton = $groupMemberDiv.find('.js-edit-group-member');
                if ($editGroupMemberButton.length) {
                    $editGroupMemberButton.attr('href', self.groupMemberDetailUrl + '?GroupMemberId=' + groupMember.Id);
                }

                if (self.allowMultiplePlacements == false) {
                    // hide any registrants (person) that are already placed
                    self.setRegistrantVisibility(groupMember.PersonId, placementGroupRegistrationInstanceId, false);
                }

                // NOTE: AttributeValues are already filtered to the configured displayed attributes when doing the REST call
                if (groupMember.GroupMemberAttributeValues && Object.keys(groupMember.GroupMemberAttributeValues).length > 0) {
                    var $attributesDiv = $('.js-groupmember-attributes-container', $groupMemberDiv);
                    var $attributesDl = $('<dl></dl>');
                    for (var displayedAttribute in groupMember.GroupMemberAttributes) {
                        if (groupMember.GroupMemberAttributeValues[displayedAttribute].ValueFormatted) {
                            $attributesDl.append('<dt>' + groupMember.GroupMemberAttributes[displayedAttribute].Name + ' </dt><dd>' + groupMember.GroupMemberAttributeValues[displayedAttribute].ValueFormatted + '</dd>');
                        }
                    }

                    $attributesDiv.append($attributesDl);
                }

                // NOTE: RegistrantAttributeValues are already filtered to the configured displayed attributes when doing the REST call
                if (groupMember.RegistrantAttributeValues && Object.keys(groupMember.RegistrantAttributeValues).length > 0) {
                    var $registrantAttributesDiv = $('.js-groupmember-attributes-container', $groupMemberDiv);
                    var $registrantAttributesDl = $('<dl></dl>');
                    for (var displayedRegistrantAttribute in groupMember.RegistrantAttributes) {
                        if (groupMember.RegistrantAttributeValues[displayedRegistrantAttribute].ValueFormatted) {
                            $registrantAttributesDl.append('<dt>' + groupMember.RegistrantAttributes[displayedRegistrantAttribute].Name + ' </dt><dd>' + groupMember.RegistrantAttributeValues[displayedRegistrantAttribute].ValueFormatted + '</dd>');
                        }
                    }

                    $registrantAttributesDiv.append($registrantAttributesDl);
                }

                if (!groupMember.GroupMemberAttributeValues && !groupMember.RegistrantAttributeValues) {
                    $('.js-groupmember-details', $groupMemberDiv).hide();
                }
            },
            /** populates the registrant list with available registrants */
            populateRegistrants: function ($registrantList, expandDetails) {
                var self = this;
                var $registrantContainer = $('.js-group-placement-registrant-container', $registrantList);
                var getGroupPlacementRegistrantsUrl = Rock.settings.get('baseUrl') + 'api/RegistrationRegistrants/GetGroupPlacementRegistrants';
                var getGroupPlacementRegistrantsParameters = {
                    RegistrantId: parseInt($('.js-registrant-id', self.$groupPlacementTool).val()) || null,
                    RegistrationTemplateId: parseInt($('.js-registration-template-id', self.$groupPlacementTool).val()) || null,
                    RegistrationInstanceId: parseInt($('.js-registration-instance-id', self.$groupPlacementTool).val()) || null,
                    RegistrationTemplatePlacementId: self.registrationTemplatePlacementId,
                    IncludeFees: $('.js-options-include-fees', self.$groupPlacementTool).val(),
                    RegistrantPersonDataViewFilterId: parseInt($('.js-options-registrant-person-dataviewfilter-id', self.$groupPlacementTool).val()) || null,
                    BlockId: self.blockId,
                    FilterFeeId: parseInt($('.js-options-filter-fee-id', self.$groupPlacementTool).val()) || null,

                };

                if ($('.js-registration-template-instance-id-list', self.$groupPlacementTool).val() != '') {
                    getGroupPlacementRegistrantsParameters.RegistrationTemplateInstanceIds = JSON.parse($('.js-registration-template-instance-id-list', self.$groupPlacementTool).val());
                }

                if ($('.js-options-displayed-registrant-attribute-ids', self.$groupPlacementTool).val() != '') {
                    getGroupPlacementRegistrantsParameters.DisplayedAttributeIds = JSON.parse($('.js-options-displayed-registrant-attribute-ids', self.$groupPlacementTool).val());
                }

                if ($('.js-options-filter-fee-item-ids', self.$groupPlacementTool).val() != '') {
                    getGroupPlacementRegistrantsParameters.FilterFeeOptionIds = JSON.parse($('.js-options-filter-fee-item-ids', self.$groupPlacementTool).val());
                }

                var $loadingNotification = self.$groupPlacementTool.find('.js-loading-notification');

                $registrantContainer.html(' ');
                $loadingNotification.fadeIn();

                $.ajax({
                    method: "POST",
                    url: getGroupPlacementRegistrantsUrl,
                    data: getGroupPlacementRegistrantsParameters
                }).done(function (registrants) {
                    if (registrants !== null) {
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

                        self.checkVisibleRegistrants();

                        self.expandOrHideRegistrantDetails(expandDetails);

                        setTimeout(function () {
                            $loadingNotification.hide();
                        }, 0)
                    }

                }).fail(function (a) {
                    console.log('fail:' + a.responseText);
                    $(".ajax-error-message").html(a.responseText);
                    $(".ajax-error").show();
                    $loadingNotification.hide();
                });
            },
            /**  populates the registrant element */
            populateRegistrantDiv: function ($registrantDiv, registrant) {

                var self = this;

                $registrantDiv.attr('data-person-id', registrant.PersonId);
                if (self.highlightGenders) {
                    $registrantDiv.attr('data-person-gender', registrant.PersonGender);
                }
                $registrantDiv.attr('data-registrant-id', registrant.RegistrantId);
                $registrantDiv.attr('data-registrant-registrationinstanceid', registrant.RegistrationInstanceId);

                $registrantDiv.find('.js-registrant-name').text(registrant.PersonName);

                // note that instead of using hide() on elements that shouldn't shown, use remove() instead to reduce the amount of html, and to help detect if the details should show when toggled

                if (self.showRegistrantInstanceName) {
                    $registrantDiv.find('.js-registrant-registrationinstance-name').text(registrant.RegistrationInstanceName);
                } else {
                    $registrantDiv.find('.js-registration-instance-name-container').remove();
                }

                // if multiple placements aren't allowed, and this registrant(person) is already in one of the placement groups, then hide the div
                // If the person is removed from a placement group, then we can show again
                if (self.allowMultiplePlacements == false && registrant.AlreadyPlacedInGroup) {
                    $registrantDiv.attr('allow-search', 'false');
                    $registrantDiv.hide();
                }

                var $feesDiv = $registrantDiv.find('.js-registrant-fees-container');

                if (registrant.Fees && Object.keys(registrant.Fees).length > 0) {

                    var $feesDl = $('<dl></dl>');
                    for (var fee in registrant.Fees) {
                        $feesDl.append('<dt>' + fee + ' </dt><dd>' + registrant.Fees[fee] + '</dd>');
                    }
                    $feesDiv.append($feesDl);
                }
                else {
                    $feesDiv.remove();
                }

                var $attributesDiv = $registrantDiv.find('.js-registrant-attributes-container');

                // NOTE: AttributeValues are already filtered to the configured displayed attributes when doing the REST call
                if (registrant.AttributeValues && Object.keys(registrant.AttributeValues).length > 0) {
                    var $attributesDl = $('<dl></dl>');
                    for (var displayedAttribute in registrant.Attributes) {
                        if (registrant.AttributeValues[displayedAttribute].ValueFormatted) {
                            $attributesDl.append('<dt>' + registrant.Attributes[displayedAttribute].Name + ' </dt><dd>' + registrant.AttributeValues[displayedAttribute].ValueFormatted + '</dd>');
                        }
                    }
                    $attributesDiv.append($attributesDl);
                }
                else {
                    $attributesDiv.remove();
                }

                // start with the details panel hidden until a hover or toggle makes them visible
                $registrantDiv.find('.js-registrant-details').hide();
            },
            showPlaceRegistrantError: function ($groupRoleMembers, jqXHR, $draggedRegistrant) {
                $draggedRegistrant.remove();

                var self = this;
                var $placeRegistrantError = $('.js-placement-place-registrant-error', $groupRoleMembers);

                // hide any other alerts that are showing
                $('.js-alert', self.$groupPlacementTool).not($placeRegistrantError).hide();
                $placeRegistrantError.find('.js-placement-place-registrant-error-text').text((jqXHR.responseJSON && jqXHR.responseJSON.Message || jqXHR.responseText));
                $placeRegistrantError.show();
            },
            /**
             * expand's or hides the details of all the registrant divs
             */
            expandOrHideRegistrantDetails: function (expand) {
                var self = this;
                self.showAllRegistrantDetails = expand;

                var setBlockUserPreferenceUrl = Rock.settings.get('baseUrl') + 'api/People/SetBlockUserPreference?blockId=' + self.blockId + '&userPreferenceKey=expandRegistrantDetails&value=' + expand;

                $.ajax({
                    method: "POST",
                    url: setBlockUserPreferenceUrl
                });

                if (expand) {
                    $('i', self.$toggleRegistrantDetails).removeClass('fa-angle-double-down').addClass('fa-angle-double-up');
                    $('.js-registrant-details').each(function (i, el) {
                        var $details = $(el);
                        if ($details.text().trim().length != '') {
                            $details.stop().slideDown();
                        }
                    });
                }
                else {
                    $('i', self.$toggleRegistrantDetails).removeClass('fa-angle-double-up').addClass('fa-angle-double-down');
                    $('.js-registrant-details', self.$groupPlacementTool).stop().slideUp();
                }
            },
            /**  */
            initializeEventHandlers: function () {
                var self = this;

                self.$groupPlacementTool.on('click', '.js-remove-group-member, .js-detach-placement-group, .js-delete-group', function () {
                    var $groupMember = $(this).closest('.js-group-member');
                    var $groupRoleMembers = $groupMember.closest('.js-group-role-members');

                    if ($(this).hasClass('js-remove-group-member')) {
                        self.removeGroupMember($groupMember, $groupRoleMembers);
                    }
                    else if ($(this).hasClass('js-detach-placement-group')) {
                        var $group = $(this).closest('.js-placement-group');
                        var groupId = $group.find('.js-placement-group-id').val();
                        var registrationInstanceId = parseInt($('.js-registration-instance-id', self.$groupPlacementTool).val()) || null;

                        Rock.dialogs.confirm('Are you sure you want to detach this placement group?', function (result) {
                            if (!result) {
                                return;
                            }
                            var detachPlacementGroupUrl = Rock.settings.get('baseUrl') + 'api/RegistrationTemplatePlacements/DetachPlacementGroup';
                            detachPlacementGroupUrl += '?groupId=' + groupId;
                            detachPlacementGroupUrl += '&registrationTemplatePlacementId=' + self.registrationTemplatePlacementId;
                            if (registrationInstanceId) {
                                detachPlacementGroupUrl += '&registrationInstanceId=' + registrationInstanceId;
                            };

                            $.ajax({
                                method: "DELETE",
                                url: detachPlacementGroupUrl
                            }).done(function () {
                                $group.hide();
                            }).fail(function (jqXHR) {
                                var $groupAlert = $('.js-placement-group-error', $group);
                                $groupAlert.find('.js-placement-group-error-text').text('Unable to detach group: ' + (jqXHR.responseJSON && jqXHR.responseJSON.Message || jqXHR.responseText));
                                $groupAlert.show();
                            });
                        });
                    }
                    else if ($(this).hasClass('js-delete-group')) {
                        var $group = $(this).closest('.js-placement-group');
                        var groupId = $group.find('.js-placement-group-id').val();
                        Rock.dialogs.confirm('Are you sure you want to delete this group?', function (result) {
                            if (!result) {
                                return;
                            }
                            var deleteGroupUrl = Rock.settings.get('baseUrl') + 'api/Groups?Id=' + groupId;
                            $.ajax({
                                method: "DELETE",
                                url: deleteGroupUrl
                            }).done(function () {
                                $group.hide();
                            }).fail(function (jqXHR) {
                                var $groupAlert = $('.js-placement-group-error', $group);
                                $groupAlert.find('.js-placement-group-error-text').text('Unable to delete group: ' + (jqXHR.responseJSON && jqXHR.responseJSON.Message || jqXHR.responseText))
                                $groupAlert.show();
                            });
                        });
                    }
                    else {
                        return;
                    }
                });

                // filter the search list when stuff is typed in the search box
                $('.js-registrant-search', self.$groupPlacementTool).on('keyup', function () {
                    var value = $(this).find('input').val().toLowerCase().trim();
                    $(".js-group-placement-registrant-container .js-registrant").filter(function () {
                        var $registrantDiv = $(this);
                        if ($registrantDiv.attr('allow-search') == 'false') {
                            return;
                        }

                        if (value == '') {
                            // show everybody
                            $registrantDiv.toggle(true);
                        }
                        else {

                            var registrantName = $registrantDiv.find('.js-registrant-name').text();
                            var registrantNameSplit = registrantName.split(' ');
                            var anyMatch = false;

                            // if the first or lastname starts with the searchstring, show the person
                            $.each(registrantNameSplit, function (nindex) {
                                if (registrantNameSplit[nindex].toLowerCase().indexOf(value) == 0) {
                                    anyMatch = true;
                                }
                            })

                            // if first or last didn't match, see if fullname starts with the search value
                            if (!anyMatch) {
                                if (registrantName.toLowerCase().indexOf(value) == 0) {
                                    anyMatch = true;
                                }
                            }

                            $registrantDiv.toggle(anyMatch);
                        }

                        self.checkVisibleRegistrants();
                    });
                });


                $('.js-group-placement-registrant-list', self.$groupPlacementTool)
                    .on('mouseenter', '.js-registrant', function () {
                        var $details = $('.js-registrant-details', $(this));
                        if ($details.text().trim().length != '') {
                            $details.stop().slideDown();
                        }
                    })
                    .on('mouseleave', '.js-registrant', function () {
                        // if the toggle for show all registrant details is active, just leave it open
                        if (!self.showAllRegistrantDetails) {
                            $('.js-registrant-details', $(this)).stop().slideUp();
                        }
                    });

                $('.js-hide-alert', self.$groupPlacementTool).click(function () {
                    // default bootstrap alert deletes the div, but we want to reuse it so just hide instead
                    $(this).closest('.js-alert').hide();
                });

                $('.js-toggle-registrant-details', self.$groupPlacementTool).click(function () {
                    var expand = !self.showAllRegistrantDetails;
                    self.expandOrHideRegistrantDetails(expand);
                });

                $('.js-toggle-group-details', self.$groupPlacementTool.closest('.block-instance')).click(function () {
                    $('i', this).toggleClass('fa-angle-double-up fa-angle-double-down');
                    // if toggle class is up, then show the details, otherwise hide them
                    var groups = $(this).closest('.js-block-instance').find('.placement-group .js-group-details');
                    var groupSlideIcon = $(this).closest('.js-block-instance').find('.js-placement-group-toggle-visibility i');
                    if ($(this).find('i').hasClass('fa-angle-double-up')) {
                        groups.slideDown();
                        groupSlideIcon.attr('class', 'fa fa-chevron-up');
                    } else {
                        groups.slideUp();
                        groupSlideIcon.attr('class', 'fa fa-chevron-down');
                    }
                });

                self.$groupPlacementTool.on('click', '.js-placement-group-toggle-visibility', function () {
                    $('i', this).toggleClass('fa-chevron-down fa-chevron-up');
                    $(this).closest('.js-placement-group').find('.js-group-details').slideToggle();
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
