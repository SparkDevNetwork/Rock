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

namespace Rock.Lava
{
    /// <summary>
    /// Represents information about a Lava template document element, such as a tag or block.
    /// </summary>
    public interface ILavaElementInfo
    {
        /// <summary>
        /// The name of the element.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The qualified name of the System.Type that implements this element.
        /// </summary>
        string SystemTypeName { get; }

        /// <summary>
        /// Can the factory method successfully produce an instance of this tag?
        /// </summary>
        bool IsAvailable { get; set; }

        /// <summary>
        /// A user-friendly summary of this element.
        /// </summary>
        /// <returns></returns>
        string ToString();
    }
}
