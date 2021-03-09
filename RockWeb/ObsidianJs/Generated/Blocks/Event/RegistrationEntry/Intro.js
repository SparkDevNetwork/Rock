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
System.register(["vue", "../../../Elements/NumberUpDown", "../../../Elements/RockButton"], function (exports_1, context_1) {
    "use strict";
    var vue_1, NumberUpDown_1, RockButton_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (NumberUpDown_1_1) {
                NumberUpDown_1 = NumberUpDown_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Event.RegistrationEntry.Intro',
                components: {
                    NumberUpDown: NumberUpDown_1.default,
                    RockButton: RockButton_1.default
                },
                setup: function () {
                    return {
                        viewModel: vue_1.inject('configurationValues')
                    };
                },
                props: {
                    initialRegistrantCount: {
                        type: Number,
                        default: 1
                    }
                },
                data: function () {
                    return {
                        numberOfRegistrants: this.initialRegistrantCount || 1
                    };
                },
                methods: {
                    onNext: function () {
                        this.$emit('next', {
                            numberOfRegistrants: this.numberOfRegistrants
                        });
                    },
                },
                template: "\n<div class=\"registrationentry-intro\">\n    <div class=\"text-left\" v-html=\"viewModel.InstructionsHtml\">\n    </div>\n    <div class=\"registrationentry-intro\">\n        <h1>How many {{viewModel.PluralRegistrantTerm}} will you be registering?</h1>\n        <NumberUpDown v-model=\"numberOfRegistrants\" class=\"margin-t-sm\" numberIncrementClasses=\"input-lg\" />\n    </div>\n    <div class=\"actions text-right\">\n        <RockButton btnType=\"primary\" @click=\"onNext\">\n            Next\n        </RockButton>\n    </div>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=Intro.js.map