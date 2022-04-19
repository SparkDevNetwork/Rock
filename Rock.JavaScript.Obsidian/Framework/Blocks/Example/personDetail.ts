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
import bus from "../../Util/bus";
import PaneledBlockTemplate from "../../Templates/paneledBlockTemplate";
import RockButton from "../../Elements/rockButton";
import TextBox from "../../Elements/textBox";
import { defineComponent, inject } from "vue";
import { useStore } from "../../Store/index";
import EmailBox from "../../Elements/emailBox";
import RockValidation from "../../Controls/rockValidation";
import RockForm from "../../Controls/rockForm";
import Loading from "../../Controls/loading";
import PrimaryBlock from "../../Controls/primaryBlock";
import { InvokeBlockActionFunc } from "../../Util/block";
import DatePicker from "../../Elements/datePicker";
import AddressControl, { getDefaultAddressControlModel } from "../../Controls/addressControl";
import { toNumber } from "../../Services/number";
import { DateTimeFormat, RockDateTime } from "../../Util/rockDateTime";
import { Person } from "@Obsidian/ViewModels/Entities/person";

const store = useStore();

export default defineComponent({
    name: "Example.PersonDetail",

    components: {
        PaneledBlockTemplate,
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
            invokeBlockAction: inject("invokeBlockAction") as InvokeBlockActionFunc
        };
    },

    data() {
        return {
            person: null as Person | null,
            personForEditing: null as Person | null,
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
                `: ${this.person.nickName || this.person.firstName} ${this.person.lastName}` :
                "";
        },

        currentPerson(): Person | null {
            return store.state.currentPerson;
        },

        currentPersonGuid(): Guid | null {
            return this.currentPerson ? this.currentPerson.guid : null;
        }
    },
    watch: {
        currentPersonGuid: {
            immediate: true,
            async handler(): Promise<void> {
                if (!this.currentPersonGuid) {
                    // Set the person empty to match the guid
                    this.person = null;
                    return;
                }

                if (this.person && this.person.guid === this.currentPersonGuid) {
                    // Already loaded
                    return;
                }

                // Sync the person with the guid
                this.isLoading = true;
                this.person = (await this.invokeBlockAction<Person>("GetPersonViewModel")).data;
                this.isLoading = false;
            }
        }
    },

    created(): void {
        bus.subscribe<string>("PersonSecondary:Message", this.receiveMessage);
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
    </PaneledBlockTemplate>
</PrimaryBlock>`
});
