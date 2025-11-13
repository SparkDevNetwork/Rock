using System;
using System.ComponentModel;

using Microsoft.Extensions.Logging;

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
        [Description( "Updates a prayer request." )]
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
            using var rockContext = _rockContextFactory.CreateRockContext();
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

        #endregion
    }
}
