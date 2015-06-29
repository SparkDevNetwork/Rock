$(document).ready(function () {
    Sys.Application.add_load(function () {
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
        })

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
    })
});

//
(function ($) {
    'use strict';
    window.Rock = window.Rock || {};

    Rock.reporting = (function () {
        var _reporting = {},
            exports = {
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

                formatFilterDefault: function (title, $selectedContent) {
                    var compareTypeText = $('.js-filter-compare', $selectedContent).find(':selected').text();
                    var compareValueText = $('.js-filter-control', $selectedContent).val();
                    var result = title;
                    if ($('.js-filter-control', $selectedContent).is(':visible')) {
                        result = title + ' ' + compareTypeText + " '" + compareValueText + "'";
                    } else {
                        result = title + ' ' + compareTypeText;
                    }

                    return result;
                }
            }

        return exports;
    }());
}(jQuery));