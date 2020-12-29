import bus from '../../Util/Bus.js';
import PaneledBlockTemplate from '../../Templates/PaneledBlockTemplate.js';
import RockButton from '../../Elements/RockButton.js';
import TextBox from '../../Elements/TextBox.js';
import { defineComponent } from '../../Vendor/Vue/vue.js';
import store from '../../Store/Index.js';
import Person from '../../Types/Models/Person.js';

export default defineComponent({
    name: 'Test.PersonDetail',
    components: {
        PaneledBlockTemplate,
        RockButton,
        TextBox
    },
    data() {
        const person: Person = {
            FirstName: 'Ted',
            LastName: 'Decker',
            PhotoUrl: '',
            FullName: 'Ted Decker',
            Guid: '',
            Id: 0
        };

        return {
            person,
            personForEditing: { ...person },
            isEditMode: false,
            messageToPublish: '',
            receivedMessage: ''
        };
    },
    methods: {
        setAreSecondaryBlocksShown(isVisible) {
            store.commit('setAreSecondaryBlocksShown', { areSecondaryBlocksShown: isVisible });
        },
        setIsEditMode(isEditMode) {
            this.isEditMode = isEditMode;
            this.setAreSecondaryBlocksShown(!isEditMode);
        },
        doEdit() {
            this.personForEditing = { ...this.person };
            this.setIsEditMode(true);
        },
        doDelete() {
            console.log('delete here');
        },
        doCancel() {
            this.personForEditing = { ...this.person };
            this.setIsEditMode(false);
        },
        doSave() {
            this.person = { ...this.personForEditing };
            this.setIsEditMode(false);
        },
        doPublish() {
            bus.publish('PersonDetail:Message', this.messageToPublish);
            this.messageToPublish = '';
        },
        receiveMessage(message: string) {
            this.receivedMessage = message;
        }
    },
    computed: {
        blockTitle(): string {
            return `${this.person.FirstName} ${this.person.LastName}`;
        }
    },
    created() {
        bus.subscribe<string>('PersonSecondary:Message', this.receiveMessage);
    },
    template:
`<PaneledBlockTemplate>
    <template v-slot:title>
        <i class="fa fa-flask"></i>
        Detail Block: {{blockTitle}}
    </template>
    <template v-slot:default>
        <template v-if="isEditMode">
            <div class="row">
                <div class="col-sm-6 col-lg-4">
                    <TextBox label="First Name" v-model="personForEditing.FirstName" />
                    <TextBox label="Last Name" v-model="personForEditing.LastName" />
                </div>
            </div>
        </template>
        <template v-else>
            <div class="row">
                <div class="col-sm-6">
                    <dl>
                        <dt>First Name</dt>
                        <dd>{{person.FirstName}}</dd>
                        <dt>Last Name</dt>
                        <dd>{{person.LastName}}</dd>
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
        </template>
        <div class="actions">
            <template v-if="isEditMode">
                <RockButton class="btn-primary" @click="doSave">Save</RockButton>
                <RockButton class="btn-link" @click="doCancel">Cancel</RockButton>
            </template>
            <template v-else>
                <RockButton class="btn-primary" @click="doEdit">Edit</RockButton>
                <RockButton class="btn-link" @click="doDelete">Delete</RockButton>
            </template>
        </div>
    </template>
</PaneledBlockTemplate>`
});
