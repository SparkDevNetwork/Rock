System.register(["vue", "../Rules/Index", "../Services/DateKey", "./RockFormField"], function (exports_1, context_1) {
    "use strict";
    var vue_1, Index_1, DateKey_1, RockFormField_1;
    var __moduleName = context_1 && context_1.id;
    function getDefaultBirthdayPickerModel() {
        return {
            Year: 0,
            Month: 0,
            Day: 0
        };
    }
    exports_1("getDefaultBirthdayPickerModel", getDefaultBirthdayPickerModel);
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            },
            function (DateKey_1_1) {
                DateKey_1 = DateKey_1_1;
            },
            function (RockFormField_1_1) {
                RockFormField_1 = RockFormField_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'BirthdayPicker',
                components: {
                    RockFormField: RockFormField_1.default
                },
                props: {
                    rules: {
                        type: String,
                        default: ''
                    },
                    modelValue: {
                        type: Object,
                        required: true
                    }
                },
                data: function () {
                    var years = [];
                    var count = 120;
                    var currentYear = new Date().getFullYear();
                    for (var i = 0; i <= count; i++) {
                        years.push(currentYear - i);
                    }
                    return {
                        years: years
                    };
                },
                computed: {
                    internalDateKey: function () {
                        var dateKey = DateKey_1.default.toDateKey(this.modelValue.Year, this.modelValue.Month, this.modelValue.Day);
                        return dateKey;
                    },
                    computedRules: function () {
                        var rules = Index_1.ruleStringToArray(this.rules);
                        if (rules.indexOf('required') !== -1 && rules.indexOf('datekey') === -1) {
                            rules.push('datekey');
                        }
                        return Index_1.ruleArrayToString(rules);
                    }
                },
                template: "\n<RockFormField\n    :modelValue=\"internalDateKey\"\n    formGroupClasses=\"birthday-picker\"\n    name=\"birthday\"\n    :rules=\"computedRules\">\n    <template #default=\"{uniqueId, field, errors, disabled}\">\n        <div class=\"control-wrapper\">\n            <div class=\"form-control-group\">\n                <select :id=\"uniqueId + '-month'\" class=\"form-control input-width-sm\" :disabled=\"disabled\" v-model=\"modelValue.Month\">\n                    <option value=\"0\"></option>\n                    <option value=\"1\">Jan</option>\n                    <option value=\"2\">Feb</option>\n                    <option value=\"3\">Mar</option>\n                    <option value=\"4\">Apr</option>\n                    <option value=\"5\">May</option>\n                    <option value=\"6\">Jun</option>\n                    <option value=\"7\">Jul</option>\n                    <option value=\"8\">Aug</option>\n                    <option value=\"9\">Sep</option>\n                    <option value=\"10\">Oct</option>\n                    <option value=\"11\">Nov</option>\n                    <option value=\"12\">Dec</option>\n                </select>\n                <span class=\"separator\">/</span>\n                <select :id=\"uniqueId + '-day'\" class=\"form-control input-width-sm\" v-model=\"modelValue.Day\">\n                    <option value=\"0\"></option>\n                    <option value=\"1\">1</option>\n                    <option value=\"2\">2</option>\n                    <option value=\"3\">3</option>\n                    <option value=\"4\">4</option>\n                    <option value=\"5\">5</option>\n                    <option value=\"6\">6</option>\n                    <option value=\"7\">7</option>\n                    <option value=\"8\">8</option>\n                    <option value=\"9\">9</option>\n                    <option value=\"10\">10</option>\n                    <option value=\"11\">11</option>\n                    <option value=\"12\">12</option>\n                    <option value=\"13\">13</option>\n                    <option value=\"14\">14</option>\n                    <option value=\"15\">15</option>\n                    <option value=\"16\">16</option>\n                    <option value=\"17\">17</option>\n                    <option value=\"18\">18</option>\n                    <option value=\"19\">19</option>\n                    <option value=\"20\">20</option>\n                    <option value=\"21\">21</option>\n                    <option value=\"22\">22</option>\n                    <option value=\"23\">23</option>\n                    <option value=\"24\">24</option>\n                    <option value=\"25\">25</option>\n                    <option value=\"26\">26</option>\n                    <option value=\"27\">27</option>\n                    <option value=\"28\">28</option>\n                    <option value=\"29\">29</option>\n                    <option value=\"30\">30</option>\n                    <option value=\"31\">31</option>\n                </select>\n                <span class=\"separator\">/</span>\n                <select :id=\"uniqueId + '-year'\" class=\"form-control input-width-sm\" v-model=\"modelValue.Year\">\n                    <option value=\"0\"></option>\n                    <option v-for=\"year in years\" :value=\"year.toString()\">{{year}}</option>\n                </select>\n            </div>\n        </div>\n    </template>\n</RockFormField>"
            }));
        }
    };
});
//# sourceMappingURL=BirthdayPicker.js.map