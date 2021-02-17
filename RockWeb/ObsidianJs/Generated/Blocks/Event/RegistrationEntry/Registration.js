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
System.register(["vue", "../../../Elements/ProgressBar", "../../../Elements/RockButton"], function (exports_1, context_1) {
    "use strict";
    var vue_1, ProgressBar_1, RockButton_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
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
                name: 'Event.RegistrationEntry.Registration',
                components: {
                    RockButton: RockButton_1.default,
                    ProgressBar: ProgressBar_1.default
                },
                setup: function () {
                    return {
                        configurationValues: vue_1.inject('configurationValues')
                    };
                },
                props: {
                    registrants: {
                        type: Array,
                        required: true
                    }
                },
                data: function () {
                    return {
                        registrationTemplateForms: (this.configurationValues['registrationTemplateForms'] || [])
                    };
                },
                computed: {
                    formCountPerRegistrant: function () {
                        return this.registrationTemplateForms.length;
                    },
                    numberOfPages: function () {
                        // All of the steps are 1 page except the "per-registrant"
                        return 3 + (this.registrants.length * this.formCountPerRegistrant);
                    },
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
                template: "\n<div class=\"registrationentry-registration-attributes\">\n    <h1>Registration Attributes</h1>\n    <ProgressBar :percent=\"completionPercentInt\" />\n\n    <div class=\"actions\">\n        <RockButton btnType=\"default\" @click=\"onPrevious\">\n            Previous\n        </RockButton>\n        <RockButton btnType=\"primary\" class=\"pull-right\" @click=\"onNext\">\n            Next\n        </RockButton>\n    </div>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=Registration.js.map