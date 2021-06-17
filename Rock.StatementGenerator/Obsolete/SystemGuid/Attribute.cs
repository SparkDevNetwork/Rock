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
using System;

namespace Rock.StatementGenerator.SystemGuid
{
    /// <summary>
    /// 
    /// </summary>

    [Obsolete( "Use FinancialStatementTemplate instead" )]
    [RockObsolete( "12.4" )]
    public static class Attribute
    {
        /// <summary>
        /// The person do not send giving statement
        /// </summary>
        public const string PERSON_DO_NOT_SEND_GIVING_STATEMENT = Rock.SystemGuid.Attribute.PERSON_DO_NOT_SEND_GIVING_STATEMENT;

        /// <summary>
        /// The defined value statement generator lava template lava template
        /// </summary>
        public const string DEFINEDVALUE_STATEMENT_GENERATOR_LAVA_TEMPLATE_LAVA_TEMPLATE = "BA34F8F8-D828-47E0-8401-5940E0A9BFBA";

        /// <summary>
        /// The defined value statement generator lava template logo
        /// </summary>
        public const string DEFINEDVALUE_STATEMENT_GENERATOR_LAVA_TEMPLATE_LOGO = "8E097DC9-386C-4ED5-B4F3-E0E88B40621E";

        /// <summary>
        /// The defined value statement generator lava template footer HTML
        /// </summary>
        public const string DEFINEDVALUE_STATEMENT_GENERATOR_LAVA_TEMPLATE_FOOTERHTML = "B20A936B-6F73-4371-A2A1-83513E58A630";

        /// <summary>
        /// The defined value statement generator lava template PDF Object Settings
        /// </summary>
        public const string DEFINEDVALUE_STATEMENT_GENERATOR_LAVA_TEMPLATE_PDFOBJECTSETTINGS = "20CB85A1-09D8-4D5B-A1A5-7FB63C7355C2";

    }
}
