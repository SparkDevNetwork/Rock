(function ($) {
    'use strict';
    
    // Private RockList "class" that will represent instances of the item list in memory
    // and provide all functionality needed by instances of a list control.
    var RockList = function (element) {
        this.$el = $(element);
    };

    // Prototype declaration for RockList, holds all new functionality of the list
    RockList.prototype = {
        constructor: RockList,
        init: function () {
            this.initListEvents();
        },

        // Wire up DOM events for rockList instance
        initListEvents: function () {
            var self = this;

            // remove event to make sure it doesn't get attached multiple times
            this.$el.off('click');

            // Selecting an item...
            this.$el.on('click', '.rocklist-item', function (e) {
                e.preventDefault();
                e.stopPropagation();

                var $rockList = $(this).parents('.rocklist'),
                    $item = $(this),
                    id = $item.closest('li').attr('data-id');

                // clear all previous selections
                $rockList.find('.selected').removeClass('selected');

                // mark the selected as selected
                $item.toggleClass('selected');

                self.$el.trigger('rockList:selected', id);
            });
        }
    };

    // jQuery plugin definition
    $.fn.rockList = function () {

        // For each element matching the selector, attempt to get an instance
        // of RockList from $el.data, if not present, create a new instance
        // of RockList and stash it there, then initialize the list.
        return this.each(function () {
            var $el = $(this);
            var rockList = $el.data('rockList');
            
            if (!rockList) {
                // create a new rocklist
                rockList = new RockList(this);
            }

            $el.data('rockList', rockList);
            rockList.init();
        });
    };
}(jQuery));