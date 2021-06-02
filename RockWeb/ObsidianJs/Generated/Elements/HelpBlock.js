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
System.register(["vue", "./JavaScriptAnchor"], function (exports_1, context_1) {
    "use strict";
    var vue_1, JavaScriptAnchor_1, HelpBlock;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (JavaScriptAnchor_1_1) {
                JavaScriptAnchor_1 = JavaScriptAnchor_1_1;
            }
        ],
        execute: function () {
            /** Displays a help block tool-tip. */
            HelpBlock = vue_1.defineComponent({
                name: 'HelpBlock',
                components: {
                    JavaScriptAnchor: JavaScriptAnchor_1.default
                },
                props: {
                    text: {
                        type: String,
                        required: true
                    }
                },
                mounted: function () {
                    var jquery = window['$'];
                    jquery(this.$el).tooltip();
                },
                template: "\n<JavaScriptAnchor class=\"help\" tabindex=\"-1\" data-toggle=\"tooltip\" data-placement=\"auto\" data-container=\"body\" data-html=\"true\" title=\"\" :data-original-title=\"text\">\n    <i class=\"fa fa-info-circle\"></i>\n</JavaScriptAnchor>"
            });
            exports_1("default", HelpBlock);
        }
    };
});
//# sourceMappingURL=HelpBlock.js.map