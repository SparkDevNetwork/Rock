Obsidian.Blocks.registerBlock({
    name: 'Test.ControlGallery',
    components: {
        PaneledBlockTemplate: Obsidian.Templates.PaneledBlockTemplate,
        DefinedTypePicker: Obsidian.Controls.DefinedTypePicker,
        DefinedValuePicker: Obsidian.Controls.DefinedValuePicker,
        CampusPicker: Obsidian.Controls.CampusPicker
    },
    data() {
        return {
            definedTypeGuid: '',
            definedValueGuid: '',
            campusGuid: ''
        };
    },
    computed: {
        campusName() {
            const campus = this.$store.getters['campuses/getByGuid'](this.campusGuid);
            return campus ? campus.Name : '';
        },
        definedTypeName() {
            const definedType = this.$store.getters['definedTypes/getByGuid'](this.definedTypeGuid);
            return definedType ? definedType.Name : '';
        }
    },
    template:
`<PaneledBlockTemplate>
    <template v-slot:title>
        <i class="fa fa-flask"></i>
        Obsidian Control Gallery
    </template>
    <template v-slot:default>
        <div class="row">
            <div class="col-sm-12 col-md-6 col-lg-4">
                <DefinedTypePicker label="Defined Type" v-model="definedTypeGuid" />
                <DefinedValuePicker label="Defined Value" v-model="definedValueGuid" :defined-type-guid="definedTypeGuid" />
                <CampusPicker label="Campus" v-model="campusGuid" />
            </div>
        </div>
        <hr />
        <div class="row">
            <div class="col-sm-12">
                <p>
                    <strong>Defined Type Guid</strong>
                    {{definedTypeGuid}}
                    <span v-if="definedTypeName">({{definedTypeName}})</span>
                </p>
                <p><strong>Defined Value Guid</strong> {{definedValueGuid}}</p>
                <p>
                    <strong>Campus Guid</strong>
                    {{campusGuid}}
                    <span v-if="campusName">({{campusName}})</span>
                </p>
            </div>
        </div>
    </template>
</PaneledBlockTemplate>`
});
