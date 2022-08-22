// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

import { computed, defineComponent, PropType, ref, watch } from "vue";
import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer";
import DropDownList from "@Obsidian/Controls/dropDownList";
import TextBox from "@Obsidian/Controls/textBox";
import { watchPropertyChanges } from "@Obsidian/Utility/block";
import { propertyRef, updateRefValue } from "@Obsidian/Utility/component";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { PageShortLinkBag } from "@Obsidian/ViewModels/Blocks/Cms/PageShortLinkDetail/pageShortLinkBag";
import { PageShortLinkDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Cms/PageShortLinkDetail/pageShortLinkDetailOptionsBag";

export default defineComponent({
    name: "Cms.PageShortLinkDetail.EditPanel",

    props: {
        modelValue: {
            type: Object as PropType<PageShortLinkBag>,
            required: true
        },

        options: {
            type: Object as PropType<PageShortLinkDetailOptionsBag>,
            required: true
        }
    },

    components: {
        AttributeValuesContainer,
        DropDownList,
        TextBox,
    },

    emits: {
        "update:modelValue": (_value: PageShortLinkBag) => true,
        "propertyChanged": (_value: string) => true
    },

    setup(props, { emit }) {
        // #region Values

        const attributes = ref(props.modelValue.attributes ?? {});
        const attributeValues = ref(props.modelValue.attributeValues ?? {});
        const site = propertyRef(props?.modelValue?.site?.value ?? "", "SiteId");
        const url = propertyRef(props.modelValue.url ?? "", "Url");
        const token = propertyRef(props.modelValue.token ?? "", "Token");

        // The properties that are being edited. This should only contain
        // objects returned by propertyRef().
        const propRefs = [url,
            token,
            site];

        // #endregion

        // #region Computed Values

        const siteOptions = computed((): ListItemBag[] => {
            return props.options.siteOptions ?? [];
        });

        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        // #endregion

        // Watch for parental changes in our model value and update all our values.
        watch(() => props.modelValue, () => {
            updateRefValue(attributes, props.modelValue.attributes ?? {});
            updateRefValue(attributeValues, props.modelValue.attributeValues ?? {});
            updateRefValue(site, props.modelValue.site?.value ?? "");
            updateRefValue(url, props.modelValue.url ?? "");
            updateRefValue(token, props.modelValue.token ?? "");
        });

        // Determines which values we want to track changes on (defined in the
        // array) and then emit a new object defined as newValue.
        watch([attributeValues, ...propRefs], () => {
            const newValue: PageShortLinkBag = {
                ...props.modelValue,
                attributeValues: attributeValues.value,
                url: url.value,
                token: token.value,
                site: { value: site.value }
            };

            emit("update:modelValue", newValue);
        });

        // Watch for any changes to props that represent properties and then
        // automatically emit which property changed.
        watchPropertyChanges(propRefs, emit);

        return {
            attributes,
            attributeValues,
            site,
            url,
            token,
            siteOptions,
        };
    },

    template: `
<fieldset>
    <div class="row">
            <div class="col-md-6">
                <DropDownList v-model="site"
                    label="Site"
                    rules="required"
                    help="The site to use for the short link."
                    :items="siteOptions" />
            </div>
            <div class="col-md-6">
                <TextBox v-model="token"
                    label="Token"
                    rules="required"
                    help="The token to use for the short link. Must be unique." />
            </div>
    </div>
    <TextBox v-model="url"
        label="URL"
        rules="required"
        help="The URL that short link will direct users to." />
    <AttributeValuesContainer v-model="attributeValues" :attributes="attributes" isEditMode :numberOfColumns="2" />
</fieldset>
`
});
