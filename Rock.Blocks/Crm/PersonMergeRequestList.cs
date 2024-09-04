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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.PersonMergeRequestList;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Displays a list of entity sets.
    /// </summary>
    [DisplayName( "Person Merge Request List" )]
    [Category( "CRM" )]
    [Description( "Lists Person Merge Requests" )]
    [IconCssClass( "fa fa-list" )]
    //[SupportedSiteTypes( SiteType.Web )]

    [SystemGuid.EntityTypeGuid( "9c1a70f8-3177-49c9-97c6-ac3e52fc36b1" )]
    [SystemGuid.BlockTypeGuid( "b2cf80f1-5588-46d5-8198-8c5816290e98" )]
    [CustomizedGrid]
    public class PersonMergeRequestList : RockListBlockType<PersonMergeRequestList.RequestData>
    {
        #region Keys

        private static class NavigationUrlKey
        {
            public const string PersonMergePage = "PersonMergePage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PersonMergeRequestListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsDeleteEnabled = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
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
        private PersonMergeRequestListOptionsBag GetBoxOptions()
        {
            var options = new PersonMergeRequestListOptionsBag();

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
                [NavigationUrlKey.PersonMergePage] = "/PersonMerge/((Key))"
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<RequestData> GetListQueryable( RockContext rockContext )
        {
            var entitySetService = new EntitySetService( rockContext );

            var entitySetPurposeGuid = Rock.SystemGuid.DefinedValue.ENTITY_SET_PURPOSE_PERSON_MERGE_REQUEST.AsGuid();

            var currentDateTime = RockDateTime.Now;
            var entitySetQry = entitySetService.Queryable()
                .Where( a => a.EntitySetPurposeValue.Guid == entitySetPurposeGuid )
                .Where( s => !s.ExpireDateTime.HasValue || s.ExpireDateTime.Value > currentDateTime );

            var qryPersonEntities = entitySetService.GetEntityItems<Person>();

            var joinQry = entitySetQry.GroupJoin( qryPersonEntities, n => n.Id, o => o.EntitySetId, ( a, b ) => new RequestData
            {
                Date = a.ModifiedDateTime,
                Note = a.Note,
                Requestor = a.CreatedByPersonAlias.Person,
                MergeRecords = b.Select( x => x.Item ).ToList(),
                EntitySet = a
            } );

            return joinQry;
        }

        /// <inheritdoc/>
        protected override IQueryable<RequestData> GetOrderedListQueryable( IQueryable<RequestData> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( s => s.Date );
        }

        /// <inheritdoc/>
        protected override GridBuilder<RequestData> GetGridBuilder()
        {
            return new GridBuilder<RequestData>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.EntitySet.IdKey )
                .AddTextField( "note", a => a.Note )
                .AddTextField( "requestor", a => a.Requestor.FullName )
                .AddDateTimeField( "date", a => a.Date )
                .AddField( "mergeRecords", a => a.MergeRecords.ConvertAll( p => p.FullName ) );
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
            var entityService = new EntitySetService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( "Merge request not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "Not authorized to delete merge request." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            // mark it as expired and RockCleanup will delete it later
            entity.ExpireDateTime = RockDateTime.Now.AddMinutes( -1 );
            entity.EntitySetPurposeValueId = null;

            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// Contains the Merge Request details for the Person Merge Request List block.
        /// </summary>
        public class RequestData
        {
            /// <summary>
            /// Gets or sets the identifier key.
            /// </summary>
            /// <value>
            /// The identifier key.
            /// </value>
            public string IdKey { get; set; }

            /// <summary>
            /// Gets or sets the date.
            /// </summary>
            /// <value>
            /// The date.
            /// </value>
            public DateTime? Date { get; set; }

            /// <summary>
            /// Gets or sets the note.
            /// </summary>
            /// <value>
            /// The note.
            /// </value>
            public string Note { get; set; }

            /// <summary>
            /// Gets or sets the requestor.
            /// </summary>
            /// <value>
            /// The requestor.
            /// </value>
            public Person Requestor { get; set; }

            /// <summary>
            /// Gets or sets the merge records.
            /// </summary>
            /// <value>
            /// The merge records.
            /// </value>
            public List<Person> MergeRecords { get; set; }

            /// <summary>
            /// Gets or sets the entity set.
            /// </summary>
            /// <value>
            /// The entity set.
            /// </value>
            public EntitySet EntitySet { get; set; }
        }

        #endregion
    }
}
