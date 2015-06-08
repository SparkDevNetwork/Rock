$(document).ready(function () {
    Sys.Application.add_load(function () {
        // handle what should happen when a different compare type is selected
        function updateFilterControls(filterCompareControl) {
            var $fieldCriteriaRow = $(filterCompareControl).closest('.field-criteria');
            var compareValue = $(filterCompareControl).val();
            var isNullCompare = (compareValue == 32 || compareValue == 64);
            if (isNullCompare) {
                $fieldCriteriaRow.find('.js-filter-control').hide();
            }
            else {
                $fieldCriteriaRow.find('.js-filter-control').show();
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