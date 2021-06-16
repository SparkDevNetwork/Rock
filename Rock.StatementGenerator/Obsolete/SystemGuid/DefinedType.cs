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
    /// Static Guids used by the Rock.StatementGenerator application
    /// </summary>
    [Obsolete( "Use FinancialStatementTemplate instead" )]
    [RockObsolete( "12.4" )]
    public static class DefinedType
    {
        /// <summary>
        /// The statement generator lava template (Legacy)
        /// </summary>
        public const string STATEMENT_GENERATOR_LAVA_TEMPLATE = "74A23516-A20A-40C9-93B5-1AB5FDFF6750";
    }
}