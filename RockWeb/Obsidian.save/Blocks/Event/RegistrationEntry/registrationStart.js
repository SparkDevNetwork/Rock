System.register(["vue", "../../../Controls/attributeValuesContainer", "../../../Controls/rockForm", "../../../Elements/rockButton"], function (exports_1, context_1) {
    "use strict";
    var vue_1, attributeValuesContainer_1, rockForm_1, rockButton_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (attributeValuesContainer_1_1) {
                attributeValuesContainer_1 = attributeValuesContainer_1_1;
            },
            function (rockForm_1_1) {
                rockForm_1 = rockForm_1_1;
            },
            function (rockButton_1_1) {
                rockButton_1 = rockButton_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "Event.RegistrationEntry.RegistrationStart",
                components: {
                    RockButton: rockButton_1.default,
                    AttributeValuesContainer: attributeValuesContainer_1.default,
                    RockForm: rockForm_1.default
                },
                setup() {
                    return {
                        registrationEntryState: vue_1.inject("registrationEntryState")
                    };
                },
                data() {
                    return {
                        attributeValues: []
                    };
                },
                computed: {
                    showPrevious() {
                        return this.registrationEntryState.firstStep === this.registrationEntryState.steps.intro;
                    }
                },
                methods: {
                    onPrevious() {
                        this.$emit("previous");
                    },
                    onNext() {
                        this.$emit("next");
                    }
                },
                watch: {
                    viewModel: {
                        immediate: true,
                        handler() {
                            this.attributeValues = this.registrationEntryState.viewModel.registrationAttributesStart.map(a => {
                                const currentValue = this.registrationEntryState.registrationFieldValues[a.attributeGuid] || "";
                                return Object.assign(Object.assign({}, a), { value: currentValue });
                            });
                        }
                    },
                    attributeValues: {
                        immediate: true,
                        deep: true,
                        handler() {
                            for (const attributeValue of this.attributeValues) {
                                this.registrationEntryState.registrationFieldValues[attributeValue.attributeGuid] = attributeValue.value;
                            }
                        }
                    }
                },
                template: `
<div class="registrationentry-registration-attributes">
    <RockForm @submit="onNext">
        <AttributeValuesContainer :attributeValues="attributeValues" isEditMode />

        <div class="actions row">
            <div class="col-xs-6">
                <RockButton v-if="showPrevious" btnType="default" @click="onPrevious">
                    Previous
                </RockButton>
            </div>
            <div class="col-xs-6 text-right">
                <RockButton btnType="primary" type="submit">
                    Next
                </RockButton>
            </div>
        </div>
    </RockForm>
</div>`
            }));
        }
    };
});
//# sourceMappingURL=registrationStart.js.map