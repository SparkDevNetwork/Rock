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

                var key = sessionStorage.getItem("com.rockrms.search");
                if (key && key != '') {
                    var $search = self.$el.parents('.smartsearch');
                    $search.find('input:hidden').val(key);
                    $search.find('a.dropdown-toggle > span').html($search.find('li[data-key="' + key + '"] > a').html());
                }

                this.$el.typeahead({
                    name: this.name,
                    limit: 15,
                    remote: {
                        url: Rock.settings.get('baseUrl') + 'api/search?type=%TYPE&term=%QUERY&$top=15',
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
                            url = Rock.settings.get('baseUrl') + targetUrl.replace('{0}', encodeURIComponent(term.trim()));

                        window.location = url;
                    };

                // Listen for typeahead's custom events and trigger search when hit
                this.$el.on('typeahead:selected typeahead:autocompleted', function (e, obj, name) {
                    search(obj.value);
                });

                // Listen for the ENTER key being pressed while in the search box and trigger search when hit
                this.$el.keydown(function (e) {
                    if (e.keyCode === 13) {
                        e.preventDefault();
                        return false;
                    }
                });
                this.$el.keyup(function (e) {
                    if (e.keyCode === 13 && "" !== $(this).val().trim() ) {
                        search($(this).val());
                    }
                });

                // Wire up "change" handler for search type "dropdown menu"
                this.$el.parents('.smartsearch').find('.dropdown-menu a').click(function () {
                    var $this = $(this),
                        text = $this.html();

                    var key = $this.parent().attr('data-key');
                    sessionStorage.setItem("com.rockrms.search", key);

                    $this.parents('.dropdown-menu').siblings('.navbar-link').find('span').html(text);
                    self.$el.parents('.smartsearch').find('input:hidden').val(key)
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