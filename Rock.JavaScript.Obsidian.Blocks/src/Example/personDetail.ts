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

import { Guid } from "@Obsidian/Types";
import bus from "@Obsidian/Utility/bus";
import Block from "@Obsidian/Templates/block";
import RockButton from "@Obsidian/Controls/rockButton";
import TextBox from "@Obsidian/Controls/textBox";
import { defineComponent } from "vue";
import { useStore } from "@Obsidian/PageState";
import EmailBox from "@Obsidian/Controls/emailBox";
import RockValidation from "@Obsidian/Controls/rockValidation";
import RockForm from "@Obsidian/Controls/rockForm";
import Loading from "@Obsidian/Controls/loading";
import PrimaryBlock from "@Obsidian/Controls/primaryBlock";
import { useInvokeBlockAction } from "@Obsidian/Utility/block";
import DatePicker from "@Obsidian/Controls/datePicker.vue";
import AddressControl, { getDefaultAddressControlModel } from "@Obsidian/Controls/addressControl";
import { toNumber } from "@Obsidian/Utility/numberUtils";
import { DateTimeFormat, RockDateTime } from "@Obsidian/Utility/rockDateTime";
import { PersonBag } from "@Obsidian/ViewModels/Entities/personBag";

const store = useStore();

export default defineComponent({
    name: "Example.PersonDetail",

    components: {
        Block,
        RockButton,
        TextBox,
        EmailBox,
        RockValidation,
        RockForm,
        Loading,
        PrimaryBlock,
        DatePicker,
        AddressControl
    },

    setup() {
        return {
            invokeBlockAction: useInvokeBlockAction()
        };
    },

    data() {
        return {
            person: null as PersonBag | null,
            personForEditing: null as PersonBag | null,
            isEditMode: false,
            messageToPublish: "",
            receivedMessage: "",
            isLoading: false,
            birthdate: null as string | null,
            address: getDefaultAddressControlModel()
        };
    },

    methods: {
        setIsEditMode(isEditMode: boolean): void {
            this.isEditMode = isEditMode;
        },

        doEdit(): void {
            this.personForEditing = this.person ? { ...this.person } : null;
            this.birthdate = this.birthdateOrNull?.toASPString("yyyy-MM-dd") ?? null;
            this.setIsEditMode(true);
        },

        doCancel(): void {
            this.setIsEditMode(false);
        },

        async doSave(): Promise<void> {
            if (this.personForEditing) {
                const match = /^(\d+)-(\d+)-(\d+)/.exec(this.birthdate ?? "");
                let birthDay: number | null = null;
                let birthMonth: number | null = null;
                let birthYear: number | null = null;

                if (match !== null) {
                    birthYear = toNumber(match[1]);
                    birthMonth = toNumber(match[2]);
                    birthDay = toNumber(match[3]);
                }

                this.person = {
                    ...this.personForEditing,
                    birthDay: birthDay,
                    birthMonth: birthMonth,
                    birthYear: birthYear
                };
                this.isLoading = true;

                await this.invokeBlockAction("EditPerson", {
                    personArgs: this.person
                });

                this.isLoading = false;
            }

            this.setIsEditMode(false);
        },

        doPublish(): void {
            bus.publish("PersonDetail:Message", this.messageToPublish);
            this.messageToPublish = "";
        },

        receiveMessage(message: string): void {
            this.receivedMessage = message;
        }
    },

    computed: {
        birthdateOrNull(): RockDateTime | null {
            if (!this.person?.birthDay || !this.person.birthMonth || !this.person.birthYear) {
                return null;
            }

            return RockDateTime.fromParts(this.person.birthYear, this.person.birthMonth, this.person.birthDay);
        },

        birthdateFormatted(): string {
            if (!this.birthdateOrNull) {
                return "Not Completed";
            }

            return this.birthdateOrNull.toLocaleString(DateTimeFormat.DateTimeShort);
        },

        blockTitle(): string {
            return this.person ?
                `Edit Yourself: ${this.person.nickName || this.person.firstName} ${this.person.lastName}` :
                "Edit Yourself";
        },

        currentPerson(): PersonBag | null {
            return store.state.currentPerson;
        },

        currentPersonKey(): Guid | null {
            return this.currentPerson?.idKey ?? null;
        }
    },
    watch: {
        currentPersonKey: {
            immediate: true,
            async handler(): Promise<void> {
                if (!this.currentPersonKey) {
                    // Set the person empty to match the guid
                    this.person = null;
                    return;
                }

                if (this.person && this.person.idKey === this.currentPersonKey) {
                    // Already loaded
                    return;
                }

                // Sync the person with the guid
                this.isLoading = true;
                this.person = (await this.invokeBlockAction<PersonBag>("GetPersonViewModel")).data;
                this.isLoading = false;
            }
        }
    },

    created(): void {
        bus.subscribe<string>("PersonSecondary:Message", this.receiveMessage);
    },

    template: `
<PrimaryBlock :hideSecondaryBlocks="isEditMode">
    <Block :title="blockTitle">
        <template #default>
            <Loading :isLoading="isLoading">
                <p v-if="!person">
                    There is no person loaded.
                </p>
                <RockForm v-else-if="isEditMode" @submit="doSave">
                    <div class="row">
                        <div class="col-sm-6">
                            <TextBox label="First Name" v-model="personForEditing.firstName" rules="required" />
                            <TextBox label="Nick Name" v-model="personForEditing.nickName" />
                            <TextBox label="Last Name" v-model="personForEditing.lastName" rules="required" />
                        </div>
                        <div class="col-sm-6">
                            <EmailBox label="Email" v-model="personForEditing.email" />
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
                                <dd>{{person.firstName}}</dd>
                                <dt>Last Name</dt>
                                <dd>{{person.lastName}}</dd>
                                <dt>Email</dt>
                                <dd>{{person.email}}</dd>
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
    </Block>
</PrimaryBlock>`
});
