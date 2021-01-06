import PaneledBlockTemplate from '../../Templates/PaneledBlockTemplate.js';
import DefinedTypePicker from '../../Controls/DefinedTypePicker.js';
import DefinedValuePicker from '../../Controls/DefinedValuePicker.js';
import CampusPicker from '../../Controls/CampusPicker.js';
import { defineComponent } from '../../Vendor/Vue/vue.js';
import store from '../../Store/Index.js';
import Campus from '../../Types/Models/Campus.js';
import DefinedType from '../../Types/Models/DefinedType.js';
import DefinedValue from '../../Types/Models/DefinedValue.js';

export default defineComponent({
    name: 'Test.ControlGallery',
    components: {
        PaneledBlockTemplate,
        DefinedTypePicker,
        DefinedValuePicker,
        CampusPicker
    },
    data() {
        return {
            definedTypeGuid: '',
            definedValueGuid: '',
            campusGuid: '',
            definedValue: null as DefinedValue | null
        };
    },
    methods: {
        onDefinedValueChange(definedValue: DefinedValue | null) {
            this.definedValue = definedValue;
        }
    },
    computed: {
        campusName(): string {
            const campus = store.getters['campuses/getByGuid'](this.campusGuid) as Campus;
            return campus ? campus.Name : '';
        },
        definedTypeName(): string {
            const definedType = store.getters['definedTypes/getByGuid'](this.definedTypeGuid) as DefinedType;
            return definedType ? definedType.Name : '';
        },
        definedValueName(): string {
            return this.definedValue ? this.definedValue.Value : '';
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
                <DefinedTypePicker v-model="definedTypeGuid" />
                <DefinedValuePicker v-model="definedValueGuid" @update:model="onDefinedValueChange" :definedTypeGuid="definedTypeGuid" />
                <CampusPicker v-model="campusGuid" />
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
                <p>
                    <strong>Defined Value Guid</strong>
                    {{definedValueGuid}}
                    <span v-if="definedValueName">({{definedValueName}})</span>
                </p>
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
