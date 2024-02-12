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
using Rock.ViewModels.Blocks.Communication.CommunicationList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Displays a list of communications.
    /// </summary>
    [DisplayName( "Communication List" )]
    [Category( "Communication" )]
    [Description( "Lists the status of all previously created communications." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [SecurityAction( Authorization.APPROVE,
        "The roles and/or users that have access to approve new communications." )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the communication details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "e4bd5cad-579e-476d-87ec-989de975bb60" )]
    [Rock.SystemGuid.BlockTypeGuid( "c3544f53-8e2d-43d6-b165-8fefc541a4eb" )]
    [CustomizedGrid]
    public class CommunicationList : RockEntityListBlockType<Rock.Model.Communication>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
            public const string RootUrl = "RootUrl";
        }

        private static class PageParameterKey
        {
            public const string CommunicationId = "CommunicationId";
        }

        private static class PreferenceKey
        {
            public const string FilterSubject = "filter-subject";
            public const string FilterCommunicationType = "filter-communication-type";
            public const string FilterStatus = "filter-status";
            public const string FilterCreatedBy = "filter-created-by";
            public const string FilterCreatedDateRangeFrom = "filter-created-date-from";
            public const string FilterCreatedDateRangeTo = "filter-created-date-to";
            public const string FilterSentDateRangeFrom = "filter-sent-date-range-from";
            public const string FilterSentDateRangeTo = "filter-sent-date-range-to";
            public const string FilterContent = "filter-content";
            public const string FilterRecipientCountRangeFrom = "filter-recipient-count-from";
            public const string FilterRecipientCountRangeTo = "filter-recipient-count-to";
        }

        #endregion Keys

        #region Properties

        protected string FilterSubject => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterSubject );

        protected string FilterCommunicationType => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterCommunicationType );

        protected string FilterStatus => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterStatus );

        protected ListItemBag FilterCreatedBy => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterCreatedBy )
            .FromJsonOrNull<ListItemBag>();

        protected DateTime? FilterCreatedDateRangeFrom { get; set; }

        protected DateTime? FilterCreatedDateRangeTo => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterCreatedDateRangeTo ).AsDateTime();

        protected DateTime? FilterSentDateRangeFrom => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterSentDateRangeFrom ).AsDateTime();

        protected DateTime? FilterSentDateRangeTo => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterSentDateRangeTo ).AsDateTime();

        protected string FilterContent => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterContent );

        protected int? FilterRecipientCountRangeFrom => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterRecipientCountRangeFrom ).AsIntegerOrNull();

        protected int? FilterRecipientCountRangeTo => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterRecipientCountRangeTo ).AsIntegerOrNull();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<CommunicationListOptionsBag>();
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
        private CommunicationListOptionsBag GetBoxOptions()
        {
            var options = new CommunicationListOptionsBag()
            {
                CanApprove = GetCanApprove(),
                CurrentPerson = GetCurrentPerson()?.ToListItemBag(),
                CommunicationTypeItems = typeof( CommunicationType ).ToEnumListItemBag(),
                StatusItems = typeof( CommunicationStatus ).ToEnumListItemBag(),
            };

            return options;
        }

        /// <summary>
        /// Determines of the current user can approve new communications.
        /// </summary>
        /// <returns></returns>
        private bool GetCanApprove()
        {
            return BlockCache.IsAuthorized( Authorization.APPROVE, GetCurrentPerson() );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, PageParameterKey.CommunicationId, "((Key))" ),
                [NavigationUrlKey.RootUrl] = RequestContext.ResolveRockUrl( "/" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Rock.Model.Communication> GetListQueryable( RockContext rockContext )
        {
            var queryable = new CommunicationService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( c => c.Status != CommunicationStatus.Transient );

            queryable = FilterBySubject( queryable );

            queryable = FilterByCommunicationType( queryable );

            queryable = FilterByCommunicationStatus( queryable );

            queryable = FilterByPerson( queryable );

            queryable = FilterByRecipientCount( queryable );

            queryable = FilterByCreatedDateRange( queryable );

            queryable = FilterBySentDateRange( queryable );

            queryable = FilterByContent( queryable );

            return queryable;
        }

        /// <summary>
        /// Filters the queryable by the selected Communication Status filter.
        /// </summary>
        /// <param name="queryable">The <see cref="Rock.Model.Communication"/> queryable</param>
        /// <returns></returns>
        private IQueryable<Model.Communication> FilterByCommunicationStatus( IQueryable<Model.Communication> queryable )
        {
            if ( !string.IsNullOrWhiteSpace( FilterStatus ) && Enum.TryParse( FilterStatus, out CommunicationStatus communicationStatus ) )
            {
                queryable = queryable.Where( c => c.Status == communicationStatus );
            }

            return queryable;
        }

        /// <summary>
        /// Filters the queryable by the selected Communication Type filter.
        /// </summary>
        /// <param name="queryable">The <see cref="Rock.Model.Communication"/> queryable</param>
        /// <returns></returns>
        private IQueryable<Model.Communication> FilterByCommunicationType( IQueryable<Model.Communication> queryable )
        {
            if ( !string.IsNullOrWhiteSpace( FilterCommunicationType ) && Enum.TryParse( FilterCommunicationType, out CommunicationType communicationType ) )
            {
                queryable = queryable.Where( c => c.CommunicationType == communicationType );
            }

            return queryable;
        }

        /// <summary>
        /// Filters the queryable by the selected subject filter.
        /// </summary>
        /// <param name="queryable">The <see cref="Rock.Model.Communication"/> queryable</param>
        /// <returns></returns>
        private IQueryable<Model.Communication> FilterBySubject( IQueryable<Model.Communication> queryable )
        {
            if ( !string.IsNullOrWhiteSpace( FilterSubject ) )
            {
                queryable = queryable.Where( c => ( string.IsNullOrEmpty( c.Subject ) && c.Name.Contains( FilterSubject ) ) || c.Subject.Contains( FilterSubject ) );
            }

            return queryable;
        }

        /// <summary>
        /// Filters the queryable by the selected Content filter.
        /// </summary>
        /// <param name="queryable">The <see cref="Rock.Model.Communication"/> queryable</param>
        /// <returns></returns>
        private IQueryable<Model.Communication> FilterByContent( IQueryable<Model.Communication> queryable )
        {
            if ( !string.IsNullOrWhiteSpace( FilterContent ) )
            {
                // Concatenate the content fields and search the result.
                // This achieves better query performance than searching the fields individually.
                queryable = queryable.Where( c =>
                                    ( c.Message + c.SMSMessage + c.PushMessage ).Contains( FilterContent ) );
            }

            return queryable;
        }

        /// <summary>
        /// Filters the queryable by the selected Send Date Range filter.
        /// </summary>
        /// <param name="queryable">The <see cref="Rock.Model.Communication"/> queryable</param>
        /// <returns></returns>
        private IQueryable<Model.Communication> FilterBySentDateRange( IQueryable<Model.Communication> queryable )
        {
            if ( FilterSentDateRangeFrom.HasValue )
            {
                queryable = queryable.Where( a => ( a.SendDateTime ?? a.FutureSendDateTime ) >= FilterSentDateRangeFrom.Value );
            }

            if ( FilterSentDateRangeTo.HasValue )
            {
                DateTime upperDate = FilterSentDateRangeTo.Value.Date.AddDays( 1 );
                queryable = queryable.Where( a => ( a.SendDateTime ?? a.FutureSendDateTime ) < upperDate );
            }

            return queryable;
        }

        /// <summary>
        /// Filters the queryable by the selected Created Date Range filter.
        /// </summary>
        /// <param name="queryable">The <see cref="Rock.Model.Communication"/> queryable</param>
        /// <returns></returns>
        private IQueryable<Model.Communication> FilterByCreatedDateRange( IQueryable<Model.Communication> queryable )
        {
            if ( !FilterCreatedDateRangeFrom.HasValue && !FilterCreatedDateRangeTo.HasValue )
            {
                FilterCreatedDateRangeFrom = RockDateTime.Today.AddDays( -7 );
            }
            else
            {
                FilterCreatedDateRangeFrom = GetBlockPersonPreferences().GetValue( PreferenceKey.FilterCreatedDateRangeFrom ).AsDateTime();
            }

            if ( FilterCreatedDateRangeFrom.HasValue )
            {
                queryable = queryable.Where( a => a.CreatedDateTime >= FilterCreatedDateRangeFrom.Value );
            }

            if ( FilterCreatedDateRangeTo.HasValue )
            {
                DateTime upperDate = FilterCreatedDateRangeTo.Value.Date.AddDays( 1 );
                queryable = queryable.Where( a => a.CreatedDateTime < upperDate );
            }

            return queryable;
        }

        /// <summary>
        /// Filters the queryable by the selected Recipient Count filter.
        /// </summary>
        /// <param name="queryable">The <see cref="Rock.Model.Communication"/> queryable</param>
        /// <returns></returns>
        private IQueryable<Model.Communication> FilterByRecipientCount( IQueryable<Model.Communication> queryable )
        {
            if ( FilterRecipientCountRangeFrom.HasValue )
            {
                queryable = queryable.Where( a => a.Recipients.Count >= FilterRecipientCountRangeFrom.Value );
            }

            if ( FilterRecipientCountRangeTo.HasValue )
            {
                queryable = queryable.Where( a => a.Recipients.Count <= FilterRecipientCountRangeTo.Value );
            }

            return queryable;
        }

        /// <summary>
        /// Filters the queryable by the selected Person filter.
        /// </summary>
        /// <param name="queryable">The <see cref="Rock.Model.Communication"/> queryable</param>
        /// <returns></returns>
        private IQueryable<Model.Communication> FilterByPerson( IQueryable<Model.Communication> queryable )
        {
            var currentPerson = GetCurrentPerson();

            if ( GetCanApprove() )
            {
                var createdBy = FilterCreatedBy?.Value.AsGuidOrNull();
                if ( FilterCreatedBy != null && createdBy.HasValue )
                {
                    queryable = queryable
                        .Where( c =>
                            c.SenderPersonAlias != null &&
                            c.SenderPersonAlias.Guid == createdBy.Value );
                }
            }
            else if ( currentPerson != null )
            {
                // If can't approve, only show current person's communications
                queryable = queryable
                    .Where( c =>
                        c.SenderPersonAlias != null &&
                        c.SenderPersonAlias.PersonId == currentPerson.Id );
            }

            return queryable;
        }

        /// <inheritdoc/>
        protected override List<Model.Communication> GetListItems( IQueryable<Model.Communication> queryable, RockContext rockContext )
        {
            return queryable.WherePersonAuthorizedToView( rockContext, GetCurrentPerson() ).ToList();
        }

        /// <inheritdoc/>
        protected override IQueryable<Model.Communication> GetOrderedListQueryable( IQueryable<Model.Communication> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( c => c.SendDateTime );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Rock.Model.Communication> GetGridBuilder()
        {
            var recipients = new CommunicationRecipientService( new RockContext() ).Queryable();

            return new GridBuilder<Rock.Model.Communication>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "communicationType", a => a.CommunicationType )
                .AddTextField( "subject", a => string.IsNullOrEmpty( a.Subject ) ? ( string.IsNullOrEmpty( a.PushTitle ) ? a.Name : a.PushTitle ) : a.Subject )
                .AddDateTimeField( "createdDateTime", a => a.CreatedDateTime )
                .AddDateTimeField( "sendDateTime", a => a.SendDateTime ?? a.FutureSendDateTime )
                .AddDateTimeField( "futureSendDateTime", a => a.FutureSendDateTime )
                .AddPersonField( "sender", a => a.SenderPersonAlias?.Person )
                .AddDateTimeField( "reviewedDateTime", a => a.ReviewedDateTime )
                .AddPersonField( "reviewer", a => a.ReviewerPersonAlias?.Person )
                .AddField( "status", a => a.Status )
                .AddField( "recipients", a => recipients.Count( r => r.CommunicationId == a.Id ) )
                .AddField( "pendingRecipients", a => recipients.Count( r => r.CommunicationId == a.Id && r.Status == CommunicationRecipientStatus.Pending ) )
                .AddField( "cancelledRecipients", a => recipients.Count( r => r.CommunicationId == a.Id && r.Status == CommunicationRecipientStatus.Cancelled ) )
                .AddField( "failedRecipients", a => recipients.Count( r => r.CommunicationId == a.Id && r.Status == CommunicationRecipientStatus.Failed ) )
                .AddField( "deliveredRecipients", a => recipients.Count( r => r.CommunicationId == a.Id && ( r.Status == CommunicationRecipientStatus.Delivered || r.Status == CommunicationRecipientStatus.Opened ) ) )
                .AddField( "openedRecipients", a => recipients.Count( r => r.CommunicationId == a.Id && r.Status == CommunicationRecipientStatus.Opened ) );
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
            using ( var rockContext = new RockContext() )
            {
                var entityService = new CommunicationService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{Rock.Model.Communication.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${Rock.Model.Communication.FriendlyTypeName}." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Creates a copy of the specified Communication.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult Copy( string key )
        {
            var linkedPageUrl = string.Empty;

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.DetailPage ) ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    var communicationService = new CommunicationService( rockContext );
                    var communicationId = Rock.Utility.IdHasher.Instance.GetId( key ) ?? 0;

                    var newCommunication = communicationService.Copy( communicationId, GetCurrentPerson()?.PrimaryAliasId );
                    if ( newCommunication != null )
                    {
                        communicationService.Add( newCommunication );
                        rockContext.SaveChanges();

                        var newCommunicationId = newCommunication.Id;

                        var pageParams = new Dictionary<string, string>
                        {
                            { PageParameterKey.CommunicationId, newCommunicationId.ToString() }
                        };

                        linkedPageUrl = this.GetLinkedPageUrl( AttributeKey.DetailPage, pageParams );
                    }
                }
            }

            return ActionOk( linkedPageUrl );
        }

        #endregion
    }
}
