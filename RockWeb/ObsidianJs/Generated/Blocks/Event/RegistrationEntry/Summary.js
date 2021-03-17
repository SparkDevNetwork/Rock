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
System.register(["vue", "../../../Controls/RockForm", "../../../Elements/CheckBox", "../../../Elements/EmailBox", "../../../Elements/JavaScriptAnchor", "../../../Elements/RockButton", "../../../Elements/TextBox"], function (exports_1, context_1) {
    "use strict";
    var vue_1, RockForm_1, CheckBox_1, EmailBox_1, JavaScriptAnchor_1, RockButton_1, TextBox_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (RockForm_1_1) {
                RockForm_1 = RockForm_1_1;
            },
            function (CheckBox_1_1) {
                CheckBox_1 = CheckBox_1_1;
            },
            function (EmailBox_1_1) {
                EmailBox_1 = EmailBox_1_1;
            },
            function (JavaScriptAnchor_1_1) {
                JavaScriptAnchor_1 = JavaScriptAnchor_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            },
            function (TextBox_1_1) {
                TextBox_1 = TextBox_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Event.RegistrationEntry.Summary',
                components: {
                    RockButton: RockButton_1.default,
                    TextBox: TextBox_1.default,
                    CheckBox: CheckBox_1.default,
                    EmailBox: EmailBox_1.default,
                    RockForm: RockForm_1.default,
                    JavaScriptAnchor: JavaScriptAnchor_1.default
                },
                methods: {
                    onPrevious: function () {
                        this.$emit('previous');
                    }
                },
                template: "\n<div class=\"registrationentry-summary\">\n    <RockForm>\n        <div class=\"well\">\n            <h4>This Registration Was Completed By</h4>\n            <div class=\"row\">\n                <div class=\"col-md-6\">\n                    <TextBox label=\"First Name\" rules=\"required\" />\n                </div>\n                <div class=\"col-md-6\">\n                    <TextBox label=\"Last Name\" rules=\"required\" />\n                </div>\n            </div>\n            <div class=\"row\">\n                <div class=\"col-md-6\">\n                    <EmailBox label=\"Send Confirmation Emails To\" rules=\"required\" />\n                    <CheckBox label=\"Should Your Account Be Updated To Use This Email Address?\" />\n                </div>\n            </div>\n        </div>\n\n        <div>\n            <h4>Payment Summary</h4>\n            <div class=\"clearfix\">\n                <div class=\"form-group pull-right\">\n                    <label class=\"control-label\">Discount Code</label>\n                    <div class=\"input-group\">\n                        <input type=\"text\" class=\"form-control input-width-md input-sm\" />\n                        <JavaScriptAnchor class=\"btn btn-default btn-sm margin-l-sm\">Apply</JavaScriptAnchor>\n                    </div>\n                </div>\n            </div>\n            <div class=\"fee-table\">\n                <div class=\"row hidden-xs fee-header\">\n                    <div class=\"col-sm-6\">\n                        <strong>Description</strong>\n                    </div>\n                    <div class=\"col-sm-3 fee-value\">\n                        <strong>Amount</strong>\n                    </div>\n                </div>\n                <div class=\"row fee-row-cost\">\n                    <div class=\"col-sm-6 fee-caption\">\n                    </div>\n                    <div class=\"col-sm-3 fee-value\">\n                        <span class=\"visible-xs-inline\">Amount:</span>\n                        $ 50.00\n                    </div>\n                </div>\n                <div class=\"row fee-row-total\">\n                    <div class=\"col-sm-6 fee-caption\">\n                        Total\n                    </div>\n                    <div class=\"col-sm-3 fee-value\">\n                        <span class=\"visible-xs-inline\">Amount:</span>\n                        $ 50.00\n                    </div>\n                </div>\n            </div>\n\n            <div class=\"row fee-totals\">\n                <div class=\"col-sm-offset-8 col-sm-4 fee-totals-options\">\n                    <div class=\"form-group static-control\">\n                        <label class=\"control-label\">\n                            Total Cost\n                        </label>\n                        <div class=\"control-wrapper\">\n                            <div class=\"form-control-static\">\n                                $50.00\n                            </div>\n                        </div>\n                    </div>\n                    <div class=\"form-group static-control\">\n                        <label class=\"control-label\">Amount Due</label>\n                        <div class=\"control-wrapper\">\n                            <div class=\"form-control-static\">\n                                $50.00\n                            </div>\n                        </div>\n                    </div>\n                </div>\n            </div>\n        </div>\n\n        <div class=\"actions\">\n            <RockButton btnType=\"default\" @click=\"onPrevious\">\n                Previous\n            </RockButton>\n            <RockButton btnType=\"primary\" class=\"pull-right\" type=\"submit\">\n                Finish\n            </RockButton>\n        </div>\n    </RockForm>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=Summary.js.map