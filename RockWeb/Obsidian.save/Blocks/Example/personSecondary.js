System.register(["../../Util/bus", "../../Templates/paneledBlockTemplate", "../../Controls/secondaryBlock", "../../Elements/rockButton", "../../Elements/textBox", "vue", "../../Store/index"], function (exports_1, context_1) {
    "use strict";
    var bus_1, paneledBlockTemplate_1, secondaryBlock_1, rockButton_1, textBox_1, vue_1, index_1, store;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (bus_1_1) {
                bus_1 = bus_1_1;
            },
            function (paneledBlockTemplate_1_1) {
                paneledBlockTemplate_1 = paneledBlockTemplate_1_1;
            },
            function (secondaryBlock_1_1) {
                secondaryBlock_1 = secondaryBlock_1_1;
            },
            function (rockButton_1_1) {
                rockButton_1 = rockButton_1_1;
            },
            function (textBox_1_1) {
                textBox_1 = textBox_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (index_1_1) {
                index_1 = index_1_1;
            }
        ],
        execute: function () {
            store = index_1.useStore();
            exports_1("default", vue_1.defineComponent({
                name: "Example.PersonSecondary",
                components: {
                    PaneledBlockTemplate: paneledBlockTemplate_1.default,
                    SecondaryBlock: secondaryBlock_1.default,
                    TextBox: textBox_1.default,
                    RockButton: rockButton_1.default
                },
                data() {
                    return {
                        messageToPublish: "",
                        receivedMessage: ""
                    };
                },
                methods: {
                    receiveMessage(message) {
                        this.receivedMessage = message;
                    },
                    doPublish() {
                        bus_1.default.publish("PersonSecondary:Message", this.messageToPublish);
                        this.messageToPublish = "";
                    },
                    doThrowError() {
                        throw new Error("This is an uncaught error");
                    }
                },
                computed: {
                    currentPerson() {
                        return store.state.currentPerson;
                    },
                    currentPersonName() {
                        var _a;
                        return ((_a = this.currentPerson) === null || _a === void 0 ? void 0 : _a.fullName) || "anonymous";
                    },
                    imageUrl() {
                        var _a;
                        return ((_a = this.currentPerson) === null || _a === void 0 ? void 0 : _a.photoUrl) || "/Assets/Images/person-no-photo-unknown.svg";
                    },
                    photoElementStyle() {
                        return `background-image: url("${this.imageUrl}"); background-size: cover; background-repeat: no-repeat;`;
                    }
                },
                created() {
                    bus_1.default.subscribe("PersonDetail:Message", this.receiveMessage);
                },
                template: `<SecondaryBlock>
    <PaneledBlockTemplate>
        <template v-slot:title>
            <i class="fa fa-flask"></i>
            Secondary Block
        </template>
        <template v-slot:default>
            <div class="row">
                <div class="col-sm-6">
                    <p>
                        Hi, {{currentPersonName}}!
                        <div class="photo-icon photo-round photo-round-sm" :style="photoElementStyle"></div>
                    </p>
                    <p>This is a secondary block. It respects the store's value indicating if secondary blocks are visible.</p>
                    <RockButton btnType="danger" btnSize="sm" @click="doThrowError">Throw Error</RockButton>
                </div>
                <div class="col-sm-6">
                    <div class="well">
                        <TextBox label="Message" v-model="messageToPublish" />
                        <RockButton btnType="primary" btnSize="sm" @click="doPublish">Publish</RockButton>
                    </div>
                    <p>
                        <strong>Detail block says:</strong>
                        {{receivedMessage}}
                    </p>
                </div>
            </div>
        </template>
    </PaneledBlockTemplate>
</SecondaryBlock>`
            }));
        }
    };
});
//# sourceMappingURL=personSecondary.js.map