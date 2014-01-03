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
                    simpleEngine = {
                        compile: function (template) {
                            return {
                                render: function (context) {
                                    return template.replace(/\{\{(\w+)\}\}/g, function (match, p1) {
                                        return context[p1];
                                    });
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
                    engine: simpleEngine,
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
            controls: {},
            initialize: function (options) {
                var autoCompleteDropDown,
                    settings = $.extend({}, exports.defaults, options);

                if (!settings.controlId) throw 'controlId is required';
                if (!settings.valueControlId) throw 'valueControlId is required';
                if (!settings.url) throw 'url is required';

                if (!exports.controls[settings.controlId]) {
                    autoCompleteDropDown = new AutoCompleteDropDown(settings);
                    exports.controls[settings.controlId] = autoCompleteDropDown;
                } else {
                    autoCompleteDropDown = exports.controls[settings.controlId];
                }

                // Delay initialization until after the DOM is ready
                $(function () {
                    autoCompleteDropDown.initialize();
                });
            }
        };

        return exports;
    }());
}(jQuery));