System.register([], function (exports_1, context_1) {
    "use strict";
    var RegistrationPersonFieldType, RegistrationFieldSource, FilterExpressionType, ComparisonType, RegistrarOption, RegistrantsSameFamily;
    var __moduleName = context_1 && context_1.id;
    return {
        setters: [],
        execute: function () {
            (function (RegistrationPersonFieldType) {
                RegistrationPersonFieldType[RegistrationPersonFieldType["FirstName"] = 0] = "FirstName";
                RegistrationPersonFieldType[RegistrationPersonFieldType["LastName"] = 1] = "LastName";
                RegistrationPersonFieldType[RegistrationPersonFieldType["Campus"] = 2] = "Campus";
                RegistrationPersonFieldType[RegistrationPersonFieldType["Address"] = 3] = "Address";
                RegistrationPersonFieldType[RegistrationPersonFieldType["Email"] = 4] = "Email";
                RegistrationPersonFieldType[RegistrationPersonFieldType["Birthdate"] = 5] = "Birthdate";
                RegistrationPersonFieldType[RegistrationPersonFieldType["Gender"] = 6] = "Gender";
                RegistrationPersonFieldType[RegistrationPersonFieldType["MaritalStatus"] = 7] = "MaritalStatus";
                RegistrationPersonFieldType[RegistrationPersonFieldType["MobilePhone"] = 8] = "MobilePhone";
                RegistrationPersonFieldType[RegistrationPersonFieldType["HomePhone"] = 9] = "HomePhone";
                RegistrationPersonFieldType[RegistrationPersonFieldType["WorkPhone"] = 10] = "WorkPhone";
                RegistrationPersonFieldType[RegistrationPersonFieldType["Grade"] = 11] = "Grade";
                RegistrationPersonFieldType[RegistrationPersonFieldType["ConnectionStatus"] = 12] = "ConnectionStatus";
                RegistrationPersonFieldType[RegistrationPersonFieldType["MiddleName"] = 13] = "MiddleName";
                RegistrationPersonFieldType[RegistrationPersonFieldType["AnniversaryDate"] = 14] = "AnniversaryDate";
            })(RegistrationPersonFieldType || (RegistrationPersonFieldType = {}));
            exports_1("RegistrationPersonFieldType", RegistrationPersonFieldType);
            (function (RegistrationFieldSource) {
                RegistrationFieldSource[RegistrationFieldSource["PersonField"] = 0] = "PersonField";
                RegistrationFieldSource[RegistrationFieldSource["PersonAttribute"] = 1] = "PersonAttribute";
                RegistrationFieldSource[RegistrationFieldSource["GroupMemberAttribute"] = 2] = "GroupMemberAttribute";
                RegistrationFieldSource[RegistrationFieldSource["RegistrantAttribute"] = 4] = "RegistrantAttribute";
            })(RegistrationFieldSource || (RegistrationFieldSource = {}));
            exports_1("RegistrationFieldSource", RegistrationFieldSource);
            (function (FilterExpressionType) {
                FilterExpressionType[FilterExpressionType["Filter"] = 0] = "Filter";
                FilterExpressionType[FilterExpressionType["GroupAll"] = 1] = "GroupAll";
                FilterExpressionType[FilterExpressionType["GroupAny"] = 2] = "GroupAny";
                FilterExpressionType[FilterExpressionType["GroupAllFalse"] = 3] = "GroupAllFalse";
                FilterExpressionType[FilterExpressionType["GroupAnyFalse"] = 4] = "GroupAnyFalse";
            })(FilterExpressionType || (FilterExpressionType = {}));
            exports_1("FilterExpressionType", FilterExpressionType);
            (function (ComparisonType) {
                ComparisonType[ComparisonType["EqualTo"] = 1] = "EqualTo";
                ComparisonType[ComparisonType["NotEqualTo"] = 2] = "NotEqualTo";
                ComparisonType[ComparisonType["StartsWith"] = 4] = "StartsWith";
                ComparisonType[ComparisonType["Contains"] = 8] = "Contains";
                ComparisonType[ComparisonType["DoesNotContain"] = 16] = "DoesNotContain";
                ComparisonType[ComparisonType["IsBlank"] = 32] = "IsBlank";
                ComparisonType[ComparisonType["IsNotBlank"] = 64] = "IsNotBlank";
                ComparisonType[ComparisonType["GreaterThan"] = 128] = "GreaterThan";
                ComparisonType[ComparisonType["GreaterThanOrEqualTo"] = 256] = "GreaterThanOrEqualTo";
                ComparisonType[ComparisonType["LessThan"] = 512] = "LessThan";
                ComparisonType[ComparisonType["LessThanOrEqualTo"] = 1024] = "LessThanOrEqualTo";
                ComparisonType[ComparisonType["EndsWith"] = 2048] = "EndsWith";
                ComparisonType[ComparisonType["Between"] = 4096] = "Between";
                ComparisonType[ComparisonType["RegularExpression"] = 8192] = "RegularExpression";
            })(ComparisonType || (ComparisonType = {}));
            exports_1("ComparisonType", ComparisonType);
            (function (RegistrarOption) {
                RegistrarOption[RegistrarOption["PromptForRegistrar"] = 0] = "PromptForRegistrar";
                RegistrarOption[RegistrarOption["PrefillFirstRegistrant"] = 1] = "PrefillFirstRegistrant";
                RegistrarOption[RegistrarOption["UseFirstRegistrant"] = 2] = "UseFirstRegistrant";
                RegistrarOption[RegistrarOption["UseLoggedInPerson"] = 3] = "UseLoggedInPerson";
            })(RegistrarOption || (RegistrarOption = {}));
            exports_1("RegistrarOption", RegistrarOption);
            (function (RegistrantsSameFamily) {
                RegistrantsSameFamily[RegistrantsSameFamily["No"] = 0] = "No";
                RegistrantsSameFamily[RegistrantsSameFamily["Yes"] = 1] = "Yes";
                RegistrantsSameFamily[RegistrantsSameFamily["Ask"] = 2] = "Ask";
            })(RegistrantsSameFamily || (RegistrantsSameFamily = {}));
            exports_1("RegistrantsSameFamily", RegistrantsSameFamily);
        }
    };
});
//# sourceMappingURL=RegistrationEntryBlockViewModel.js.map