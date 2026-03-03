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

namespace Rock.ViewModels.Blocks.Example.ModelMap
{
    /// <summary>
    /// Represents the detailed information about a model class.
    /// </summary>
    public class ModelMapModelBag
    {
        /// <summary>
        /// Gets or sets the name of the model.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the formatted model description.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the formatted model example.
        /// </summary>
        public string Example { get; set; }

        /// <summary>
        /// Gets or sets the list of properties in the model.
        /// </summary>
        public List<ModelMapPropertyBag> Properties { get; set; }

        /// <summary>
        /// Gets or sets the list of methods in the model.
        /// </summary>
        public List<ModelMapMethodBag> Methods { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this model is obsolete.
        /// </summary>
        public bool IsObsolete { get; set; }

        /// <summary>
        /// Gets or sets the message explaining why the model is obsolete, if applicable.
        /// </summary>
        public string ObsoleteMessage { get; set; }

        /// <summary>
        /// Gets or sets the name of the database table associated with this model.
        /// </summary>
        public string TableName { get; set; }
    }
}
