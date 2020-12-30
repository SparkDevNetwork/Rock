define(["require", "exports", "../Elements/DropDownList.js"], function (require, exports, DropDownList_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.createCommonEntityPicker = void 0;
    /**
    * Generate and add a common entity picker. Common entities are those stored in the Obsidian store.
    * @param {any} entityName The entity name (ex: Campus)
    * @param {any} getOptionsFunc A function called with the store as a parameter that should return the
    * options object list for the drop down list.
    */
    function createCommonEntityPicker(entityName, getOptionsFunc) {
        return {
            name: entityName + "Picker",
            components: {
                DropDownList: DropDownList_js_1.default
            },
            props: {
                modelValue: {
                    type: String,
                    required: true
                },
                label: {
                    type: String,
                    required: true
                },
                required: {
                    type: Boolean,
                    default: false
                }
            },
            emits: [
                'update:modelValue'
            ],
            data: function () {
                return {
                    internalValue: '',
                    isLoading: false
                };
            },
            computed: {
                options: function () {
                    return getOptionsFunc(this.$store);
                }
            },
            methods: {
                onChange: function () {
                    this.$emit('update:modelValue', this.internalValue);
                }
            },
            watch: {
                value: function () {
                    this.internalValue = this.modelValue;
                }
            },
            template: "<DropDownList v-model=\"internalValue\" @change=\"onChange\" :disabled=\"isLoading\" :label=\"label\" :options=\"options\" />"
        };
    }
    exports.createCommonEntityPicker = createCommonEntityPicker;
});
//# sourceMappingURL=controlGenerator.js.map