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

import { Guid } from "@Obsidian/Types";
import { doApiCall, HttpBodyData, HttpMethod, HttpResult, HttpUrlParams } from "./Util/http";
import { Component, defineComponent, PropType, provide, reactive } from "vue";
import { BlockConfig, BlockHttp, InvokeBlockActionFunc } from "./Util/block";
import { useStore } from "./Store/index";
import { RockDateTime } from "./Util/rockDateTime";

type LogItem = {
    date: RockDateTime;
    method: HttpMethod;
    url: string;
};

const store = useStore();

export default defineComponent({
    name: "RockBlock",
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
    setup(props) {
        const log: LogItem[] = reactive([]);

        const writeLog = (method: HttpMethod, url: string): void => {
            log.push({
                date: RockDateTime.now(),
                method,
                url
            });
        };

        const httpCall = async <T>(method: HttpMethod, url: string, params: HttpUrlParams = undefined, data: HttpBodyData = undefined): Promise<HttpResult<T>> => {
            writeLog(method, url);
            return await doApiCall<T>(method, url, params, data);
        };

        const get = async <T>(url: string, params: HttpUrlParams = undefined): Promise<HttpResult<T>> => {
            return await httpCall<T>("GET", url, params);
        };

        const post = async <T>(url: string, params: HttpUrlParams = undefined, data: HttpBodyData = undefined): Promise<HttpResult<T>> => {
            return await httpCall<T>("POST", url, params, data);
        };

        const invokeBlockAction: InvokeBlockActionFunc = async <T>(actionName: string, data: HttpBodyData = undefined) => {
            return await post<T>(`/api/v2/BlockActions/${store.state.pageGuid}/${props.config.blockGuid}/${actionName}`, undefined, {
                __context: {
                    pageParameters: store.state.pageParameters
                },
                ...data
            });
        };

        const blockHttp: BlockHttp = { get, post };

        provide("http", blockHttp);
        provide("invokeBlockAction", invokeBlockAction);
        provide("configurationValues", props.config.configurationValues);
    },
    data() {
        return {
            blockGuid: this.config.blockGuid,
            error: "",
            finishTimeMs: null as null | number
        };
    },
    methods: {
        clearError() {
            this.error = "";
        }
    },
    computed: {
        renderTimeMs(): number | null {
            if (!this.finishTimeMs || !this.startTimeMs) {
                return null;
            }

            return this.finishTimeMs - this.startTimeMs;
        },
        pageGuid(): Guid {
            return store.state.pageGuid;
        }
    },
    errorCaptured(err: unknown) {
        const defaultMessage = "An unknown error was caught from the block.";

        if (err instanceof Error) {
            this.error = err.message || defaultMessage;
        }
        else if (err) {
            this.error = JSON.stringify(err) || defaultMessage;
        }
        else {
            this.error = defaultMessage;
        }
    },
    mounted() {
        this.finishTimeMs = RockDateTime.now().toMilliseconds();
        const componentName = this.blockComponent?.name || "";
        const nameParts = componentName.split(".");
        let subtitle = nameParts[ 0 ] || "";

        if (subtitle && subtitle.indexOf("(") !== 0) {
            subtitle = `(${subtitle})`;
        }

        if (nameParts.length) {
            store.addPageDebugTiming({
                title: nameParts[ 1 ] || "<Unnamed>",
                subtitle: subtitle,
                startTimeMs: this.startTimeMs,
                finishTimeMs: this.finishTimeMs
            });
        }
    },

    // Note: We are using a custom alert so there is no dependency on the
    // Controls package.
    template: `
<div class="obsidian-block">
    <div v-if="!blockComponent" class="alert alert-danger">
        <strong>Not Found</strong>
        Could not find block component: "{{this.config.blockFileUrl}}"
    </div>
    <div v-if="error" class="alert alert-danger">
        <strong>Uncaught Error</strong>
        {{error}}
    </div>
    <component :is="blockComponent" />
</div>`
});
