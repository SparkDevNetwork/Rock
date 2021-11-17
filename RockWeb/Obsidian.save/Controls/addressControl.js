System.register(["vue", "../Elements/rockFormField", "../Elements/dropDownList", "../Elements/rockLabel", "../Elements/textBox", "../Util/guid"], function (exports_1, context_1) {
    "use strict";
    var vue_1, rockFormField_1, dropDownList_1, rockLabel_1, textBox_1, guid_1, stateOptions, AddressControlBase;
    var __moduleName = context_1 && context_1.id;
    function getDefaultAddressControlModel() {
        return {
            state: "AZ",
            country: "US"
        };
    }
    exports_1("getDefaultAddressControlModel", getDefaultAddressControlModel);
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (rockFormField_1_1) {
                rockFormField_1 = rockFormField_1_1;
            },
            function (dropDownList_1_1) {
                dropDownList_1 = dropDownList_1_1;
            },
            function (rockLabel_1_1) {
                rockLabel_1 = rockLabel_1_1;
            },
            function (textBox_1_1) {
                textBox_1 = textBox_1_1;
            },
            function (guid_1_1) {
                guid_1 = guid_1_1;
            }
        ],
        execute: function () {
            stateOptions = [
                "AL", "AK", "AS", "AZ", "AR", "CA", "CO", "CT", "DE", "DC", "FM",
                "FL", "GA", "GU", "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA",
                "ME", "MH", "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV",
                "NH", "NJ", "NM", "NY", "NC", "ND", "MP", "OH", "OK", "OR", "PW",
                "PA", "PR", "RI", "SC", "SD", "TN", "TX", "UT", "VT", "VI", "VA",
                "WA", "WV", "WI", "WY"
            ]
                .map(o => ({ value: o, text: o }));
            exports_1("AddressControlBase", AddressControlBase = vue_1.defineComponent({
                name: "AddressControlBase",
                components: {
                    TextBox: textBox_1.default,
                    RockLabel: rockLabel_1.default,
                    DropDownList: dropDownList_1.default
                },
                props: {
                    modelValue: {
                        type: Object,
                        default: {}
                    },
                    id: {
                        type: String,
                        default: ""
                    }
                },
                setup(props) {
                    const uniqueId = props.id || `rock-addresscontrol-${guid_1.newGuid}`;
                    return {
                        uniqueId,
                        stateOptions
                    };
                },
                template: `
<div :id="uniqueId">
    <TextBox placeholder="Address Line 1" :rules="rules" v-model="modelValue.street1" validationTitle="Address Line 1" />
    <TextBox placeholder="Address Line 2" v-model="modelValue.street2" validationTitle="Address Line 2" />
    <div class="form-row">
        <TextBox placeholder="City" :rules="rules" v-model="modelValue.city" class="col-sm-6" validationTitle="City" />
        <DropDownList :showBlankItem="false" v-model="modelValue.state" class="col-sm-3" :options="stateOptions" />
        <TextBox placeholder="Zip" :rules="rules" v-model="modelValue.postalCode" class="col-sm-3" validationTitle="Zip" />
    </div>
</div>
`
            }));
            exports_1("default", vue_1.defineComponent({
                name: "AddressControl",
                components: {
                    RockFormField: rockFormField_1.default,
                    AddressControlBase
                },
                props: {
                    modelValue: {
                        type: Object,
                        default: {}
                    }
                },
                template: `
<RockFormField formGroupClasses="address-control" #default="{uniqueId, field, disabled}" name="addresscontrol" v-model.lazy="modelValue">
    <div class="control-wrapper">
        <AddressControlBase v-model.lazy="modelValue" v-bind="field" :disabled="disabled" />
    </div>
</RockFormField>
`
            }));
        }
    };
});
//# sourceMappingURL=addressControl.js.map