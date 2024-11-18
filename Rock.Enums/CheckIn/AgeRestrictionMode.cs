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

namespace Rock.Enums.CheckIn
{
    /// <summary>
    /// Defines the age restriction for a check-in configuration. This applies
    /// a filter when considering which people can be listed on the family
    /// member selection screen. Using one of the Hide modes will hide those
    /// people from the screen even if there is a valid opportunity for them to
    /// check into.
    /// </summary>
    public enum AgeRestrictionMode
    {
        /// <summary>
        /// All ages will be shown.
        /// </summary>
        ShowAll = 0,

        /// <summary>
        /// Adults will not be shown or available for check-in.
        /// </summary>
        HideAdults = 1,

        /// <summary>
        /// Children will not be shown or available for check-in.
        /// </summary>
        HideChildren = 2
    }
}
