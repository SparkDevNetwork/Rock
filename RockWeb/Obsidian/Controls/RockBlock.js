Obsidian.Controls.RockBlock = {
    props: {
        config: {
            type: Object,
            required: true
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
            log: [],
            blockComponent: Obsidian.Blocks[this.config.blockFileIdentifier]
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

            return axios({
                method,
                url,
                data,
                params
            });
        },
        httpGet(url, params) {
            return this.httpCall('GET', url, params);
        },
        httpPost(url, params, data) {
            return this.httpCall('POST', url, params, data);
        },
        blockAction(actionName, data) {
            try {
                return this.httpPost(`/api/blocks/action/${this.pageGuid}/${this.blockGuid}/${actionName}`, undefined, data);
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
        Could not find JS block component: "{{this.config.blockFileIdentifier}}"
    </div>
</div>`
};
