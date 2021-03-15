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
                name: 'Event.RegistrationEntry.Summary',
                components: {
                    ProgressBar: ProgressBar_1.default,
                    RockButton: RockButton_1.default
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
                    },
                    numberOfPages: {
                        type: Number,
                        required: true
                    }
                },
                data: function () {
                    return {
                        registrationTemplateForms: (this.configurationValues['registrationTemplateForms'] || [])
                    };
                },
                computed: {
                    completionPercentDecimal: function () {
                        return (this.numberOfPages - 1) / this.numberOfPages;
                    },
                    completionPercentInt: function () {
                        return this.completionPercentDecimal * 100;
                    }
                },
                methods: {
                    onPrevious: function () {
                        this.$emit('previous');
                    }
                },
                template: "\n<div>\n    <h1>Summary</h1>\n    <ProgressBar :percent=\"completionPercentInt\" />\n    <div class=\"actions\">\n        <RockButton btnType=\"default\" @click=\"onPrevious\">\n            Previous\n        </RockButton>\n    </div>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=Summary.js.map