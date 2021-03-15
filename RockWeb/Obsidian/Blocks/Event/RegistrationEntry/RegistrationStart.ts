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

import { defineComponent, inject, PropType } from 'vue';
import AttributeValuesContainer from '../../../Controls/AttributeValuesContainer';
import ProgressBar from '../../../Elements/ProgressBar';
import RockButton from '../../../Elements/RockButton';
import { Guid } from '../../../Util/Guid';
import AttributeValue from '../../../ViewModels/CodeGenerated/AttributeValueViewModel';
import { RegistrationEntryBlockViewModel } from './RegistrationEntryBlockViewModel';

export default defineComponent({
    name: 'Event.RegistrationEntry.RegistrationStart',
    components: {
        RockButton,
        ProgressBar,
        AttributeValuesContainer
    },
    setup() {
        return {
            viewModel: inject('configurationValues') as RegistrationEntryBlockViewModel
        };
    },
    props: {
        registrantCount: {
            type: Number as PropType<number>,
            required: true
        },
        numberOfPages: {
            type: Number as PropType<number>,
            required: true
        },
        registrationFieldValues: {
            type: Object as PropType<Record<Guid, unknown>>,
            required: true
        }
    },
    data() {
        return {
            attributeValues: [] as AttributeValue[]
        };
    },
    computed: {
        completionPercentDecimal(): number {
            return 1 / this.numberOfPages;
        },
        completionPercentInt(): number {
            return this.completionPercentDecimal * 100;
        }
    },
    methods: {
        onPrevious() {
            this.$emit('previous');
        },
        onNext() {
            this.$emit('next');
        }
    },
    watch: {
        viewModel: {
            immediate: true,
            handler() {
                this.attributeValues = this.viewModel.RegistrationAttributesStart.map(a => ({
                    Attribute: a,
                    AttributeId: a.Id,
                    Value: ''
                } as AttributeValue));
            }
        },
        attributeValues: {
            immediate: true,
            deep: true,
            handler() {
                for (const attributeValue of this.attributeValues) {
                    const attribute = attributeValue.Attribute;

                    if (attribute) {
                        this.registrationFieldValues[attribute.Guid] = attributeValue.Value;
                    }
                }
            }
        }
    },
    template: `
<div class="registrationentry-registration-attributes">
    <h1>{{viewModel.RegistrationAttributeTitleStart}}</h1>
    <ProgressBar :percent="completionPercentInt" />

    <AttributeValuesContainer :attributeValues="attributeValues" isEditMode />

    <div class="actions">
        <RockButton btnType="default" @click="onPrevious">
            Previous
        </RockButton>
        <RockButton btnType="primary" class="pull-right" @click="onNext">
            Next
        </RockButton>
    </div>
</div>`
});