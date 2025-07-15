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
using System.IO;

using Rock.Attribute;

namespace Rock.Cms.StructuredContent
{
    /// <summary>
    /// Defines a component that can provide additional functionality to the
    /// standard structured content system in Rock.
    /// </summary>
    /// <remarks>
    /// This is an internal API that supports the Rock infrastructure and not
    /// subject to the same compatibility standards as public APIs. It may be
    /// changed or removed without notice in any release. You should only use
    /// it directly in your code with extreme caution and knowing that doing so
    /// can result in application failures when updating to a new Rock release.
    /// </remarks>
    [RockInternal( "17.1" )]
    public interface IStructuredContentBlockRenderer
    {
        /// <summary>
        /// Renders the block data to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="data">The block data.</param>
        void Render( TextWriter writer, dynamic data );
    }
}
