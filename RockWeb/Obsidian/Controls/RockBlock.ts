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
import { doApiCall, HttpBodyData, HttpMethod, HttpResult, HttpUrlParams } from '../Util/Http';
import { Component, defineComponent, inject, PropType, provide, reactive } from 'vue';
import { BlockConfig } from '../Index';
import store, { ReportDebugTimingArgs } from '../Store/Index';
import { Guid } from '../Util/Guid';
import Alert from '../Elements/Alert';

export type InvokeBlockActionFunc = <T>(actionName: string, data?: HttpBodyData) => Promise<HttpResult<T>>;
export type BlockHttpGet = <T>( url: string, params?: HttpUrlParams ) => Promise<HttpResult<T>>;
export type BlockHttpPost = <T>( url: string, params?: HttpUrlParams, data?: HttpBodyData ) => Promise<HttpResult<T>>;

export type BlockHttp = {
    get: BlockHttpGet;
    post: BlockHttpPost;
};

type LogItem = {
    date: Date;
    method: HttpMethod;
    url: string;
};

export function standardBlockSetup()
{
    return {
        configurationValues: inject( 'configurationValues' ) as { Message: string; },
        invokeBlockAction: inject( 'invokeBlockAction' ) as InvokeBlockActionFunc
    };
}

export default defineComponent( {
    name: 'RockBlock',
    components: {
        Alert
    },
    props: {
        config: {
            type: Object as PropType<BlockConfig>,
            required: true
        },
        blockComponent: {
            type: Object as PropType<Component>,
            default: null
        },
        startTimeMs: {
            type: Number as PropType<number>,
            required: true
        }
    },
    setup( props )
    {
        const log: LogItem[] = reactive( [] );

        const writeLog = ( method: HttpMethod, url: string ) =>
        {
            log.push( {
                date: new Date(),
                method,
                url
            } );
        };

        const httpCall = async <T>( method: HttpMethod, url: string, params: HttpUrlParams = undefined, data: HttpBodyData = undefined ) =>
        {
            writeLog( method, url );
            return await doApiCall<T>( method, url, params, data );
        };

        const get = async <T>( url: string, params: HttpUrlParams = undefined ) =>
        {
            return await httpCall<T>( 'GET', url, params );
        };

        const post = async <T>( url: string, params: HttpUrlParams = undefined, data: HttpBodyData = undefined ) =>
        {
            return await httpCall<T>( 'POST', url, params, data );
        };

        const invokeBlockAction: InvokeBlockActionFunc = async <T>( actionName: string, data: HttpBodyData = undefined ) =>
        {
            return await post<T>( `/api/blocks/action/${props.config.blockGuid}/${actionName}`, undefined, {
                __context: {
                    pageParameters: store.state.pageParameters
                },
                ...data
            } );
        };

        const blockHttp: BlockHttp = { get, post };

        provide( 'http', blockHttp );
        provide( 'invokeBlockAction', invokeBlockAction );
        provide( 'configurationValues', props.config.configurationValues );
    },
    data()
    {
        return {
            blockGuid: this.config.blockGuid,
            error: '',
            finishTimeMs: null as null | number
        };
    },
    methods: {
        clearError()
        {
            this.error = '';
        }
    },
    computed: {
        renderTimeMs(): number | null
        {
            if ( !this.finishTimeMs || !this.startTimeMs )
            {
                return null;
            }

            return this.finishTimeMs - this.startTimeMs;
        },
        pageGuid(): Guid
        {
            return store.state.pageGuid;
        }
    },
    errorCaptured( err: unknown )
    {
        const defaultMessage = 'An unknown error was caught from the block.';

        if ( err instanceof Error )
        {
            this.error = err.message || defaultMessage;
        }
        else if ( err )
        {
            this.error = JSON.stringify( err ) || defaultMessage;
        }
        else
        {
            this.error = defaultMessage;
        }
    },
    mounted()
    {
        this.finishTimeMs = ( new Date() ).getTime();
        const componentName = this.blockComponent?.name || '';
        const nameParts = componentName.split( '.' );
        let subtitle = nameParts[ 0 ] || '';

        if ( subtitle && subtitle.indexOf( '(' ) !== 0 )
        {
            subtitle = `(${subtitle})`;
        }

        if ( nameParts.length )
        {
            store.commit( 'reportOnLoadDebugTiming', {
                Title: nameParts[ 1 ] || '<Unnamed>',
                Subtitle: subtitle,
                StartTimeMs: this.startTimeMs,
                FinishTimeMs: this.finishTimeMs
            } as ReportDebugTimingArgs );
        }
    },
    template: `
<div class="obsidian-block">
    <Alert v-if="!blockComponent" alertType="danger">
        <strong>Not Found</strong>
        Could not find block component: "{{this.config.blockFileUrl}}"
    </Alert>
    <Alert v-if="error" alertType="danger" :dismissible="true" @dismiss="clearError">
        <strong>Uncaught Error</strong>
        {{error}}
    </Alert>
    <component :is="blockComponent" />
</div>`
} );
