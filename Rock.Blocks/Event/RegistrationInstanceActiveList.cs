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
using Rock.ViewModels.Blocks.Event.RegistrationInstanceActiveList;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays a list of active registration instances.
    /// </summary>

    [DisplayName( "Registration Instance Active List" )]
    [Category( "Event" )]
    [Description( "Displays a list of registration instances." )]
    [IconCssClass( "ti ti-file" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the registration instance details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "3951453c-e9fc-4f43-8b7b-794c5acfcabe" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "5e899ccb-3c24-4f7d-9843-2f1cb00aed8f" )]
    [Rock.SystemGuid.BlockTypeGuid( "CFE8CAFA-587B-4EF2-A457-18047AC6BA39" )]
    [CustomizedGrid]
    public class RegistrationInstanceActiveList : RockEntityListBlockType<RegistrationInstance>
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

        private static class PageParameterKey
        {
            public const string CategoryId = "CategoryId";
            public const string RegistrationTemplateId = "RegistrationTemplateId";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// Non-temporary registrant counts keyed by registration instance Id, populated once per grid load in <see cref="GetListItems"/>.
        /// </summary>
        private Dictionary<int, int> _registrantCounts = new Dictionary<int, int>();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<RegistrationInstanceActiveListOptionsBag>();
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
        private RegistrationInstanceActiveListOptionsBag GetBoxOptions()
        {
            var options = new RegistrationInstanceActiveListOptionsBag();
            options.IsGridVisible = PageParameter( PageParameterKey.CategoryId ).IsNullOrWhiteSpace() && PageParameter( PageParameterKey.RegistrationTemplateId ).IsNullOrWhiteSpace();
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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "RegistrationInstanceId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<RegistrationInstance> GetListQueryable( RockContext rockContext )
        {
            var qry = new RegistrationInstanceService( rockContext )
                    .Queryable()
                    .Include( i => i.RegistrationTemplate.Category )
                    .Where( i =>
                        ( i.StartDateTime <= RockDateTime.Now || !i.StartDateTime.HasValue ) &&
                        ( i.EndDateTime > RockDateTime.Now || !i.EndDateTime.HasValue ) &&
                        i.IsActive );

            return qry;
        }

        /// <inheritdoc/>
        protected override IQueryable<RegistrationInstance> GetOrderedListQueryable( IQueryable<RegistrationInstance> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( i => i.StartDateTime );
        }

        /// <inheritdoc/>
        protected override List<RegistrationInstance> GetListItems( IQueryable<RegistrationInstance> queryable, RockContext rockContext )
        {
            var listItems = base.GetListItems( queryable, RockContext )
                .Where( i => i.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .ToList();

            if ( listItems.Count == 0 )
            {
                return listItems;
            }

            var instanceIds = listItems.ConvertAll( i => i.Id );

            _registrantCounts = new RegistrationRegistrantService( RockContext ).Queryable().AsNoTracking()
                .Where( rr => !rr.Registration.IsTemporary && instanceIds.Contains( rr.Registration.RegistrationInstanceId ) )
                .GroupBy( rr => rr.Registration.RegistrationInstanceId )
                .Select( g => new { RegistrationInstanceId = g.Key, Count = g.Count() } )
                .ToDictionary( c => c.RegistrationInstanceId, c => c.Count );

            return listItems;
        }

        /// <inheritdoc/>
        protected override GridBuilder<RegistrationInstance> GetGridBuilder()
        {
            return new GridBuilder<RegistrationInstance>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddDateTimeField( "startDateTime", a => a.StartDateTime )
                .AddDateTimeField( "endDateTime", a => a.EndDateTime )
                .AddTextField( "details", a => a.Details )
                .AddField( "registrantsCount", a => _registrantCounts.GetValueOrDefault( a.Id, 0 ) )
                .AddField( "isActive", a => a.IsActive )
                .AddAttributeFields( GetGridAttributes() );
        }

        #endregion
    }
}
