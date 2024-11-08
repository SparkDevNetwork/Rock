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

using System.ComponentModel;

namespace Rock.Enums.Crm
{
    /// <summary>
    /// The age bracket of a person
    /// </summary>
    public enum AgeBracket
    {
        /// <summary>
        /// Unknown Age range
        /// </summary>
        [Description( "Unknown" )]
        Unknown = 0,

        /// <summary>
        /// Age range 0 - 5
        /// </summary>
        /// 0 - 12 was later split into 0-5 and 6-12 hence the irregular enum value.
        [Description( "0 - 5" )]
        ZeroToFive = 1,

        /// <summary>
        /// Age range 6 - 12
        /// </summary>
        [Description( "6 - 12" )]
        SixToTwelve = 2,

        /// <summary>
        /// Age range 13 - 17
        /// </summary>
        [Description( "13 - 17" )]
        ThirteenToSeventeen = 3,

        /// <summary>
        /// Age range 18 - 24
        /// </summary>
        [Description( "18 - 24" )]
        EighteenToTwentyFour = 4,

        /// <summary>
        /// Age range 25 - 34
        /// </summary>
        [Description( "25 - 34" )]
        TwentyFiveToThirtyFour = 5,

        /// <summary>
        /// Age range 35 - 44
        /// </summary>
        [Description( "35 - 44" )]
        ThirtyFiveToFortyFour = 6,

        /// <summary>
        /// Age range 45 - 54
        /// </summary>
        [Description( "45 - 54" )]
        FortyFiveToFiftyFour = 7,

        /// <summary>
        /// Age range 55 - 64
        /// </summary>
        [Description( "55 - 64" )]
        FiftyFiveToSixtyFour = 8,

        /// <summary>
        /// Age range 65+
        /// </summary>
        [Description( "65+" )]
        SixtyFiveOrOlder = 9
    }
}
