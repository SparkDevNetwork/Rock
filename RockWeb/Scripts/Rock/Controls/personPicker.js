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

            var promise = null;
            var lastSelectedPersonId = null;

            $('#' + controlId + '_personPicker').autocomplete({
                source: function (request, response) {

                    // abort any searches that haven't returned yet, so that we don't get a pile of results in random order
                    if (promise && promise.state() === 'pending') {
                        promise.abort();
                    }

                    promise = $.ajax({
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

            var debouncerTimeout;

            // flag indicating the details are currently animating the expand
            var expanding = false;

            // debouncing function from John Hann
            // http://unscriptable.com/index.php/2009/03/20/debouncing-javascript-methods/
            var debouncePersonPicker = function (func, threshold, execAsap)
            {
              return function debounced()
              {
                var obj = this, args = arguments;
                var e = args[0];
                if (e.type == 'click') {
                  execAsap = true;
                } else {
                  execAsap = false;
                }

                function delayed()
                {
                  if (!execAsap)
                    func.apply(obj, args);
                  debouncerTimeout = null;
                };

                if (debouncerTimeout) {
                  clearTimeout(debouncerTimeout);
                }

                if (execAsap) {
                  func.apply(obj, args);
                }

                debouncerTimeout = setTimeout(delayed, threshold || 500);
              };
            }

            // if the mouse leaves the picker item, clear the debouncer so that isn't show/hide any stuff in the queue
            $('#' + controlId + ' .picker-select').on('mouseleave', '.picker-select-item', function (e)
            {
              if (debouncerTimeout) {
                clearTimeout(debouncerTimeout);
              }
            });

            $('#' + controlId + ' .picker-select').on('click mouseenter', '.picker-select-item', debouncePersonPicker(function (e)
            {
                if (e.type == 'click' && $(e.target).is(':input') == false) {
                  // only process the click event if it has bubbled up to the input tag
                  return;
                }

                e.stopPropagation();

                var $selectedItem = $(this).closest('.picker-select-item');
                var $itemDetails = $selectedItem.find('.picker-select-item-details');

                var selectedPersonId = $selectedItem.attr('data-person-id');

                if ($itemDetails.is(':visible')) {
                    
                  if (selectedPersonId == lastSelectedPersonId && e.type == 'click' && expanding == false ) {
                        // if they are clicking the same person twice in a row (and the details are done expanding), assume that's the one they want to pick
                        $('#' + controlId + '_btnSelect').get(0).click();
                    } else {
                       
                       // if it is already visible but isn't the same one twice, just leave it open
                    }
                }

                // hide other open details
                $('#' + controlId + ' .picker-select-item-details').filter(':visible').each(function ()
                {
                  var $el = $(this),
                     currentPersonId = $el.closest('.picker-select-item').attr('data-person-id');

                  if (currentPersonId != selectedPersonId) {

                    // The selected details slides up in a weird way if we try to hide details that are open above it, set this to false to see
                    var onlyHideBelowItems = true;

                    if (!onlyHideBelowItems || ($el.offset().top > $selectedItem.offset().top)) {
                      $el.hide();
                      exports.personPickers[controlId].updateScrollbar();
                    }
                  }
                });

                lastSelectedPersonId = selectedPersonId;

                if ($itemDetails.attr('data-has-details') == 'false') {
                    // add a spinner in case we have to wait on the server for a little bit
                    var $spinner = $selectedItem.find('.loading-notification');
                    $spinner.fadeIn(800);

                    // fetch the search details from the server
                    $.get(restDetailUrl + '?Id=' + selectedPersonId, function (responseText, textStatus, jqXHR) {
                        $itemDetails.attr('data-has-details', true);

                        // hide then set the html so that we can get the slideDown effect
                        $itemDetails.stop().hide().html(responseText);
                        showItemDetails($itemDetails);

                        $spinner.stop().fadeOut(200);
                    });
                } else {
                  showItemDetails($selectedItem.find('.picker-select-item-details:hidden'));
                }
            }));

            var showItemDetails = function ($itemDetails)
            {
              if ($itemDetails.length) {
                expanding = true;
                $itemDetails.slideDown(function ()
                {
                  exports.personPickers[controlId].updateScrollbar();
                  expanding = false;
                });
              }
            }

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

            var setSelectedPerson = function (selectedValue, selectedText) {
                var selectedPersonLabel = $('#' + controlId + '_selectedPersonLabel'),
                    hiddenPersonId = $('#' + controlId + '_hfPersonId'),
                    hiddenPersonName = $('#' + controlId + '_hfPersonName');

                hiddenPersonId.val(selectedValue);
                hiddenPersonName.val(selectedText);

                selectedPersonLabel.val(selectedValue);
                selectedPersonLabel.text(selectedText);

                $('#' + controlId).find('.picker-menu').slideUp();
            }

            $('#' + controlId + '_btnSelect').click(function () {
                var radInput = $('#' + controlId).find('input:checked'),
                    selectedValue = radInput.val(),
                    selectedText = radInput.closest('.picker-select-item').find('label').text();

                setSelectedPerson(selectedValue, selectedText);
            });

            $('#' + controlId + ' .js-select-self').on('click', function () {
                var selectedValue = $('#' + controlId + ' .js-self-person-id').val(),
                    selectedText = $('#' + controlId + ' .js-self-person-name').val();

                setSelectedPerson(selectedValue, selectedText);

                // fire the postBack of the btnSelect if there is one
                var postBackUrl = $('#' + controlId + '_btnSelect').prop('href');
                if (postBackUrl) {
                    window.location = postBackUrl;
                }
            });
        };

        PersonPicker.prototype.updateScrollbar = function () {
            // first, update this control's scrollbar, then the modal's
            var $container = $('#' + this.controlId).find('.scroll-container')

            if ($container.is(':visible')) {
                $container.tinyscrollbar_update('relative');
            }

            // update the outer modal scrollbar
            Rock.dialogs.updateModalScrollBar(this.controlId);
        }

        PersonPicker.prototype.initialize = function () {
            $.extend($.ui.autocomplete.prototype, {
                _renderItem: function ($ul, item) {
                    if (this.options.html) {
                        // override jQueryUI autocomplete's _renderItem so that we can do Html for the listitems
                        // derived from http://github.com/scottgonzalez/jquery-ui-extensions

                        var inactiveWarning = "";

                        if (!item.IsActive && item.RecordStatus) {
                            inactiveWarning = " <small>(" + item.RecordStatus + ")</small>";
                        }

                        var $div = $('<div/>').attr('class', 'radio'),

                            $label = $('<label/>')
                                .html(item.Name + inactiveWarning + ' <i class="fa fa-refresh fa-spin margin-l-md loading-notification" style="display: none; opacity: .4;"></i>')
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
                            $li.addClass('is-inactive');
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

            var $control = $('#' + this.controlId);
            $control.find('.scroll-container').tinyscrollbar({ size: 120, sizethumb: 20 });

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