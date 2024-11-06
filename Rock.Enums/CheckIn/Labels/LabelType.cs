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
namespace Rock.Enums.CheckIn.Labels
{
    /// <summary>
    /// The type of label defined in the check-in system. This determines
    /// under what conditions the label is printed.
    /// </summary>
    public enum LabelType
    {
        /// <summary>
        /// The label is printed once per check-in session.
        /// </summary>
        Family = 0,

        /// <summary>
        /// The label is printed for every person.
        /// </summary>
        Person = 1,

        /// <summary>
        /// The label is printed for every attendance record.
        /// </summary>
        Attendance = 2,

        /// <summary>
        /// The label is printed once for each person during check-out.
        /// </summary>
        Checkout = 3,

        /// <summary>
        /// The label is printed once for each combination of person and
        /// location.
        /// </summary>
        PersonLocation = 4,
    }
}
