System.register(["vue", "./ComponentFromUrl"], function (exports_1, context_1) {
    "use strict";
    var vue_1, ComponentFromUrl_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (ComponentFromUrl_1_1) {
                ComponentFromUrl_1 = ComponentFromUrl_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'GatewayControl',
                components: {
                    ComponentFromUrl: ComponentFromUrl_1.default
                },
                props: {
                    gatewayControlModel: {
                        type: Object,
                        required: true
                    }
                },
                computed: {
                    url: function () {
                        return this.gatewayControlModel.FileUrl;
                    },
                    settings: function () {
                        return this.gatewayControlModel.Settings;
                    }
                },
                template: "\n<ComponentFromUrl :url=\"url\" :settings=\"settings\" />"
            }));
        }
    };
});
//# sourceMappingURL=GatewayControl.js.map