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
    /// The different ways a family search can be performed in check-in.
    /// </summary>
    public enum FamilySearchMode
    {
        /// <summary>
        /// Search for family based on phone number.
        /// </summary>
        PhoneNumber = 0,

        /// <summary>
        /// Search for family based on name.
        /// </summary>
        Name = 1,

        /// <summary>
        /// Search by phone and name at the same time. If the input has at
        /// least one non-numeric character the search will assume to be
        /// name. Otherwise phone number will be used.
        /// </summary>
        NameAndPhone = 2,

        /// <summary>
        /// Search for family based on a barcode, proximity card, etc.
        /// </summary>
        ScannedId = 3,

        /// <summary>
        /// Search for family based on a Family Id.
        /// </summary>
        FamilyId = 4
    }
}
