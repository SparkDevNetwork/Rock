(function ($) {
    'use strict';
    window.Rock = window.Rock || {};

    Rock.personalLinks = (function () {
        var _quickReturnItemArray = [],
            $bookmarkButton,
            $bookmarkPanel,
            _quickLinksLocalStorageKey = 'quickLinks_',
            exports = {
                initialize: function (clientId) {
                    $bookmarkButton = $('.js-rock-bookmark');
                    $bookmarkPanel = $('.js-bookmark-panel');
                    $bookmarkButton.off("click").on("click", function (e) {
                        e.preventDefault();

                        // if one of the configuration options is open (AddLink or AddSection), don't hide the links
                        var bookMarkConfigurationMode = $(".js-bookmark-configuration").length > 0;

                        if (!bookMarkConfigurationMode) {

                            // Show/hide the personalLinks
                            Rock.personalLinks.showPersonalLinks($bookmarkButton, $bookmarkPanel);
                        }
                    });

                    this.registerEvents();

                    var personalLinksData = this.getPersonalLinksData();

                    var $personalLinksLinks = $('.js-personal-links-container .js-personal-links-links');
                    if ($personalLinksLinks.length) {
                        // the local storage will be specific to a person, so get the key from the data attribute
                        _quickLinksLocalStorageKey = $personalLinksLinks.attr('data-quick-links-local-storage-key');

                        var modificationHash = $personalLinksLinks.attr('data-personal-links-modification-hash');
                        var needsUpdate = personalLinksData == null || personalLinksData.ModificationHash == null || personalLinksData.ModificationHash != modificationHash;

                        if (needsUpdate) {
                            var getPersonalLinksDataUrl = Rock.settings.get('baseUrl') + 'api/PersonalLinksController/GetPersonalLinksData';

                            $.ajax({
                                type: "GET",
                                url: getPersonalLinksDataUrl,
                            }).done(function (personalLinksData) {
                                Rock.personalLinks.updatePersonalLinksLocalStorageData(personalLinksData);
                                Rock.personalLinks.buildAllLinks();
                            });
                        }
                    }

                    if (personalLinksData) {
                        // Build the links, even if we are we are in 'needsUpdate' mode, just in case the REST call takes a second.
                        // This way the links won't be blank while waiting, and when the REST call does return, the buildAllLinks will
                        // run again with the updated data.
                        this.buildAllLinks();
                    }
                }, saveQuickReturnsToLocalStorage: function () {
                    localStorage.setItem(_quickLinksLocalStorageKey, JSON.stringify(_quickReturnItemArray));
                },
                unregisterEvents: function () {
                    $(document).off(".personalLinks");
                },
                registerEvents: function () {
                    $(document).off("mouseup.personalLinks").on("mouseup.personalLinks", function (e) {

                        var isBookmarkButton = $(".js-rock-bookmark").is(e.target) || $(".js-rock-bookmark").has(e.target).length != 0;
                        // 'js-rock-bookmark' has it's own handler, so ignore if this is from js-rock-bookmark
                        if (isBookmarkButton) {
                            return;
                        }

                        // if the target of the click isn't the bookmarkPanel or a descendant of the bookmarkPanel
                        if (!$bookmarkPanel.is(e.target) && $bookmarkPanel.has(e.target).length === 0) {

                            // if one of the configuration options is open (AddLink or AddSection), don't hide the links
                            var bookMarkConfigurationMode = $(".js-bookmark-configuration").length > 0;

                            if (!bookMarkConfigurationMode) {
                                // hide the bookmark links
                                Rock.personalLinks.hidePersonalLinks();
                            }
                        }
                    });

                    $(window).on("resize.personalLinks", function (e) { Rock.personalLinks.positionPersonalLinks($bookmarkButton, $bookmarkPanel) });
                },
                savePersonalLinksToLocalStorage: function (personalLinksData) {
                    localStorage.setItem("personalLinksData", JSON.stringify(personalLinksData));
                },
                getLocalStorage: function () {
                    _quickReturnItemArray = JSON.parse(localStorage.getItem(_quickLinksLocalStorageKey));

                    if (_quickReturnItemArray === null) {
                        _quickReturnItemArray = new Array();
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

                    const found = _quickReturnItemArray.some(el => el.url.toLowerCase() === window.location.href.toLowerCase());
                    if (found) {
                        _quickReturnItemArray = _quickReturnItemArray.filter(function (el) {
                            return !(el.url.toLowerCase() === window.location.href.toLowerCase() && el.type.toLowerCase() === type.toLowerCase());
                        });
                    }

                    // Check if the return items already has this exact entry, if so remove it. This allows the new item to be at the top
                    this.removeDuplicateQuickReturnItems(returnItem);

                    // Add new item
                    _quickReturnItemArray.push(returnItem);

                    var arrLength = _quickReturnItemArray.length;
                    if (arrLength > 20) {
                        _quickReturnItemArray.splice(0, arrLength - 20);
                    }

                    this.saveQuickReturnsToLocalStorage();
                    this.buildQuickReturns();
                },
                removeDuplicateQuickReturnItems: function (newReturnItem) {

                    // Find duplicate items
                    for (var i = 0; i < _quickReturnItemArray.length; i++) {
                        if (this.quickReturnItemIsEqual(_quickReturnItemArray[ i ], newReturnItem)) {
                            _quickReturnItemArray.splice(i, 1);
                            i--; // Decrement index as we just removed an item so we need to reset it back one
                        }
                    }
                },
                // Determines if the two return items are the same item
                // Equality is defined as matching: type, typeOrder and itemName. Url is not considered to prevent
                // multiple items for the person profile pages. 
                quickReturnItemIsEqual: function (returnItemA, returnItemB) {
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
                    var cloneItemArray = _quickReturnItemArray.slice(0);
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
                        var type = cloneItemArray[ i ].type;
                        if (!types[ type ]) {
                            types[ type ] = [];
                        }
                        types[ type ].push(cloneItemArray[ i ]);
                    }
                    var itemsByType = [];
                    for (var itemType in types) {
                        itemsByType.push({ type: itemType, items: types[ itemType ] });
                    }
                    return itemsByType;
                },
                buildQuickReturns: function () {
                    var $quickReturns = $('.js-personal-links-container .js-personal-links-quick-return');
                    if ($quickReturns.length) {
                        var quickReturnsByType = Rock.personalLinks.getQuickReturns();
                        var quickReturnHtml = '';
                        if (quickReturnsByType.length > 0) {
                            for (var i = 0; i < quickReturnsByType.length; i++) {
                                if (quickReturnsByType[ i ].items.length > 0) {
                                    var itemsHtml = '<li><strong>' + quickReturnsByType[ i ].type + '</strong></li>';
                                    for (var item in quickReturnsByType[ i ].items) {
                                        itemsHtml += ' <li><a href="' + quickReturnsByType[ i ].items[ item ].url + '">' + quickReturnsByType[ i ].items[ item ].itemName + '</a></li>';
                                    }
                                    quickReturnHtml += '<ul class="list-unstyled">' + itemsHtml + '</ul>';
                                }
                            }
                        }

                        $quickReturns.html(quickReturnHtml);
                    }
                },
                getPersonalLinksData: function () {
                    return JSON.parse(localStorage.getItem("personalLinksData"));
                },
                buildAllLinks: function () {
                    this.buildPersonalLinks();
                    this.buildQuickReturns();
                },
                buildPersonalLinks: function () {
                    var $personalLinksLinks = $('.js-personal-links-container .js-personal-links-links');
                    if ($personalLinksLinks.length) {

                        var personalLinksData = Rock.personalLinks.getPersonalLinksData();

                        if (personalLinksData == null) {
                            return;
                        }

                        var personalSections = personalLinksData.PersonLinksSectionList;
                        var personalLinksHtml = '';
                        if (personalSections.length > 0) {
                            for (var i = 0; i < personalSections.length; i++) {
                                if (personalSections[ i ].PersonalLinks.length > 0) {
                                    var itemsHtml = '';
                                    itemsHtml = '<li><strong>' + personalSections[ i ].Name + '</strong></li>';

                                    for (var personalLinkIndex in personalSections[ i ].PersonalLinks) {
                                        var personalLink = personalSections[ i ].PersonalLinks[ personalLinkIndex ];
                                        itemsHtml += ' <li><a href="' + personalLink.Url + '">' + personalLink.Name + '</a></li>';
                                    }
                                    personalLinksHtml += '<ul class="list-unstyled">' + itemsHtml + '</ul>';
                                }
                            }
                        }

                        $personalLinksLinks.html(personalLinksHtml);
                    }
                },
                positionPersonalLinks: function ($button, $panel) {
                    var bottom = ($button.position().top + $button.outerHeight(true));
                    var buttonRight = ($button.offset().left + $button.outerWidth());
                    var buttonCenter = ($button.offset().left + ($button.outerWidth() / 2));

                    var left = (buttonCenter - ($panel.outerWidth() / 2));
                    var leftMax = $(window).width() - $panel.outerWidth();
                    left = Math.max(0, Math.min(left, leftMax));

                    $panel.css('top', bottom).css('left', left);
                },
                updatePersonalLinksLocalStorageData: function (personalLinksLocalStorageData) {
                    this.savePersonalLinksToLocalStorage(personalLinksLocalStorageData);
                },
                hidePersonalLinks: function () {
                    $('.js-personal-link-popover').addClass('d-none');
                    this.unregisterEvents();

                },
                showPersonalLinks: function ($button, $panel) {

                    this.registerEvents();

                    if (typeof $button !== "undefined" && $panel !== null && $panel.hasClass('d-none')) {
                        Rock.personalLinks.positionPersonalLinks($button, $panel);

                        $panel.removeClass('d-none');
                    } else {
                        this.hidePersonalLinks();
                    }
                }
            }
        return exports;
    }());
}(jQuery));
