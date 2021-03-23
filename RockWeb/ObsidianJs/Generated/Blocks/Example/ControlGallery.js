System.register(["../../Templates/PaneledBlockTemplate", "../../Controls/DefinedTypePicker", "../../Controls/DefinedValuePicker", "../../Controls/CampusPicker", "vue", "../../Store/Index", "../../Elements/TextBox", "../../Elements/EmailBox", "../../Elements/CurrencyBox", "../../Elements/PanelWidget", "../../Elements/DatePicker", "../../Elements/BirthdayPicker", "../../Elements/NumberUpDown", "../../Controls/AddressControl", "../../Elements/Toggle", "../../Elements/ItemsWithPreAndPostHtml"], function (exports_1, context_1) {
    "use strict";
    var PaneledBlockTemplate_1, DefinedTypePicker_1, DefinedValuePicker_1, CampusPicker_1, vue_1, Index_1, TextBox_1, EmailBox_1, CurrencyBox_1, PanelWidget_1, DatePicker_1, BirthdayPicker_1, NumberUpDown_1, AddressControl_1, Toggle_1, ItemsWithPreAndPostHtml_1, GalleryAndResult;
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
            }
        ],
        execute: function () {
            GalleryAndResult = vue_1.defineComponent({
                name: 'GalleryAndResult',
                components: {
                    PanelWidget: PanelWidget_1.default
                },
                template: "\n<PanelWidget>\n    <template #header><slot name=\"header\" /></template>\n    <div class=\"row\">\n        <div class=\"col-md-6\">\n            <slot name=\"gallery\" />\n        </div>\n        <div class=\"col-md-6\">\n            <slot name=\"result\" />\n        </div>\n    </div>\n</PanelWidget>"
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
                    ItemsWithPreAndPostHtml: ItemsWithPreAndPostHtml_1.default
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
                        ]
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
                template: "\n<PaneledBlockTemplate>\n    <template v-slot:title>\n        <i class=\"fa fa-flask\"></i>\n        Obsidian Control Gallery\n    </template>\n    <template v-slot:default>\n        <GalleryAndResult>\n            <template #header>\n                TextBox\n            </template>\n            <template #gallery>\n                <TextBox label=\"Text 1\" v-model=\"text\" :maxLength=\"10\" showCountDown />\n                <TextBox label=\"Text 2\" v-model=\"text\" />\n            </template>\n            <template #result>\n                {{text}}\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                DatePicker\n            </template>\n            <template #gallery>\n                <DatePicker label=\"Date 1\" v-model=\"date\" />\n                <DatePicker label=\"Date 2\" v-model=\"date\" />\n            </template>\n            <template #result>\n                {{date === null ? 'null' : date}}\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                CurrencyBox\n            </template>\n            <template #gallery>\n                <CurrencyBox label=\"Currency 1\" v-model=\"currency\" />\n                <CurrencyBox label=\"Currency 2\" v-model=\"currency\" />\n            </template>\n            <template #result>\n                {{currency}}\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                EmailBox\n            </template>\n            <template #gallery>\n                <EmailBox label=\"EmailBox 1\" v-model=\"email\" />\n                <EmailBox label=\"EmailBox 2\" v-model=\"email\" />\n            </template>\n            <template #result>\n                {{email}}\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                BirthdayPicker\n            </template>\n            <template #gallery>\n                <BirthdayPicker label=\"BirthdayPicker 1\" v-model=\"birthday\" />\n                <BirthdayPicker label=\"BirthdayPicker 2\" v-model=\"birthday\" />\n            </template>\n            <template #result>\n                {{birthday.Month}} / {{birthday.Day}} / {{birthday.Year}}\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                Defined Type and Value\n            </template>\n            <template #gallery>\n                <DefinedTypePicker v-model=\"definedTypeGuid\" />\n                <DefinedValuePicker v-model=\"definedValueGuid\" @update:model=\"onDefinedValueChange\" :definedTypeGuid=\"definedTypeGuid\" />\n            </template>\n            <template #result>\n                <p>\n                    <strong>Defined Type Guid</strong>\n                    {{definedTypeGuid}}\n                    <span v-if=\"definedTypeName\">({{definedTypeName}})</span>\n                </p>\n                <p>\n                    <strong>Defined Value Guid</strong>\n                    {{definedValueGuid}}\n                    <span v-if=\"definedValueName\">({{definedValueName}})</span>\n                </p>\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                CampusPicker\n            </template>\n            <template #gallery>\n                <CampusPicker v-model=\"campusGuid\" />\n                <CampusPicker v-model=\"campusGuid\" label=\"Campus 2\" />\n            </template>\n            <template #result>\n                <p>\n                    <strong>Campus Guid</strong>\n                    {{campusGuid}}\n                    <span v-if=\"campusName\">({{campusName}})</span>\n                </p>\n                <p>\n                    <strong>Campus Id</strong>\n                    {{campusId}}\n                </p>\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                NumberUpDown\n            </template>\n            <template #gallery>\n                <NumberUpDown label=\"NumberUpDown 1\" v-model=\"numberUpDown\" />\n                <NumberUpDown label=\"NumberUpDown 2\" v-model=\"numberUpDown\" />\n            </template>\n            <template #result>\n                {{numberUpDown}}\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                AddressControl\n            </template>\n            <template #gallery>\n                <AddressControl v-model=\"address\" />\n                <AddressControl label=\"Address 2\" v-model=\"address\" />\n            </template>\n            <template #result>\n                <pre>{{JSON.stringify(address, null, 2)}}</pre>\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                Toggle\n            </template>\n            <template #gallery>\n                <Toggle label=\"Toggle 1\" v-model=\"toggle\" />\n                <Toggle label=\"Toggle 2\" v-model=\"toggle\" />\n            </template>\n            <template #result>\n                {{toggle}}\n            </template>\n        </GalleryAndResult>\n        <GalleryAndResult>\n            <template #header>\n                ItemsWithPreAndPostHtml\n            </template>\n            <template #gallery>\n                <TextBox label=\"Item 1 - Pre Html\" v-model=\"prePostHtmlItems[0].PreHtml\" />\n                <TextBox label=\"Item 1 - Post Html\" v-model=\"prePostHtmlItems[0].PostHtml\" />\n                <TextBox label=\"Item 2 - Pre Html\" v-model=\"prePostHtmlItems[1].PreHtml\" />\n                <TextBox label=\"Item 2 - Post Html\" v-model=\"prePostHtmlItems[1].PostHtml\" />\n            </template>\n            <template #result>\n                <ItemsWithPreAndPostHtml :items=\"prePostHtmlItems\">\n                    <template #item1>\n                        <div style=\"background-color: #fcc; padding: 5px;\">This is item 1</div>\n                    </template>\n                    <template #item2>\n                        <div style=\"background-color: #ccf; padding: 5px;\">This is item 2</div>\n                    </template>\n                </ItemsWithPreAndPostHtml>\n            </template>\n        </GalleryAndResult>\n    </template>\n</PaneledBlockTemplate>"
            }));
        }
    };
});
//# sourceMappingURL=ControlGallery.js.map