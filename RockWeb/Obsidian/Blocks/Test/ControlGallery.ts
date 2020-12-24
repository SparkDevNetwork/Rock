import PaneledBlockTemplate from "../../Templates/PaneledBlockTemplate.js";
import DefinedTypePicker from "../../Controls/DefinedTypePicker.js";
import DefinedValuePicker from "../../Controls/DefinedValuePicker.js";
import CampusPicker from "../../Controls/CampusPicker.js";
import { defineComponent } from '../../Vendor/Vue/vue.js';
import store from '../../Store/index.js';

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
            campusGuid: ''
        };
    },
    computed: {
        campusName() {
            const campus = store.getters['campuses/getByGuid'](this.campusGuid);
            return campus ? campus.Name : '';
        },
        definedTypeName() {
            const definedType = store.getters['definedTypes/getByGuid'](this.definedTypeGuid);
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
})
