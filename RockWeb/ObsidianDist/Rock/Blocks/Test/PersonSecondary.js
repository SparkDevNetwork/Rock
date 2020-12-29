define(["require", "exports", "../../Util/Bus.js", "../../Templates/PaneledBlockTemplate.js", "../../Controls/SecondaryBlock.js", "../../Elements/RockButton.js", "../../Elements/TextBox.js", "../../Vendor/Vue/vue.js", "../../Store/Index.js"], function (require, exports, Bus_js_1, PaneledBlockTemplate_js_1, SecondaryBlock_js_1, RockButton_js_1, TextBox_js_1, vue_js_1, Index_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.default = vue_js_1.defineComponent({
        name: 'Test.PersonSecondary',
        components: {
            PaneledBlockTemplate: PaneledBlockTemplate_js_1.default,
            SecondaryBlock: SecondaryBlock_js_1.default,
            TextBox: TextBox_js_1.default,
            RockButton: RockButton_js_1.default
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
                Bus_js_1.default.publish('PersonSecondary:Message', this.messageToPublish);
                this.messageToPublish = '';
            }
        },
        computed: {
            currentPerson: function () {
                return Index_js_1.default.state.currentPerson;
            },
            currentPersonName: function () {
                return this.currentPerson ? this.currentPerson.FullName : 'anonymous';
            },
            imageUrl: function () {
                if (this.currentPerson && this.currentPerson.PhotoUrl) {
                    return this.currentPerson.PhotoUrl;
                }
                return '/Assets/Images/person-no-photo-unknown.svg';
            },
            photoElementStyle: function () {
                return "background-image: url(\"" + this.imageUrl + "\"); background-size: cover; background-repeat: no-repeat;";
            }
        },
        created: function () {
            Bus_js_1.default.subscribe('PersonDetail:Message', this.receiveMessage);
        },
        template: "<SecondaryBlock>\n    <PaneledBlockTemplate>\n        <template v-slot:title>\n            <i class=\"fa fa-flask\"></i>\n            Secondary Block\n        </template>\n        <template v-slot:default>\n            <div class=\"row\">\n                <div class=\"col-sm-6\">\n                    <p>\n                        Hi, {{currentPersonName}}!\n                        <div class=\"photo-icon photo-round photo-round-sm\" :style=\"photoElementStyle\"></div>\n                    </p>\n                    <p>This is a secondary block. It respects the store's value indicating if secondary blocks are visible.</p>\n                </div>\n                <div class=\"col-sm-6\">\n                    <div class=\"well\">\n                        <TextBox label=\"Message\" v-model=\"messageToPublish\" />\n                        <RockButton class=\"btn-primary btn-sm\" @click=\"doPublish\">Publish</RockButton>\n                    </div>\n                    <p>\n                        <strong>Detail block says:</strong>\n                        {{receivedMessage}}\n                    </p>\n                </div>\n            </div>\n        </template>\n    </PaneledBlockTemplate>\n</SecondaryBlock>"
    });
});
//# sourceMappingURL=PersonSecondary.js.map