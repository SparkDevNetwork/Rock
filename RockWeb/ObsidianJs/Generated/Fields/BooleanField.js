define(["require", "exports", "../Vendor/Vue/vue.js", "./Index.js"], function (require, exports, vue_js_1, Index_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var fieldTypeGuid = '1EDAFDED-DFE6-4334-B019-6EECBA89E05A';
    exports.default = Index_js_1.registerFieldType(fieldTypeGuid, vue_js_1.defineComponent({
        name: 'BooleanField',
        props: {
            modelValue: {
                type: String,
                required: true
            }
        },
        computed: {
            safeValue: function () {
                return (this.modelValue || '').trim().toLowerCase();
            },
            valueIsNull: function () {
                return !this.safeValue;
            },
            valueIsTrue: function () {
                return ['true', 'yes', 't', 'y', '1'].indexOf(this.safeValue) !== -1;
            },
            valueIsFalse: function () {
                return !this.valueIsTrue && !this.valueIsNull;
            },
            valueAsBooleanOrNull: function () {
                if (this.valueIsNull) {
                    return null;
                }
                return this.valueIsTrue;
            },
            valueAsYesNoOrNull: function () {
                if (this.valueIsNull) {
                    return null;
                }
                return this.valueIsTrue ? 'Yes' : 'No';
            }
        },
        template: "\n<span>{{ valueAsYesNoOrNull }}</span>"
    }));
});
//# sourceMappingURL=BooleanField.js.map