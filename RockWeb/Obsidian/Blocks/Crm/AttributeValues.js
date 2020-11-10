Obsidian.Blocks.registerBlock({
    name: 'Crm.AttributeValues',
    components: {
        PaneledBlockTemplate: Obsidian.Templates.PaneledBlockTemplate,
        Loading: Obsidian.Controls.Loading,
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
    methods: {
        async loadData() {
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
        }
    },
    async mounted() {
        await this.loadData();
    },
    template:
`<PaneledBlockTemplate class="panel-persondetails">
    <template v-slot:title>
        <i :class="configurationValues.BlockIconCssClass"></i>
        {{ configurationValues.BlockTitle }}
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
