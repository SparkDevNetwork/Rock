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

                var $blockInstance = $control.closest('.block-instance')[0];
                self.$groupScheduler = $control;
                self.$resourceList = $('.group-scheduler-resourcelist', $control);
                self.$additionalPersonIds = $('.js-resource-additional-person-ids', self.$resourceList);

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

                        if (target.classList.contains('js-scheduler-source-container')) {
                            // deal with the resource that was dragged back into the unscheduled resources
                            var $unscheduledResource = $(el);
                            $unscheduledResource.attr('data-status', 'unscheduled');

                            var personId = $unscheduledResource.attr('data-person-id')

                            var additionalPersonIds = self.$additionalPersonIds.val().split(',');
                            additionalPersonIds.push(personId);

                            self.$additionalPersonIds.val(additionalPersonIds);

                            var attendanceId = $unscheduledResource.attr('data-attendance-id');
                            var $occurrence = $(source).closest('.js-scheduled-occurrence');
                            self.removeResource(attendanceId, $occurrence);
                        }
                        else {
                            // deal with the resource that was dragged into an scheduled occurrence (location)
                            var scheduledPersonAddPendingUrl = Rock.settings.get('baseUrl') + 'api/Attendances/ScheduledPersonAddPending';
                            var scheduledPersonAddConfirmedUrl = Rock.settings.get('baseUrl') + 'api/Attendances/ScheduledPersonAddConfirmed';
                            var $scheduledResource = $(el);

                            var personId = $scheduledResource.attr('data-person-id');
                            var attendanceOccurrenceId = $(target).closest('.js-scheduled-occurrence').find('.js-attendanceoccurrence-id').val();
                            var $occurrence = $(el).closest('.js-scheduled-occurrence');
                            var scheduledPersonAddUrl = scheduledPersonAddPendingUrl;

                            // if they were dragged from another occurrence, remove them from that first
                            if (source.classList.contains('js-scheduler-target-container')) {

                                var attendanceId = $scheduledResource.attr('data-attendance-id')

                                // if getting dragged from one to another, and they were confirmed already, add them as confirmed to the other occurrence
                                var sourceConfirmationStatus = $scheduledResource.attr('data-status');
                                if (sourceConfirmationStatus == 'confirmed') {
                                    scheduledPersonAddUrl = scheduledPersonAddConfirmedUrl;
                                }
                                var $sourceOccurrence = $(source).closest('.js-scheduled-occurrence');
                                self.removeResource(attendanceId, $sourceOccurrence);
                            }
                            else {
                                // if they weren't dragged from another occurrence, set the data-status to pending so it looks correct while waiting for $.ajax to return, but it'll get updated again after posting to scheduledPersonAddUrl
                                $scheduledResource.attr('data-status', 'pending');
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

                        var $resourceDiv = $('.js-scheduled-resource-template').find('.js-resource').clone();
                        self.populateResourceDiv($resourceDiv, scheduledAttendanceItem);
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
                        $statusLight.attr('data-status', 'meets-desired');
                    }
                    else {
                        // no capacities defined, so just hide it
                        $statusLight.attr('data-status', 'none');
                    }

                    // set the progressbar max range to desired capacity if known
                    var progressMax = desiredCapacity;
                    var totalScheduled = (totalPending + totalConfirmed );
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
                        self.populateResourceDiv($resourceDiv, schedulerResource);
                        $resourceContainer.append($resourceDiv);
                    }

                    resourceContainerParent.append($resourceContainer);

                    setTimeout(function () {
                        $loadingNotification.hide();
                    }, 0)

                }).fail(function (a, b, c) {
                    debugger
                    console.log('fail');
                    $loadingNotification.hide();
                });

            },
            /**  populates the resource element (both scheduled and unscheduled) */
            populateResourceDiv: function ($resourceDiv, schedulerResource) {
                $resourceDiv.attr('data-status', schedulerResource.ConfirmationStatus);
                $resourceDiv.attr('data-person-id', schedulerResource.PersonId);
                $resourceDiv.attr('data-has-scheduling-conflict', schedulerResource.HasSchedulingConflict);
                $resourceDiv.attr('data-has-blackout-conflict', schedulerResource.HasBlackoutConflict);
                $resourceDiv.attr('data-has-requirements-conflict', schedulerResource.HasGroupRequirementsConflict);
                var $resourceMeta = $resourceDiv.find('.js-resource-meta');

                if (schedulerResource.HasBlackoutConflict) {
                    $resourceDiv.attr('title', schedulerResource.PersonName + " cannot be scheduled due to a blackout.");
                    $resourceDiv.tooltip();
                } else if (schedulerResource.HasGroupRequirementsConflict) {
                    $resourceDiv.attr('title', schedulerResource.PersonName + " does not meet the requirements for this group.");
                    $resourceDiv.tooltip();
                } else if (schedulerResource.HasSchedulingConflict) {
                    $resourceDiv.attr('title', schedulerResource.PersonName + " has a scheduling conflict.");
                    $resourceDiv.tooltip();
                }

                $resourceDiv.find('.js-resource-name').text(schedulerResource.PersonName);
                if (schedulerResource.Note) {
                    $resourceDiv.addClass('has-note');
                    $resourceMeta.parent().prepend('<div class="resource-note js-resource-note hide-transit">'+ schedulerResource.Note +'</div>');
                }

                if (schedulerResource.ConflictNote) {
                    $resourceMeta.append('<span class="resource-warning hide-transit">' + schedulerResource.ConflictNote + '</span>')
                }

                if (schedulerResource.HasSchedulingConflict) {
                    $resourceMeta.append('<span class="resource-scheduling-conflict hide-transit" title="Scheduling Conflict"><i class="fa fa-user-clock"></i></span>');
                }

                if (schedulerResource.HasBlackoutConflict) {
                    $resourceMeta.append('<span class="resource-blackout-status hide-transit" title="Blackout"><i class="fa fa-user-times"></i></span>');
                }

                if (schedulerResource.HasGroupRequirementsConflict) {
                    $resourceMeta.append('<span class="resource-requirements-conflict hide-transit" title="Group Requirements Not Met"><i class="fa fa-exclamation-triangle"></i></span>');
                }

                if (schedulerResource.LastAttendanceDateTime) {
                    $resourceMeta.append('<span class="resource-lastattendeddate hide-transit" title="Last Attended" data-datetime="' + schedulerResource.LastAttendanceDateTime + '">' + schedulerResource.LastAttendanceDateTimeFormatted + '</span>');
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

                // add autoscroll capabilities during dragging
                $(window).mousemove(function (e) {
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







