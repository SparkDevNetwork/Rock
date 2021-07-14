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
import DropDownList, { DropDownListOption } from '../../../Elements/DropDownList';
import RadioButtonList from '../../../Elements/RadioButtonList';
import Person from '../../../ViewModels/CodeGenerated/PersonViewModel';
import { getRegistrantBasicInfo, RegistrationEntryState } from '../RegistrationEntry';
import StringFilter from '../../../Services/String';
import RockButton from '../../../Elements/RockButton';
import RegistrantPersonField from './RegistrantPersonField';
import RegistrantAttributeField from './RegistrantAttributeField';
import Alert from '../../../Elements/Alert';
import { RegistrantInfo, RegistrantsSameFamily, RegistrationEntryBlockFamilyMemberViewModel, RegistrationEntryBlockFormFieldViewModel, RegistrationEntryBlockFormViewModel, RegistrationEntryBlockViewModel, RegistrationFieldSource } from './RegistrationEntryBlockViewModel';
import { areEqual, Guid } from '../../../Util/Guid';
import RockForm from '../../../Controls/RockForm';
import FeeField from './FeeField';
import ItemsWithPreAndPostHtml, { ItemWithPreAndPostHtml } from '../../../Elements/ItemsWithPreAndPostHtml';

export default defineComponent( {
    name: 'Event.RegistrationEntry.Registrant',
    components: {
        RadioButtonList,
        RockButton,
        RegistrantPersonField,
        RegistrantAttributeField,
        Alert,
        RockForm,
        FeeField,
        DropDownList,
        ItemsWithPreAndPostHtml
    },
    props: {
        currentRegistrant: {
            type: Object as PropType<RegistrantInfo>,
            required: true
        },
        isWaitList: {
            type: Boolean as PropType<boolean>,
            required: true
        }
    },
    setup ()
    {
        const registrationEntryState = inject( 'registrationEntryState' ) as RegistrationEntryState;

        return {
            registrationEntryState
        };
    },
    data ()
    {
        return {
            fieldSources: {
                PersonField: RegistrationFieldSource.PersonField,
                PersonAttribute: RegistrationFieldSource.PersonAttribute,
                GroupMemberAttribute: RegistrationFieldSource.GroupMemberAttribute,
                RegistrantAttribute: RegistrationFieldSource.RegistrantAttribute
            }
        };
    },
    computed: {
        showPrevious (): boolean
        {
            return this.registrationEntryState.FirstStep !== this.registrationEntryState.Steps.perRegistrantForms;
        },
        viewModel (): RegistrationEntryBlockViewModel
        {
            return this.registrationEntryState.ViewModel;
        },
        currentFormIndex (): number
        {
            return this.registrationEntryState.CurrentRegistrantFormIndex;
        },
        currentForm (): RegistrationEntryBlockFormViewModel | null
        {
            return this.formsToShow[ this.currentFormIndex ] || null;
        },
        isLastForm (): boolean
        {
            return ( this.currentFormIndex + 1 ) === this.formsToShow.length;
        },

        /** The filtered list of forms that will be shown */
        formsToShow (): RegistrationEntryBlockFormViewModel[]
        {
            if ( !this.isWaitList )
            {
                return this.viewModel.registrantForms;
            }

            return this.viewModel.registrantForms.filter( form => form.fields.some( field => field.showOnWaitList ) );
        },

        /** The filtered fields to show on the current form */
        currentFormFields (): RegistrationEntryBlockFormFieldViewModel[]
        {
            return ( this.currentForm?.fields || [] )
                .filter( f => !this.isWaitList || f.showOnWaitList );
        },

        /** The current fields as pre-post items to allow pre-post HTML to be rendered */
        prePostHtmlItems (): ItemWithPreAndPostHtml[]
        {
            return this.currentFormFields
                .map( f => ( {
                    PreHtml: f.preHtml,
                    PostHtml: f.postHtml,
                    SlotName: f.guid
                } ) );
        },
        currentPerson (): Person | null
        {
            return this.$store.state.currentPerson;
        },
        pluralFeeTerm (): string
        {
            return StringFilter.toTitleCase( this.viewModel.pluralFeeTerm || 'fees' );
        },

        /** The radio options that are displayed to allow the user to pick another person that this
         *  registrant is part of a family. */
        familyOptions (): DropDownListOption[]
        {
            const options: DropDownListOption[] = [];
            const usedFamilyGuids: Record<Guid, boolean> = {};

            if ( this.viewModel.registrantsSameFamily !== RegistrantsSameFamily.Ask )
            {
                return options;
            }

            // Add previous registrants as options
            for ( let i = 0; i < this.registrationEntryState.CurrentRegistrantIndex; i++ )
            {
                const registrant = this.registrationEntryState.Registrants[ i ];
                const info = getRegistrantBasicInfo( registrant, this.viewModel.registrantForms );

                if ( !usedFamilyGuids[ registrant.FamilyGuid ] && info?.FirstName && info?.LastName )
                {
                    options.push( {
                        key: registrant.FamilyGuid,
                        text: `${info.FirstName} ${info.LastName}`,
                        value: registrant.FamilyGuid
                    } );

                    usedFamilyGuids[ registrant.FamilyGuid ] = true;
                }
            }

            // Add the current person (registrant) if not already added
            if ( this.currentPerson?.primaryFamilyGuid && this.currentPerson.fullName && !usedFamilyGuids[ this.currentPerson.primaryFamilyGuid ] )
            {
                options.push( {
                    key: this.currentPerson.primaryFamilyGuid,
                    text: this.currentPerson.fullName,
                    value: this.currentPerson.primaryFamilyGuid
                } );
            }

            options.push( {
                key: this.currentRegistrant.OwnFamilyGuid,
                text: 'None of the above',
                value: this.currentRegistrant.OwnFamilyGuid
            } );

            return options;
        },

        /** The people that can be picked from because they are members of the same family. */
        familyMemberOptions (): DropDownListOption[]
        {
            const selectedFamily = this.currentRegistrant.FamilyGuid;

            if ( !selectedFamily )
            {
                return [];
            }

            const usedFamilyMemberGuids = this.registrationEntryState.Registrants
                .filter( r => r.PersonGuid && r.PersonGuid !== this.currentRegistrant.PersonGuid )
                .map( r => r.PersonGuid );

            return this.viewModel.familyMembers
                .filter( fm =>
                    areEqual( fm.familyGuid, selectedFamily ) &&
                    !usedFamilyMemberGuids.includes( fm.guid ) )
                .map( fm => ( {
                    key: fm.guid,
                    text: fm.fullName,
                    value: fm.guid
                } ) );
        },
        uppercaseRegistrantTerm (): string
        {
            return StringFilter.toTitleCase( this.viewModel.registrantTerm );
        },
        firstName (): string
        {
            return getRegistrantBasicInfo( this.currentRegistrant, this.viewModel.registrantForms ).FirstName;
        },
        familyMember (): RegistrationEntryBlockFamilyMemberViewModel | null
        {
            const personGuid = this.currentRegistrant.PersonGuid;

            if ( !personGuid )
            {
                return null;
            }

            return this.viewModel.familyMembers.find( fm => areEqual( fm.guid, personGuid ) ) || null;
        }
    },
    methods: {
        onPrevious ()
        {
            if ( this.currentFormIndex <= 0 )
            {
                this.$emit( 'previous' );
                return;
            }

            this.registrationEntryState.CurrentRegistrantFormIndex--;
        },
        onNext ()
        {
            const lastFormIndex = this.formsToShow.length - 1;

            if ( this.currentFormIndex >= lastFormIndex )
            {
                this.$emit( 'next' );
                return;
            }

            this.registrationEntryState.CurrentRegistrantFormIndex++;
        },

        /** Copy the values that are to have current values used */
        copyValuesFromFamilyMember ()
        {
            if ( !this.familyMember )
            {
                // Nothing to copy
                return;
            }

            // If the family member selection is made then set all form fields where use existing value is enabled
            for ( const form of this.viewModel.registrantForms )
            {
                for ( const field of form.fields )
                {
                    if ( field.guid in this.familyMember.fieldValues )
                    {
                        const familyMemberValue = this.familyMember.fieldValues[ field.guid ];

                        if ( !familyMemberValue )
                        {
                            delete this.currentRegistrant.FieldValues[ field.guid ];
                        }
                        else if ( typeof familyMemberValue === 'object' )
                        {
                            this.currentRegistrant.FieldValues[ field.guid ] = { ...familyMemberValue };
                        }
                        else
                        {
                            this.currentRegistrant.FieldValues[ field.guid ] = familyMemberValue;
                        }
                    }
                }
            }
        }
    },
    watch: {
        'currentRegistrant.FamilyGuid' ()
        {
            // Clear the person guid if the family changes
            this.currentRegistrant.PersonGuid = '';
        },
        familyMember: {
            handler ()
            {
                if ( !this.familyMember )
                {
                    // If the family member selection is cleared then clear all form fields
                    for ( const form of this.viewModel.registrantForms )
                    {
                        for ( const field of form.fields )
                        {
                            delete this.currentRegistrant.FieldValues[ field.guid ];
                        }
                    }
                }
                else
                {
                    // If the family member selection is made then set all form fields where use existing value is enabled
                    this.copyValuesFromFamilyMember();
                }
            }
        }
    },
    created ()
    {
        this.copyValuesFromFamilyMember();
    },
    template: `
<div>
    <RockForm @submit="onNext">
        <template v-if="currentFormIndex === 0">
            <div v-if="familyOptions.length > 1" class="well js-registration-same-family">
                <RadioButtonList :label="(firstName || uppercaseRegistrantTerm) + ' is in the same immediate family as'" rules='required:{"allowEmptyString": true}' v-model="currentRegistrant.FamilyGuid" :options="familyOptions" validationTitle="Family" />
            </div>
            <div v-if="familyMemberOptions.length" class="row">
                <div class="col-md-6">
                    <DropDownList v-model="currentRegistrant.PersonGuid" :options="familyMemberOptions" label="Family Member to Register" />
                </div>
            </div>
        </template>

        <ItemsWithPreAndPostHtml :items="prePostHtmlItems">
            <template v-for="field in currentFormFields" :key="field.guid" v-slot:[field.guid]>
                <RegistrantPersonField v-if="field.fieldSource === fieldSources.PersonField" :field="field" :fieldValues="currentRegistrant.FieldValues" :isKnownFamilyMember="!!currentRegistrant.PersonGuid" />
                <RegistrantAttributeField v-else-if="field.fieldSource === fieldSources.RegistrantAttribute || field.fieldSource === fieldSources.PersonAttribute" :field="field" :fieldValues="currentRegistrant.FieldValues" />
                <Alert alertType="danger" v-else>Could not resolve field source {{field.fieldSource}}</Alert>
            </template>
        </ItemsWithPreAndPostHtml>

        <div v-if="!isWaitList && isLastForm && viewModel.fees.length" class="well registration-additional-options">
            <h4>{{pluralFeeTerm}}</h4>
            <template v-for="fee in viewModel.fees" :key="fee.Guid">
                <FeeField :fee="fee" v-model="currentRegistrant.FeeItemQuantities" />
            </template>
        </div>

        <div class="actions row">
            <div class="col-xs-6">
                <RockButton v-if="showPrevious" btnType="default" @click="onPrevious">
                    Previous
                </RockButton>
            </div>
            <div class="col-xs-6 text-right">
                <RockButton btnType="primary" type="submit">
                    Next
                </RockButton>
            </div>
        </div>
    </RockForm>
</div>`
} );