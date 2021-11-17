System.register(["vue"], function (exports_1, context_1) {
    "use strict";
    var vue_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "ItemsWithPreAndPostHtml",
                props: {
                    items: {
                        type: Array,
                        required: true
                    }
                },
                methods: {
                    onDismiss: function () {
                        this.$emit("dismiss");
                    }
                },
                computed: {
                    augmentedItems() {
                        return this.items.map(i => (Object.assign(Object.assign({}, i), { innerSlotName: `inner-${i.slotName}` })));
                    },
                    innerTemplate() {
                        if (!this.items.length) {
                            return "<slot />";
                        }
                        const templateParts = this.items.map(i => `${i.preHtml}<slot name="inner-${i.slotName}" />${i.postHtml}`);
                        return templateParts.join("");
                    },
                    innerComponent() {
                        return {
                            name: "InnerItemsWithPreAndPostHtml",
                            template: this.innerTemplate
                        };
                    }
                },
                template: `
<component :is="innerComponent">
    <template v-for="item in augmentedItems" :key="item.slotName" v-slot:[item.innerSlotName]>
        <slot :name="item.slotName" />
    </template>
</component>`
            }));
        }
    };
});
//# sourceMappingURL=itemsWithPreAndPostHtml.js.map