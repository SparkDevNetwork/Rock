(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};
    Rock.controls.emailEditor = Rock.controls.emailEditor || {};
    Rock.controls.emailEditor.$currentRsvpComponent = $(false);

    Rock.controls.emailEditor.rsvpComponentHelper = (function () {
        var exports = {
            initializeEventHandlers: function () {
                var self = this;

                $('#component-rsvp-group .rsvp-group .js-item-id-value').on('change', function (e) {
                    self.setGroup();
                });
                $('#component-rsvp-occurrence').on('change', function (e) {
                    self.setOccurrence();
                });

                $('#component-rsvp-accepttext').on('input', function (e) {
                    self.setAcceptButtonText();
                });
                $('#component-rsvp-acceptbackgroundcolor').colorpicker().on('changeColor', function () {
                    self.setAcceptButtonBackgroundColor();
                });
                $('#component-rsvp-acceptfontcolor').colorpicker().on('changeColor', function () {
                    self.setAcceptButtonFontColor();
                });

                $('#component-rsvp-declinetext').on('input', function (e) {
                    self.setDeclineButtonText();
                });
                $('#component-rsvp-declinebackgroundcolor').colorpicker().on('changeColor', function () {
                    self.setDeclineButtonBackgroundColor();
                });
                $('#component-rsvp-declinefontcolor').colorpicker().on('changeColor', function () {
                    self.setDeclineButtonFontColor();
                });

                $('#component-rsvp-buttonfont').on('change', function (e) {
                    self.setButtonFont();
                });
                $('#component-rsvp-buttonfontweight').on('change', function (e) {
                    self.setButtonFontWeight()
                });
                $('#component-rsvp-buttonfontsize').on('input', function (e) {
                    self.setButtonFontSize();
                });
                $('#component-rsvp-buttonpadding').on('input', function (e) {
                    self.setButtonPadding();
                });
                $('#component-rsvp-buttonalign').on('change', function (e) {
                    self.setButtonAlign();
                });
            },
            setProperties: function ($rsvpComponent) {
                Rock.controls.emailEditor.$currentRsvpComponent = $rsvpComponent;
                var selectedGroupId = $rsvpComponent.find('.rsvp-group-id').val();
                var selectedOccurrenceValue = $rsvpComponent.find('.rsvp-occurrence-value').val();

                var acceptButtonText = $rsvpComponent.find('.rsvp-accept-link').text();
                var declineButtonText = $rsvpComponent.find('.rsvp-decline-link').text();
                var acceptButtonBackgroundColor = $rsvpComponent.find('.accept-button-shell').css('backgroundColor');
                var declineButtonBackgroundColor = $rsvpComponent.find('.decline-button-shell').css('backgroundColor');
                var acceptButtonFontColor = $rsvpComponent.find('.rsvp-accept-link').css('color');
                var declineButtonFontColor = $rsvpComponent.find('.rsvp-decline-link').css('color');

                var buttonAlign = $rsvpComponent.find('.rsvp-innerwrap').attr('align');
                var buttonFont = $rsvpComponent.find('.rsvp-accept-link').css('font-family');
                var buttonFontWeight = $rsvpComponent.find('.rsvp-accept-link')[0].style['font-weight'];
                var buttonFontSize = $rsvpComponent.find('.rsvp-accept-link').css('font-size');
                var buttonPadding = $rsvpComponent.find('.rsvp-accept-content')[0].style['padding'];

                var selectElement = $('#component-rsvp-occurrence');
                selectElement.html('');
                selectElement.append('<option value=""></option>');

                $('#component-rsvp-group .rocktree-name').removeClass('selected');
                if ((selectedGroupId == '') || (selectedGroupId == '0')) {
                    $('.js-rsvp-advanced-settings').hide();
                    $('.js-rsvp-show-advanced-settings').addClass('disabled').text('Show Advanced Settings');
                    $('#component-rsvp-occurrence').attr('disabled', 'disabled');
                    $('#component-rsvp-group .rsvp-group .js-item-id-value').val('');
                    $('#component-rsvp-group .rsvp-group .js-item-name-value').val('');
                    $('#component-rsvp-group .rsvp-group .selected-names').text('');
                    $('#component-rsvp-group .rsvp-group .picker-select-none').removeClass('rollover-item');
                    $('#component-rsvp-group .rsvp-group .picker-select-none').hide();
                } else {
                    this.getOccurrenceValues(selectedGroupId, selectedOccurrenceValue);
                    $('#component-rsvp-occurrence').removeAttr('disabled');
                    $('#component-rsvp-group .rocktree-item[data-id="' + selectedGroupId + '"] .rocktree-name').addClass('selected');
                    var groupName = $('#component-rsvp-group .rocktree-item[data-id="' + selectedGroupId + '"] .rocktree-name').text();
                    $('#component-rsvp-group .rsvp-group .js-item-id-value').val(groupName);
                    $('#component-rsvp-group .rsvp-group .js-item-name-value').val(groupName);
                    $('#component-rsvp-group .rsvp-group .selected-names').text(groupName);
                    $('#component-rsvp-group .picker-select-none').addClass('rollover-item');
                    $('#component-rsvp-group .picker-select-none').show();
                }

                $('#component-rsvp-accepttext').val(acceptButtonText);
                $('#component-rsvp-declinetext').val(declineButtonText);
                $('#component-rsvp-acceptbackgroundcolor').colorpicker('setValue', acceptButtonBackgroundColor);
                $('#component-rsvp-declinebackgroundcolor').colorpicker('setValue', declineButtonBackgroundColor);
                $('#component-rsvp-acceptfontcolor').colorpicker('setValue', acceptButtonFontColor);
                $('#component-rsvp-declinefontcolor').colorpicker('setValue', declineButtonFontColor);

                $('#component-rsvp-buttonalign').val(buttonAlign);
                $('#component-rsvp-buttonfont').val(buttonFont);
                $('#component-rsvp-buttonfontweight').val(buttonFontWeight);
                $('#component-rsvp-buttonfontsize').val(buttonFontSize);
                $('#component-rsvp-buttonpadding').val(buttonPadding);
            },

            setGroup: function () {
                var groupId = $('#component-rsvp-group .rsvp-group .js-item-id-value').val();

                var selectElement = $('#component-rsvp-occurrence');
                selectElement.html('');
                selectElement.append('<option value=""></option>');

                if ((groupId == '') || (groupId == '0')) {
                    $('.js-rsvp-advanced-settings').hide();
                    $('.js-rsvp-show-advanced-settings').addClass('disabled').text('Show Advanced Settings');
                    $('#component-rsvp-occurrence').attr('disabled', 'disabled');

                    Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-group-id').val('');
                    Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-occurrence-value').val('');
                } else {
                    var selectedGroupId = Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-group-id').val();
                    if (groupId != selectedGroupId) {
                        $('#component-rsvp-occurrence').removeAttr('disabled');
                        Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-group-id').val(groupId);
                        this.getOccurrenceValues(groupId, '');
                    }
                }
            },
            handleOccurrenceAjaxResponse: function (ajaxResult, selectedValue) {
                var selectValues = {};
                var locationIds = '';

                if (ajaxResult != '') {
                    $.each(ajaxResult, function (k, v) {
                        if (v.LocationId) {
                            if (locationIds != '') { locationIds += ','; }
                            locationIds += v.LocationId;
                        }
                        selectValues[k] = v.Id + '|' + v.GroupId + '|' + v.LocationId + '|' + v.ScheduleId + '|' + v.OccurrenceDate
                    });
                }

                self = this;
                if (locationIds != '') {
                    // AJAX call to get location titles.
                    var callback = function (response) {
                        self.handleLocationTitleAjaxResponse(response, selectValues, selectedValue);
                    };
                    restUrl = 'api/locations/GetLocationTitles?locationIds=' + locationIds;
                    $.ajax({
                        url: restUrl,
                        async: false,
                        success: callback
                    });
                } else {
                    self.handleLocationTitleAjaxResponse('', selectValues, selectedValue);
                }

            },
            handleLocationTitleAjaxResponse: function (ajaxResult, selectValues, selectedValue) {
                var locationTitles = {};
                if (ajaxResult != '') {
                    $.each(result, function (k, v) {
                        console.log(v);
                        locationTitles[k] = v;
                    });
                }

                var selectElement = $('#component-rsvp-occurrence');
                // Loop through each of the results and append the option to the dropdown
                for (var key in selectValues) {
                    var optionValue = selectValues[key];
                    var locationId = optionValue.split('|')[2];
                    var occurrenceDate = optionValue.split('|')[4];
                    //BUG format date for display;
                    var objDate = new Date(JSON.parse('"' + occurrenceDate + '"'))
                    var displayValue = objDate.toLocaleString('default', { month: 'long' }) + ' ' + objDate.getDate() + ', ' + objDate.getFullYear() + ' (' + objDate.toLocaleTimeString() + ')';
                    if (locationId && locationId != "null" && locationId != '') {
                        displayValue += ' - ' + locationTitles[locationId];
                    }
                    selectElement.append('<option value="' + optionValue + '">' + displayValue + '</option>');
                }

                selectElement.val(selectedValue);

            },
            getOccurrenceValues: function (groupId, selectedValue) {
                if (selectedValue == '') {
                    $('.js-rsvp-advanced-settings').hide();
                    $('.js-rsvp-show-advanced-settings').addClass('disabled').text('Show Advanced Settings');
                } else {
                    $('.js-rsvp-show-advanced-settings').removeClass('disabled');
                }

                // AJAX call to get occurrences.
                self = this;
                var callback = function (response) {
                    self.handleOccurrenceAjaxResponse(response, selectedValue);
                };
                var restUrl = 'api/AttendanceOccurrences/GetFutureGroupOccurrences?groupId=' + groupId;
                $.ajax({
                    url: restUrl,
                    success: callback
                });
            },
            setOccurrence: function () {
                var occurrenceValue = $('#component-rsvp-occurrence').val();
                if (occurrenceValue == '') {
                    $('.js-rsvp-advanced-settings').hide();
                    $('.js-rsvp-show-advanced-settings').addClass('disabled').text('Show Advanced Settings');
                    Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-occurrence-value').val('');
                } else {
                    $('.js-rsvp-show-advanced-settings').removeClass('disabled');

                    var occurrenceValueParameters = occurrenceValue.split('|');
                    var occurrenceId = occurrenceValueParameters[0];
                    var groupId = occurrenceValueParameters[1];
                    var locationId = occurrenceValueParameters[2];
                    var scheduleId = occurrenceValueParameters[3];
                    var occurrenceDate = occurrenceValueParameters[4];

                    if (occurrenceId == '0') {
                        //  This occurrence doesn't exist, so we need to make an AJAX call to create it, and then update the selected value to include the correct ID.
                        var restUrl = 'api/AttendanceOccurrences/CreateGroupOccurrence?groupId=' + groupId + '&occurrenceDate=' + occurrenceDate;
                        if (locationId != 'null') {
                            restUrl = restUrl + '&locationId=' + locationId;
                        }
                        if (scheduleId != 'null') {
                            restUrl = restUrl + '&scheduleId=' + scheduleId;
                        }
                        $.ajax({
                            url: restUrl,
                            method: 'POST',
                            success: function (result) {
                                occurrenceId = result.Id;
                                $('#component-rsvp-occurrence option:selected').val(occurrenceId + '|' + groupId + '|' + locationId + '|' + scheduleId + '|' + occurrenceDate);

                                Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-occurrence-value').val(occurrenceId + '|' + groupId + '|' + locationId + '|' + scheduleId + '|' + occurrenceDate);
                                Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-accept-link').attr('href', '{{ \'Global\' | Attribute:\'PublicApplicationRoot\' }}RSVP?isAccept=1&p={{ Person | PersonActionIdentifier:\'RSVP\' }}&AttendanceOccurrenceId=' + occurrenceId);
                                Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-decline-link').attr('href', '{{ \'Global\' | Attribute:\'PublicApplicationRoot\' }}RSVP/?isAccept=0&p={{ Person | PersonActionIdentifier:\'RSVP\' }}&AttendanceOccurrenceId=' + occurrenceId);
                            }
                        });
                    } else {
                        Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-occurrence-value').val(occurrenceId + '|' + groupId + '|' + locationId + '|' + scheduleId + '|' + occurrenceDate);
                        Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-accept-link').attr('href', '{{ \'Global\' | Attribute:\'PublicApplicationRoot\' }}RSVP?isAccept=1&p={{ Person | PersonActionIdentifier:\'RSVP\' }}&AttendanceOccurrenceId=' + occurrenceId);
                        Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-decline-link').attr('href', '{{ \'Global\' | Attribute:\'PublicApplicationRoot\' }}RSVP/?isAccept=0&p={{ Person | PersonActionIdentifier:\'RSVP\' }}&AttendanceOccurrenceId=' + occurrenceId);
                    }
                }
            },

            setAcceptButtonText: function () {
                var text = $('#component-rsvp-accepttext').val();
                Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-accept-link')
                    .text(text)
                    .attr('title', text);
            },
            setAcceptButtonBackgroundColor: function () {
                var color = $('#component-rsvp-acceptbackgroundcolor').colorpicker('getValue');
                Rock.controls.emailEditor.$currentRsvpComponent.find('.accept-button-shell').css('backgroundColor', color);
            },
            setAcceptButtonFontColor: function () {
                var color = $('#component-rsvp-acceptfontcolor').colorpicker('getValue');
                Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-accept-link').css('color', color);
            },
            
            setDeclineButtonText: function () {
                var text = $('#component-rsvp-declinetext').val();
                Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-decline-link')
                    .text(text)
                    .attr('title', text);
            },
            setDeclineButtonBackgroundColor: function () {
                var color = $('#component-rsvp-declinebackgroundcolor').colorpicker('getValue');
                Rock.controls.emailEditor.$currentRsvpComponent.find('.decline-button-shell').css('backgroundColor', color);
            },
            setDeclineButtonFontColor: function () {
                var color = $('#component-rsvp-declinefontcolor').colorpicker('getValue');
                Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-decline-link').css('color', color);
            },

            setButtonFont: function () {
                var selectValue = $('#component-rsvp-buttonfont').val();
                Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-accept-link').css('font-family', selectValue);
                Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-decline-link').css('font-family', selectValue);
            },
            setButtonFontWeight: function () {
                var selectValue = $('#component-rsvp-buttonfontweight').val();
                Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-accept-link').css('font-weight', selectValue);
                Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-decline-link').css('font-weight', selectValue);
            },
            setButtonFontSize: function () {
                var text = $('#component-rsvp-buttonfontsize').val()
                Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-accept-link').css('font-size', text);
                Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-decline-link').css('font-size', text);
            },
            setButtonPadding: function () {
                var text = $('#component-rsvp-buttonpadding').val()
                Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-accept-content').css('padding', text);
                Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-decline-content').css('padding', text);
            },
            setButtonAlign: function () {
                var selectValue = $('#component-rsvp-buttonalign').val();
                Rock.controls.emailEditor.$currentRsvpComponent.find('.rsvp-innerwrap')
                    .attr('align', selectValue)
                    .css('text-align', selectValue);
            }
        }

        return exports;
    }());
}(jQuery));



