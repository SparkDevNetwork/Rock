System.register(["vue", "../../Controls/RockBlock", "../../Elements/Alert", "../../Elements/RockButton", "../../Templates/PaneledBlockTemplate"], function (exports_1, context_1) {
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
    var __generator = (this && this.__generator) || function (thisArg, body) {
        var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
        return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
        function verb(n) { return function (v) { return step([n, v]); }; }
        function step(op) {
            if (f) throw new TypeError("Generator is already executing.");
            while (_) try {
                if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
                if (y = 0, t) op = [op[0] & 2, t.value];
                switch (op[0]) {
                    case 0: case 1: t = op; break;
                    case 4: _.label++; return { value: op[1], done: false };
                    case 5: _.label++; y = op[1]; op = [0]; continue;
                    case 7: op = _.ops.pop(); _.trys.pop(); continue;
                    default:
                        if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                        if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                        if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                        if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                        if (t[2]) _.ops.pop();
                        _.trys.pop(); continue;
                }
                op = body.call(thisArg, _);
            } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
            if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
        }
    };
    var vue_1, RockBlock_1, Alert_1, RockButton_1, PaneledBlockTemplate_1, StarkDetailOptions;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (RockBlock_1_1) {
                RockBlock_1 = RockBlock_1_1;
            },
            function (Alert_1_1) {
                Alert_1 = Alert_1_1;
            },
            function (RockButton_1_1) {
                RockButton_1 = RockButton_1_1;
            },
            function (PaneledBlockTemplate_1_1) {
                PaneledBlockTemplate_1 = PaneledBlockTemplate_1_1;
            }
        ],
        execute: function () {
            StarkDetailOptions = vue_1.defineComponent({
                name: 'Utility.StarkDetailOptions',
                setup: RockBlock_1.standardBlockSetup,
                components: {
                    PaneledBlockTemplate: PaneledBlockTemplate_1.default,
                    Alert: Alert_1.default,
                    RockButton: RockButton_1.default
                },
                data: function () {
                    return {
                        configMessage: '',
                        blockActionMessage: ''
                    };
                },
                methods: {
                    loadBlockActionMessage: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            var response;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4, this.invokeBlockAction('GetMessage', {
                                            paramFromClient: 'This is a value sent to the server from the client.'
                                        })];
                                    case 1:
                                        response = _a.sent();
                                        if (response.data) {
                                            this.blockActionMessage = response.data.Message;
                                        }
                                        else {
                                            this.blockActionMessage = response.errorMessage || 'An error occurred';
                                        }
                                        return [2];
                                }
                            });
                        });
                    }
                },
                created: function () {
                    this.configMessage = this.configurationValues.Message;
                },
                mounted: function () {
                },
                template: "\n<PaneledBlockTemplate>\n    <template #title>\n        <i class=\"fa fa-star\"></i>\n        Blank Detail Block\n    </template>\n    <template #titleAside>\n        <div class=\"panel-labels\">\n            <span class=\"label label-info\">Vue</span>\n        </div>\n    </template>\n    <template #drawer>\n        An example block that uses Vue\n    </template>\n    <template #default>\n        <Alert alertType=\"info\">\n            <h4>Stark Template Block</h4>\n            <p>This block serves as a starting point for creating new blocks. After copy/pasting it and renaming the resulting file be sure to make the following changes:</p>\n\n            <strong>Changes to the Codebehind (.cs) File</strong>\n            <ul>\n                <li>Update the namespace to match your directory</li>\n                <li>Update the class name</li>\n                <li>Fill in the DisplayName, Category and Description attributes</li>\n            </ul>\n\n            <strong>Changes to the Vue component (.ts/.js) File</strong>\n            <ul>\n                <li>Remove this text... unless you really like it...</li>\n            </ul>\n        </Alert>\n        <div>\n            <h4>Value from Configuration</h4>\n            <p>\n                This value came from the C# file and was provided to the JavaScript before the Vue component was even mounted:\n            </p>\n            <pre>{{ configMessage }}</pre>\n            <h4>Value from Block Action</h4>\n            <p>\n                This value will come from the C# file using a \"Block Action\". Block Actions allow the Vue Component to communicate with the\n                C# code behind (much like a Web Forms Postback):\n            </p>\n            <RockButton btnType=\"primary\" btnSize=\"sm\" @click=\"loadBlockActionMessage\">Invoke Block Action</RockButton>\n            <pre>{{ blockActionMessage }}</pre>\n        </div>\n    </template>\n</PaneledBlockTemplate>"
            });
            exports_1("default", StarkDetailOptions);
        }
    };
});
//# sourceMappingURL=StarkDetail.js.map