var loadFunction = function () {
    // handle what should happen when a different compare type is selected
    function updateFilterControls(filterCompareControl) {
        var $fieldCriteriaRow = $(filterCompareControl).closest('.field-criteria');
        var compareValue = $(filterCompareControl).val();
        var isNullCompare = (compareValue == 32 || compareValue == 64);
        var isBetweenCompare = (compareValue == 4096);
        if (isNullCompare) {
            $fieldCriteriaRow.find('.js-filter-control').hide();
            $fieldCriteriaRow.find('.js-filter-control-between').hide();
        }
        else if (isBetweenCompare) {
            $fieldCriteriaRow.find('.js-filter-control').hide();
            $fieldCriteriaRow.find('.js-filter-control-between').show();
        }
        else {
            $fieldCriteriaRow.find('.js-filter-control').show();
            $fieldCriteriaRow.find('.js-filter-control-between').hide();
        }
    }

    $('.js-filter-compare').each(function (e) {
        updateFilterControls(this);
    });

    $('.js-filter-compare').change(function () {
        updateFilterControls(this);
    });

    // handle property selection changes from the EntityFieldFilter
    $('select.entity-property-selection').change(function () {
        var $parentRow = $(this).closest('.js-filter-row');
        $parentRow.find('div.field-criteria').hide();
        $parentRow.find('div.field-criteria').eq($(this).find(':selected').index()).show();
    });

    // activity animation on filter field cootrol
    $('.filter-item > header').click(function () {
        $(this).siblings('.panel-body').slideToggle();
        $(this).children('div.pull-left').children('div').slideToggle();

        $expanded = $(this).children('input.filter-expanded');
        $expanded.val($expanded.val() == 'True' ? 'False' : 'True');

        $('a.filter-view-state > i', this).toggleClass('fa-chevron-down');
        $('a.filter-view-state > i', this).toggleClass('fa-chevron-up');
    });

    // fix so that the Remove button will fire its event, but not the parent event 
    $('.filter-item a.btn-danger').click(function (event) {
        event.stopImmediatePropagation();
    });

    $('.filter-item-select').click(function (event) {
        event.stopImmediatePropagation();
    });
}

$(document).ready(function () {
    loadFunction();
    Sys.Application.add_load(loadFunction);
});

//
(function ($) {
    'use strict';
    window.Rock = window.Rock || {};

    Rock.reporting = (function () {
        var _reporting = {},
            exports = {
                //
                formatFilterForDateField: function (title, $selectedContent) {
                    var betweenMode = $('.js-filter-control-between', $selectedContent).is(':visible');
                    var dateValue = '';
                    if (betweenMode) {
                        return title + ' during ' + $('.js-slidingdaterange-text-value', $selectedContent).val();
                    } else {
                        var useCurrentDateOffset = $('.js-current-date-checkbox', $selectedContent).is(':checked');

                        if (useCurrentDateOffset) {
                            var daysOffset = $('.js-current-date-offset', $selectedContent).val();
                            if (daysOffset > 0) {
                                dateValue = 'Current Date plus ' + daysOffset + ' days';
                            }
                            else if (daysOffset < 0) {
                                dateValue = 'Current Date minus ' + -daysOffset + ' days';
                            }
                            else {
                                dateValue = 'Current Date';
                            }
                        }
                        else {
                            dateValue = $('.js-date-picker input', $selectedContent).filter(':visible').val();
                        }
                        return title + ' ' + $('.js-filter-compare', $selectedContent).find(':selected').text() + ' \'' + dateValue + '\''
                    }
                },

                //
                formatFilterForDateTimeField: function (title, $selectedContent) {
                    var betweenMode = $('.js-filter-control-between', $selectedContent).is(':visible');
                    var dateValue = '';
                    var timeValue = '';
                    if (betweenMode) {
                        return title + ' during ' + $('.js-slidingdaterange-text-value', $selectedContent).val();
                    } else {
                        var useCurrentDateOffset = $('.js-current-datetime-checkbox', $selectedContent).is(':checked');

                        if (useCurrentDateOffset) {
                            var minutesOffset = Number($('.js-current-datetime-offset', $selectedContent).val());
                            if (minutesOffset > 0) {
                                dateValue = 'Current Time plus ' + minutesOffset + ' minutes';
                            }
                            else if (minutesOffset < 0) {
                                dateValue = 'Current Time minus ' + -minutesOffset + ' minutes';
                            }
                            else {
                                dateValue = 'Current Time';
                            }
                        }
                        else {
                            dateValue = $('input.js-datetime-date', $selectedContent).filter(':visible').val() || '';
                            timeValue = $('input.js-datetime-time', $selectedContent).filter(':visible').val() || '';
                        }

                        var dateTimeValue = (dateValue + ' ' + timeValue).trim();
                        return title + ' ' + $('.js-filter-compare', $selectedContent).find(':selected').text() + ' \'' + dateTimeValue + '\''
                    }
                },

                //
                formatFilterForDefinedValueField: function (title, $selectedContent) {
                    var selectedItems = '';
                    $('input:checked', $selectedContent).each(
                        function () {
                            selectedItems += selectedItems == '' ? '' : ' OR ';
                            selectedItems += ' \'' + $(this).parent().text() + '\'';
                        });

                    return title + ' is ' + selectedItems
                },

                // NOTE: this is specifically for the Rock.Reporting.DataFilter.OtherDataViewFilter (and similar) components
                formatFilterForOtherDataViewFilter: function (title, $selectedContent) {
                    var dataViewName = $('.js-dataview .js-item-name-value', $selectedContent).val();
                    return title + ' ' + dataViewName;
                },

                //
                formatFilterForSelectSingleField: function (title, $selectedContent) {
                    var selectedItems = '';
                    $('input:checked', $selectedContent).each(
                        function () {
                            selectedItems += selectedItems == '' ? '' : ' OR ';
                            selectedItems += ' \'' + $(this).parent().text() + ' \''
                        });

                    return title + ' is ' + selectedItems
                },

                // NOTE: this is specifically for the Rock.Reporting.DataFilter.Person.InGroupFilter component
                formatFilterForGroupFilterField: function (title, $selectedContent) {
                    var groupNames = $('.js-group-picker', $selectedContent).find('.selected-names').text();
                    var checkedRoles = $('.js-roles', $selectedContent).find(':checked').closest('label');
                    var result = title + ' ' + groupNames;
                    var includeChildGroups = $('.js-include-child-groups', $selectedContent).is(':checked');
                    if (includeChildGroups) {

                        var includeDescendantGroups = $('.js-include-child-groups-descendants', $selectedContent).is(':checked');
                        var includeSelectedGroups = $('.js-include-selected-groups', $selectedContent).is(':checked');
                        var includeInactiveGroups = $('.js-include-inactive-groups', $selectedContent).is(':checked');
                        if (includeDescendantGroups) {
                            result = result + ' OR descendant groups';
                        } else {
                            result = result + ' OR child groups';
                        }

                        if (includeInactiveGroups) {
                            result += ", including inactive groups";
                        }

                        if (!includeSelectedGroups) {
                            result = result + ', NOT including selected groups';
                        }
                    }

                    if (checkedRoles.length > 0) {
                        var roleCommaList = checkedRoles.map(function () { { return $(this).text() } }).get().join(',');
                        result = result + ', with role(s): ' + roleCommaList;
                    }

                    var groupMemberStatus = $('.js-group-member-status option:selected', $selectedContent).text();
                    if (groupMemberStatus) {
                        result = result + ', with member status:' + groupMemberStatus;
                    }
                    
                    var dateAddedDateRangeText = $('.js-dateadded-sliding-date-range .js-slidingdaterange-text-value', $selectedContent).val()
                    if (dateAddedDateRangeText) {
                      result = result + ', added to group in Date Range: ' + dateAddedDateRangeText;
                    }

                    var firstAttendanceDateRangeText = $('.js-firstattendance-sliding-date-range .js-slidingdaterange-text-value', $selectedContent).val()
                    if (firstAttendanceDateRangeText) {
                      result = result + ', first attendance to group in Date Range: ' + firstAttendanceDateRangeText;
                    }

                    var lastAttendanceDateRangeText = $('.js-lastattendance-sliding-date-range .js-slidingdaterange-text-value', $selectedContent).val()
                    if (lastAttendanceDateRangeText) {
                      result = result + ', last attendance to group in Date Range: ' + lastAttendanceDateRangeText;
                    }

                    return result;
                },

                // NOTE: this is specifically for the Rock.Reporting.DataFilter.Person.HasPhoneFilter component
                formatFilterForHasPhoneFilter: function ($content) {

                    var has;
                    if ($('.js-hasphoneoftype', $content).find(':selected').val() == "True") {
                        has = "Has ";
                    } else {
                        has = "Doesn't have ";
                    }

                    var phoneType = $('.js-phonetype', $content).find(':selected').text();
                    var sms = $('.js-hassms', $content).find(':selected').text();

                    if (sms == 'Yes') {
                        sms = ' AND has SMS Enabled';
                    }
                    else if (sms == 'No') {
                        sms = " AND doesn't have SMS Enabled";
                    }

                    var result = has + phoneType + sms;

                    return result;
                },

                //
                formatFilterDefault: function (title, $selectedContent) {
                    var compareTypeText = $('.js-filter-compare', $selectedContent).find(':selected').text();
                    var compareValueText = $('.js-filter-control', $selectedContent).find(':selected').map(function () { return this.text; }).get().join("', '");
                  if (compareValueText == "") {
                    var compareValueText = $('.js-filter-control', $selectedContent).find(':checked').next().map(function () { return $(this).text(); }).get().join("', '");
                    }

                    var result = title;
                    if ($('.js-filter-control', $selectedContent).is(':visible')) {
                        result = title + ' ' + compareTypeText + " '" + compareValueText + "'";
                    } else {
                        result = title + ' ' + compareTypeText;
                    }

                    return result;
                }
            };

        return exports;
    }());
}(jQuery));
