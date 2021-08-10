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
import { defineComponent, PropType } from 'vue';
import LoadingIndicator from '../Elements/LoadingIndicator';
import { ValidationField } from './GatewayControl';

type Settings = {
    PublicApiKey: string;
    GatewayUrl: string;
};

type Tokenizer = {
    create: () => void;
    submit: () => void;
};

interface Response {
    status: 'success' | 'error' | 'validation';
}

interface SuccessResponse extends Response {
    token: string;
}

interface ErrorResponse extends Response {
    message: string;
}

interface ValidationResponse extends Response {
    invalid: string[];
}

export default defineComponent( {
    name: 'MyWellGatewayControl',
    components: {
        LoadingIndicator
    },
    props: {
        settings: {
            type: Object as PropType<Settings>,
            required: true
        },
        submit: {
            type: Boolean as PropType<boolean>,
            required: true
        }
    },
    data ()
    {
        return {
            tokenizer: null as Tokenizer | null,
            loading: true
        };
    },
    methods: {
        async mountControl ()
        {
            const globalVarName = 'Tokenizer';

            if ( !window[ globalVarName ] )
            {
                const script = document.createElement( 'script' );
                script.type = 'text/javascript';
                script.src = 'https://sandbox.gotnpgateway.com/tokenizer/tokenizer.js'; // TODO - this should come from the gateway
                document.getElementsByTagName( 'head' )[ 0 ].appendChild( script );

                const sleep = () => new Promise( ( resolve ) => setTimeout( resolve, 20 ) );

                while ( !window[ globalVarName ] )
                {
                    await sleep();
                }
            }

            const settings = this.getTokenizerSettings();
            this.tokenizer = new window[ globalVarName ]( settings ) as Tokenizer;
            this.tokenizer.create();
        },
        handleResponse ( response: Response | null | undefined )
        {
            this.loading = false;

            if ( !response?.status || response.status === 'error' )
            {
                const errorResponse = ( response as ErrorResponse | null ) || null;
                this.$emit( 'error', errorResponse?.message || 'There was an unexpected problem communicating with the gateway.' );
                console.error( 'MyWell response was errored:', JSON.stringify( response ) );
                return;
            }

            if ( response.status === 'validation' )
            {
                const validationResponse = ( response as ValidationResponse | null ) || null;

                if ( !validationResponse?.invalid?.length )
                {
                    this.$emit( 'error', 'There was a validation issue, but the invalid field was not specified.' );
                    console.error( 'MyWell response was errored:', JSON.stringify( response ) );
                    return;
                }

                const validationFields: ValidationField[] = [];

                for ( const myWellField of validationResponse.invalid )
                {
                    switch ( myWellField )
                    {
                        case 'cc':
                            validationFields.push( ValidationField.CardNumber );
                            break;
                        case 'exp':
                            validationFields.push( ValidationField.Expiry );
                            break;
                        default:
                            console.error( 'Unknown MyWell validation field', myWellField );
                            break;
                    }
                }

                if ( !validationFields.length )
                {
                    this.$emit( 'error', 'There was a validation issue, but the invalid field could not be inferred.' );
                    console.error( 'MyWell response contained unexpected values:', JSON.stringify( response ) );
                    return;
                }

                this.$emit( 'validationRaw', validationFields );
                return;
            }

            if ( response.status === 'success' )
            {
                const successResponse = ( response as SuccessResponse | null ) || null;

                if ( !successResponse?.token )
                {
                    this.$emit( 'error', 'There was an unexpected problem communicating with the gateway.' );
                    console.error( 'MyWell response does not have the expected token:', JSON.stringify( response ) );
                    return;
                }

                this.$emit( 'successRaw', successResponse.token );
                return;
            }

            this.$emit( 'error', 'There was an unexpected problem communicating with the gateway.' );
            console.error( 'MyWell response has invalid status:', JSON.stringify( response ) );
        },

        /** Generates the tokenizer settings */
        getTokenizerSettings (): unknown
        {
            return {
                onLoad: () => { this.loading = false; },
                apikey: this.publicApiKey,
                url: this.gatewayUrl,
                container: this.$refs[ 'container' ],
                submission: ( resp: Response ) =>
                {
                    this.handleResponse( resp );
                },
                settings: {
                    payment: {
                        types: [ 'card' ],
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
    computed: {
        publicApiKey (): string
        {
            return this.settings.PublicApiKey;
        },
        gatewayUrl (): string
        {
            return this.settings.GatewayUrl;
        }
    },
    watch: {
        submit ()
        {
            if ( this.submit && this.tokenizer )
            {
                this.loading = true;
                this.tokenizer.submit();
            }
        }
    },
    async mounted ()
    {
        await this.mountControl();
    },
    template: `
<div>
    <div ref="container" style="min-height: 49px;"></div>
    <div v-if="loading" class="text-center">
        <LoadingIndicator />
    </div>
</div>`
} );
