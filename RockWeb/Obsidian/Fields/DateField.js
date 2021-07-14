System.register(["vue", "./Index", "../Elements/DatePicker", "../Services/Date", "../Services/Boolean", "../Services/Number", "../Elements/DatePartsPicker"], function (exports_1, context_1) {
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
    var vue_1, Index_1, DatePicker_1, Date_1, Boolean_1, Number_1, DatePartsPicker_1, fieldTypeGuid, ConfigurationValueKey;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (Index_1_1) {
                Index_1 = Index_1_1;
            },
            function (DatePicker_1_1) {
                DatePicker_1 = DatePicker_1_1;
            },
            function (Date_1_1) {
                Date_1 = Date_1_1;
            },
            function (Boolean_1_1) {
                Boolean_1 = Boolean_1_1;
            },
            function (Number_1_1) {
                Number_1 = Number_1_1;
            },
            function (DatePartsPicker_1_1) {
                DatePartsPicker_1 = DatePartsPicker_1_1;
            }
        ],
        execute: function () {
            fieldTypeGuid = '6B6AA175-4758-453F-8D83-FCD8044B5F36';
            (function (ConfigurationValueKey) {
                ConfigurationValueKey["Format"] = "format";
                ConfigurationValueKey["DisplayDiff"] = "displayDiff";
                ConfigurationValueKey["DisplayCurrentOption"] = "displayCurrentOption";
                ConfigurationValueKey["DatePickerControlType"] = "datePickerControlType";
                ConfigurationValueKey["FutureYearCount"] = "futureYearCount";
            })(ConfigurationValueKey || (ConfigurationValueKey = {}));
            exports_1("default", Index_1.registerFieldType(fieldTypeGuid, vue_1.defineComponent({
                name: 'DateField',
                components: {
                    DatePicker: DatePicker_1.default,
                    DatePartsPicker: DatePartsPicker_1.default
                },
                props: Index_1.getFieldTypeProps(),
                data: function () {
                    return {
                        internalValue: '',
                        internalDateParts: DatePartsPicker_1.getDefaultDatePartsPickerModel(),
                        formattedString: ''
                    };
                },
                setup: function () {
                    return {
                        http: vue_1.inject('http')
                    };
                },
                computed: {
                    datePartsAsDate: function () {
                        var _a;
                        if (!((_a = this.internalDateParts) === null || _a === void 0 ? void 0 : _a.Day) || !this.internalDateParts.Month || !this.internalDateParts.Year) {
                            return null;
                        }
                        return new Date(this.internalDateParts.Year, this.internalDateParts.Month - 1, this.internalDateParts.Day) || null;
                    },
                    isDatePartsPicker: function () {
                        var _a;
                        var config = this.configurationValues[ConfigurationValueKey.DatePickerControlType];
                        return ((_a = config === null || config === void 0 ? void 0 : config.Value) === null || _a === void 0 ? void 0 : _a.toLowerCase()) === 'date parts picker';
                    },
                    isCurrentDateValue: function () {
                        return this.internalValue.indexOf('CURRENT') === 0;
                    },
                    asDate: function () {
                        return Date_1.asDateOrNull(this.internalValue);
                    },
                    dateFormatTemplate: function () {
                        var formatConfig = this.configurationValues[ConfigurationValueKey.Format];
                        return (formatConfig === null || formatConfig === void 0 ? void 0 : formatConfig.Value) || 'MM/dd/yyyy';
                    },
                    elapsedString: function () {
                        var dateValue = this.isDatePartsPicker ? this.datePartsAsDate : this.asDate;
                        if (this.isCurrentDateValue || !dateValue) {
                            return '';
                        }
                        var formatConfig = this.configurationValues[ConfigurationValueKey.DisplayDiff];
                        var displayDiff = Boolean_1.asBoolean(formatConfig === null || formatConfig === void 0 ? void 0 : formatConfig.Value);
                        if (!displayDiff) {
                            return '';
                        }
                        return Date_1.asElapsedString(dateValue);
                    },
                    configAttributes: function () {
                        var attributes = {};
                        var displayCurrentConfig = this.configurationValues[ConfigurationValueKey.DisplayCurrentOption];
                        if (displayCurrentConfig === null || displayCurrentConfig === void 0 ? void 0 : displayCurrentConfig.Value) {
                            var displayCurrent = Boolean_1.asBoolean(displayCurrentConfig.Value);
                            attributes.displayCurrentOption = displayCurrent;
                            attributes.isCurrentDateOffset = displayCurrent;
                        }
                        var futureYearConfig = this.configurationValues[ConfigurationValueKey.FutureYearCount];
                        if (futureYearConfig === null || futureYearConfig === void 0 ? void 0 : futureYearConfig.Value) {
                            var futureYears = Number_1.toNumber(futureYearConfig.Value);
                            if (futureYears > 0) {
                                attributes.futureYearCount = futureYears;
                            }
                        }
                        return attributes;
                    }
                },
                methods: {
                    syncModelValue: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            var asDate;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        this.internalValue = this.modelValue || '';
                                        asDate = Date_1.asDateOrNull(this.modelValue);
                                        if (asDate) {
                                            this.internalDateParts.Year = asDate.getFullYear();
                                            this.internalDateParts.Month = asDate.getMonth() + 1;
                                            this.internalDateParts.Day = asDate.getDate();
                                        }
                                        else {
                                            this.internalDateParts.Year = 0;
                                            this.internalDateParts.Month = 0;
                                            this.internalDateParts.Day = 0;
                                        }
                                        return [4, this.fetchAndSetFormattedValue()];
                                    case 1:
                                        _a.sent();
                                        return [2];
                                }
                            });
                        });
                    },
                    fetchAndSetFormattedValue: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            var parts, diff, _a, _b;
                            return __generator(this, function (_c) {
                                switch (_c.label) {
                                    case 0:
                                        if (!this.isCurrentDateValue) return [3, 1];
                                        parts = this.internalValue.split(':');
                                        diff = parts.length === 2 ? Number_1.toNumber(parts[1]) : 0;
                                        if (diff === 1) {
                                            this.formattedString = 'Current Date plus 1 day';
                                        }
                                        else if (diff > 0) {
                                            this.formattedString = "Current Date plus " + diff + " days";
                                        }
                                        else if (diff === -1) {
                                            this.formattedString = 'Current Date minus 1 day';
                                        }
                                        else if (diff < 0) {
                                            this.formattedString = "Current Date minus " + Math.abs(diff) + " days";
                                        }
                                        else {
                                            this.formattedString = 'Current Date';
                                        }
                                        return [3, 6];
                                    case 1:
                                        if (!(this.isDatePartsPicker && this.datePartsAsDate)) return [3, 3];
                                        _a = this;
                                        return [4, this.getFormattedDateString(this.datePartsAsDate, this.dateFormatTemplate)];
                                    case 2:
                                        _a.formattedString = _c.sent();
                                        return [3, 6];
                                    case 3:
                                        if (!(!this.isDatePartsPicker && this.asDate)) return [3, 5];
                                        _b = this;
                                        return [4, this.getFormattedDateString(this.asDate, this.dateFormatTemplate)];
                                    case 4:
                                        _b.formattedString = _c.sent();
                                        return [3, 6];
                                    case 5:
                                        this.formattedString = '';
                                        _c.label = 6;
                                    case 6: return [2];
                                }
                            });
                        });
                    },
                    getFormattedDateString: function (value, format) {
                        return __awaiter(this, void 0, void 0, function () {
                            var get, result;
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0:
                                        get = this.http.get;
                                        return [4, get('api/Utility/FormatDate', { value: value, format: format })];
                                    case 1:
                                        result = _a.sent();
                                        return [2, result.data || "" + value];
                                }
                            });
                        });
                    }
                },
                watch: {
                    datePartsAsDate: function () {
                        if (this.isDatePartsPicker) {
                            this.$emit('update:modelValue', Date_1.toRockDateOrNull(this.datePartsAsDate) || '');
                        }
                    },
                    internalValue: function () {
                        if (!this.isDatePartsPicker) {
                            this.$emit('update:modelValue', this.internalValue || '');
                        }
                    },
                    modelValue: {
                        immediate: true,
                        handler: function () {
                            return __awaiter(this, void 0, void 0, function () {
                                return __generator(this, function (_a) {
                                    switch (_a.label) {
                                        case 0: return [4, this.syncModelValue()];
                                        case 1:
                                            _a.sent();
                                            return [2];
                                    }
                                });
                            });
                        }
                    },
                    dateFormatTemplate: function () {
                        return __awaiter(this, void 0, void 0, function () {
                            return __generator(this, function (_a) {
                                switch (_a.label) {
                                    case 0: return [4, this.fetchAndSetFormattedValue()];
                                    case 1:
                                        _a.sent();
                                        return [2];
                                }
                            });
                        });
                    }
                },
                template: "\n<DatePartsPicker v-if=\"isEditMode && isDatePartsPicker\" v-model=\"internalDateParts\" v-bind=\"configAttributes\" />\n<DatePicker v-else-if=\"isEditMode\" v-model=\"internalValue\" v-bind=\"configAttributes\" />\n<span v-else>\n    {{ formattedString }}\n    <template v-if=\"elapsedString\">\n        ({{ elapsedString }})\n    </template>\n</span>"
            })));
        }
    };
});
//# sourceMappingURL=DateField.js.map