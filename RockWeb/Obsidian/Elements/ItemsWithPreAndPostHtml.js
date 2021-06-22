System.register(["vue"], function (exports_1, context_1) {
    "use strict";
    var __assign = (this && this.__assign) || function () {
        __assign = Object.assign || function(t) {
            for (var s, i = 1, n = arguments.length; i < n; i++) {
                s = arguments[i];
                for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                    t[p] = s[p];
            }
            return t;
        };
        return __assign.apply(this, arguments);
    };
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
                name: 'ItemsWithPreAndPostHtml',
                props: {
                    items: {
                        type: Array,
                        required: true
                    }
                },
                methods: {
                    onDismiss: function () {
                        this.$emit('dismiss');
                    }
                },
                computed: {
                    augmentedItems: function () {
                        return this.items.map(function (i) { return (__assign(__assign({}, i), { InnerSlotName: "inner-" + i.SlotName })); });
                    },
                    innerTemplate: function () {
                        if (!this.items.length) {
                            return '<slot />';
                        }
                        var templateParts = this.items.map(function (i) { return i.PreHtml + "<slot name=\"inner-" + i.SlotName + "\" />" + i.PostHtml; });
                        return templateParts.join('');
                    },
                    innerComponent: function () {
                        return {
                            name: 'InnerItemsWithPreAndPostHtml',
                            template: this.innerTemplate
                        };
                    }
                },
                template: "\n<component :is=\"innerComponent\">\n    <template v-for=\"item in augmentedItems\" :key=\"item.SlotName\" v-slot:[item.InnerSlotName]>\n        <slot :name=\"item.SlotName\" />\n    </template>\n</component>"
            }));
        }
    };
});
//# sourceMappingURL=ItemsWithPreAndPostHtml.js.map