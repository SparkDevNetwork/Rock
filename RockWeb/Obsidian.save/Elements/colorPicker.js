System.register(["vue", "./rockFormField"], function (exports_1, context_1) {
    "use strict";
    var vue_1, rockFormField_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (rockFormField_1_1) {
                rockFormField_1 = rockFormField_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "ColorPicker",
                components: {
                    RockFormField: rockFormField_1.default
                },
                props: {
                    modelValue: {
                        type: String,
                        required: true
                    },
                    placeholder: {
                        type: String,
                        default: ""
                    }
                },
                emits: [
                    "update:modelValue"
                ],
                data: function () {
                    return {
                        internalValue: this.modelValue
                    };
                },
                mounted() {
                    const $colorPicker = window["$"](this.$refs.colorPicker);
                    $colorPicker.colorpicker();
                    $colorPicker.find("> input").on("change", () => {
                        this.internalValue = $colorPicker.find("> input").val();
                    });
                },
                computed: {},
                watch: {
                    internalValue() {
                        this.$emit("update:modelValue", this.internalValue);
                    },
                    modelValue() {
                        this.internalValue = this.modelValue;
                    }
                },
                template: `
<RockFormField
    v-model="internalValue"
    formGroupClasses="rock-color-picker"
    name="colorpicker">
    <template #default="{uniqueId, field, errors, disabled, tabIndex}">
        <div class="control-wrapper">
            <div ref="colorPicker" class="input-group input-width-lg">
                <input :id="uniqueId" type="text" class="form-control" v-bind="field" :disabled="disabled" :placeholder="placeholder" :tabindex="tabIndex" />
                <span class="input-group-addon">
                    <i></i>
                </span>
            </div>
        </div>
    </template>
</RockFormField>`
            }));
        }
    };
});
//# sourceMappingURL=colorPicker.js.map