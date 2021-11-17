System.register(["../../Util/block", "vue", "../../Elements/alert", "../../Elements/rockButton", "../../Templates/paneledBlockTemplate"], function (exports_1, context_1) {
    "use strict";
    var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
        function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
        return new (P || (P = Promise))(function (resolve, reject) {
            function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
            function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
            function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
            step((generator = generator.apply(thisArg, _arguments || [])).next());
        });
    };
    var block_1, vue_1, alert_1, rockButton_1, paneledBlockTemplate_1, StarkDetailOptions;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (block_1_1) {
                block_1 = block_1_1;
            },
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (alert_1_1) {
                alert_1 = alert_1_1;
            },
            function (rockButton_1_1) {
                rockButton_1 = rockButton_1_1;
            },
            function (paneledBlockTemplate_1_1) {
                paneledBlockTemplate_1 = paneledBlockTemplate_1_1;
            }
        ],
        execute: function () {
            StarkDetailOptions = vue_1.defineComponent({
                name: "Utility.StarkDetailOptions",
                setup() {
                    const invokeBlockAction = block_1.useInvokeBlockAction();
                    const configurationValues = block_1.useConfigurationValues();
                    return {
                        configurationValues,
                        invokeBlockAction
                    };
                },
                components: {
                    PaneledBlockTemplate: paneledBlockTemplate_1.default,
                    Alert: alert_1.default,
                    RockButton: rockButton_1.default
                },
                data() {
                    return {
                        configMessage: "",
                        blockActionMessage: ""
                    };
                },
                methods: {
                    loadBlockActionMessage() {
                        return __awaiter(this, void 0, void 0, function* () {
                            const response = yield this.invokeBlockAction("GetMessage", {
                                paramFromClient: "This is a value sent to the server from the client."
                            });
                            if (response.data) {
                                this.blockActionMessage = response.data.message;
                            }
                            else {
                                this.blockActionMessage = response.errorMessage || "An error occurred";
                            }
                        });
                    }
                },
                created() {
                    this.configMessage = this.configurationValues.message;
                },
                mounted() {
                },
                template: `
<PaneledBlockTemplate>
    <template #title>
        <i class="fa fa-star"></i>
        Blank Detail Block
    </template>
    <template #titleAside>
        <div class="panel-labels">
            <span class="label label-info">Vue</span>
        </div>
    </template>
    <template #drawer>
        An example block that uses Vue
    </template>
    <template #default>
        <Alert alertType="info">
            <h4>Stark Template Block</h4>
            <p>This block serves as a starting point for creating new blocks. After copy/pasting it and renaming the resulting file be sure to make the following changes:</p>

            <strong>Changes to the Codebehind (.cs) File</strong>
            <ul>
                <li>Update the namespace to match your directory</li>
                <li>Update the class name</li>
                <li>Fill in the DisplayName, Category and Description attributes</li>
            </ul>

            <strong>Changes to the Vue component (.ts/.js) File</strong>
            <ul>
                <li>Remove this text... unless you really like it...</li>
            </ul>
        </Alert>
        <div>
            <h4>Value from Configuration</h4>
            <p>
                This value came from the C# file and was provided to the JavaScript before the Vue component was even mounted:
            </p>
            <pre>{{ configMessage }}</pre>
            <h4>Value from Block Action</h4>
            <p>
                This value will come from the C# file using a "Block Action". Block Actions allow the Vue Component to communicate with the
                C# code behind (much like a Web Forms Postback):
            </p>
            <RockButton btnType="primary" btnSize="sm" @click="loadBlockActionMessage">Invoke Block Action</RockButton>
            <pre>{{ blockActionMessage }}</pre>
        </div>
    </template>
</PaneledBlockTemplate>`
            });
            exports_1("default", StarkDetailOptions);
        }
    };
});
//# sourceMappingURL=starkDetail.js.map