(function(global) {
    let vue;
    let SankeyDiagram;
    let importsReady = false;
    let readyListeners = [];

    global.Rock = global.Rock || {};
    Rock.Lava = Rock.Lava || {};
    Rock.Lava.Shortcode = Rock.Lava.Shortcode || {}
    Rock.Lava.Shortcode.SankeyDiagram = {
        init: function (selector, edges, nodes, settings, nodeTooltipActionLabel) {
            function run() {
                vue.createApp({
                    components: { SankeyDiagram },
                    setup: () => ({ edges, nodes, settings, nodeTooltipActionLabel }),
                    template: `<SankeyDiagram :flowNodes="nodes" :flowEdges="edges" :settings="settings" :nodeTooltipActionLabel="nodeTooltipActionLabel" />`,
                }).mount(selector);
            }

            if (importsReady && typeof vue !== "undefined" && typeof SankeyDiagram !== "undefined") {
                run()
            }
            else {
                readyListeners.push(run)
            }
        }
    }

    function setImports(imports) {
        vue = imports[0];
        SankeyDiagram = imports[1].default;
        importsReady = true;

        if(readyListeners.length > 0) {
            readyListeners.forEach(function(cb) {
                cb()
            });

            readyListeners = [];
        }
    }

    Obsidian.onReady(() => {
        let vueImport = System.import("vue");
        let sankeyDiagramImport = System.import("@Obsidian/Controls/internal/SankeyDiagram/sankeyDiagram.obs");

        Promise.all([vueImport, sankeyDiagramImport]).then(setImports);
    });
})(window)
