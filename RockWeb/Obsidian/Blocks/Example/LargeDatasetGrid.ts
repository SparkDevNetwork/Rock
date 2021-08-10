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
import PaneledBlockTemplate from '../../Templates/PaneledBlockTemplate';
import { defineComponent } from 'vue';
import GridRow from '../../Controls/GridRow';
import GridColumn from '../../Controls/GridColumn';
import GridProfileLinkColumn from '../../Controls/GridProfileLinkColumn';
import BlockActionSourcedGrid from '../../Controls/BlockActionSourcedGrid';
import Dialog from '../../Controls/Dialog';
import { RowContext } from '../../Controls/Grid';

export default defineComponent({
    name: 'Example.LargeDatasetGrid',
    components: {
        PaneledBlockTemplate,
        BlockActionSourcedGrid,
        GridColumn,
        GridRow,
        GridProfileLinkColumn,
        Dialog
    },
    data ()
    {
        return {
            rowContextClicked: null as RowContext | null,
            isRowClickedDialogOpen: false
        };
    },
    methods: {
        onRowClick ( rowContext ): void
        {
            this.rowContextClicked = rowContext;
            this.isRowClickedDialogOpen = true;
        }
    },
    template: `
<PaneledBlockTemplate>
    <template #title>
        <i class="fa fa-dumbbell"></i>
        Large Dataset Grid
    </template>
    <template #default>
        <div class="grid grid-panel">
            <BlockActionSourcedGrid blockActionName="GetAttributeValues" #default="rowContext" rowItemText="Attribute Values" rowIdKey="Id">
                <GridRow :rowContext="rowContext" @click:body="onRowClick">
                    <GridColumn title="Id" property="Id" sortExpression="Id" />
                    <GridColumn title="Guid" property="Guid" sortExpression="Guid" />
                    <GridColumn title="Attribute" property="Attribute" sortExpression="Attribute.Id" />
                    <GridColumn title="Value" property="Value" sortExpression="Value" />
                </GridRow>
            </BlockActionSourcedGrid>
        </div>
        <Dialog v-model="isRowClickedDialogOpen">
            <template #header>
                <h3>Row Clicked</h3>
            </template>
            <template #default>
                <pre>{{ JSON.stringify( rowContextClicked, null, 2 ) }}</pre>
            </template>
        </Dialog>
    </template>
</PaneledBlockTemplate>`
});
