// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
System.register(["vue", "../Util/Guid", "./RockFormField", "../Services/String"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Guid_1, RockFormField_1, String_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Guid_1_1) {
                Guid_1 = Guid_1_1;
            },
            function (RockFormField_1_1) {
                RockFormField_1 = RockFormField_1_1;
            },
            function (String_1_1) {
                String_1 = String_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'PhoneNumberBox',
                components: {
                    RockFormField: RockFormField_1.default
                },
                props: {
                    modelValue: {
                        type: String,
                        default: ''
                    }
                },
                emits: [
                    'update:modelValue'
                ],
                data: function () {
                    return {
                        uniqueId: "rock-phonenumberbox-" + Guid_1.newGuid(),
                        internalValue: ''
                    };
                },
                methods: {
                    onChange: function () {
                        this.internalValue = this.formattedValue;
                    }
                },
                computed: {
                    strippedValue: function () {
                        return String_1.stripPhoneNumber(this.internalValue);
                    },
                    formattedValue: function () {
                        return String_1.formatPhoneNumber(this.internalValue);
                    }
                },
                watch: {
                    strippedValue: function () {
                        this.$emit('update:modelValue', this.strippedValue);
                    },
                    modelValue: {
                        immediate: true,
                        handler: function () {
                            var stripped = String_1.stripPhoneNumber(this.modelValue);
                            if (stripped !== this.strippedValue) {
                                this.internalValue = String_1.formatPhoneNumber(stripped);
                            }
                        }
                    }
                },
                template: "\n<RockFormField\n    v-model=\"internalValue\"\n    @change=\"onChange\"\n    formGroupClasses=\"rock-phonenumber-box\"\n    name=\"phonenumberbox\">\n    <template #default=\"{uniqueId, field, errors, disabled, inputGroupClasses}\">\n        <div class=\"control-wrapper\">\n            <div class=\"input-group phone-number-box\" :class=\"inputGroupClasses\">\n                <span class=\"input-group-addon\">\n                    <i class=\"fa fa-phone-square\"></i>\n                </span>\n                <input :id=\"uniqueId\" type=\"text\" class=\"form-control\" v-bind=\"field\" :disabled=\"disabled\" />\n            </div>\n        </div>\n    </template>\n</RockFormField>"
            }));
        }
    };
});
//# sourceMappingURL=PhoneNumberBox.js.map