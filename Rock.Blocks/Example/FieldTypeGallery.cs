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

using System.ComponentModel;

using Rock.Attribute;
using Rock.Model;

namespace Rock.Blocks.Example
{
    /// <summary>
    /// Allows the user to try out various field types.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Field Type Gallery" )]
    [Category( "Obsidian > Example" )]
    [Description( "Allows the user to try out various field types." )]
    [IconCssClass( "fa fa-flask" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.OBSIDIAN_EXAMPLE_FIELD_TYPE_GALLERY )]
    [Rock.SystemGuid.BlockTypeGuid( "50B7B326-8212-44E6-8CF6-515B1FF75A19")]
    public class FieldTypeGallery : RockBlockType
    {
    }
}
