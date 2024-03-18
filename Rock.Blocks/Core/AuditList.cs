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
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.AuditList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of audits.
    /// </summary>

    [DisplayName( "Audit List" )]
    [Category( "Core" )]
    [Description( "Displays a list of audits." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "8d4a9e56-30f1-4a2d-bd00-7803d7d51909" )]
    [Rock.SystemGuid.BlockTypeGuid( "120552e2-5c36-4220-9a73-fbbbd75b0964" )]
    [CustomizedGrid]
    public class AuditList : RockEntityListBlockType<Audit>
    {
        #region Keys

        private static class PreferenceKey
        {
            public const string FilterEntityType = "filter-entity-type";

            public const string FilterEntityId = "filter-entity-id";

            public const string FilterWho = "filter-who";
        }

        #endregion Keys

        #region Properties

        protected string FilterEntityType => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterEntityType );

        protected int? FilterEntityId => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterEntityId )
            .AsIntegerOrNull();

        protected ListItemBag FilterWho => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterWho )
            .FromJsonOrNull<ListItemBag>();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<AuditListOptionsBag>();
            var builder = GetGridBuilder();

            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private AuditListOptionsBag GetBoxOptions()
        {
            var options = new AuditListOptionsBag()
            {
                EntityTypeItems = EntityTypeCache.All()
                .Where( e => e.IsEntity )
                .OrderBy( e => e.FriendlyName )
                .ToListItemBagList()
            };

            return options;
        }

        /// <inheritdoc/>
        protected override IQueryable<Audit> GetListQueryable( RockContext rockContext )
        {
            var query = base.GetListQueryable( rockContext )
                .AsNoTracking()
                .Include( a => a.Details )
                .Include( a => a.PersonAlias )
                .Include( a => a.PersonAlias.Person );

            // Filter by Entity Type
            if ( Guid.TryParse( FilterEntityType, out var filterEntityType ) )
            {
                query = query.Where( a => a.EntityType.Guid == filterEntityType );
            }

            // Filter by Entity Id
            if ( FilterEntityId.HasValue && FilterEntityId > 0 )
            {
                query = query.Where( a => a.EntityId == FilterEntityId );
            }

            // Filter by Who/Person
            if ( FilterWho != null && Guid.TryParse( FilterWho.Value, out var personGuid ) )
            {
                query = query.Where( a => a.PersonAlias.Guid == personGuid );
            }

            return query;
        }

        /// <inheritdoc/>
        protected override GridBuilder<Audit> GetGridBuilder()
        {
            int? nullInt = null;

            return new GridBuilder<Audit>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "auditType", a => a.AuditType.ConvertToStringSafe() )
                .AddTextField( "entityType", a => a.EntityType.FriendlyName )
                .AddTextField( "title", a => a.Title )
                .AddDateTimeField( "dateTime", a => a.DateTime )
                .AddField( "entityId", a => a.EntityId )
                .AddField( "properties", a => a.Details.Count() )
                .AddPersonField( "person", a => a.PersonAlias?.Person )
                .AddField( "personId", a => a.PersonAlias != null ? a.PersonAlias.PersonId : nullInt );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets a list of <see cref="Rock.Model.AuditDetail"/> for the specified <paramref name="idKey"/>
        /// </summary>
        /// <param name="idKey">The identifier key of the audit whose details should be loaded.</param>
        /// <returns>A list of <see cref="Rock.Model.AuditDetail"/>
        /// whose <seealso cref="Rock.Model.Audit"/>.IdKey matches the <paramref name="idKey"/>
        /// </returns>
        [BlockAction]
        public BlockActionResult GetAuditDetails( string idKey )
        {
            using ( var rockContext = new RockContext() )
            {
                var auditId = new AuditService( rockContext ).Get( idKey )?.Id;

                if ( auditId == null )
                {
                    return ActionBadRequest( $"Audit Information not found" );
                }

                var auditDetailService = new AuditDetailService( rockContext );

                var auditDetails = auditDetailService
                    .Queryable()
                    .AsNoTracking()
                    .Where( a => a.AuditId == auditId )
                    .Select( a => new {
                        a.Property,
                        a.OriginalValue,
                        a.CurrentValue
                    })
                    .ToList();

                if ( !auditDetails.Any() )
                {
                    return ActionBadRequest( $"No Properties" );
                }

                return ActionOk( auditDetails );
            }
        }

        #endregion
    }
}
