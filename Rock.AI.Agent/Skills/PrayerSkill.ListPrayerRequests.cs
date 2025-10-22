using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class PrayerSkill
    {
        #region Tool(s)

        [AgentToolGuid( "99F1EDE0-F431-49BE-80F5-97032710143B" )]
        [AgentUsage( "Most filters are optional. If none are provided, the most recent prayer requests are returned." )]
        [AgentUsage( "Use either the first and last name filter or the requested by IdKey, not both." )]
        [AgentUsage( "Results are paginated (PageNumber is required)." )]
        public RockToolResult ListPrayerRequests(
           [Description("Optional. If provided, only prayer requests in this category will be returned.")]
            string categoryIdKey = "",
           [Description("Optional. If provided, only prayer requests that are children of this category will be returned. If not provided, and a default parent category is configured for the skill, that will be used.")]
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
            // Normalize and validate the inputs.
            var pgNumber = pageNumber < 1 ? 1 : pageNumber;
            const int pageSize = 10;
            var offset = ( pgNumber - 1 ) * pageSize;
            var take = pageSize + 1; // lookahead for hasMore

            if ( startDate.HasValue && endDate.HasValue && endDate.Value <= startDate.Value )
            {
                return RockToolResult.Error( "The endDate must be after the startDate." );
            }

            // Resolve category (wins over parent) and then parent category
            CategoryCache category = null;
            if ( categoryIdKey.IsNotNullOrWhiteSpace() )
            {
                var categoryId = IdHasher.Instance.GetId( categoryIdKey );
                category = CategoryCache.Get( categoryId ?? 0 );
                if ( category == null )
                {
                    return RockToolResult.Error( "Invalid category provided." );
                }
            }

            CategoryCache parentCategory = null;
            if ( category == null )
            {
                if ( parentCategoryIdKey.IsNotNullOrWhiteSpace() )
                {
                    var parentCategoryId = IdHasher.Instance.GetId( parentCategoryIdKey );
                    parentCategory = CategoryCache.Get( parentCategoryId ?? 0 );
                    if ( parentCategory == null )
                    {
                        return RockToolResult.Error( "Invalid parent category provided." );
                    }
                }
                else if ( ConfigurationValues.TryGetValue( ConfigurationKey.ParentCategory, out var parentCategoryGuid ) )
                {
                    parentCategory = CategoryCache.Get( parentCategoryGuid.AsGuid() );
                    if ( parentCategory == null )
                    {
                        return RockToolResult.Error( "The configured parent category is not valid." );
                    }
                }
            }

            // Query
            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var service = new PrayerRequestService( rockContext );
                int? requestedByPersonAliasId = null;

                if ( requestedByPersonIdKey.IsNotNullOrWhiteSpace() )
                {
                    var personService = new PersonService( rockContext );
                    var requestedByPersonId = IdHasher.Instance.GetId( requestedByPersonIdKey );

                    if ( requestedByPersonId == null )
                    {
                        return RockToolResult.Error( "The requestedByPersonIdKey is not valid." );
                    }

                    var requestedByPerson = personService.Get( requestedByPersonId.Value );
                    if ( requestedByPerson == null )
                    {
                        return RockToolResult.Error( "The requestedByPersonIdKey is not valid." );
                    }

                    requestedByPersonAliasId = requestedByPerson.PrimaryAliasId;
                }

                // Base query (no tracking for read-only)
                var qry = service.Queryable().AsNoTracking();

                // Category filter
                if ( category != null )
                {
                    qry = qry.Where( pr => pr.CategoryId == category.Id );
                }
                else if ( parentCategory != null )
                {
                    // Only direct children of the parent category 
                    qry = qry.Where( pr => pr.Category != null && pr.Category.ParentCategoryId == parentCategory.Id );
                }
                // Date range
                if ( startDate.HasValue )
                {
                    qry = qry.Where( pr => pr.EnteredDateTime >= startDate.Value );
                }
                if ( endDate.HasValue )
                {
                    // exclusive end is usually cleaner for paging windows
                    qry = qry.Where( pr => pr.EnteredDateTime < endDate.Value );
                }

                // Optional flags
                if ( isPublic.HasValue )
                {
                    qry = qry.Where( pr => pr.IsPublic == isPublic.Value );
                }
                if ( isUrgent.HasValue )
                {
                    qry = qry.Where( pr => pr.IsUrgent == isUrgent.Value );
                }

                if ( isActive.HasValue )
                {
                    qry = qry.Where( pr => pr.IsActive == isActive.Value );
                }

                // Either match on the requested by person alias id, or first/last name
                if ( requestedByPersonAliasId.HasValue )
                {
                    qry = qry.Where( pr => pr.RequestedByPersonAliasId == requestedByPersonAliasId.Value );
                }
                else
                {
                    if ( firstName.IsNotNullOrWhiteSpace() )
                    {
                        var fn = firstName.Trim();
                        qry = qry.Where( pr => pr.FirstName.Contains( fn ) );
                    }

                    else if ( lastName.IsNotNullOrWhiteSpace() )
                    {
                        var ln = lastName.Trim();
                        qry = qry.Where( pr => pr.LastName.Contains( ln ) );
                    }
                }

                // Sort: newest first; tie-break by Id for determinism
                qry = qry.OrderByDescending( pr => pr.EnteredDateTime ).ThenBy( pr => pr.Id );

                var includeCategoryInItem = category == null;

                // If there is a specific category filter, we don't need to include the Category in each item.
                // We need to separate this out so there is no EF join to Category if we don't need it.
                var items = qry
                    .Skip( offset ).Take( take )
                    .Select( prayerRequest => new PrayerRequestResult
                    {
                        Id = prayerRequest.Id,
                        Text = prayerRequest.Text,
                        EnteredDateTime = prayerRequest.EnteredDateTime,
                        IsUrgent = prayerRequest.IsUrgent,
                        IsActive = prayerRequest.IsActive,
                        IsApproved = prayerRequest.IsApproved,
                        IsPublic = prayerRequest.IsPublic,
                        PrayerCount = prayerRequest.PrayerCount,
                        Category = ( includeCategoryInItem && prayerRequest.Category != null )
                            ? new KeyNameResult { Id = prayerRequest.Category.Id, Name = prayerRequest.Category.Name }
                            : null
                    } )
                    .ToList();

                var hasMore = items.Count > pageSize;
                if ( hasMore )
                {
                    items.RemoveAt( items.Count - 1 );
                }

                // Slim it down for the history content
                var historyItems = items.Select( pr => new
                {
                    IdKey = pr.IdKey,
                    Text = pr.Text.Truncate( 200 ),
                } );

                // Metadata 
                var meta = new Dictionary<string, object>
                {
                    { "pageNumber", pgNumber },
                    { "pageSize", pageSize },
                    { "returnedRows", items.Count },
                    { "hasMore", hasMore },
                    { "startDate", startDate },
                    { "endDate", endDate },
                    { "filters", new Dictionary<string, object>
                        {
                            { "category", category?.Name ?? "Undefined" },
                            { "parentCategory", parentCategory?.Name ?? "Undefined" },
                            { "isPublic", isPublic },
                            { "isUrgent", isUrgent },
                            { "firstName", firstName },
                            { "lastName", lastName }
                        }
                    }
                };

                return RockToolResult.Success( items )
                    .WithMetadata( meta )
                    .WithHistoryContent( new
                    {
                        Items = historyItems,
                        PageNumber = pageNumber
                    } );
            }
        }

        #endregion
    }
}
