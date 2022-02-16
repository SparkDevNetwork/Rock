(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    /** JS helper for the groupScheduler */
    Rock.controls.groupScheduler = (function () {
        var exports = {
            /** initializes the javascript for the groupScheduler */
            initialize: function (options) {
                if (!options.id) {
                    throw 'id is required';
                }

                var self = this;

                var $control = $('#' + options.id);

                if ($control.length === 0) {
                    return;
                }

                var $blockInstance = $control.closest('.block-instance')[0];
                self.$groupScheduler = $control;
                self.$resourceList = $('.js-group-scheduler-resourcelist', $control);
                self.$additionalPersonIds = $('.js-resource-additional-person-ids', self.$resourceList);

                // initialize dragula
                var containers = [];

                // add the resource list as a dragula container
                containers.push($control.find('.js-scheduler-source-container')[0]);

                // add all the occurrences (locations) as dragula containers
                var targets = $control.find('.js-scheduler-target-container').toArray();

                $.each(targets, function (i) {
                    containers.push(targets[i]);
                });

                self.resourceListDrake = dragula(containers, {
                    isContainer: function (el) {
                        return false;
                    },
                    moves: function (el, source, handle, sibling) {
                        if (source.classList.contains('js-scheduler-source-container') && ($(el).data('has-blackout-conflict') || $(el).data('has-requirements-conflict'))) {
                            // don't let resources with a full blackout or requirement conflicts to be scheduled
                            return false;
                        }

                        if (source.classList.contains('js-scheduler-target-container')) {
                            // If multi-group mode this will be a group, and resources can only be dragged out of this occurrence column if it is the currently selected (active) group
                            // In single-group mode, this will be a schedule/day, so resources can be dragged out of any occurrence column

                            var $occurrenceColumn = $(source).closest('.js-occurrence-column');
                            var isSchedulerTargetColumn = $occurrenceColumn.data("is-scheduler-target-column")
                            if (!isSchedulerTargetColumn) {
                                return false;
                            }
                        }

                        return true;
                    },
                    copy: function (el, source) {
                        if (source.classList.contains('js-scheduler-source-container')) {
                            // if the selected schedule(s)/locations(s) result in multiple time slots for the selected week, keep the resource in the list of available resources
                            return $(el).data('displayed-time-slot-count') > 1;
                        }
                        return false;
                    },
                    accepts: function (el, target, source, sibling) {
                        if (target.classList.contains('js-scheduler-target-container')) {
                            var $resourceDiv = $(el);

                            // the occurrence of this target occurrence column
                            // If multi-group mode this occurrence column will be a group, and resources can only be dragged into it is the the currently selected (active) group
                            // In single-group mode, this occurrence column will be a schedule/day, and any target column can be dragged into
                            var $occurrenceColumn = $(target).closest('.js-occurrence-column');
                            var isSchedulerTargetColumn = $occurrenceColumn.data("is-scheduler-target-column")
                            if (!isSchedulerTargetColumn) {
                                return false;
                            }

                            var blackoutDates = $resourceDiv.data('blackout-dates');
                            var allowedDate = $resourceDiv.data('occurrenceDate');
                            var singleScheduleMode = $resourceDiv.data('displayed-time-slot-count') === 1;
                            var hasConflict = $resourceDiv.data('has-scheduling-conflict');

                            // If SingleSchedule mode, and there is a conflict, prevent dragging
                            //
                            // If Multi-Schedule mode, the conflicts will be listed, but the person can be dragged into slots.
                            // If that slot has a conflict, an error message will be displayed to explain the conflict
                            if (singleScheduleMode && hasConflict) {
                                return false;
                            }

                            var $targetOccurrence = $(target).closest('.js-scheduled-occurrence')
                            var targetOccurrenceDate = new Date($targetOccurrence.find('.js-attendanceoccurrence-date').val()).getTime();

                            if (blackoutDates) {
                                // In the case of a schedule that has multiple occurrences during the week, the blackout logic is done when attempting to drag, vs preventing them from dragging
                                // if there are blackout dates, see if they are trying to drag into an occurrence within those blackout dates
                                var blackdateDateList = blackoutDates.map(function (d) {
                                    // get the numeric value of date so we can compare
                                    return new Date(d).getTime();
                                })

                                if (blackdateDateList.includes(targetOccurrenceDate)) {
                                    $resourceDiv.data('allow-drop', false);
                                    return false;
                                }
                            }

                            // if getting dragged from the 'No Location Specified' div, restrict to occurrences that have the same date as they signed up for
                            var $sourceOccurrence = $resourceDiv.closest('.js-scheduled-occurrence');
                            //var allowedDate = $resourceDiv.data('occurrenceDate');
                            var hasLocation = $resourceDiv.data('hasLocation');

                            if ($sourceOccurrence.length && hasLocation === 0) {
                                if (allowedDate !== targetOccurrenceDate) {
                                    $resourceDiv.data('allow-drop', false);
                                    return false;
                                }
                            }
                        }

                        $(el).data('allow-drop', true);
                        return true;
                    },
                    invalid: function (el, handle) {
                        // ignore drag if they are clicking on the actions menu of a resource
                        var isMenu = $(el).closest('.js-resource-actions').length;
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
                        if (source === target) {
                            // don't do anything if a person is dragged around within the same occurrence
                            return;
                        }

                        if (target === null) {
                            // don't do anything if a person is dragged into an invalid container
                            return;
                        }

                        if ($(el).data('allow-drop') === false) {
                            // move the el back to the source container
                            $(el).detach().appendTo($(source));
                            return;
                        }

                        if (target.classList.contains('js-scheduler-source-container')) {
                            // deal with the resource that was dragged back into the unscheduled resources
                            var $unscheduledResource = $(el);
                            $unscheduledResource.attr('data-status', 'unscheduled');

                            var personId = $unscheduledResource.attr('data-person-id')

                            var additionalPersonIds = self.$additionalPersonIds.val().split(',');
                            additionalPersonIds.push(personId);

                            self.$additionalPersonIds.val(additionalPersonIds);

                            var $occurrence = $(source).closest('.js-scheduled-occurrence');
                            self.removeResource($unscheduledResource, $occurrence, false);
                        }
                        else {
                            // deal with the resource that was dragged into an scheduled occurrence (location)
                            var scheduledPersonAddPendingUrl = Rock.settings.get('baseUrl') + 'api/Attendances/ScheduledPersonAddPending';
                            var scheduledPersonAddConfirmedUrl = Rock.settings.get('baseUrl') + 'api/Attendances/ScheduledPersonAddConfirmed';
                            var $scheduledResource = $(el);

                            var personId = $scheduledResource.attr('data-person-id');
                            var attendanceOccurrenceId = $(target).closest('.js-scheduled-occurrence').data('attendanceoccurrence-id');

                            var canSchedulePersonUrl = Rock.settings.get('baseUrl') + 'api/Attendances/CanSchedulePerson';
                            var canSchedulePersonParams = '?personId=' + personId + '&attendanceOccurrenceId=' + attendanceOccurrenceId;

                            // if they were dragged from another occurrence, we can exclude the source occurrence when checking CanSchedulePerson
                            if (source.classList.contains('js-scheduler-target-container')) {
                                var fromAttendanceOccurrenceId = $(source).closest('.js-scheduled-occurrence').data('attendanceoccurrence-id');
                                canSchedulePersonParams += '&fromAttendanceOccurrenceId=' + fromAttendanceOccurrenceId;
                            }

                            // first do a GET to CanSchedulePerson to see if the person can be scheduled for the specified occurrence
                            $.ajax({
                                method: "GET",
                                url: canSchedulePersonUrl + canSchedulePersonParams
                            }).done(function () {

                                var $occurrence = $(el).closest('.js-scheduled-occurrence');
                                var scheduledPersonAddUrl = scheduledPersonAddPendingUrl;

                                // if they were dragged from another occurrence, remove them from that first
                                if (source.classList.contains('js-scheduler-target-container')) {

                                    // if getting dragged from one to another, and they were confirmed already, add them as confirmed to the other occurrence
                                    var sourceConfirmationStatus = $scheduledResource.attr('data-status');
                                    if (sourceConfirmationStatus === 'confirmed') {
                                        scheduledPersonAddUrl = scheduledPersonAddConfirmedUrl;
                                    }
                                    var $sourceOccurrence = $(source).closest('.js-scheduled-occurrence');
                                    self.removeResource($scheduledResource, $sourceOccurrence, false);
                                }
                                else {
                                    // if they weren't dragged from another occurrence, set the data-status to pending so it looks correct while waiting for $.ajax to return, but it'll get updated again after posting to scheduledPersonAddUrl
                                    $scheduledResource.attr('data-status', 'pending');
                                }

                                var refreshAllOccurrences = $scheduledResource.data('displayed-time-slot-count') > 1;

                                // add as pending (or confirmed if already confirmed) to target occurrence
                                $.ajax({
                                    method: "PUT",
                                    url: scheduledPersonAddUrl + '?personId=' + personId + '&attendanceOccurrenceId=' + attendanceOccurrenceId
                                }).done(function (scheduledAttendance) {
                                    // after adding a resource, repopulate the list of resources for the occurrence
                                    if (refreshAllOccurrences) {

                                        // After scheduling resource they stay in the list if there are time-slots that can still be dragged into.
                                        // but that resource stays in the list, the Assignments section will need to be repopulated
                                        self.updateSchedulerResource(self.$resourceList, $scheduledResource);

                                        // If there are multiple time-slots during the week for the selected schedule, repopulate all of them to make sure all that the schedule conflict warnings are accurate
                                        self.populateAllScheduledOccurrences();
                                    } else {
                                        // if there is only one time slot shown, we only have to repopulate one occurrence.
                                        // Also, we don't need to repopulate the resources since the person is no longer listed
                                        self.populateScheduledOccurrence($occurrence);
                                    }
                                }).fail(function (jqXHR) {
                                    var $occurrence = $(el).closest('.js-scheduled-occurrence');
                                    self.showSchedulePersonError($occurrence, jqXHR, $scheduledResource);
                                });
                            }).fail(function (jqXHR) {
                                var $occurrence = $(el).closest('.js-scheduled-occurrence');
                                self.showSchedulePersonError($occurrence, jqXHR, $scheduledResource);
                            });

                        }
                        self.trimSourceContainer();
                    });

                this.trimSourceContainer();
                this.initializeEventHandlers();

                self.populateSchedulerResources(self.$resourceList);

                self.populateAllScheduledOccurrences();
            },
            /** trims the source container if it just has whitespace, so that the :empty css selector works */
            trimSourceContainer: function () {
                // if js-scheduler-source-container just has whitespace in it, trim it so that the :empty css selector works
                var $sourceContainer = $('.js-scheduler-source-container');
                if (($.trim($sourceContainer.html()) === "")) {
                    $sourceContainer.html("");
                }
            },
            /** Removes the resource from an occurrence and repopulates the UI */
            removeResource: function ($scheduledResource, $occurrence, rebuildResourceList) {
                var self = this;

                var attendanceId = $scheduledResource.attr('data-attendance-id');
                var refreshAllOccurrences = $scheduledResource.data('displayed-time-slot-count') > 1;

                // unschedule and repopulate ui
                var scheduledPersonRemoveUrl = Rock.settings.get('baseUrl') + 'api/Attendances/ScheduledPersonRemove';

                $.ajax({
                    method: "PUT",
                    url: scheduledPersonRemoveUrl + '?attendanceId=' + attendanceId
                }).done(function (scheduledAttendance) {

                    // $scheduledResource is the div that is being dragged from an occurrence.
                    // There are a few scenarios
                    // 1) The Person is already in the list (they weren't removed when they were scheduled, but they could still be scheduled for other spots)
                    //   - in the case of #1, just find the existing div in $resourceList and update it (and delete the dragged resource div)
                    // 2) The Person was removed via Drag and is not in the Resource List (they got scheduled for all possible visible time-slots)
                    //  - in the case of #2, the dragged div will end up in the resource list (in the position it was dragged to), and we'll update that
                    // 3) The Person was removed via the js-remove button (not dragged) and is also not in the Resource List (they got scheduled for all possible visible time-slots)
                    //  - in the case of #3, then we don't have a $scheduledResource div, so we don't have anything to update. In that case, we'll refresh the whole list
                    if (rebuildResourceList) {
                        // the scheduled person was removed via the js-remove button (case #3), so we can't easily populate the resource list without rebuilding the whole thing
                        self.populateSchedulerResources(self.$resourceList);
                    }
                    else {
                        // After un-scheduling resource they go back into the list (or get updated in the list)
                        // but resource info needs to be updated so the Assignments Section, conflict, etc is repopulated
                        self.updateSchedulerResource(self.$resourceList, $scheduledResource);
                    }

                    // after removing a resource, repopulate the list of resources for the occurrence
                    // If there are multiple occurrences during the week for the selected schedule, repopulate all of them to make sure all that the schedule conflict warnings are accurate
                    if (refreshAllOccurrences) {
                        self.populateAllScheduledOccurrences();

                    } else {
                        self.populateScheduledOccurrence($occurrence);
                    }
                }).fail(function (a) {
                    console.log('fail:' + a.responseText);
                    $(".ajax-error-message").html(a.responseText);
                    $(".ajax-error").show();

                });
            },
            /** populates the scheduled (requested/scheduled) resources for all the occurrence divs */
            populateAllScheduledOccurrences: function () {
                var self = this;
                var occurrenceEls = $(".js-scheduled-occurrence", self.$groupScheduler).toArray();
                $.each(occurrenceEls, function (i) {
                    var $occurrence = $(occurrenceEls[i]);
                    self.populateScheduledOccurrence($occurrence);
                });
            },
            /** populates the scheduled (requested/scheduled) resources for the occurrence div */
            populateScheduledOccurrence: function ($occurrence) {
                var getScheduledUrl = Rock.settings.get('baseUrl') + 'api/Attendances/GetAttendingSchedulerResources';
                var attendanceOccurrenceId = $occurrence.data('attendanceoccurrence-id');
                var $schedulerTargetContainer = $occurrence.find('.js-scheduler-target-container');

                var minimumCapacity = $occurrence.data('minimum-capacity');
                var desiredCapacity = $occurrence.data('desired-capacity');
                var maximumCapacity = $occurrence.data('maximum-capacity');
                var $schedulingStatusContainer = $occurrence.find('.js-scheduling-status');
                var $autoschedulerWarning = $occurrence.find('.js-autoscheduler-warning');
                var occurrenceDate = new Date($occurrence.data('attendanceoccurrence-date')).getTime();
                var hasLocation = $occurrence.data('has-location')

                if (!desiredCapacity) {
                    $autoschedulerWarning.show();
                    $autoschedulerWarning.tooltip({ html: true });
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
                    if ($occurrence.data('has-location') === 0) {
                        if (scheduledAttendanceItems.length === 0) {
                            $occurrence.hide();
                        } else {
                            $occurrence.show();
                        }
                    }

                    // temporarily detach $schedulerTargetContainer to speed up adding the resourcedivs
                    var schedulerTargetContainerParent = $schedulerTargetContainer.parent();
                    $schedulerTargetContainer.detach();

                    $.each(scheduledAttendanceItems, function (i) {
                        var scheduledAttendanceItem = scheduledAttendanceItems[i];

                        // add up status numbers
                        if (scheduledAttendanceItem.ConfirmationStatus === 'confirmed') {
                            totalConfirmed++;
                        } else if (scheduledAttendanceItem.ConfirmationStatus === 'declined') {
                            totalDeclined++;
                        } else {
                            totalPending++;
                        }

                        var $resourceDiv = $('.js-scheduled-resource-template').find('.js-resource').clone();
                        $resourceDiv.data('occurrenceDate', occurrenceDate);
                        $resourceDiv.data('hasLocation', hasLocation);
                        self.populateResourceDiv($resourceDiv, scheduledAttendanceItem);
                        $schedulerTargetContainer.append($resourceDiv);
                    });

                    schedulerTargetContainerParent.append($schedulerTargetContainer);

                    var totalScheduled = totalConfirmed + totalPending;
                    var belowDesired = Math.max(0, (desiredCapacity - totalScheduled));
                    $occurrence.attr('data-total-scheduled', totalScheduled);

                    $occurrence.attr('data-empty-spots', belowDesired);
                    $occurrence.css("--desiredSpots", desiredCapacity);

                    if (minimumCapacity && (totalScheduled < minimumCapacity)) {
                        $occurrence.attr('data-status', 'below-minimum');
                    }
                    else if (desiredCapacity && (totalScheduled < desiredCapacity)) {
                        $occurrence.attr('data-status', 'below-desired');
                    }
                    else if (desiredCapacity && (totalScheduled >= desiredCapacity)) {
                        $occurrence.attr('data-status', 'meets-desired');
                    }
                    else {
                        // no capacities defined, so just hide it
                        $occurrence.attr('data-status', 'none');
                    }

                    // set the progressbar max range to desired capacity if known
                    var progressMax = desiredCapacity;
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
            showSchedulePersonError: function ($occurrence, jqXHR, $draggedPerson) {
                $draggedPerson.remove();

                var self = this;
                var $schedulePersonError = $('.js-scheduler-schedule-person-error', $occurrence);

                // hide any other alerts that are showing
                $('.js-alert', self.$groupScheduler).not($schedulePersonError).hide();
                $schedulePersonError.find('.js-scheduler-schedule-person-error-text').text((jqXHR.responseJSON && jqXHR.responseJSON.Message || jqXHR.responseText));
                $schedulePersonError.show();
            },
            /** gets a specific Scheduler Resource and updates the contents of the $resource with any changed data **/
            updateSchedulerResource: function ($resourceList, $scheduledResource) {
                var self = this;

                var personId = $scheduledResource.attr('data-person-id');
                var groupMemberId = $scheduledResource.attr('data-groupmember-id');

                if (!personId) {
                    return;
                }

                var attendanceOccurrenceScheduleIds = [];
                if ($('.js-occurrence-schedule-ids', $resourceList).val() !== '') {
                    attendanceOccurrenceScheduleIds = $('.js-occurrence-schedule-ids', $resourceList).val().split(',');
                }

                var attendanceOccurrenceLocationIds = [];
                var $attendanceOccurrenceLocationField = $('.js-attendance-occurrence-location-ids', $resourceList);
                if ($attendanceOccurrenceLocationField.val() !== undefined && $attendanceOccurrenceLocationField.val() !== '' && $attendanceOccurrenceLocationField.val() !== "all") {
                    attendanceOccurrenceLocationIds = $attendanceOccurrenceLocationField.val().split(',');
                }
                
                var schedulerResourceParameters = {
                    AttendanceOccurrenceGroupId: Number($('.js-occurrence-group-id', $resourceList).val()),
                    AttendanceOccurrenceScheduleIds: attendanceOccurrenceScheduleIds,
                    AttendanceOccurrenceLocationIds: attendanceOccurrenceLocationIds,
                    AttendanceOccurrenceSundayDate: $('.js-occurrence-sunday-date', $resourceList).val(),
                    ResourceGroupId: $('.js-resource-group-id', $resourceList).val(),
                    GroupMemberFilterType: $('.js-resource-groupmemberfiltertype', $resourceList).val(),
                    ResourceDataViewId: $('.js-resource-dataview-id', $resourceList).val(),
                };

                var getSchedulerResourceUrl = Rock.settings.get('baseUrl') + 'api/Attendances/GetSchedulerResource?personId=' + personId;

                $.ajax({
                    method: "POST",
                    url: getSchedulerResourceUrl,
                    data: schedulerResourceParameters
                }).done(function (schedulerResource) {

                    var dataSelector;
                    if (groupMemberId) {
                        dataSelector = '[data-groupmember-id=' + groupMemberId + ']';
                    }
                    else {
                        dataSelector = '[data-person-id=' + personId + ']';
                    }

                    var $unscheduledResource = $(dataSelector, $resourceList);

                    // remove other resource divs for this groupmember/person (if any)
                    $unscheduledResource.not(':first').remove();

                    var $resourceTemplateDiv = $('.js-unscheduled-resource-template').find('.js-resource');
                    $unscheduledResource.empty();
                    $unscheduledResource.html($resourceTemplateDiv.html());

                    self.populateResourceDiv($unscheduledResource, schedulerResource);
                }).fail(function (a) {
                    console.log('fail:' + a.responseText);
                    $(".ajax-error-message").html(a.responseText);
                    $(".ajax-error").show();

                });
            },
            /** populates the resource list with unscheduled resources */
            populateSchedulerResources: function ($resourceList) {
                var self = this;
                var $resourceContainer = $('.js-scheduler-source-container', $resourceList);
                var getSchedulerResourcesUrl = Rock.settings.get('baseUrl') + 'api/Attendances/GetSchedulerResources';

                // javascript creates a non-empty [''] array on empty string, so only split if there are additionalPersonIds specified
                var additionalPersonIds = [];
                if (self.$additionalPersonIds.val() !== '') {
                    additionalPersonIds = self.$additionalPersonIds.val().split(',');
                }

                var attendanceOccurrenceScheduleIds = [];
                if ($('.js-occurrence-schedule-ids', $resourceList).val() !== '') {
                    attendanceOccurrenceScheduleIds = $('.js-occurrence-schedule-ids', $resourceList).val().split(',');
                }

                var attendanceOccurrenceLocationIds = [];
                var $attendanceOccurrenceLocationField = $('.js-attendance-occurrence-location-ids', $resourceList);
                if ($attendanceOccurrenceLocationField.val() !== undefined && $attendanceOccurrenceLocationField.val() !== '' && $attendanceOccurrenceLocationField.val() !== "all") {
                    attendanceOccurrenceLocationIds = $attendanceOccurrenceLocationField.val().split(',');
                }
                
                var schedulerResourceParameters = {
                    AttendanceOccurrenceGroupId: Number($('.js-occurrence-group-id', $resourceList).val()),
                    AttendanceOccurrenceScheduleIds: attendanceOccurrenceScheduleIds,
                    AttendanceOccurrenceLocationIds: attendanceOccurrenceLocationIds,
                    AttendanceOccurrenceSundayDate: $('.js-occurrence-sunday-date', $resourceList).val(),
                    ResourceGroupId: $('.js-resource-group-id', $resourceList).val(),
                    GroupMemberFilterType: $('.js-resource-groupmemberfiltertype', $resourceList).val(),
                    ResourceListSourceType: $('.js-resource-scheduler-resource-list-source-type', $resourceList).val(),
                    ResourceDataViewId: $('.js-resource-dataview-id', $resourceList).val(),
                    ResourceAdditionalPersonIds: additionalPersonIds
                };

                var $loadingNotification = $resourceList.find('.js-loading-notification');

                $resourceContainer.html(' ');
                $loadingNotification.fadeIn();

                $.ajax({
                    method: "POST",
                    url: getSchedulerResourcesUrl,
                    data: schedulerResourceParameters
                }).done(function (schedulerResources) {
                    var resourceContainerParent = $resourceContainer.parent();

                    // temporarily detach $resourceContainer to speed up adding the resourcedivs
                    $resourceContainer.detach();
                    $resourceContainer.html('');
                    var $resourceTemplate = $('.js-unscheduled-resource-template').find('.js-resource');
                    for (var i = 0; i < schedulerResources.length; i++) {
                        var schedulerResource = schedulerResources[i];
                        var $resourceDiv = $resourceTemplate.clone();
                        self.populateResourceDiv($resourceDiv, schedulerResource);
                        $resourceContainer.append($resourceDiv);
                    }

                    resourceContainerParent.append($resourceContainer);

                    setTimeout(function () {
                        $loadingNotification.hide();
                    }, 0)

                }).fail(function (a) {
                    console.log('fail:' + a.responseText);
                    $(".ajax-error-message").html(a.responseText);
                    $(".ajax-error").show();
                    $loadingNotification.hide();
                });

            },
            /**  populates the resource element (both scheduled and unscheduled) */
            populateResourceDiv: function ($resourceDiv, schedulerResource) {
                $resourceDiv.attr('data-status', schedulerResource.ConfirmationStatus);
                $resourceDiv.attr('data-person-id', schedulerResource.PersonId);
                $resourceDiv.attr('data-has-scheduling-conflict', schedulerResource.HasSchedulingConflict);
                $resourceDiv.attr('data-matches-preference', schedulerResource.MatchesPreference)

                // entirely blacked out for all the occurrences for the selected week and schedule
                $resourceDiv.attr('data-has-blackout-conflict', schedulerResource.HasBlackoutConflict);

                // blacked out for some of the occurrences for the selected week and schedule, but has some dates that are not blacked out
                $resourceDiv.attr('data-has-partial-blackout-conflict', schedulerResource.HasPartialBlackoutConflict);
                $resourceDiv.data('blackout-dates', schedulerResource.BlackoutDates);

                $resourceDiv.attr('data-displayed-time-slot-count', schedulerResource.DisplayedTimeSlotCount);

                $resourceDiv.attr('data-has-requirements-conflict', schedulerResource.HasGroupRequirementsConflict);
                var $resourceMeta = $resourceDiv.find('.js-resource-meta');

                // if the person is a member of the occurrence group, we can show their role and set the groupmember-id
                if (schedulerResource.GroupRole) {
                    var $resourceMemberRole = $resourceDiv.find('.js-resource-member-role');
                    $resourceMemberRole.append(schedulerResource.GroupRole.Name);
                }

                if (schedulerResource.GroupMemberId) {
                    $resourceDiv.attr('data-groupmember-id', schedulerResource.GroupMemberId);
                }
                else {
                    $resourceDiv.find('.js-update-preference').hide();
                }

                if (schedulerResource.ResourcePreferenceList && schedulerResource.ResourcePreferenceList.length > 0) {
                    var $resourcePreferences = $resourceDiv.find('.js-resource-preferences');
                    $resourcePreferences.append("<span class='resource-header'>Preference</span>")
                    for (var i = 0; i < schedulerResource.ResourcePreferenceList.length; i++) {
                        var resourcePreference = schedulerResource.ResourcePreferenceList[i];
                        var preferenceHtml = '<div class="resource-preference">'
                            + '<span class="resource-preference-schedule">'
                            + resourcePreference.ScheduleName
                            + '</span>';

                        if (resourcePreference.LocationName) {
                            preferenceHtml += ' - ' + '<span class="resource-preference-location">'
                                + resourcePreference.LocationName
                                + '</span>'
                                + '</div>';
                        }

                        $resourcePreferences.append(preferenceHtml);
                    }
                }

                if (schedulerResource.ResourceScheduledList && schedulerResource.ResourceScheduledList.length > 0) {
                    var $resourceScheduled = $resourceDiv.find('.js-resource-scheduled');
                    $resourceScheduled.append("<span class='resource-header'>Assignments</span>")
                    for (var i = 0; i < schedulerResource.ResourceScheduledList.length; i++) {
                        var resourceScheduled = schedulerResource.ResourceScheduledList[i];
                        var scheduledHtml = '<div class="resource-scheduled">'
                            + '<span class="resource-scheduled-schedule">'
                            + resourceScheduled.ScheduleName
                            + '</span>';

                        if (resourceScheduled.LocationName) {
                            scheduledHtml += ' - ' + '<span class="resource-scheduled-location">'
                                + resourceScheduled.LocationName
                                + '</span>'
                                + '</div>';
                        }

                        $resourceScheduled.append(scheduledHtml);
                    }
                }

                if (schedulerResource.HasBlackoutConflict) {
                    $resourceDiv.attr('title', schedulerResource.PersonName + " cannot be scheduled due to a blackout.");
                    $resourceDiv.tooltip({ html: true });
                } else if (schedulerResource.HasGroupRequirementsConflict) {
                    $resourceDiv.attr('title', schedulerResource.PersonName + " does not meet the requirements for this group.");
                    $resourceDiv.tooltip({ html: true });
                } else if (schedulerResource.HasSchedulingConflict || schedulerResource.HasPartialBlackoutConflict) {
                    var resourceTooltip = schedulerResource.PersonName;
                    if (schedulerResource.HasSchedulingConflict) {
                        var schedulingConflicts = schedulerResource.SchedulingConflicts;
                        var schedulingConflictHtml = '';
                        for (var i = 0; i < schedulingConflicts.length; i++) {

                            var schedulingConflict = schedulingConflicts[i];
                            var schedulingConflictsMessage = '';
                            schedulingConflictHtml += '<div class="resource-scheduled">'
                                + '<span class="resource-scheduled-schedule">'
                                + schedulingConflict.ScheduleName
                                + '</span>';

                            if (schedulingConflict.GroupId) {
                                schedulingConflictHtml += ' - ' + '<span class="resource-scheduled-group">'
                                    + schedulingConflict.GroupName
                                    + '</span>';
                            }

                            if (schedulingConflict.LocationName) {
                                schedulingConflictHtml += ' - ' + '<span class="resource-scheduled-location">'
                                    + schedulingConflict.LocationName
                                    + '</span>';
                            }

                            schedulingConflictsMessage += schedulingConflictHtml + '</div>';
                        }

                        resourceTooltip += " has scheduling conflicts: <br>" + '<div class="resource-scheduled small">' + schedulingConflictsMessage + '</div';
                    }
                    if (schedulerResource.HasSchedulingConflict && schedulerResource.HasPartialBlackoutConflict) {
                        resourceTooltip += " and";
                    }

                    if (schedulerResource.HasPartialBlackoutConflict) {
                        var blackdateDates = schedulerResource.BlackoutDates.map(function (d) {
                            return new Date(d).toLocaleDateString()
                        })
                        var firstBlackoutDate = blackdateDates[0];
                        var lastBlackoutDate = blackdateDates[blackdateDates.length - 1];
                        resourceTooltip += " has a blackout from " + firstBlackoutDate + " to " + lastBlackoutDate;
                    }

                    $resourceDiv.attr('title', resourceTooltip + ".");
                    $resourceDiv.tooltip({ html: true });
                }

                var resourceName = $resourceDiv.find('.js-resource-name');
                resourceName.text(schedulerResource.PersonName);

                if (schedulerResource.ConfirmationStatus === 'declined') {
                    var resourceNameToolTipHtml = schedulerResource.DeclinedReason || 'No reason given.';
                    resourceName.attr('data-original-title', resourceNameToolTipHtml);
                    resourceName.tooltip({ html: true });
                }                

                if (schedulerResource.Note) {
                    $resourceDiv.addClass('has-note');
                    $resourceMeta.parent().prepend('<div class="resource-note js-resource-note hide-transit">' + schedulerResource.Note + '</div>');
                }

                var $resourceNameMeta = $resourceDiv.find('.js-resource-name-meta');
                if (schedulerResource.ConflictNote) {
                    $resourceNameMeta.append('<span class="resource-warning hide-transit">' + schedulerResource.ConflictNote + '</span>')
                }

                if (schedulerResource.HasSchedulingConflict) {
                    $resourceNameMeta.append('<span class="resource-scheduling-conflict hide-transit" title="Scheduling Conflict"><i class="fa fa-user-clock"></i></span>');
                }

                if (schedulerResource.HasBlackoutConflict) {
                    $resourceNameMeta.append('<span class="resource-blackout-status hide-transit" title="Blackout"><i class="fa fa-user-times"></i></span>');
                }

                if (schedulerResource.HasPartialBlackoutConflict) {
                    $resourceNameMeta.append('<span class="resource-partial-blackout-status hide-transit" title="Partial Blackout"><i class="fa fa-user-clock"></i></span>');
                }

                if (schedulerResource.HasGroupRequirementsConflict) {
                    $resourceNameMeta.append('<span class="resource-requirements-conflict hide-transit" title="Group Requirements Not Met"><i class="fa fa-exclamation-triangle"></i></span>');
                }

                if (schedulerResource.LastAttendanceDateTime) {
                    $resourceNameMeta.append('<span class="resource-lastattendeddate hide-transit" title="Last Attended" data-datetime="' + schedulerResource.LastAttendanceDateTime + '">' + schedulerResource.LastAttendanceDateTimeFormatted + '</span>');
                }

                // stuff that only applies to unscheduled resource
                if (schedulerResource.IsAlreadyScheduledForGroup !== null) {
                    $resourceDiv.attr('data-is-scheduled', schedulerResource.IsAlreadyScheduledForGroup);
                }

                // stuff that only applies to scheduled resource
                if (schedulerResource.AttendanceId) {
                    $resourceDiv.attr('data-attendance-id', schedulerResource.AttendanceId);
                }
            },
            /**  */
            initializeEventHandlers: function () {
                var self = this;

                $('.js-hide-alert', self.$groupScheduler).click(function () {
                    // default bootstrap alert deletes the div, but we want to reuse it so just hide instead
                    $(this).closest('.js-alert').hide();
                });

                self.$groupScheduler.on('click', '.js-markconfirmed, .js-markdeclined, .js-markpending, .js-resendconfirmation, .js-remove', function (a, b, c) {
                    var $resource = $(this).closest('.js-resource');
                    var attendanceId = $resource.attr('data-attendance-id');
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
                        scheduledPersonUrl = Rock.settings.get('baseUrl') + 'api/Attendances/ScheduledPersonSendConfirmationCommunication';
                    }
                    else if ($(this).hasClass('js-remove')) {
                        var $occurrence = $resource.closest('.js-scheduled-occurrence');

                        // In some cases, the removed person is still in the unscheduled list (because they only "partially" scheduled and could be scheduled for other schedules that might be listed)
                        // Also, in the case of "dragging" to remove, we can use the dragged div for the unscheduled div.
                        // In those cases, we only need to refresh the information on the unscheduled resource.
                        // However, in the case of js-remove, we could be in a situation where there isn't a div that we can use to do a simple refresh. If so, we'll rebuild the whole list.
                        var personId = $resource.attr('data-person-id');
                        var rebuildResourceList = true;
                        if (personId) {
                            var unscheduledResourceDataSelector = '[data-person-id=' + personId + ']';
                            rebuildResourceList = $(unscheduledResourceDataSelector, self.$resourceList).length === 0;
                        }

                        // remove (unschedule) the resource (which will also refresh/rebuild the resource list)
                        self.removeResource($resource, $occurrence, rebuildResourceList);

                        return;
                    }
                    else {
                        return;
                    }

                    $.ajax({
                        method: "PUT",
                        url: scheduledPersonUrl + '?attendanceId=' + attendanceId
                    }).done(function () {
                        // after updating a resource, repopulate the list of resources for this occurrence
                        var $occurrence = $resource.closest('.js-scheduled-occurrence');
                        self.populateScheduledOccurrence($occurrence);
                    }).fail(function (a) {
                        console.log('fail:' + a.responseText);
                        $(".ajax-error-message").html(a.responseText);
                        $(".ajax-error").show();
                    })
                });

                // add autoscroll capabilities during dragging
                $(window).on('mousemove', function (e) {
                    if (self.resourceListDrake.dragging) {
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
                        else if (editorMousePositionProportion < .10 && editorScrollLevel !== 0) {
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
