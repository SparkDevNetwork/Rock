System.register([], function (exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
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
    function smoothScrollToTop() {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
    exports_1("smoothScrollToTop", smoothScrollToTop);
    return {
        setters: [],
        execute: function () {
            exports_1("default", {
                smoothScrollToTop: smoothScrollToTop
            });
        }
    };
});
//# sourceMappingURL=Page.js.map