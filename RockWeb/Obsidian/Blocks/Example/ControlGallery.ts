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
import PaneledBlockTemplate from '../../Templates/PaneledBlockTemplate.js';
import DefinedTypePicker from '../../Controls/DefinedTypePicker.js';
import DefinedValuePicker from '../../Controls/DefinedValuePicker.js';
import CampusPicker from '../../Controls/CampusPicker.js';
import { defineComponent } from 'vue';
import store from '../../Store/Index.js';
import TextBox from '../../Elements/TextBox.js';
import EmailBox from '../../Elements/EmailBox.js';
import DefinedValue from '../../ViewModels/CodeGenerated/DefinedValueViewModel.js';
import Campus from '../../ViewModels/CodeGenerated/CampusViewModel.js';
import DefinedType from '../../ViewModels/CodeGenerated/DefinedTypeViewModel.js';
import CurrencyBox from '../../Elements/CurrencyBox.js';
import PanelWidget from '../../Elements/PanelWidget.js';
import DatePicker from '../../Elements/DatePicker.js';
import { RockDateType } from '../../Util/RockDate.js';
import BirthdayPicker from '../../Elements/BirthdayPicker.js';
import NumberUpDown from '../../Elements/NumberUpDown.js';

const GalleryAndResult = defineComponent({
    name: 'GalleryAndResult',
    components: {
        PanelWidget
    },
    template: `
<PanelWidget>
    <template #header><slot name="header" /></template>
    <div class="row">
        <div class="col-md-6">
            <slot name="gallery" />
        </div>
        <div class="col-md-6">
            <slot name="result" />
        </div>
    </div>
</PanelWidget>`
});

export default defineComponent({
    name: 'Example.ControlGallery',
    components: {
        PaneledBlockTemplate,
        DefinedTypePicker,
        DefinedValuePicker,
        CampusPicker,
        GalleryAndResult,
        TextBox,
        CurrencyBox,
        EmailBox,
        DatePicker,
        BirthdayPicker,
        NumberUpDown
    },
    data() {
        return {
            definedTypeGuid: '',
            definedValueGuid: '',
            campusGuid: '',
            definedValue: null as DefinedValue | null,
            text: 'Some two-way bound text',
            currency: 1.234,
            email: 'joe@joes.co',
            date: null as RockDateType | null,
            numberUpDown: 1,
            birthday: {
                month: 1,
                day: 1,
                year: 2020
            }
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
            return this.campus?.Name || '';
        },
        campusId(): number | null {
            return this.campus ? this.campus.Id : null;
        },
        definedTypeName(): string {
            const definedType = store.getters['definedTypes/getByGuid'](this.definedTypeGuid) as DefinedType;
            return definedType?.Name || '';
        },
        definedValueName(): string {
            return this.definedValue?.Value || '';
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
            <template #header>
                TextBox
            </template>
            <template #gallery>
                <TextBox label="Text 1" v-model="text" :maxLength="10" showCountDown />
                <TextBox label="Text 2" v-model="text" />
            </template>
            <template #result>
                {{text}}
            </template>
        </GalleryAndResult>
        <GalleryAndResult>
            <template #header>
                DatePicker
            </template>
            <template #gallery>
                <DatePicker label="Date 1" v-model="date" />
                <DatePicker label="Date 2" v-model="date" />
            </template>
            <template #result>
                {{date === null ? 'null' : date}}
            </template>
        </GalleryAndResult>
        <GalleryAndResult>
            <template #header>
                CurrencyBox
            </template>
            <template #gallery>
                <CurrencyBox label="Currency 1" v-model="currency" />
                <CurrencyBox label="Currency 2" v-model="currency" />
            </template>
            <template #result>
                {{currency}}
            </template>
        </GalleryAndResult>
        <GalleryAndResult>
            <template #header>
                EmailBox
            </template>
            <template #gallery>
                <EmailBox label="EmailBox 1" v-model="email" />
                <EmailBox label="EmailBox 2" v-model="email" />
            </template>
            <template #result>
                {{email}}
            </template>
        </GalleryAndResult>
        <GalleryAndResult>
            <template #header>
                BirthdayPicker
            </template>
            <template #gallery>
                <BirthdayPicker label="BirthdayPicker 1" v-model:day="birthday.day" v-model:month="birthday.month" v-model:year="birthday.year" />
                <BirthdayPicker label="BirthdayPicker 2" v-model:day="birthday.day" v-model:month="birthday.month" v-model:year="birthday.year" />
            </template>
            <template #result>
                {{birthday.month}} / {{birthday.day}} / {{birthday.year}}
            </template>
        </GalleryAndResult>
        <GalleryAndResult>
            <template #header>
                Defined Type and Value
            </template>
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
            <template #header>
                CampusPicker
            </template>
            <template #gallery>
                <CampusPicker v-model="campusGuid" />
                <CampusPicker v-model="campusGuid" label="Campus 2" />
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
        <GalleryAndResult>
            <template #header>
                NumberUpDown
            </template>
            <template #gallery>
                <NumberUpDown label="NumberUpDown 1" v-model="numberUpDown" />
                <NumberUpDown label="NumberUpDown 2" v-model="numberUpDown" />
            </template>
            <template #result>
                {{numberUpDown}}
            </template>
        </GalleryAndResult>
    </template>
</PaneledBlockTemplate>`
});
