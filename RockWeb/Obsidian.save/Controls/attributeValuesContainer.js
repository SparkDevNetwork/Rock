System.register(["vue", "./rockField", "../Elements/loadingIndicator"], function (exports_1, context_1) {
    "use strict";
    var vue_1, rockField_1, loadingIndicator_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (rockField_1_1) {
                rockField_1 = rockField_1_1;
            },
            function (loadingIndicator_1_1) {
                loadingIndicator_1 = loadingIndicator_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "AttributeValuesContainer",
                components: {
                    RockField: rockField_1.default,
                    LoadingIndicator: loadingIndicator_1.default
                },
                props: {
                    isEditMode: {
                        type: Boolean,
                        default: false
                    },
                    attributeValues: {
                        type: Array,
                        required: true
                    },
                    showEmptyValues: {
                        type: Boolean,
                        default: true
                    },
                    showAbbreviatedName: {
                        type: Boolean,
                        default: false
                    }
                },
                computed: {
                    validAttributeValues() {
                        return this.attributeValues;
                    }
                },
                template: `
<suspense>
    <template v-for="a in validAttributeValues">
        <RockField
            :isEditMode="isEditMode"
            :attributeValue="a"
            :showEmptyValue="showEmptyValues"
            :showAbbreviatedName="showAbbreviatedName" />
    </template>
    <template #fallback>
        <LoadingIndicator />
    </template>
</suspense>
`
            }));
        }
    };
});
//# sourceMappingURL=attributeValuesContainer.js.map