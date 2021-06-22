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
import AttributeValuesContainer from '../../../Controls/AttributeValuesContainer';
import RockForm from '../../../Controls/RockForm';
import RockButton from '../../../Elements/RockButton';
import AttributeValue from '../../../ViewModels/CodeGenerated/AttributeValueViewModel';
import { RegistrationEntryState } from '../RegistrationEntry';

export default defineComponent( {
    name: 'Event.RegistrationEntry.RegistrationEnd',
    components: {
        RockButton,
        AttributeValuesContainer,
        RockForm
    },
    setup()
    {
        return {
            registrationEntryState: inject( 'registrationEntryState' ) as RegistrationEntryState
        };
    },
    data()
    {
        return {
            attributeValues: [] as AttributeValue[]
        };
    },
    methods: {
        onPrevious()
        {
            this.$emit( 'previous' );
        },
        onNext()
        {
            this.$emit( 'next' );
        }
    },
    watch: {
        viewModel: {
            immediate: true,
            handler()
            {
                this.attributeValues = this.registrationEntryState.ViewModel.RegistrationAttributesEnd.map( a =>
                {
                    const currentValue = this.registrationEntryState.RegistrationFieldValues[ a.Guid ] || '';

                    return {
                        Attribute: a,
                        AttributeId: a.Id,
                        Value: currentValue
                    } as AttributeValue;
                } );
            }
        },
        attributeValues: {
            immediate: true,
            deep: true,
            handler()
            {
                for ( const attributeValue of this.attributeValues )
                {
                    const attribute = attributeValue.Attribute;

                    if ( attribute )
                    {
                        this.registrationEntryState.RegistrationFieldValues[ attribute.Guid ] = attributeValue.Value;
                    }
                }
            }
        }
    },
    template: `
<div class="registrationentry-registration-attributes">
    <RockForm @submit="onNext">
        <AttributeValuesContainer :attributeValues="attributeValues" isEditMode />

        <div class="actions">
            <RockButton btnType="default" @click="onPrevious">
                Previous
            </RockButton>
            <RockButton btnType="primary" class="pull-right" type="submit">
                Next
            </RockButton>
        </div>
    </RockForm>
</div>`
} );