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
import bus from '../../Util/Bus';
import PaneledBlockTemplate from '../../Templates/PaneledBlockTemplate';
import RockButton from '../../Elements/RockButton';
import TextBox from '../../Elements/TextBox';
import { defineComponent, inject } from 'vue';
import store from '../../Store/Index';
import EmailBox from '../../Elements/EmailBox';
import RockValidation from '../../Controls/RockValidation';
import RockForm from '../../Controls/RockForm';
import CampusPicker from '../../Controls/CampusPicker';
import { Guid } from '../../Util/Guid';
import Loading from '../../Controls/Loading';
import PrimaryBlock from '../../Controls/PrimaryBlock';
import { InvokeBlockActionFunc } from '../../Controls/RockBlock';
import Campus from '../../ViewModels/CodeGenerated/CampusViewModel';
import Person from '../../ViewModels/CodeGenerated/PersonViewModel';
import { asDateString } from '../../Services/Date';
import RockDate, { RockDateType, toRockDate } from '../../Util/RockDate';
import DatePicker from '../../Elements/DatePicker';
import AddressControl, { getDefaultAddressControlModel } from '../../Controls/AddressControl';

declare type PersonViewModel = {
    Id: number;
    Guid: Guid;
    FirstName: string;
    NickName: string;
    LastName: string;
    Email: string;
    PrimaryCampusId: number | null;
    BirthDay: number | null;
    BirthMonth: number | null;
    BirthYear: number | null;
};

export default defineComponent({
    name: 'Example.PersonDetail',
    components: {
        PaneledBlockTemplate,
        RockButton,
        TextBox,
        EmailBox,
        RockValidation,
        RockForm,
        CampusPicker,
        Loading,
        PrimaryBlock,
        DatePicker,
        AddressControl
    },
    setup() {
        return {
            invokeBlockAction: inject('invokeBlockAction') as InvokeBlockActionFunc
        };
    },
    data() {
        return {
            person: null as PersonViewModel | null,
            personForEditing: null as PersonViewModel | null,
            isEditMode: false,
            messageToPublish: '',
            receivedMessage: '',
            isLoading: false,
            campusGuid: '' as Guid,
            birthdate: null as RockDateType | null,
            address: getDefaultAddressControlModel()
        };
    },
    methods: {
        setIsEditMode(isEditMode): void {
            this.isEditMode = isEditMode;
        },
        doEdit(): void {
            this.personForEditing = this.person ? { ...this.person } : null;
            this.campusGuid = this.campus?.Guid || '';
            this.birthdate = this.birthdateOrNull ? toRockDate(this.birthdateOrNull) : null;
            this.setIsEditMode(true);
        },
        doCancel(): void {
            this.setIsEditMode(false);
        },
        async doSave(): Promise<void> {
            if (this.personForEditing) {
                this.person = {
                    ...this.personForEditing,
                    BirthDay: RockDate.getDay(this.birthdate),
                    BirthMonth: RockDate.getMonth(this.birthdate),
                    BirthYear: RockDate.getYear(this.birthdate),
                    PrimaryCampusId: store.getters['campuses/getByGuid'](this.campusGuid)?.Id || null
                };
                this.isLoading = true;

                await this.invokeBlockAction('EditPerson', {
                    personArgs: this.person
                });

                this.isLoading = false;
            }

            this.setIsEditMode(false);
        },
        doPublish(): void {
            bus.publish('PersonDetail:Message', this.messageToPublish);
            this.messageToPublish = '';
        },
        receiveMessage(message: string): void {
            this.receivedMessage = message;
        }
    },
    computed: {
        birthdateOrNull(): Date | null {
            if (!this.person?.BirthDay || !this.person.BirthMonth || !this.person.BirthYear) {
                return null;
            }

            return new Date(`${this.person.BirthYear}-${this.person.BirthMonth}-${this.person.BirthDay}`);
        },
        birthdateFormatted(): string {
            if (!this.birthdateOrNull) {
                return 'Not Completed';
            }

            return asDateString(this.birthdateOrNull);
        },
        campus(): Campus | null {
            if (this.person) {
                return store.getters['campuses/getById'](this.person.PrimaryCampusId) || null;
            }

            return null;
        },
        campusName(): string {
            return this.campus?.Name || '';
        },
        blockTitle(): string {
            return this.person ?
                `: ${this.person.NickName || this.person.FirstName} ${this.person.LastName}` :
                '';
        },
        currentPerson(): Person | null {
            return store.state.currentPerson;
        },
        currentPersonGuid(): Guid | null {
            return this.currentPerson ? this.currentPerson.Guid : null;
        }
    },
    watch: {
        currentPersonGuid: {
            immediate: true,
            async handler() {
                if (!this.currentPersonGuid) {
                    // Set the person empty to match the guid
                    this.person = null;
                    return;
                }

                if (this.person && this.person.Guid === this.currentPersonGuid) {
                    // Already loaded
                    return;
                }

                // Sync the person with the guid
                this.isLoading = true;
                this.person = (await this.invokeBlockAction<PersonViewModel>('GetPersonViewModel')).data;
                this.isLoading = false;
            }
        }
    },
    created(): void {
        bus.subscribe<string>('PersonSecondary:Message', this.receiveMessage);
    },
    template: `
<PrimaryBlock :hideSecondaryBlocks="isEditMode">
    <PaneledBlockTemplate>
        <template v-slot:title>
            <i class="fa fa-flask"></i>
            Edit Yourself{{blockTitle}}
        </template>
        <template v-slot:default>
            <Loading :isLoading="isLoading">
                <p v-if="!person">
                    There is no person loaded.
                </p>
                <RockForm v-else-if="isEditMode" @submit="doSave">
                    <div class="row">
                        <div class="col-sm-6">
                            <TextBox label="First Name" v-model="personForEditing.FirstName" rules="required" />
                            <TextBox label="Nick Name" v-model="personForEditing.NickName" />
                            <TextBox label="Last Name" v-model="personForEditing.LastName" rules="required" />
                        </div>
                        <div class="col-sm-6">
                            <EmailBox v-model="personForEditing.Email" />
                            <CampusPicker v-model="campusGuid" />
                            <DatePicker label="Birthdate" v-model="birthdate" rules="required" />
                        </div>
                        <div class="col-sm-12">
                            <AddressControl v-model="address" />
                        </div>
                    </div>
                    <div class="actions">
                        <RockButton btnType="primary" type="submit">Save</RockButton>
                        <RockButton btnType="link" @click="doCancel">Cancel</RockButton>
                    </div>
                </RockForm>
                <template v-else>
                    <div class="row">
                        <div class="col-sm-6">
                            <dl>
                                <dt>First Name</dt>
                                <dd>{{person.FirstName}}</dd>
                                <dt>Last Name</dt>
                                <dd>{{person.LastName}}</dd>
                                <dt>Email</dt>
                                <dd>{{person.Email}}</dd>
                                <dt>Campus</dt>
                                <dd>{{campusName || 'None'}}</dd>
                                <dt>Birthdate</dt>
                                <dd>{{birthdateFormatted}}</dd>
                            </dl>
                        </div>
                        <div class="col-sm-6">
                            <div class="well">
                                <TextBox label="Message" v-model="messageToPublish" />
                                <RockButton btnType="primary" btnSize="sm" @click="doPublish">Publish</RockButton>
                            </div>
                            <p>
                                <strong>Secondary block says:</strong>
                                {{receivedMessage}}
                            </p>
                        </div>
                    </div>
                    <div class="actions">
                        <RockButton btnType="primary" @click="doEdit">Edit</RockButton>
                    </div>
                </template>
            </Loading>
        </template>
    </PaneledBlockTemplate>
</PrimaryBlock>`
});
