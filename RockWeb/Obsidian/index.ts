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
import { App, Component, createApp, markRaw } from 'vue';
import RockBlock from './Controls/RockBlock';
import store from './Store/Index';
import { Guid } from './Util/Guid';
import './Rules/Index';
import Person from './ViewModels/CodeGenerated/PersonViewModel';
import Entity from './ViewModels/Entity';
import PageDebugTimings, { DebugTimingViewModel } from './Controls/PageDebugTimings';
import Alert from './Elements/Alert';

export type ConfigurationValues = Record<string, unknown>;

export type BlockConfig = {
    blockFileUrl: string;
    rootElement: Element;
    blockGuid: Guid;
    configurationValues: ConfigurationValues;
};

export type PageConfig = {
    executionStartTime: Date;
    pageId: number;
    pageGuid: Guid;
    pageParameters: Record<string, unknown>,
    currentPerson: Person | null,
    contextEntities: Record<string, Entity>,
    loginUrlWithReturnUrl: string
};

type DebugTimingConfig = {
    elementId: string;
    debugTimingViewModels: DebugTimingViewModel[]
};

/**
* This should be called once per block on the page. The config contains configuration provided by the block's server side logic
* counterpart.  This adds the block to the page and allows it to begin initializing.
* @param config
* @param blockComponent
*/
export async function initializeBlock ( config: BlockConfig ): Promise<App>
{
    const blockPath = `${config.blockFileUrl}.js`;
    let blockComponent: Component | null = null;
    let errorMessage = '';

    try
    {
        const blockComponentModule = await import( blockPath );
        blockComponent = blockComponentModule ?
            ( blockComponentModule.default || blockComponentModule ) :
            null;
    }
    catch ( e )
    {
        // Log the error, but continue setting up the app so the UI will show the user an error
        console.error( e );
        errorMessage = `${e}`;
    }

    const name = `Root${config.blockFileUrl.replace( /\//g, '.' )}`;
    const startTimeMs = ( new Date() ).getTime();

    const app = createApp( {
        name,
        components: {
            RockBlock,
            Alert
        },
        data ()
        {
            return {
                config: config,
                blockComponent: blockComponent ? markRaw( blockComponent ) : null,
                startTimeMs,
                errorMessage
            };
        },
        template: `
<Alert v-if="errorMessage" alertType="danger">
    <strong>Error Initializing Block</strong>
    <br />
    {{errorMessage}}
</Alert>
<RockBlock v-else :config="config" :blockComponent="blockComponent" :startTimeMs="startTimeMs" />`
    } );

    app.use( store );
    app.mount( config.rootElement );

    return app;
}

/**
* This should be called once per page with data from the server that pertains to the entire page. This includes things like
* page parameters and context entities.
* @param {object} pageData
*/
export async function initializePage ( pageConfig: PageConfig )
{
    await store.dispatch( 'initialize', { pageConfig } );
}

/**
 * Shows the Obsidian debug timings
 * @param debugTimingConfig
 */
export function initializePageTimings ( config: DebugTimingConfig )
{
    const rootElement = document.getElementById( config.elementId );

    if ( !rootElement )
    {
        console.error( 'Could not show Obsidian debug timings because the HTML element did not resolve.' );
        return;
    }

    const app = createApp( {
        name: 'PageDebugTimingsRoot',
        components: {
            PageDebugTimings
        },
        data ()
        {
            return {
                viewModels: config.debugTimingViewModels
            };
        },
        template: `<PageDebugTimings :serverViewModels="viewModels" />`
    } );
    app.use( store );
    app.mount( rootElement );
}