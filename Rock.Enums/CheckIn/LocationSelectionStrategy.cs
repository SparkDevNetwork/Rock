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
    /// If a group has more than one available location how should one be
    /// selected. Choosing an option other than "Ask" will auto select a location
    /// for the attendee.
    /// </summary>
    public enum LocationSelectionStrategy
    {
        /// <summary>
        /// The Ask strategy will present a list of rooms to the user so they
        /// can select one. This is the default behavior.
        /// </summary>
        Ask = 0,

        /// <summary>
        /// The balance strategy will attempt to fill all locations with an
        /// equal number of persons up to the soft threshold.
        /// </summary>
        Balance = 1,

        /// <summary>
        /// The Fill In Order strategy will fill in the locations in a group in
        /// their sort order. When the location's soft threshold is reached the
        /// next one is used untill it fills up.
        /// </summary>
        FillInOrder = 2
    }
}
