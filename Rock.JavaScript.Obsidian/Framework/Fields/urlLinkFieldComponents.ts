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
import { defineComponent, computed } from "vue";
import { getFieldEditorProps } from "./utils";
import { useVModelPassthrough } from "../Util/component";
import UrlLinkBox from "../Elements/urlLinkBox";
import { asBooleanOrNull } from "../Services/boolean"

export const EditComponent = defineComponent({
    name: "UrlLinkField.Edit",

    components: {
        UrlLinkBox
    },

    props: getFieldEditorProps(),

    emits: [
        "update:modelValue"
    ],

    setup(props, { emit }) {
        let value = useVModelPassthrough(props, "modelValue", emit);

        let requiresTrailingSlash = computed(() => asBooleanOrNull(props.configurationValues.ShouldRequireTrailingForwardSlash) ?? false)

        return { value, requiresTrailingSlash };
    },
    template: `
<UrlLinkBox v-model="value" :requires-trailing-slash="requiresTrailingSlash" />
`
});
