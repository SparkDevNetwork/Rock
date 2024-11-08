"use strict";
var Rock;
(function (Rock) {
    var UI;
    (function (UI) {
        const DefaultOptions = {
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
        class NetworkGraph {
            constructor(elementSelector, options) {
                this.options = Object.assign({}, DefaultOptions, options);
                this.element = document.querySelector(elementSelector);
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
            getVariableColor(varName) {
                const div = document.createElement("div");
                div.style.background = `var(${varName})`;
                div.style.display = "none";
                document.body.append(div);
                const color = window.getComputedStyle(div).background;
                div.remove();
                return color;
            }
            loadNodes(nodes) {
                for (let i = 0; i < nodes.length; i++) {
                    this.graph.addNode(nodes[i].id, {
                        label: nodes[i].label,
                        color: nodes[i].color || this.options.nodeColor,
                        originalColor: nodes[i].color || this.options.nodeColor,
                        size: nodes[i].size
                    });
                }
            }
            loadEdges(edges) {
                for (let i = 0; i < edges.length; i++) {
                    this.graph.addEdgeWithKey(edges[i].id, edges[i].source, edges[i].target, {
                        label: edges[i].label,
                        color: edges[i].color || this.options.edgeColor,
                        originalColor: edges[i].color || this.options.edgeColor,
                        size: edges[i].size
                    });
                }
            }
            calculateNodeSizes() {
                this.graph.forEachNode((id) => {
                    let size = this.graph.getNodeAttribute(id, "size");
                    if (size) {
                        size = this.options.baseNodeSize + size;
                    }
                    else {
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
            calculateEdgeSizes() {
                this.graph.forEachEdge((id) => {
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
            layout() {
                graphologyLibrary.layout.random.assign(this.graph);
                const atlasSettings = graphologyLibrary.layoutForceAtlas2.inferSettings(this.graph);
                graphologyLibrary.layoutForceAtlas2.assign(this.graph, {
                    iterations: 100,
                    settings: atlasSettings
                });
                graphologyLibrary.layoutNoverlap.assign(this.graph);
            }
            render() {
                const settings = {
                    renderEdgeLabels: true
                };
                if (this.options.labelColor) {
                    settings.labelColor = {
                        color: this.options.labelColor
                    };
                }
                const renderer = new Sigma.Sigma(this.graph, this.element, settings);
                renderer.on("enterNode", (event) => {
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
                renderer.on("leaveNode", (event) => {
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
                let dragNode;
                renderer.on("downNode", (event) => {
                    isDragging = true;
                    dragNode = event.node;
                    this.graph.setNodeAttribute(dragNode, "highlighted", true);
                });
                renderer.getMouseCaptor().on("mousemovebody", (event) => {
                    if (!isDragging || !dragNode) {
                        return;
                    }
                    const pos = renderer.viewportToGraph(event);
                    this.graph.setNodeAttribute(dragNode, "x", pos.x);
                    this.graph.setNodeAttribute(dragNode, "y", pos.y);
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
                });
            }
            getRelatedNodesAndEdgesToDepth(nodeId, depth) {
                const nodes = [];
                const edges = [];
                nodes.push(nodeId);
                if (depth > 0) {
                    this.graph.neighbors(nodeId).forEach((neighborId) => {
                        const children = this.getRelatedNodesAndEdgesToDepth(neighborId, depth - 1);
                        nodes.push(...children.nodes);
                        edges.push(...children.edges);
                    });
                }
                return { nodes, edges };
            }
            startAnimationForNodesAndEdges(nodes, edges, highlight) {
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
        UI.NetworkGraph = NetworkGraph;
        class Animation {
            constructor(duration, callback) {
                this.isStopped = false;
                this.start = new Date().getTime();
                this.duration = duration;
                this.callback = callback;
                requestAnimationFrame(() => this.animateFrame());
            }
            animateFrame() {
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
            abort() {
                if (!this.isStopped) {
                    this.isStopped = true;
                }
            }
            cancel() {
                if (!this.isStopped) {
                    this.isStopped = true;
                    this.callback(1);
                }
            }
        }
    })(UI = Rock.UI || (Rock.UI = {}));
})(Rock || (Rock = {}));
//# sourceMappingURL=networkgraph.js.map