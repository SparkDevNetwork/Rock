define(["require", "exports", "../../Templates/PaneledBlockTemplate.js", "../../Controls/DefinedTypePicker.js", "../../Controls/DefinedValuePicker.js", "../../Controls/CampusPicker.js", "../../Vendor/Vue/vue.js", "../../Store/Index.js"], function (require, exports, PaneledBlockTemplate_js_1, DefinedTypePicker_js_1, DefinedValuePicker_js_1, CampusPicker_js_1, vue_js_1, Index_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = vue_js_1.defineComponent({
        name: 'Test.ControlGallery',
        components: {
            PaneledBlockTemplate: PaneledBlockTemplate_js_1.default,
            DefinedTypePicker: DefinedTypePicker_js_1.default,
            DefinedValuePicker: DefinedValuePicker_js_1.default,
            CampusPicker: CampusPicker_js_1.default
        },
        data: function () {
            return {
                definedTypeGuid: '',
                definedValueGuid: '',
                campusGuid: '',
                definedValue: null
            };
        },
        methods: {
            onDefinedValueChange: function (definedValue) {
                this.definedValue = definedValue;
            }
        },
        computed: {
            campusName: function () {
                var campus = Index_js_1.default.getters['campuses/getByGuid'](this.campusGuid);
                return campus ? campus.Name : '';
            },
            definedTypeName: function () {
                var definedType = Index_js_1.default.getters['definedTypes/getByGuid'](this.definedTypeGuid);
                return definedType ? definedType.Name : '';
            },
            definedValueName: function () {
                return this.definedValue ? this.definedValue.Value : '';
            }
        },
        template: "<PaneledBlockTemplate>\n    <template v-slot:title>\n        <i class=\"fa fa-flask\"></i>\n        Obsidian Control Gallery\n    </template>\n    <template v-slot:default>\n        <div class=\"row\">\n            <div class=\"col-sm-12 col-md-6 col-lg-4\">\n                <DefinedTypePicker v-model=\"definedTypeGuid\" />\n                <DefinedValuePicker v-model=\"definedValueGuid\" @update:model=\"onDefinedValueChange\" :definedTypeGuid=\"definedTypeGuid\" />\n                <CampusPicker v-model=\"campusGuid\" />\n            </div>\n        </div>\n        <hr />\n        <div class=\"row\">\n            <div class=\"col-sm-12\">\n                <p>\n                    <strong>Defined Type Guid</strong>\n                    {{definedTypeGuid}}\n                    <span v-if=\"definedTypeName\">({{definedTypeName}})</span>\n                </p>\n                <p>\n                    <strong>Defined Value Guid</strong>\n                    {{definedValueGuid}}\n                    <span v-if=\"definedValueName\">({{definedValueName}})</span>\n                </p>\n                <p>\n                    <strong>Campus Guid</strong>\n                    {{campusGuid}}\n                    <span v-if=\"campusName\">({{campusName}})</span>\n                </p>\n            </div>\n        </div>\n    </template>\n</PaneledBlockTemplate>"
    });
});
//# sourceMappingURL=ControlGallery.js.map