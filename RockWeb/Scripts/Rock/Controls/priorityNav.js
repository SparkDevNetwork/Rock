(function($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.priorityNav = (function() {
        var RESIZE_DURATION = 500;
        var TAB_KEYCODE = 9;
        var ClassName = {
            PRIORITY: 'priority',
            HIDE: 'sr-only',
            RESIZING: 'resizing'
        };
        var Selector = {
            NAV_ELEMENTS: "li:not('.overflow-nav')",
            FIRST_ELEMENT: 'li:first',
            PRIORITY_ELEMENT: '.priority'
        };
        var MenuLabelDefault = 'More';
        var MenuLabelAllHiddenDefault = 'Menu';
        var MenuLabelCarat = '<i class="ml-1 ti ti-chevron-down" />'
        var MenuTemplate = function(MenuLabel) {
            return '<li class="overflow-nav dropdown d-none"><a href="#" class="dropdown-toggle nav-link overflow-nav-link" data-toggle="dropdown" role="button" aria-haspopup="true">' + MenuLabel + '</a><ul class="overflow-nav-list dropdown-menu dropdown-menu-right"></ul></li>';
        };
        var PriorityNav = function (options) {
            var element = $('.nav-tabs');

            if (options && options.controlId) {
                element = $('#' + options.controlId);
            } else {
                options = '';
            }

            this._element = element;
            this._config = options;

            if ($(element).is('ul')) {
                this._$menu = $(element);
            } else {
                this._$menu = $(element)
                    .find('ul')
                    .first();
            }
            this._initMenu();
            this._$allNavElements = this._$menu.find(Selector.NAV_ELEMENTS);
            this._bindUIActions();
            this._setupMenu();
        };

        PriorityNav.prototype = {
            constructor: PriorityNav,
            initialize: function() {},
            _initMenu: function() {
                var MenuLabel = this._config.MenuLabel

                if (typeof MenuLabel === 'undefined') {
                  MenuLabel = MenuLabelDefault
                }

                // If an overflow-nav already exists on this menu (e.g. from a prior
                // initialization triggered by an ASP.NET partial postback), remove it and
                // restore any nav items it had hidden so the menu can be rebuilt cleanly
                // instead of appending a duplicate "More" tab on every postback.
                var $existingOverflowNav = this._$menu.find('.overflow-nav');
                if ($existingOverflowNav.length) {
                    var $hiddenItems = this._$menu.find(Selector.NAV_ELEMENTS).filter('.' + ClassName.HIDE);
                    $hiddenItems.removeClass(ClassName.HIDE);
                    $hiddenItems.find('a').attr('tabindex', 0);
                    $existingOverflowNav.remove();
                }

                // add menu template
                this._$menu.append(MenuTemplate(MenuLabel));
            },

            _setupMenu: function() {
                var $allNavElements = this._$allNavElements;
                // Checking position of the menu
                var menuPosition = this._$menu.position();
                // Get position of right
                var menuRight = menuPosition.left + this._$menu.outerWidth();
                // Checking top position of first item (sometimes changes)
                var firstPos = this._$menu.find(Selector.FIRST_ELEMENT).position();

                // Empty collection in which to put menu items to move
                var $wrappedElements = $();

                // Used to snag the previous menu item in addition to ones that have wrapped
                var first = true;

                // Loop through all the nav items...
                this._$allNavElements.each(function(i) {
                    var $elm = $(this);

                    // ...in which to find wrapped elements
                    var pos = $elm.position();
                    // Get position of right
                    var right = pos.left + $elm.outerWidth();

                    if (pos.top !== firstPos.top || right > menuRight) {
                        // If element is wrapped, add it to set
                        $wrappedElements = $wrappedElements.add($elm);

                        // Add the previous one too, if first
                        if (first) {
                            $wrappedElements = $wrappedElements.add($allNavElements.eq(i - 1));
                            first = false;
                        }
                    }
                });

                if ($wrappedElements.length) {
                    // Clone set before altering
                    var newSet = $wrappedElements.clone();

                    // Hide ones that we're moving
                    $wrappedElements.addClass(ClassName.HIDE);
                    $wrappedElements.find('a').attr('tabindex', -1);

                    // Add wrapped elements to dropdown
                    this._$menu.find('.overflow-nav-list').append(newSet);

                    // Show new menu
                    this._$menu.find('.overflow-nav').removeClass('d-none');

                    // Check if menu doesn't overflow after process
                    if (this._$menu.find('.overflow-nav').position().top !== firstPos.top) {
                        var $item = $(this._element)
                            .find('.' + ClassName.HIDE)
                            .first()
                            .prev();
                        var $itemDuplicate = $item.clone();

                        $item.addClass(ClassName.HIDE);
                        $item.find('a').attr('tabindex', -1);

                        this._$menu.find('.overflow-nav-list').prepend($itemDuplicate);
                    }

                    if ($allNavElements.length == $wrappedElements.length) {
                        this._$menu.find('.overflow-nav-link').html(MenuLabelAllHiddenDefault + ' ' + MenuLabelCarat);
                        this._$menu.find('.overflow-nav-list').removeClass('dropdown-menu-right');
                    } else {
                        this._$menu.find('.overflow-nav-link').html(MenuLabelDefault + ' ' + MenuLabelCarat);
                        this._$menu.find('.overflow-nav-list').addClass('dropdown-menu-right');
                    }
                }

                // hide menu from AT
                this._$menu.find('.overflow-nav').attr('aria-hidden', true);
            },

            _tearDown: function() {
                // If the overflow menu's Bootstrap dropdown is currently open, close it before
                // rebuilding. Any listener on hide.bs.dropdown (for example profile.js on the
                // Person Profile, which toggles ".overflow-visible" on the parent .zone-nav)
                // needs to run so the layout _setupMenu measures is the collapsed one.
                // Otherwise the <ul> is allowed to stretch past its container, no wrap is
                // detected, and the "More" tab stays hidden after a window resize.
                var $overflowNav = this._$menu.find('.overflow-nav');
                if ($overflowNav.hasClass('open')) {
                    $overflowNav.find('.dropdown-toggle').dropdown('toggle');
                }

                this._$menu.find('.overflow-nav-list').empty();
                this._$menu.find('.overflow-nav').addClass('d-none');
                this._$allNavElements.removeClass(ClassName.HIDE);
                this._$allNavElements.find('a').attr('tabindex', 0);
            },

            _bindUIActions: function() {
                var self = this;

                // Namespace the resize handler so re-initializing (for example after a partial
                // postback) can detach the previous handler instead of stacking N handlers on
                // window, which would otherwise cause tearDown/setupMenu to run N times per resize.
                var resizeNamespace = 'resize.priorityNav';
                if (self._config && self._config.controlId) {
                    resizeNamespace += '-' + self._config.controlId;
                }

                $(window).off(resizeNamespace).on(resizeNamespace, function() {
                    self._$menu.addClass(ClassName.RESIZING);

                    setTimeout( function() {
                        self._tearDown();
                        self._setupMenu();
                        self._$menu.removeClass(ClassName.RESIZING);
                    }, RESIZE_DURATION);
                });

                this._$menu.find('.overflow-nav .dropdown-toggle').on('keyup', function(e) {
                    if (e.which === TAB_KEYCODE) {
                        $(e.target).dropdown('toggle');
                    }
                });
            }
        }

        var exports = {
            defaults: {
                controlId: null,
                name: 'prioritynav'
            },
            controls: {},
            initialize: function (options) {
                var settings = $.extend({}, exports.defaults, options);

                if (!settings.controlId) throw 'controlId is required';

                var priorityNav = new PriorityNav(settings);

                // Delay initialization until after the DOM is ready
                $(function () {
                    priorityNav.initialize();
                });
            }
        };

        return exports;
    })();
})(jQuery);
