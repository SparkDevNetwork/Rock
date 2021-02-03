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
using Fluid;
using Irony.Parsing;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// An implementation of the Fluid Grammar definition that is modified to include Lava syntax variations.
    /// </summary>
    internal class LavaFluidGrammar : FluidGrammar
    {
        public LavaFluidGrammar()
        {
            // Redefine the format of string literal terms to prevent the backslash ("\") from being interpreted as an escape character.
            // This is necessary to ensure that arguments passed to the RegexMatch filters include the literal backslash as expected.
            var stringLiteralSingle = new StringLiteral( "string1", "'", StringOptions.AllowsDoubledQuote | StringOptions.NoEscapes | StringOptions.AllowsLineBreak );
            var stringLiteralDouble = new StringLiteral( "string2", "\"", StringOptions.AllowsDoubledQuote | StringOptions.NoEscapes | StringOptions.AllowsLineBreak );

            var number = new NumberLiteral( "number", NumberOptions.AllowSign )
            {
                DefaultIntTypes = new TypeCode[] { TypeCode.Decimal },
                DefaultFloatType = TypeCode.Decimal
            };

            Term.Rule = MemberAccess | stringLiteralSingle | stringLiteralDouble | number | Boolean;
        }
    }
}
