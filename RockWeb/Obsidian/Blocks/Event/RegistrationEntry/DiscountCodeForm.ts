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

import { defineComponent, inject } from 'vue';
import { InvokeBlockActionFunc } from '../../../Controls/RockBlock';
import Alert from '../../../Elements/Alert';
import RockButton from '../../../Elements/RockButton';
import TextBox from '../../../Elements/TextBox';
import { asFormattedString } from '../../../Services/Number';
import { RegistrationEntryState } from '../RegistrationEntry';
import {  RegistrationEntryBlockViewModel } from './RegistrationEntryBlockViewModel';

type CheckDiscountCodeResult = {
    DiscountCode: string;
    UsagesRemaining: number | null;
    DiscountAmount: number;
    DiscountPercentage: number;
};

export default defineComponent( {
    name: 'Event.RegistrationEntry.DiscountCodeForm',
    components: {
        RockButton,
        TextBox,
        Alert
    },
    setup ()
    {
        return {
            invokeBlockAction: inject( 'invokeBlockAction' ) as InvokeBlockActionFunc,
            registrationEntryState: inject( 'registrationEntryState' ) as RegistrationEntryState
        };
    },
    data ()
    {
        return {
            /** Is there an AJAX call in-flight? */
            loading: false,

            /** The bound value to the discount code input */
            discountCodeInput: '',

            /** A warning message about the discount code that is a result of a failed AJAX call */
            discountCodeWarningMessage: ''
        };
    },
    computed: {
        /** The success message displayed once a discount code has been applied */
        discountCodeSuccessMessage (): string
        {
            const discountAmount = this.registrationEntryState.DiscountAmount;
            const discountPercent = this.registrationEntryState.DiscountPercentage;

            if ( !discountPercent && !discountAmount )
            {
                return '';
            }

            const discountText = discountPercent ?
                `${asFormattedString( discountPercent * 100, 0 )}%` :
                `$${asFormattedString( discountAmount, 2 )}`;

            return `Your ${discountText} discount code for all registrants was successfully applied.`;
        },

        /** Should the discount panel be shown? */
        isDiscountPanelVisible (): boolean
        {
            return this.viewModel.hasDiscountsAvailable;
        },

        /** This is the data sent from the C# code behind when the block initialized. */
        viewModel (): RegistrationEntryBlockViewModel
        {
            return this.registrationEntryState.ViewModel;
        }
    },
    methods: {
        /** Send a user input discount code to the server so the server can check and send back
         *  the discount amount. */
        async tryDiscountCode ()
        {
            this.loading = true;

            try
            {
                const result = await this.invokeBlockAction<CheckDiscountCodeResult>( 'CheckDiscountCode', {
                    code: this.discountCodeInput
                } );

                if ( result.isError || !result.data )
                {
                    this.discountCodeWarningMessage = `'${this.discountCodeInput}' is not a valid Discount Code.`;
                }
                else
                {
                    this.discountCodeWarningMessage = '';
                    this.registrationEntryState.DiscountAmount = result.data.DiscountAmount;
                    this.registrationEntryState.DiscountPercentage = result.data.DiscountPercentage;
                    this.registrationEntryState.DiscountCode = result.data.DiscountCode;
                }
            }
            finally
            {
                this.loading = false;
            }
        }
    },
    watch: {
        'registrationEntryState.DiscountCode': {
            immediate: true,
            handler ()
            {
                this.discountCodeInput = this.registrationEntryState.DiscountCode;
            }
        }
    },
    template: `
<div v-if="isDiscountPanelVisible || discountCodeInput" class="clearfix">
    <Alert v-if="discountCodeWarningMessage" alertType="warning">{{discountCodeWarningMessage}}</Alert>
    <Alert v-if="discountCodeSuccessMessage" alertType="success">{{discountCodeSuccessMessage}}</Alert>
    <div class="form-group pull-right">
        <label class="control-label">Discount Code</label>
        <div class="input-group">
            <input type="text" :disabled="loading || !!discountCodeSuccessMessage" class="form-control input-width-md input-sm" v-model="discountCodeInput" />
            <RockButton v-if="!discountCodeSuccessMessage" btnSize="sm" :isLoading="loading" class="margin-l-sm" @click="tryDiscountCode">
                Apply
            </RockButton>
        </div>
    </div>
</div>`
} );