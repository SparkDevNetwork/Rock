<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <RockFormField formGroupClasses="address-control" #default="{ uniqueId, field }" name="addresscontrol" v-model.lazy="internalValue">
        <div class="control-wrapper">
            <Loading :id="id || uniqueId" :isLoading="isLoading">
                <div class="form-row" v-if="country.isVisible">
                    <div class="form-group col-sm-6">
                        <DropDownList v-model="internalValue.country" :items="countryOptions" :validationTitle="country.label" :rules="country.rules" />
                    </div>
                </div>
                <div class="form-group" v-if="address1.isVisible">
                    <TextBox v-model="internalValue.street1" :placeholder="address1.label" :validationTitle="address1.label" :rules="address1.rules" />
                </div>
                <div class="form-group" v-if="address2.isVisible">
                    <TextBox v-model="internalValue.street2" :placeholder="address2.label" :validationTitle="address2.label" :rules="address2.rules" />
                </div>
                <div class="form-row">
                    <div class="form-group" :class="county.isVisible ? 'col-sm-3' : 'col-sm-6'" v-if="city.isVisible">
                        <TextBox v-model="internalValue.city" :placeholder="city.label" :validationTitle="city.label" :rules="city.rules" />
                    </div>
                    <div class="form-group col-sm-3" v-if="county.isVisible">
                        <TextBox v-model="internalValue.locality" :placeholder="county.label" :validationTitle="county.label" :rules="county.rules" />
                    </div>
                    <div class="form-group col-sm-3" v-if="state.isVisible">
                        <DropDownList v-if="hasStateList" :showBlankItem="false" v-model="internalValue.state" :items="stateOptions" :validationTitle="state.label" :rules="state.rules" />
                        <TextBox v-else v-model="internalValue.state" :placeholder="state.label" :validationTitle="state.label" :rules="state.rules" />
                    </div>
                    <div class="form-group col-sm-3" v-if="zip.isVisible">
                        <TextBox v-model="internalValue.postalCode" :placeholder="zip.label" :validationTitle="zip.label" :rules="zip.rules" inputmode="numeric" />
                    </div>
                </div>
            </Loading>
        </div>
    </RockFormField>
</template>

<script lang="ts" setup>
    import { PropType, reactive, ref, watch } from "vue";
    import RockFormField from "./rockFormField";
    import DropDownList from "./dropDownList";
    import TextBox from "./textBox";
    import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
    import { AddressControlBag } from "@Obsidian/ViewModels/Controls/addressControlBag";
    import { AddressControlGetConfigurationOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/addressControlGetConfigurationOptionsBag";
    import { AddressControlConfigurationBag } from "@Obsidian/ViewModels/Rest/Controls/addressControlConfigurationBag";
    import { RequirementLevel } from "@Obsidian/Enums/Controls/requirementLevel";
    import { post } from "@Obsidian/Utility/http";
    import Loading from "./loading";


    type FullAddress = Required<{
        [Property in keyof AddressControlBag]: NonNullable<AddressControlBag[Property]>
    }>;

    const props = defineProps({
        modelValue: {
            type: Object as PropType<AddressControlBag>,
            default: {}
        },

        id: {
            type: String as PropType<string>,
            default: ""
        },

        showCounty: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        showAddressLine2: {
            type: Boolean as PropType<boolean>,
            default: true
        },

        useCountryAbbreviation: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    });

    const emit = defineEmits<{
        (e: "update:modelValue", value: AddressControlBag): void
    }>();

    const internalValue = reactive<FullAddress>({
        city: props.modelValue.city ?? "",
        country: props.modelValue.country ?? "",
        postalCode: props.modelValue.postalCode ?? "",
        state: props.modelValue.state ?? "",
        street1: props.modelValue.street1 ?? "",
        street2: props.modelValue.street2 ?? "",
        locality: props.modelValue.locality ?? ""
    });

    watch(internalValue, () => {
        emit("update:modelValue", internalValue);
    });

    watch(() => props.modelValue, () => {
        // Update internalValue if there are differences
        Object.entries(props.modelValue).forEach(([key, val]) => {
            if (val === null || val === undefined) {
                internalValue[key] = "";
            }
            else if (val !== internalValue[key]) {
                internalValue[key] = val;
            }
        });
    });

    const isLoading = ref<boolean>(false);

    type FieldConfig = {
        isVisible: boolean,
        label: string,
        rules: string
    };

    const country = reactive<FieldConfig>({
        isVisible: true,
        label: "Country",
        rules: ""
    });
    const address1 = reactive<FieldConfig>({
        isVisible: true,
        label: "Address Line 1",
        rules: ""
    });
    const address2 = reactive<FieldConfig>({
        isVisible: true,
        label: "Address Line 2",
        rules: ""
    });
    const city = reactive<FieldConfig>({
        isVisible: true,
        label: "City",
        rules: ""
    });
    const county = reactive<FieldConfig>({
        isVisible: true,
        label: "County",
        rules: ""
    });
    const state = reactive<FieldConfig>({
        isVisible: true,
        label: "State",
        rules: ""
    });
    const zip = reactive<FieldConfig>({
        isVisible: true,
        label: "Zip",
        rules: ""
    });

    const countryOptions = ref<ListItemBag[]>([]);
    const stateOptions = ref<ListItemBag[]>([]);
    const hasStateList = ref<boolean>(false);

    async function getConfiguration(): Promise<void> {
        isLoading.value = true;
        const options: Partial<AddressControlGetConfigurationOptionsBag> = {
            useCountryAbbreviation: props.useCountryAbbreviation,
            countryCode: props.modelValue.country
        };
        const result = await post<AddressControlConfigurationBag>("/api/v2/Controls/AddressControlGetConfiguration", undefined, options);

        if (result.isSuccess && result.data) {
            const x = result.data;

            // Label and rules are static
            country.isVisible = x.showCountrySelection;

            // Label is static
            address1.isVisible = x.addressLine1Requirement != RequirementLevel.Unavailable;
            address1.rules = getRules(x.addressLine1Requirement);

            // Address Line 2 is only shown if it's required or it's requested by the prop; Label is static
            address2.isVisible = x.addressLine2Requirement == RequirementLevel.Required || (props.showAddressLine2 && x.addressLine2Requirement != RequirementLevel.Unavailable);
            address2.rules = getRules(x.addressLine2Requirement);

            city.isVisible = x.cityRequirement != RequirementLevel.Unavailable;
            city.rules = getRules(x.cityRequirement);
            city.label = x.cityLabel ?? city.label;

            // County / Locality is only shown if it's required or it's requested by the prop
            county.isVisible = x.localityRequirement == RequirementLevel.Required || (props.showCounty && x.localityRequirement != RequirementLevel.Unavailable);
            county.rules = getRules(x.localityRequirement);
            county.label = x.localityLabel ?? county.label;

            state.isVisible = x.stateRequirement != RequirementLevel.Unavailable;
            state.rules = getRules(x.stateRequirement);
            state.label = x.stateLabel ?? state.label;

            zip.isVisible = x.postalCodeRequirement != RequirementLevel.Unavailable;
            zip.rules = getRules(x.postalCodeRequirement);
            zip.label = x.postalCodeLabel ?? zip.label;

            countryOptions.value = x.countries ?? [];
            stateOptions.value = x.states ?? [];
            hasStateList.value = x.hasStateList;

            const countryValue = (x.selectedCountry || x.defaultCountry) ?? "";
            const stateValue = x.defaultState ?? "";

            // If we don't have a country set yet, and we have a good countryValue, set to that
            if (!internalValue.country && countryValue) {
                internalValue.country = countryValue;
            }

            // If we don't have a state set yet, and we have a good stateValue, set to that
            if (!internalValue.state && stateValue) {
                internalValue.state = stateValue;
            }
        }
        else {
            console.error(result.errorMessage ?? "Unknown error while loading data.");
        }
        isLoading.value = false;
    }

    function getRules(reqLvl: RequirementLevel): string {
        return reqLvl == RequirementLevel.Required ? "required" : "";
    }

    watch(() => internalValue.country, (currentVal, oldVal) => {
        if (currentVal != oldVal) {
            getConfiguration();
        }
    }, { deep: true });

    // Initialize with configuration from the server
    getConfiguration();

</script>