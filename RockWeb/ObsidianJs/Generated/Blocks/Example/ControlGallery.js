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
System.register(["../../Templates/PaneledBlockTemplate", "../../Controls/DefinedTypePicker", "../../Controls/DefinedValuePicker", "../../Controls/CampusPicker", "vue", "../../Store/Index", "../../Elements/TextBox", "../../Elements/EmailBox", "../../Elements/CurrencyBox", "../../Elements/PanelWidget", "../../Elements/DatePicker", "../../Elements/BirthdayPicker", "../../Elements/NumberUpDown", "../../Controls/AddressControl", "../../Elements/Toggle", "../../Elements/ItemsWithPreAndPostHtml", "../../Elements/StaticFormControl", "../../Elements/ProgressTracker", "../../Controls/RockForm", "../../Elements/RockButton", "../../Elements/RadioButtonList", "../../Elements/DropDownList", "../../Controls/Dialog", "../../Elements/CheckBox", "../../Elements/PhoneNumberBox", "../../Elements/HelpBlock"], function (exports_1, context_1) {
    "use strict";
    var PaneledBlockTemplate_1, DefinedTypePicker_1, DefinedValuePicker_1, CampusPicker_1, vue_1, Index_1, TextBox_1, EmailBox_1, CurrencyBox_1, PanelWidget_1, DatePicker_1, BirthdayPicker_1, NumberUpDown_1, AddressControl_1, Toggle_1, ItemsWithPreAndPostHtml_1, StaticFormControl_1, ProgressTracker_1, RockForm_1, RockButton_1, RadioButtonList_1, DropDownList_1, Dialog_1, CheckBox_1, PhoneNumberBox_1, HelpBlock_1, GalleryAndResult, PhoneNumberBoxGallery, HelpBlockGallery, DropDownListGallery, RadioButtonListGallery, CheckBoxGallery, DialogGallery;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (PaneledBlockTemplate_1_1) {
                PaneledBlockTemplate_1 = PaneledBlockTemplate_1_1;
            },
            function (DefinedTypePicker_1_1) {
                DefinedTypePicker_1 = DefinedTypePicker_1_1;
            },
            function (DefinedValuePicker_1_1) {
                DefinedValuePicker_1 = DefinedValuePicker_1_1;
            },
            function (CampusPicker_1_1) {
                CampusPicker_1 = CampusPicker_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            },
            function (TextBox_1_1) {
                TextBox_1 = TextBox_1_1;
            },
            function (EmailBox_1_1) {
                EmailBox_1 = EmailBox_1_1;
            },
            function (CurrencyBox_1_1) {
                CurrencyBox_1 = CurrencyBox_1_1;
            },
            function (PanelWidget_1_1) {
                PanelWidget_1 = PanelWidget_1_1;
            },
            function (DatePicker_1_1) {
                DatePicker_1 = DatePicker_1_1;
            },
            function (BirthdayPicker_1_1) {
                BirthdayPicker_1 = BirthdayPicker_1_1;
            },
            function (NumberUpDown_1_1) {
                NumberUpDown_1 = NumberUpDown_1_1;
            },
            function (AddressControl_1_1) {
                AddressControl_1 = AddressControl_1_1;
            },
            function (Toggle_1_1) {
                Toggle_1 = Toggle_1_1;
            },
            function (ItemsWithPreAndPostHtml_1_1) {
                ItemsWithPreAndPostHtml_1 = ItemsWithPreAndPostHtml_1_1;
            },
            function (StaticFormControl_1_1) {
                StaticFormControl_1 = StaticFormControl_1_1;
            },
            function (ProgressTracker_1_1) {
                ProgressTracker_1 = ProgressTracker_1_1;
            },
            function (RockForm_1_1) {
                RockForm_1 = RockForm_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            },
            function (RadioButtonList_1_1) {
                RadioButtonList_1 = RadioButtonList_1_1;
            },
            function (DropDownList_1_1) {
                DropDownList_1 = DropDownList_1_1;
            },
            function (Dialog_1_1) {
                Dialog_1 = Dialog_1_1;
            },
            function (CheckBox_1_1) {
                CheckBox_1 = CheckBox_1_1;
            },
            function (PhoneNumberBox_1_1) {
                PhoneNumberBox_1 = PhoneNumberBox_1_1;
            },
            function (HelpBlock_1_1) {
                HelpBlock_1 = HelpBlock_1_1;
            }
        ],
        execute: function () {
            GalleryAndResult = vue_1.defineComponent({
                name: 'GalleryAndResult',
                components: {
                    PanelWidget: PanelWidget_1.default
                },
                props: {
                    splitWidth: {
                        type: Boolean,
                        default: true
                    }
                },
                template: "\n<PanelWidget>\n    <template #header><slot name=\"header\" /></template>\n    <div v-if=\"splitWidth\" class=\"row\">\n        <div class=\"col-md-6\">\n            <slot name=\"gallery\" />\n        </div>\n        <div class=\"col-md-6\">\n            <slot name=\"result\" />\n        </div>\n    </div>\n    <template v-else>\n        <div>\n            <slot name=\"gallery\" />\n        </div>\n        <div>\n            <slot name=\"result\" />\n        </div>\n    </template>\n</PanelWidget>"
            });
            PhoneNumberBoxGallery = vue_1.defineComponent({
                name: 'PhoneNumberBoxGallery',
                components: {
                    GalleryAndResult: GalleryAndResult,
                    PhoneNumberBox: PhoneNumberBox_1.default
                },
                data: function () {
                    return {
                        phoneNumber: ''
                    };
                },
                template: "\n<GalleryAndResult>\n    <template #header>\n        PhoneNumberBox\n    </template>\n    <template #gallery>\n        <PhoneNumberBox label=\"Phone 1\" v-model=\"phoneNumber\" />\n        <PhoneNumberBox label=\"Phone 2\" v-model=\"phoneNumber\" />\n    </template>\n    <template #result>\n        {{phoneNumber}}\n    </template>\n</GalleryAndResult>"
            });
            HelpBlockGallery = vue_1.defineComponent({
                name: 'HelpBlockGallery',
                components: {
                    GalleryAndResult: GalleryAndResult,
                    HelpBlock: HelpBlock_1.default
                },
                template: "\n<GalleryAndResult>\n    <template #header>\n        HelpBlock\n    </template>\n    <template #gallery>\n        <HelpBlock text=\"This is some helpful text that explains something.\" />\n    </template>\n</GalleryAndResult>"
            });
            DropDownListGallery = vue_1.defineComponent({
                name: 'DropDownListGallery',
                components: {
                    GalleryAndResult: GalleryAndResult,
                    DropDownList: DropDownList_1.default
                },
                data: function () {
                    return {
                        value: 'a',
                        options: [
                            { key: 'a', text: 'A Text', value: 'a' },
                            { key: 'b', text: 'B Text', value: 'b' },
                            { key: 'c', text: 'C Text', value: 'c' },
                            { key: 'd', text: 'D Text', value: 'd' }
                        ]
                    };
                },
                template: "\n<GalleryAndResult>\n    <template #header>\n        DropDownList\n    </template>\n    <template #gallery>\n        <DropDownList label=\"Select 1\" v-model=\"value\" :options=\"options\" />\n        <DropDownList label=\"Select 2\" v-model=\"value\" :options=\"options\" />\n        <DropDownList label=\"Enhanced Select 1\" v-model=\"value\" :options=\"options\" enhanceForLongLists />\n        <DropDownList label=\"Enhanced Select 2\" v-model=\"value\" :options=\"options\" enhanceForLongLists />\n    </template>\n    <template #result>\n        {{value}}\n    </template>\n</GalleryAndResult>"
            });
            RadioButtonListGallery = vue_1.defineComponent({
                name: 'RadioButtonListGallery',
                components: {
                    GalleryAndResult: GalleryAndResult,
                    RadioButtonList: RadioButtonList_1.default,
                    Toggle: Toggle_1.default,
                    NumberUpDown: NumberUpDown_1.default
                },
                data: function () {
                    return {
                        value: 'a',
                        isHorizontal: true,
                        repeatColumns: 0,
                        options: [
                            { key: 'a', text: 'A Text', value: 'a' },
                            { key: 'b', text: 'B Text', value: 'b' },
                            { key: 'c', text: 'C Text', value: 'c' },
                            { key: 'd', text: 'D Text', value: 'd' },
                            { key: 'e', text: 'E Text', value: 'e' },
                            { key: 'f', text: 'F Text', value: 'f' },
                            { key: 'g', text: 'G Text', value: 'g' }
                        ]
                    };
                },
                template: "\n<GalleryAndResult :splitWidth=\"false\">\n    <template #header>\n        RadioButtonList\n    </template>\n    <template #gallery>\n        <NumberUpDown label=\"Horizontal Columns\" v-model=\"repeatColumns\" :min=\"0\" />\n        <Toggle label=\"Horizontal\" v-model=\"isHorizontal\" />\n        <RadioButtonList label=\"Radio List 1\" v-model=\"value\" :options=\"options\" :horizontal=\"isHorizontal\" :repeatColumns=\"repeatColumns\" />\n        <RadioButtonList label=\"Radio List 2\" v-model=\"value\" :options=\"options\" />\n    </template>\n    <template #result>\n        Value: {{value}}\n    </template>\n</GalleryAndResult>"
            });
            CheckBoxGallery = vue_1.defineComponent({
                name: 'CheckBoxGallery',
                components: {
                    GalleryAndResult: GalleryAndResult,
                    CheckBox: CheckBox_1.default
                },
                data: function () {
                    return {
                        isChecked: false
                    };
                },
                template: "\n<GalleryAndResult>\n    <template #header>\n        CheckBox\n    </template>\n    <template #gallery>\n        <CheckBox label=\"Check 1\" v-model=\"isChecked\" />\n        <CheckBox label=\"Check 2\" v-model=\"isChecked\" />\n    </template>\n    <template #result>\n        {{isChecked}}\n    </template>\n</GalleryAndResult>"
            });
            DialogGallery = vue_1.defineComponent({
                name: 'DialogGallery',
                components: {
                    GalleryAndResult: GalleryAndResult,
                    RockButton: RockButton_1.default,
                    Dialog: Dialog_1.default,
                    CheckBox: CheckBox_1.default
                },
                data: function () {
                    return {
                        isDialogVisible: false,
                        isDismissible: false
                    };
                },
                template: "\n<GalleryAndResult>\n    <template #header>\n        Dialog\n    </template>\n    <template #gallery>\n        <RockButton @click=\"isDialogVisible = true\">Show</RockButton>\n        <CheckBox label=\"Dismissible\" v-model=\"isDismissible\" />\n    </template>\n    <template #result>\n        <Dialog v-model=\"isDialogVisible\" :dismissible=\"isDismissible\">\n            <template #header>\n                <h4>Romans 11:33-36</h4>\n            </template>\n            <template #default>\n                <p>\n                    Oh, the depth of the riches<br />\n                    and the wisdom and the knowledge of God!<br />\n                    How unsearchable his judgments<br />\n                    and untraceable his ways!<br />\n                    For who has known the mind of the Lord?<br />\n                    Or who has been his counselor?<br />\n                    And who has ever given to God,<br />\n                    that he should be repaid?<br />\n                    For from him and through him<br />\n                    and to him are all things.<br />\n                    To him be the glory forever. Amen.\n                </p>\n            </template>\n            <template #footer>\n                <RockButton @click=\"isDialogVisible = false\" btnType=\"primary\">OK</RockButton>\n                <RockButton @click=\"isDialogVisible = false\" btnType=\"default\">Cancel</RockButton>\n            </template>\n        </Dialog>\n    </template>\n</GalleryAndResult>"
            });
            exports_1("default", vue_1.defineComponent({
                name: 'Example.ControlGallery',
                components: {
                    PaneledBlockTemplate: PaneledBlockTemplate_1.default,
                    DefinedTypePicker: DefinedTypePicker_1.default,
                    DefinedValuePicker: DefinedValuePicker_1.default,
                    CampusPicker: CampusPicker_1.default,
                    GalleryAndResult: GalleryAndResult,
                    TextBox: TextBox_1.default,
                    CurrencyBox: CurrencyBox_1.default,
                    EmailBox: EmailBox_1.default,
                    DatePicker: DatePicker_1.default,
                    BirthdayPicker: BirthdayPicker_1.default,
                    NumberUpDown: NumberUpDown_1.default,
                    AddressControl: AddressControl_1.default,
                    Toggle: Toggle_1.default,
                    ItemsWithPreAndPostHtml: ItemsWithPreAndPostHtml_1.default,
                    StaticFormControl: StaticFormControl_1.default,
                    ProgressTracker: ProgressTracker_1.default,
                    RockForm: RockForm_1.default,
                    RockButton: RockButton_1.default,
                    RadioButtonListGallery: RadioButtonListGallery,
                    DialogGallery: DialogGallery,
                    CheckBoxGallery: CheckBoxGallery,
                    PhoneNumberBoxGallery: PhoneNumberBoxGallery,
                    DropDownListGallery: DropDownListGallery,
                    HelpBlockGallery: HelpBlockGallery
                },
                data: function () {
                    return {
                        definedTypeGuid: '',
                        definedValueGuid: '',
                        campusGuid: '',
                        definedValue: null,
                        text: 'Some two-way bound text',
                        currency: 1.234,
                        email: 'joe@joes.co',
                        date: null,
                        numberUpDown: 1,
                        address: AddressControl_1.getDefaultAddressControlModel(),
                        toggle: false,
                        birthday: {
                            Month: 1,
                            Day: 1,
                            Year: 2020
                        },
                        prePostHtmlItems: [
                            { PreHtml: '<div class="row"><div class="col-sm-6">', PostHtml: '</div>', SlotName: 'item1' },
                            { PreHtml: '<div class="col-sm-6">', PostHtml: '</div></div>', SlotName: 'item2' }
                        ],
                        progressTrackerIndex: 0,
                        progressTrackerItems: [
                            { Key: 'S', Title: 'Start', Subtitle: 'The beginning' },
                            { Key: '1', Title: 'Step 1', Subtitle: 'The first step' },
                            { Key: '2', Title: 'Step 2', Subtitle: 'The second step' },
                            { Key: '3', Title: 'Step 3', Subtitle: 'The third step' },
                            { Key: '4', Title: 'Step 4', Subtitle: 'The fourth step' },
                            { Key: '5', Title: 'Step 5', Subtitle: 'The fifth step' },
                            { Key: '6', Title: 'Step 6', Subtitle: 'The sixth step' },
                            { Key: '7', Title: 'Step 7', Subtitle: 'The seventh step' },
                            { Key: '8', Title: 'Step 8', Subtitle: 'The eighth step' },
                            { Key: 'F', Title: 'Finish', Subtitle: 'The finish' }
                        ],
                        ruleTestCurrency: 1,
                        ruleTestText: '',
                        rules: 'required',
                        isDialogVisible: false
                    };
                },
                methods: {
                    onDefinedValueChange: function (definedValue) {
                        this.definedValue = definedValue;
                    }
                },
                computed: {
                    campus: function () {
                        return Index_1.default.getters['campuses/getByGuid'](this.campusGuid) || null;
                    },
                    campusName: function () {
                        var _a;
                        return ((_a = this.campus) === null || _a === void 0 ? void 0 : _a.Name) || '';
                    },
                    campusId: function () {
                        return this.campus ? this.campus.Id : null;
                    },
                    definedTypeName: function () {
                        var definedType = Index_1.default.getters['definedTypes/getByGuid'](this.definedTypeGuid);
                        return (definedType === null || definedType === void 0 ? void 0 : definedType.Name) || '';
                    },
                    definedValueName: function () {
                        var _a;
                        return ((_a = this.definedValue) === null || _a === void 0 ? void 0 : _a.Value) || '';
                    }
                },
                template: "\n<PaneledBlockTemplate>\n    <template v-slot:title>\n        <i class=\"fa fa-flask\"></i>\n        Obsidian Control Gallery\n    </template>\n    <template v-slot:default>\n        <GalleryAndResult>\n            <template #header>\n                TextBox\n            </template>\n            <template #gallery>\n                <TextBox label=\"Text 1\" v-model=\"text\" :maxLength=\"10\" showCountDown />\n                <TextBox label=\"Text 2\" v-model=\"text\" />\n            </template>\n            <template #result>\n                {{text}}\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                DatePicker\n            </template>\n            <template #gallery>\n                <DatePicker label=\"Date 1\" v-model=\"date\" />\n                <DatePicker label=\"Date 2\" v-model=\"date\" />\n            </template>\n            <template #result>\n                {{date === null ? 'null' : date}}\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                CurrencyBox\n            </template>\n            <template #gallery>\n                <CurrencyBox label=\"Currency 1\" v-model=\"currency\" />\n                <CurrencyBox label=\"Currency 2\" v-model=\"currency\" />\n            </template>\n            <template #result>\n                {{currency}}\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                EmailBox\n            </template>\n            <template #gallery>\n                <EmailBox label=\"EmailBox 1\" v-model=\"email\" />\n                <EmailBox label=\"EmailBox 2\" v-model=\"email\" />\n            </template>\n            <template #result>\n                {{email}}\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                BirthdayPicker\n            </template>\n            <template #gallery>\n                <BirthdayPicker label=\"BirthdayPicker 1\" v-model=\"birthday\" />\n                <BirthdayPicker label=\"BirthdayPicker 2\" v-model=\"birthday\" />\n            </template>\n            <template #result>\n                {{birthday.Month}} / {{birthday.Day}} / {{birthday.Year}}\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                Defined Type and Value\n            </template>\n            <template #gallery>\n                <DefinedTypePicker v-model=\"definedTypeGuid\" />\n                <DefinedValuePicker v-model=\"definedValueGuid\" @update:model=\"onDefinedValueChange\" :definedTypeGuid=\"definedTypeGuid\" />\n            </template>\n            <template #result>\n                <p>\n                    <strong>Defined Type Guid</strong>\n                    {{definedTypeGuid}}\n                    <span v-if=\"definedTypeName\">({{definedTypeName}})</span>\n                </p>\n                <p>\n                    <strong>Defined Value Guid</strong>\n                    {{definedValueGuid}}\n                    <span v-if=\"definedValueName\">({{definedValueName}})</span>\n                </p>\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                CampusPicker\n            </template>\n            <template #gallery>\n                <CampusPicker v-model=\"campusGuid\" />\n                <CampusPicker v-model=\"campusGuid\" label=\"Campus 2\" />\n            </template>\n            <template #result>\n                <p>\n                    <strong>Campus Guid</strong>\n                    {{campusGuid}}\n                    <span v-if=\"campusName\">({{campusName}})</span>\n                </p>\n                <p>\n                    <strong>Campus Id</strong>\n                    {{campusId}}\n                </p>\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                NumberUpDown\n            </template>\n            <template #gallery>\n                <NumberUpDown label=\"NumberUpDown 1\" v-model=\"numberUpDown\" />\n                <NumberUpDown label=\"NumberUpDown 2\" v-model=\"numberUpDown\" />\n            </template>\n            <template #result>\n                {{numberUpDown}}\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                StaticFormControl\n            </template>\n            <template #gallery>\n                <StaticFormControl label=\"StaticFormControl 1\" v-model=\"numberUpDown\" />\n                <StaticFormControl label=\"StaticFormControl 2\" v-model=\"numberUpDown\" />\n            </template>\n            <template #result>\n                {{numberUpDown}}\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                AddressControl\n            </template>\n            <template #gallery>\n                <AddressControl v-model=\"address\" />\n                <AddressControl label=\"Address 2\" v-model=\"address\" />\n            </template>\n            <template #result>\n                <pre>{{JSON.stringify(address, null, 2)}}</pre>\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                Toggle\n            </template>\n            <template #gallery>\n                <Toggle label=\"Toggle 1\" v-model=\"toggle\" />\n                <Toggle label=\"Toggle 2\" v-model=\"toggle\" />\n            </template>\n            <template #result>\n                {{toggle}}\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                ItemsWithPreAndPostHtml\n            </template>\n            <template #gallery>\n                <TextBox label=\"Item 1 - Pre Html\" v-model=\"prePostHtmlItems[0].PreHtml\" />\n                <TextBox label=\"Item 1 - Post Html\" v-model=\"prePostHtmlItems[0].PostHtml\" />\n                <TextBox label=\"Item 2 - Pre Html\" v-model=\"prePostHtmlItems[1].PreHtml\" />\n                <TextBox label=\"Item 2 - Post Html\" v-model=\"prePostHtmlItems[1].PostHtml\" />\n            </template>\n            <template #result>\n                <ItemsWithPreAndPostHtml :items=\"prePostHtmlItems\">\n                    <template #item1>\n                        <div style=\"background-color: #fcc; padding: 5px;\">This is item 1</div>\n                    </template>\n                    <template #item2>\n                        <div style=\"background-color: #ccf; padding: 5px;\">This is item 2</div>\n                    </template>\n                </ItemsWithPreAndPostHtml>\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult :splitWidth=\"false\">\n            <template #header>\n                ProgressTracker\n            </template>\n            <template #gallery>\n                <NumberUpDown label=\"Index\" v-model=\"progressTrackerIndex\" :min=\"-100\" :max=\"100\" />\n            </template>\n            <template #result>\n                <ProgressTracker :items=\"progressTrackerItems\" :currentIndex=\"progressTrackerIndex\" />\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult :splitWidth=\"false\">\n            <template #header>\n                Rules\n            </template>\n            <template #gallery>\n                <TextBox label=\"Rules\" v-model=\"rules\" help=\"Try 'required', 'gte:1', 'lt:2', and others. Combine rules like this: 'required|lt:7|gt:6'\" />\n                <hr />\n                <RockForm>\n                    <TextBox label=\"Text\" v-model=\"ruleTestText\" :rules=\"rules\" />\n                    <CurrencyBox label=\"Currency\" v-model=\"ruleTestCurrency\" :rules=\"rules\" />\n                    <RockButton btnType=\"primary\" type=\"submit\">Test</RockButton>\n                </RockForm>\n            </template>\n        </GalleryAndResult>\n        <RadioButtonListGallery />\n        <DialogGallery />\n        <CheckBoxGallery />\n        <PhoneNumberBoxGallery />\n        <DropDownListGallery />\n        <HelpBlockGallery />\n    </template>\n</PaneledBlockTemplate>"
            }));
        }
    };
});
//# sourceMappingURL=ControlGallery.js.map