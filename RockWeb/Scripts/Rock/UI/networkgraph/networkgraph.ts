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

declare var graphology: any;
declare var graphologyLibrary: any;
declare var Sigma: any;

namespace Rock.UI {
    /**
     * The options that can be passed into the network graph.
     */
    interface Options {
        /**
         * This value will be added to all edge sizes as a way to provide
         * a common boost to the size of all edges.
         */
        baseEdgeSize: number;

        /**
         * This value will be added to all node sizes as a way to provide
         * a common boost to the size of all nodes.
         */
        baseNodeSize: number;

        /**
         * The default color for any edge that does not specify a color.
         */
        edgeColor?: string | null;

        /**
         * The list of edges in the graph.
         */
        edges?: GraphEdge[];

        /**
         * The color to use when highlighting nodes and edges.
         */
        highlightColor?: string | null;

        /**
         * The scale factor to apply to highlighted edges and nodes. A value
         * of 0.5 means half size, 2.0 means double size, 1.0 means leave
         * at original size.
         */
        highlightScaleFactor?: number;

        /**
         * The text color to use for labels.
         */
        labelColor?: string | null;

        /**
         * The maximum size a edge is allowed to be. Any edge larger than
         * this will be set to this size.
         */
        maximumEdgeSize: number;

        /**
         * The maximum size a node is allowed to be. Any node larger than
         * this will be set to this size.
         */
        maximumNodeSize: number;

        /**
         * The minimum size a edge is allowed to be. Any edge smaller than
         * this will be set to this size.
         */
        minimumEdgeSize: number;

        /**
         * The minimum size a node is allowed to be. Any node smaller than
         * this will be set to this size.
         */
        minimumNodeSize: number;

        /**
         * The default color for any node that does not specify a color.
         */
        nodeColor?: string | null;

        /**
         * The list of nodes in the graph.
         */
        nodes?: GraphNode[];
    }

    /**
     * Defines the structure of a single node on the graph.
     */
    interface GraphNode {
        /** Gets or sets the optional color for this node. */
        color?: string | null;

        /** Gets or sets the unique identifier of this node. */
        id: string;

        /** Gets or sets the label to display on this node. */
        label?: string | null;

        /** Gets or sets the optional size for this node. */
        size?: number | null;
    }

    /**
     * Defines the structure of a single edge on the graph.
     */
    interface GraphEdge {
        /** Gets or sets the optional color for this edge. */
        color?: string | null;

        /** Gets or sets the unique identifier of this edge. */
        id: string;

        /** Gets or sets the label to display on this edge. */
        label?: string | null;

        /** Gets or sets the optional size for this edge. */
        size?: number | null;

        /** Gets or sets the source node identifier. */
        source: string;

        /** Gets or sets the target node identifier. */
        target: string;
    }

    /**
     * Defines the structure of a single item animation.
     */
    interface GraphAnimation {
        /** The id of the node or edge. */
        id: string;

        /** The value to animate from. */
        from: number;

        /** The value to animate to. */
        to: number;
    }

    /**
     * Default options to use if not otherwise specified.
     */
    const DefaultOptions: Options = {
        baseEdgeSize: 0,
        baseNodeSize: 0,
        edgeColor: null,
        edges: [],
        highlightColor: null,
        highlightScaleFactor: 1.1,
        labelColor: null,
        maximumEdgeSize: 25,
        maximumNodeSize: 100,
        minimumEdgeSize: 0,
        minimumNodeSize: 0,
        nodeColor: null,
        nodes: [],
    };

    export class NetworkGraph {
        /** The options this media player was initialized with. */
        private options: Required<Options>;

        /** The element that was found by the elementId. */
        private element: HTMLElement;

        /** The graph data. */
        private graph: any;

        private animation?: Animation;

        /**
         * Creates a new media player using the specified element identifier
         * as the placeholder for the player.
         *
         * @param elementSelector The identifier or CSS selector of the placeholder element. All contents will be replaced with the media player.
         * @param options The options to initialize this instance with.
         */
        constructor(elementSelector: string, options: Options) {
            this.options = Object.assign({}, DefaultOptions, options) as Required<Options>;

            this.element = document.querySelector(elementSelector) as HTMLElement;

            if (this.element === null) {
                throw "Element not found to initialize graph with.";
            }

            this.graph = new graphology({ type: "undirected" });
            this.options.nodeColor = this.options.nodeColor || this.getVariableColor("--theme-medium");
            this.options.edgeColor = this.options.edgeColor || this.getVariableColor("--theme-medium");
            this.options.highlightColor = this.options.highlightColor || this.getVariableColor("--color-primary");

            this.loadNodes(this.options.nodes);
            this.loadEdges(this.options.edges);
            this.calculateNodeSizes();
            this.calculateEdgeSizes();

            this.layout();
            this.render();
        }

        /**
         * Gets the CSS color associated with the named variable.
         * 
         * @param varName The name of the variable, including the -- prefix.
         * 
         * @returns The string that represents the CSS color.
         */
        private getVariableColor(varName: string): string {
            const div = document.createElement("div");

            div.style.background = `var(${varName})`;
            div.style.display = "none";
            document.body.append(div);

            const color = window.getComputedStyle(div).background;

            div.remove();

            return color;
        }

        /**
         * Loads the nodes from the list into the graph data.
         * 
         * @param nodes The list of nodes.
         */
        private loadNodes(nodes: GraphNode[]): void {
            for (let i = 0; i < nodes.length; i++) {
                this.graph.addNode(nodes[i].id, {
                    label: nodes[i].label,
                    color: nodes[i].color || this.options.nodeColor,
                    originalColor: nodes[i].color || this.options.nodeColor,
                    size: nodes[i].size
                });
            }
        }

        /**
         * Loads the edges from the list into the graph data.
         * 
         * @param edges The list of edges.
         */
        private loadEdges(edges: GraphEdge[]): void {
            for (let i = 0; i < edges.length; i++) {
                this.graph.addEdgeWithKey(edges[i].id, edges[i].source, edges[i].target, {
                    label: edges[i].label,
                    color: edges[i].color || this.options.edgeColor,
                    originalColor: edges[i].color || this.options.edgeColor,
                    size: edges[i].size
                });
            }

        }

        /**
         * Perform final calculation of node sizes by auto-calculation and
         * limiting to configured minimums and maximums.
         */
        private calculateNodeSizes(): void {
            this.graph.forEachNode((id: string) => {
                let size = this.graph.getNodeAttribute(id, "size");

                if (size) {
                    size = this.options.baseNodeSize + size;
                }
                else {
                    // Plus one because a node without connections has a degree of 0.
                    size = this.options.baseNodeSize + this.graph.degree(id) + 1;
                }

                if (this.options.minimumNodeSize) {
                    size = Math.max(this.options.minimumNodeSize, size);
                }

                if (this.options.maximumNodeSize) {
                    size = Math.min(this.options.maximumNodeSize, size);
                }

                this.graph.setNodeAttribute(id, "size", size);
                this.graph.setNodeAttribute(id, "originalSize", size);
            });
        }

        /**
         * Perform final calculation of edge sizes and limiting to configured
         * minimums and maximums.
         */
        private calculateEdgeSizes(): void {
            this.graph.forEachEdge((id: string) => {
                let size = this.graph.getEdgeAttribute(id, "size");

                if (size) {
                    size = this.options.baseEdgeSize + size;
                }
                else {
                    size = 1;
                }

                if (this.options.minimumEdgeSize) {
                    size = Math.max(this.options.minimumEdgeSize, size);
                }

                if (this.options.maximumEdgeSize) {
                    size = Math.min(this.options.maximumEdgeSize, size);
                }

                this.graph.setEdgeAttribute(id, "size", size);
                this.graph.setEdgeAttribute(id, "originalSize", size);
            });
        }

        /**
         * Perform layout calculation on the graph data to position all the
         * nodes and edges.
         */
        private layout(): void {
            graphologyLibrary.layout.random.assign(this.graph);

            const atlasSettings = graphologyLibrary.layoutForceAtlas2.inferSettings(this.graph);
            graphologyLibrary.layoutForceAtlas2.assign(this.graph, {
                iterations: 100,
                settings: atlasSettings
            });

            graphologyLibrary.layoutNoverlap.assign(this.graph);
        }

        /**
         * Renders the graph into the canvas.
         */
        private render(): void {
            const settings: Record<string, unknown> = {
                renderEdgeLabels: true
            };

            if (this.options.labelColor) {
                settings.labelColor = {
                    color: this.options.labelColor
                };
            }

            const renderer = new Sigma.Sigma(this.graph, this.element, settings);

            // Update nodes and edges when the mouse hovers over a node.
            renderer.on("enterNode", (event: any) => {
                if (this.animation) {
                    this.animation.cancel();
                }

                const items = this.getRelatedNodesAndEdgesToDepth(event.node, 1);

                if (this.options.highlightColor) {
                    items.nodes.forEach(id => this.graph.setNodeAttribute(id, "color", this.options.highlightColor));
                    items.edges.forEach(id => this.graph.setEdgeAttribute(id, "color", this.options.highlightColor));
                }

                this.animation = this.startAnimationForNodesAndEdges(items.nodes, items.edges, true);
            });

            // Restore nodes and edges when the mouse stops hovering a node.
            renderer.on("leaveNode", (event: any) => {
                if (this.animation) {
                    this.animation.abort();
                }

                const items = this.getRelatedNodesAndEdgesToDepth(event.node, 1);

                if (this.options.highlightColor) {
                    items.nodes.forEach(id => this.graph.setNodeAttribute(id, "color", this.graph.getNodeAttribute(id, "originalColor")));
                    items.edges.forEach(id => this.graph.setEdgeAttribute(id, "color", this.graph.getEdgeAttribute(id, "originalColor")));
                }

                this.animation = this.startAnimationForNodesAndEdges(items.nodes, items.edges, false);
            });

            let isDragging = false;
            let dragNode: string | undefined;

            renderer.on("downNode", (event: any) => {
                isDragging = true;
                dragNode = event.node;
                this.graph.setNodeAttribute(dragNode, "highlighted", true);
            });

            renderer.getMouseCaptor().on("mousemovebody", (event: any) => {
                if (!isDragging || !dragNode) {
                    return;
                }

                const pos = renderer.viewportToGraph(event);

                this.graph.setNodeAttribute(dragNode, "x", pos.x);
                this.graph.setNodeAttribute(dragNode, "y", pos.y);

                // Prevent sigma to move camera:
                event.preventSigmaDefault();
                event.original.preventDefault();
                event.original.stopPropagation();
            });

            renderer.getMouseCaptor().on("mouseup", () => {
                if (dragNode) {
                    this.graph.setNodeAttribute(dragNode, "highlighted", false);
                }

                isDragging = false;
                dragNode = undefined;
            })
        }

        /**
         * Get related nodes and edges up to a maximum depth from the specified node.
         * A depth of 1 will include immediate edges and the other nodes for those edges.
         * 
         * @param nodeId The main node to be queried.
         * @param depth The maximum depth of nodes and edges to traverse.
         * 
         * @returns An object that identifies the nodes and edges.
         */
        private getRelatedNodesAndEdgesToDepth(nodeId: string, depth: number): { nodes: string[], edges: string[] } {
            const nodes: string[] = [];
            const edges: string[] = [];

            nodes.push(nodeId);

            if (depth > 0) {
                this.graph.neighbors(nodeId).forEach((neighborId: string) => {
                    const children = this.getRelatedNodesAndEdgesToDepth(neighborId, depth - 1);

                    nodes.push(...children.nodes);
                    edges.push(...children.edges);
                });
            }

            return { nodes, edges };
        }

        /**
         * Starts a size animation for the specified nodes and edges.
         * 
         * @param nodes The nodes to be animated.
         * @param edges The edges to be animated.
         * @param highlight If true the items will grow; otherwise false.
         * 
         * @returns The animation object.
         */
        private startAnimationForNodesAndEdges(nodes: string[], edges: string[], highlight: boolean): Animation {
            const itemsToAnimate = {
                nodes: nodes.map(id => ({
                    id,
                    from: this.graph.getNodeAttribute(id, "size"),
                    to: this.graph.getNodeAttribute(id, "originalSize") * (highlight ? this.options.highlightScaleFactor : 1)
                })),
                edges: edges.map(id => ({
                    id,
                    from: this.graph.getEdgeAttribute(id, "size"),
                    to: this.graph.getEdgeAttribute(id, "originalSize") * (highlight ? this.options.highlightScaleFactor : 1)
                }))
            };

            return new Animation(250, p => {
                itemsToAnimate.nodes.forEach(n => {
                    this.graph.setNodeAttribute(n.id, "size", n.from + ((n.to - n.from) * p));

                });

                itemsToAnimate.edges.forEach(e => {
                    this.graph.setNodeAttribute(e.id, "size", e.from + ((e.to - e.from) * p));

                });
            });
        }
    }

    /**
     * Simple class to handle linear animations.
     */
    class Animation {
        /** The start timestamp. */
        private start: number;

        /** The target duration of the animation. */
        private duration: number;

        /** True if this animation has stopped. */
        private isStopped = false;

        /** The callback that will update the values. */
        private callback: (progress: number) => void;

        /**
         * Creates a new animation instance.
         * 
         * @param duration The duration in milliseconds.
         * @param callback The callback that will update the values.
         */
        public constructor(duration: number, callback: (progress: number) => void) {
            this.start = new Date().getTime();
            this.duration = duration;
            this.callback = callback;

            requestAnimationFrame(() => this.animateFrame());
        }

        /**
         * Animate a single frame.
         */
        private animateFrame(): void {
            if (this.isStopped) {
                return;
            }

            var progress = Math.min(1, (new Date().getTime() - this.start) / this.duration);

            try {
                this.callback(progress);
            }
            finally {
                if (progress === 1) {
                    this.abort();
                }
                else {
                    requestAnimationFrame(() => this.animateFrame());
                }
            }
        }

        /**
         * Abort the animation and leave the values as is.
         */
        public abort(): void {
            if (!this.isStopped) {
                this.isStopped = true;
            }
        }

        /**
         * Cancel the animation and jump the value to the target value.
         */
        public cancel(): void {
            if (!this.isStopped) {
                this.isStopped = true;
                this.callback(1);
            }
        }
    }
}
