import { doApiCall } from '../Util/http.js';

export default {
    name: 'RockBlock',
    props: {
        config: {
            type: Object,
            required: true
        },
        blockComponent: {
            type: Object,
            default: null
        }
    },
    provide() {
        return {
            http: {
                get: this.httpGet,
                post: this.httpPost
            },
            blockAction: this.blockAction,
            configurationValues: this.config.configurationValues
        };
    },
    data() {
        return {
            blockGuid: this.config.blockGuid,
            log: []
        };
    },
    computed: {
        pageGuid() {
            return this.$store.state.pageGuid;
        }
    },
    methods: {
        httpCall(method, url, params, data) {
            this.log.push({
                method,
                timestamp: new Date(),
                url
            });

            return doApiCall(method, url, params, data);
        },
        httpGet(url, params) {
            return this.httpCall('GET', url, params);
        },
        httpPost(url, params, data) {
            return this.httpCall('POST', url, params, data);
        },
        blockAction(actionName, data) {
            try {
                return this.httpPost(`/api/blocks/action/${this.blockGuid}/${actionName}`, undefined, data);
            }
            catch (e) {
                if (e.response && e.response.data && e.response.data.Message) {
                    throw e.response.data.Message;
                }

                throw e;
            }
        }
    },
    template:
`<div class="obsidian-block">
    <component :is="blockComponent" />
    <div v-if="!blockComponent" class="alert alert-danger">
        Could not find block component: "{{this.config.blockFileIdentifier}}"
    </div>
</div>`
};
