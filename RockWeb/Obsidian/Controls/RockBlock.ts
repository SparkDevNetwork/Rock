import { doApiCall } from '../Util/http.js';
import { defineComponent, PropType, provide, reactive } from '../Vendor/Vue/vue.js';
import { BlockConfig, VueComponent } from '../index.js';
import store from '../Store/index.js';

export type HttpResult = { data: unknown };
export type BlockAction = (actionName: string, data: object | undefined) => HttpResult;
export type BlockHttp = {
    get: (url: string, params: object | undefined) => HttpResult;
    post: (url: string, params: object | undefined, data: object | undefined) => HttpResult;
};

type LogItem = {
    date: Date;
    method: string;
    url: string;
};

export default defineComponent({
    name: 'RockBlock',
    props: {
        config: {
            type: Object as PropType<BlockConfig>,
            required: true
        },
        blockComponent: {
            type: Object as PropType<VueComponent>,
            default: null
        }
    },
    setup(props) {
        const log: LogItem[] = reactive([]);

        const writeLog = (method: string, url: string) => {
            log.push({
                date: new Date(),
                method,
                url
            });
        };

        const httpCall = (method: string, url: string, params: object | undefined = undefined, data: object | undefined = undefined) => {
            writeLog(method, url);
            return doApiCall(method, url, params, data);
        };

        const get = (url: string, params: object | undefined = undefined) => {
            return httpCall('GET', url, params);
        };

        const post = (url: string, params: object | undefined = undefined, data: object | undefined = undefined) => {
            return httpCall('POST', url, params, data);
        };

        const blockAction: BlockAction = (actionName: string, data: object | undefined = undefined) => {
            try {
                return post(`/api/blocks/action/${props.config.blockGuid}/${actionName}`, undefined, data);
            }
            catch (e) {
                if (e.response && e.response.data && e.response.data.Message) {
                    throw e.response.data.Message;
                }

                throw e;
            }
        };

        const blockHttp: BlockHttp = { get, post };

        provide('http', blockHttp);
        provide('blockAction', blockAction);
        provide('configurationValues', props.config.configurationValues);
    },
    data() {
        return {
            blockGuid: this.config.blockGuid,
            log: [] as LogItem[]
        };
    },
    computed: {
        pageGuid() {
            return store.state.pageGuid;
        }
    },
    template:
`<div class="obsidian-block">
    <component :is="blockComponent" />
    <div v-if="!blockComponent" class="alert alert-danger">
        Could not find block component: "{{this.config.blockFileIdentifier}}"
    </div>
</div>`
});
