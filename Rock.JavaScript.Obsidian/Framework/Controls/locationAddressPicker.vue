<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <ContentDropDownPicker
                           v-bind="formFieldProps"
                           :modelValue="pickerValue"
                           iconCssClass="fa fa-map-marker"
                           :showClear="!!pickerValue"
                           @primaryButtonClicked="select"
                           @secondaryButtonClicked="cancel"
                           @clearButtonClicked="clear"
                           pickerContentBoxHeight="auto"
                           disablePickerContentBoxScroll
                           disableAutoCloseOnPrimaryAction
                           v-model:showPopup="showPopup">

        <template #innerLabel>
            <span class="selected-names" v-html="pickerLabel"></span>
        </template>

        <RockValidation :errors="errors" />
        <AddressControl v-model="controlValue" rules="required" />

        <template #primaryButtonLabel>
            <Loading :isLoading="isLoading">Select</Loading>
        </template>
    </ContentDropDownPicker>
</template>

<script setup lang="ts">
    import { PropType, ref, watch } from "vue";
    import { standardRockFormFieldProps, updateRefValue, useStandardRockFormFieldProps } from "@Obsidian/Utility/component";
    import ContentDropDownPicker from "./contentDropDownPicker.vue";
    import AddressControl, { AddressControlValue } from "./addressControl";
    import { post } from "@Obsidian/Utility/http";
    import { FormError } from "@Obsidian/Utility/form";
    import RockValidation from "./rockValidation";
    import { LocationAddressPickerValidateAddressOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/locationAddressPickerValidateAddressOptionsBag";
    import { LocationAddressPickerValidateAddressResultsBag } from "@Obsidian/ViewModels/Rest/Controls/locationAddressPickerValidateAddressResultsBag";
    import Loading from "./loading";

    const props = defineProps({
        ...standardRockFormFieldProps,

        /**
         * Geographical Point or Polygon coordinates in Well Known Text format
         */
        modelValue: {
            type: Object as PropType<AddressControlValue>,
            required: true
        }
    });

    const emit = defineEmits<{
        (e: "update:modelValue", value: AddressControlValue): void
    }>();

    // #region Values

    // Address Control
    const controlValue = ref({ ...props.modelValue });

    // This Picker
    const pickerValue = ref({ ...props.modelValue });
    const pickerLabel = ref("");
    const errors = ref<FormError[]>([]);
    const isLoading = ref(false);
    const showPopup = ref(false);

    const formFieldProps = useStandardRockFormFieldProps(props);

    // #endregion

    // #region Event Handlers

    async function select(): Promise<void> {
        isLoading.value = true;
        const options: LocationAddressPickerValidateAddressOptionsBag = { ...controlValue.value };
        const response = await post<LocationAddressPickerValidateAddressResultsBag>("/api/v2/Controls/LocationAddressPickerValidateAddress", undefined, options);

        if (response.isSuccess && response.data) {
            if (response.data.isValid) {
                errors.value = [];
                pickerValue.value = { ...response.data.address } as AddressControlValue;
                controlValue.value = { ...response.data.address } as AddressControlValue;
                pickerLabel.value = response.data.addressString ?? "";
                showPopup.value = false;
            }
            else {
                errors.value = [{ name: "Invalid", text: response.data.errorMessage ?? "Please enter a valid address." }];
            }
        }
        else {
            console.error(response.errorMessage ?? "Unknown error while validating address.");
            errors.value = [{ name: "Server", text: response.errorMessage ?? "An unknown error occurred. Please try again." }];
        }
        isLoading.value = false;
    }

    function cancel(): void {
        // Reset the map values to the picker values when selection is cancelled
        controlValue.value = pickerValue.value;
    }

    function clear(): void {
        pickerValue.value = {};
        pickerLabel.value = "";
    }

    // #endregion

    // #region Watchers

    watch(() => props.modelValue, () => {
        updateRefValue(controlValue, { ...props.modelValue });
        updateRefValue(pickerValue, { ...props.modelValue });
    });

    watch(pickerValue, () => {
        emit("update:modelValue", pickerValue.value);
    });

    // #endregion
</script>
