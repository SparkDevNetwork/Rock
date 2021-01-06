System.register(["../../Util/Bus.js", "../../Templates/PaneledBlockTemplate.js", "../../Elements/RockButton.js", "../../Elements/TextBox.js", "../../Vendor/Vue/vue.js", "../../Store/Index.js", "../../Elements/EmailInput.js", "../../Controls/RockValidation.js", "../../Controls/RockForm.js", "../../Controls/CampusPicker.js"], function (exports_1, context_1) {
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
    var Bus_js_1, PaneledBlockTemplate_js_1, RockButton_js_1, TextBox_js_1, vue_js_1, Index_js_1, EmailInput_js_1, RockValidation_js_1, RockForm_js_1, CampusPicker_js_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (Bus_js_1_1) {
                Bus_js_1 = Bus_js_1_1;
            },
            function (PaneledBlockTemplate_js_1_1) {
                PaneledBlockTemplate_js_1 = PaneledBlockTemplate_js_1_1;
            },
            function (RockButton_js_1_1) {
                RockButton_js_1 = RockButton_js_1_1;
            },
            function (TextBox_js_1_1) {
                TextBox_js_1 = TextBox_js_1_1;
            },
            function (vue_js_1_1) {
                vue_js_1 = vue_js_1_1;
            },
            function (Index_js_1_1) {
                Index_js_1 = Index_js_1_1;
            },
            function (EmailInput_js_1_1) {
                EmailInput_js_1 = EmailInput_js_1_1;
            },
            function (RockValidation_js_1_1) {
                RockValidation_js_1 = RockValidation_js_1_1;
            },
            function (RockForm_js_1_1) {
                RockForm_js_1 = RockForm_js_1_1;
            },
            function (CampusPicker_js_1_1) {
                CampusPicker_js_1 = CampusPicker_js_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_js_1.defineComponent({
                name: 'Test.PersonDetail',
                components: {
                    PaneledBlockTemplate: PaneledBlockTemplate_js_1.default,
                    RockButton: RockButton_js_1.default,
                    TextBox: TextBox_js_1.default,
                    EmailInput: EmailInput_js_1.default,
                    RockValidation: RockValidation_js_1.default,
                    RockForm: RockForm_js_1.default,
                    CampusPicker: CampusPicker_js_1.default
                },
                data: function () {
                    var person = {
                        FirstName: 'John',
                        LastName: 'Smith',
                        PhotoUrl: '',
                        FullName: 'John Smith',
                        Email: 'john@smith.com',
                        Guid: '',
                        Id: 0
                    };
                    return {
                        person: person,
                        personForEditing: __assign({}, person),
                        isEditMode: false,
                        messageToPublish: '',
                        receivedMessage: '',
                        campusId: ''
                    };
                },
                methods: {
                    setAreSecondaryBlocksShown: function (isVisible) {
                        Index_js_1.default.commit('setAreSecondaryBlocksShown', { areSecondaryBlocksShown: isVisible });
                    },
                    setIsEditMode: function (isEditMode) {
                        this.isEditMode = isEditMode;
                        this.setAreSecondaryBlocksShown(!isEditMode);
                    },
                    doEdit: function () {
                        this.personForEditing = __assign({}, this.person);
                        this.setIsEditMode(true);
                    },
                    doDelete: function () {
                        console.log('delete here');
                    },
                    doCancel: function () {
                        this.personForEditing = __assign({}, this.person);
                        this.setIsEditMode(false);
                    },
                    doSave: function () {
                        this.person = __assign({}, this.personForEditing);
                        this.setIsEditMode(false);
                    },
                    doPublish: function () {
                        Bus_js_1.default.publish('PersonDetail:Message', this.messageToPublish);
                        this.messageToPublish = '';
                    },
                    receiveMessage: function (message) {
                        this.receivedMessage = message;
                    }
                },
                computed: {
                    blockTitle: function () {
                        return this.person.FirstName + " " + this.person.LastName;
                    }
                },
                created: function () {
                    Bus_js_1.default.subscribe('PersonSecondary:Message', this.receiveMessage);
                },
                template: "<PaneledBlockTemplate>\n    <template v-slot:title>\n        <i class=\"fa fa-flask\"></i>\n        Detail Block: {{blockTitle}}\n    </template>\n    <template v-slot:default>\n        <RockForm v-if=\"isEditMode\" @submit=\"doSave\">\n            <div class=\"row\">\n                <div class=\"col-sm-6\">\n                    <TextBox label=\"First Name\" v-model=\"personForEditing.FirstName\" rules=\"required\" disabled />\n                    <TextBox label=\"Last Name\" v-model=\"personForEditing.LastName\" />\n                </div>\n                <div class=\"col-sm-6\">\n                    <EmailInput v-model=\"personForEditing.Email\" rules=\"required\" />\n                    <CampusPicker v-model=\"campusId\" rules=\"required\" />\n                </div>\n            </div>\n            <div class=\"actions\">\n                <RockButton class=\"btn-primary\" type=\"submit\">Save</RockButton>\n                <RockButton class=\"btn-link\" @click=\"doCancel\">Cancel</RockButton>\n            </div>\n        </RockForm>\n        <template v-else>\n            <div class=\"row\">\n                <div class=\"col-sm-6\">\n                    <dl>\n                        <dt>First Name</dt>\n                        <dd>{{person.FirstName}}</dd>\n                        <dt>Last Name</dt>\n                        <dd>{{person.LastName}}</dd>\n                        <dt>Email</dt>\n                        <dd>{{person.Email}}</dd>\n                    </dl>\n                </div>\n                <div class=\"col-sm-6\">\n                    <div class=\"well\">\n                        <TextBox label=\"Message\" v-model=\"messageToPublish\" />\n                        <RockButton class=\"btn-primary btn-sm\" @click=\"doPublish\">Publish</RockButton>\n                    </div>\n                    <p>\n                        <strong>Secondary block says:</strong>\n                        {{receivedMessage}}\n                    </p>\n                </div>\n            </div>\n            <div class=\"actions\">\n                <RockButton class=\"btn-primary\" @click=\"doEdit\">Edit</RockButton>\n                <RockButton class=\"btn-link\" @click=\"doDelete\">Delete</RockButton>\n            </div>\n        </template>\n    </template>\n</PaneledBlockTemplate>"
            }));
        }
    };
});
//# sourceMappingURL=PersonDetail.js.map