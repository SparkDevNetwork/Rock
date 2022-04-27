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

import { computed, defineComponent, PropType } from "vue";
import { toDecimalPlaces } from "../Services/number";

export type FlowNode = {
    id: number;
    name: string;
    color: string;
    order: number;
};

export type FlowEdge = {
    targetId: number;
    sourceId: number | null;
    level: number;
    units: number;
    tooltip: string;
};

type Point = { x: number; y: number };
type Rectangle = Point & { width: number; height: number };
type Path = { sourcePoint: Point, targetPoint: Point, thickness: number };

type FlowDiagramInFlow = FlowEdge & Path;

type FlowDiagramLevelNode = FlowNode & Rectangle & {
    level: number;
    inFlows: FlowDiagramInFlow[];
};

type FlowDiagramLevel = FlowDiagramLevelNode[];

type FlowDiagramData = FlowDiagramLevel[];

// All units are based on the SVG grid units.
export type FlowNodeDiagramSettings = {
    nodeWidth?: number; // The width of the nodes.
    nodeVerticalSpacing?: number; // The vertical gap between the nodes
    nodeHorizontalSpacing?: number; // The width of the gap between the node levels that the flows go through.
    chartHeight?: number; // The viewBox height, also px height of SVG if not shrunk by too small of a container.
};

type FlowNodeDiagramSettingsFull = {
    nodeWidth: number;
    nodeVerticalSpacing: number;
    nodeHorizontalSpacing: number;
    chartHeight: number;
};

const defaultSettings = {
    nodeWidth: 12,
    nodeVerticalSpacing: 12,
    nodeHorizontalSpacing: 200,
    chartHeight: 900
};


/**
 * Displays a Flow Node (or maybe better know as Sankey) Diagram as an SVG.
 */
export default defineComponent({
    name: "FlowNodeDiagram",

    props: {
        // Details about the nodes that are being "flowed" between.
        flowNodes: {
            type: Array as PropType<FlowNode[]>,
            default: () => []
        },

        // Details about flows between nodes
        flowEdges: {
            type: Array as PropType<FlowEdge[]>,
            default: () => []
        },

        // Settings that control the sizes of different items in the diagram.
        settings: {
            type: Object as PropType<FlowNodeDiagramSettings>,
            default: () => ({})
        },

        // Show a spinner to indicate the data is being loaded if true.
        isLoading: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    setup(props) {
        const settings = computed<FlowNodeDiagramSettingsFull>(() => ({ ...defaultSettings, ...props.settings }));
        const nodeCount = computed(() => props.flowNodes.length);
        const levelsCount = computed(() => props.flowEdges.reduce((count, edge) => Math.max(count, edge.level), 0));
        const chartWidth = computed(() => {
            // 24 is the left margin before the first level nodes so we have room for labels and a bit of padding.
            return 24 +
                // nodes
                (settings.value.nodeWidth * nodeCount.value) +
                // flows
                (settings.value.nodeHorizontalSpacing * (levelsCount.value - 1));
        });

        const diagramData = computed<FlowDiagramData>(() => {
            if (levelsCount.value == 0) {
                return [];
            }

            const data: FlowDiagramData = [];
            let useableHeight = settings.value.chartHeight - settings.value.nodeVerticalSpacing * (nodeCount.value - 1);
            let previousX = 0;
            let currentX = 24;

            for (let level = 1; level <= levelsCount.value; level++) {
                // TODO HERE
                // get flows for level, grouped by node, add up data going into each node
                // use data to calculate node heights (% of chartHeight - space*nodes-1)
                // use heights to determine
            }

            return data;
        });

        function round(num: number): number {
            return toDecimalPlaces(num, 2);
        }

        return {
            settings,
            nodeCount,
            levelsCount,
            chartWidth,
            diagramData,
            round
        };
    },

    template: `
<v-style>
.flow-node-diagram-container {
    position: relative;
    width: 100%;
    height: {{settings.chartHeight}}px;
    background: rgba(0,0,0,.05);
}

.loadingContainer {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    display: flex;
    justify-content: center;
    align-items: center;
}

.fade-enter-from,
.fade-leave-to {
    opacity: 0;
}

.fade-enter-active,
.fade-leave-active {
    transition: opacity .1s ease-in-out;
}
</v-style>

<div class="flow-node-diagram-container">
    <transition name="fade" mode="out-in" appear>

        <div v-if="isLoading" class="loadingContainer">
            <h3>Loading...</h3>
        </div>

        <svg v-else :width="chartWidth" :height="settings.chartHeight" :viewBox="'0 0 ' + chartWidth + ' ' + settings.chartHeight">
            <text x="20" y="0" font-size="12" text-anchor="end" transform="rotate(-90, 20, 2)" dx="-3">Baptism</text>
            <rect x="26" y="0" height="200" width="12" fill="purple"></rect>
            <path d="M38 0 C105 0 171 100 238 100 V 180 C171 180 105 80 38 80V0Z" fill="#AAAAAA" fill-opacity="0.6"></path>
            <rect x="238" y="100" height="120" width="12" fill="purple"></rect>
        </svg>

    </transition>
</div>
`
});