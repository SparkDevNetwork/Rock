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

using System.Threading.Tasks;

using Rock.Attribute;

namespace Rock.Blocks
{
    /// <summary>
    /// A Rock block that is designed to exist on a web page.
    /// </summary>
    /// <seealso cref="Rock.Blocks.IRockBlockType" />
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "1.13" )]
    public interface IRockWebBlockType : IRockBlockType
    {
        /// <summary>
        /// Gets the control markup that will be used to initialize this block.
        /// The returned text will be injected into the HTML page.
        /// </summary>
        /// <returns>A string that contains HTML markup.</returns>
        Task<string> GetControlMarkupAsync();
    }
}
