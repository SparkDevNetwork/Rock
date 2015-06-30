(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.personPicker = (function () {
        var PersonPicker = function (options) {
            this.controlId = options.controlId;
            this.restUrl = options.restUrl;
            this.restDetailUrl = options.restDetailUrl;
            this.defaultText = options.defaultText || '';
        };

        PersonPicker.prototype.initializeEventHandlers = function () {
            var controlId = this.controlId,
                restUrl = this.restUrl,
                restDetailUrl = this.restDetailUrl || (Rock.settings.get('baseUrl') + 'api/People/GetSearchDetails'),
                defaultText = this.defaultText;

            var includeBusinesses = $('#' + controlId).find('.js-include-businesses').val() == '1' ? 'true' : 'false';

            $('#' + controlId + '_personPicker').autocomplete({
                source: function (request, response) {
                    var promise = $.ajax({
                        url: restUrl + "?name=" + encodeURIComponent(request.term) + "&includeHtml=false&includeDetails=false&includeBusinesses=" + includeBusinesses + "&includeDeceased=true",
                        dataType: 'json'
                    });

                    promise.done(function (data) {
                        $('#' + controlId + '_personPickerItems').first().html('');
                        response($.map(data, function (item) {
                            return item;
                        }));

                        exports.personPickers[controlId].updateScrollbar();
                    });

                    // Is this needed? If an error is thrown on the server, we should see an exception in the log now...
                    promise.fail(function (xhr, status, error) {
                        console.log(status + ' [' + error + ']: ' + xhr.responseText);
                        var errorCode = xhr.status;
                        if (errorCode == 401) {
                            $('#' + controlId + '_personPickerItems').first().html("<li class='text-danger'>Sorry, you're not authorized to search.</li>");
                        }
                    });
                },
                minLength: 3,
                html: true,
                appendTo: '#' + controlId + '_personPickerItems',
                pickerControlId: controlId,
                messages: {
                    noResults: function () { },
                    results: function () { }
                }
            });

            $('#' + controlId + ' a.picker-label').click(function (e) {
                e.preventDefault();
                $('#' + controlId).find('.picker-menu').first().slideToggle(function () {
                    exports.personPickers[controlId].updateScrollbar();
                    $(this).find('.picker-search').focus();
                });
            });

            $('#' + controlId + ' .picker-select').on('click', '.picker-select-item :input', function (e) {
                e.stopPropagation();

                var $selectedItem = $(this).closest('.picker-select-item');

                var selectedPersonId = $selectedItem.attr('data-person-id');
                var alreadySelected = $selectedItem.find('.picker-select-item-details').is(':visible');
                if (alreadySelected) {
                    $('#' + controlId + '_btnSelect').get(0).click();
                }

                // hide other open details
                $('#' + controlId + ' .picker-select-item-details').each(function () {
                    var $el = $(this),
                       currentPersonId = $el.closest('.picker-select-item').attr('data-person-id');

                    if (currentPersonId != selectedPersonId) {
                        $el.slideUp();
                    }
                });

                var $itemDetails = $selectedItem.find('.picker-select-item-details');

                debugger

                if ($itemDetails.attr('data-has-details') == 'false') {
                    $itemDetails.load(restDetailUrl + '?Id=' + selectedPersonId, function (a, b, c) {
                        exports.personPickers[controlId].updateScrollbar();
                    });
                }

                $selectedItem.find('.picker-select-item-details:hidden').slideDown(function () {
                    exports.personPickers[controlId].updateScrollbar();
                });
            });

            $('#' + controlId).hover(
                function () {

                    // only show the X if there there is something picked
                    if ($('#' + controlId + '_hfPersonId').val() || '0' !== '0') {
                        $('#' + controlId + '_btnSelectNone').stop().show();
                    }
                },
                function () {
                    $('#' + controlId + '_btnSelectNone').fadeOut(500);
                });

            $('#' + controlId + '_btnCancel').click(function () {
                $(this).closest('.picker-menu').slideUp(function () {
                    exports.personPickers[controlId].updateScrollbar();
                });
            });

            $('#' + controlId + '_btnSelectNone').click(function (e) {

                var selectedValue = '0',
                    selectedText = defaultText,
                    $selectedItemLabel = $('#' + controlId + '_selectedItemLabel'),
                    $hiddenItemId = $('#' + controlId + '_hfPersonId'),
                    $hiddenItemName = $('#' + controlId + '_hfPersonName');

                $hiddenItemId.val(selectedValue);
                $hiddenItemName.val(selectedText);
                $selectedItemLabel.val(selectedValue);
                $selectedItemLabel.text(selectedText);
            });

            $('#' + controlId + '_btnSelect').click(function () {
                var radInput = $('#' + controlId).find('input:checked'),

                    selectedValue = radInput.val(),
                    selectedText = radInput.closest('.picker-select-item').find('label').text(),

                    selectedPersonLabel = $('#' + controlId + '_selectedPersonLabel'),

                    hiddenPersonId = $('#' + controlId + '_hfPersonId'),
                    hiddenPersonName = $('#' + controlId + '_hfPersonName');

                hiddenPersonId.val(selectedValue);
                hiddenPersonName.val(selectedText);

                selectedPersonLabel.val(selectedValue);
                selectedPersonLabel.text(selectedText);

                $(this).closest('.picker-menu').slideUp();
            });
        };

        PersonPicker.prototype.updateScrollbar = function () {
            // update the outer modal scrollbar
            Rock.dialogs.updateModalScrollBar(this.controlId);
        }

        PersonPicker.prototype.initialize = function () {
            $.extend($.ui.autocomplete.prototype, {
                _renderItem: function ($ul, item) {
                    if (this.options.html) {
                        // override jQueryUI autocomplete's _renderItem so that we can do Html for the listitems
                        // derived from http://github.com/scottgonzalez/jquery-ui-extensions

                        var $div = $('<div/>').attr('class', 'radio'),

                            $label = $('<label/>')
                                .text(item.Name)
                                .prependTo($div),

                            $radio = $('<input type="radio" name="person-id" />')
                                .attr('id', item.Id)
                                .attr('value', item.Id)
                                .prependTo($label),

                            $li = $('<li/>')
                                .addClass('picker-select-item')
                                .attr('data-person-id', item.Id)
                                .html($div),

                            $resultSection = $(this.options.appendTo);

                        if (item.PickerItemDetailsHtml) {
                            $(item.PickerItemDetailsHtml).appendTo($li);
                        }
                        else {
                            var $itemDetailsDiv = $('<div/>')
                                .addClass('picker-select-item-details clearfix')
                                .attr('data-has-details', false)
                                .hide();

                            $itemDetailsDiv.appendTo($li);
                        }

                        if (!item.IsActive) {
                            $li.addClass('inactive');
                        }

                        return $resultSection.append($li);
                    }
                    else {
                        return $('<li></li>')
                            .data('item.autocomplete', item)
                            .append($('<a></a>').text(item.label))
                            .appendTo($ul);
                    }
                }
            });

            this.initializeEventHandlers();
        };

        var exports = {
            personPickers: {},
            findControl: function (controlId) {
                return exports.personPickers[controlId];
            },
            initialize: function (options) {
                if (!options.controlId) throw '`controlId` is required.';
                if (!options.restUrl) throw '`restUrl` is required.';

                var personPicker = new PersonPicker(options);
                exports.personPickers[options.controlId] = personPicker;
                personPicker.initialize();
            }
        };

        return exports;
    }());
}(jQuery));