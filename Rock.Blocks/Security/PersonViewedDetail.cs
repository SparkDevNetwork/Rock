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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Security.PersonViewedDetail;
using Rock.Web.Cache;

namespace Rock.Blocks.Security
{
    /// <summary>
    /// Displays the details of person views.
    /// </summary>

    [DisplayName( "Person Viewed Detail" )]
    [Category( "Security" )]
    [Description( "Displays the details of person views." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]


    [Rock.SystemGuid.EntityTypeGuid( "4bd62b58-6e21-4769-ae19-2fca3638fd07" )]
    //Was [Rock.SystemGuid.BlockTypeGuid( "2c46e0cc-ba4c-4bb3-af2f-963052336c3b" )]
    [Rock.SystemGuid.BlockTypeGuid( "132D18F3-D169-4260-94E0-84F42A40B356" )]
    [CustomizedGrid]
    public class PersonViewedDetail : RockEntityListBlockType<PersonViewed>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKeys
        {
            public const string ViewedBy = "ViewedBy";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PersonViewedDetailOptionsBag>();
            var builder = GetGridBuilder();

            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        private Person GetPersonFromPageParameter( string pageParamKey )
        {
            var paramValue = PageParameter( pageParamKey );

            return new PersonService( RockContext ).Get( paramValue, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private PersonViewedDetailOptionsBag GetBoxOptions()
        {
            bool viewedBy = Convert.ToBoolean(PageParameter( PageParameterKeys.ViewedBy ));
            var targetPerson = GetPersonFromPageParameter( "TargetId" );
            var viewerPerson = GetPersonFromPageParameter( "ViewerId" );
            var options = new PersonViewedDetailOptionsBag();
            if ( targetPerson != null && viewerPerson != null )
            {
                options.Title = viewedBy
                    ? $"{viewerPerson.FullName} Viewed {targetPerson.FullName}"
                    : $"{targetPerson.FullName} Viewed by {viewerPerson.FullName}";
            }

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "PersonViewedId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<PersonViewed> GetListQueryable( RockContext rockContext )
        {
            var targetPerson = GetPersonFromPageParameter( "TargetId" );
            var viewerPerson = GetPersonFromPageParameter( "ViewerId" );
            var result = base.GetListQueryable( rockContext )
                .Where( p =>
                    p.ViewerPersonAlias != null &&
                    p.ViewerPersonAlias.PersonId == viewerPerson.Id &&
                    p.TargetPersonAlias != null &&
                    p.TargetPersonAlias.PersonId == targetPerson.Id );
            return result;
        }

        protected override IQueryable<PersonViewed> GetOrderedListQueryable( IQueryable<PersonViewed> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( p => p.ViewDateTime );
        }

        /// <inheritdoc/>
        protected override GridBuilder<PersonViewed> GetGridBuilder()
        {
            return new GridBuilder<PersonViewed>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "source", a => a.Source )
                .AddDateTimeField( "date", a => a.ViewDateTime)
                .AddTextField( "ipAddress", a => a.IpAddress );
        }

        #endregion

        #region Block Actions

        #endregion
    }
}

