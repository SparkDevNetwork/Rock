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

namespace Rock.ViewModels.Blocks.Communication.CommunicationDetail
{
    /// <summary>
    /// A bag that contains information about a request to create a personal template, for the communication detail block.
    /// </summary>
    public class CreatePersonalTemplateRequestBag
    {
        /// <summary>
        /// Gets or sets the name to use for the new communication template.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description to use for the new communication template.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the category to use for the new communication template.
        /// </summary>
        public Guid? CategoryGuid { get; set; }
    }
}
