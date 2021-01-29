import { doApiCall, HttpBodyData, HttpMethod, HttpResult, HttpUrlParams } from '../Util/http.js';
import { Component, defineComponent, PropType, provide, reactive } from '../Vendor/Vue/vue.js';
import { BlockConfig } from '../Index.js';
import store from '../Store/Index.js';
import { Guid } from '../Util/Guid.js';
import Alert from '../Elements/Alert.js';

export type InvokeBlockActionFunc = <T>(actionName: string, data?: HttpBodyData) => Promise<HttpResult<T>>;

export type BlockHttp = {
    get: <T>(url: string, params?: HttpUrlParams) => Promise<HttpResult<T>>;
    post: <T>(url: string, params?: HttpUrlParams, data?: HttpBodyData) => Promise<HttpResult<T>>;
};

type LogItem = {
    date: Date;
    method: HttpMethod;
    url: string;
};

export default defineComponent({
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
        }
    },
    setup(props) {
        const log: LogItem[] = reactive([]);

        const writeLog = (method: HttpMethod, url: string) => {
            log.push({
                date: new Date(),
                method,
                url
            });
        };

        const httpCall = async <T>(method: HttpMethod, url: string, params: HttpUrlParams = undefined, data: HttpBodyData = undefined) => {
            writeLog(method, url);
            return await doApiCall<T>(method, url, params, data);
        };

        const get = async <T>(url: string, params: HttpUrlParams = undefined) => {
            return await httpCall<T>('GET', url, params);
        };

        const post = async <T>(url: string, params: HttpUrlParams = undefined, data: HttpBodyData = undefined) => {
            return await httpCall<T>('POST', url, params, data);
        };

        const invokeBlockAction: InvokeBlockActionFunc = async <T>(actionName: string, data: HttpBodyData = undefined) => {
            return await post<T>(`/api/blocks/action/${props.config.blockGuid}/${actionName}`, undefined, data);
        };

        const blockHttp: BlockHttp = { get, post };

        provide('http', blockHttp);
        provide('invokeBlockAction', invokeBlockAction);
        provide('configurationValues', props.config.configurationValues);
    },
    data() {
        return {
            blockGuid: this.config.blockGuid,
            error: ''
        };
    },
    methods: {
        clearError() {
            this.error = '';
        }
    },
    computed: {
        pageGuid(): Guid {
            return store.state.pageGuid;
        }
    },
    errorCaptured(err: unknown) {
        if (err instanceof Error) {
            this.error = err.message;
        }
    },
    template:
`<div class="obsidian-block">
    <Alert v-if="!blockComponent" class="alert-danger">
        <strong>Not Found</strong>
        Could not find block component: "{{this.config.blockFileUrl}}"
    </Alert>
    <Alert v-if="error" :dismissible="true" @dismiss="clearError" class="alert-danger">
        <strong>Uncaught Error</strong>
        {{error}}
    </Alert>
    <component :is="blockComponent" />
</div>`
});
