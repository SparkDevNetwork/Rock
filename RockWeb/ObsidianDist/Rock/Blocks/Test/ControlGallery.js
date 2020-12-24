define(["require", "exports", "../../Templates/PaneledBlockTemplate.js", "../../Controls/DefinedTypePicker.js", "../../Controls/DefinedValuePicker.js", "../../Controls/CampusPicker.js"], function (require, exports, PaneledBlockTemplate_js_1, DefinedTypePicker_js_1, DefinedValuePicker_js_1, CampusPicker_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = {
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
                campusGuid: ''
            };
        },
        computed: {
            campusName: function () {
                var campus = this.$store.getters['campuses/getByGuid'](this.campusGuid);
                return campus ? campus.Name : '';
            },
            definedTypeName: function () {
                var definedType = this.$store.getters['definedTypes/getByGuid'](this.definedTypeGuid);
                return definedType ? definedType.Name : '';
            }
        },
        template: "<PaneledBlockTemplate>\n    <template v-slot:title>\n        <i class=\"fa fa-flask\"></i>\n        Obsidian Control Gallery\n    </template>\n    <template v-slot:default>\n        <div class=\"row\">\n            <div class=\"col-sm-12 col-md-6 col-lg-4\">\n                <DefinedTypePicker label=\"Defined Type\" v-model=\"definedTypeGuid\" />\n                <DefinedValuePicker label=\"Defined Value\" v-model=\"definedValueGuid\" :defined-type-guid=\"definedTypeGuid\" />\n                <CampusPicker label=\"Campus\" v-model=\"campusGuid\" />\n            </div>\n        </div>\n        <hr />\n        <div class=\"row\">\n            <div class=\"col-sm-12\">\n                <p>\n                    <strong>Defined Type Guid</strong>\n                    {{definedTypeGuid}}\n                    <span v-if=\"definedTypeName\">({{definedTypeName}})</span>\n                </p>\n                <p><strong>Defined Value Guid</strong> {{definedValueGuid}}</p>\n                <p>\n                    <strong>Campus Guid</strong>\n                    {{campusGuid}}\n                    <span v-if=\"campusName\">({{campusName}})</span>\n                </p>\n            </div>\n        </div>\n    </template>\n</PaneledBlockTemplate>"
    };
});
//# sourceMappingURL=ControlGallery.js.map