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
import { ProcessTransactionArgs } from '../Blocks/Finance/TransactionEntry';
import { defineComponent, PropType } from 'vue';

type Settings = {
    PublicApiKey: string;
    GatewayUrl: string;
};

type Tokenizer = {
    create: () => void;
    submit: () => void;
};

type Response = {
    token: string;
};

export default defineComponent({
    name: 'MyWellGatewayControl',
    props: {
        settings: {
            type: Object as PropType<Settings>,
            required: true
        },
        submit: {
            type: Boolean as PropType<boolean>,
            required: true
        },
        args: {
            type: Object as PropType<ProcessTransactionArgs>,
            required: true
        }
    },
    data() {
        return {
            tokenizer: null as Tokenizer | null,
            token: '' as string
        };
    },
    methods: {
        handleResponse(resp: Response) {
            this.token = resp.token;
            this.args.ReferenceNumber = this.token;
            this.$emit('done');
        }
    },
    computed: {
        publicApiKey(): string {
            return this.settings.PublicApiKey;
        },
        gatewayUrl(): string {
            return this.settings.GatewayUrl;
        },
        tokenizerSettings(): unknown {
            return {
                apikey: this.publicApiKey,
                url: this.gatewayUrl,
                container: this.$refs['container'],
                submission: (resp: Response) => {
                    this.handleResponse(resp);
                },
                settings: {
                    payment: {
                        types: ['card'],
                        ach: {
                            'sec_code': 'web'
                        }
                    },
                    styles: {
                        body: {
                            color: 'rgb(51, 51, 51)'
                        },
                        '#app': {
                            padding: '5px 15px'
                        },
                        'input,select': {
                            'color': 'rgb(85, 85, 85)',
                            'border-radius': '4px',
                            'background-color': 'rgb(255, 255, 255)',
                            'border': '1px solid rgb(204, 204, 204)',
                            'box-shadow': 'rgba(0, 0, 0, 0.075) 0px 1px 1px 0px inset',
                            'padding': '6px 12px',
                            'font-size': '14px',
                            'height': '34px',
                            'font-family': 'OpenSans, \'Helvetica Neue\', Helvetica, Arial, sans-serif'
                        },
                        'input:focus,select:focus': {
                            'border': '1px solid #66afe9',
                            'box-shadow': '0 0 0 3px rgba(102,175,233,0.6)'
                        },
                        'select': {
                            'padding': '6px 4px'
                        },
                        '.fieldsetrow': {
                            'margin-left': '-2.5px',
                            'margin-right': '-2.5px'
                        },
                        '.card > .fieldset': {
                            'padding': '0 !important',
                            'margin': '0 2.5px 5px !important'
                        },
                        'input[type=number]::-webkit-inner-spin-button,input[type=number]::-webkit-outer-spin-button': {
                            '-webkit-appearance': 'none',
                            'margin': '0'
                        }
                    }
                }
            };
        }
    },
    watch: {
        submit() {
            if (!this.token && this.submit && this.tokenizer) {
                this.tokenizer.submit();
            }
        }
    },
    mounted() {
        this.tokenizer = new window['Tokenizer'](this.tokenizerSettings) as Tokenizer;
        this.tokenizer.create();
    },
    template: `
<div v-if="!token">
    <div ref="container"></div>
</div>`
});
