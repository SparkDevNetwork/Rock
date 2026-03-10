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
using Rock.Cms;
using Rock.Enums.Cms;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.SmartSearch;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Provides extensible options for searching in Rock.
    /// </summary>
    [DisplayName( "Smart Search" )]
    [Category( "Core" )]
    [Description( "Provides extensible options for searching in Rock." )]
    [IconCssClass( "ti ti-search" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [DefaultBlockRole( BlockRole.System )]
    [Rock.SystemGuid.EntityTypeGuid( "A9F9C061-0073-4A7A-93DB-693A1F17D585" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "9DAFE2D5-AC68-44AC-B648-A83CE39C8788" )]
    [Rock.SystemGuid.BlockTypeGuid( "9D406BD5-88C1-45E5-AFEA-70F9CFB66C74" )]
    public class SmartSearch : RockBlockType
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new CustomBlockBox<object, SmartSearchOptionsBag>
            {
                Options = new SmartSearchOptionsBag()
            };
        }

        #endregion Methods
    }
}
