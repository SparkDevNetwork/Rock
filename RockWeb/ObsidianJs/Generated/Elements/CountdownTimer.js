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
System.register(["vue"], function (exports_1, context_1) {
    "use strict";
    var vue_1, AlertType, CountdownTimer;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            }
        ],
        execute: function () {
            /** The type of the alert box to display. Ex: 'success' will appear green and as if something good happened. */
            (function (AlertType) {
                AlertType["default"] = "default";
                AlertType["success"] = "success";
                AlertType["info"] = "info";
                AlertType["danger"] = "danger";
                AlertType["warning"] = "warning";
                AlertType["primary"] = "primary";
                AlertType["validation"] = "validation";
            })(AlertType || (AlertType = {}));
            exports_1("AlertType", AlertType);
            /** Displays a countdown and decremements the seconds. */
            CountdownTimer = vue_1.defineComponent({
                name: 'CountdownTimer',
                props: {
                    /** Seconds until 0:00 */
                    modelValue: {
                        type: Number,
                        required: true
                    }
                },
                data: function () {
                    return {
                        handle: null
                    };
                },
                computed: {
                    timeString: function () {
                        var minutes = Math.floor(this.modelValue / 60);
                        var seconds = Math.floor(this.modelValue % 60);
                        return minutes + ":" + (seconds < 10 ? '0' + seconds : seconds);
                    },
                },
                methods: {
                    onInterval: function () {
                        if (this.modelValue <= 0) {
                            this.$emit('update:modelValue', 0);
                            return;
                        }
                        this.$emit('update:modelValue', Math.floor(this.modelValue - 1));
                    }
                },
                mounted: function () {
                    var _this = this;
                    if (this.handle) {
                        clearInterval(this.handle);
                    }
                    this.handle = setInterval(function () { return _this.onInterval(); }, 1000);
                },
                unmounted: function () {
                    if (this.handle) {
                        clearInterval(this.handle);
                        this.handle = null;
                    }
                },
                template: "\n<span>{{timeString}}</span>"
            });
            exports_1("default", CountdownTimer);
        }
    };
});
//# sourceMappingURL=CountdownTimer.js.map