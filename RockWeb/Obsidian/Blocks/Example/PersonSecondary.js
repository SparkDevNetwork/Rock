System.register(["../../Util/Bus", "../../Templates/PaneledBlockTemplate", "../../Controls/SecondaryBlock", "../../Elements/RockButton", "../../Elements/TextBox", "vue", "../../Store/Index"], function (exports_1, context_1) {
    "use strict";
    var Bus_1, PaneledBlockTemplate_1, SecondaryBlock_1, RockButton_1, TextBox_1, vue_1, Index_1;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (Bus_1_1) {
                Bus_1 = Bus_1_1;
            },
            function (PaneledBlockTemplate_1_1) {
                PaneledBlockTemplate_1 = PaneledBlockTemplate_1_1;
            },
            function (SecondaryBlock_1_1) {
                SecondaryBlock_1 = SecondaryBlock_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            },
            function (TextBox_1_1) {
                TextBox_1 = TextBox_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: 'Example.PersonSecondary',
                components: {
                    PaneledBlockTemplate: PaneledBlockTemplate_1.default,
                    SecondaryBlock: SecondaryBlock_1.default,
                    TextBox: TextBox_1.default,
                    RockButton: RockButton_1.default
                },
                data: function () {
                    return {
                        messageToPublish: '',
                        receivedMessage: ''
                    };
                },
                methods: {
                    receiveMessage: function (message) {
                        this.receivedMessage = message;
                    },
                    doPublish: function () {
                        Bus_1.default.publish('PersonSecondary:Message', this.messageToPublish);
                        this.messageToPublish = '';
                    },
                    doThrowError: function () {
                        throw new Error('This is an uncaught error');
                    }
                },
                computed: {
                    currentPerson: function () {
                        return Index_1.default.state.currentPerson;
                    },
                    currentPersonName: function () {
                        var _a;
                        return ((_a = this.currentPerson) === null || _a === void 0 ? void 0 : _a.fullName) || 'anonymous';
                    },
                    imageUrl: function () {
                        var _a;
                        return ((_a = this.currentPerson) === null || _a === void 0 ? void 0 : _a.photoUrl) || '/Assets/Images/person-no-photo-unknown.svg';
                    },
                    photoElementStyle: function () {
                        return "background-image: url(\"" + this.imageUrl + "\"); background-size: cover; background-repeat: no-repeat;";
                    }
                },
                created: function () {
                    Bus_1.default.subscribe('PersonDetail:Message', this.receiveMessage);
                },
                template: "<SecondaryBlock>\n    <PaneledBlockTemplate>\n        <template v-slot:title>\n            <i class=\"fa fa-flask\"></i>\n            Secondary Block\n        </template>\n        <template v-slot:default>\n            <div class=\"row\">\n                <div class=\"col-sm-6\">\n                    <p>\n                        Hi, {{currentPersonName}}!\n                        <div class=\"photo-icon photo-round photo-round-sm\" :style=\"photoElementStyle\"></div>\n                    </p>\n                    <p>This is a secondary block. It respects the store's value indicating if secondary blocks are visible.</p>\n                    <RockButton btnType=\"danger\" btnSize=\"sm\" @click=\"doThrowError\">Throw Error</RockButton>\n                </div>\n                <div class=\"col-sm-6\">\n                    <div class=\"well\">\n                        <TextBox label=\"Message\" v-model=\"messageToPublish\" />\n                        <RockButton btnType=\"primary\" btnSize=\"sm\" @click=\"doPublish\">Publish</RockButton>\n                    </div>\n                    <p>\n                        <strong>Detail block says:</strong>\n                        {{receivedMessage}}\n                    </p>\n                </div>\n            </div>\n        </template>\n    </PaneledBlockTemplate>\n</SecondaryBlock>"
            }));
        }
    };
});
//# sourceMappingURL=PersonSecondary.js.map