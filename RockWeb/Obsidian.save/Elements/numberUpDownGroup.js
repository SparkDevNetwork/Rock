System.register(["vue", "./numberUpDown", "./rockFormField"], function (exports_1, context_1) {
    "use strict";
    var vue_1, numberUpDown_1, rockFormField_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (numberUpDown_1_1) {
                numberUpDown_1 = numberUpDown_1_1;
            },
            function (rockFormField_1_1) {
                rockFormField_1 = rockFormField_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "NumberUpDownGroup",
                components: {
                    RockFormField: rockFormField_1.default,
                    NumberUpDownInternal: numberUpDown_1.NumberUpDownInternal
                },
                props: {
                    modelValue: {
                        type: Object,
                        required: true
                    },
                    options: {
                        type: Array,
                        required: true
                    }
                },
                computed: {
                    total() {
                        let total = 0;
                        for (const option of this.options) {
                            total += (this.modelValue[option.key] || 0);
                        }
                        return total;
                    }
                },
                template: `
<RockFormField
    :modelValue="total"
    formGroupClasses="margin-b-md number-up-down-group"
    name="numberupdowngroup">
    <template #default="{uniqueId, field, errors, disabled}">
        <div class="control-wrapper">
            <div v-for="option in options" :key="option.key" class="margin-l-sm margin-b-sm">
                <div v-if="option.label" class="margin-b-sm">
                    {{option.label}}
                </div>
                <NumberUpDownInternal v-model="modelValue[option.key]" :min="option.min" :max="option.max" class="margin-t-sm" />
            </div>
        </div>
    </template>
</RockFormField>`
            }));
        }
    };
});
//# sourceMappingURL=numberUpDownGroup.js.map