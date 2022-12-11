<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <div class="js-defined-value-editor well" style="">
        <div>
            <div id=" " class="alert alert-validation" style="display:none;">

            </div>
        </div>
        <fieldset>
            <div class="form-group data-text-box  required">
                <label class="control-label" for="">Value</label>
                <div class="control-wrapper">
                    <input name="" type="text" maxlength="250" class="form-control" placeholder="Value"><span class="validation-error help-inline" style="display:none;"></span>
                </div><span class="validation-error help-inline" style="display:none;">Value is required.</span>
            </div>
            <div class="form-group data-text-box ">
                <label class="control-label" for=""> </label>
                <div class="control-wrapper">
                    <input type="hidden" name="" value="True">
                    <textarea name="" rows="3" cols="20" class="form-control" placeholder="Description"></textarea>
                    <span class="validation-error help-inline" style="display:none;"></span>
                </div>
            </div>
            <div class="attributes">

            </div>
        </fieldset>
        <div class="row">
            <div class="col-md-12">
                <a class="btn btn-primary btn-xs" href="javascript:WebForm_DoPostBackWithOptions(new WebForm_PostBackOptions(&quot;&quot;, &quot;&quot;, true, &quot;&quot;, &quot;&quot;, false, true))">Add</a>
                <a onclick="javascript:$('.-js-defined-value-editor').fadeToggle(400, 'swing', function() {
                $('.-js-defined-value-selector').fadeToggle();
                }); return false;" class="btn btn-link btn-xs" href="javascript:__doPostBack('','')">Cancel</a>
            </div>
        </div>
    </div>
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
