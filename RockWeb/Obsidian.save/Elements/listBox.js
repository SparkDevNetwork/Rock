System.register(["vue", "../Util/guid", "./rockFormField"], function (exports_1, context_1) {
    "use strict";
    var vue_1, guid_1, rockFormField_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (guid_1_1) {
                guid_1 = guid_1_1;
            },
            function (rockFormField_1_1) {
                rockFormField_1 = rockFormField_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "ListBox",
                components: {
                    RockFormField: rockFormField_1.default
                },
                props: {
                    modelValue: {
                        type: Array,
                        default: []
                    },
                    options: {
                        type: Array,
                        required: true
                    },
                    formControlClasses: {
                        type: String,
                        default: ""
                    },
                    enhanceForLongLists: {
                        type: Boolean,
                        default: false
                    }
                },
                data: function () {
                    return {
                        uniqueId: `rock-listbox-${guid_1.newGuid()}`,
                        internalValue: [],
                        isMounted: false
                    };
                },
                computed: {
                    compiledFormControlClasses() {
                        if (this.enhanceForLongLists) {
                            return this.formControlClasses + " chosen-select";
                        }
                        return this.formControlClasses;
                    }
                },
                methods: {
                    getChosenJqueryEl() {
                        const jquery = window["$"];
                        let $chosenDropDown = jquery(this.$refs["theSelect"]);
                        if (!$chosenDropDown || !$chosenDropDown.length) {
                            $chosenDropDown = jquery(`#${this.uniqueId}`);
                        }
                        return $chosenDropDown;
                    },
                    createOrDestroyChosen() {
                        if (!this.isMounted) {
                            return;
                        }
                        const $chosenDropDown = this.getChosenJqueryEl();
                        if (this.enhanceForLongLists) {
                            $chosenDropDown
                                .chosen({
                                width: "100%",
                                placeholder_text_multiple: " ",
                                placeholder_text_single: " "
                            })
                                .change(() => {
                                this.internalValue = $chosenDropDown.val();
                            });
                        }
                        else {
                            $chosenDropDown.chosen("destroy");
                        }
                    },
                    syncValue() {
                        if (this.internalValue.length === this.modelValue.length && this.internalValue.every((v, i) => v === this.modelValue[i])) {
                            return;
                        }
                        this.internalValue = this.modelValue;
                        if (this.enhanceForLongLists) {
                            this.$nextTick(() => {
                                const $chosenDropDown = this.getChosenJqueryEl();
                                $chosenDropDown.trigger("chosen:updated");
                            });
                        }
                    }
                },
                watch: {
                    modelValue: {
                        immediate: true,
                        handler() {
                            this.syncValue();
                        }
                    },
                    options: {
                        immediate: true,
                        handler() {
                            this.syncValue();
                        }
                    },
                    internalValue() {
                        this.$emit("update:modelValue", this.internalValue);
                    },
                    enhanceForLongLists() {
                        this.createOrDestroyChosen();
                    }
                },
                mounted() {
                    this.isMounted = true;
                    this.createOrDestroyChosen();
                },
                template: `
<RockFormField
    :modelValue="internalValue"
    formGroupClasses="rock-drop-down-list"
    name="dropdownlist">
    <template #default="{uniqueId, field, errors, disabled}">
        <div class="control-wrapper">
            <select :id="uniqueId" class="form-control" :class="compiledFormControlClasses" :disabled="disabled" v-bind="field" v-model="internalValue" ref="theSelect" multiple>
                <option v-if="showBlankItem" :value="blankValue"></option>
                <option v-for="o in options" :key="o.value" :value="o.value">{{o.text}}</option>
            </select>
        </div>
    </template>
</RockFormField>
`
            }));
        }
    };
});
//# sourceMappingURL=listBox.js.map