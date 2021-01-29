System.register(["../Vendor/Vue/vue.js"], function (exports_1, context_1) {
    "use strict";
    var vue_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'MyWellGatewayControl',
                props: {
                    settings: {
                        type: Object,
                        required: true
                    },
                    submit: {
                        type: Boolean,
                        required: true
                    },
                    args: {
                        type: Object,
                        required: true
                    }
                },
                data: function () {
                    return {
                        tokenizer: null,
                        token: ''
                    };
                },
                methods: {
                    handleResponse: function (resp) {
                        this.token = resp.token;
                        this.args.ReferenceNumber = this.token;
                        this.$emit('done');
                    }
                },
                computed: {
                    publicApiKey: function () {
                        return this.settings.PublicApiKey;
                    },
                    gatewayUrl: function () {
                        return this.settings.GatewayUrl;
                    },
                    tokenizerSettings: function () {
                        var _this = this;
                        return {
                            apikey: this.publicApiKey,
                            url: this.gatewayUrl,
                            container: this.$refs['container'],
                            submission: function (resp) {
                                _this.handleResponse(resp);
                            },
                            settings: {
                                payment: {
                                    types: ['card'],
                                    ach: {
                                        'sec_code': 'web'
                                    }
                                },
                                styles: {
                                    body: {
                                        color: 'rgb(51, 51, 51)'
                                    },
                                    '#app': {
                                        padding: '5px 15px'
                                    },
                                    'input,select': {
                                        'color': 'rgb(85, 85, 85)',
                                        'border-radius': '4px',
                                        'background-color': 'rgb(255, 255, 255)',
                                        'border': '1px solid rgb(204, 204, 204)',
                                        'box-shadow': 'rgba(0, 0, 0, 0.075) 0px 1px 1px 0px inset',
                                        'padding': '6px 12px',
                                        'font-size': '14px',
                                        'height': '34px',
                                        'font-family': 'OpenSans, \'Helvetica Neue\', Helvetica, Arial, sans-serif'
                                    },
                                    'input:focus,select:focus': {
                                        'border': '1px solid #66afe9',
                                        'box-shadow': '0 0 0 3px rgba(102,175,233,0.6)'
                                    },
                                    'select': {
                                        'padding': '6px 4px'
                                    },
                                    '.fieldsetrow': {
                                        'margin-left': '-2.5px',
                                        'margin-right': '-2.5px'
                                    },
                                    '.card > .fieldset': {
                                        'padding': '0 !important',
                                        'margin': '0 2.5px 5px !important'
                                    },
                                    'input[type=number]::-webkit-inner-spin-button,input[type=number]::-webkit-outer-spin-button': {
                                        '-webkit-appearance': 'none',
                                        'margin': '0'
                                    }
                                }
                            }
                        };
                    }
                },
                watch: {
                    submit: function () {
                        if (!this.token && this.submit && this.tokenizer) {
                            this.tokenizer.submit();
                        }
                    }
                },
                mounted: function () {
                    this.tokenizer = new window['Tokenizer'](this.tokenizerSettings);
                    this.tokenizer.create();
                },
                template: "\n<div v-if=\"!token\">\n    <div ref=\"container\"></div>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=MyWellGatewayControl.js.map