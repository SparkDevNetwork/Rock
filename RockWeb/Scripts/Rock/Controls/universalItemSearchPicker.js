(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.universalItemSearchPicker = (function () {
        var UniversalItemSearchPicker = function (options) {
            this.controlId = options.controlId;
            this.restUrl = options.restUrl;
            this.areDetailsAlwaysVisible = options.areDetailsAlwaysVisible;
            this.iScroll = null;
            this.$pickerControl = $('#' + this.controlId);
            this.$pickerScrollContainer = this.$pickerControl.find('.js-universalitemsearchpicker-scroll-container');
        };

        UniversalItemSearchPicker.prototype.initializeEventHandlers = function () {
            var controlId = this.controlId;
            var restUrl = this.restUrl;
            var areDetailsAlwaysVisible = this.areDetailsAlwaysVisible;

            var $pickerControl = this.$pickerControl;
            var $searchFields = $pickerControl.find('input.js-universalitemsearchpicker-search-field');
            var $searchFieldName = $pickerControl.find('input.js-universalitemsearchpicker-search-name');
            var $searchFieldIncludeInactive = $pickerControl.find("input.js-include-inactive");
            var $searchResults = $pickerControl.find('.js-universalitemsearchpicker-searchresults');
            var $pickerToggle = $pickerControl.find('.js-universalitemsearchpicker-toggle');
            var $pickerMenu = $pickerControl.find('.js-universalitemsearchpicker-menu');
            var $pickerSelect = $pickerControl.find('.js-universalitemsearchpicker-select');
            var $pickerSelectNone = $pickerControl.find('.js-picker-select-none');
            var $pickerItemValue = $pickerControl.find('.js-item-value');
            var $pickerItemName = $pickerControl.find('.js-item-name');
            var $pickerCancel = $pickerControl.find('.js-universalitemsearchpicker-cancel');
            var promise = null;
            var lastSelectedItemValue = null;

            var autoCompletes = $searchFields.autocomplete({
                source: function (request, response) {

                    var search = {
                        value: $searchFieldName.val()
                    };

                    // make sure that at least one of the search fields has 3 chars in it
                    if (search.value.length < 3) {
                        return;
                    }

                    // abort any searches that haven't returned yet, so that we don't get a pile of results in random order
                    if (promise && promise.state() === 'pending') {
                        promise.abort();
                    }

                    var searchBag = {
                        value: search.value,
                        isInactiveIncluded: $searchFieldIncludeInactive && $searchFieldIncludeInactive.is(":checked") == true
                    };

                    // set the timeout to 20 seconds, just in case it takes a long time to search
                    promise = $.ajax({
                        url: restUrl,
                        method: "POST",
                        data: JSON.stringify(searchBag),
                        contentType: "application/json",
                        timeout: 20000,
                        dataType: 'json'
                    });

                    // Display a wait indicator to show that the search is now running.
                    if ($('.js-searching-notification').length == 0) {
                        $searchResults.prepend('<i class="fa fa-refresh fa-spin margin-l-md js-searching-notification" style="display: none; opacity: .4;"></i>');
                    }
                    $('.js-searching-notification').fadeIn(800);

                    promise.done(function (data) {
                        $searchResults.html('');

                        response(data);

                        exports.universalItemSearchPickers[controlId].updateScrollbar();
                    });

                    promise.fail(function (xhr, status, error) {
                        console.log(status + ' [' + error + ']: ' + xhr.responseText);
                        var errorCode = xhr.status;
                        if (errorCode == 401) {
                            $searchResults.html("<li class='text-danger'>Sorry, you're not authorized to search.</li>");
                        }

                        $('.js-searching-notification').remove();
                    });
                },
                // Set minLength to 0, but check that at least one field has 3 chars before fetching from REST.
                // To minimize load on the server, don't trigger the search until a reasonable delay after the last keypress.
                minLength: 0,
                delay: 750,
                html: true,
                appendTo: $searchResults,
                pickerControlId: controlId,
                messages: {
                    noResults: function () { },
                    results: function () { }
                }
            });

            var autoCompleteCustomRenderItem = function ($ul, item) {
                // override jQueryUI autocomplete's _renderItem so that we can do HTML for the ListItems
                // derived from http://github.com/scottgonzalez/jquery-ui-extensions

                var $div = $('<div/>').attr('class', 'radio');

                var $labelText = $('<span/>')
                    .addClass('label-text d-flex gap')
                    .append($('<span/>').addClass('flex-grow-1').text(item.title));

                if (item.labels) {
                    var $labels = $('<span/>').addClass('d-flex gap');

                    for (let labelIndex = 0; labelIndex < item.labels.length; labelIndex++) {
                        $('<span/>')
                            .text(item.labels[labelIndex].text)
                            .addClass('label label-' + item.labels[labelIndex].value)
                            .appendTo($labels);
                    }

                    $labelText.append($labels);
                }

                var $label = $('<label/>')
                    .append($labelText)
                    .prependTo($div);

                $('<input type="radio" name="item-id" />')
                    .attr('value', item.Id)
                    .prependTo($label);

                var $li = $('<li/>')
                    .addClass('picker-select-item js-picker-select-item')
                    .css('list-style', 'none')
                    .attr('data-item-value', item.value)
                    .attr('data-item-name', item.title)
                    .html($div);

                var $resultSection = $(this.options.appendTo);

                var $itemDetailsDiv = $('<div/>')
                    .addClass('picker-select-item-details js-picker-select-item-details clearfix');

                if (item.description) {
                    var $descriptionDiv = $('<div/>');

                    $descriptionDiv.addClass("picker-select-item-description mb-2");
                    $descriptionDiv.text(item.description);

                    $itemDetailsDiv.append($descriptionDiv);
                }

                if (item.details) {
                    for (let detailIndex = 0; detailIndex < item.details.length; detailIndex++) {
                        var detail = item.details[detailIndex];
                        var $detailDl = $('<dl/>').addClass("d-flex");
                        $detailDl.append($("<dt/>").addClass("mr-2 text-nowrap").text(detail.value));
                        $detailDl.append($("<dd/>").text(detail.text));

                        $itemDetailsDiv.append($detailDl);
                    }
                }

                if (!areDetailsAlwaysVisible) {
                    $itemDetailsDiv.hide();
                }

                $itemDetailsDiv.appendTo($li);

                if (item.isInactive) {
                    $li.addClass('is-inactive');
                }

                return $resultSection.append($li);
            }

            // each search field has its own autocomplete object, so we'll need override with our custom _renderItem to each
            $.each(autoCompletes, function (a, b, c) {
                var autoComplete = $(autoCompletes[a]).data('ui-autocomplete');

                // Debugging: override close to prevent it canceling when loosing focus when debugging
                // autoComplete.close = function () { };

                autoComplete._renderItem = autoCompleteCustomRenderItem;
            });

            $pickerToggle.on('click', function (e) {
                e.preventDefault();
                $(this).toggleClass("active");
                $pickerMenu.toggle(0, function () {
                    exports.universalItemSearchPickers[controlId].updateScrollbar();
                    $searchFieldName.trigger('focus');
                });
            });

            $pickerControl.on('click', '.js-picker-select-item', function (e) {
                if (e.type == 'click' && $(e.target).is(':input') == false) {
                    // only process the click event if it has bubbled up to the input tag
                    return;
                }

                e.stopPropagation();

                var $selectedItem = $(this).closest('.js-picker-select-item');
                var $itemDetails = $selectedItem.find('.js-picker-select-item-details');

                var selectedItemValue = $selectedItem.attr('data-item-value');

                if ($itemDetails.is(':visible')) {
                    if (selectedItemValue == lastSelectedItemValue && e.type == 'click') {
                        // if they are clicking the same item twice in a row
                        // (and the details are done expanding), assume that's
                        // the one they want to pick
                        $pickerSelect.trigger('click');
                    } else {
                        // if it is already visible but isn't the same one twice, just leave it open
                    }
                }

                lastSelectedItemValue = selectedItemValue;

                showItemDetails($selectedItem.find('.picker-select-item-details:hidden'));
            });

            var showItemDetails = function ($itemDetails) {
                if ($itemDetails.length) {
                    $itemDetails.slideDown(function () {
                        exports.universalItemSearchPickers[controlId].updateScrollbar();
                    });
                }
            }

            $pickerControl.on('mouseenter',
                function () {
                    // only show the X if there is something picked
                    if (($pickerItemValue.val() || '') !== '') {
                        $pickerSelectNone.addClass('show-hover');
                    }
                });

            $pickerCancel.on('click', function () {
                clearSearchFields();
                $pickerMenu.slideUp(function () {
                    exports.universalItemSearchPickers[controlId].updateScrollbar();
                });
            });

            $pickerSelectNone.on('click', function (e) {
                // prevent the click from bubbling up to the pickerControl click event
                e.preventDefault();
                e.stopPropagation();

                $pickerItemValue.val('');
                $pickerItemName.val('');
                // run onclick event from the button
                $(this).trigger('onclick');
            });

            // disable the enter key : this will prevent the enter key from clearing the loaded search query.
            $pickerControl.on('keypress', function (e) {
                if (e.which == 13) {
                    return false;
                }
            });

            var setSelectedItem = function (selectedValue, selectedText) {
                var selectedItemLabel = $pickerControl.find('.js-universalitemsearchpicker-selecteditem-label');

                $pickerItemValue.val(selectedValue);
                $pickerItemName.val(selectedText);

                selectedItemLabel.val(selectedValue);
                selectedItemLabel.text(selectedText);

                $pickerMenu.slideUp();
            }

            var clearSearchFields = function () {
                $searchFieldName.val('');
            }

            $pickerSelect.on('click', function () {
                var $selectedItem = $pickerControl.find('input:checked').closest('.js-picker-select-item');
                var selectedItemValue = $selectedItem.attr('data-item-value');
                var selectedText = $selectedItem.attr('data-item-name');

                setSelectedItem(selectedItemValue, selectedText);
                clearSearchFields();

                // Fire the postBack for the Select button.
                var postBackUrl = $pickerSelect.prop('href');
                if (postBackUrl) {
                    window.location = postBackUrl;
                }
            });
        };

        UniversalItemSearchPicker.prototype.updateScrollbar = function () {
            var self = this;

            // first, update this control's scrollbar, then the modal's

            if (self.$pickerScrollContainer.is(':visible')) {
                if (self.iScroll) {
                    self.iScroll.refresh();
                }
            }

            // update the outer modal scrollbar
            Rock.dialogs.updateModalScrollBar(this.controlId);
        }

        UniversalItemSearchPicker.prototype.initialize = function () {
            this.iScroll = new IScroll($('.viewport', this.$pickerControl)[0], {
                mouseWheel: true,
                indicators: {
                    el: $('.track', this.$pickerScrollContainer)[0],
                    interactive: true,
                    resize: false,
                    listenY: true,
                    listenX: false,
                },
                click: false,
                preventDefaultException: { tagName: /.*/ }
            });

            this.initializeEventHandlers();
        };

        var exports = {
            universalItemSearchPickers: {},
            findControl: function (controlId) {
                return exports.universalItemSearchPickers[controlId];
            },
            initialize: function (options) {
                if (!options.controlId) throw '`controlId` is required.';
                if (!options.restUrl) throw '`restUrl` is required.';

                var picker = new UniversalItemSearchPicker(options);
                exports.universalItemSearchPickers[options.controlId] = picker;
                picker.initialize();
            }
        };
        return exports;
    }());
}(jQuery));
