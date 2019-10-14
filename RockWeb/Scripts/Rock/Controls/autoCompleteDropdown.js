(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.autoCompleteDropDown = (function () {
        var exports,
            AutoCompleteDropDown = function (options) {
                this.controlId = options.controlId;
                this.valueControlId = options.valueControlId;
                this.$el = $('#' + this.controlId);
                this.name = options.name;
                this.url = options.url;
                this.limit = options.limit;
                this.idProperty = options.idProperty;
                this.valuekey = options.valuekey;
                this.template = options.template;
                this.header = options.header;
                this.footer = options.footer;
            };

        AutoCompleteDropDown.prototype = {
            constructor: AutoCompleteDropDown,
            initialize: function () {
                var self = this,
                    Liquid = require('liquid'),
                    liquidEngine = {
                        compile: function (template) {
                            var parsedTemplate = Liquid.Template.parse(template);
                            return {
                                render: function (context) {
                                    return parsedTemplate.render(context);
                                }
                            };
                        }
                    };

                this.$el.typeahead({
                    name: this.name,
                    limit: this.limit,
                    valueKey: this.valuekey,
                    template: this.template,
                    header: this.header,
                    footer: this.footer,
                    engine: liquidEngine,
                    remote: {
                        url: Rock.settings.get('baseUrl') + this.url,
                        filter: function (response) {
                            response.forEach(function (item) {
                                item['tokens'] = item[self.valuekey].replace(/,/g, '').split(" ");
                            });
                            return response;
                        }
                    }
                });

                this.initializeEventHandlers();
            },

            initializeEventHandlers: function () {
                var self = this,
                    idProperty = this.idProperty,
                    setValue = function (datum) {
                        $('#' + self.valueControlId).val(datum[idProperty]);
                    };

                // Listen for typeahead's custom events and trigger search when hit
                this.$el.on('typeahead:selected typeahead:autocompleted', function (e, obj, name) {
                    setValue(obj);
                    if (typeof self.onSelected === "function") {
                        self.onSelected(obj);
                    }
                });

            }
        };

        exports = {
            defaults: {
                controlId: null,
                name: 'autoComplete',
                valueControlId: null,
                limit: 5,
                idProperty: 'Id',
                valuekey: 'value',
                template: '<p>{{value}}</p>',
                header: '',
                footer: ''
            },
            initialize: function (options) {
                var autoCompleteDropDown,
                    settings = $.extend({}, exports.defaults, options);

                if (!settings.controlId) throw 'controlId is required';
                if (!settings.valueControlId) throw 'valueControlId is required';
                if (!settings.url) throw 'url is required';

                autoCompleteDropDown = new AutoCompleteDropDown(settings);

                // Delay initialization until after the DOM is ready
                $(function () {
                    autoCompleteDropDown.initialize();
                });

                return autoCompleteDropDown;
            }
        };

        return exports;
    }());
}(jQuery));
