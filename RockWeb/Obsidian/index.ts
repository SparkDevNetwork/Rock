import { App, Component, createApp, markRaw } from './Vendor/Vue/vue.js';
import RockBlock from './Controls/RockBlock.js';
import store from './Store/Index.js';
import { Guid } from './Util/Guid.js';
import Person from './Types/Models/Person.js';
import Entity from './Types/Models/Entity.js';
import './Rules/Index.js';

export type BlockSettings = Record<string, unknown>;

export type BlockConfig = {
    blockFileUrl: string;
    rootElement: Element;
    blockGuid: Guid;
    blockSettings: BlockSettings;
};

export type PageConfig = {
    pageId: number;
    pageGuid: Guid;
    pageParameters: Record<string, unknown>,
    currentPerson: Person | null,
    contextEntities: Record<string, Entity>
};

/**
* This should be called once per block on the page. The config contains configuration provided by the block's server side logic
* counterpart.  This adds the block to the page and allows it to begin initializing.
* @param config
* @param blockComponent
*/
export async function initializeBlock(config: BlockConfig): Promise<App> {
    const blockPath = `${config.blockFileUrl}.js`;
    let blockComponent: Component | null = null;

    try {
        const blockComponentModule = await import(blockPath);
        blockComponent = blockComponentModule ?
            (blockComponentModule.default || blockComponentModule) :
            null;
    }
    catch (e) {
        // Log the error, but continue setting up the app so the UI will show the user an error
        console.error(e);
    }

    const name = `Root${config.blockFileUrl.replace(/\//g, '.')}`;

    const app = createApp({
        name,
        components: {
            RockBlock
        },
        data() {
            return {
                config: config,
                blockComponent: blockComponent ? markRaw(blockComponent) : null
            };
        },
        template: `<RockBlock :config="config" :blockComponent="blockComponent" />`
    });
    app.use(store);
    app.mount(config.rootElement);

    return app;
}

/**
* This should be called once per page with data from the server that pertains to the entire page. This includes things like
* page parameters and context entities.
* @param {object} pageData
*/
export async function initializePage(pageConfig: PageConfig) {
    await store.dispatch('initialize', { pageConfig });
}