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
import { App, Component, createApp, defineComponent, h, markRaw, VNode } from "vue";
import RockBlock from "./rockBlock";
import { useStore } from "./Store/index";
import "./Rules/index";
import { DebugTiming } from "@Obsidian/ViewModels/Utility/debugTiming";
import { BlockConfig } from "./Util/block";
import { PageConfig } from "./Util/page";
import { RockDateTime } from "./Util/rockDateTime";

type DebugTimingConfig = {
    elementId: string;
    debugTimingViewModels: DebugTiming[];
};

const store = useStore();

/**
 * This is a special use component that allows developers to include style
 * tags inside a string-literal component (i.e. not an SFC). It should only
 * be used temporarily until the styling team can move the styles into the
 * LESS and CSS files.
 */
const developerStyle = defineComponent({
    render(): VNode {
        return h("style", {}, this.$slots.default ? this.$slots.default() : undefined);
    }
});


/**
* This should be called once per block on the page. The config contains configuration provided by the block's server side logic
* counterpart.  This adds the block to the page and allows it to begin initializing.
* @param config
* @param blockComponent
*/
export async function initializeBlock(config: BlockConfig): Promise<App> {
    const blockPath = `${config.blockFileUrl}.js`;
    let blockComponent: Component | null = null;
    let errorMessage = "";

    try {
        const blockComponentModule = await import(blockPath);
        blockComponent = blockComponentModule ?
            (blockComponentModule.default || blockComponentModule) :
            null;
    }
    catch (e) {
        // Log the error, but continue setting up the app so the UI will show the user an error
        console.error(e);
        errorMessage = `${e}`;
    }

    const name = `Root${config.blockFileUrl.replace(/\//g, ".")}`;
    const startTimeMs = RockDateTime.now().toMilliseconds();

    const app = createApp({
        name,
        components: {
            RockBlock
        },
        data() {
            return {
                config: config,
                blockComponent: blockComponent ? markRaw(blockComponent) : null,
                startTimeMs,
                errorMessage
            };
        },

        // Note: We are using a custom alert so there is not a dependency on
        // the Controls package.
        template: `
<div v-if="errorMessage" class="alert alert-danger">
    <strong>Error Initializing Block</strong>
    <br />
    {{errorMessage}}
</div>
<RockBlock v-else :config="config" :blockComponent="blockComponent" :startTimeMs="startTimeMs" />`
    });

    app.component("v-style", developerStyle);
    app.mount(config.rootElement);

    return app;
}

/**
* This should be called once per page with data from the server that pertains to the entire page. This includes things like
* page parameters and context entities.
* @param {object} pageData
*/
export async function initializePage(pageConfig: PageConfig): Promise<void> {
    await store.initialize(pageConfig);
}

/**
 * Shows the Obsidian debug timings
 * @param debugTimingConfig
 */
export async function initializePageTimings(config: DebugTimingConfig): Promise<void> {
    const rootElement = document.getElementById(config.elementId);

    if (!rootElement) {
        console.error("Could not show Obsidian debug timings because the HTML element did not resolve.");
        return;
    }

    const pageDebugTimings = (await import("./Controls/pageDebugTimings")).default;

    const app = createApp({
        name: "PageDebugTimingsRoot",
        components: {
            PageDebugTimings: pageDebugTimings
        },
        data() {
            return {
                viewModels: config.debugTimingViewModels
            };
        },
        template: `<PageDebugTimings :serverViewModels="viewModels" />`
    });
    app.mount(rootElement);
}
