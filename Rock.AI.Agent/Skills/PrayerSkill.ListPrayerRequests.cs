using System;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class PrayerSkill
    {
        #region Tool(s)
        [Description( "Lists prayer requests." )]
        [AgentToolGuid( "99F1EDE0-F431-49BE-80F5-97032710143B" )]
        [AgentUsage( "Most filters are optional. If none are provided, the most recent prayer requests are returned." )]
        [AgentUsage( "Use either the first and last name filter or the requested by IdKey, not both." )]
        [AgentUsage( "Results are paginated (PageNumber is required)." )]
        public IAgentToolResult ListPrayerRequests(
            string categoryIdKey = "",

            [Description("Optional. If provided, only prayer requests that are children of this category will be returned.")]
            string parentCategoryIdKey = "",

            DateTime? startDate = null,
            DateTime? endDate = null,
            bool? isPublic = null,
            bool? isUrgent = null,
            bool? isActive = null,

            [Description("Optional. If provided, only prayer requests where the first name contains this value will be returned.")]
            string firstName = null,

            [Description("Optional. If provided, only prayer requests where the last name contains this value will be returned.")]
            string lastName = null,

            [Description("Optional. The IdKey of the person this prayer is about.")]
            string requestedByPersonIdKey = null,

            int pageNumber = 1 )
        {
            var helper = new AgentToolHelper( AgentRequestContext, _logger );

            var categoryId = helper.GetOptionalEntity<Model.Category>( categoryIdKey )?.Id;

            int? parentCategoryId = null;
            if ( !categoryId.HasValue )
            {
                parentCategoryId = helper.GetOptionalEntity<Model.Category>( parentCategoryIdKey )?.Id;

                if ( !parentCategoryId.HasValue && ConfigurationValues.TryGetValue( ConfigurationKey.ParentCategory, out var parentCategoryGuid ) )
                {
                    parentCategoryId = CategoryCache.Get( parentCategoryGuid.AsGuid(), AgentRequestContext.RockContext )?.Id;

                    if ( !parentCategoryId.HasValue )
                    {
                        helper.AddError( "The configured parent category is not valid." );
                    }
                }
            }

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            // Query
            var qry = new PrayerRequestService( AgentRequestContext.RockContext )
                .Queryable();

            if ( categoryId.HasValue )
            {
                qry = qry.Where( pr => pr.CategoryId == categoryId );
            }
            else if ( parentCategoryId.HasValue )
            {
                // Only direct children of the parent category 
                qry = qry.Where( pr => pr.Category != null && pr.Category.ParentCategoryId == parentCategoryId );
            }

            qry = helper.WhereOptionalIdKey( qry, pr => pr.RequestedByPersonAlias.PersonId, requestedByPersonIdKey );
            qry = helper.WhereOptionalPropertyBetween( qry, pr => pr.EnteredDateTime, startDate, endDate );
            qry = helper.WhereOptionalProperty( qry, pr => pr.IsPublic, isPublic );
            qry = helper.WhereOptionalProperty( qry, pr => pr.IsUrgent, isUrgent );
            qry = helper.WhereOptionalProperty( qry, pr => pr.IsActive, isActive );

            // Only match on first or last name if a person was not specified.
            if ( requestedByPersonIdKey.IsNullOrWhiteSpace() )
            {
                var fn = firstName?.Trim();
                var ln = lastName?.Trim();

                if ( fn.IsNotNullOrWhiteSpace() )
                {
                    qry = qry.Where( pr => pr.FirstName.Contains( fn ) );
                }

                if ( ln.IsNotNullOrWhiteSpace() )
                {
                    qry = qry.Where( pr => pr.LastName.Contains( ln ) );
                }
            }

            // Sort: newest first; tie-break by Id for determinism
            qry = qry.OrderByDescending( pr => pr.EnteredDateTime )
                .ThenBy( pr => pr.Id );

            var includeCategoryInItem = categoryId.HasValue;
            var itemQry = qry
                .Select( pr => new PrayerRequestResult
                {
                    Id = pr.Id,
                    Text = pr.Text,
                    EnteredDateTime = pr.EnteredDateTime,
                    IsUrgent = pr.IsUrgent,
                    IsActive = pr.IsActive,
                    IsApproved = pr.IsApproved,
                    IsPublic = pr.IsPublic,
                    PrayerCount = pr.PrayerCount,
                    Category = ( !categoryId.HasValue && pr.Category != null )
                        ? new KeyNameResult
                        {
                            Id = pr.Category.Id,
                            Name = pr.Category.Name
                        }
                        : null
                } );

            var page = helper.GetPaginatedItems( itemQry, pageNumber );

            // Slim it down for the history content
            var historyPage = page.WithItems( page.Items.Select( pr => new
            {
                pr.IdKey,
                Text = pr.Text.Truncate( 200 ),
            } ) );

            return Success( page )
                .WithHistoryContent( historyPage );
        }

        #endregion
    }
}
