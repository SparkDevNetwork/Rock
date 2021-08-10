(function ($) {
    'use strict';
    window.Rock = window.Rock || {};

    Rock.personalLinks = (function () {
        var _returnItemArray = {},
        exports = {
            setLocalStorage: function() {
                localStorage.setItem("quickReturn", JSON.stringify(_returnItemArray));
            },
            getLocalStorage: function() {
                _returnItemArray = JSON.parse(localStorage.getItem("quickReturn"));
                if (_returnItemArray === null) {
                    _returnItemArray = new Array();
                }
            },

            addQuickReturn: function (type, typeOrder, itemName) {
                this.getLocalStorage();
                var today = new Date();
                var returnItem = {
                    type: type,
                    typeOrder: typeOrder,
                    createdDateTime: today,
                    itemName: itemName,
                    url: window.location.href
                };

                const found = _returnItemArray.some(el => el.url.toLowerCase() === window.location.href.toLowerCase());
                if (found)
                {
                    _returnItemArray = _returnItemArray.filter(function (el) {
                        return !(el.url.toLowerCase() === window.location.href.toLowerCase() && el.type.toLowerCase() === type.toLowerCase());
                    });
                }

                // Check if the return items already has this exact entry, if so remove it. This allows the new item to be at the top
                this.removeDuplicates(returnItem);

                // Add new item
                _returnItemArray.push(returnItem);

                var arrLength = _returnItemArray.length;
                if (arrLength > 20) {
                    _returnItemArray.splice(0, arrLength - 20);
                }

                this.setLocalStorage();
            },
            removeDuplicates: function (newReturnItem) {

                // Find duplicate items
                for (var i = 0; i < _returnItemArray.length; i++) {
                    if (this.returnItemIsEqual(_returnItemArray[i], newReturnItem)) {
                        _returnItemArray.splice(i, 1);
                        i--; // Decrement index as we just removed an item so we need to reset it back one
                    }
                }
            },
            // Determines if the two return items are the same item
            // Equality is defined as matching: type, typeOrder and itemName. Url is not considered to prevent
            // multiple items for the person profile pages. 
            returnItemIsEqual: function (returnItemA, returnItemB) {
                var isEqual = true;

                if (returnItemA.type != returnItemB.type) {
                    isEqual = false;
                }

                if (returnItemA.typeOrder != returnItemB.typeOrder) {
                    isEqual = false;
                }

                if (returnItemA.itemName != returnItemB.itemName) {
                    isEqual = false;
                }

                return isEqual;
            },
            getQuickReturns: function () {
                this.getLocalStorage();
                var cloneItemArray = _returnItemArray.slice(0);
                cloneItemArray.sort(function (a, b) {
                    // Sort by count
                    var dtypeOrder = a.typeOrder - b.typeOrder;
                    if (dtypeOrder) return dtypeOrder;

                    // If there is a tie, sort by year
                    var dCreatedDateTime = new Date(b.createdDateTime) - new Date(a.createdDateTime);
                    return dCreatedDateTime;
                });

                var types = {};
                for (var i = 0; i < cloneItemArray.length; i++) {
                    var type = cloneItemArray[i].type;
                    if (!types[type]) {
                        types[type] = [];
                    }
                    types[type].push(cloneItemArray[i]);
                }
                var itemsByType = [];
                for (var itemType in types) {
                    itemsByType.push({ type: itemType, items: types[itemType] });
                }
                return itemsByType;
            },
            buildQuickReturn: function () {
                if ($('#divQuickReturn').length) {
                    var quickReturnsByType = Rock.personalLinks.getQuickReturns();
                    var quickReturnHtml = '';
                    if (quickReturnsByType.length > 0) {
                        for (var i = 0; i < quickReturnsByType.length; i++) {
                            if (quickReturnsByType[i].items.length > 0) {
                                var itemsHtml = '<li><strong>' + quickReturnsByType[i].type + '</strong></li>';
                                for (var item in quickReturnsByType[i].items) {
                                    itemsHtml += ' <li><a href="' + quickReturnsByType[i].items[item].url + '">' + quickReturnsByType[i].items[item].itemName + '</a></li>';
                                }
                                quickReturnHtml += '<ul class="list-unstyled">' + itemsHtml + '</ul>';
                            }
                        }
                    }

                    $('#divQuickReturn').html(quickReturnHtml);
                }
            },
            positionPersonalLinks: function (button, panel) {
                var bottom = (button.position().top + button.outerHeight(true));
                var buttonRight = (button.offset().left + button.outerWidth());
                var buttonCenter = (button.offset().left + (button.outerWidth() / 2));

                var left =  (buttonCenter - (panel.outerWidth() / 2));
                var leftMax = $(window).width() - panel.outerWidth();
                left = Math.max(0,Math.min(left,leftMax));

                panel.css('top',bottom).css('left',left);
            },
            showPersonalLinks: function (element, trigger) {
                if (typeof trigger !== "undefined" && element !== null && element.hasClass( 'd-none' )) {
                    Rock.personalLinks.positionPersonalLinks(trigger, element);

                    $(document).off("mouseup.personalLinks").on("mouseup.personalLinks", function (e) {
                        // if the target of the click isn't the container or a descendant of the container
                        if (!element.is(e.target) && element.has(e.target).length === 0) {

                            var isBookmarkButton = $(".js-rock-bookmark").is(e.target) || $(".js-rock-bookmark").has(e.target).length != 0;

                            // 'js-rock-bookmark' has it's own handler, so ignore if this is from js-rock-bookmark
                            if (!isBookmarkButton) {

                                // if one of the configuration options is open (AddLink or AddSection), don't hide the links
                                var bookMarkConfigurationMode = $(".js-bookmark-configuration").length > 0;
                                console.log('bookMarkConfigurationMode:' + bookMarkConfigurationMode);

                                if (!bookMarkConfigurationMode) {
                                    // hide the bookmark links
                                    Rock.personalLinks.showPersonalLinks(null, true);
                                }
                            }
                        }
                    });

                    $(window).on("resize.personalLinks", function(e) { Rock.personalLinks.positionPersonalLinks(trigger, element) });
                    element.removeClass('d-none')

                    Rock.personalLinks.buildQuickReturn();
                } else {
                    $('.js-personal-link-popover').addClass('d-none');
                    $(document).off(".personalLinks")
                }
            }
        }
        return exports;
    }());
}(jQuery));
