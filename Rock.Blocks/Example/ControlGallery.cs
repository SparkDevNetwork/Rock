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
using Rock.ViewModels.Blocks.Example.ControlGallery;

namespace Rock.Blocks.Example
{
    /// <summary>
    /// Allows the user to try out various controls.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Control Gallery" )]
    [Category( "Obsidian > Example" )]
    [Description( "Allows the user to try out various controls." )]
    [IconCssClass( "fa fa-flask" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [BooleanField( "Show Reflection",
        Description = "When enabled, a Show Reflection option will be enabled that will add a second control to demonstrate two-way databinding.  This is typically only useful to developers when they are developing a new control.",
        DefaultValue = "false",
        Order = 0,
        Key = AttributeKey.ShowReflection )]

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.OBSIDIAN_EXAMPLE_CONTROL_GALLERY )]
    [Rock.SystemGuid.BlockTypeGuid( "6FAB07FF-D4C6-412B-B13F-7B881ECBFAD0" )]
    public class ControlGallery : RockBlockType
    {
        public static class AttributeKey
        {
            public const string ShowReflection = "ShowReflection";
        }

        /// <inheritdoc/>
        public override string ObsidianFileUrl => base.ObsidianFileUrl.ReplaceIfEndsWith( ".obs", string.Empty );

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ControlGalleryInitializationBox();

            box.ShowReflection = GetAttributeValue( AttributeKey.ShowReflection ).AsBoolean();

            return box;
        }
    }
}
