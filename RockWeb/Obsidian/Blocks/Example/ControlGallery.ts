import PaneledBlockTemplate from '../../Templates/PaneledBlockTemplate.js';
import DefinedTypePicker from '../../Controls/DefinedTypePicker.js';
import DefinedValuePicker from '../../Controls/DefinedValuePicker.js';
import CampusPicker from '../../Controls/CampusPicker.js';
import { defineComponent } from '../../Vendor/Vue/vue.js';
import store from '../../Store/Index.js';
import Campus from '../../Types/Models/Campus.js';
import DefinedType from '../../Types/Models/DefinedType.js';
import DefinedValue from '../../Types/Models/DefinedValue.js';
import TextBox from '../../Elements/TextBox.js';

const GalleryAndResult = defineComponent({
    name: 'GalleryAndResult',
    template: `
<div class="row">
    <div class="col-md-6">
        <slot name="gallery" />
    </div>
    <div class="col-md-6">
        <slot name="result" />
    </div>
</div>
<hr />`
});

export default defineComponent({
    name: 'Example.ControlGallery',
    components: {
        PaneledBlockTemplate,
        DefinedTypePicker,
        DefinedValuePicker,
        CampusPicker,
        GalleryAndResult,
        TextBox
    },
    data() {
        return {
            definedTypeGuid: '',
            definedValueGuid: '',
            campusGuid: '',
            campusId: 0 as number | null,
            definedValue: null as DefinedValue | null,
            text: 'Some two-way bound text'
        };
    },
    methods: {
        onDefinedValueChange(definedValue: DefinedValue | null) {
            this.definedValue = definedValue;
        }
    },
    computed: {
        campus(): Campus | null {
            return store.getters['campuses/getByGuid'](this.campusGuid) || null;
        },
        campusName(): string {
            return this.campus ? this.campus.Name : '';
        },
        definedTypeName(): string {
            const definedType = store.getters['definedTypes/getByGuid'](this.definedTypeGuid) as DefinedType;
            return definedType ? definedType.Name : '';
        },
        definedValueName(): string {
            return this.definedValue ? this.definedValue.Value : '';
        }
    },
    template: `
<PaneledBlockTemplate>
    <template v-slot:title>
        <i class="fa fa-flask"></i>
        Obsidian Control Gallery
    </template>
    <template v-slot:default>
        <GalleryAndResult>
            <template #gallery>
                <TextBox label="Text 1" v-model="text" />
                <TextBox label="Text 2" v-model="text" />
            </template>
            <template #result>
                {{text}}
            </template>
        </GalleryAndResult>
        <GalleryAndResult>
            <template #gallery>
                <DefinedTypePicker v-model="definedTypeGuid" />
                <DefinedValuePicker v-model="definedValueGuid" @update:model="onDefinedValueChange" :definedTypeGuid="definedTypeGuid" />
            </template>
            <template #result>
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
            </template>
        </GalleryAndResult>
        <GalleryAndResult>
            <template #gallery>
                <CampusPicker v-model="campusGuid" v-model:id="campusId" />
            </template>
            <template #result>
                <p>
                    <strong>Campus Guid</strong>
                    {{campusGuid}}
                    <span v-if="campusName">({{campusName}})</span>
                </p>
                <p>
                    <strong>Campus Id</strong>
                    {{campusId}}
                </p>
            </template>
        </GalleryAndResult>
    </template>
</PaneledBlockTemplate>`
});
