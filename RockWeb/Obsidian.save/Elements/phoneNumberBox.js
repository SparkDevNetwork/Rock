System.register(["vue", "../Util/guid", "./rockFormField", "../Services/string"], function (exports_1, context_1) {
    "use strict";
    var vue_1, guid_1, rockFormField_1, string_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (guid_1_1) {
                guid_1 = guid_1_1;
            },
            function (rockFormField_1_1) {
                rockFormField_1 = rockFormField_1_1;
            },
            function (string_1_1) {
                string_1 = string_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "PhoneNumberBox",
                components: {
                    RockFormField: rockFormField_1.default
                },
                props: {
                    modelValue: {
                        type: String,
                        default: ""
                    }
                },
                emits: [
                    "update:modelValue"
                ],
                data: function () {
                    return {
                        uniqueId: `rock-phonenumberbox-${guid_1.newGuid()}`,
                        internalValue: ""
                    };
                },
                methods: {
                    onChange() {
                        this.internalValue = this.formattedValue;
                    }
                },
                computed: {
                    strippedValue() {
                        return string_1.stripPhoneNumber(this.internalValue);
                    },
                    formattedValue() {
                        return string_1.formatPhoneNumber(this.internalValue);
                    }
                },
                watch: {
                    formattedValue() {
                        this.$emit("update:modelValue", this.formattedValue);
                    },
                    modelValue: {
                        immediate: true,
                        handler() {
                            const stripped = string_1.stripPhoneNumber(this.modelValue);
                            if (stripped !== this.strippedValue) {
                                this.internalValue = string_1.formatPhoneNumber(stripped);
                            }
                        }
                    }
                },
                template: `
<RockFormField
    v-model="internalValue"
    @change="onChange"
    formGroupClasses="rock-phonenumber-box"
    name="phonenumberbox">
    <template #default="{uniqueId, field, errors, disabled, inputGroupClasses}">
        <div class="control-wrapper">
            <div class="input-group phone-number-box" :class="inputGroupClasses">
                <span class="input-group-addon">
                    <i class="fa fa-phone-square"></i>
                </span>
                <input :id="uniqueId" type="text" class="form-control" v-bind="field" :disabled="disabled" />
            </div>
        </div>
    </template>
</RockFormField>`
            }));
        }
    };
});
//# sourceMappingURL=phoneNumberBox.js.map