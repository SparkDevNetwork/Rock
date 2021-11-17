System.register(["vue", "./rockFormField"], function (exports_1, context_1) {
    "use strict";
    var vue_1, rockFormField_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (rockFormField_1_1) {
                rockFormField_1 = rockFormField_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "RadioButtonList",
                components: {
                    RockFormField: rockFormField_1.default
                },
                props: {
                    options: {
                        type: Array,
                        default: []
                    },
                    modelValue: {
                        type: String,
                        default: ""
                    },
                    repeatColumns: {
                        type: Number,
                        default: 0
                    },
                    horizontal: {
                        type: Boolean,
                        default: false
                    }
                },
                emits: [
                    "update:modelValue"
                ],
                data() {
                    return {
                        internalValue: ""
                    };
                },
                computed: {
                    containerClasses() {
                        const classes = [];
                        if (this.repeatColumns > 0) {
                            classes.push(`in-columns in-columns-${this.repeatColumns}`);
                        }
                        if (this.horizontal) {
                            classes.push("rockradiobuttonlist-horizontal");
                        }
                        else {
                            classes.push("rockradiobuttonlist-vertical");
                        }
                        return classes.join(" ");
                    }
                },
                methods: {
                    getOptionUniqueId(uniqueId, option) {
                        const key = option.value.replace(" ", "-");
                        return `${uniqueId}-${key}`;
                    }
                },
                watch: {
                    internalValue() {
                        this.$emit("update:modelValue", this.internalValue);
                    },
                    modelValue: {
                        immediate: true,
                        handler() {
                            this.internalValue = this.modelValue;
                        }
                    }
                },
                template: `
<RockFormField formGroupClasses="rock-radio-button-list" #default="{uniqueId}" name="radiobuttonlist" v-model="internalValue">
    <div class="control-wrapper">
        <div class="controls rockradiobuttonlist" :class="containerClasses">
            <span>
                <template v-if="horizontal">
                    <label v-for="option in options" class="radio-inline" :for="getOptionUniqueId(uniqueId, option)">
                        <input :id="getOptionUniqueId(uniqueId, option)" :name="uniqueId" type="radio" :value="option.value" v-model="internalValue" />
                        <span class="label-text">{{option.text}}</span>
                    </label>
                </template>
                <template v-else>
                    <div v-for="option in options" class="radio">
                        <label :for="getOptionUniqueId(uniqueId, option)">
                            <input :id="getOptionUniqueId(uniqueId, option)" :name="uniqueId" type="radio" :value="option.value" v-model="internalValue" />
                            <span class="label-text">{{option.text}}</span>
                        </label>
                    </div>
                </template>
            </span>
        </div>
    </div>
</RockFormField>`
            }));
        }
    };
});
//# sourceMappingURL=radioButtonList.js.map