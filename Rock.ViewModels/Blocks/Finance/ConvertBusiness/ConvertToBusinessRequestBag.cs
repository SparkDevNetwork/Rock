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

namespace Rock.ViewModels.Blocks.Finance.ConvertBusiness
{
    /// <summary>
    /// Represents the client request to convert a person record into a business record.
    /// </summary>
    public class ConvertToBusinessRequestBag
    {
        /// <summary>
        /// The person alias GUID that identifies the selected source person record.
        /// </summary>
        public Guid? SourcePersonAliasGuid { get; set; }

        /// <summary>
        /// The business name to store on the converted business record.
        /// </summary>
        public string BusinessName { get; set; }
    }
}
