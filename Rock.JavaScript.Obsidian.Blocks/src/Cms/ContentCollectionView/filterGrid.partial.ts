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
import InlineCheckBox from "@Obsidian/Controls/inlineCheckBox";
import { FilterOptionsBag } from "@Obsidian/ViewModels/Blocks/Cms/ContentCollectionView/filterOptionsBag";
import { DragReorder, useDragReorder } from "@Obsidian/Directives/dragDrop";
import { useVModelPassthrough } from "@Obsidian/Utility/component";


export default defineComponent({
    name: "Cms.ContentCollectionView.CustomSettings.FilterGrid",

    components: {
        InlineCheckBox
    },

    directives: {
        DragReorder
    },

    props: {
        modelValue: {
            type: Array as PropType<FilterOptionsBag[]>,
            required: true
        }
    },

    emits: {
        "update:modelValue": (_value: FilterOptionsBag[]) => true,
        "edit": (_value: string) => true
    },

    setup(props, { emit }) {
        const filters = useVModelPassthrough(props, "modelValue", emit);

        const reorderDragOptions = useDragReorder(filters);

        const onEditClick = (rowName: string): void => {
            emit("edit", rowName);
        };

        return {
            filters,
            onEditClick,
            reorderDragOptions
        };
    },

    template: `
<div class="grid">
    <div class="table-no-border">
        <table class="grid-table table table-condensed table-light">
            <thead>
                <tr align="left">
                    <th class="grid-columncommand"></th>
                    <th>Show</th>
                    <th>Filter</th>
                    <th>Filter Header Markup</th>
                    <th class="grid-columncommand"></th>
                </tr>
            </thead>

            <tbody v-drag-reorder="reorderDragOptions">
                <tr v-for="row in filters" :key="row.name" align="left">
                    <td class="grid-columnreorder" align="center">
                        <a class="minimal reorder-handle" href="#">
                            <i class="fa fa-bars"></i>
                        </a>
                    </td>
                    
                    <td class="grid-select-field" align="center">
                        <InlineCheckBox v-model="row.show" />
                    </td>
                    <td>{{ row.name }}</td>
                    
                    <td class="grid-bool-field" align="center">
                        <i v-if="row.headerMarkup" class="fa fa-check"></i>
                    </td>
                    
                    <td class="grid-columncommand" align="center">
                        <a class="btn btn-default btn-sm" href="#" @click.prevent="onEditClick(row.name)">
                            <i class="fa fa-pencil"></i>
                        </a>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</div>
`
});
