Obsidian.Blocks['Test.ControlGallery'] = {
    name: 'Test.ControlGallery',
    components: {
        PaneledBlockTemplate: Obsidian.Templates.PaneledBlockTemplate,
        RockDefinedTypePicker: Obsidian.Controls.RockDefinedTypePicker,
        RockDefinedValuePicker: Obsidian.Controls.RockDefinedValuePicker
    },
    data() {
        return {
            definedTypeGuid: '',
            definedValueGuid: ''
        };
    },
    template:
`<PaneledBlockTemplate>
    <template slot="title">
        <i class="fa fa-flask"></i>
        Obsidian Control Gallery
    </template>
    <template>
        <div class="row">
            <div class="col-sm-12 col-md-6 col-lg-4">
                <RockDefinedTypePicker label="Defined Type" v-model="definedTypeGuid" />
                <RockDefinedValuePicker label="Defined Value" v-model="definedValueGuid" :defined-type-guid="definedTypeGuid" />
            </div>
        </div>
        <hr />
        <div class="row">
            <div class="col-sm-12">
                <p><strong>Defined Type Guid</strong> {{definedTypeGuid}}</p>
                <p><strong>Defined Value Guid</strong> {{definedValueGuid}}</p>
            </div>
        </div>
    </template>
</PaneledBlockTemplate>`
};
