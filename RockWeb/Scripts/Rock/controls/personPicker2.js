(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.personPicker2 = (function () {
        var PersonPicker2 = function (options) {
            this.controlId = options.controlId;
            this.restUrl = options.restUrl;
        };

        PersonPicker2.prototype.initializeEventHandlers = function () {
            var controlId = this.controlId,
                restUrl = this.restUrl;


            // TODO: Can we use TypeHead here (already integrated into BootStrap) instead of jQueryUI?
            // Might be a good opportunity to break the dependency on jQueryUI.
            $('#personPicker_' + controlId).autocomplete({
                source: function (request, response) {
                    var promise = $.ajax({
                        url: restUrl + request.term + "/false",
                        dataType: 'json'
                    });

                    promise.done(function (data) {
                        $('#personPickerItems_' + controlId).first().html('');
                        response($.map(data, function (item) {
                            return item;
                        }));
                        $('#person-search-results.scroll-container').tinyscrollbar_update();
                    });

                    // Is this needed? If an error is thrown on the server, we should see an exception in the log now...
                    promise.fail(function (xhr, status, error) {
                        console.log(status + ' [' + error + ']: ' + xhr.responseText);

                        // TODO: Display some feedback to the user that something went wrong?
                    });
                },
                minLength: 3,
                appendTo: '#personPickerItems_' + controlId,
                messages: {
                    noResults: function () {},
                    results: function () {}
                }
            });

            $('#personPickerItems_' + controlId).on('click', '.picker-select-item', function () {
                $('#hfPersonId_' + controlId).val($(this).attr('data-person-id'));
            });

            $('#person-search-results.scroll-container').tinyscrollbar();
        };

        PersonPicker2.prototype.initialize = function () {
            $.extend($.ui.autocomplete.prototype, {
                _renderItem: function ($ul, item) {

                    // override jQueryUI autocomplete's _renderItem so that we can do Html for the listitems
                    // derived from http://github.com/scottgonzalez/jquery-ui-extensions

                    var $label = $('<label/>').text(item.Name),

                        $radio = $('<input type="radio" name="person-id" />')
                            .attr('id', item.Id)
                            .attr('value', item.Id)
                            .prependTo($label),

                        $li = $('<li/>')
                            .addClass('picker-select-item')
                            .attr('data-person-id', item.Id)
                            .html($label),

                        $resultSection = $(this.options.appendTo);

                    $label.append(' (' + item.Gender);

                    if (item.Age > 0) {
                        $label.append(' Age: ' + item.Age);
                    }

                    $label.append(')');

                    if (!item.IsActive) {
                        $li.addClass('disabled');
                    }

                    return $resultSection.append($li);
                }
            });
            
            this.initializeEventHandlers();
        };

        var exports = {
            personPickers2: {},
            findControl: function (controlId) {
                return exports.personPickers2[controlId];
            },
            initialize: function (options) {
                if (!options.controlId) throw '`controlId` is required.';
                if (!options.restUrl) throw '`restUrl` is required.';
                
                var personPicker2 = new PersonPicker2(options);
                exports.personPickers2[options.controlId] = personPicker2;
                personPicker2.initialize();
            }
        };

        return exports;
    }());
}(jQuery));