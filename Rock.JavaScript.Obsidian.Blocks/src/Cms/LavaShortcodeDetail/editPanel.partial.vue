<!-- Copyright by the Spark Development Network; Licensed under the Rock Community License -->
<template>
    <fieldset>
        <div class="row">
            <div class="col-md-6">
                <TextBox v-model="name"
                         label="Name"
                         rules="required" />
            </div>

            <div class="col-md-6">
                <CheckBox v-model="isActive"
                          label="Active" />
            </div>
        </div>

        <TextBox v-model="description"
                 label="Description"
                 textMode="multiline" />

        <RadioButtonList v-model="tagType"
                         label="Tag Type"
                        :horizontal="true"
                         :items="tagTypes" />
<!-- horizontal="true" -->
        <CodeEditor
                    v-model="markup"
                    theme="rock"
                    mode="text"
                    :editorHeight="200" />

        <CategoryPicker label="Categories"
                        v-model="categories"
                        :multiple="true"
                         />
                        <!-- :entityTypeGuid="entityTypeGuid" /> -->

        <AttributeValuesContainer v-model="attributeValues" :attributes="attributes" isEditMode :numberOfColumns="2" />
    </fieldset>
</template>

<script setup lang="ts">
    import { PropType, ref, watch } from "vue";
    import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer";
    import CheckBox from "@Obsidian/Controls/checkBox";
    import CategoryPicker from "@Obsidian/Controls/categoryPicker";
    import TextBox from "@Obsidian/Controls/textBox";
    import CodeEditor from "@Obsidian/Controls/codeEditor";
    import RadioButtonList from "@Obsidian/Controls/radioButtonList";
    import { watchPropertyChanges } from "@Obsidian/Utility/block";
    import { propertyRef, updateRefValue } from "@Obsidian/Utility/component";
    import KeyValueList, { KeyValueItem } from "@Obsidian/Controls/keyValueList";
    import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
    ///import { TagType } from "@Obsidian/Enums/Cms/tagType";
    import { LavaShortcodeBag } from "@Obsidian/ViewModels/Blocks/Cms/LavaShortcodeDetail/lavaShortcodeBag";
    import { LavaShortcodeDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Cms/LavaShortcodeDetail/lavaShortcodeDetailOptionsBag";
    //import { Category } from "../../../../Rock.JavaScript.Obsidian/Framework/SystemGuids";

    const props = defineProps({
        modelValue: {
            type: Object as PropType<LavaShortcodeBag>,
            required: true
        },

        options: {
            type: Object as PropType<LavaShortcodeDetailOptionsBag>,
            required: true
        }
    });

    const emit = defineEmits<{
        (e: "update:modelValue", value: LavaShortcodeBag): void,
        (e: "propertyChanged", value: string): void
    }>();

    // #region Values

    const attributes = ref(props.modelValue.attributes ?? {});
    const attributeValues = ref(props.modelValue.attributeValues ?? {});
    const description = propertyRef(props.modelValue.description ?? "", "Description");
    const isActive = propertyRef(props.modelValue.isActive ?? false, "IsActive");
    const name = propertyRef(props.modelValue.name ?? "", "Name");
    const tagName = propertyRef(props.modelValue.tagName ?? "", "TagName");
    const tagType = propertyRef(props.modelValue.tagType, "TagType");
    const tagTypes = ref(props.modelValue.tagTypes ?? [] );
    const markup = propertyRef(props.modelValue.markup ?? "", "Markup");
    const categories = propertyRef((props.modelValue.categories ?? []).map((s): KeyValueItem => ({ key: s.value, value: s.text })), "Categories");
    const tagTypeItems: ListItemBag[] = [
        {
            value: "1",
            text: "Inline"
        },
        {
            value: "2",
            text: "Block"
        }
    ];
    // The properties that are being edited. This should only contain
    // objects returned by propertyRef().
    const propRefs = [description, isActive, name, categories, tagName, tagType, markup];

    // #endregion

    // #region Computed Values

    // #endregion

    // #region Functions

    // #endregion

    // #region Event Handlers

    // #endregion

    // Watch for parental changes in our model value and update all our values.
    watch(() => props.modelValue, () => {
        updateRefValue(attributes, props.modelValue.attributes ?? {});
        updateRefValue(attributeValues, props.modelValue.attributeValues ?? {});
        updateRefValue(description, props.modelValue.description ?? "");
        updateRefValue(isActive, props.modelValue.isActive ?? false);
        updateRefValue(name, props.modelValue.name ?? "");
        updateRefValue(categories, (props.modelValue.categories ?? []).map((c): KeyValueItem => ({ key: c.value, value: c.text })));
        updateRefValue(tagName, props.modelValue.tagName ?? "");
        updateRefValue(tagType, props.modelValue.tagType);
        updateRefValue(markup, props.modelValue.markup ?? "");
    });

    // Determines which values we want to track changes on (defined in the
    // array) and then emit a new object defined as newValue.
    watch([attributeValues, ...propRefs], () => {
        const newValue: LavaShortcodeBag = {
            ...props.modelValue,
            attributeValues: attributeValues.value,
            description: description.value,
            isActive: isActive.value,
            name: name.value,
            categories: categories.value.map((c): ListItemBag => ({ value: c.key ?? "", text: c.value ?? "" })),
            tagName: tagName.value,
            tagType: tagType.value,
            markup: markup.value
        };

        emit("update:modelValue", newValue);
    });

    // Watch for any changes to props that represent properties and then
    // automatically emit which property changed.
    watchPropertyChanges(propRefs, emit);
</script>
