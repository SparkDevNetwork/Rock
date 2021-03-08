// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
System.register([], function (exports_1, context_1) {
    "use strict";
    var RegistrationPersonFieldType, RegistrationFieldSource, FilterExpressionType, ComparisonType;
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
        }
    };
});
//# sourceMappingURL=RegistrationEntryBlockViewModel.js.map