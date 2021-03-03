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
System.register(["vue", "../../../Controls/RockField", "../../../Elements/Alert"], function (exports_1, context_1) {
    "use strict";
    var vue_1, RockField_1, Alert_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (RockField_1_1) {
                RockField_1 = RockField_1_1;
            },
            function (Alert_1_1) {
                Alert_1 = Alert_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Event.RegistrationEntry.RegistrantAttributeField',
                components: {
                    Alert: Alert_1.default,
                    RockField: RockField_1.default
                },
                props: {
                    field: {
                        type: Object,
                        required: true
                    }
                },
                data: function () {
                    return {
                        fieldControlComponent: null,
                        fieldControlComponentProps: {},
                        value: ''
                    };
                },
                computed: {
                    attribute: function () {
                        return this.field.Attribute || null;
                    },
                    props: function () {
                        if (!this.attribute) {
                            return {};
                        }
                        return {
                            fieldTypeGuid: this.attribute.FieldTypeGuid,
                            isEditMode: true,
                            label: this.attribute.Name,
                            help: this.attribute.Description,
                            rules: this.field.IsRequired ? 'required' : ''
                        };
                    }
                },
                template: "\n<RockField v-if=\"attribute\" v-model=\"value\" v-bind=\"props\" />\n<Alert v-else alertType=\"danger\">Could not resolve attribute field</Alert>"
            }));
        }
    };
});
//# sourceMappingURL=RegistrantAttributeField.js.map