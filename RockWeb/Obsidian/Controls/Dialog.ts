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
import RockButton from '../Elements/RockButton';

export enum ValidationField
{
    CardNumber,
    Expiry,
    SecurityCode
}

export default defineComponent( {
    name: 'Dialog',
    components: {
        RockButton
    },
    props: {
        modelValue: {
            type: Boolean as PropType<boolean>,
            required: true
        },
        dismissible: {
            type: Boolean as PropType<boolean>,
            default: true
        }
    },
    data ()
    {
        return {
            doShake: false
        };
    },
    computed: {
        hasHeader (): boolean
        {
            return !!this.$slots[ 'header' ];
        }
    },
    methods: {
        close ()
        {
            this.$emit( 'update:modelValue', false );
        },
        shake ()
        {
            if ( !this.doShake )
            {
                this.doShake = true;
                setTimeout( () => this.doShake = false, 1000 );
            }
        },
        centerOnScreen ()
        {
            this.$nextTick( () =>
            {
                const div = this.$refs[ 'modalDiv' ] as HTMLElement | null;

                if ( !div )
                {
                    return;
                }

                const height = div.offsetHeight;
                const margin = height / 2;
                div.style.marginTop = `-${margin}px`;
            } );
        }
    },
    watch: {
        modelValue: {
            immediate: true,
            handler ()
            {
                const body = document.body;
                const cssClasses = [ 'modal-open', 'page-overflow' ];

                if ( this.modelValue )
                {
                    for ( const cssClass of cssClasses )
                    {
                        body.classList.add( cssClass );
                    }

                    this.centerOnScreen();
                }
                else
                {
                    for ( const cssClass of cssClasses )
                    {
                        body.classList.remove( cssClass );
                    }
                }
            }
        }
    },
    template: `
<div v-if="modelValue">
    <div @click="shake" class="modal-scrollable" style="z-index: 1060;">
        <div @click.stop ref="modalDiv" class="modal fade in" :class="{'animated shake': doShake}" tabindex="-1" role="dialog" style="display: block;">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div v-if="hasHeader" class="modal-header">
                        <button v-if="dismissible" @click="close" type="button" class="close" style="margin-top: -10px;">×</button>
                        <slot name="header" />
                    </div>
                    <div class="modal-body">
                        <button v-if="!hasHeader && dismissible" @click="close" type="button" class="close" style="margin-top: -10px;">×</button>
                        <slot />
                    </div>
                    <div v-if="$slots.footer" class="modal-footer">
                        <slot name="footer" />
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="modal-backdrop fade in" style="z-index: 1050;"></div>
</div>`
} );
