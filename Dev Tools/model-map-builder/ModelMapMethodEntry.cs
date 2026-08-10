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

namespace Rock.ModelMapBuilder
{
    /// <summary>
    /// A method of a model. Only included in the output when methods are
    /// explicitly requested (see the <c>--include-methods</c> option).
    /// </summary>
    internal class ModelMapMethodEntry
    {
        /// <summary>
        /// Gets or sets the method signature (e.g. Name(Type param1, Type param2)).
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// Gets or sets the method's XML documentation comment.
        /// </summary>
        public ModelMapComment Comment { get; set; }

        /// <summary>
        /// Determines whether <see cref="Comment"/> should be serialized.
        /// </summary>
        public bool ShouldSerializeComment()
        {
            return Comment != null && !Comment.IsEmpty;
        }

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
    }
}
