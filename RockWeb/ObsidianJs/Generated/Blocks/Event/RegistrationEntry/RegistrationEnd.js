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
System.register(["vue", "../../../Controls/AttributeValuesContainer", "../../../Elements/ProgressBar", "../../../Elements/RockButton"], function (exports_1, context_1) {
    "use strict";
    var vue_1, AttributeValuesContainer_1, ProgressBar_1, RockButton_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (AttributeValuesContainer_1_1) {
                AttributeValuesContainer_1 = AttributeValuesContainer_1_1;
            },
            function (ProgressBar_1_1) {
                ProgressBar_1 = ProgressBar_1_1;
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
                    ProgressBar: ProgressBar_1.default,
                    AttributeValuesContainer: AttributeValuesContainer_1.default
                },
                setup: function () {
                    return {
                        viewModel: vue_1.inject('configurationValues')
                    };
                },
                props: {
                    registrants: {
                        type: Array,
                        required: true
                    },
                    numberOfPages: {
                        type: Number,
                        required: true
                    }
                },
                data: function () {
                    return {
                        attributeValues: []
                    };
                },
                computed: {
                    completionPercentDecimal: function () {
                        return (this.numberOfPages - 2) / this.numberOfPages;
                    },
                    completionPercentInt: function () {
                        return this.completionPercentDecimal * 100;
                    }
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
                            this.attributeValues = this.viewModel.RegistrationAttributesEnd.map(function (a) { return ({
                                Attribute: a,
                                AttributeId: a.Id,
                                Value: ''
                            }); });
                        }
                    }
                },
                template: "\n<div class=\"registrationentry-registration-attributes\">\n    <h1>{{viewModel.RegistrationAttributeTitleEnd}}</h1>\n    <ProgressBar :percent=\"completionPercentInt\" />\n\n    <AttributeValuesContainer :attributeValues=\"attributeValues\" isEditMode />\n\n    <div class=\"actions\">\n        <RockButton btnType=\"default\" @click=\"onPrevious\">\n            Previous\n        </RockButton>\n        <RockButton btnType=\"primary\" class=\"pull-right\" @click=\"onNext\">\n            Next\n        </RockButton>\n    </div>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=RegistrationEnd.js.map