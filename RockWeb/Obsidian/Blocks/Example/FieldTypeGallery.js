System.register(["../../Templates/PaneledBlockTemplate", "vue", "../../Elements/PanelWidget", "../../Controls/AttributeValuesContainer", "../../Elements/TextBox"], function (exports_1, context_1) {
    "use strict";
    var __assign = (this && this.__assign) || function () {
        __assign = Object.assign || function(t) {
            for (var s, i = 1, n = arguments.length; i < n; i++) {
                s = arguments[i];
                for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                    t[p] = s[p];
            }
            return t;
        };
        return __assign.apply(this, arguments);
    };
    var PaneledBlockTemplate_1, vue_1, PanelWidget_1, AttributeValuesContainer_1, TextBox_1, GetAttributeValueData, GalleryAndResult, GetFieldTypeGalleryComponent;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (PaneledBlockTemplate_1_1) {
                PaneledBlockTemplate_1 = PaneledBlockTemplate_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (PanelWidget_1_1) {
                PanelWidget_1 = PanelWidget_1_1;
            },
            function (AttributeValuesContainer_1_1) {
                AttributeValuesContainer_1 = AttributeValuesContainer_1_1;
            },
            function (TextBox_1_1) {
                TextBox_1 = TextBox_1_1;
            }
        ],
        execute: function () {
            GetAttributeValueData = function (name, initialValue, fieldTypeGuid, configValues) {
                var configurationValues = {};
                for (var key in configValues) {
                    configurationValues[key] = {
                        Name: '',
                        Description: '',
                        Value: configValues[key]
                    };
                }
                return [
                    {
                        Attribute: {
                            Name: name + " 1",
                            Description: "This is the description of the " + name + " without an initial value",
                            FieldTypeGuid: fieldTypeGuid,
                            QualifierValues: configurationValues
                        },
                        Value: ''
                    },
                    {
                        Attribute: {
                            Name: name + " 2",
                            Description: "This is the description of the " + name + " with an initial value",
                            FieldTypeGuid: fieldTypeGuid,
                            QualifierValues: configurationValues
                        },
                        Value: initialValue
                    }
                ];
            };
            GalleryAndResult = vue_1.defineComponent({
                name: 'GalleryAndResult',
                components: {
                    PanelWidget: PanelWidget_1.default,
                    AttributeValuesContainer: AttributeValuesContainer_1.default
                },
                props: {
                    title: {
                        type: String,
                        required: true
                    },
                    attributeValues: {
                        type: Array,
                        required: true
                    }
                },
                computed: {
                    value1Json: function () {
                        return JSON.stringify(this.attributeValues[0].Value, null, 4);
                    },
                    value2Json: function () {
                        return JSON.stringify(this.attributeValues[1].Value, null, 4);
                    }
                },
                template: "\n<PanelWidget>\n    <template #header>{{title}}</template>\n    <div class=\"row\">\n        <div class=\"col-md-6\">\n            <h4>Qualifier Values</h4>\n            <slot />\n            <hr />\n            <h4>Attribute Values Container (edit)</h4>\n            <AttributeValuesContainer :attributeValues=\"attributeValues\" :isEditMode=\"true\" />\n        </div>\n        <div class=\"col-md-6\">\n            <h4>Attribute Values Container (view)</h4>\n            <AttributeValuesContainer :attributeValues=\"attributeValues\" :isEditMode=\"false\" />\n            <hr />\n            <h4>Values</h4>\n            <p>\n                <strong>Value 1</strong>\n                <pre>{{value1Json}}</pre>\n            </p>\n            <p>\n                <strong>Value 2</strong>\n                <pre>{{value2Json}}</pre>\n            </p>\n        </div>\n    </div>\n</PanelWidget>"
            });
            GetFieldTypeGalleryComponent = function (name, initialValue, fieldTypeGuid, initialConfigValues) {
                return vue_1.defineComponent({
                    name: name + "Gallery",
                    components: {
                        GalleryAndResult: GalleryAndResult,
                        TextBox: TextBox_1.default
                    },
                    data: function () {
                        return {
                            name: name,
                            configValues: __assign({}, initialConfigValues),
                            attributeValues: GetAttributeValueData(name, initialValue, fieldTypeGuid, initialConfigValues)
                        };
                    },
                    computed: {
                        configKeys: function () {
                            var keys = [];
                            for (var _i = 0, _a = this.attributeValues; _i < _a.length; _i++) {
                                var attributeValue = _a[_i];
                                for (var key in attributeValue.Attribute.QualifierValues) {
                                    if (keys.indexOf(key) === -1) {
                                        keys.push(key);
                                    }
                                }
                            }
                            return keys;
                        }
                    },
                    watch: {
                        configValues: {
                            deep: true,
                            handler: function () {
                                for (var _i = 0, _a = this.attributeValues; _i < _a.length; _i++) {
                                    var attributeValue = _a[_i];
                                    for (var key in attributeValue.Attribute.QualifierValues) {
                                        var value = this.configValues[key] || '';
                                        attributeValue.Attribute.QualifierValues[key].Value = value;
                                    }
                                }
                            }
                        }
                    },
                    template: "\n<GalleryAndResult :title=\"name\" :attributeValues=\"attributeValues\">\n    <TextBox v-for=\"configKey in configKeys\" :key=\"configKey\" :label=\"configKey\" v-model=\"configValues[configKey]\" />\n</GalleryAndResult>"
                });
            };
            exports_1("default", vue_1.defineComponent({
                name: 'Example.FieldTypeGallery',
                components: {
                    PaneledBlockTemplate: PaneledBlockTemplate_1.default,
                    TextGallery: GetFieldTypeGalleryComponent('Text', 'Hello', '9C204CD0-1233-41C5-818A-C5DA439445AA', {
                        ispassword: 'false',
                        maxcharacters: '10',
                        showcountdown: 'true'
                    }),
                    DateGallery: GetFieldTypeGalleryComponent('Date', '2009-02-11', '6B6AA175-4758-453F-8D83-FCD8044B5F36', {
                        format: 'MMM yyyy',
                        displayDiff: 'true',
                        displayCurrentOption: 'true',
                        datePickerControlType: 'Date Parts Picker',
                        futureYearCount: '2'
                    }),
                    SingleSelectGallery: GetFieldTypeGalleryComponent('SingleSelect', 'pizza', '7525C4CB-EE6B-41D4-9B64-A08048D5A5C0', {
                        repeatColumns: '4',
                        fieldtype: 'rb',
                        values: 'pizza^Pizza,sub^Sub'
                    }),
                    MemoGallery: GetFieldTypeGalleryComponent('Memo', 'This is a memo', 'C28C7BF3-A552-4D77-9408-DEDCF760CED0', {
                        numberofrows: '10',
                        maxcharacters: '100',
                        showcountdown: 'true',
                        allowhtml: 'true'
                    }),
                    BooleanGallery: GetFieldTypeGalleryComponent('Boolean', 't', '1EDAFDED-DFE6-4334-B019-6EECBA89E05A', {
                        truetext: 'This is true',
                        falsetext: 'This is false',
                        BooleanControlType: '2'
                    }),
                    PhoneNumberGallery: GetFieldTypeGalleryComponent('PhoneNumber', '(321) 456-7890', '6B1908EC-12A2-463A-A7BD-970CE0FAF097', {}),
                    GenderGallery: GetFieldTypeGalleryComponent('Gender', '2', '2E28779B-4C76-4142-AE8D-49EA31DDB503', {}),
                    ColorGallery: GetFieldTypeGalleryComponent('Color', '#ee7725', 'D747E6AE-C383-4E22-8846-71518E3DD06F', {
                        selectiontype: 'Color Picker'
                    }),
                    DefinedValueGallery: GetFieldTypeGalleryComponent('DefinedValue (WIP)', 'af28af43-8461-41ac-a2c5-85122712be96', '59D5A94C-94A0-4630-B80A-BB25697D74C7', {
                        definedtype: '92',
                        allowmultiple: '',
                        displaydescription: 'true',
                        enhancedselection: '',
                        includeInactive: '',
                        AllowAddingNewValues: '',
                        RepeatColumns: ''
                    })
                },
                template: "\n<PaneledBlockTemplate>\n    <template v-slot:title>\n        <i class=\"fa fa-flask\"></i>\n        Obsidian Field Type Gallery\n    </template>\n    <template v-slot:default>\n        <TextGallery />\n        <DateGallery />\n        <SingleSelectGallery />\n        <MemoGallery />\n        <BooleanGallery />\n        <PhoneNumberGallery />\n        <GenderGallery />\n        <ColorGallery />\n        <DefinedValueGallery />\n    </template>\n</PaneledBlockTemplate>"
            }));
        }
    };
});
//# sourceMappingURL=FieldTypeGallery.js.map