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

using Rock.Enums;

namespace Rock.Model
{
    /// <summary>
    /// Reporting Field Comparison Types
    /// </summary>
    [Flags]
    [EnumDomain( "Reporting" )]
    public enum ComparisonType
    {
        /// <summary>
        /// Equal
        /// </summary>
        [EnumOrder( 1 )]
        EqualTo = 0x1,

        /// <summary>
        /// Not equal
        /// </summary>
        [EnumOrder( 2 )]
        NotEqualTo = 0x2,

        /// <summary>
        /// Starts with
        /// </summary>
        /// <remarks>
        /// The order for <see cref="StartsWith"/> is set so that it is displayed before <see cref="EndsWith"/>
        /// </remarks>
        [EnumOrder( 11 )]
        StartsWith = 0x4,

        /// <summary>
        /// Contains
        /// </summary>
        [EnumOrder( 3 )]
        Contains = 0x8,

        /// <summary>
        /// Does not contain
        /// </summary>
        [EnumOrder( 4 )]
        DoesNotContain = 0x10,

        /// <summary>
        /// Is blank
        /// </summary>
        [EnumOrder( 5 )]
        IsBlank = 0x20,

        /// <summary>
        /// Is not blank
        /// </summary>
        [EnumOrder( 6 )]
        IsNotBlank = 0x40,

        /// <summary>
        /// Greater than
        /// </summary>
        [EnumOrder( 7 )]
        GreaterThan = 0x80,

        /// <summary>
        /// Greater than or equal
        /// </summary>
        [EnumOrder( 8 )]
        GreaterThanOrEqualTo = 0x100,

        /// <summary>
        /// Less than
        /// </summary>
        [EnumOrder( 9 )]
        LessThan = 0x200,

        /// <summary>
        /// Less than or equal
        /// </summary>
        [EnumOrder( 10 )]
        LessThanOrEqualTo = 0x400,

        /// <summary>
        /// Ends with
        /// </summary>
        /// /// <remarks>
        /// The order for <see cref="StartsWith"/> is set so that it is displayed before <see cref="EndsWith"/>
        /// </remarks>
        [EnumOrder( 12 )]
        EndsWith = 0x800,

        /// <summary>
        /// Between
        /// </summary>
        [EnumOrder( 13 )]
        Between = 0x1000,

        /// <summary>
        /// Regular Expression
        /// </summary>
        [EnumOrder( 14 )]
        RegularExpression = 0x2000,
    }
}
