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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Administration.ExternalApplicationList;
using Rock.ViewModels.Blocks.Security.AuthScopeList;
using Rock.Web.Cache;

namespace Rock.Blocks.Administration
{
    /// <summary>
    /// Displays a list of auth scopes.
    /// </summary>
    [DisplayName( "External Application List" )]
    [Category( "Administration" )]
    [Description( "Will list all of the defined type values with the type of External Application.  This provides a way for users to select any one of these files." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( SiteType.Web )]

    [SystemGuid.EntityTypeGuid( "0EB5D32A-D26F-4B4F-A336-D7206539895D" )]
    // Was [SystemGuid.BlockTypeGuid( "2A18F4BF-633F-47CE-A228-3F908AA5A189" )]
    [Rock.SystemGuid.BlockTypeGuid( "850A0541-D31A-4559-94D1-9DAD5F52EFDF" )]
    [CustomizedGrid]
    public class ExternalApplicationList : RockListBlockType<ExternalApplicationList.ExternalApplicationData>
    {
        #region Keys

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<ExternalApplicationListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private ExternalApplicationListOptionsBag GetBoxOptions()
        {
            var options = new ExternalApplicationListOptionsBag();

            return options;
        }

        /// <inheritdoc/>
        protected override IQueryable<ExternalApplicationData> GetListQueryable(RockContext rockContext)
        {
            // Get the GUID for the External Application DefinedType.
            var externalApplicationDefinedTypeGuid = Rock.SystemGuid.DefinedType.EXTERNAL_APPLICATION.AsGuid();

            // Get all DefinedValues for the External Application DefinedType.
            var definedValueService = new DefinedValueService(rockContext);
            var externalApplications = definedValueService.GetByDefinedTypeGuid(externalApplicationDefinedTypeGuid).AsNoTracking().ToList();

            var externalApplicationDataList = new List<ExternalApplicationData>();

            foreach (var definedValue in externalApplications)
            {
                definedValue.LoadAttributes();

                var externalApplicationData = new ExternalApplicationData
                {
                    IdKey = definedValue.IdKey,
                    Value = definedValue.Value,
                    Description = definedValue.Description,
                    Vendor = definedValue.GetAttributeValue("Vendor"),
                    DownloadUrl = definedValue.GetAttributeValue("DownloadUrl"),
                    Icon = definedValue.GetAttributeValue("Icon"),
                    IsActive = definedValue.IsActive,
                    IsSystem = definedValue.IsSystem
                };

                externalApplicationDataList.Add(externalApplicationData);
            }

            return externalApplicationDataList.AsQueryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<ExternalApplicationData> GetOrderedListQueryable( IQueryable<ExternalApplicationData> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( a => a.Value );
        }

        /// <inheritdoc/>
        protected override GridBuilder<ExternalApplicationData> GetGridBuilder()
        {
            var gridBuilder = new GridBuilder<ExternalApplicationData>();

            gridBuilder.WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "value", a => a.Value )
                .AddTextField( "description", a => a.Description )
                .AddTextField( "vendor", a => a.Vendor)
                .AddTextField( "downloadUrl", a => a.DownloadUrl )
                .AddField( "icon", a => a.Icon )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "isSystem", a => a.IsSystem );

            return gridBuilder;
        }

        #endregion

        #region Block Actions

        #endregion

        #region Support Classes

        public class ExternalApplicationData
        {
            public string IdKey { get; set; }
            public string Value { get; set; }
            public string Description { get; set; }
            public string Vendor { get; set; }
            public string DownloadUrl { get; set; }
            public string Icon { get; set; }
            public bool IsActive { get; set; }
            public bool IsSystem { get; set; }
        }

        #endregion
    }
}
