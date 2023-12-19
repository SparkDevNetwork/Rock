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
using System.Runtime.Serialization;

namespace Rock.CheckIn
{
    /// <summary>
    /// A POCO to represent an entity list item that may be selected for check-in.
    /// </summary>
    [DataContract]
    public class CheckInEntityListItem
    {
        /// <summary>
        /// The entity identifier.
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// The entity name.
        /// </summary>
        [DataMember]
        public string Name { get; set; }
    }
}
