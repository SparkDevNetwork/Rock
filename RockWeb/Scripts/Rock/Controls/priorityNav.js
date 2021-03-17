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
        var MenuLabelCarat = '<i class="ml-1 fa fa-angle-down" />'
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

                // add menu template
                this._$menu.append(MenuTemplate(MenuLabel));
            },

            _setupMenu: function() {
                var $allNavElements = this._$allNavElements;

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
                this._$menu.find('.overflow-nav-list').empty();
                this._$menu.find('.overflow-nav').addClass('d-none');
                this._$allNavElements.removeClass(ClassName.HIDE);
                this._$allNavElements.find('a').attr('tabindex', 0);
            },

            _bindUIActions: function() {
                var self = this;
                $(window).on('resize', function() {
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
