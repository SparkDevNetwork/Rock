import bus from '../../Util/Bus.js';
import PaneledBlockTemplate from '../../Templates/PaneledBlockTemplate.js';
import RockButton from '../../Elements/RockButton.js';
import TextBox from '../../Elements/TextBox.js';
import { defineComponent } from '../../Vendor/Vue/vue.js';
import store from '../../Store/Index.js';
import Person from '../../Types/Models/Person.js';
import EmailInput from '../../Elements/EmailInput.js';
import RockValidation from '../../Controls/RockValidation.js';
import RockForm from '../../Controls/RockForm.js';
import CampusPicker from '../../Controls/CampusPicker.js';

export default defineComponent({
    name: 'Test.PersonDetail',
    components: {
        PaneledBlockTemplate,
        RockButton,
        TextBox,
        EmailInput,
        RockValidation,
        RockForm,
        CampusPicker
    },
    data() {
        const person: Person = {
            FirstName: 'John',
            LastName: 'Smith',
            PhotoUrl: '',
            FullName: 'John Smith',
            Email: 'john@smith.com',
            Guid: '',
            Id: 0
        };

        return {
            person,
            personForEditing: { ...person } as Person,
            isEditMode: false,
            messageToPublish: '',
            receivedMessage: '',
            campusId: ''
        };
    },
    methods: {
        setAreSecondaryBlocksShown(isVisible): void {
            store.commit('setAreSecondaryBlocksShown', { areSecondaryBlocksShown: isVisible });
        },
        setIsEditMode(isEditMode): void {
            this.isEditMode = isEditMode;
            this.setAreSecondaryBlocksShown(!isEditMode);
        },
        doEdit(): void {
            this.personForEditing = { ...this.person };
            this.setIsEditMode(true);
        },
        doDelete(): void {
            console.log('delete here');
        },
        doCancel(): void {
            this.personForEditing = { ...this.person };
            this.setIsEditMode(false);
        },
        doSave(): void {
            this.person = { ...this.personForEditing };
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
        blockTitle(): string {
            return `${this.person.FirstName} ${this.person.LastName}`;
        }
    },
    created(): void {
        bus.subscribe<string>('PersonSecondary:Message', this.receiveMessage);
    },
    template:
`<PaneledBlockTemplate>
    <template v-slot:title>
        <i class="fa fa-flask"></i>
        Detail Block: {{blockTitle}}
    </template>
    <template v-slot:default>
        <RockForm v-if="isEditMode" @submit="doSave">
            <div class="row">
                <div class="col-sm-6">
                    <TextBox label="First Name" v-model="personForEditing.FirstName" rules="required" disabled />
                    <TextBox label="Last Name" v-model="personForEditing.LastName" />
                </div>
                <div class="col-sm-6">
                    <EmailInput v-model="personForEditing.Email" rules="required" />
                    <CampusPicker v-model="campusId" rules="required" />
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
                <RockButton class="btn-link" @click="doDelete">Delete</RockButton>
            </div>
        </template>
    </template>
</PaneledBlockTemplate>`
});
