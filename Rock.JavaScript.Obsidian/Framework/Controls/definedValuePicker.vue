<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <BaseAsyncPicker v-model="internalValue" v-bind="standardProps" :items="itemsSource" />
</template>

<script setup lang="ts">
    import { Guid } from "@Obsidian/Types";
    import { useSecurityGrantToken } from "@Obsidian/Utility/block";
    import { standardAsyncPickerProps, useStandardAsyncPickerProps, useVModelPassthrough } from "@Obsidian/Utility/component";
    import { useHttp } from "@Obsidian/Utility/http";
    import { DefinedValuePickerGetDefinedValuesOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/definedValuePickerGetDefinedValuesOptionsBag";
    import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
    import { PropType, ref, watch } from "vue";
    import BaseAsyncPicker from "./baseAsyncPicker";
    import RockFormField from "./rockFormField";

    const props = defineProps({
        modelValue: {
            type: Object as PropType<ListItemBag | ListItemBag[] | null>,
            required: false
        },

        definedTypeGuid: {
            type: String as PropType<Guid>,
            required: false
        },

        ...standardAsyncPickerProps
    });

    const emit = defineEmits<{
        (e: "update:modelValue", _value: ListItemBag | ListItemBag[] | null): void
    }>();


    const standardProps = useStandardAsyncPickerProps(props);
    const securityGrantToken = useSecurityGrantToken();
    const http = useHttp();
    const internalValue = useVModelPassthrough(props, "modelValue", emit);
    const itemsSource = ref<(() => Promise<ListItemBag[]>) | null>(null);

    const loadItems = async (): Promise<ListItemBag[]> => {
        const options: Partial<DefinedValuePickerGetDefinedValuesOptionsBag> = {
            definedTypeGuid: props.definedTypeGuid,
            securityGrantToken: securityGrantToken.value
        };
        const url = "/api/v2/Controls/DefinedValuePickerGetDefinedValues";
        const result = await http.post<ListItemBag[]>(url, undefined, options);

        if (result.isSuccess && result.data) {
            return result.data;
        }
        else {
            console.error(result.errorMessage ?? "Unknown error while loading data.");
            return [];
        }
    };

    watch(() => props.definedTypeGuid, () => {
        // Pass as a wrapped function to ensure lazy loading works.
        itemsSource.value = () => loadItems();
    });

    itemsSource.value = () => loadItems();
</script>
