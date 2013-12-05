(function () {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.searchField = (function () {
        var exports,
            SearchField = function (options) {
                this.controlId = options.controlId;
                this.$el = $('#' + this.controlId);
                this.name = options.name;
            };

        SearchField.prototype = {
            constructor: SearchField,
            initialize: function () {
                var self = this;

                this.$el.typeahead({
                    name: this.name,
                    remote: {
                        url: Rock.settings.get('baseUrl') + 'api/search?type=%TYPE&term=%QUERY',
                        replace: function (url, uriEncodedQuery) {
                            var query = url;
                            query = query.replace('%TYPE', self.$el.parents('.smartsearch').find('input:hidden').val());
                            query = query.replace('%QUERY', uriEncodedQuery);
                            return query;
                        }
                    }
                });

                this.initializeEventHandlers();
            },
            initializeEventHandlers: function () {
                var self = this,
                    search = function (term) {
                        var keyVal = self.$el.parents('.smartsearch').find('input:hidden').val(),
                            $li = self.$el.parents('.smartsearch').find('li[data-key="' + keyVal + '"]'),
                            targetUrl = $li.attr('data-target'),
                            url = Rock.settings.get('baseUrl') + targetUrl.replace('{0}', encodeURIComponent(term));

                        window.location = url;
                    };

                // Listen for typeahead's custom events and trigger search when hit
                this.$el.on('typeahead:selected typeahead:autocompleted', function (e, obj, name) {
                    search(obj.value);
                });

                // Listen for the ENTER key being pressed while in the search box and trigger search when hit
                this.$el.keyup(function (e) {
                    if (e.keyCode === 13) {
                        search($(this).val());
                    }
                });

                // Wire up "change" handler for search type "dropdown menu"
                this.$el.parents('.smartsearch').find('.dropdown-menu a').click(function () {
                    var $this = $(this),
                        text = $this.html();

                    $this.parents('.dropdown-menu').siblings('.navbar-link').find('span').html(text);
                    self.$el.parents('.smartsearch').find('input:hidden').val($this.parent().attr('data-key'));
                });
            }
        };

        exports = {
            defaults: {
                controlId: null,
                name: 'search'
            },
            controls: {},
            initialize: function (options) {
                var searchField,
                    settings = $.extend({}, exports.defaults, options);

                if (!settings.controlId) throw 'controlId is required';
                
                if (!exports.controls[settings.controlId]) {
                    searchField = new SearchField(settings);
                    exports.controls[settings.controlId] = searchField;
                } else {
                    searchField = exports.controls[settings.controlId];
                }

                // Delay initialization until after the DOM is ready
                $(function () {
                    searchField.initialize();
                });
            }
        };

        return exports;
    }());
}());