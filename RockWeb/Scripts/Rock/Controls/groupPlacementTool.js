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

                        debugger

                        if (target.classList.contains('js-group-placement-registrant-container')) {
                            // deal with the group member that was dragged back into the registrants
                            // TODO Delete from group, then refresh the group role container
                        }
                        else {
                            // deal with the registrant that was dragged into a group's group role
                            // TODO Add to group

                            debugger
                            
                            var $groupRoleMembers = $(target).closest('.js-group-role-members');
                            var $placementGroup = $groupRoleMembers.closest('.js-placement-group');

                            var groupTypeRoleId = $groupRoleMembers.find('.js-grouptyperole-id').val()
                            var personId = $(el).attr('data-person-id');
                            var groupId = $placementGroup.find('.js-placement-group-id').val();

                            var groupMember = {
                                IsSystem: false,
                                GroupId: groupId,
                                PersonId: personId,
                                GroupRoleId: groupTypeRoleId,
                                GroupMemberStatus: 1
                            }

                            var addGroupMemberUrl = Rock.settings.get('baseUrl') + 'api/GroupMembers';

                            // add as pending (or confirmed if already confirmed) to target occurrence
                            $.ajax({
                                method: "POST",
                                url: addGroupMemberUrl,
                                data: groupMember
                            }).done(function (groupMemberId) {
                                //todo?
                            }).fail(function (a, b, c) {
                                console.log('fail');
                            });
                        }
                        self.trimSourceContainer();
                    });

                this.trimSourceContainer();
                this.initializeEventHandlers();

                self.populateRegistrants(self.$registrantList);

                self.populateAllScheduledOccurrences();
            },
            /** trims the source container if it just has whitespace, so that the :empty css selector works */
            trimSourceContainer: function () {
                // if js-scheduler-source-container just has whitespace in it, trim it so that the :empty css selector works
                var $sourceContainer = $('.js-scheduler-source-container');
                if (($.trim($sourceContainer.html()) == "")) {
                    $sourceContainer.html("");
                }
            },
            /** Removes the registrant and repopulates the UI */
            removeResource: function ($scheduledResource, $occurrence) {
                var self = this;

                var attendanceId = $scheduledResource.attr('data-attendance-id');
                var refreshAllOccurrences = $scheduledResource.data('occurrence-date-count') > 1;

                // unschedule and repopulate ui
                var scheduledPersonRemoveUrl = Rock.settings.get('baseUrl') + 'api/Attendances/ScheduledPersonRemove';

                $.ajax({
                    method: "PUT",
                    url: scheduledPersonRemoveUrl + '?attendanceId=' + attendanceId
                }).done(function (scheduledAttendance) {
                    // after removing a registrant, repopulate the list of unscheduled registrants
                    self.populateRegistrants(self.$registrantList);

                    // after removing a registrant, repopulate the list of registrants for the occurrence
                    // If there are multiple occurrences during the week for the selected schedule, repopulate all of them to make sure all that the schedule conflict warnings are accurate
                    if (refreshAllOccurrences) {
                        self.populateAllScheduledOccurrences();

                    } else {
                        self.populateScheduledOccurrence($occurrence);
                    }
                }).fail(function (a, b, c) {
                    console.log('fail');
                });
            },
            /** populates the scheduled (requested/scheduled) registrants for all the occurrence divs */
            populateAllScheduledOccurrences: function () {
                var self = this;
                var occurrenceEls = $(".js-scheduled-occurrence", self.$groupPlacementTool).toArray();
                $.each(occurrenceEls, function (i) {
                    var $occurrence = $(occurrenceEls[i]);
                    self.populateScheduledOccurrence($occurrence);
                });
            },
            /** populates the scheduled (requested/scheduled) registrants for the occurrence div */
            populateScheduledOccurrence: function ($occurrence) {
                var getScheduledUrl = Rock.settings.get('baseUrl') + 'api/Attendances/GetAttendingSchedulerResources';
                var attendanceOccurrenceId = $occurrence.find('.js-attendanceoccurrence-id').val();
                var $schedulerTargetContainer = $occurrence.find('.js-scheduler-target-container');

                var minimumCapacity = $occurrence.find('.js-minimum-capacity').val();
                var desiredCapacity = $occurrence.find('.js-desired-capacity').val();
                var maximumCapacity = $occurrence.find('.js-maximum-capacity').val();
                var $schedulingStatusContainer = $occurrence.find('.js-scheduling-status');
                var $autoschedulerWarning = $occurrence.find('.js-autoscheduler-warning');
                var occurrenceDate = new Date($occurrence.find('.js-attendanceoccurrence-date').val()).getTime();
                var hasLocation = $occurrence.data('has-location')

                if (!desiredCapacity) {
                    $autoschedulerWarning.show();
                    $autoschedulerWarning.tooltip();
                }
                else {
                    $autoschedulerWarning.hide();
                }

                var self = this;
                $.get(getScheduledUrl + '?attendanceOccurrenceId=' + attendanceOccurrenceId, function (scheduledAttendanceItems) {
                    $schedulerTargetContainer.html('');
                    var totalPending = 0;
                    var totalConfirmed = 0;
                    var totalDeclined = 0;

                    // hide the scheduled occurrence when it is empty if is the one that doesn't have a Location assigned
                    if ($occurrence.data('has-location') == 0) {
                        if (scheduledAttendanceItems.length == 0) {
                            $occurrence.hide();
                        } else {
                            $occurrence.show();
                        }
                    }

                    $.each(scheduledAttendanceItems, function (i) {
                        var scheduledAttendanceItem = scheduledAttendanceItems[i];

                        // add up status numbers
                        if (scheduledAttendanceItem.ConfirmationStatus == 'confirmed') {
                            totalConfirmed++;
                        } else if (scheduledAttendanceItem.ConfirmationStatus == 'declined') {
                            totalDeclined++;
                        } else {
                            totalPending++;
                        }

                        var $registrantDiv = $('.js-scheduled-registrant-template').find('.js-registrant').clone();
                        $registrantDiv.data('occurrenceDate', occurrenceDate);
                        $registrantDiv.data('hasLocation', hasLocation);
                        self.populateResourceDiv($registrantDiv, scheduledAttendanceItem);
                        $schedulerTargetContainer.append($registrantDiv);
                    });

                    var $statusLight = $schedulingStatusContainer.find('.js-scheduling-status-light');

                    var totalPendingOrConfirmed = totalConfirmed + totalPending;

                    if (minimumCapacity && (totalPendingOrConfirmed < minimumCapacity)) {
                        $statusLight.attr('data-status', 'below-minimum');
                    }
                    else if (desiredCapacity && (totalPendingOrConfirmed < desiredCapacity)) {
                        $statusLight.attr('data-status', 'below-desired');
                    }
                    else if (desiredCapacity && (totalPendingOrConfirmed >= desiredCapacity)) {
                        $statusLight.attr('data-status', 'meets-desired');
                    }
                    else {
                        // no capacities defined, so just hide it
                        $statusLight.attr('data-status', 'none');
                    }

                    // set the progressbar max range to desired capacity if known
                    var progressMax = desiredCapacity;
                    var totalScheduled = (totalPending + totalConfirmed);
                    if (!progressMax) {
                        // desired capacity isn't known, so just have it act as a stacked bar based on the sum of pending,confirmed,declined
                        progressMax = totalScheduled;
                    }

                    if (totalScheduled > desiredCapacity) {
                        // more scheduled then desired, so base the progress bar on the total scheduled
                        progressMax = totalScheduled;
                    }

                    var toolTipHtml = '<div>Confirmed: ' + totalConfirmed + '<br/>Pending: ' + totalPending + '<br/>Declined: ' + totalDeclined + '</div>';

                    $schedulingStatusContainer.attr('data-original-title', toolTipHtml);
                    $schedulingStatusContainer.tooltip({ html: true });

                    var confirmedPercent = !progressMax || (totalConfirmed * 100 / progressMax);
                    var pendingPercent = !progressMax || (totalPending * 100 / progressMax);
                    var minimumPercent = !progressMax || (minimumCapacity * 100 / progressMax);
                    var desiredPercent = !progressMax || (desiredCapacity * 100 / progressMax);

                    var $progressMinimumIndicator = $schedulingStatusContainer.find('.js-minimum-indicator');
                    var $progressDesiredIndicator = $schedulingStatusContainer.find('.js-desired-indicator');

                    if (desiredCapacity && minimumCapacity && minimumCapacity > 0) {
                        $progressMinimumIndicator
                            .attr('data-minimum-value', minimumCapacity)
                            .css({ 'margin-left': minimumPercent + '%' }).show();

                        $progressDesiredIndicator
                            .attr('data-desired-value', desiredCapacity)
                            .css({ 'margin-left': desiredPercent + '%' }).show();
                    }
                    else {
                        // if neither desired capacity or minimum is defined, showing a minimum indicator on progress bar really doesn't make sense.
                        $progressMinimumIndicator.hide();
                        $progressDesiredIndicator.hide();
                    }

                    var $progressConfirmed = $schedulingStatusContainer.find('.js-scheduling-progress-confirmed');
                    $progressConfirmed.css({ 'width': confirmedPercent + '%' });
                    $progressConfirmed.find('.js-progress-text-percent').val(confirmedPercent);

                    var $progressPending = $schedulingStatusContainer.find('.js-scheduling-progress-pending');
                    $progressPending.css({ 'width': pendingPercent + '%' });
                    $progressPending.find('.js-progress-text-percent').val(pendingPercent);

                });
            },
            /** populates the registrant list with available registrants */
            populateRegistrants: function ($registrantList) {
                var self = this;
                var $registrantContainer = $('.js-group-placement-registrant-container', $registrantList);
                var getGroupPlacementRegistrantsUrl = Rock.settings.get('baseUrl') + 'api/RegistrationRegistrants/GetGroupPlacementRegistrants';
                var getGroupPlacementRegistrantsParameters = {
                    RegistrationTemplateId: Number($('.js-registration-template-id', $registrantList).val()),
                    RegistrationInstanceId: Number($('.js-registration-instance-id', $registrantList).val()),
                    RegistrationTemplatePlacementId: $('.js-registration-template-placement-id', $registrantList).val(),
                    IncludeFees: $('.js-options-include-fees', $registrantList).val(),
                    DataFilterId: Number($('.js-options-datafilter-id', $registrantList).val()),
                };

                if ($('.js-registration-template-instance-id-list', $registrantList).val() != '') {
                    getGroupPlacementRegistrantsParameters.RegistrationTemplateInstanceIds = JSON.parse($('.js-registration-template-instance-id-list', $registrantList).val());
                }

                if ($('.js-options-displayed-attribute-ids', $registrantList).val() != '') {
                    getGroupPlacementRegistrantsParameters.DisplayedAttributeIds = JSON.parse($('.js-options-displayed-attribute-ids', $registrantList).val());
                }

                var $loadingNotification = $registrantList.find('.js-loading-notification');

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

                if (registrant.PersonGender == 2) {
                    $registrantDiv.addClass('registrant-gender-female')
                } else {
                    $registrantDiv.addClass('registrant-gender-male')
                }

                $registrantDiv.find('.js-registrant-name').text(registrant.PersonName);
                $registrantDiv.find('.js-registrant-registrationinstance-name').text(registrant.RegistrationInstanceName);
                
                var $feesDiv = $registrantDiv.find('.js-registrant-fees-container');
                var $attributesDiv = $registrantDiv.find('.js-registrant-attributes-container');

                for (var fee in registrant.Fees) {
                    $feesDiv.append('<dt>' + fee + ' </dt><dd>' + registrant.Fees[fee] + '</dd>');
                }

                for (var displayedAttributeValue in registrant.DisplayedAttributeValues) {
                    $attributesDiv.append('<dt>' + displayedAttributeValue + ' </dt><dd>' + registrant.DisplayedAttributeValues[displayedAttributeValue] + '</dd>');
                }


            },
            /**  */
            initializeEventHandlers: function () {
                var self = this;

                self.$groupPlacementTool.on('click', '.js-markconfirmed, .js-markdeclined, .js-markpending, .js-resendconfirmation', function (a, b, c) {
                    var $registrant = $(this).closest('.js-registrant');
                    var attendanceId = $registrant.attr('data-attendance-id');
                    var scheduledPersonUrl;
                    if ($(this).hasClass('js-markconfirmed')) {
                        scheduledPersonUrl = Rock.settings.get('baseUrl') + 'api/Attendances/ScheduledPersonConfirm';
                    }
                    else if ($(this).hasClass('js-markdeclined')) {
                        scheduledPersonUrl = Rock.settings.get('baseUrl') + 'api/Attendances/ScheduledPersonDecline';
                    }
                    else if ($(this).hasClass('js-markpending')) {
                        scheduledPersonUrl = Rock.settings.get('baseUrl') + 'api/Attendances/ScheduledPersonPending';
                    }
                    else if ($(this).hasClass('js-resendconfirmation')) {
                        scheduledPersonUrl = Rock.settings.get('baseUrl') + 'api/Attendances/ScheduledPersonSendConfirmationEmail';
                    }
                    else {
                        return;
                    }

                    $.ajax({
                        method: "PUT",
                        url: scheduledPersonUrl + '?attendanceId=' + attendanceId
                    }).done(function () {
                        // after updating a registrant, repopulate the list of registrants for this occurrence
                        var $occurrence = $registrant.closest('.js-scheduled-occurrence');
                        self.populateScheduledOccurrence($occurrence);
                    }).fail(function (a, b, c) {
                        console.log('fail');
                    })
                });

                // add autoscroll capabilities during dragging
                $(window).mousemove(function (e) {
                    if (self.registrantListDrake.dragging) {
                        // editor scrollbar
                        // automatically scroll the editor (inner scrollbar) if the mouse gets within 10% of the top or 10% of the bottom while dragger
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
