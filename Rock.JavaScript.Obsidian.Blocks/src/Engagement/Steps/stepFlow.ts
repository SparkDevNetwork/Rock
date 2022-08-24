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

import { useConfigurationValues, useInvokeBlockAction } from "@Obsidian/Utility/block";
import { defineComponent, ref } from "vue";
import Alert from "@Obsidian/Controls/alert";
import RockButton from "@Obsidian/Controls/rockButton";
import Block from "@Obsidian/Templates/block";
import SectionHeader from "@Obsidian/Controls/sectionHeader";
import RockForm from "@Obsidian/Controls/rockForm";
import SlidingDateRangePicker from "@Obsidian/Controls/slidingDateRangePicker";
import { SlidingDateRange } from "@Obsidian/Utility/slidingDateRange";
import NumberBox from "@Obsidian/Controls/numberBox";
import DropDownList from "@Obsidian/Controls/dropDownList";
import FlowNodeDiagram from "./StepFlow/flowNodeDiagram.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { emptyGuid } from "@Obsidian/Utility/guid";
import { FlowNodeDiagramNodeBag } from "@Obsidian/ViewModels/Blocks/Engagement/Steps/flowNodeDiagramNodeBag";
import { FlowNodeDiagramEdgeBag } from "@Obsidian/ViewModels/Blocks/Engagement/Steps/flowNodeDiagramEdgeBag";
import { FlowNodeDiagramSettingsBag } from "@Obsidian/ViewModels/Blocks/Engagement/Steps/flowNodeDiagramSettingsBag";
import { StepFlowInitializationBox } from "@Obsidian/ViewModels/Blocks/Engagement/Steps/stepFlowInitializationBox";
import { StepFlowGetDataBag } from "@Obsidian/ViewModels/Blocks/Engagement/Steps/stepFlowGetDataBag";
import { syncRefsWithQueryParams } from "@Obsidian/Utility/url";


/**
 * Step Flow
 * Domain: Steps
 *
 * Show the flow of individuals as they move through the four step types in the Discipleship Path program.
 *
 */
export default defineComponent({
    /**
     * This is the name that will appear in the browser debug tools. This is mostly for organization and
     * doesn't affect function.
     */
    name: "Steps.StepFlow",

    /** These are the child components that are used by this block component */
    components: {
        Block,
        Alert,
        RockButton,
        SectionHeader,
        RockForm,
        SlidingDateRangePicker,
        NumberBox,
        DropDownList,
        FlowNodeDiagram,
    },

    /** This allows for standard block tools, such as invokeBlockAction, to be available to this block */
    setup() {
        // #region Variables

        const invokeBlockAction = useInvokeBlockAction();
        const configurationValues = useConfigurationValues<StepFlowInitializationBox>();

        const flowNodes = ref<FlowNodeDiagramNodeBag[]>([]);

        const flowEdges = ref<FlowNodeDiagramEdgeBag[]>([]);

        const isLoading = ref(false);

        const dateRange = ref<SlidingDateRange | null>(null);
        const maxLevels = ref(4);
        const campus = ref(emptyGuid);

        syncRefsWithQueryParams({ dateRange, maxLevels, campus });

        const campusOptions = ref<ListItemBag[]>([
            {
                value: emptyGuid,
                text: "All Campuses",
                category: null
            },
            ...(configurationValues.campuses ?? [])
        ]);

        const settings = ref<FlowNodeDiagramSettingsBag>({
            nodeWidth: configurationValues.nodeWidth,
            nodeVerticalSpacing: configurationValues.nodeVerticalSpacing,
            chartWidth: configurationValues.chartWidth,
            chartHeight: configurationValues.chartHeight
        });

        // #endregion

        // #region Event Handlers

        async function fetchData(): Promise<void | boolean> {
            if (isLoading.value) {
                // Still Loading Previous Request. Don't want to start another one.
                return;
            }

            isLoading.value = true;

            // If 1900-01-01 is passed to `new Date`, it'll parse it as GMT (if single-digit month starting with a 0).
            // If you change the separators to / instead, it'll be parsed in the current client's time zone, which is
            // preferable, so we convert the dates here. Then we convert them to ISO Strings for the server.
            const startDateString = (dateRange.value?.lowerDate || "").replace(/-/g, "/");
            const startDate = startDateString.length > 0 ? new Date(startDateString).toISOString() : undefined;
            const endDateString = (dateRange.value?.upperDate || "").replace(/-/g, "/");
            const endDate = endDateString.length > 0 ? new Date(endDateString).toISOString() : undefined;

            // Use a copy of the current dateRange or a default if unset
            const dateRangeParam: SlidingDateRange = dateRange.value ? { ...(dateRange.value) } : { rangeType: -1 };

            dateRangeParam.lowerDate = startDate;
            dateRangeParam.upperDate = endDate;

            const response = await invokeBlockAction<StepFlowGetDataBag>("GetData", {
                dateRange: dateRangeParam,
                maxLevels: maxLevels.value,
                campus: campus.value,
            });

            isLoading.value = false;

            if (response.data) {
                flowEdges.value = response.data.edges ?? [];
                flowNodes.value = response.data.nodes ?? [];
            }
            else {
                throw new Error(response.errorMessage || "An error occurred");
            }
        }

        // #endregion

        // Fetch the data on load with the defaults
        fetchData();

        return {
            flowNodes,
            flowEdges,
            isLoading,
            dateRange,
            maxLevels,
            campus,
            campusOptions,
            settings,
            configurationValues,
            fetchData
        };
    },

    // #region Template
    template: `
<Block title="Step Flow">
    <template v-if="configurationValues.programName" #default>
        <SectionHeader :title="configurationValues.programName + ' Path Flow'" :description="'The flow below shows how individuals move through the ' + configurationValues.stepTypeCount + ' step types in the ' + configurationValues.programName + ' Path program. You can filter the steps shown by date range or the number of levels to limit&nbsp;to.'" />

        <RockForm @submit="fetchData">
            <div class="row form-row d-flex align-items-start flex-wrap">
                <div class="col-xs-12 col-lg-3">
                    <SlidingDateRangePicker v-model="dateRange" formGroupClasses="" label="Step Completion Date Range" help="Limit steps to those that have been completed in the provided date range." />
                </div>
                <NumberBox v-model="maxLevels" :decimalCount="0" :minimumValue="2" rules="required" formGroupClasses="col" label="Max Levels to Display" help="The maximum number of levels to show in the flow. It's possible that an individual could take the same level twice in the course of completing a step program." />
                <DropDownList v-model="campus" formGroupClasses="col" label="Campus" :items="campusOptions" :showBlankItem="false" />
                <div class="col flex-grow-0">
                    <div class="form-group">
                        <label class="control-label">&nbsp;</label>
                        <RockButton class="btn-square" type="submit" :disabled="isLoading"><i class="fa fa-refresh" :class="{'fa-spin': isLoading}"></i></RockButton>
                    </div>
                </div>
            </div>
        </RockForm>

        <FlowNodeDiagram :flowNodes="flowNodes" :flowEdges="flowEdges" :isLoading="isLoading" :settings="settings" />
    </template>

    <template v-else #default>
        <Alert alert-type="warning">No Step Program ID Provided</Alert>
    </template>
</Block>`
    // #endregion
});
