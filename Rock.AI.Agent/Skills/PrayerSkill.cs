using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// Provides functionality to manage prayer.
    /// </summary>
    [AgentSkillGuid( "0EF2BBFD-52D9-441B-9BE5-F4C5D2B42ED0" )]
    [EntityTypeGuid( "6033D65E-C782-45BA-9A74-23F9B9353A27" )]
    [Description( "This skill provides functionality to manage prayer." )]
    internal sealed class PrayerSkill : AgentSkillComponent
    {
        #region Fields

        private readonly ILogger<PrayerSkill> _logger;
        private readonly IRockContextFactory _rockContextFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PrayerSkill"/> class.
        /// </summary>
        /// <param name="rockContextFactory">Factory to create rock contexts.</param>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        public PrayerSkill( IRockContextFactory rockContextFactory, ILogger<PrayerSkill> logger )
        {
            _rockContextFactory = rockContextFactory ?? throw new ArgumentNullException( nameof( rockContextFactory ) );
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Configuration

        /// <summary>
        /// The configuration keys that are used to configure this skill.
        /// </summary>
        private static class ConfigurationKey
        {
            /// <summary>
            /// The parent category to use by default when listing or adding prayer requests.
            /// </summary>
            public const string ParentCategory = "parentCategory";
        }

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Controls/Internal/AI/Skills/prayerSkill.obs" ),
                Options = new Dictionary<string, string>
                {
                    ["prayerCategories"] = GetPrayerCategoryOptions( rockContext )?.ToCamelCaseJson( false, false )
                },
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfiguration( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            var publicConfiguration = new Dictionary<string, string>();

            if ( privateConfiguration.TryGetValue( ConfigurationKey.ParentCategory, out var prayerCategoriesString ) )
            {
                publicConfiguration[ConfigurationKey.ParentCategory] = prayerCategoriesString;
            }

            return publicConfiguration;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfiguration( Dictionary<string, string> publicConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            var privateConfiguration = new Dictionary<string, string>();

            if ( publicConfiguration.TryGetValue( ConfigurationKey.ParentCategory, out var groupTypesString ) )
            {
                privateConfiguration[ConfigurationKey.ParentCategory] = groupTypesString;
            }

            return privateConfiguration;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the queryable version of the prayer categories.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        private IQueryable<Rock.Model.Category> GetPrayerCategoriesQueryable( RockContext rockContext )
        {
            var prayerRequestEntityType = EntityTypeCache.Get<PrayerRequest>( false );

            if ( prayerRequestEntityType == null )
            {
                return null;
            }

            var categoryService = new CategoryService( rockContext );
            return categoryService.GetByEntityTypeId( prayerRequestEntityType.Id );
        }

        /// <summary>
        /// Gets list item bags representing the prayer categories.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        private List<ListItemBag> GetPrayerCategoryOptions( RockContext rockContext )
        {
            return GetPrayerCategoriesQueryable( rockContext )?.ToListItemBagList();
        }

        #endregion

        #region Agent Tools

        [AgentToolGuid( "4E4A5AC6-85DC-4773-A03D-9BC1722366FD" )]
        public RockToolResult LookupPrayerCategories()
        {
            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var queryable = GetPrayerCategoriesQueryable( rockContext );
                if ( queryable == null )
                {
                    return RockToolResult.Error( "PrayerRequest entity type is not available." );
                }

                var prayerCategories = queryable
                    .Select( pc => new CategoryResult
                    {
                        Id = pc.Id,
                        Description = pc.Description,
                        Name = pc.Name,
                    } )
                    .ToList();

                // Lose the description for history content.
                var trimmedCategories = prayerCategories.Select( pc => new KeyNameResult
                {
                    Id = pc.Id,
                    Name = pc.Name,
                } ).ToList();

                return RockToolResult.Success( prayerCategories )
                    .WithHistoryContent( trimmedCategories, "prayer-categories" );
            }
        }

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

        [AgentToolGuid( "3AE458AB-C06C-47BC-AD2D-86EB19E556F1" )]
        [AgentUsage( "If a personIdKey is provided, first and last name will be determined from their Person record." )]
        [AgentToolPrerequisite( "Call the LookupPrayerCategories function to determine available categories. Select one that matches the prayer request sentiment." )]
        [AgentToolPrerequisite( "Call the SearchPerson function to first determine if there is an idKey you can use instead of first/last name." )]
        public RockToolResult AddPrayerRequest(
            string requestText,
            string categoryIdKey,

            [Description( "The IdKey of the person needing prayer. If provided without a first or last name, first and last name will be determined from their Person record." )]
            string personIdKey = null,
            string firstName = null,
            string lastName = null,
            bool isPublic = false,
            bool isUrgent = false )
        {
            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                // We need either a first + last name or a requested by person id.
                if ( ( firstName.IsNullOrWhiteSpace() || lastName.IsNullOrWhiteSpace() ) && personIdKey.IsNullOrWhiteSpace() )
                {
                    return RockToolResult.Error( "You must provide either a first and last name, or a personIdKey." );
                }

                var prayerRequestService = new PrayerRequestService( rockContext );
                var categoryId = IdHasher.Instance.GetId( categoryIdKey );

                // We need to give this prayer request a category. If one wasn't provided,
                // get a list of available categories. Return instructions to the LLM to call the LookupPrayerCategories
                // and have the user pick one (with recommendation).
                if ( categoryId == null )
                {
                    return RockToolResult.Error( "Invalid prayer category provided." )
                        .WithInstructions( "Call the LookupPrayerCategories function to determine available categories. Select one that matches the prayer request sentiment." );
                }

                // Validate that the provided category id is valid, and a prayer category.
                var category = CategoryCache.Get( categoryId.Value );
                var prayerRequestEntityType = EntityTypeCache.Get<PrayerRequest>( false );
                if ( category == null || category.EntityTypeId != prayerRequestEntityType.Id )
                {
                    return RockToolResult.Error( "Invalid prayer category provided." );
                }

                // If we have a personIdKey, use that to lookup the person and get their name.
                int? requestedByPersonAliasId = null;
                var email = string.Empty;
                if ( personIdKey.IsNotNullOrWhiteSpace() )
                {
                    var personService = new PersonService( rockContext );
                    var personId = IdHasher.Instance.GetId( personIdKey );
                    if ( personId == null )
                    {
                        return RockToolResult.Error( "The personIdKey is not valid." );
                    }

                    var person = personService.Get( personId.Value );
                    if ( person == null )
                    {
                        return RockToolResult.Error( "The personIdKey is not valid." );
                    }

                    if ( firstName.IsNullOrWhiteSpace() )
                    {
                        firstName = person.NickName;
                    }

                    if ( lastName.IsNullOrWhiteSpace() )
                    {
                        lastName = person.LastName;
                    }

                    lastName = person.LastName;
                    requestedByPersonAliasId = person.PrimaryAliasId;
                    email = person.Email;
                }

                var newPrayerRequest = new PrayerRequest
                {
                    Id = 0,
                    FirstName = firstName,
                    LastName = lastName,
                    RequestedByPersonAliasId = requestedByPersonAliasId,
                    CategoryId = category.Id,
                    Text = requestText,
                    IsActive = true,
                    IsPublic = isPublic,
                    IsUrgent = isUrgent,
                    Email = email,
                };

                var isInternal = AgentRequestContext.AudienceType == Enums.AI.Agent.AudienceType.Internal;

                // If this is an internal request, we will auto-approve it. If it's external, it will
                // need to be approved by a moderator.
                newPrayerRequest.IsApproved = isInternal;

                var currentPerson = AgentRequestContext.RockRequestContext.CurrentPerson;
                if ( currentPerson != null )
                {
                    newPrayerRequest.RequestedByPersonAliasId = currentPerson.PrimaryAliasId;
                }

                prayerRequestService.Add( newPrayerRequest );

                try
                {
                    rockContext.SaveChanges();
                }
                catch ( Exception ex )
                {
                    _logger.LogError( ex, "An error occurred while saving a new prayer request." );
                    return RockToolResult.Error( "An error occurred while saving the prayer request." );
                }

                var instructions = ( newPrayerRequest.IsApproved ?? false )
                    ? "The prayer request has been added and approved. Display the text, category, and note if the prayer request was marked as urgent or public."
                    : "The prayer request has been added and is pending approval. Display the text, category, and note if the prayer request was marked as urgent or public.";

                return RockToolResult.Success( new PrayerRequestResult
                {
                    Id = newPrayerRequest.Id,
                    Text = newPrayerRequest.Text,
                    Category = new KeyNameResult
                    {
                        Id = category.Id,
                        Name = category.Name,
                    },
                    IsUrgent = newPrayerRequest.IsUrgent,
                    IsApproved = newPrayerRequest.IsApproved,
                    IsPublic = newPrayerRequest.IsPublic,
                } )
                .WithHistoryContent( newPrayerRequest.IdKey, newPrayerRequest.IdKey )
                .WithInstructions( instructions );
            }
        }

        [AgentToolGuid( "6A2F2659-DEA5-4BA0-9BE7-2329FF231776" )]
        public RockToolResult UpdatePrayerRequest(
            string prayerRequestIdKey,
            string personIdKey = null,
            string firstName = null,
            string lastName = null,
            string prayerRequest = null,
            string categoryIdKey = null,
            bool? isPublic = null,
            bool? isUrgent = null,

            [Description("Description of how God has answered the prayer request.")]
            string answer = "" )
        {
            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var prayerRequestService = new PrayerRequestService( rockContext );
                var existingPrayerRequest = prayerRequestService.Get( prayerRequestIdKey, false );

                if ( existingPrayerRequest == null )
                {
                    return RockToolResult.Error( "Invalid prayer request provided." );
                }

                if ( prayerRequest.IsNotNullOrWhiteSpace() )
                {
                    existingPrayerRequest.Text = prayerRequest;
                }

                if ( isPublic != null )
                {
                    existingPrayerRequest.IsPublic = isPublic;
                }

                if ( isUrgent != null )
                {
                    existingPrayerRequest.IsUrgent = isUrgent;
                }

                if ( answer.IsNotNullOrWhiteSpace() )
                {
                    existingPrayerRequest.Answer = answer;
                }

                if ( categoryIdKey.IsNotNullOrWhiteSpace() )
                {
                    var categoryId = IdHasher.Instance.GetId( categoryIdKey );
                    if ( categoryId == null )
                    {
                        return RockToolResult.Error( "Invalid prayer category provided." );
                    }
                    var category = CategoryCache.Get( categoryId.Value );
                    var prayerRequestEntityType = EntityTypeCache.Get<PrayerRequest>( false );
                    if ( category == null || category.EntityTypeId != prayerRequestEntityType.Id )
                    {
                        return RockToolResult.Error( "Invalid prayer category provided." );
                    }

                    existingPrayerRequest.CategoryId = category.Id;
                }

                if ( personIdKey.IsNotNullOrWhiteSpace() )
                {
                    var personService = new PersonService( rockContext );
                    var personId = IdHasher.Instance.GetId( personIdKey );
                    if ( personId == null )
                    {
                        return RockToolResult.Error( "The personIdKey is not valid." );
                    }
                    var person = personService.Get( personId.Value );
                    if ( person == null )
                    {
                        return RockToolResult.Error( "The personIdKey is not valid." );
                    }

                    existingPrayerRequest.FirstName = person.NickName;
                    existingPrayerRequest.LastName = person.LastName;
                    existingPrayerRequest.RequestedByPersonAliasId = person.PrimaryAliasId;
                }

                if ( firstName.IsNotNullOrWhiteSpace() )
                {
                    existingPrayerRequest.FirstName = firstName;
                }

                if ( lastName.IsNotNullOrWhiteSpace() )
                {
                    existingPrayerRequest.LastName = lastName;
                }

                try
                {
                    rockContext.SaveChanges();
                }
                catch ( Exception ex )
                {
                    _logger.LogError( ex, "An error occurred while saving a prayer request." );
                    return RockToolResult.Error( "An error occurred while saving the prayer request." );
                }

                return RockToolResult.Success( new PrayerRequestResult
                {
                    Id = existingPrayerRequest.Id,
                    Text = existingPrayerRequest.Text,
                    Category = new KeyNameResult
                    {
                        Id = existingPrayerRequest.Category.Id,
                        Name = existingPrayerRequest.Category.Name,
                    }
                } )
                .WithHistoryContent( existingPrayerRequest.IdKey, existingPrayerRequest.IdKey );
            }
        }

        /// <summary>
        /// Deletes the prayer request with the provided idKey.
        /// </summary>
        /// <param name="idKey"></param>
        /// <returns></returns>
        [AgentToolGuid( "423AFDB5-1095-4D55-8631-4F284FC0AFED" )]
        [AgentGuardrail( "This action will permanently delete the specified prayer request. Ensure that this action is intentional and that you have the correct prayer request identifier before proceeding." )]
        public RockToolResult DeletePrayerRequest( string idKey )
        {
            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var prayerRequestService = new PrayerRequestService( rockContext );
                var existingPrayerRequest = prayerRequestService.Get( idKey, false );
                if ( existingPrayerRequest == null )
                {
                    return RockToolResult.Error( "Invalid prayer request provided." );
                }
                prayerRequestService.Delete( existingPrayerRequest );
                try
                {
                    rockContext.SaveChanges();
                }
                catch ( Exception ex )
                {
                    _logger.LogError( ex, "An error occurred while deleting a prayer request." );
                    return RockToolResult.Error( "An error occurred while deleting the prayer request." );
                }

                return RockToolResult.Success()
                    .WithHistoryContent( existingPrayerRequest.IdKey, existingPrayerRequest.IdKey )
                    .WithInstructions( "The prayer request has been deleted." );
            }
        }

        #endregion
    }
}
