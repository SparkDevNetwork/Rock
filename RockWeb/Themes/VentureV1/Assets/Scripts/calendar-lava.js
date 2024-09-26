export default function calendarLava() {

    // Converts Checkboxes into functional select options
    Sys.Application.add_load(function() {
        $('.js-custom-filters').each((i, obj) => {
            $(obj).find('div[id*="pnlFilters"] .rock-control-wrapper').addClass('d-none');
            $(obj).find('div[id*="pnlSearch"]').addClass('d-none');

            $(obj).find('.btn-group.hidden-print').addClass('d-none');

            let filters = $(obj).find('div[id*="pnlFilters"] .form-group');
            let filtersLength = $(filters).size();
            if (filtersLength < 1) {
                filters = $(obj).find('div[id*="pnlSearch"] .form-group');
            }

            let campusDropdown = $(obj).find('.js-custom-dropdown-campus');
            let campusDropdownOptions = $(campusDropdown).find('.dropdown-menu');
            let audienceDropdown = $(obj).find('.js-custom-dropdown-audience');
            let audienceDropdownOptions = $(audienceDropdown).find('.dropdown-menu');
            let preferenceAreaDropdown = $(obj).find('.js-custom-dropdown-preference-area');
            let preferenceAreaDropdownOptions = $(preferenceAreaDropdown).find('.dropdown-menu');

            let campusOptions = $(filters).find('input[Id*="Campus"]');
            let audienceOptions = $(filters).find('input[Id*="Category"]');
            let preferenceAreaOptions = $(filters).find('input[Id*="_cbList"]');

            let campusButton = $('#dropdownMenuCampus');
            let audienceButton = $('#dropdownMenuAudience');
            let preferenceAreaButton = $('#dropdownMenuPreferenceArea');

            addDropdownOptions(campusOptions, campusDropdownOptions);
            addDropdownOptions(audienceOptions, audienceDropdownOptions);
            addDropdownOptions(preferenceAreaOptions, preferenceAreaDropdownOptions);

            linkDropdown(campusOptions, campusDropdownOptions);
            linkDropdown(audienceOptions, audienceDropdownOptions);
            linkDropdown(preferenceAreaOptions, preferenceAreaDropdownOptions);

            updateFilterButtons(campusOptions, campusButton);
            updateFilterButtons(audienceOptions, audienceButton);
            updateFilterButtons(preferenceAreaOptions, preferenceAreaButton);

        });

    });
}

$(document).ready(function () {
    linkPageParams();
});

function addDropdownOptions (options, dropdownOptions) {
    $(options).each((i, obj) => {
        let optionVal = $(obj).val();
        let optionName = $(obj).parent().find('.label-text').html();
        let optionId = $(obj).attr('id');

        if (!optionId.endsWith('_hf')) {
            $(dropdownOptions).append(`<li><a href="#" name="${ optionName }" value="${ optionVal }">${ optionName }</a></li>`);
        }
    });
}

function linkDropdown (options, dropdownOptions) {
    $(dropdownOptions).find('a').each((i, obj) => {
        $(obj).on('click', (e) => {
            e.preventDefault();

            let clickedValue = $(obj).attr('value');

            if (clickedValue == 'All') {
                $(options).each((x, objx) => {
                    if ($(objx).attr('checked')) {
                        $(objx).click();
                    }
                });
            } else {
                $(options).each((x, objx) => {
                    $(objx).attr('checked',false);
                });

                $(options).each((x, objx) => {
                    if ( $(objx).val() == clickedValue ) {
                        $(objx).click();
                    }
                });
            }

            // added for serving opps because the search button is not automatically triggered unlike the event calendar
            let searchPanel = $('div[id*="pnlSearch"]');
            if ($(searchPanel).size() > 0) {
                $('a[Id*=btnSearch]').click();
                window.location.href=`${$('a[Id*=btnSearch]').attr('href')}`;
            }
        });
    });
}

function updateFilterButtons (options, button) {
    $(options).each((i, obj) => {
        if ($(obj).attr('checked')) {
            let name = $(obj).parent().find('.label-text').html();

            $(button).html(`${name} <span class="caret"></span>`);
        }
    })
}

function linkPageParams () {
    let filterParameters = $('.js-custom-dropdown button').map(function () {
        return $(this).data('param');
    }).toArray();

    $(filterParameters).each((i, obj) => {
        let parameter = $.urlParam(filterParameters[i]);
        let paramValue = 0;

        if ( parameter && parameter != 0 ) {
            let paramParts = parameter.split('=');
            paramValue = paramParts[1];
        }

        let filterOptions = $(`.js-custom-dropdown button[data-param="${filterParameters[i]}"] + ul a`);

        $(filterOptions).each((x, objx) => {
            let optionValue = $(objx).attr('value');
            if (optionValue == paramValue) {
                $(objx).click();
            }
        });
    });
}

$.urlParam = function(name){
	var results = new RegExp('[\?&]' + name + '=([^&#]*)').exec(window.location.href);
    if (results) {
        return results[0];
    }
    else {
        return '0'
    }
}

