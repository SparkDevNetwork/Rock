System.register(["vue", "./utils", "../Controls/addressControl"], function (exports_1, context_1) {
    "use strict";
    var vue_1, utils_1, addressControl_1, EditComponent;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (utils_1_1) {
                utils_1 = utils_1_1;
            },
            function (addressControl_1_1) {
                addressControl_1 = addressControl_1_1;
            }
        ],
        execute: function () {
            exports_1("EditComponent", EditComponent = vue_1.defineComponent({
                name: "AddressField.Edit",
                components: {
                    AddressControl: addressControl_1.default
                },
                props: utils_1.getFieldEditorProps(),
                setup(props, { emit }) {
                    const internalValue = vue_1.ref({});
                    vue_1.watch(() => props.modelValue, () => {
                        try {
                            internalValue.value = JSON.parse(props.modelValue || "{}");
                        }
                        catch (_a) {
                            internalValue.value = {};
                        }
                    }, { immediate: true });
                    vue_1.watch(() => internalValue.value, () => {
                        emit("update:modelValue", JSON.stringify(internalValue.value));
                    }, { deep: true });
                    return {
                        internalValue
                    };
                },
                template: `
<AddressControl v-model="internalValue" />
`
            }));
        }
    };
});
//# sourceMappingURL=addressFieldComponents.js.map