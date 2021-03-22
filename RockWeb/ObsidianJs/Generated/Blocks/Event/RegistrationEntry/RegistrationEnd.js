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
System.register(["vue", "../../../Controls/AttributeValuesContainer", "../../../Controls/RockForm", "../../../Elements/RockButton"], function (exports_1, context_1) {
    "use strict";
    var vue_1, AttributeValuesContainer_1, RockForm_1, RockButton_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (AttributeValuesContainer_1_1) {
                AttributeValuesContainer_1 = AttributeValuesContainer_1_1;
            },
            function (RockForm_1_1) {
                RockForm_1 = RockForm_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Event.RegistrationEntry.RegistrationEnd',
                components: {
                    RockButton: RockButton_1.default,
                    AttributeValuesContainer: AttributeValuesContainer_1.default,
                    RockForm: RockForm_1.default
                },
                setup: function () {
                    return {
                        registrationEntryState: vue_1.inject('registrationEntryState')
                    };
                },
                data: function () {
                    return {
                        attributeValues: []
                    };
                },
                methods: {
                    onPrevious: function () {
                        this.$emit('previous');
                    },
                    onNext: function () {
                        this.$emit('next');
                    }
                },
                watch: {
                    viewModel: {
                        immediate: true,
                        handler: function () {
                            var _this = this;
                            this.attributeValues = this.registrationEntryState.ViewModel.RegistrationAttributesEnd.map(function (a) {
                                var currentValue = _this.registrationEntryState.RegistrationFieldValues[a.Guid] || '';
                                return {
                                    Attribute: a,
                                    AttributeId: a.Id,
                                    Value: currentValue
                                };
                            });
                        }
                    },
                    attributeValues: {
                        immediate: true,
                        deep: true,
                        handler: function () {
                            for (var _i = 0, _a = this.attributeValues; _i < _a.length; _i++) {
                                var attributeValue = _a[_i];
                                var attribute = attributeValue.Attribute;
                                if (attribute) {
                                    this.registrationEntryState.RegistrationFieldValues[attribute.Guid] = attributeValue.Value;
                                }
                            }
                        }
                    }
                },
                template: "\n<div class=\"registrationentry-registration-attributes\">\n    <RockForm @submit=\"onNext\">\n        <AttributeValuesContainer :attributeValues=\"attributeValues\" isEditMode />\n\n        <div class=\"actions\">\n            <RockButton btnType=\"default\" @click=\"onPrevious\">\n                Previous\n            </RockButton>\n            <RockButton btnType=\"primary\" class=\"pull-right\" type=\"submit\">\n                Next\n            </RockButton>\n        </div>\n    </RockForm>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=RegistrationEnd.js.map