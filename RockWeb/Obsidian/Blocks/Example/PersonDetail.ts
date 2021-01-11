import bus from '../../Util/Bus.js';
import PaneledBlockTemplate from '../../Templates/PaneledBlockTemplate.js';
import RockButton from '../../Elements/RockButton.js';
import TextBox from '../../Elements/TextBox.js';
import { defineComponent, inject } from '../../Vendor/Vue/vue.js';
import store from '../../Store/Index.js';
import Person from '../../Types/Models/Person.js';
import EmailInput from '../../Elements/EmailInput.js';
import RockValidation from '../../Controls/RockValidation.js';
import RockForm from '../../Controls/RockForm.js';
import CampusPicker from '../../Controls/CampusPicker.js';
import { Guid } from '../../Util/Guid.js';
import Loading from '../../Controls/Loading.js';
import PrimaryBlock from '../../Controls/PrimaryBlock.js';
import { BlockAction } from '../../Controls/RockBlock.js';
import Campus from '../../Types/Models/Campus.js';

declare type PersonViewModel = {
    Id: number;
    Guid: Guid;
    FirstName: string;
    NickName: string;
    LastName: string;
    Email: string;
    PrimaryCampusId: number | null;
};

export default defineComponent({
    name: 'Example.PersonDetail',
    components: {
        PaneledBlockTemplate,
        RockButton,
        TextBox,
        EmailInput,
        RockValidation,
        RockForm,
        CampusPicker,
        Loading,
        PrimaryBlock
    },
    setup() {
        return {
            blockAction: inject('blockAction') as BlockAction
        };
    },
    data() {
        return {
            person: null as PersonViewModel | null,
            personForEditing: null as PersonViewModel | null,
            isEditMode: false,
            messageToPublish: '',
            receivedMessage: '',
            isLoading: false
        };
    },
    methods: {
        setIsEditMode(isEditMode): void {
            this.isEditMode = isEditMode;
        },
        doEdit(): void {
            this.personForEditing = this.person ? { ...this.person } : null;
            this.setIsEditMode(true);
        },
        doCancel(): void {
            this.setIsEditMode(false);
        },
        async doSave(): Promise<void> {
            if (this.personForEditing) {
                this.person = { ...this.personForEditing };
                this.isLoading = true;

                await this.blockAction('EditPerson', {
                    personGuid: this.person.Guid,
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
        campus(): Campus | null {
            if (this.person) {
                return store.getters['campuses/getById'](this.person.PrimaryCampusId) || null;
            }

            return null;
        },
        campusName(): string {
            return this.campus ? this.campus.Name : '';
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
                this.person = (await this.blockAction<PersonViewModel>('GetPersonViewModel', {
                    personGuid: this.currentPersonGuid
                })).data;
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
                            <EmailInput v-model="personForEditing.Email" />
                            <CampusPicker v-model:id="personForEditing.PrimaryCampusId" />
                        </div>
                    </div>
                    <div class="actions">
                        <RockButton class="btn-primary" type="submit">Save</RockButton>
                        <RockButton class="btn-link" @click="doCancel">Cancel</RockButton>
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
                            </dl>
                        </div>
                        <div class="col-sm-6">
                            <div class="well">
                                <TextBox label="Message" v-model="messageToPublish" />
                                <RockButton class="btn-primary btn-sm" @click="doPublish">Publish</RockButton>
                            </div>
                            <p>
                                <strong>Secondary block says:</strong>
                                {{receivedMessage}}
                            </p>
                        </div>
                    </div>
                    <div class="actions">
                        <RockButton class="btn-primary" @click="doEdit">Edit</RockButton>
                    </div>
                </template>
            </Loading>
        </template>
    </PaneledBlockTemplate>
</PrimaryBlock>`
});
