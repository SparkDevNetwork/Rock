System.register(["vue"], function (exports_1, context_1) {
    "use strict";
    var vue_1, PaneledBlockTemplate;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            }
        ],
        execute: function () {
            PaneledBlockTemplate = vue_1.defineComponent({
                name: "PaneledBlockTemplate",
                data() {
                    return {
                        isDrawerOpen: false
                    };
                },
                methods: {
                    onDrawerPullClick() {
                        this.isDrawerOpen = !this.isDrawerOpen;
                    }
                },
                template: `
<div class="panel panel-block">
    <div class="panel-heading rollover-container">
        <h1 class="panel-title pull-left">
            <slot name="title" />
        </h1>
        <slot name="titleAside" />
    </div>
    <div v-if="$slots.drawer" class="panel-drawer rock-panel-drawer" :class="isDrawerOpen ? 'open' : ''">
        <div class="drawer-content" v-show="isDrawerOpen">
            <slot name="drawer" />
        </div>
        <div class="drawer-pull" @click="onDrawerPullClick">
            <i :class="isDrawerOpen ? 'fa fa-chevron-up' : 'fa fa-chevron-down'"></i>
        </div>
    </div>
    <div class="panel-body">
        <div class="block-content">
            <slot />
        </div>
    </div>
</div>`
            });
            exports_1("default", PaneledBlockTemplate);
        }
    };
});
//# sourceMappingURL=paneledBlockTemplate.js.map