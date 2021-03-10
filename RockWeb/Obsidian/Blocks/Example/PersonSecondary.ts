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
import SecondaryBlock from '../../Controls/SecondaryBlock';
import RockButton from '../../Elements/RockButton';
import TextBox from '../../Elements/TextBox';
import { defineComponent } from 'vue';
import store from '../../Store/Index';
import Person from '../../ViewModels/CodeGenerated/PersonViewModel';

export default defineComponent({
    name: 'Example.PersonSecondary',
    components: {
        PaneledBlockTemplate,
        SecondaryBlock,
        TextBox,
        RockButton
    },
    data() {
        return {
            messageToPublish: '',
            receivedMessage: ''
        };
    },
    methods: {
        receiveMessage(message: string): void {
            this.receivedMessage = message;
        },
        doPublish(): void {
            bus.publish('PersonSecondary:Message', this.messageToPublish);
            this.messageToPublish = '';

        },
        doThrowError(): void {
            throw new Error('This is an uncaught error');
        }
    },
    computed: {
        currentPerson(): Person | null {
            return store.state.currentPerson;
        },
        currentPersonName(): string {
            return this.currentPerson?.FullName || 'anonymous';
        },
        imageUrl(): string {
            return this.currentPerson?.PhotoUrl || '/Assets/Images/person-no-photo-unknown.svg';
        },
        photoElementStyle(): string {
            return `background-image: url("${this.imageUrl}"); background-size: cover; background-repeat: no-repeat;`;
        }
    },
    created() {
        bus.subscribe<string>('PersonDetail:Message', this.receiveMessage);
    },
    template:
`<SecondaryBlock>
    <PaneledBlockTemplate>
        <template v-slot:title>
            <i class="fa fa-flask"></i>
            Secondary Block
        </template>
        <template v-slot:default>
            <div class="row">
                <div class="col-sm-6">
                    <p>
                        Hi, {{currentPersonName}}!
                        <div class="photo-icon photo-round photo-round-sm" :style="photoElementStyle"></div>
                    </p>
                    <p>This is a secondary block. It respects the store's value indicating if secondary blocks are visible.</p>
                    <RockButton btnType="danger" btnSize="sm" @click="doThrowError">Throw Error</RockButton>
                </div>
                <div class="col-sm-6">
                    <div class="well">
                        <TextBox label="Message" v-model="messageToPublish" />
                        <RockButton btnType="primary" btnSize="sm" @click="doPublish">Publish</RockButton>
                    </div>
                    <p>
                        <strong>Detail block says:</strong>
                        {{receivedMessage}}
                    </p>
                </div>
            </div>
        </template>
    </PaneledBlockTemplate>
</SecondaryBlock>`
});
