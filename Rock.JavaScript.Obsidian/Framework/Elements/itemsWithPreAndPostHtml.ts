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
import { defineComponent, PropType } from "vue";

export type ItemWithPreAndPostHtml = {
    slotName: string;
    preHtml: string;
    postHtml: string;
};

export default defineComponent({
    name: "ItemsWithPreAndPostHtml",
    props: {
        items: {
            type: Array as PropType<ItemWithPreAndPostHtml[]>,
            required: true
        }
    },
    methods: {
        onDismiss: function () {
            this.$emit("dismiss");
        }
    },
    computed: {
        augmentedItems(): Record<string, string>[] {
            return this.items.map(i => ({
                ...i,
                innerSlotName: `inner-${i.slotName}`
            } as Record<string, string>));
        },
        innerTemplate(): string {
            if ( !this.items.length ) {
                return "<slot />";
            }

            const templateParts = this.items.map(i => `${i.preHtml}<slot name="inner-${i.slotName}" />${i.postHtml}`);
            return templateParts.join("");
        },
        innerComponent(): Record<string, unknown> {
            return {
                name: "InnerItemsWithPreAndPostHtml",
                template: this.innerTemplate
            };
        }
    },
    template: `
<component :is="innerComponent">
    <template v-for="item in augmentedItems" :key="item.slotName" v-slot:[item.innerSlotName]>
        <slot :name="item.slotName" />
    </template>
</component>`
});
