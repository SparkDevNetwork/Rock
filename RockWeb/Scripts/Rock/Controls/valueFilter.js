(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};
    const DefaultComparisonTypes = [
        { Value: 1, Text: 'Equal To' },
        { Value: 2, Text: 'Not Equal To' },
        { Value: 4, Text: 'Starts With' },
        { Value: 8, Text: 'Contains' },
        { Value: 16, Text: 'Does Not Contain' },
        { Value: 32, Text: 'Is Blank' },
        { Value: 64, Text: 'Is Not Blank' },
        { Value: 2048, Text: 'Ends With' }
    ];
    const GroupAll = 1;
    const GroupAny = 2;
    const GroupAllFalse = 3;
    const GroupAnyFalse = 4;

    Rock.controls.valueFilter = (function () {
        //
        // Setup the UI so it reflects the initial data configured on the filter.
        //
        function loadInitialData($filter) {
            var data;

            //
            // Try to parse the initial data.
            //
            try {
                data = JSON.parse($filter.find('input[type="hidden"]').val());
            }
            catch (ex) {
                data = null;
            }

            //
            // Initialize a new filter if we have no current settings.
            //
            if (data === null) {
                data = { ExpressionType: GroupAny, Filters: [] };
                $filter.find('input[type="hidden"]').val(JSON.stringify(data));
            }

            //
            // Grab the objects we need from the filter control.
            //
            var $filterTypeAll = $filter.data('filterTypeAll');
            var $filterTypeAny = $filter.data('filterTypeAny');
            var options = $filter.data('options');

            //
            // Set the initial state of the Any/All toggle buttons.
            //
            if (data.ExpressionType === GroupAll) {
                $filterTypeAll.addClass(options.btnToggleOnClass).removeClass(options.btnToggleOffClass);
                $filterTypeAny.addClass(options.btnToggleOffClass).removeClass(options.btnToggleOnClass);
            }
            else {
                $filterTypeAny.addClass(options.btnToggleOnClass).removeClass(options.btnToggleOffClass);
                $filterTypeAll.addClass(options.btnToggleOffClass).removeClass(options.btnToggleOnClass);
            }

            //
            // Add the initial field rows.
            //
            for (var index in data.Filters) {
                var expression = data.Filters[index];

                addFilterRow($filter, expression.Value, expression.Comparison);
            }

            if (data.Filters.length === 0) {
                addFilterRow($filter, '', 8);
                updateData($filter);
            }
        }

        //
        // Setup the initial event handlers for the controls in filter.
        //
        function addControlEvents($filter) {
            //
            // Load all the objects we need from the filter.
            //
            var $filterTypeAll = $filter.data('filterTypeAll');
            var $filterTypeAny = $filter.data('filterTypeAny');
            var options = $filter.data('options');

            //
            // Add a click handler on the "Any" toggle button that will turn off the
            // "All" option and turn on the "Any" option instead.
            //
            $filterTypeAny.on('click', function (e) {
                e.preventDefault();

                $filterTypeAny.addClass(options.btnToggleOnClass).removeClass(options.btnToggleOffClass);
                $filterTypeAll.addClass(options.btnToggleOffClass).removeClass(options.btnToggleOnClass);

                updateData($filter);
            });

            //
            // Add a click handler on the "All" toggle button that will turn off the
            // "Any" option and turn on the "All" option instead.
            //
            $filterTypeAll.on('click', function (e) {
                e.preventDefault();

                $filterTypeAll.addClass(options.btnToggleOnClass).removeClass(options.btnToggleOffClass);
                $filterTypeAny.addClass(options.btnToggleOffClass).removeClass(options.btnToggleOnClass);

                updateData($filter);
            });

            //
            // Add a click handler to the "Add" button that will add a new filter row.
            //
            $filter.data('addButton').on('click', function (e) {
                e.preventDefault();
                addFilterRow($filter, '', 8);
                updateData($filter);
            });
        }

        //
        // Add a new field row.
        //
        function addFilterRow($filter, value, type) {
            var options = $filter.data('options');
            var $comparisonButton = null;

            //
            // Create the row container as an input-group so things look pretty.
            //
            var $row = $('<div class="input-group margin-t-sm"></div>');

            //
            // Create the text field the user can type into.
            //
            var $text = $('<input type="text" class="form-control">').appendTo($row);

            //
            // Create the drop down that indicates what type of filter row this is.
            //
            var $fieldTypeContainer = $('<span class="input-group-btn" />').appendTo($row);
            var $fieldTypeBtn = $('<button type="button" class="btn btn-default dropdown-toggle" data-toggle="dropdown"><span /> <span class="caret" /></button>')
                .appendTo($fieldTypeContainer);
            var $fieldTypeOptions = $('<ul class="dropdown-menu dropdown-menu-right"></ul>')
                .appendTo($fieldTypeContainer);
            for (var i in options.comparisonTypes) {
                var $btn = $('<li><a href="#" data-value="' + options.comparisonTypes[i].Value + '">' + options.comparisonTypes[i].Text + '</a></li>').appendTo($fieldTypeOptions);
                if (type === options.comparisonTypes[i].Value) {
                    $comparisonButton = $btn;
                }
            }
            $comparisonButton = $comparisonButton || $fieldTypeOptions.find('li:first-child');

            //
            // Create the delete button.
            //
            var $btnDeleteContainer = $('<span class="input-group-btn" />').appendTo($row);
            $('<button type="button" class="btn btn-danger btn-square"><i class="fa fa-times"></i></button>')
                .appendTo($btnDeleteContainer);

            //
            // Set default values.
            //
            $text.val(value);
            changeFilterRowType($filter, $comparisonButton, true);

            //
            // Setup event handlers.
            //
            addFilterRowEvents($filter, $row);

            //
            // Append this new filter row to the DOM.
            //
            $filter.data('rowContainer').append($row);
        }

        //
        // Setup the event handlers for a newly created filter row.
        //
        function addFilterRowEvents($filter, $field) {
            //
            // When the text has changed, update the stored data.
            //
            $field.find('input[type="text"]').on('change', function () {
                updateData($filter);
            });

            //
            // When the user clicks on one of the filter type drop down options, update
            // the filter row type.
            //
            $field.find('ul > li > a').on('click', function (e) {
                e.preventDefault();
                changeFilterRowType($filter, $(this));
            });

            //
            // Remove a single row from the filter.
            //
            $field.find('button.btn-danger').on('click', function (e) {
                e.preventDefault();
                $(this).closest('.input-group').remove();
                updateData($filter);
            });
        }

        //
        // Update the filter row type based on what the user clicked.
        //
        function changeFilterRowType($filter, $btn, skipUpdate) {
            var $btnGroup = $btn.closest('.input-group-btn');
            $btnGroup.find('.dropdown-toggle span:first-child').text($btn.text());

            if ($btn.data('value') === 32 || $btn.data('value') === 64) {
                $filter.find('input[type="text"]').val('').prop('disabled', true);
            }
            else {
                $filter.find('input[type="text"]').prop('disabled', false);
            }

            if (skipUpdate !== true) {
                updateData($filter);
            }
        }

        //
        // Update the hidden field to match what is in the UI.
        //
        function updateData($filter) {
            var options = $filter.data('options');
            var filters = [];

            //
            // Walk each row of the filter and build up the filters collection.
            //
            $filter.data('rowContainer').find('.input-group').each(function (index, element) {
                var $field = $(element);
                var text = $field.find('input[type="text"]').val();
                var typeText = $field.find('button.dropdown-toggle span:first-child').text();
                var type = $field.find('ul li a').filter(function () { return $(this).text() === typeText; }).data('value');

                filters.push({ Value: text, Comparison: type });
            });

            //
            // If there is only one filter, it is a Contains filter and has no value, then get rid of it.
            //
            if (filters.length === 1 && filters[0].Value === '' && filters[0].Comparison === 8) {
                filters = [];
            }

            //
            // Build the final filtered text value.
            //
            var data = {
                ExpressionType: $filter.data('filterTypeAny').hasClass(options.btnToggleOnClass) ? GroupAny : GroupAll,
                Filters: filters
            };

            $filter.find('input[type="hidden"]').val(JSON.stringify(data));
        }

        var exports = {
            initialize: function (options) {
                if (!options.controlId) {
                    throw 'controlId is required';
                }

                //
                // Make sure we have minimum default options
                //
                options = $.extend({
                    btnToggleOnClass: 'btn-info',
                    btnToggleOffClass: 'btn-default',
                    required: true,
                    requiredMessage: 'The field is required',
                    hideFilterMode: false,
                    comparisonTypes: DefaultComparisonTypes,
                    defaultComparison: 8
                }, options);

                //
                // Setup the comparsionTypesByValue object.
                //
                options.comparisonTypeByValue = {};
                for (var i = 0; i < options.comparisonTypes.length; i++) {
                    options.comparisonTypeByValue[options.comparisonTypes[i].Value] = options.comparisonTypes[i].Text;
                }

                //
                // Find the filter control placeholder.
                //
                var $filter = $('#' + options.controlId);

                //
                // Setup the "Any/All" buttons for the filter.
                //
                var $filterTypeContainer = $('<div class="text-right" />').appendTo($filter);
                var $filterTypeGroup = $('<div class="btn-group" role="group" />').appendTo($filterTypeContainer);
                var $filterTypeAny = $('<button type="button" class="btn btn-default btn-xs">Any</button>').appendTo($filterTypeGroup);
                var $filterTypeAll = $('<button type="button" class="btn btn-default btn-xs">All</button>').appendTo($filterTypeGroup);
                if (options.hideFilterMode === true) {
                    $filterTypeContainer.addClass('hidden');
                }

                //
                // Setup the placeholder div that will contain all the filter  rows.
                //
                var $rowContainer = $('<div />').appendTo($filter);

                //
                // Setup the add button.
                //
                var $addButtonContainer = $('<div class="text-right" />').appendTo($filter);
                var $addButton = $('<a href="#" class="btn btn-default btn-square btn-sm margin-t-sm"><i class="fa fa-plus"></i></a>').appendTo($addButtonContainer);

                //
                // Store all the objects we need access to later as jQuery data objects
                // on the filter control.
                //
                $filter.data('options', options);
                $filter.data('filterTypeAny', $filterTypeAny);
                $filter.data('filterTypeAll', $filterTypeAll);
                $filter.data('rowContainer', $rowContainer);
                $filter.data('addButton', $addButton);

                //
                // Setup initial control events.
                //
                addControlEvents($filter);

                //
                // Setup UI to reflect initial data.
                //
                loadInitialData($filter);
            },
            clientValidate: function (validator, args) {
                var $filter = $(validator).prev();
                var required = $filter.data('options').required;
                var data = JSON.parse($filter.find('input[type="hidden"]').val());

                var isValid = !required || data.Filters.length > 0;

                if (isValid) {
                    $filter.closest('.form-group').removeClass('has-error');
                    args.IsValid = true;
                }
                else {
                    $filter.closest('.form-group').addClass('has-error');
                    args.IsValid = false;
                    validator.errormessage = $filter.data('options').requiredMessage;
                }
            }
        };

        return exports;
    }());
}(jQuery));
