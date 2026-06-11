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

namespace Rock.ViewModels.Blocks.Example.ModelMap
{
    /// <summary>
    /// Represents a method of a model class.
    /// </summary>
    public class ModelMapMethodBag
    {

        /// <summary>
        /// Gets or sets the metadata token identifier of the method.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the XML documentation comments for the method.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this method is inherited from a base class.
        /// </summary>
        public bool IsInherited { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this method is obsolete.
        /// </summary>
        public bool IsObsolete { get; set; }

        /// <summary>
        /// Gets or sets the message explaining why the method is obsolete, if applicable.
        /// </summary>
        public string ObsoleteMessage { get; set; }

        /// <summary>
        /// Gets or sets the method signature (e.g., Name(Type Param1, Type Param2)).
        /// </summary>
        public string Signature { get; set; }
    }
}
