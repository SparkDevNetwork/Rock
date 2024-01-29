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
import { App, Component, createApp, defineComponent, h, markRaw, onMounted, provide, VNode } from "vue";
import RockBlock from "./rockBlock.partial";
import { useStore } from "@Obsidian/PageState";
import "@Obsidian/ValidationRules";
import "@Obsidian/FieldTypes/index";
import { DebugTiming } from "@Obsidian/ViewModels/Utility/debugTiming";
import { ObsidianBlockConfigBag } from "@Obsidian/ViewModels/Cms/obsidianBlockConfigBag";
import { PageConfig } from "@Obsidian/Utility/page";
import { RockDateTime } from "@Obsidian/Utility/rockDateTime";
import { BasicSuspenseProvider, provideSuspense } from "@Obsidian/Utility/suspense";
import { alert } from "@Obsidian/Utility/dialogs";
import { HttpBodyData, HttpMethod, HttpResult, HttpUrlParams } from "@Obsidian/Types/Utility/http";
import { doApiCall, provideHttp } from "@Obsidian/Utility/http";
import { createInvokeBlockAction, provideBlockGuid } from "@Obsidian/Utility/block";

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
export async function initializeBlock(config: ObsidianBlockConfigBag): Promise<App> {
    const blockPath = `${config.blockFileUrl}.js`;
    let blockComponent: Component | null = null;
    let errorMessage = "";

    if (!config || !config.blockFileUrl || !config.blockGuid || !config.rootElementId) {
        console.error("Invalid block configuration:", config);
        throw "Could not initialize Obsidian block because the configuration is invalid.";
    }

    const rootElement = document.getElementById(config.rootElementId);

    if (!rootElement) {
        throw "Could not initialize Obsidian block because the root element was not found.";
    }

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
        setup() {
            let isLoaded = false;

            // Create a suspense provider so we can monitor any asynchronous load
            // operations that should delay the display of the page.
            const suspense = new BasicSuspenseProvider(undefined);
            provideSuspense(suspense);

            /** Called to note on the body element that this block is loading. */
            const startLoading = (): void => {
                let pendingCount = parseInt(document.body.getAttribute("data-obsidian-pending-blocks") ?? "0");
                pendingCount++;
                document.body.setAttribute("data-obsidian-pending-blocks", pendingCount.toString());
            };

            /** Called to note when this block has finished loading. */
            const finishedLoading = (): void => {
                if (isLoaded) {
                    return;
                }

                isLoaded = true;

                rootElement.classList.remove("obsidian-block-loading");

                // Get the number of pending blocks. If this is the last one
                // then signal the page that all blocks are loaded and ready.
                let pendingCount = parseInt(document.body.getAttribute("data-obsidian-pending-blocks") ?? "0");
                if (pendingCount > 0) {
                    pendingCount--;
                    document.body.setAttribute("data-obsidian-pending-blocks", pendingCount.toString());
                    if (pendingCount === 0) {
                        document.body.classList.remove("obsidian-loading");
                    }
                }
            };

            // Start loading and wait for up to 5 seconds for the block to finish.
            startLoading();
            setTimeout(finishedLoading, 5000);

            // Called when all our child components have initialized.
            onMounted(() => {
                if (!suspense.hasPendingOperations()) {
                    finishedLoading();
                }
                else {
                    suspense.addFinishedHandler(() => {
                        finishedLoading();
                    });
                }
            });

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
    app.mount(rootElement);

    return app;
}

/**
 * Loads and shows a custom block action. This is a special purpose function
 * designed to be used only by the WebForms PageZoneBlocksEditor.ascx.cs control.
 * It will be removed once WebForms blocks are no longer supported.
 *
 * @param actionFileUrl The component file URL for the action handler.
 * @param pageGuid The unique identifier of the page.
 * @param blockGuid The unique identifier of the block.
 */
export async function showCustomBlockAction(actionFileUrl: string, pageGuid: string, blockGuid: string): Promise<void> {
    let actionComponent: Component | null = null;

    try {
        const actionComponentModule = await import(actionFileUrl);
        actionComponent = actionComponentModule ?
            (actionComponentModule.default || actionComponentModule) :
            null;
    }
    catch (e) {
        // Log the error, but continue setting up the app so the UI will show the user an error
        console.error(e);
        alert("There was an error trying to show these settings.");
        return;
    }

    const name = `Action${actionFileUrl.replace(/\//g, ".")}`;

    const app = createApp({
        name,
        components: {
        },
        setup() {
            // Create a suspense provider so we can monitor any asynchronous load
            // operations that should delay the display of the page.
            const suspense = new BasicSuspenseProvider(undefined);
            provideSuspense(suspense);

            const httpCall = async <T>(method: HttpMethod, url: string, params: HttpUrlParams = undefined, data: HttpBodyData = undefined): Promise<HttpResult<T>> => {
                return await doApiCall<T>(method, url, params, data);
            };

            const get = async <T>(url: string, params: HttpUrlParams = undefined): Promise<HttpResult<T>> => {
                return await httpCall<T>("GET", url, params);
            };

            const post = async <T>(url: string, params: HttpUrlParams = undefined, data: HttpBodyData = undefined): Promise<HttpResult<T>> => {
                return await httpCall<T>("POST", url, params, data);
            };

            const invokeBlockAction = createInvokeBlockAction(post, pageGuid, blockGuid, store.state.pageParameters);

            provideHttp({
                doApiCall,
                get,
                post
            });
            provide("invokeBlockAction", invokeBlockAction);
            provideBlockGuid(blockGuid);

            return {
                actionComponent,
                onCustomActionClose
            };
        },

        // Note: We are using a custom alert so there is not a dependency on
        // the Controls package.
        template: `<component :is="actionComponent" @close="onCustomActionClose" />`
    });

    function onCustomActionClose(): void {
        app.unmount();
        rootElement.remove();
    }

    const rootElement = document.createElement("div");
    document.body.append(rootElement);

    app.component("v-style", developerStyle);
    app.mount(rootElement);
}

/**
 * This should be called once per page with data from the server that pertains to the entire page. This includes things like
 * page parameters and context entities.
 *
 * @param {object} pageConfig
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

    const pageDebugTimings = (await import("@Obsidian/Controls/Internal/pageDebugTimings.obs")).default;

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
