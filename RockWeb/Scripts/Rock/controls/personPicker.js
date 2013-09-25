(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.personPicker = (function () {
        var PersonPicker = function (options) {
            this.controlId = options.controlId;
            this.restUrl = options.restUrl;
        };

        PersonPicker.prototype.initializeEventHandlers = function () {
            var controlId = this.controlId,
                restUrl = this.restUrl;

            // TODO: Can we use TypeHead here (already integrated into BootStrap) instead of jQueryUI?
            // Might be a good opportunity to break the dependency on jQueryUI.
            $('#personPicker_' + controlId).autocomplete({
                source: function (request, response) {
                    var promise = $.ajax({
                        url: restUrl + request.term,
                        dataType: 'json'
                    });

                    promise.done(function (data) {
                        $('#personPickerItems_' + controlId).first().html('');
                        response($.map(data, function (item) {
                            return item;
                        }));
                    });

                    // Is this needed? If an error is thrown on the server, we should see an exception in the log now...
                    promise.fail(function (xhr, status, error) {
                        console.log(status + ' [' + error + ']: ' + xhr.responseText);

                        // TODO: Display some feedback to the user that something went wrong?
                    });
                },
                minLength: 3,
                html: true,
                appendTo: '#personPickerItems_' + controlId,
                messages: {
                    noResults: function () {},
                    results: function () {}
                }
            });

            $('#' + controlId + ' a.picker-label').click(function (e) {
                e.preventDefault();
                $('#' + controlId).find('.picker-menu').first().toggle();
            });

            $('.picker-select').on('click', '.picker-select-item', function () {
                var $selectedItem = $(this).attr('data-person-id');

                // hide other open details
                $('.picker-select-item-details').each(function () {
                    var $el = $(this),
                        $currentItem = $el.closest('.picker-select-item').attr('data-person-id');

                    if ($currentItem != $selectedItem) {
                        $el.slideUp();
                    }
                });

                $(this).find('.picker-select-item-details:hidden').slideDown();
            });

            $('#btnCancel_' + controlId).click(function () {
                $(this).closest('.picker-menu').slideUp();
            });

            $('#btnSelect_' + controlId).click(function () {
                var radInput = $('#' + controlId).find('input:checked'),

                    selectedValue = radInput.val(),
                    selectedText = radInput.closest('.picker-select-item').find('label').text(),

                    selectedPersonLabel = $('#selectedPersonLabel_' + controlId),

                    hiddenPersonId = $('#hfPersonId_' + controlId),
                    hiddenPersonName = $('#hfPersonName_' + controlId);

                hiddenPersonId.val(selectedValue);
                hiddenPersonName.val(selectedText);

                selectedPersonLabel.val(selectedValue);
                selectedPersonLabel.text(selectedText);

                $(this).closest('.picker-menu').slideUp();
            });
        };

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

                        $(item.PickerItemDetailsHtml).appendTo($li);

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