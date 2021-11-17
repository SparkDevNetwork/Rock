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
                name: "Rating",
                components: {
                    RockFormField: rockFormField_1.default
                },
                props: {
                    modelValue: {
                        type: Number,
                        default: 0
                    },
                    maxRating: {
                        type: Number,
                        default: 5
                    }
                },
                setup(props, { emit }) {
                    const internalValue = vue_1.ref(props.modelValue);
                    const hoverValue = vue_1.ref(null);
                    const showClear = vue_1.computed(() => internalValue.value > 0);
                    vue_1.watch(() => props.modelValue, () => internalValue.value = props.modelValue);
                    vue_1.watchEffect(() => emit("update:modelValue", internalValue.value));
                    const setRating = (value) => {
                        internalValue.value = value;
                    };
                    const onClear = (e) => {
                        e.preventDefault();
                        setRating(0);
                        return false;
                    };
                    const classForRating = (position) => {
                        var _a;
                        const filledCount = Math.min(props.maxRating, (_a = hoverValue.value) !== null && _a !== void 0 ? _a : internalValue.value);
                        return position <= filledCount ? "fa fa-rating-selected" : "fa fa-rating-unselected";
                    };
                    const setHover = (position) => {
                        hoverValue.value = position;
                    };
                    const clearHover = () => {
                        hoverValue.value = null;
                    };
                    return {
                        classForRating,
                        clearHover,
                        hoverValue,
                        internalValue,
                        onClear,
                        setHover,
                        setRating,
                        showClear
                    };
                },
                template: `
<RockFormField
    :modelValue="internalValue"
    formGroupClasses="rock-rating"
    name="rock-rating">
    <template #default="{uniqueId, field, errors, disabled}">
        <div class="control-wrapper">
            <div class="rating-input">
                <i v-for="i in maxRating" :key="i" :class="classForRating(i)" @click="setRating(i)" @mouseover="setHover(i)" @mouseleave="clearHover()"></i>
                <a v-if="showClear" class="clear-rating" href="#" v-on:click="onClear" @mouseover="setHover(0)" @mouseleave="clearHover()">
                    <span class="fa fa-remove"></span>
                </a>
            </div>
        </div>
    </template>
</RockFormField>
`
            }));
        }
    };
});
//# sourceMappingURL=rating.js.map