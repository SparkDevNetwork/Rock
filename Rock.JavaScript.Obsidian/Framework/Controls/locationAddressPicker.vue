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
                           disablePickerContentBoxScroll>

        <template #innerLabel>
            <span class="selected-names" v-html="pickerLabel"></span>
        </template>

        <AddressControl v-model="controlValue" />
    </ContentDropDownPicker>
</template>

<script setup lang="ts">
    import { PropType, ref, watch } from "vue";
    import { standardRockFormFieldProps, updateRefValue, useStandardRockFormFieldProps } from "@Obsidian/Utility/component";
    import ContentDropDownPicker from "@Obsidian/Controls/contentDropDownPicker.vue";
    import AddressControl, { getDefaultAddressControlModel, AddressControlValue } from "./addressControl";

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

    const controlValue = ref({ ...props.modelValue });

    const pickerValue = ref({ ...props.modelValue });
    const pickerLabel = ref("");

    const formFieldProps = useStandardRockFormFieldProps(props);

    // #endregion

    // #region Functions

    // #endregion

    // #region Event Handlers

    function select(): void {
        const address = { ...controlValue.value };
        pickerValue.value = address;
        pickerLabel.value = `${address.street1} ${address.street2}<br>${address.city}, ${address.state} ${address.postalCode}`;
    }

    function cancel(): void {
        // Reset the map values to the picker values when selection is cancelled
        controlValue.value = pickerValue.value;
    }

    function clear(): void {
        pickerValue.value = getDefaultAddressControlModel();
        pickerLabel.value = "";
    }

    // #endregion

    // #region Watchers

    watch(() => props.modelValue, () => {
        console.log("UPDATE MODEL VALUE");
        updateRefValue(controlValue, { ...props.modelValue });
        updateRefValue(pickerValue, { ...props.modelValue });
    });

    watch(pickerValue, () => {
        console.log("UPDATE PICKER VALUE");
        emit("update:modelValue", pickerValue.value);
    });

    // #endregion
</script>
