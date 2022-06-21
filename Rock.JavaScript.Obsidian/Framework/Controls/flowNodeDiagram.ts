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

import { computed, defineComponent, PropType, reactive } from "vue";
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
    totalUnits: number;
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

function round(num: number): number {
    return toDecimalPlaces(num, 2);
}

/**
 * Component for displaying a whole level's nodes and flows in the diagram.
 * Used by the FlowNodeDiagram component defined below it.
 */
const FlowNodeDiagramLevel = defineComponent({ // eslint-disable-line @typescript-eslint/naming-convention
    name: "FlowNodeDiagramLevel",

    props: {
        levelData: {
            type: Array as PropType<FlowDiagramLevel>,
            required: true
        },
        levelNumber: {
            type: Number as PropType<number>,
            required: true
        }
    },

    events: {
        showTooltip: (_html?: string, _e?: MouseEvent) => true
    },

    setup(props, { emit }) {
        // Construct path dimensions and coordinates for a flow
        function flowPoints ({sourcePoint, targetPoint, thickness}: FlowDiagramInFlow): string {
            const oneThirdX = round((targetPoint.x - sourcePoint.x) / 3) + sourcePoint.x;
            const twoThirdsX = round((targetPoint.x - sourcePoint.x) * 2 / 3) + sourcePoint.x;
            const sourceBottom = sourcePoint.y + thickness;
            const targetBottom = targetPoint.y + thickness;

            const start = `M${sourcePoint.x} ${sourcePoint.y}`;
            const curve1 = `C${oneThirdX} ${sourcePoint.y} ${twoThirdsX} ${targetPoint.y} ${targetPoint.x} ${targetPoint.y}`;
            const vertical1 = `V${targetBottom}`;
            const curve2 = `C${twoThirdsX} ${targetBottom} ${oneThirdX} ${sourceBottom} ${sourcePoint.x} ${sourceBottom}`;
            const vertical2 = `V${sourcePoint.y}`;
            const end = "Z";

            return start + curve1 + vertical1 + curve2 + vertical2 + end;
        }

        // Calculate the rotation transformation for the text label of the given node
        function textTransform ({x, y}: Point): string {
            return `rotate(-90, ${x - 6}, ${y})`;
        }

        function nodeClass(node: FlowDiagramLevelNode): string {
            return `node node-${node.id} level-${props.levelNumber}`;
        }

        function flowClass(flow: FlowDiagramInFlow): string {
            return `edge node-${flow.sourceId} node-${flow.targetId} level-${props.levelNumber - 1}_${props.levelNumber}`;
        }

        function onHover (flow: FlowDiagramInFlow, e: MouseEvent): void {
            emit("showTooltip", flow.tooltip, e);
        }

        function onUnHover (): void {
            emit("showTooltip");
        }

        return {
            flowPoints,
            textTransform,
            nodeClass,
            flowClass,
            onHover,
            onUnHover
        };
    },

    template: `
<g v-if="levelNumber == 1">
    <text v-for="node in levelData" key="node.id + 'text'" :x="node.x - 6" :y="node.y" :transform="textTransform(node)" dx="-3" font-size="12" text-anchor="end">
        {{ node.name }}
    </text>
</g>
<g v-if="levelNumber > 1">
    <template v-for="node in levelData" key="node.id + 'flows'">
        <path
            v-for="(flow, index) in node.inFlows"
            key="node.id + 'flow' + index"
            :d="flowPoints(flow)"
            fill="#AAAAAA"
            :fill-opacity="0.6"
            @mousemove="onHover(flow, $event)"
            @mouseout="onUnHover"
            :class="flowClass(flow)"
        ></path>
    </template>
</g>
<g>
    <rect
        v-for="node in levelData"
        key="node.id" :x="node.x"
        :y="node.y"
        :width="node.width"
        :height="node.height"
        :fill="node.color"
        :class="nodeClass(node)"
    ></rect>
</g>
`
});


/**
 * Displays a Flow Node (or maybe better know as Sankey) Diagram as an SVG.
 */
export default defineComponent({
    name: "FlowNodeDiagram",

    components: { FlowNodeDiagramLevel },

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
            const calculated = 24 + /* nodes */ (settings.value.nodeWidth * nodeCount.value) + /* flows */ (settings.value.nodeHorizontalSpacing * (levelsCount.value - 1));

            // Want to make sure it always has a minimum size so we can display "loading" while we have no data
            return Math.max(calculated, 200);
        });
        // Set the chart height based on settings if we have chart data, otherwise set it to a minimal value
        const chartHeight = computed(() => nodeCount.value > 0 ? settings.value.chartHeight : 50);
        // Nodes have a certain order, so let's make sure they're in order

        const diagramData = computed<FlowDiagramData>(() => {
            type FlowPosition = {
                id: number;
                nextLeftY: number;
                nextRightY: number;
            };

            // Make sure nodes are in order
            const orderedNodes = [...props.flowNodes].sort((nodeA, nodeB) => nodeA.order - nodeB.order);

            if (levelsCount.value == 0) {
                return [] as FlowDiagramData;
            }

            const data: FlowDiagramData = [];
            const { nodeWidth, nodeHorizontalSpacing, nodeVerticalSpacing, chartHeight } = settings.value;
            const totalNodeVerticalGap = nodeVerticalSpacing * (nodeCount.value - 1);
            let previousTotalUnits = 0;
            let useableHeight = chartHeight - totalNodeVerticalGap;
            let previousX = 0;
            let currentX = 24; // start with enough room for text labels

            // Hold data about Y positioning of flows on a node
            const flowPositionData: FlowPosition[][] = [[]];

            for (let level = 1; level <= levelsCount.value; level++) {
                // Set up an element for this level
                flowPositionData.push([]);

                // Get everything flowing into the nodes at this level.
                const levelFlows = props.flowEdges.filter(flow => flow.level == level);

                // Total number of units flowing into this level.
                const totalLevelUnits = levelFlows.reduce((tot, {units}) => tot + units, 0);

                if (level > 1) {
                    useableHeight = round(totalLevelUnits / previousTotalUnits * useableHeight);
                }

                // The starting Y position for the next node
                let currentY = (chartHeight - (useableHeight + totalNodeVerticalGap)) / 2;

                // Construct the base diagram nodes, which we'll fill in calculations for later.
                const levelNodes: FlowDiagramLevel = orderedNodes.map(node => {
                    // Get the flows coming into this node and order them by the order of the source nodes
                    const nodeInFlows: FlowEdge[] = levelFlows.filter(flow => flow.targetId == node.id).sort((flowA, flowB): number => {
                        const nodeOrderA = orderedNodes.findIndex(node => node.id == flowA.sourceId);
                        const nodeOrderB = orderedNodes.findIndex(node => node.id == flowB.sourceId);

                        return nodeOrderA - nodeOrderB;
                    });

                    // Calculate size of the node
                    const totalUnits = nodeInFlows.reduce((total, flow) => total + flow.units, 0);
                    const height = round(totalUnits / totalLevelUnits * useableHeight);

                    const nodeFlowPosition = {
                        id: node.id,
                        nextLeftY: currentY,
                        nextRightY: currentY
                    };

                    flowPositionData[level].push(nodeFlowPosition);

                    // Calculate the incoming flows' path points and store on the flow object
                    const inFlows: FlowDiagramInFlow[] = nodeInFlows.map(flow => {
                        const sourcePoint: Point = { x: previousX + nodeWidth, y: 0 };
                        const targetPoint: Point = { x: currentX, y: nodeFlowPosition.nextLeftY };
                        const thickness = round(flow.units / totalUnits * height);

                        nodeFlowPosition.nextLeftY += thickness;

                        if (level > 1) {
                            const prevNodeFlowPosition = flowPositionData[level - 1].find(node => node.id == flow.sourceId);

                            if (prevNodeFlowPosition) {
                                sourcePoint.y = prevNodeFlowPosition.nextRightY;
                                prevNodeFlowPosition.nextRightY += thickness;
                            }
                        }

                        return {
                            ...flow,
                            sourcePoint,
                            targetPoint,
                            thickness
                        };
                    });

                    const levelNode = {
                        ...node,
                        x: currentX,
                        y: currentY,
                        width: nodeWidth,
                        height,
                        totalUnits,
                        inFlows
                    };

                    // Set up for the next node
                    currentY += height + nodeVerticalSpacing;

                    return levelNode;
                });

                // Set up for the next level
                previousTotalUnits = totalLevelUnits;
                previousX = currentX;
                currentX += nodeWidth + nodeHorizontalSpacing;

                data.push(levelNodes);
            } // End for each level

            return data;
        });

        const tooltip = reactive({
            isShown: false,
            html: "",
            x: 0,
            y: 0,
            side: "left"
        });

        function showTooltip (html: string, e: MouseEvent): void {
            if (html && e) {
                tooltip.isShown = true;
                tooltip.html = html;
                tooltip.x = e.offsetX + 15;
                tooltip.y = e.offsetY + 15;

                if (e.clientX + 250 /* magic number... approx max width of a tooltip */ > document.documentElement.clientWidth) {
                    tooltip.x = 0;
                    tooltip.side = "right";
                }
                else {
                    tooltip.side = "left";
                }
            }
            else {
                tooltip.isShown = false;
            }
        }

        return {
            settings,
            nodeCount,
            levelsCount,
            chartWidth,
            chartHeight,
            diagramData,
            tooltip,
            showTooltip
        };
    },

    template: `
<v-style>
.flow-node-diagram-container {
    position: relative;
    width: max-content;
    max-width: 100%;
}

.flow-node-diagram-container .flow-tooltip {
    position: absolute;
    background: white;
    {{ tooltip.side }}: {{ tooltip.x }}px;
    top: {{ tooltip.y }}px;
    width: max-content;
    border: 1px solid #ddd;
    border-radius: 5px;
    padding: 1rem;
}

.flow-node-diagram-container svg {
    width: {{ chartWidth }}px;
    max-width: 100%;
    height: auto;
    min-height: 50px;
}

.flow-node-diagram-container .loadingContainer {
    position: absolute;
    z-index: 0;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    display: flex;
    justify-content: center;
    align-items: center;
    background: rgba(255,255,255,.75);
}

.flow-node-diagram-container .loadingContainer h3 {
    margin: 0;
}

.flow-node-diagram-container .fade-enter-from,
.flow-node-diagram-container .fade-leave-to {
    opacity: 0;
}

.flow-node-diagram-container .fade-enter-active,
.flow-node-diagram-container .fade-leave-active {
    transition: opacity .2s ease-in-out;
}
</v-style>

<div class="flow-node-diagram-container">
    <div class="flow-tooltip" v-html="tooltip.html" v-if="tooltip.isShown" />

    <svg :width="chartWidth" :height="chartHeight" :viewBox="'0 0 ' + chartWidth + ' ' + chartHeight">
        <FlowNodeDiagramLevel
            v-for="(level, levelNum) in diagramData"
            key="levelNum"
            :levelData="level"
            :levelNumber="levelNum + 1"
            @showTooltip="showTooltip"
        />
    </svg>

    <transition name="fade" appear>
        <div v-if="isLoading" class="loadingContainer">
            <h3>Loading...</h3>
        </div>
    </transition>
</div>
`
});