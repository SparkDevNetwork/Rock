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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.AssessmentHistory;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Displays Assessment History on the Person Profile's History tab. Allows a person to see and delete (if needed) pending
    /// assessment requests.
    /// </summary>

    [DisplayName( "Assessment History" )]
    [Category( "CRM" )]
    [Description( "Displays Assessment History on the Person Profile's History tab. Allows a person to see and delete (if needed) pending assessment requests." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( SiteType.Web )]

    [ContextAware( typeof( Person ) )]
    [Rock.SystemGuid.EntityTypeGuid( "BAE276ED-91C1-4D4B-B086-75A7D77B4576" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "2BC87A2F-B2ED-45C1-91D3-F5AE757BBDC5" )]
    [Rock.SystemGuid.BlockTypeGuid( "E7EB1E42-FEA7-4735-83FE-A618BD2616BF" )]
    [CustomizedGrid]
    public class AssessmentHistory : RockEntityListBlockType<Assessment>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<AssessmentHistoryOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = GetIsDeleteEnabled();
            box.ExpectedRowCount = null;
            box.Options = new AssessmentHistoryOptionsBag();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Determines if the current person is allowed to delete pending assessments.
        /// </summary>
        /// <returns><c>true</c> if the delete button should be available; otherwise <c>false</c>.</returns>
        private bool GetIsDeleteEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the identifier of the person whose assessment history should be
        /// displayed, from either the page context or the PersonId page parameter.
        /// </summary>
        /// <returns>The person identifier, or <c>null</c> if no person could be resolved.</returns>
        private int? GetPersonId()
        {
            var contextPerson = RequestContext.GetContextEntity<Person>();

            if ( contextPerson != null )
            {
                return contextPerson.Id;
            }

            var personKey = PageParameter( PageParameterKey.PersonId );

            return new PersonService( RockContext )
                .GetSelect( personKey, p => ( int? ) p.Id, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <inheritdoc/>
        protected override IQueryable<Assessment> GetListQueryable( RockContext rockContext )
        {
            var personId = GetPersonId();

            if ( !personId.HasValue )
            {
                return Enumerable.Empty<Assessment>().AsQueryable();
            }

            return base.GetListQueryable( rockContext )
                .Include( a => a.AssessmentType )
                .Include( a => a.RequesterPersonAlias.Person )
                .Where( a => a.PersonAlias.PersonId == personId.Value );
        }

        /// <inheritdoc/>
        protected override IQueryable<Assessment> GetOrderedListQueryable( IQueryable<Assessment> queryable, RockContext rockContext )
        {
            return queryable
                .OrderByDescending( a => a.RequestedDateTime )
                .ThenBy( a => a.AssessmentType.Title );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Assessment> GetGridBuilder()
        {
            return new GridBuilder<Assessment>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "assessmentType", a => a.AssessmentType.Title )
                .AddTextField( "status", a => a.Status == AssessmentRequestStatus.Complete ? "Complete" : "Pending" )
                .AddField( "isCompleted", a => a.Status == AssessmentRequestStatus.Complete )
                .AddDateTimeField( "requestedDateTime", a => a.RequestedDateTime )
                .AddPersonField( "requestedBy", a => a.RequesterPersonAlias?.Person )
                .AddDateTimeField( "completedDateTime", a => a.CompletedDateTime );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Deletes the specified pending assessment.
        /// </summary>
        /// <param name="key">The identifier of the assessment to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            if ( !GetIsDeleteEnabled() )
            {
                return ActionBadRequest( $"Not authorized to delete {Assessment.FriendlyTypeName}." );
            }

            var entityService = new AssessmentService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{Assessment.FriendlyTypeName} not found." );
            }

            // Completed assessments are part of the person's permanent history
            // and must never be removed, only pending requests can be deleted.
            if ( entity.Status != AssessmentRequestStatus.Pending )
            {
                return ActionBadRequest( "Only pending assessments can be deleted." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion Block Actions
    }
}
