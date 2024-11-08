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
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Security.SecurityChangeAuditList;
using Rock.Web.Cache;

namespace Rock.Blocks.Security
{
    /// <summary>
    /// Displays a list of auth audit logs.
    /// </summary>
    [DisplayName( "Security Change Audit List" )]
    [Category( "Security" )]
    [Description( "Block for Security Change Audit List." )]
    [IconCssClass( "fa fa-list" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    [SystemGuid.EntityTypeGuid( "5a2e4f3c-9915-4b67-8ffe-87056d2e68df" )]
    [SystemGuid.BlockTypeGuid( "cfe6f48b-ed85-4fa8-b068-efe116b32284" )]
    [CustomizedGrid]
    public class SecurityChangeAuditList : RockEntityListBlockType<AuthAuditLog>
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<SecurityChangeAuditListOptionsBag>();
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
        private SecurityChangeAuditListOptionsBag GetBoxOptions()
        {
            var options = new SecurityChangeAuditListOptionsBag();

            return options;
        }

        /// <inheritdoc/>
        protected override IQueryable<AuthAuditLog> GetListQueryable( RockContext rockContext )
        {
            return new AuthAuditLogService( rockContext ).Queryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<AuthAuditLog> GetOrderedListQueryable( IQueryable<AuthAuditLog> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( a => a.ChangeDateTime );
        }

        /// <inheritdoc/>
        protected override GridBuilder<AuthAuditLog> GetGridBuilder()
        {
            return new GridBuilder<AuthAuditLog>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddDateTimeField( "changeDateTime", a => a.ChangeDateTime )
                .AddTextField( "entityType", a => a.EntityType?.FriendlyName )
                .AddField( "entityId", a => a.EntityId )
                .AddPersonField( "changeBy", a => a.ChangeByPersonAlias?.Person )
                .AddTextField( "action", a => a.Action )
                .AddTextField( "preAllowOrDeny", a => a.PreAllowOrDeny )
                .AddTextField( "postAllowOrDeny", a => a.PostAllowOrDeny )
                .AddTextField( "group", a => a.Group?.Name )
                .AddTextField( "individual", a => a.PersonAlias?.Person?.ToString() )
                .AddTextField( "specialRole", a => a.SpecialRole.ConvertToStringSafe() )
                .AddField( "preOrder", a => a.PreOrder )
                .AddField( "postOrder", a => a.PostOrder )
                .AddField( "change", a => ( int ) a.ChangeType );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new AuthAuditLogService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{AuthAuditLog.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {AuthAuditLog.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }
}
