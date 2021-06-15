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
import { defineComponent, inject } from 'vue';
import { InvokeBlockActionFunc } from '../../../Controls/RockBlock';
import Alert from '../../../Elements/Alert';
import PaneledBlockTemplate from '../../../Templates/PaneledBlockTemplate';
import PluginWidget from './PluginWidget';
import Grid from '../../../Controls/Grid';
import GridRow from '../../../Controls/GridRow';
import GridColumn from '../../../Controls/GridColumn';

export default defineComponent({
    name: 'com_rocksolidchurchdemo.PageDebug.WidgetsList',
    components: {
        PaneledBlockTemplate,
        Alert,
        Grid,
        GridRow,
        GridColumn
    },
    setup() {
        return {
            invokeBlockAction: inject('invokeBlockAction') as InvokeBlockActionFunc
        };
    },
    data() {
        return {
            isLoading: false,
            errorMessage: '',
            widgets: [] as PluginWidget[]
        };
    },
    methods: {
        async fetchWidgets(): Promise<void> {
            if (this.isLoading) {
                return;
            }

            this.isLoading = true;
            this.errorMessage = '';

            try {
                const result = await this.invokeBlockAction<PluginWidget[]>('GetWidgets');

                if (result.data) {
                    this.widgets = result.data;
                }
                else {
                    this.widgets = [];
                }
            }
            catch (e) {
                this.errorMessage = `An exception occurred: ${e}`;
            }
            finally {
                this.isLoading = false;
            }
        }
    },
    async mounted(): Promise<void> {
        await this.fetchWidgets();
    },
    template: `
<PaneledBlockTemplate>
    <template #title>
        <i class="fa fa-fan"></i>
        Plugin Widgets
    </template>
    <template #default>
        <Alert v-if="errorMessage" alertType="danger">
            {{errorMessage}}
        </Alert>
        <div class="grid grid-panel">
            <Grid :gridData="widgets" rowIdKey="Guid" #default="rowContext" rowItemText="Widget">
                <GridRow :rowContext="rowContext">
                    <GridColumn title="The String" property="ThisIsTheString" />
                    <GridColumn title="The Int" property="ThisIsTheInt" />
                </GridRow>
            </Grid>
        </div>
    </template>
</PaneledBlockTemplate>`
});