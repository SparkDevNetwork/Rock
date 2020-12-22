import RockBlock from './Controls/RockBlock.js'
import store from './Store/index.js'

export type VueComponent = {
    name: string;
    [key: string]: string | object | Function;
}

export type BlockConfig = {
    blockFileIdentifier: string;
    rootElement: Element;
    blockGuid: string;
    configurationValues: { [key: string]: string | number | null | object };
};

/**
* This should be called once per block on the page. The config contains configuration provided by the block's server side logic
* counterpart.  This adds the block to the page and allows it to begin initializing.
* @param config
* @param blockComponent
*/
export function initializeBlock(config: BlockConfig, blockComponent: VueComponent | null) {
    // eslint-disable-next-line
    // @ts-ignore
    Vue.createApp({
        name: `Root.${config.blockFileIdentifier}`,
        components: {
            RockBlock
        },
        data() {
            return {
                config: config,
                // eslint-disable-next-line
                // @ts-ignore
                blockComponent: blockComponent ? Vue.markRaw(blockComponent) : null
            };
        },
        template: `<RockBlock :config="config" :blockComponent="blockComponent" />`
    })
    .use(store)
    .mount(config.rootElement);
}

/**
* This should be called once per page with data from the server that pertains to the entire page. This includes things like
* page parameters and context entities.
* @param {object} pageData
*/
export async function initializePage(pageData) {
    await store.dispatch('initialize', { pageData });
}