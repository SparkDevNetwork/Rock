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

import { computed, defineComponent } from "vue";
import { getFieldEditorProps } from "./utils";
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { ConfigurationKey } from "./binaryFileField.partial";
import { safeParseJson } from "@Obsidian/Utility/stringUtils";
import { ConfigurationComponent } from "./fileFieldComponents";

export const EditComponent = defineComponent({
    name: "BinaryFileField.Edit",

    components: {
        DropDownList
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const binaryFileOptions = computed((): ListItemBag[] => {
            return safeParseJson<ListItemBag[]>(props.configurationValues[ConfigurationKey.BinaryFileOptions] || "[]") ?? [];
        });

        const internalValue = computed({
            get() {
                return safeParseJson<ListItemBag>(props.modelValue || "{}")?.value;
            },
            set(value) {
                const itemBag = binaryFileOptions.value.find(o => o.value === value);
                emit("update:modelValue", JSON.stringify(itemBag));
            }
        });

        return {
            internalValue,
            binaryFileOptions
        };
    },

    template: `
<DropDownList v-model="internalValue" :items="binaryFileOptions" showBlankItem />
`
});

export { ConfigurationComponent };
