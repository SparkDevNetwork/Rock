System.register(["vue", "../Elements/DropDownList", "../Elements/RockLabel", "../Elements/TextBox", "../Rules/Index", "../Util/Guid"], function (exports_1, context_1) {
    "use strict";
    var vue_1, DropDownList_1, RockLabel_1, TextBox_1, Index_1, Guid_1;
    var __moduleName = context_1 && context_1.id;
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
                        uniqueId: "rock-addresscontrol-" + Guid_1.newGuid(),
                        states: [
                            'AL',
                            'AK',
                            'AS',
                            'AZ',
                            'AR',
                            'CA',
                            'CO',
                            'CT',
                            'DE',
                            'DC',
                            'FM',
                            'FL',
                            'GA',
                            'GU',
                            'HI',
                            'ID',
                            'IL',
                            'IN',
                            'IA',
                            'KS',
                            'KY',
                            'LA',
                            'ME',
                            'MH',
                            'MD',
                            'MA',
                            'MI',
                            'MN',
                            'MS',
                            'MO',
                            'MT',
                            'NE',
                            'NV',
                            'NH',
                            'NJ',
                            'NM',
                            'NY',
                            'NC',
                            'ND',
                            'MP',
                            'OH',
                            'OK',
                            'OR',
                            'PW',
                            'PA',
                            'PR',
                            'RI',
                            'SC',
                            'SD',
                            'TN',
                            'TX',
                            'UT',
                            'VT',
                            'VI',
                            'VA',
                            'WA',
                            'WV',
                            'WI',
                            'WY'
                        ]
                    };
                },
                computed: {
                    isRequired: function () {
                        var rules = Index_1.ruleStringToArray(this.rules);
                        return rules.indexOf('required') !== -1;
                    },
                    stateOptions: function () {
                        return this.states.map(function (s) { return ({
                            key: s,
                            value: s,
                            text: s
                        }); });
                    }
                },
                template: "\n<div class=\"form-group address-control\" :class=\"isRequired ? 'required' : ''\">\n    <RockLabel v-if=\"label || help\" :for=\"uniqueId\" :help=\"help\">\n        {{label}}\n    </RockLabel>\n    <div class=\"control-wrapper\">\n        <TextBox placeholder=\"Address Line 1\" :rules=\"rules\" v-model=\"modelValue.Street1\" validationTitle=\"Address Line 1\" />\n        <TextBox placeholder=\"Address Line 2\" :rules=\"rules\" v-model=\"modelValue.Street2\" validationTitle=\"Address Line 2\" />\n        <div class=\"form-row\">\n            <TextBox placeholder=\"City\" :rules=\"rules\" v-model=\"modelValue.City\" class=\"col-sm-6\" validationTitle=\"City\" />\n            <DropDownList placeholder=\"State\" v-model=\"modelValue.State\" class=\"col-sm-3\" :options=\"stateOptions\" :showBlankItem=\"true\" :rules=\"rules\" />\n            <TextBox placeholder=\"Zip\" :rules=\"rules\" v-model=\"modelValue.PostalCode\" class=\"col-sm-3\" validationTitle=\"Zip\" />\n        </div>\n    </div>\n</div>"
            }));
        }
    };
});
//# sourceMappingURL=AddressControl.js.map