import { defineComponent, inject } from '../../Vendor/Vue/vue.js';
import PaneledBlockTemplate from '../../Templates/PaneledBlockTemplate.js';
import Loading from '../../Controls/Loading.js';
import RockField from '../../Controls/RockField.js';
import { BlockAction } from '../../Controls/RockBlock.js';
import { BlockSettings } from '../../Index.js';
import store from '../../Store/Index.js';
import { Guid } from '../../Util/Guid.js';
import Person from '../../Types/Models/Person.js';
import JavaScriptAnchor from '../../Elements/JavaScriptAnchor.js';
import RockForm from '../../Controls/RockForm.js';
import TextBox from '../../Elements/TextBox.js';
import RockButton from '../../Elements/RockButton.js';
import AttributeValue from '../../Types/Models/AttributeValue.js';

export default defineComponent({
    name: 'Crm.AttributeValues',
    components: {
        PaneledBlockTemplate,
        Loading,
        RockField,
        JavaScriptAnchor,
        RockForm,
        TextBox,
        RockButton
    },
    setup() {
        return {
            blockAction: inject('blockAction') as BlockAction,
            blockSettings: inject('blockSettings') as BlockSettings
        };
    },
    data() {
        return {
            isLoading: true,
            isEditMode: false,
            attributeDataList: [] as AttributeValue[]
        };
    },
    computed: {
        personGuid(): Guid | null {
            const person = (store.getters.personContext || null) as Person | null;
            return person ? person.Guid : null;
        },
        nonEmptyAttributeValues(): AttributeValue[] {
            return this.attributeDataList.filter(av => !!av.Value);
        }
    },
    methods: {
        goToViewMode() {
            this.isEditMode = false;
        },
        goToEditMode() {
            this.isEditMode = true;
        },
        getAttributeLabel(attributeValue: AttributeValue) {
            const useAbbreviatedNames = this.blockSettings.UseAbbreviatedNames as boolean;

            if (useAbbreviatedNames) {
                return attributeValue.AttributeAbbreviatedName || attributeValue.AttributeName;
            }

            return attributeValue.AttributeName;
        },
        async loadData() {
            if (!this.personGuid) {
                this.attributeDataList = [];
                return;
            }

            try {
                this.isLoading = true;
                const result = await this.blockAction<AttributeValue[]>('GetAttributeValueList', {
                    PersonGuid: this.personGuid
                });

                if (result.data) {
                    this.attributeDataList = result.data;
                }
            }
            finally {
                this.isLoading = false;
            }
        },
        async doSave(): Promise<void> {
            this.isLoading = true;

            const keyArgsMap = {};

            for (const a of this.attributeDataList) {
                keyArgsMap[a.AttributeKey] = a;
            }

            await this.blockAction('SaveAttributeValues', {
                personGuid: this.personGuid,
                keyArgsMap
            });

            this.isLoading = false;
            this.goToViewMode();
        }
    },
    watch: {
        personGuid: {
            immediate: true,
            async handler() {
                if (this.personGuid) {
                    await this.loadData();
                }
            }
        }
    },
    template: `
<PaneledBlockTemplate class="panel-persondetails">
    <template v-slot:title>
        <i :class="blockSettings.BlockIconCssClass"></i>
        {{ blockSettings.BlockTitle }}
    </template>
    <template v-slot:titleAside>
        <div class="actions rollover-item pull-right">
            <JavaScriptAnchor title="Order Attributes" class="btn-link edit">
                <i class="fa fa-bars"></i>
            </JavaScriptAnchor>
            <JavaScriptAnchor title="Edit Attributes" class="btn-link edit" @click="goToEditMode">
                <i class="fa fa-pencil"></i>
            </JavaScriptAnchor>
        </div>
    </template>
    <template v-slot:default>
        <Loading :isLoading="isLoading">
            <div v-if="!isEditMode" v-for="a in nonEmptyAttributeValues" class="form-group static-control">
                <label class="control-label">
                    {{ getAttributeLabel(a) }}
                </label>
                <div class="control-wrapper">
                    <div class="form-control-static">
                        <RockField :fieldTypeGuid="a.AttributeFieldTypeGuid" v-model="a.Value" />
                    </div>
                </div>
            </div>
            <RockForm v-else @submit="doSave">
                <template v-for="a in attributeDataList">
                    <RockField
                        edit
                        :fieldTypeGuid="a.AttributeFieldTypeGuid"
                        v-model="a.Value"
                        :label="getAttributeLabel(a)"
                        :help="a.AttributeDescription"
                        :rules="a.AttributeIsRequired ? 'required' : ''"
                        :configurationValues="a.AttributeQualifierValues"  />
                </template>
                <div class="actions">
                    <RockButton class="btn-primary btn-xs" type="submit">Save</RockButton>
                    <RockButton class="btn-link btn-xs" @click="goToViewMode">Cancel</RockButton>
                </div>
            </RockForm>
        </Loading>
    </template>
</PaneledBlockTemplate>`
});
