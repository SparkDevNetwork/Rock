using System;
using System.ComponentModel;

using Microsoft.Extensions.Logging;

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

        #endregion
    }
}
