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
import { toDecimalPlaces } from "@Obsidian/Utility/numberUtils";

import { FlowNodeDiagramNodeBag } from "@Obsidian/ViewModels/Blocks/Engagement/Steps/flowNodeDiagramNodeBag";
import { FlowNodeDiagramEdgeBag } from "@Obsidian/ViewModels/Blocks/Engagement/Steps/flowNodeDiagramEdgeBag";
import { FlowNodeDiagramSettingsBag } from "@Obsidian/ViewModels/Blocks/Engagement/Steps/flowNodeDiagramSettingsBag";

type Point = { x: number; y: number };
type Rectangle = Point & { width: number; height: number };
type Path = { sourcePoint: Point, targetPoint: Point, thickness: number };

type FlowDiagramInFlow = FlowNodeDiagramEdgeBag & Path;
type FlowDiagramLevelNode = FlowNodeDiagramNodeBag & Rectangle & {
    totalUnits: number;
    inFlows: FlowDiagramInFlow[];
};

type FlowDiagramLevel = FlowDiagramLevelNode[];
type FlowDiagramData = FlowDiagramLevel[];

type NonNullValues<T> = { [P in keyof T]: NonNullable<T[P]> };
type FlowNodeDiagramSettingsFull = NonNullValues<Required<FlowNodeDiagramSettingsBag>>;

const defaultSettings: FlowNodeDiagramSettingsFull = {
    nodeWidth: 12,
    nodeVerticalSpacing: 12,
    chartWidth: 1200,
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
        const visibleNodes = computed(() => {
            return props.levelData.filter(node => node.height > 0);
        });

        // Construct path dimensions and coordinates for a flow
        function flowPoints({ sourcePoint, targetPoint, thickness }: FlowDiagramInFlow): string {
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
        function textTransform({ x, y }: Point): string {
            return `rotate(-90, ${x - 6}, ${y})`;
        }

        function nodeClass(node: FlowDiagramLevelNode): string {
            return `node node-${node.id} level-${props.levelNumber}`;
        }

        function flowClass(flow: FlowDiagramInFlow): string {
            return `edge node-${flow.sourceId} node-${flow.targetId} level-${props.levelNumber - 1}_${props.levelNumber}`;
        }

        function onHoverFlow(flow: FlowDiagramInFlow, e: MouseEvent): void {
            emit("showTooltip", flow.tooltip, e);
        }

        function onHoverNode(node: FlowDiagramLevelNode, e: MouseEvent): void {
            emit("showTooltip", `<strong>${node.name}</strong><br>Total Steps Taken: ${node.totalUnits}`, e);
        }

        function onUnHover(): void {
            emit("showTooltip");
        }

        return {
            visibleNodes,
            flowPoints,
            textTransform,
            nodeClass,
            flowClass,
            onHoverFlow,
            onHoverNode,
            onUnHover
        };
    },

    template: `
<g v-if="levelNumber == 1">
    <text v-for="node in visibleNodes" key="node.id + 'text'" :x="node.x - 6" :y="node.y" :transform="textTransform(node)" dx="-3" font-size="12" text-anchor="end">
        {{ node.name }}
    </text>
</g>
<g v-if="levelNumber > 1">
    <template v-for="node in levelData" key="node.id + 'flows'">
        <path
            v-for="(flow, index) in node.inFlows"
            key="node.id + 'flow' + index"
            :d="flowPoints(flow)"
            fill="rgb(170, 170, 170, 0.6)"
            @mousemove="onHoverFlow(flow, $event)"
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
        @mousemove="onHoverNode(node, $event)"
        @mouseout="onUnHover"
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
            type: Array as PropType<FlowNodeDiagramNodeBag[]>,
            default: () => []
        },

        // Details about flows between nodes
        flowEdges: {
            type: Array as PropType<FlowNodeDiagramEdgeBag[]>,
            default: () => []
        },

        // Settings that control the sizes of different items in the diagram.
        settings: {
            type: Object as PropType<FlowNodeDiagramSettingsBag>,
            default: () => ({})
        },

        // Show a spinner to indicate the data is being loaded if true.
        isLoading: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },

    setup(props) {
        const settings = computed<FlowNodeDiagramSettingsFull>(() => {
            const settings = { ...defaultSettings };
            Object.entries(props.settings).forEach(([key, value]) => {
                if (value !== undefined && value !== null) {
                    settings[key] = value;
                }
            });

            return settings;
        });
        const nodeCount = computed(() => props.flowNodes.length);
        const levelsCount = computed(() => props.flowEdges.reduce((count, edge) => Math.max(count, edge.level), 0));
        const chartWidth = computed(() => settings.value.chartWidth);
        const nodeHorizontalSpacing = computed(() => {
            const flowSpace = settings.value.chartWidth - /* nodes */ (settings.value.nodeWidth * levelsCount.value) - /* Label Text */ 24;
            return flowSpace / (levelsCount.value - 1);
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
            const { nodeWidth, nodeVerticalSpacing, chartHeight } = settings.value;
            const totalNodeVerticalGap = nodeVerticalSpacing * (nodeCount.value - 1);
            let previousTotalUnits = 0;
            let useableHeight = chartHeight - totalNodeVerticalGap - 50; // The 50 gives some padding at the bottom for long labels
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
                const totalLevelUnits = levelFlows.reduce((tot, { units }) => tot + units, 0);

                if (level > 1) {
                    useableHeight = round(totalLevelUnits / previousTotalUnits * useableHeight);
                }

                // The starting Y position for the next node
                let currentY = (chartHeight - (useableHeight + totalNodeVerticalGap)) / 2;

                // Construct the base diagram nodes, which we'll fill in calculations for later.
                const levelNodes: FlowDiagramLevel = orderedNodes.map(node => {
                    // Get the flows coming into this node and order them by the order of the source nodes
                    const nodeInFlows: FlowNodeDiagramEdgeBag[] = levelFlows.filter(flow => flow.targetId == node.id).sort((flowA, flowB): number => {
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
                    currentY += height + (height > 0 ? nodeVerticalSpacing : 0);

                    return levelNode;
                });

                // Set up for the next level
                previousTotalUnits = totalLevelUnits;
                previousX = currentX;
                currentX += nodeWidth + nodeHorizontalSpacing.value;

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

        function showTooltip(html: string, e: MouseEvent): void {
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
    margin: 0 auto;
}

.flow-node-diagram-container .flow-tooltip {
    position: absolute;
    background: #fff;
    {{ tooltip.side }}: {{ tooltip.x }}px;
    top: {{ tooltip.y }}px;
    max-width: 260px;
    border: 1px solid #ddd;
    border-radius: 4px;
    padding: 8px;
    font-size: 14px;
    box-shadow: 0 1px 2px 0 rgba(0,0,0,.05)
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

.step-flow-svg .edge:hover {
    fill: rgba(170, 170, 170, 0.8);
}
</v-style>

<div class="flow-node-diagram-container">
    <div class="flow-tooltip" v-html="tooltip.html" v-if="tooltip.isShown" />

    <svg class="step-flow-svg mx-auto" :width="chartWidth" :height="chartHeight" :viewBox="'0 0 ' + chartWidth + ' ' + chartHeight">
        <FlowNodeDiagramLevel
            v-for="(level, levelNum) in diagramData"
            :key="'level' + levelNum"
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
