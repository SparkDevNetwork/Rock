(function ($) {
    /**
     * @typedef EventCalendarEvent
     *
     * @property {string} name - The name of the event.
     * @property {string} start - The starting date and time of the event.
     * @property {string} end - The ending date and time of the event, leave unset or null to indicate an all-day event (optional).
     * @property {string} class - Any additional CSS class names to be applied to the event (optional).
     * @property {string|number} relatedId - The identifier to use when indicating that multiple events are related (optional).
     * @property {string} tooltip - An optional string to use as a tooltip for the event (optional).
     * @property {string} popover - An HTML string to use as the content for a popover on click (optional).
     */

    /**
     * @callback EventTemplateCallback
     * 
     * A callback that provides the event DOM object for a given event. Returned DOM element
     * should include the "event" class on the top level object.
     * 
     * @param {EventCalendar} calendar - The calendar object requesting the event model.
     * @param {EventCalendarEvent} event - The event object to be use when generating the model.
     * @param {Object} options - The additional options to consider when constructing the model.
     *
     * @returns {Object} jQuery object or HTML string.
     */

    /**
     * @callback ToolbarTemplateCallback
     * 
     * A callback that defines the entire top toolbar. Returned DOM element should include at
     * least an element with the "toolbar-title" class to support automatic title updates.
     *
     * @param {EventCalendar} calendar - The calendar object requesting the event model.
     *
     * @returns {Object} A jQuery object or HTML string.
     */

    /**
     * @callback EventsCallback
     * 
     * A callback that returns the events that should be included on the calendar.
     * 
     * @param {moment} start - The date to start retrieving events, considered >= comparison type.
     * @param {moment} end - The date to stop retrieving events, considered < comparison type.
     * @param {function} callback - The function to call when the events have been retrieved, pass an array of EventCalendarEvent objects.
     */

    /**
     * @callback EventNameTemplateCallback
     * 
     * A callback that returns the event name for a given event. Allows for simple customization
     * of what is displayed as the title. The returned object should contain a span with class of
     * "name".
     * 
     * @param {EventCalendar} calendar - The calendar object requesting the event name.
     * @param {EventCalendarEvent} event - The event object whose name is being requested.
     * @param {object} options - Additional options to be taken into consideration while constructing the name.
     * 
     * @returns {Object} A jQuery object or HTML string.
     */

    /**
     * @typedef {Object} EventCalendarSettings
     *
     * @property {string[]} dayLabels - The labels to use when rendering the day names, beginning with Sunday.
     * @property {boolean} highlightRelatedEvents - If true then the "related' class will by applied to related events when hovering over one.
     * @property {EventCalendarEvent[]|EventsCallback} events - An array or callback function used to retrieve events.
     * @property {EventTemplateCallback} eventTemplate - A callback function used to generate an event DOM.
     * @property {ToolbarTemplateCallback} toolbarTemplate - A callback function used to generate the toolbar DOM.
     * @property {EventNameTemplateCallback} eventNameTemplate - A callback function used to generate the rendered name of the events in the calendar.
     * @property {MonthLayout} monthLayout - The visual style to be used for the month layout.
     * @property {WeekLayout} weekLayout - The visual style to be used for the week layout.
     * @property {DayLayout} dayLayout - The visual style to be used for the day layout.
     * @property {number[]} timeViewRange - An array of two numbers that specify the start and end minutes of the time view layouts.
     * @property {Layout} layout - The default layout to be used when the calendar is first displayed.
     */

    /**
     * Pack various items into as few bags as possible. Each item
     * consists of a start value, an end value, and an item to
     * be packed. Basically this is a packing ruitine for a calendar
     * day view.
     * 
     * @param {Object[]} items - The array of items to be packed into bags.
     * @param {EventCalendar.PackBagCallback} pack - The user-callback function to call to perform final packing.
     */
    function PackItemsIntoBags(items, pack) {
        /**
         * This callback is used to perform final packing of items into bags.
         *
         * @callback EventCalendar.PackBagCallback
         *
         * @param {any} item - The item to be packed.
         * @param {number} bagIndex - The bag number the item should be packed into.
         * @param {number} bagCount - The total number of bags in this area.
         * @param {number} itemIndex - The item position inside this bag.
         * @param {number} itemCount - The number of items inside this bag.
         *
         * @returns {void}
         */

        /**
         * Pack the items into their final positions.
         * 
         * @param {Array} bags - The bags that need to be packed.
         * @param {EventCalendar.PackBagCallback} pack - The user-callback function to call to perform final packing.
         */
        function PackBags(bags, pack) {
            //
            // Loop through all bags.
            //
            for (var b = 0; b < bags.length; b++) {
                var bag = bags[b];

                //
                // Each bag contains an array of items to be placed in that bag.
                //
                for (var i = 0; i < bag.length; i++) {
                    pack(bag[i].item, b, bags.length, i, bag.length);
                }
            }
        }

        /**
         * Test if the two items collide.
         * 
         * @param {Object} a - The left-hand item to check for collision.
         * @param {Object} b - The right-hand item to check for collision.
         * 
         * @returns {boolean} true if the two items collide, otherwise false.
         */
        function collidesWith(a, b) {
            return a.end > b.start && a.start < b.end;
        }

        //
        // Sort items by start, and then by end.
        //
        items = items.sort(function (e1, e2) {
            if (e1.start < e2.start) {
                return -1;
            }
            else if (e1.start > e2.start) {
                return 1;
            }
            else if (e1.end < e2.end) {
                return -1;
            }
            else if (e1.end > e2.end) {
                return 1;
            }
            else {
                return 0;
            }
        });

        var bags = [];
        var furthestEnding = null;

        //
        // Iterate over the sorted array and start building the stacked
        // layout.
        //
        items.forEach(function (item) {
            console.log(item, furthestEnding);
            //
            // If this event begins after the longest event ends thus far, we can start
            // over with a new set of bags.
            //
            if (furthestEnding !== null && item.start > furthestEnding) {
                PackBags(bags, pack);
                bags = [];
                furthestEnding = null;
            }

            //
            // Determine if we can place this item in an open hole without
            // adding a new bag.
            //
            var placed = false;
            for (var i = 0; i < bags.length; i++) {
                var bag = bags[i];

                //
                // If it doesn't collide with the last item in the bag, then we can place it
                // in the bag.
                //
                if (!collidesWith(bag[bag.length - 1], item)) {
                    bag.push(item);
                    placed = true;
                    break;
                }
            }

            //
            // Nope, need a new bag.
            //
            if (!placed) {
                bags.push([item]);
            }

            //
            // If this item extends our furthest end, then extend it.
            //
            if (furthestEnding === null || item.end > furthestEnding) {
                furthestEnding = item.end;
            }
        });

        //
        // If we have any outstanding bags, do final packing.
        //
        if (bags.length > 0) {
            PackBags(bags, pack);
        }
    }

    /**
     * Get the absolute positioning in % (0-1.0) for the given
     * timestamp, relative to the dayStart and dayEnd values in minutes.
     * 
     * @param {string|moment} time - The datetime string that will be used to determine the time.
     * @param {number} dayStart - The number of minutes into the day that represent the top of the view (example: 420 = 7am).
     * @param {number} dayEnd - The number of inites into the day taht represents the bottom of the view (example: 1080 = 6pm).
     * 
     * @returns {number} Percentage inside the view for this time mark.
     */
    function GetPositionForTime(time, dayStart, dayEnd) {
        var m = moment(time);
        var t = m.hour() * 60 + m.minute();

        t = Math.round(t / 15) * 15;

        dayStart = dayStart || 0;
        dayEnd = dayEnd || 1440;
        dayTotal = dayEnd - dayStart;

        //
        // Clamp to start or end, otherwise find mid-position.
        //
        if (t <= dayStart) {
            return 0;
        }
        else if (t >= dayEnd) {
            return 1;
        }
        else {
            return (t - dayStart) / dayTotal;
        }
    }

    /**
     * Define the default event template constructor.
     * 
     * @param {EventCalendar} calendar - The calendar object requesting the event model.
     * @param {EventCalendarEvent} event - The event object to be use when generating the model.
     * @param {Object} options - The additional options to consider when constructing the model.
     * 
     * @returns {Object} jQuery object or HTML string.
     */
    function DefaultEventTemplate(calendar, event, options) {
        var $event = $('<div class="event"></div>')
            .data('event', event);

        //
        // Add custom classes specified on the event.
        //
        if (event.class) {
            $event.addClass(event.class);
        }

        var $name = $(calendar.settings.eventNameTemplate(calendar, event, options));

        //
        // If there is a URL associated with the even then create a
        // hyperlink, otherwise just the text-name.
        //
        if (event.url) {
            $event.append($('<a href="' + event.url + '"></a>').append($name));
        }
        else if (event.popover) {
            var $popover = $('<a href="#" onclick="return false"></a>')
                .append($name);

            $popover.popover({
                content: event.popover,
                container: 'body',
                html: true,
                placement: 'top auto',
                trigger: 'click'
            });

            $event.append($popover);
        }
        else {
            $event.append($name);
        }

        //
        // Setup a tooltip handler if there is a tooltip property.
        //
        if (event.tooltip) {
            $event.tooltip({
                container: 'body',
                title: event.tooltip,
                delay: {
                    show: 250,
                    hide: 100
                },
                trigger: 'hover'
            });
        }

        return $event;
    }

    /**
     * The default event name generator. Simply returns the name, and if requested, the time.
     * 
     * @param {EventCalendar} _calendar - The calendar object requesting the event name.
     * @param {EventCalendarEvent} event - The event object whose name is being requested.
     * @param {object} options - Additional options to be taken into consideration while constructing the name.
     *
     * @returns {string} A string that identifies the name of the event.
     */
    function DefaultEventNameTemplate(_calendar, event, options) {
        var eventName = event.name;

        if (options.includeTime === true) {
            eventName = moment(event.start).format('LT') + ' - ' + eventName;
        }

        return $('<span class="name"></span>').text(eventName);
    }

    /**
     * Define the default toolbar template constructor.
     * 
     * @param {EventCalendar} calendar - The calendar object requesting the event model.
     * 
     * @returns {Object} A jQuery objct or HTML string.
     */
    function DefaultToolbarTemplate(calendar) {
        var $toolbar = $('<div class="calendar-toolbar"></div>')
            .append($('<div class="row"></div>')
                .append($('<div class="col-sm-6 toolbar-title"></div>'))
                .append($('<div class="col-sm-6 toolbar-actions"></div>')));

        var $actions = $toolbar.find('.toolbar-actions');

        //
        // If the user specified custom actions to be included, then
        // add those in now.
        //
        if (calendar.settings.customActions) {
            var customActions = [];

            if (typeof calendar.settings.customActions === 'string') {
                //
                // User specified raw HTML.
                //
                customActions.push($(calendar.settings.customActions));
            }
            else if (typeof calendar.settings.customActions === 'function') {
                //
                // User specified a function to generate the actions.
                //
                var customActionsResult = calendar.settings.customActions();

                if (typeof customActionsResult === 'object' && Array.isArray(customActionsResult)) {
                    customActionsResult.forEach(function (a) {
                        customActions.push($(a));
                    });
                }
                else {
                    customActions.push($(customActionsResult));
                }
            }

            //
            // For each action supplied, append it to our action list.
            //
            customActions.forEach(function (a) {
                $actions.append(a);
            });
        }

        //
        // Append the layout select button.
        //
        var $layoutBtn = $('<div class="btn-group"><button type="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" class="btn btn-default btn-sm cal-btn-layout dropdown-toggle"><span class="title"></span> <span class="caret"></span></button><ul class="dropdown-menu dropdown-menu-right"></ul>');
        if (calendar.settings.monthLayout !== Constants.MonthLayout.None) {
            $layoutBtn.find('.dropdown-menu').append('<li><a href="#" class="cal-btn-month">Month</a></li>');
        }
        if (calendar.settings.weekLayout !== Constants.WeekLayout.None) {
            $layoutBtn.find('.dropdown-menu').append('<li><a href="#" class="cal-btn-week">Week</a></li>');
        }
        if (calendar.settings.dayLayout !== Constants.DayLayout.None) {
            $layoutBtn.find('.dropdown-menu').append('<li><a href="#" class="cal-btn-day">Day</a></li>');
        }
        if ($layoutBtn.find('.dropdown-menu').children().length > 0) {
            $actions.append($layoutBtn);
        }

        //
        // Append the navigation buttons.
        //
        var $navbtn = $('<div class="btn-group"></div>');
        $navbtn.append('<a href="#" class="btn btn-primary btn-sm cal-btn-previous"><i class="fa fa-chevron-left"></i></a>');
        $navbtn.append('<a href="#" class="btn btn-primary btn-sm cal-btn-today">Today</a>');
        $navbtn.append('<a href="#" class="btn btn-primary btn-sm cal-btn-next"><i class="fa fa-chevron-right"></i></a>');
        $actions.append($navbtn);

        return $toolbar;
    }

    /**
     * Various constants used by the event calendar logic.
     */
    const Constants = {
        /**
         * The various layouts available with this calendar.
         * 
         * @readonly
         * @enum {number}
         */
        Layout: {
            /** The calendar should be viewed one month at a time. */
            Month: 0,

            /** The calendar should be viewed one week at a time. */
            Week: 1,

            /** The calendar should be viewed one day at a time. */
            Day: 2
        },

        /**
         * The display options for month layout.
         * 
         * @readonly
         * @enum {number}
         */
        MonthLayout: {
            /** Month layout will not be shown. */
            None: 0,

            /** Month layout shows only all-day events. */
            Compact: 1,

            /** Month layout will show both all-day events and hourly events. */
            Full: 2
        },

        /**
         * The display options for the week layout.
         * 
         * @readonly
         * @enum {number}
         */
        WeekLayout: {
            /** Week layout will not be shown. */
            None: 0,

            /** Week layout shows all-day events and hourly events in a single row. */
            Compact: 1,

            /** Week layout shows all-day events in one row and hourly events in a single row. */
            Basic: 2,

            /** Week layout shows all-day events in one row and hourly events in a time view row. */
            TimeView: 3
        },

        /**
         * The display options for the day layout.
         * 
         * @readonly
         * @enum {number}
         */
        DayLayout: {
            /** Day layout will not be shown. */
            None: 0,

            /** Day layout shows all-day events and hourly events in a single row. */
            Compact: 1,

            /** Day layout shows all-day events in one row and hourly events in a single row. */
            Basic: 2,

            /** Day layout shows all-day events in one row and hourly events in a time view row. */
            TimeView: 3
        }
    };

    /**
     * The default calendar settings to use if they are not specified.
     * 
     * @type EventCalendarSettings
     */
    const DefaultSettings = {
        dayLabels: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
        highlightRelatedEvents: true,
        events: function () { return []; },
        eventTemplate: DefaultEventTemplate,
        eventNameTemplate: DefaultEventNameTemplate,
        toolbarTemplate: DefaultToolbarTemplate,
        monthLayout: Constants.MonthLayout.Full,
        weekLayout: Constants.WeekLayout.Basic,
        dayLayout: Constants.DayLayout.TimeView,
        timeViewRange: [420, 1080],
        layout: Constants.Layout.Month
    };

    /**
     * The EventCalendar is responsible for displaying events in a calendar style view.
     * 
     * @param {Object} element - The jQuery element to attach the calendar to.
     * @param {EventCalendarSettings} settings - User supplied settings for the calendar to use.
     *
     * @this EventCalendar
     */
    function EventCalendar(element, settings) {
        /**
         * The jQuery element this calendar is attached to.
         * 
         * @readonly
         * @type {Object}
         */
        this.$element = element;

        /**
         * The current settings for the calendar.
         * 
         * @readonly
         * @type {EventCalendarSettings} */
        this.settings = $.extend(DefaultSettings, settings);

        /**
         * The current date to be used for displaying the calendar.
         * 
         * @protected
         * @type {moment}
         * */
        this.date = null;

        /**
         * The current layout being used to display the calendar.
         * 
         * @protected
         * @type {Layout}
         */
        this.layout = null;

        this.$element.empty();
        this.$element.data('EventCalendar', this);
        this.$element.addClass('calendar-container');
        this.$element.append(this._getWorkingOverlay());
        this.$element.append(this._getToolbar());
        this.$element.append('<div class="calendar-view"></div>');

        this.setLayout(this.settings.layout);
    }

    EventCalendar.Constants = Constants;

    /**
     * Gets an overlay that can be used to indicate to the user that we are
     * currently working in the background.
     * 
     * @private
     * 
     * @returns {object} A jQuery object that can be added to the DOM.
     */
    EventCalendar.prototype._getWorkingOverlay = function () {
        return $('<div class="calendar-working"><i class="fa fa-spinner fa-spin"></i></div>');
    };

    /**
     * Gets the toolbar that will be used for this calendar.
     * 
     * @private
     * 
     * @returns {object} A jQuery object that can be added to the DOM.
     */
    EventCalendar.prototype._getToolbar = function () {
        var _this = this;
        var $toolbar = $(this.settings.toolbarTemplate(this));

        $toolbar.find('.cal-btn-month').on('click', function (e) {
            e.preventDefault();
            $(this).blur();

            _this.setLayout(Constants.Layout.Month);
            _this.update();
        });
        $toolbar.find('.cal-btn-week').on('click', function (e) {
            e.preventDefault();
            $(this).blur();

            _this.setLayout(Constants.Layout.Week);
            _this.update();
        });
        $toolbar.find('.cal-btn-day').on('click', function (e) {
            e.preventDefault();
            $(this).blur();

            _this.setLayout(Constants.Layout.Day);
            _this.update();
        });

        $toolbar.find('.cal-btn-previous').on('click', function (e) {
            e.preventDefault();
            $(this).blur();

            _this.previous();
            _this.update();
        });
        $toolbar.find('.cal-btn-today').on('click', function (e) {
            e.preventDefault();
            $(this).blur();

            _this.today();
            _this.update();
        });
        $toolbar.find('.cal-btn-next').on('click', function (e) {
            e.preventDefault();
            $(this).blur();

            _this.next();
            _this.update();
        });

        return $toolbar;
    };

    /**
     * Gets the header DOM object to use for displaying a week. Specifically,
     * this builds the row that shows the day names above the calendar.
     * 
     * @private
     * 
     * @returns {object} A jQuery object that can be added to the DOM.
     */
    EventCalendar.prototype._getWeekHeader = function () {
        var $week = $('<div class="week header"></div>');

        if (this.layout === Constants.Layout.Month) {
            $week.append($('<div class="week-selector"></div>'));
        }

        for (var i = 0; i < 7; i++) {
            $('<div class="cell"></div>')
                .text(this.settings.dayLabels[i])
                .appendTo($week);
        }

        return $week;
    };

    /**
     * Gets the DOM object to use for displaying a standard day cell.
     * 
     * @private
     *
     * @param {moment} date - The date to build this cell for.
     * @param {string} className - The class name to give this cell.
     * @param {boolean} includeIndicator - If true, then the day number indicator should be included.
     * 
     * @returns {object} A jQuery object that can be added to the DOM.
     */
    EventCalendar.prototype._getDayCell = function (date, className, includeIndicator) {
        var now = moment();
        var $cell = $('<div class="cell"></div>')
            .addClass(className);

        if (this.layout === Constants.Layout.Month) {
            $cell.addClass(this.date.month() === date.month() ? 'active' : 'inactive');
        }
        else {
            $cell.addClass('active');
        }

        if (date.year() === now.year() && date.month() === now.month() && date.date() === now.date()) {
            $cell.addClass('today');
        }

        if (includeIndicator === true) {
            var $d = $('<span></span>').text(date.date()).data('date', moment(date));

            if (this.settings.dayLayout !== Constants.DayLayout.None) {
                var _this = this;

                $d.addClass('clickable');
                $d.on('click', function () {
                    _this.setLayout(Constants.Layout.Day);
                    _this.setDate($(this).data('date'));
                    _this.update();
                });
            }

            $cell.append($('<div class="indicator"></div>').append($d));
        }

        $cell.append($('<div class="events"></div>'));

        return $cell;
    };

    /**
     * Gets the DOM object to use for displaying a time view cell.
     * 
     * @private
     * 
     * @param {moment} date - The date to build this cell for.
     * @param {boolean} includeIndicator - If true, then the day number indicator should be included.
     * 
     * @returns {object} A jQuery object that can be added to the DOM.
     */
    EventCalendar.prototype._getTimeViewCell = function (date, includeIndicator) {
        var $cell = $('<div class="cell timeview"></div>');
        var now = moment();

        $cell.addClass('active');

        if (date.year() === now.year() && date.month() === now.month() && date.date() === now.date()) {
            $cell.addClass('today');
        }

        if (includeIndicator === true) {
            $cell.append($('<div class="indicator"></div>').text(date.date()));
        }
        $cell.append($('<div class="events"></div>'));
        $cell.append($('<div class="time-marker"><span class="time"></span><span class="line"></span></div>').hide());

        return $cell;
    };

    /**
     * Gets the DOM object to use for displaying the specified event.
     * 
     * @private
     * 
     * @param {EventCalendarEvent} event - The event to be transformed into a DOM object.
     * @param {any} options - The additional options to pass to the event template function.
     * 
     * @returns {object} A jQuery object that can be added to the DOM.
     */
    EventCalendar.prototype._getEvent = function (event, options) {
        var $container = $('<div class="event-container"></div>');
        var $event = this.settings.eventTemplate(this, event, options);

        $container.append($event);

        //
        // If the settings specify highlighting related events, then do so.
        //
        if (event.relatedId && this.settings.highlightRelatedEvents === true) {
            $event.attr('data-related-id', event.relatedId);

            $event.on('mouseenter', function () {
                var relatedId = $(this).attr('data-related-id');
                $(this).closest('.calendar-view').find('[data-related-id="' + relatedId + '"]').addClass('related');
            }).on('mouseleave', function () {
                var relatedId = $(this).attr('data-related-id');
                $(this).closest('.calendar-view').find('[data-related-id="' + relatedId + '"]').removeClass('related');
            });
        }

        return $container;
    };

    /**
     * Renders the events into the all-day cells. This takes into account cases where
     * hourly events should also be displayed in the same cell.
     * 
     * @private
     * 
     * @param {EventCalendarEvent[]} events - The events to be considered for rendering.
     */
    EventCalendar.prototype._renderAllDayEvents = function (events) {
        var includeHourly = false;

        if (this.layout === Constants.Layout.Month && this.settings.monthLayout === Constants.MonthLayout.Full) {
            includeHourly = true;
        }

        if (this.layout === Constants.Layout.Week && this.settings.weekLayout === Constants.WeekLayout.Compact) {
            includeHourly = true;
        }

        for (var i in events) {
            var event = events[i];

            var eventStart = moment(event.start);
            var eventEnd = event.end ? moment(event.end) : null;

            if (eventEnd === null || includeHourly === true) {
                var $day = this.$element.find('.calendar-view .cell.day[data-date="' + eventStart.format('L') + '"]');

                if ($day.length > 0) {
                    $day.find('.events').append(this._getEvent(event, { includeTime: eventEnd !== null }));
                }
            }
        }
    };

    /**
     * Renders the appropriate events into the time view cells.
     * 
     * @private
     * 
     * @param {EventCalendarEvent[]} events - The events to be considered for rendering.
     */
    EventCalendar.prototype._renderTimeViewEvents = function (events) {
        var $day;

        //
        // Put all the events in the appropriate days.
        //
        for (var i in events) {
            var event = events[i];

            var eventStart = moment(event.start);
            var eventEnd = event.end ? moment(event.end) : null;

            //
            // Only interested in hourly events.
            //
            if (eventEnd !== null) {
                $day = this.$element.find('.calendar-view .cell.timeview[data-date="' + eventStart.format('L') + '"]');

                if ($day.length > 0) {
                    var $event = this._getEvent(event, {});

                    var start = GetPositionForTime(event.start, $day.data('start-minute'), $day.data('end-minute')) * 100;
                    var end = GetPositionForTime(event.end, $day.data('start-minute'), $day.data('end-minute')) * 100;

                    $event.css('top', start + '%')
                        .css('bottom', 100 - end + '%')
                        .data('start', start)
                        .data('end', end);

                    $day.find('.events').append($event);
                }
            }
        }

        //
        // Now pack them all so they align properly.
        //
        var $days = this.$element.find('.calendar-view .timeview');
        for (i = 0; i < $days.length; i++) {
            $day = $($days[i]);

            //
            // Get all the events in this day in a format we can pack.
            //
            var items = $day.find('.events').children().toArray().map(function (e) {
                return {
                    item: $(e),
                    start: $(e).data('start'),
                    end: $(e).data('end')
                };
            });

            //
            // Pack those items into bags.
            //
            if (items.length > 0) {
                PackItemsIntoBags(items, function ($element, bagIndex, bagCount, _itemIndex, _itemCount) {
                    $element.css('left', bagIndex / bagCount * 100 + '%')
                        .css('width', 100 / bagCount + '%');
                });
            }
        }
    };

    /**
     * Renders the appropriate events into the hourly day cells. If there are no hourly day
     * cells then this function does nothing.
     * 
     * @private
     * 
     * @param {EventCalendarEvent[]} events - The events to be considered for rendering.
     */
    EventCalendar.prototype._renderHourlyEvents = function (events) {
        //
        // Put all the events in the appropriate days.
        //
        for (var i in events) {
            var event = events[i];

            var eventStart = moment(event.start);
            var eventEnd = event.end ? moment(event.end) : null;

            //
            // Only place an event if it is hourly.
            //
            if (eventEnd !== null) {
                var $day = this.$element.find('.calendar-view .cell.hourly[data-date="' + eventStart.format('L') + '"]');

                if ($day.length > 0) {
                    $day.find('.events').append(this._getEvent(event, { includeTime: true }));
                }
            }
        }
    };

    /**
     * Renders all events into the proper cells that currently exist in the layout.
     * 
     * @private
     * 
     * @param {EventCalendarEvent[]} events - The events to be considered for rendering.
     */
    EventCalendar.prototype._renderEvents = function (events) {
        this.$element.find('.calendar-view .cell .events').empty();

        if (this.layout === Constants.Layout.Month) {
            this._renderAllDayEvents(events);
        }
        else if (this.layout === Constants.Layout.Week) {
            this._renderAllDayEvents(events);
            this._renderHourlyEvents(events);
            this._renderTimeViewEvents(events);
        }
        else if (this.layout === Constants.Layout.Day) {
            this._renderTimeViewEvents(events);
        }
    };

    /**
     * Update the toolbar to reflect the current information about the layout and date.
     * 
     * @private
     */
    EventCalendar.prototype._updateToolbar = function () {
        var $title = this.$element.find('.calendar-toolbar .toolbar-title');

        if (this.layout === Constants.Layout.Month) {
            $title.text(this.date.format('MMMM YYYY'));
        }
        else if (this.layout === Constants.Layout.Week) {
            //
            // Only add 6 days, otherwise we actaully end up on the next week.
            //
            var end = moment(this.date).add(6, 'days');
            $title.text(this.date.format('MMM D, YYYY') + ' - ' + end.format('MMM D, YYYY'));
        }
        else if (this.layout === Constants.Layout.Day) {
            $title.text(this.date.format('MMM D, YYYY'));
        }
    };

    /**
     * Generate the day cell layout we need to display all the event days.
     *
     * @private
     */
    EventCalendar.prototype._layoutDayCells = function () {
        var $calendar = this.$element.find('.calendar-view');
        var endDate = this.getEventsEndDate();
        var $week = null;

        for (var date = this.getEventsStartDate(); date < endDate; date.add(1, 'days')) {
            //
            // Start a new week.
            //
            if (date.day() === 0 || $week === null) {
                $week = $('<div class="week"></div>')
                    .appendTo($calendar);

                //
                // If we support week layout and are currently in month layout, then
                // add a clickable week selector bar.
                //
                if (this.settings.weekLayout !== Constants.WeekLayout.None && this.layout === Constants.Layout.Month) {
                    var _this = this;
                    var $selector = $('<div class="week-selector"></div>')
                        .appendTo($week);

                    $selector.on('click', function () {
                        _this.setLayout(Constants.Layout.Week);
                        _this.setDate(moment($(this).closest('.week').children('.cell').first().data('moment')));
                        _this.update();
                    });
                }
            }

            //
            // Generate the day cell and set the options we need.
            //
            var $cell = this._getDayCell(date, 'day', true);
            $cell.attr('data-date', date.format('L'))
                .data('moment', moment(date));
            $week.append($cell);
        }
    };

    /**
     * Generate the hourly cell layout we need to display all the event days.
     *
     * @private
     */
    EventCalendar.prototype._layoutHourlyCells = function () {
        var $calendar = this.$element.find('.calendar-view');
        var endDate = this.getEventsEndDate();
        var $week = null;

        for (var date = this.getEventsStartDate(); date < endDate; date.add(1, 'days')) {
            //
            // Start a new week.
            //
            if (date.day() === 0 || $week === null) {
                $week = $('<div class="week hourly"></div>').appendTo($calendar);
            }

            //
            // Generate the hourly cell we need and set options.
            //
            var $cell = this._getDayCell(date, 'hourly', false);
            $cell.attr('data-date', date.format('L'))
                .data('moment', moment(date));
            $week.append($cell);
        }
    };

    /**
     * Generate the timeview cell layout we need to display all the event days.
     *
     * @private
     */
    EventCalendar.prototype._layoutTimeViewCells = function () {
        var $calendar = this.$element.find('.calendar-view');
        var endDate = this.getEventsEndDate();
        var $week = null;

        for (var date = this.getEventsStartDate(); date < endDate; date.add(1, 'days')) {
            //
            // Start a new week.
            //
            if (date.day() === 0 || $week === null) {
                $week = $('<div class="week"></div>').appendTo($calendar);
            }

            //
            // Generate the cell and set default options.
            //
            var $cell = this._getTimeViewCell(date);
            $cell.attr('data-date', date.format('L'))
                .data('start-minute', this.settings.timeViewRange[0])
                .data('end-minute', this.settings.timeViewRange[1]);
            $week.append($cell);

            //
            // Add indicator to show time of day on mouse hover.
            //
            $cell.find('.events').on('mousemove', function (e) {
                var $tv = $(this).closest('.timeview');
                var startMinute = $tv.data('start-minute');
                var endMinute = $tv.data('end-minute');
                var y = e.pageY - $(this).offset().top;

                var minute = startMinute + Math.round((endMinute - startMinute) * y / $(this).height());

                minute = Math.round(minute / 15) * 15;

                y = (minute - startMinute) * $(this).height() / (endMinute - startMinute);

                y += $(this).offset().top - $tv.offset().top;
                y += 1;

                $tv.find('.time-marker').css('top', y + 'px');
                $tv.find('.time-marker .time').text(moment({ year: 2000, months: 0, day: 1 }).add(minute, 'minutes').format('LT'));
            }).on('mouseenter', function () {
                $(this).closest('.timeview').find('.time-marker').show();
            }).on('mouseleave', function () {
                $(this).closest('.timeview').find('.time-marker').hide();
            });
        }
    };

    /**
     * Update the calendar's layout to fit the month view layout.
     *
     * @private
     */
    EventCalendar.prototype._updateMonthLayout = function () {
        var $calendar = this.$element.find('.calendar-view');

        $calendar.addClass('month-layout');

        $calendar.append(this._getWeekHeader());
        this._layoutDayCells();
    };

    /**
     * Update the calendar's layout to fit the week view layout.
     *
     * @private
     */
    EventCalendar.prototype._updateWeekLayout = function () {
        var $calendar = this.$element.find('.calendar-view');

        $calendar.addClass('week-layout');

        $calendar.append(this._getWeekHeader());
        this._layoutDayCells();

        if (this.settings.weekLayout === Constants.WeekLayout.Basic) {
            this._layoutHourlyCells();
        }
        else if (this.settings.weekLayout === Constants.WeekLayout.TimeView) {
            this._layoutTimeViewCells();
        }
    };

    /**
     * Update the calendar's layout to fit the day view layout.
     *
     * @private
     */
    EventCalendar.prototype._updateDayLayout = function () {
        var $calendar = this.$element.find('.calendar-view');

        $calendar.addClass('day-layout');

        this._layoutTimeViewCells();
    };

    /**
     * Update the calendar layout to match the current layout style and date range.
     *
     * @private
     */
    EventCalendar.prototype._updateLayout = function () {
        //
        // Rebuild the calendar.
        //
        var $calendar = this.$element.find('.calendar-view');
        $calendar.empty()
            .removeClass('month-layout')
            .removeClass('week-layout')
            .removeClass('day-layout');

        if (this.layout === Constants.Layout.Month) {
            this._updateMonthLayout();
        }
        else if (this.layout === Constants.Layout.Week) {
            this._updateWeekLayout();
        }
        else if (this.layout === Constants.Layout.Day) {
            this._updateDayLayout();
        }
    };

    /**
     * Move to the previous date range relative to the current layout.
     */
    EventCalendar.prototype.previous = function () {
        var date = moment(this.date);

        if (this.layout === Constants.Layout.Month) {
            this.setDate(date.subtract(1, 'months'));
        }
        else if (this.layout === Constants.Layout.Week) {
            this.setDate(date.subtract(7, 'days'));
        }
        else if (this.layout === Constants.Layout.Day) {
            this.setDate(date.subtract(1, 'days'));
        }
    };

    /**
     * Move to the next date range relative to the current layout.
     */
    EventCalendar.prototype.next = function () {
        var date = moment(this.date);

        if (this.layout === Constants.Layout.Month) {
            this.setDate(date.add(1, 'months'));
        }
        else if (this.layout === Constants.Layout.Week) {
            this.setDate(date.add(7, 'days'));
        }
        else if (this.layout === Constants.Layout.Day) {
            this.setDate(date.add(1, 'days'));
        }
    };

    /**
     * Set the date such as the calendar will show today's date in the current layout.
     */
    EventCalendar.prototype.today = function () {
        this.setDate(moment());
    };

    /**
     * Set the date for the calendar. This is used as the starting date and will be adjusted
     * to match the current layout's requirements.
     * 
     * @param {string} date - The date at which to use as the starting target point for the calendar, should be in ISO format.
     */
    EventCalendar.prototype.setDate = function (date) {
        var m = moment(date);
        if (this.layout === Constants.Layout.Month) {
            this.date = moment({ year: m.year(), month: m.month(), date: 1 });
        }
        else if (this.layout === Constants.Layout.Week) {
            m.subtract(m.day(), 'days');
            this.date = moment({ year: m.year(), month: m.month(), date: m.date() });
        }
        else if (this.layout === Constants.Layout.Day) {
            this.date = moment({ year: m.year(), month: m.month(), date: m.date() });
        }
    };

    /**
     * Set the current layout for the calendar. This may also update the date if required by
     * the new layout.
     * 
     * @param {Layout} layout - The new layout to be used.
     */
    EventCalendar.prototype.setLayout = function (layout) {
        if (layout === Constants.Layout.Month || layout === Constants.Layout.Week || layout === Constants.Layout.Day) {
            this.layout = layout;

            if (layout === Constants.Layout.Month) {
                this.$element.find('.calendar-toolbar .cal-btn-layout .title').text('Month');
            }
            else if (layout === Constants.Layout.Week) {
                this.$element.find('.calendar-toolbar .cal-btn-layout .title').text('Week');
            }
            else if (layout === Constants.Layout.Day) {
                this.$element.find('.calendar-toolbar .cal-btn-layout .title').text('Day');
            }

            if (this.date) {
                this.setDate(this.date);
            }
            else {
                this.setDate(moment());
            }
        }
    };

    /**
     * Gets the inclusive start date to use for displaying events. This date may be outside the
     * expected bounds. For example, since a month view always starts on Sunday, this
     * date may include additional days so that those extra days can be filled in.
     * 
     * @returns {moment} The date as a MomentJS object.
     */
    EventCalendar.prototype.getEventsStartDate = function () {
        var date = moment(this.date);

        if (this.layout === Constants.Layout.Month || this.layout === Constants.Layout.Week) {
            //
            // Ensure we start at beginning of week.
            //
            date.subtract(date.day(), 'days');
        }

        return date;
    };

    /**
     * Gets the exclusive end date to use for displaying events, meaning your comparison should be
     * event.date >= startDate && event.date < endDate. This date may be outside the expected bounds.
     * See the getEventsStartDate function for details.
     * 
     * @returns {moment} The date as a MomentJS object.
     */
    EventCalendar.prototype.getEventsEndDate = function () {
        var endDate = moment(this.date);

        if (this.layout === Constants.Layout.Month) {
            endDate = moment(this.date).add(1, 'months');

            //
            // Ensure we end at end of week. Since this is an exclusive date (< end)
            // we actually move to the start of the next week.
            //
            while (endDate.day() > 0) {
                endDate.add(1, 'days');
            }
        }
        else if (this.layout === Constants.Layout.Week) {
            endDate = moment(this.date).add(7, 'days');
        }
        else if (this.layout === Constants.Layout.Day) {
            endDate = moment(this.date).add(1, 'days');
        }

        return endDate;
    };

    /**
     * Update the calendar to reflect any changes that have been made to the settings, such
     * as layout and date.
     */
    EventCalendar.prototype.update = function () {
        var _this = this;

        this._updateToolbar();
        this._updateLayout();

        //
        // Get the events.        
        //
        if (typeof this.settings.events === 'function') {
            var updateId = Math.random();
            this._lastUpdateId = updateId;
            var timeoutRef = setTimeout(function () {
                if (updateId === _this._lastUpdateId) {
                    _this.$element.find('.calendar-working').show();
                }
            }, 250);
            this.settings.events(this.getEventsStartDate(), this.getEventsEndDate(), function (events) {
                clearTimeout(timeoutRef);

                if (updateId === _this._lastUpdateId) {
                    _this._renderEvents(events);
                    _this.$element.find('.calendar-working').hide();
                }
            });
        }
        else if (typeof this.settings.events === 'object' && Array.isArray(this.settings.events)) {
            _this._renderEvents(this.settings.events);
        }
    };

    window.EventCalendar = EventCalendar;
})(jQuery);
