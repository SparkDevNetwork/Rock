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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Reporting.ReportList;

namespace Rock.Blocks.Reporting
{
    /// <summary>
    /// Displays a list of reports.
    /// </summary>
    [DisplayName( "Report List" )]
    [Category( "Reporting" )]
    [Description( "Lists all reports under a specified report category." )]
    [IconCssClass( "fa fa-list" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    [CategoryField( "Report Category",
        Description = "Category to use to list reports for.",
        AllowMultiple = false,
        EntityTypeName = "Rock.Model.Report",
        IsRequired = true,
        DefaultValue = AttributeDefaultValues.Organization,
        Key = AttributeKey.ReportCategory,
        Order = 0 )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the report details.",
        Key = AttributeKey.DetailPage,
        Order = 1 )]

    [Rock.SystemGuid.EntityTypeGuid( "084e3594-e399-4639-8461-88333399cba2" )]
    [Rock.SystemGuid.BlockTypeGuid( "7c01525c-2fcc-4f0b-a9b5-25e8146af0d7" )]
    [CustomizedGrid]
    public class ReportList : RockEntityListBlockType<Report>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string ReportCategory = "ReportCategory";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class AttributeDefaultValues
        {
            public const string Organization = "89e54497-5e98-4f1b-b83a-95bfb685da91";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<ReportListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private ReportListOptionsBag GetBoxOptions()
        {
            var options = new ReportListOptionsBag();

            return options;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "ReportId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Report> GetListQueryable( RockContext rockContext )
        {
            var reportService = new ReportService( rockContext );

            var reportCategory = GetAttributeValue( AttributeKey.ReportCategory ).AsGuid();

            return reportService.Queryable().Where( r => r.Category.Guid == reportCategory );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Report> GetGridBuilder()
        {
            return new GridBuilder<Report>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "description", a => a.Description )
                .AddField( "isSystem", a => a.IsSystem );
        }

        #endregion
    }
}
