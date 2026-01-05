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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Cms.EmailForm
{
    /// <summary>
    /// Bag for the Send block action request.
    /// </summary>
    public partial class EmailFormRequestBag
    {
        /// <summary>
        /// Gets or sets the form fields.
        /// </summary>
        public Dictionary<string, string> FormFields { get; set; }

        /// <summary>
        /// Gets or sets the attachment file GUIDs.
        /// </summary>
        public List<Guid> AttachmentGuids { get; set; }
    }
}