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

                if ($control.length == 0) {
                    return;
                }

                self.$groupScheduler = $control;
                self.$resourceList = $('.group-scheduler-resourcelist', $control);
                self.$additionalPersonIds = $('.js-resource-additional-person-ids', self.$resourceList)

                // initialize dragula
                var containers = [];

                // add the resource list as a dragular container
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
                            // don't let resources with blackout or requirement conflicts to be scheduled
                            return false;
                        }
                        return true;
                    },
                    copy: function (el, source) {
                        return false;
                    },
                    accepts: function (el, target) {
                        return true;
                    },
                    invalid: function (el, handle) {
                        // ignore drag if they are clicking on the actions menu of a resource
                        var isMenu = $(el).closest('.js-resource-actions').length;
                        return isMenu;
                    },
                    ignoreInputTextSelection: true
                })
                    .on('drag', function (el) {
                        if (self.resourceScroll) {
                            // disable the scroller while dragging so that the scroller doesn't move while we are dragging
                            //self.resourceScroll.disable();
                        }
                        $('body').addClass('state-drag');
                    })
                    .on('dragend', function (el) {
                        if (self.resourceScroll) {
                            // re-enable the scroller when done dragging
                            //self.resourceScroll.enable();
                        }
                        $('body').removeClass('state-drag');
                    })
                    .on('drop', function (el, target, source, sibling) {
                        if (target.classList.contains('js-scheduler-source-container')) {
                            // deal with the resource that was dragged back into the unscheduled resources
                            var $unscheduledResource = $(el);
                            $unscheduledResource.attr('data-state', 'unscheduled');

                            var personId = $unscheduledResource.attr('data-person-id')

                            var additionalPersonIds = self.$additionalPersonIds.val().split(',');
                            additionalPersonIds.push(personId);

                            self.$additionalPersonIds.val(additionalPersonIds);

                            var attendanceId = $unscheduledResource.attr('data-attendance-id')
                            var $occurrence = $(source).closest('.js-scheduled-occurrence');
                            self.removeResource(attendanceId, $occurrence);
                        }
                        else {
                            // deal with the resource that was dragged into an scheduled occurrence (location)
                            var scheduledPersonAddPendingUrl = Rock.settings.get('baseUrl') + 'api/Attendances/ScheduledPersonAddPending';
                            var scheduledPersonAddConfirmedUrl = Rock.settings.get('baseUrl') + 'api/Attendances/ScheduledPersonAddConfirmed';
                            var $scheduledResource = $(el);
                            $scheduledResource.attr('data-state', 'scheduled');
                            var personId = $scheduledResource.attr('data-person-id')
                            var attendanceOccurrenceId = $(target).closest('.js-scheduled-occurrence').find('.js-attendanceoccurrence-id').val();
                            var $occurrence = $(el).closest('.js-scheduled-occurrence');
                            var scheduledPersonAddUrl = scheduledPersonAddPendingUrl;

                            // if they were dragged from another occurrence, unschedule from that first
                            if (source.classList.contains('js-scheduler-target-container')) {

                                var attendanceId = $scheduledResource.attr('data-attendance-id')

                                // if getting dragged from one to another, and they were confirmed already, add them as confirmed to the other occurrence
                                var sourceConfirmationStatus = $scheduledResource.attr('data-state');
                                if (sourceConfirmationStatus == 'scheduled') {
                                    scheduledPersonAddUrl = scheduledPersonAddConfirmedUrl;
                                }
                                var $sourceOccurrence = $(source).closest('.js-scheduled-occurrence');
                                self.removeResource(attendanceId, $sourceOccurrence);
                            }

                            // add as pending (or confirmed if already confirmed) to target occurrence
                            $.ajax({
                                method: "PUT",
                                url: scheduledPersonAddUrl + '?personId=' + personId + '&attendanceOccurrenceId=' + attendanceOccurrenceId
                            }).done(function (scheduledAttendance) {
                                // after adding a resource, repopulate the list of resources for the occurrence
                                self.populateScheduledOccurrence($occurrence);
                            }).fail(function (a, b, c) {
                                debugger
                                console.log('fail');
                            });
                        }
                        self.trimSourceContainer();
                    });

                this.trimSourceContainer();
                this.initializeEventHandlers();

                self.populateSchedulerResources(self.$resourceList);

                var occurrenceEls = $(".js-scheduled-occurrence", $control).toArray();
                $.each(occurrenceEls, function (i) {
                    var $occurrence = $(occurrenceEls[i]);
                    self.populateScheduledOccurrence($occurrence);
                });
            },
            /** trims the source container if it just has whitespace, so that the :empty css selector works */
            trimSourceContainer: function () {
                // if js-scheduler-source-container just has whitespace in it, trim it so that the :empty css selector works
                var $sourceContainer = $('.js-scheduler-source-container');
                if (($.trim($sourceContainer.html()) == "")) {
                    $sourceContainer.html("");
                }
            },
            /** Removes the resource and repopulates the UI */
            removeResource: function (attendanceId, $occurrence) {
                var self = this;

                // unschedule and repopulate ui
                var scheduledPersonRemoveUrl = Rock.settings.get('baseUrl') + 'api/Attendances/ScheduledPersonRemove';

                $.ajax({
                    method: "PUT",
                    url: scheduledPersonRemoveUrl + '?attendanceId=' + attendanceId
                }).done(function (scheduledAttendance) {
                    // after removing a resource, repopulate the list of unscheduled resources
                    self.populateSchedulerResources(self.$resourceList);

                    // after removing a resource, repopulate the list of resources for the occurrence
                    self.populateScheduledOccurrence($occurrence);
                }).fail(function (a, b, c) {
                    debugger
                    console.log('fail');
                });
            },
            /** populates the scheduled (requested/scheduled) resources for the occurrence div */
            populateScheduledOccurrence: function ($occurrence) {
                var getScheduledUrl = Rock.settings.get('baseUrl') + 'api/Attendances/GetAttendingSchedulerResources';
                var attendanceOccurrenceId = $occurrence.find('.js-attendanceoccurrence-id').val();
                var $schedulerTargetContainer = $occurrence.find('.js-scheduler-target-container');

                var minimumCapacity = $occurrence.find('.js-minimum-capacity').val();
                var desiredCapacity = $occurrence.find('.js-desired-capacity').val();
                var maximumCapacity = $occurrence.find('.js-maximum-capacity').val();
                var $schedulingStatusContainer = $occurrence.find('.js-scheduling-status');
                var $autoschedulerWarning = $occurrence.find('.js-autoscheduler-warning');
                if (!desiredCapacity) {
                    $autoschedulerWarning.show();
                    $autoschedulerWarning.tooltip({ container: 'body' });
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

                        var $resourceDiv = $('.js-scheduled-resource-template').find('.js-resource').clone();
                        self.populateResourceDiv($resourceDiv, scheduledAttendanceItem, 'scheduled');
                        $schedulerTargetContainer.append($resourceDiv);
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
                        $statusLight.attr('data-status', 'meets-desired'); ``
                    }
                    else {
                        // no capacities defined, so just hide it
                        $statusLight.attr('data-status', 'none');
                    }

                    // set the progressbar max range to desired capacity if known
                    var progressMax = desiredCapacity;
                    var totalScheduled = (totalPending + totalConfirmed + totalDeclined);
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
                    $schedulingStatusContainer.tooltip({ 'html': 'true', container: 'body' });

                    var confirmedPercent = !progressMax || (totalConfirmed * 100 / progressMax);
                    var pendingPercent = !progressMax || (totalPending * 100 / progressMax);
                    var declinedPercent = !progressMax || (totalDeclined * 100 / progressMax);
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

                    var $progressDeclined = $schedulingStatusContainer.find('.js-scheduling-progress-declined');
                    $progressDeclined.css({ 'width': declinedPercent + '%' });
                    $progressDeclined.find('.js-progress-text-percent').val(declinedPercent);


                });
            },
            /** populates the resource list with unscheduled resources */
            populateSchedulerResources: function ($resourceList) {
                var self = this;
                var $resourceContainer = $('.js-scheduler-source-container', $resourceList);
                var getSchedulerResourcesUrl = Rock.settings.get('baseUrl') + 'api/Attendances/GetSchedulerResources';

                // javascript creates a non-empty [''] array on empty string, so only split if there are additionalPersonIds specified
                var additionalPersonIds = [];
                if (self.$additionalPersonIds.val() != '') {
                    additionalPersonIds = self.$additionalPersonIds.val().split(',');
                }

                var schedulerResourceParameters = {
                    AttendanceOccurrenceGroupId: Number($('.js-occurrence-group-id', $resourceList).val()),
                    AttendanceOccurrenceScheduleId: Number($('.js-occurrence-schedule-id', $resourceList).val()),
                    AttendanceOccurrenceSundayDate: $('.js-occurrence-sunday-date', $resourceList).val(),
                    ResourceGroupId: $('.js-resource-group-id', $resourceList).val(),
                    GroupMemberFilterType: $('.js-resource-groupmemberfiltertype', $resourceList).val(),
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
                        self.populateResourceDiv($resourceDiv, schedulerResource, 'unscheduled');
                        $resourceContainer.append($resourceDiv);
                    }

                    resourceContainerParent.append($resourceContainer);

                    setTimeout(function () {
                        $loadingNotification.hide();
                        //self.resourceScroll.refresh();
                    }, 0)

                }).fail(function (a, b, c) {
                    debugger
                    console.log('fail');
                    $loadingNotification.hide();
                });

            },
            /**  populates the resource element (both scheduled and unscheduled) */
            populateResourceDiv: function ($resourceDiv, schedulerResource, state) {
                $resourceDiv.attr('data-state', state);
                $resourceDiv.attr('data-person-id', schedulerResource.PersonId);
                $resourceDiv.attr('data-has-scheduling-conflict', schedulerResource.HasSchedulingConflict);
                $resourceDiv.attr('data-has-blackout-conflict', schedulerResource.HasBlackoutConflict);
                $resourceDiv.attr('data-has-requirements-conflict', schedulerResource.HasGroupRequirementsConflict);

                if (schedulerResource.HasBlackoutConflict) {
                    $resourceDiv.attr('title', schedulerResource.PersonName + " cannot be scheduled due to a blackout.");
                    $resourceDiv.tooltip({ container: 'body' });
                }

                $resourceDiv.find('.js-resource-name').text(schedulerResource.PersonName);
                if (schedulerResource.Note) {
                    $resourceDiv.find('.js-resource-note').text(schedulerResource.Note);
                }

                if (schedulerResource.ConflictNote) {
                    $resourceDiv.find('.js-resource-warning').text(schedulerResource.ConflictNote);
                }

                if (schedulerResource.LastAttendanceDateTime) {
                    var $lastAttendedDate = $resourceDiv.find('.js-resource-lastattendeddate');
                    $lastAttendedDate.attr('data-datetime', schedulerResource.LastAttendanceDateTime);
                    $lastAttendedDate.text(schedulerResource.LastAttendanceDateTimeFormatted);
                }


                if (schedulerResource.ConfirmationStatus) {
                    $resourceDiv.find('.js-resource-status').attr('data-status', schedulerResource.ConfirmationStatus);
                }

                // stuff that only applies to unscheduled resource
                if (schedulerResource.IsAlreadyScheduledForGroup != null) {
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

                self.$groupScheduler.on('click', '.js-markconfirmed, .js-markdeclined, .js-markpending, .js-resendconfirmation', function (a, b, c) {
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
                        scheduledPersonUrl = Rock.settings.get('baseUrl') + 'api/Attendances/ScheduledPersonSendConfirmationEmail';
                    }
                    else {
                        return;
                    }

                    var attendanceId = $resource.attr('data-attendance-id')
                    $.ajax({
                        method: "PUT",
                        url: scheduledPersonUrl + '?attendanceId=' + attendanceId
                    }).done(function () {
                        // after updating a resource, repopulate the list of resources for this occurrence
                        var $occurrence = $resource.closest('.js-scheduled-occurrence');
                        self.populateScheduledOccurrence($occurrence);
                    }).fail(function (a, b, c) {
                        debugger
                        console.log('fail');
                    })
                });

            }
        };

        return exports;
    }());
}(jQuery));







