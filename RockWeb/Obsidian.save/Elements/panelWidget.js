System.register(["vue", "./rockButton"], function (exports_1, context_1) {
    "use strict";
    var vue_1, rockButton_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (rockButton_1_1) {
                rockButton_1 = rockButton_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "PanelWidget",
                components: {
                    RockButton: rockButton_1.default
                },
                props: {
                    isDefaultOpen: {
                        type: Boolean,
                        default: false
                    }
                },
                data() {
                    return {
                        isOpen: this.isDefaultOpen
                    };
                },
                methods: {
                    toggle() {
                        this.isOpen = !this.isOpen;
                    }
                },
                template: `
<section class="panel panel-widget rock-panel-widget">
    <header class="panel-heading clearfix clickable" @click="toggle">
        <div class="pull-left">
            <slot name="header" />
        </div>
        <div class="pull-right">
            <RockButton btnType="link" btnSize="xs">
                <i v-if="isOpen" class="fa fa-chevron-up"></i>
                <i v-else class="fa fa-chevron-down"></i>
            </RockButton>
        </div>
    </header>
    <div v-if="isOpen" class="panel-body">
        <slot />
    </div>
</section>`
            }));
        }
    };
});
//# sourceMappingURL=panelWidget.js.map