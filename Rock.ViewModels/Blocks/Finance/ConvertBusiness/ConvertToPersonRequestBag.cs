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

using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Finance.ConvertBusiness
{
    /// <summary>
    /// Represents the client request to convert a business record into a person record.
    /// </summary>
    public class ConvertToPersonRequestBag
    {
        /// <summary>
        /// The person alias GUID that identifies the selected source business record.
        /// </summary>
        public Guid? SourcePersonAliasGuid { get; set; }

        /// <summary>
        /// The first name to set on the converted person record.
        /// </summary>
        public string PersonFirstName { get; set; }

        /// <summary>
        /// The last name to set on the converted person record.
        /// </summary>
        public string PersonLastName { get; set; }

        /// <summary>
        /// The connection status to assign to the converted person record.
        /// </summary>
        public ListItemBag PersonConnectionStatus { get; set; }

        /// <summary>
        /// The marital status to assign to the converted person record.
        /// </summary>
        public ListItemBag PersonMaritalStatus { get; set; }

        /// <summary>
        /// The gender to assign to the converted person record.
        /// </summary>
        public Gender PersonGender { get; set; }
    }
}
