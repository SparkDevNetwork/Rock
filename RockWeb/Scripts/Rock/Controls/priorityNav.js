(function($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.priorityNav = (function() {
        const RESIZE_DURATION = 500;
        const TAB_KEYCODE = 9;

        const ClassName = {
            PRIORITY: 'priority',
            HIDE: 'sr-only',
            RESIZING: 'resizing'
        };

        const Selector = {
            NAV_ELEMENTS: "li:not('.overflow-nav')",
            FIRST_ELEMENT: 'li:first',
            PRIORITY_ELEMENT: '.priority'
        };

        const MenuLabelDefault = 'More';
        const MenuLabelAllHiddenDefault = 'Menu';

        var MenuTemplate = function(MenuLabel) {
            return '<li class="overflow-nav dropdown"><a href="#" class="dropdown-toggle nav-link overflow-nav-link" data-toggle="dropdown" role="button" aria-haspopup="true">' + MenuLabel + '</a><ul class="overflow-nav-list dropdown-menu dropdown-menu-right"></ul></li>';
        };

        var exports = {
            initialize: function(options) {
                let element = $('.nav-tabs');

                if (options && options.id) {
                    element = $('#' + options.id);
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
            },

            _initMenu: function() {
                let MenuLabel = this._config.MenuLabel

                if (typeof MenuLabel === 'undefined') {
                  MenuLabel = MenuLabelDefault
                }

                // add menu template
                this._$menu.append(MenuTemplate(MenuLabel));
            },

            _setupMenu: function() {
                const $allNavElements = this._$allNavElements;

                // Checking top position of first item (sometimes changes)
                const firstPos = this._$menu.find(Selector.FIRST_ELEMENT).position();

                // Empty collection in which to put menu items to move
                let $wrappedElements = $();

                // Used to snag the previous menu item in addition to ones that have wrapped
                let first = true;

                // Loop through all the nav items...
                this._$allNavElements.each(function(i) {
                    const $elm = $(this);

                    // ...in which to find wrapped elements
                    const pos = $elm.position();

                    if (pos.top !== firstPos.top) {
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
                    const newSet = $wrappedElements.clone();

                    // Hide ones that we're moving
                    $wrappedElements.addClass(ClassName.HIDE);
                    $wrappedElements.find('a').attr('tabindex', -1);

                    // Add wrapped elements to dropdown
                    this._$menu.find('.overflow-nav-list').append(newSet);

                    // Show new menu
                    this._$menu.find('.overflow-nav').removeClass('hidden');

                    // Check if menu doesn't overflow after process
                    if (this._$menu.find('.overflow-nav').position().top !== firstPos.top) {
                        const $item = $(this._element)
                            .find('.' + ClassName.HIDE)
                            .first()
                            .prev();
                        const $itemDuplicate = $item.clone();

                        $item.addClass(ClassName.HIDE);
                        $item.find('a').attr('tabindex', -1);

                        this._$menu.find('.overflow-nav-list').prepend($itemDuplicate);
                    }

                    if ($allNavElements.length == $wrappedElements.length) {
                        this._$menu.find('.overflow-nav-link').text(MenuLabelAllHiddenDefault);
                        this._$menu.find('.overflow-nav-list').removeClass('dropdown-menu-right');
                    } else {
                        this._$menu.find('.overflow-nav-link').text(MenuLabelDefault);
                        this._$menu.find('.overflow-nav-list').addClass('dropdown-menu-right');
                    }
                }

                // hide menu from AT
                this._$menu.find('.overflow-nav').attr('aria-hidden', true);
            },

            _tearDown: function() {
                this._$menu.find('.overflow-nav-list').empty();
                this._$menu.find('.overflow-nav').addClass('hidden');
                this._$allNavElements.removeClass(ClassName.HIDE);
                this._$allNavElements.find('a').attr('tabindex', 0);
            },

            _bindUIActions: function() {
                $(window).on('resize', function() {
                    this._$menu.addClass(ClassName.RESIZING);

                    setTimeout( function() {
                        this._tearDown();
                        this._setupMenu();
                        this._$menu.removeClass(ClassName.RESIZING);
                    }, RESIZE_DURATION);
                });

                this._$menu.find('.overflow-nav .dropdown-toggle').on('keyup', function(e) {
                    if (e.which === TAB_KEYCODE) {
                        $(e.target).dropdown('toggle');
                    }
                });
            }
        };

        return exports;
    })();
})(jQuery);
