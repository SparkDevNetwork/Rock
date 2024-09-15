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

namespace Rock.Enums.CheckIn.Labels
{
    /// <summary>
    /// The sub-type used to identify the specific text field on the label.
    /// </summary>
    public enum TextFieldSubType
    {
        /// <summary>
        /// A custom text field that does not use a data source.
        /// </summary>
        Custom = 0,

        /// <summary>
        /// A text field that uses one of the Attendee Info data sources.
        /// </summary>
        AttendeeInfo = 1,

        /// <summary>
        /// A text field that uses one of the Check-in Info data sources.
        /// </summary>
        [Description( "Check-in Info" )]
        CheckInInfo = 2,

        /// <summary>
        /// A text field that uses one of the Achievement Info data sources.
        /// </summary>
        AchievementInfo = 3
    }
}
