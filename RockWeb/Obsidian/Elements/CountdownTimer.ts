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

/** The type of the alert box to display. Ex: 'success' will appear green and as if something good happened. */
export enum AlertType {
    default = 'default',
    success = 'success',
    info = 'info',
    danger = 'danger',
    warning = 'warning',
    primary = 'primary',
    validation = 'validation'
}

/** Displays a countdown and decremements the seconds. */
const CountdownTimer = defineComponent( {
    name: 'CountdownTimer',
    props: {
        /** Seconds until 0:00 */
        modelValue: {
            type: Number as PropType<number>,
            required: true
        }
    },
    data ()
    {
        return {
            handle: null as null | number
        };
    },
    computed: {
        timeString (): string
        {
            const minutes = Math.floor( this.modelValue / 60 );
            const seconds = Math.floor( this.modelValue % 60 );
            return `${minutes}:${seconds < 10 ? '0' + seconds : seconds}`;
        },
    },
    methods: {
        onInterval ()
        {
            if ( this.modelValue <= 0 )
            {
                this.$emit( 'update:modelValue', 0 );
                return;
            }

            this.$emit( 'update:modelValue', Math.floor( this.modelValue - 1 ) );
        }
    },
    mounted ()
    {
        if ( this.handle )
        {
            clearInterval( this.handle );
        }

        this.handle = setInterval( () => this.onInterval(), 1000 ) as unknown as number;
    },
    unmounted ()
    {
        if ( this.handle )
        {
            clearInterval( this.handle );
            this.handle = null;
        }
    },
    template: `
<span>{{timeString}}</span>`
} );

export default CountdownTimer;
