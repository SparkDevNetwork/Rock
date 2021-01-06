import { defineComponent, inject } from '../../Vendor/Vue/vue.js';
import PaneledBlockTemplate from '../../Templates/PaneledBlockTemplate.js';
import Loading from '../../Controls/Loading.js';
import RockField from '../../Controls/RockField.js';
import { BlockAction } from '../../Controls/RockBlock.js';
import { BlockSettings } from '../../Index.js';
import store from '../../Store/Index.js';
import { Guid } from '../../Util/Guid.js';
import Person from '../../Types/Models/Person.js';

type AttributeData = {
    Label: string;
    Value: string;
    FieldTypeGuid: string;
};

export default defineComponent({
    name: 'Crm.AttributeValues',
    components: {
        PaneledBlockTemplate,
        Loading,
        RockField
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
            attributeDataList: [] as AttributeData[]
        };
    },
    computed: {
        personGuid(): Guid | null {
            const person = (store.getters.personContext || null) as Person | null;
            return person ? person.Guid : null;
        }
    },
    methods: {
        async loadData() {
            if (!this.personGuid) {
                this.attributeDataList = [];
                return;
            }

            try {
                this.isLoading = true;
                const result = await this.blockAction<AttributeData[]>('GetAttributeDataList', {
                    PersonGuid: this.personGuid
                });

                if (result.data) {
                    this.attributeDataList = result.data;
                }
            }
            finally {
                this.isLoading = false;
            }
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
    <template v-slot:default>
        <Loading :isLoading="isLoading">
            <fieldset class="attribute-values ui-sortable">
                <div v-for="a in attributeDataList" class="form-group static-control">
                    <label class="control-label">
                        {{ a.Label }}
                    </label>
                    <div class="control-wrapper">
                        <div class="form-control-static">
                            <RockField :fieldTypeGuid="a.FieldTypeGuid" v-model="a.Value" />
                        </div>
                    </div>
                </div>
            </fieldset>
        </Loading>
    </template>
</PaneledBlockTemplate>`
});
