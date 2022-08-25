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
namespace Rock.SystemGuid
{
    /// <summary>
    /// Rock Guid attribute to be used to specify the <see cref="Rock.Model.BlockType">BlockType.Guid</see> for <see cref="Rock.Web.UI.RockBlock"/> and <see cref="Rock.Blocks.IRockBlockType" /> classes.
    /// So, this would include
    /// <list type="bullet">
    /// <item>WebForm Blocks</item>
    /// <item>Mobile Blocks</item>
    /// <item>Obsidian Blocks</item>
    /// </list>
    /// <para>
    /// This should be added to these classes by the developer. However, the CodeGenerator will catch any missing ones and add it.
    /// </para>
    /// </summary>
    /// <seealso cref="RockGuidAttribute" />
    [System.AttributeUsage( System.AttributeTargets.Class, Inherited = true, AllowMultiple = false )]
    public class BlockTypeGuidAttribute : RockGuidAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockTypeGuidAttribute"/> class.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        public BlockTypeGuidAttribute( string guid )
            : base( guid )
        {
        }
    }
}