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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.RegistrationInstanceList;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays a list of registration instances.
    /// </summary>
    [DisplayName( "Registration Instance List" )]
    [Category( "Event" )]
    [Description( "Lists all the instances of the given registration template." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the registration instance details.",
        Key = AttributeKey.DetailPage )]
    [Rock.SystemGuid.EntityTypeGuid( "f9bde297-09cd-456b-bfe3-31fe9eb28d5b" )]
    [Rock.SystemGuid.BlockTypeGuid( "3a56fe6a-f216-4ef3-9059-acc1f5906428" )]
    [CustomizedGrid]
    public class RegistrationInstanceList : RockEntityListBlockType<RegistrationInstance>
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
            public const string RegistrationTemplateId = "RegistrationTemplateId";
        }

        #endregion Keys

        #region Fields

        private RegistrationTemplate _template;

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<RegistrationInstanceListOptionsBag>();
            var builder = GetGridBuilder();
            var isAddDeleteEnabled = GetIsAddDeleteEnabled();

            box.IsAddEnabled = isAddDeleteEnabled;
            box.IsDeleteEnabled = isAddDeleteEnabled;
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
        private RegistrationInstanceListOptionsBag GetBoxOptions()
        {
            var template = GetRegistrationTemplate();
            var options = new RegistrationInstanceListOptionsBag()
            {
                CanView = template?.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) == true,
                WaitListEnabled = template?.WaitListEnabled == true,
                TemplateName = template?.Name
            };
            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            return GetRegistrationTemplate()?.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) == true;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var template = GetRegistrationTemplate();

            var queryParams = new Dictionary<string, string>()
            {
                { "RegistrationInstanceId", "((Key))" },
                { "RegistrationTemplateId", template?.IdKey },
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, queryParams )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<RegistrationInstance> GetListQueryable( RockContext rockContext )
        {
            var templateId = GetRegistrationTemplate()?.Id;
            IEnumerable<RegistrationInstance> registrationInstances = new List<RegistrationInstance>();

            if ( templateId.HasValue )
            {
                registrationInstances = new RegistrationInstanceService( rockContext ).Queryable()
                    .AsNoTracking()
                    .Where( i => i.RegistrationTemplateId == templateId );
            }

            return registrationInstances.AsQueryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<RegistrationInstance> GetOrderedListQueryable( IQueryable<RegistrationInstance> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( a => a.StartDateTime );
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
                .AddField( "isActive", a => a.IsActive )
                .AddField( "registrants", a => a.Registrations.Where( r => !r.IsTemporary ).SelectMany( r => r.Registrants ).Count( r => !r.OnWaitList ) )
                .AddField( "waitList", a => a.Registrations.Where( r => !r.IsTemporary ).SelectMany( r => r.Registrants ).Count( r => r.OnWaitList ) );
        }

        private RegistrationTemplate GetRegistrationTemplate()
        {
            var idParameter = PageParameter( PageParameterKey.RegistrationTemplateId );
            var templateId = idParameter.AsIntegerOrNull() ?? Rock.Utility.IdHasher.Instance.GetId( idParameter );

            if ( templateId.HasValue && _template == null )
            {
                _template = new RegistrationTemplateService( RockContext )
                    .Queryable( "GroupType.Roles" )
                    .AsNoTracking()
                    .FirstOrDefault( i => i.Id == templateId.Value );
            }

            return _template;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new RegistrationInstanceService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{RegistrationInstance.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete ${RegistrationInstance.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            RockContext.WrapTransaction( () =>
            {
                new RegistrationService( RockContext ).DeleteRange( entity.Registrations );
                entityService.Delete( entity );
                RockContext.SaveChanges();
            } );

            return ActionOk();
        }

        #endregion Block Actions
    }
}