System.register(["vue", "../Elements/DropDownList", "../Elements/RockLabel", "../Elements/TextBox", "../Rules/Index", "../Util/Guid"], function (exports_1, context_1) {
    "use strict";
    var vue_1, DropDownList_1, RockLabel_1, TextBox_1, Index_1, Guid_1;
    var __moduleName = context_1 && context_1.id;
    function getDefaultAddressControlModel() {
        return {
            Street1: '',
            Street2: '',
            City: '',
            State: 'AZ',
            PostalCode: '',
            Country: 'US'
        };
    }
    exports_1("getDefaultAddressControlModel", getDefaultAddressControlModel);
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (DropDownList_1_1) {
                DropDownList_1 = DropDownList_1_1;
            },
            function (RockLabel_1_1) {
                RockLabel_1 = RockLabel_1_1;
            },
            function (TextBox_1_1) {
                TextBox_1 = TextBox_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            },
            function (Guid_1_1) {
                Guid_1 = Guid_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'AddressControl',
                components: {
                    TextBox: TextBox_1.default,
                    RockLabel: RockLabel_1.default,
                    DropDownList: DropDownList_1.default
                },
                props: {
                    modelValue: {
                        type: Object,
                        required: true
                    },
                    label: {
                        type: String,
                        default: 'Address'
                    },
                    help: {
                        type: String,
                        default: ''
                    },
                    rules: {
                        type: String,
                        default: ''
                    }
                },
                data: function () {
                    return {
                        state: '',
                        uniqueId: "rock-addresscontrol-" + Guid_1.newGuid(),
                        stateOptions: [
                            { key: 'AL', value: 'AL', text: 'AL' },
                            { key: 'AK', value: 'AK', text: 'AK' },
                            { key: 'AS', value: 'AS', text: 'AS' },
                            { key: 'AZ', value: 'AZ', text: 'AZ' },
                            { key: 'AR', value: 'AR', text: 'AR' },
                            { key: 'CA', value: 'CA', text: 'CA' },
                            { key: 'CO', value: 'CO', text: 'CO' },
                            { key: 'CT', value: 'CT', text: 'CT' },
                            { key: 'DE', value: 'DE', text: 'DE' },
                            { key: 'DC', value: 'DC', text: 'DC' },
                            { key: 'FM', value: 'FM', text: 'FM' },
                            { key: 'FL', value: 'FL', text: 'FL' },
                            { key: 'GA', value: 'GA', text: 'GA' },
                            { key: 'GU', value: 'GU', text: 'GU' },
                            { key: 'HI', value: 'HI', text: 'HI' },
                            { key: 'ID', value: 'ID', text: 'ID' },
                            { key: 'IL', value: 'IL', text: 'IL' },
                            { key: 'IN', value: 'IN', text: 'IN' },
                            { key: 'IA', value: 'IA', text: 'IA' },
                            { key: 'KS', value: 'KS', text: 'KS' },
                            { key: 'KY', value: 'KY', text: 'KY' },
                            { key: 'LA', value: 'LA', text: 'LA' },
                            { key: 'ME', value: 'ME', text: 'ME' },
                            { key: 'MH', value: 'MH', text: 'MH' },
                            { key: 'MD', value: 'MD', text: 'MD' },
                            { key: 'MA', value: 'MA', text: 'MA' },
                            { key: 'MI', value: 'MI', text: 'MI' },
                            { key: 'MN', value: 'MN', text: 'MN' },
                            { key: 'MS', value: 'MS', text: 'MS' },
                            { key: 'MO', value: 'MO', text: 'MO' },
                            { key: 'MT', value: 'MT', text: 'MT' },
                            { key: 'NE', value: 'NE', text: 'NE' },
                            { key: 'NV', value: 'NV', text: 'NV' },
                            { key: 'NH', value: 'NH', text: 'NH' },
                            { key: 'NJ', value: 'NJ', text: 'NJ' },
                            { key: 'NM', value: 'NM', text: 'NM' },
                            { key: 'NY', value: 'NY', text: 'NY' },
                            { key: 'NC', value: 'NC', text: 'NC' },
                            { key: 'ND', value: 'ND', text: 'ND' },
                            { key: 'MP', value: 'MP', text: 'MP' },
                            { key: 'OH', value: 'OH', text: 'OH' },
                            { key: 'OK', value: 'OK', text: 'OK' },
                            { key: 'OR', value: 'OR', text: 'OR' },
                            { key: 'PW', value: 'PW', text: 'PW' },
                            { key: 'PA', value: 'PA', text: 'PA' },
                            { key: 'PR', value: 'PR', text: 'PR' },
                            { key: 'RI', value: 'RI', text: 'RI' },
                            { key: 'SC', value: 'SC', text: 'SC' },
                            { key: 'SD', value: 'SD', text: 'SD' },
                            { key: 'TN', value: 'TN', text: 'TN' },
                            { key: 'TX', value: 'TX', text: 'TX' },
                            { key: 'UT', value: 'UT', text: 'UT' },
                            { key: 'VT', value: 'VT', text: 'VT' },
                            { key: 'VI', value: 'VI', text: 'VI' },
                            { key: 'VA', value: 'VA', text: 'VA' },
                            { key: 'WA', value: 'WA', text: 'WA' },
                            { key: 'WV', value: 'WV', text: 'WV' },
                            { key: 'WI', value: 'WI', text: 'WI' },
                            { key: 'WY', value: 'WY', text: 'WY' }
                        ]
                    };
                },
                computed: {
                    isRequired: function () {
                        var rules = Index_1.ruleStringToArray(this.rules);
                        return rules.indexOf('required') !== -1;
                    }
                },
                template: "\n<div class=\"form-group address-control\" :class=\"isRequired ? 'required' : ''\">\n    <RockLabel v-if=\"label || help\" :for=\"uniqueId\" :help=\"help\">\n        {{label}}\n    </RockLabel>\n    <div class=\"control-wrapper\">\n        <TextBox placeholder=\"Address Line 1\" :rules=\"rules\" v-model=\"modelValue.Street1\" validationTitle=\"Address Line 1\" />\n        <TextBox placeholder=\"Address Line 2\" v-model=\"modelValue.Street2\" validationTitle=\"Address Line 2\" />\n        <div class=\"form-row\">\n            <TextBox placeholder=\"City\" :rules=\"rules\" v-model=\"modelValue.City\" class=\"col-sm-6\" validationTitle=\"City\" />\n            <DropDownList :showBlankItem=\"false\" v-model=\"modelValue.State\" class=\"col-sm-3\" :options=\"stateOptions\" />\n            <TextBox placeholder=\"Zip\" :rules=\"rules\" v-model=\"modelValue.PostalCode\" class=\"col-sm-3\" validationTitle=\"Zip\" />\n        </div>\n    </div>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=AddressControl.js.map