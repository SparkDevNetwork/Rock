Obsidian.Blocks['Crm/AttributeValues'] = {
    name: 'Crm_AttributeValues',
    components: {
        PaneledBlockTemplate: Obsidian.Templates.PaneledBlockTemplate,
        RockLoading: Obsidian.Controls.RockLoading,
        RockField: Obsidian.Controls.RockField
    },
    inject: [
        'configurationValues',
        'blockAction'
    ],
    data() {
        return {
            isLoading: true,
            attributeDataList: []
        };
    },
    async created() {
        try {
            this.isLoading = true;
            const result = await this.blockAction('GetAttributeDataList');

            if (result.data) {
                this.attributeDataList = result.data;
            }
        }
        finally {
            this.isLoading = false;
        }
    },
    template:
`<PaneledBlockTemplate class="panel-persondetails">
    <template slot="title">
        <i :class="configurationValues.BlockIconCssClass"></i>
        {{ configurationValues.BlockTitle }}
    </template>
    <template>
        <RockLoading :isLoading="isLoading">
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
        </RockLoading>
    </template>
</PaneledBlockTemplate>`
};
