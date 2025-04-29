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

using Rock.ViewModels.Controls;
using Rock.ViewModels.Rest.Controls;

namespace Rock.ViewModels.Workflow
{
    /// <summary>
    /// The values for the Person Entry component on a Workflow Form.
    /// </summary>
    public class PersonEntryValuesBag
    {
        /// <summary>
        /// The primary person.
        /// </summary>
        public PersonBasicEditorBag Person { get; set; }

        /// <summary>
        /// The spouse of the person.
        /// </summary>
        public PersonBasicEditorBag Spouse { get; set; }

        /// <summary>
        /// The campus unique identifier.
        /// </summary>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// The marital status between the two individuals.
        /// </summary>
        public Guid? MaritalStatusGuid { get; set; }

        /// <summary>
        /// The address of both individuals.
        /// </summary>
        public AddressControlBag Address { get; set; }
    }
}
