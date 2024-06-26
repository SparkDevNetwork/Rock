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
using System.Reflection;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Follow;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Calculates, saves, and notifies followers of all the active following suggestions
    /// </summary>
    [DisplayName( "Send Following Suggestion Notification" )]
    [Description( "Calculates and sends any following suggestions to those people that are eligible for following." )]

    [SystemCommunicationField( "Following Suggestion Notification Email Template", required: true, order: 0, key: "EmailTemplate" )]
    [SecurityRoleField( "Eligible Followers", "The group that contains individuals who should receive following suggestions", true, order: 1 )]
    public class SendFollowingSuggestions : RockJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SendFollowingSuggestions()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            var exceptionMsgs = new List<string>();

            Guid? groupGuid = GetAttributeValue( "EligibleFollowers" ).AsGuidOrNull();
            Guid? systemEmailGuid = GetAttributeValue( "EmailTemplate" ).AsGuidOrNull();
            int followingSuggestionsEmailsSent = 0;
            int followingSuggestionsSuggestionsTotal = 0;

            if ( groupGuid.HasValue && systemEmailGuid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var followingService = new FollowingService( rockContext );

                    // The people who are eligible to get following suggestions based on the group type setting for this job
                    var eligiblePersonIds = new GroupMemberService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            m.Group != null &&
                            m.Group.Guid.Equals( groupGuid.Value ) &&
                            m.GroupMemberStatus == GroupMemberStatus.Active &&
                            m.Person != null &&
                            m.Person.Email != null &&
                            m.Person.Email != string.Empty &&
                            m.Person.EmailPreference != EmailPreference.DoNotEmail &&
                            m.Person.IsEmailActive )
                        .Select( m => m.PersonId )
                        .Distinct();

                    // check to see if there are any event types that require notification
                    var followerPersonIds = new List<int>();
                    if ( new FollowingEventTypeService( rockContext )
                        .Queryable().AsNoTracking()
                        .Any( e => e.IsNoticeRequired ) )
                    {
                        // if so, include all eligible people
                        followerPersonIds = eligiblePersonIds.ToList();
                    }
                    else
                    {
                        // if not, filter the list of eligible people down to only those that actually have subscribed to one or more following events
                        followerPersonIds = new FollowingEventSubscriptionService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( f => eligiblePersonIds.Contains( f.PersonAlias.PersonId ) )
                            .Select( f => f.PersonAlias.PersonId )
                            .Distinct()
                            .ToList();
                    }

                    if ( followerPersonIds.Any() )
                    {
                        // Get the primary person alias id for each of the followers
                        var primaryAliasIds = new Dictionary<int, int>();
                        new PersonAliasService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( a =>
                                followerPersonIds.Contains( a.PersonId ) &&
                                a.PersonId == a.AliasPersonId )
                            .ToList()
                            .ForEach( a => primaryAliasIds.TryAdd( a.PersonId, a.Id ) );

                        // Get current date/time. 
                        var timestamp = RockDateTime.Now;

                        var suggestionTypes = new FollowingSuggestionTypeService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( s => s.IsActive )
                            .OrderBy( s => s.Name )
                            .ToList();

                        var components = new Dictionary<int, SuggestionComponent>();
                        var suggestedEntities = new Dictionary<int, Dictionary<int, IEntity>>();

                        foreach ( var suggestionType in suggestionTypes )
                        {
                            try
                            {
                                // Get the suggestion type component
                                var suggestionComponent = suggestionType.GetSuggestionComponent();
                                if ( suggestionComponent != null )
                                {
                                    components.Add( suggestionType.Id, suggestionComponent );

                                    var suggestionTypeComponent = new SuggestionTypeComponent( suggestionType );

                                    // Get the entitytype for this suggestion type
                                    var suggestionEntityType = EntityTypeCache.Get( suggestionComponent.FollowedType );
                                    if ( suggestionEntityType != null )
                                    {
                                        var entityIds = new List<int>();

                                        // Call the components method to return all of it's suggestions
                                        suggestionTypeComponent.PersonEntitySuggestions = suggestionComponent.GetSuggestions( suggestionType, followerPersonIds );

                                        // If any suggestions were returned by the component
                                        if ( suggestionTypeComponent.PersonEntitySuggestions.Any() )
                                        {
                                            int entityTypeId = suggestionEntityType.Id;
                                            string reasonNote = suggestionType.ReasonNote;

                                            // Get the existing followings for any of the followers 
                                            suggestionTypeComponent.ExistingFollowings = new Dictionary<int, List<int>>();
                                            foreach ( var following in followingService.Queryable( "PersonAlias" ).AsNoTracking().Where( f => f.EntityTypeId == entityTypeId && string.IsNullOrEmpty( f.PurposeKey ) && followerPersonIds.Contains( f.PersonAlias.PersonId ) ) )
                                            {
                                                suggestionTypeComponent.ExistingFollowings.TryAdd( following.PersonAlias.PersonId, new List<int>() );
                                                suggestionTypeComponent.ExistingFollowings[following.PersonAlias.PersonId].Add( following.EntityId );
                                            }

                                            // Loop through each follower
                                            foreach ( var followerPersonId in suggestionTypeComponent.PersonEntitySuggestions.Select( s => s.PersonId ).Distinct() )
                                            {
                                                ProcessFollowerPersonId( followerPersonId, suggestionTypeComponent, primaryAliasIds, entityIds, entityTypeId, timestamp );
                                            }
                                        }

                                        // If any entities are being suggested for this type, query database for them and save to dictionary
                                        if ( entityIds.Any() )
                                        {
                                            if ( suggestionEntityType.AssemblyName != null )
                                            {
                                                // get the actual type of what is being followed 
                                                Type entityType = suggestionEntityType.GetEntityType();
                                                if ( entityType != null )
                                                {
                                                    // Get generic queryable method and query all the entities that are being followed
                                                    Type[] modelType = { entityType };
                                                    Type genericServiceType = typeof( Rock.Data.Service<> );
                                                    Type modelServiceType = genericServiceType.MakeGenericType( modelType );
                                                    Rock.Data.IService serviceInstance = Activator.CreateInstance( modelServiceType, new object[] { rockContext } ) as IService;
                                                    MethodInfo qryMethod = serviceInstance.GetType().GetMethod( "Queryable", new Type[] { } );
                                                    var entityQry = qryMethod.Invoke( serviceInstance, new object[] { } ) as IQueryable<IEntity>;
                                                    var entityList = entityQry.AsNoTracking().Where( q => entityIds.Contains( q.Id ) ).ToList();
                                                    if ( entityList != null && entityList.Any() )
                                                    {
                                                        var entities = new Dictionary<int, IEntity>();
                                                        entityList.ForEach( e => entities.Add( e.Id, e ) );
                                                        suggestedEntities.Add( suggestionType.Id, entities );
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch ( Exception ex )
                            {
                                exceptionMsgs.Add( string.Format( "An exception occurred calculating suggestions for the '{0}' suggestion type:{1}    {2}", suggestionType.Name, Environment.NewLine, ex.Messages().AsDelimited( Environment.NewLine + "   " ) ) );
                                ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
                            }
                        }

                        var allSuggestions = new FollowingSuggestedService( rockContext )
                            .Queryable( "PersonAlias" )
                            .Where( s => s.Status == FollowingSuggestedStatus.PendingNotification )
                            .ToList();

                        var suggestionPersonIds = allSuggestions
                            .Where( s => followerPersonIds.Contains( s.PersonAlias.PersonId ) )
                            .Select( s => s.PersonAlias.PersonId )
                            .Distinct()
                            .ToList();

                        foreach ( var person in new PersonService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( p => suggestionPersonIds.Contains( p.Id ) )
                            .ToList() )
                        {
                            try
                            {
                                var personSuggestionNotices = new List<FollowingSuggestionNotices>();

                                foreach ( var suggestionType in suggestionTypes )
                                {
                                    var component = components.ContainsKey( suggestionType.Id ) ? components[suggestionType.Id] : null;
                                    if ( component != null && suggestedEntities.ContainsKey( suggestionType.Id ) )
                                    {
                                        var entities = new List<IEntity>();
                                        foreach ( var suggestion in allSuggestions
                                            .Where( s =>
                                                s.PersonAlias.PersonId == person.Id &&
                                                s.SuggestionTypeId == suggestionType.Id )
                                            .ToList() )
                                        {
                                            if ( suggestedEntities[suggestionType.Id].ContainsKey( suggestion.EntityId ) )
                                            {
                                                entities.Add( suggestedEntities[suggestionType.Id][suggestion.EntityId] );
                                                suggestion.LastPromotedDateTime = timestamp;
                                                suggestion.Status = FollowingSuggestedStatus.Suggested;
                                            }
                                        }

                                        var notices = new List<string>();
                                        foreach ( var entity in component.SortEntities( entities ) )
                                        {
                                            notices.Add( component.FormatEntityNotification( suggestionType, entity ) );
                                        }

                                        if ( notices.Any() )
                                        {
                                            personSuggestionNotices.Add( new FollowingSuggestionNotices( suggestionType, notices ) );
                                        }
                                    }
                                }

                                if ( personSuggestionNotices.Any() )
                                {
                                    // Send the notice
                                    var mergeFields = new Dictionary<string, object>();
                                    mergeFields.Add( "Person", person );
                                    mergeFields.Add( "Suggestions", personSuggestionNotices.OrderBy( s => s.SuggestionType.Order ).ToList() );

                                    var emailMessage = new RockEmailMessage( systemEmailGuid.Value );
                                    emailMessage.AddRecipient( new RockEmailMessageRecipient( person, mergeFields ) );
                                    var errors = new List<string>();
                                    emailMessage.Send( out errors );
                                    exceptionMsgs.AddRange( errors );

                                    followingSuggestionsEmailsSent += 1;
                                    followingSuggestionsSuggestionsTotal += personSuggestionNotices.Count();
                                }

                                rockContext.SaveChanges();
                            }
                            catch ( Exception ex )
                            {
                                exceptionMsgs.Add( string.Format( "An exception occurred sending suggestions to '{0}':{1}    {2}", person.FullName, Environment.NewLine, ex.Messages().AsDelimited( Environment.NewLine + "   " ) ) );
                                ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
                            }
                        }
                    }
                }
            }

            this.Result = string.Format( "A total of {0} following suggestions sent to {1} people", followingSuggestionsSuggestionsTotal, followingSuggestionsEmailsSent );

            if ( exceptionMsgs.Any() )
            {
                throw new Exception( "One or more exceptions occurred calculating suggestions..." + Environment.NewLine + exceptionMsgs.AsDelimited( Environment.NewLine ) );
            }
        }

        /// <summary>
        /// Processes the follower PersonId for the SuggestionType
        /// </summary>
        /// <param name="followerPersonId">The follower person identifier.</param>
        /// <param name="suggestionTypeComponent">The suggestion type component.</param>
        /// <param name="primaryAliasIds">The primary alias ids.</param>
        /// <param name="entityIds">The entity ids.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="timestamp">The timestamp.</param>
        private void ProcessFollowerPersonId( int followerPersonId, SuggestionTypeComponent suggestionTypeComponent, Dictionary<int, int> primaryAliasIds, List<int> entityIds, int entityTypeId, DateTime timestamp )
        {
            using ( var suggestionContext = new RockContext() )
            {
                var followingSuggestedService = new FollowingSuggestedService( suggestionContext );

                // Read all the existing suggestions for this type and the returned followers
                var existingSuggestions = followingSuggestedService
                    .Queryable( "PersonAlias" )
                    .Where( s => s.SuggestionTypeId == suggestionTypeComponent.FollowingSuggestionType.Id
                        && s.PersonAlias.PersonId == followerPersonId )
                    .ToList();

                // Look  through the returned suggestions
                foreach ( var followedEntityId in suggestionTypeComponent.PersonEntitySuggestions.Where( s => s.PersonId == followerPersonId ).Select( s => s.EntityId ) )
                {
                    // Make sure person isn't already following this entity
                    if ( !suggestionTypeComponent.ExistingFollowings.ContainsKey( followerPersonId )
                        || !suggestionTypeComponent.ExistingFollowings[followerPersonId].Contains( followedEntityId ) )
                    {
                        // If this person had a primary alias id
                        if ( primaryAliasIds.ContainsKey( followerPersonId ) )
                        {
                            entityIds.Add( followedEntityId );

                            // Look for existing suggestion for this person and entity
                            var suggestion = existingSuggestions
                                .Where( s => s.EntityId == followedEntityId )
                                .OrderByDescending( s => s.StatusChangedDateTime )
                                .FirstOrDefault();

                            // If not found add it if needed
                            if ( suggestion == null )
                            {
                                bool addSuggestion;
                                ProcessFollowingSuggestionAndPersonAliasEntity( followerPersonId, suggestionTypeComponent, entityIds, entityTypeId, suggestionContext, followedEntityId, out addSuggestion );

                                if ( addSuggestion )
                                {
                                    // This is a new entity ID so insert it
                                    suggestion = new FollowingSuggested
                                    {
                                        EntityTypeId = entityTypeId,
                                        EntityId = followedEntityId,
                                        PersonAliasId = primaryAliasIds[followerPersonId],
                                        SuggestionTypeId = suggestionTypeComponent.FollowingSuggestionType.Id,
                                        Status = FollowingSuggestedStatus.PendingNotification,
                                        StatusChangedDateTime = timestamp
                                    };

                                    followingSuggestedService.Add( suggestion );
                                }
                            }
                            else
                            {
                                ProcessFollowingSuggestionAndPersonAliasEntity( followerPersonId, suggestionTypeComponent, entityIds, entityTypeId, suggestionContext, followedEntityId );

                                // If found, and it has not been ignored, and it's time to promote again, update the promote date
                                if ( suggestion.Status != FollowingSuggestedStatus.Ignored &&
                                    suggestionTypeComponent.FollowingSuggestionType.ReminderDays.HasValue &&
                                    (
                                        !suggestion.LastPromotedDateTime.HasValue ||
                                        suggestion.LastPromotedDateTime.Value.AddDays( suggestionTypeComponent.FollowingSuggestionType.ReminderDays.Value ) <= timestamp
                                    ) )
                                {
                                    if ( suggestion.Status != FollowingSuggestedStatus.PendingNotification )
                                    {
                                        suggestion.StatusChangedDateTime = timestamp;
                                        suggestion.Status = FollowingSuggestedStatus.PendingNotification;
                                    }
                                }
                            }
                        }
                    }
                }

                // Save the suggestions for this type
                suggestionContext.SaveChanges();
            }
        }

        /// <summary>
        /// Checks to see if the PersonAlias entity should be added to the FollowingSuggestion table. If the entity is not a PersonAlias no action is taken.
        /// If the Entity is a PersonAlias then this method will check if the associated Person has another PersonAlias that is being followed.
        /// If there is another PersonAlias being followed then the Following and FollowingSuggestion are updated with this EntityId (PersonAliasId)
        /// which is the PrimaryPersonAliasId.
        /// </summary>
        /// <param name="followerPersonId">The follower person identifier.</param>
        /// <param name="suggestionTypeComponent">The suggestion type component.</param>
        /// <param name="entityIdToBeSavedAsSuggestions">The entity identifier to be saved as suggestions.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="suggestionContext">The suggestion context.</param>
        /// <param name="followedEntityId">The followed entity identifier.</param>
        private void ProcessFollowingSuggestionAndPersonAliasEntity( int followerPersonId, SuggestionTypeComponent suggestionTypeComponent, List<int> entityIdToBeSavedAsSuggestions, int entityTypeId, RockContext suggestionContext, int followedEntityId )
        {
            bool addSuggestion;
            ProcessFollowingSuggestionAndPersonAliasEntity( followerPersonId, suggestionTypeComponent, entityIdToBeSavedAsSuggestions, entityTypeId, suggestionContext, followedEntityId, out addSuggestion );
        }

        /// <summary>
        /// Checks to see if the PersonAlias entity should be added to the FollowingSuggestion table. If the entity is not a PersonAlias then addSuggestion will be true.
        /// If the Entity is a PersonAlias then this method will check if the associated Person has another PersonAlias that is being followed.
        /// If there is another PersonAlias being followed then the Following and FollowingSuggestion are updated with this EntityId (PersonAliasId)
        /// which is the PrimaryPersonAliasId.
        /// </summary>
        /// <param name="followerPersonId">The follower person identifier.</param>
        /// <param name="suggestionTypeComponent">The suggestion type component.</param>
        /// <param name="entityIdToBeSavedAsSuggestions">The entity ids.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="suggestionContext">The suggestion context.</param>
        /// <param name="followedEntityId">The followed entity identifier.</param>
        /// <param name="addSuggestion">if set to <c>true</c> then the suggestion should be inserted.</param>
        /// <returns>
        /// True if the PersonAlias following suggestion should be inserted or if the EntityType is not a PersonAlias.
        /// </returns>
        private void ProcessFollowingSuggestionAndPersonAliasEntity( int followerPersonId, SuggestionTypeComponent suggestionTypeComponent, List<int> entityIdToBeSavedAsSuggestions, int entityTypeId, RockContext suggestionContext, int followedEntityId, out bool addSuggestion )
        {
            addSuggestion = true;

            // If the Entity is not a PersonAlias no other checks are needed just return with addSuggestion = true
            if ( !IsEntityTypePersonAlias( entityTypeId ) )
            {
                return;
            }

            // since this is a person alias see if a different alias is being used to follow
            var followedPersonId = new PersonAliasService( suggestionContext ).Get( followedEntityId ).PersonId;
            var followedPersonAliases = new PersonAliasService( suggestionContext )
                .Queryable()
                .Where( a => a.PersonId == followedPersonId && a.Id != followedEntityId )
                .Select( a => a.Id )
                .ToList();

            if ( !followedPersonAliases.Any() )
            {
                // There are no alternate person alias' there is nothing else to check so just return with addSuggestion = true
                return;
            }

            int existingFollowingPersonAliasId = suggestionTypeComponent.ExistingFollowings
                .GetValueOrDefault( followerPersonId, new List<int>() )
                .Where( f => followedPersonAliases.Contains( f ) )
                .FirstOrDefault();

            // Update the existing following record to use the primary PersonAlias ID and remove it from the list.
            if ( existingFollowingPersonAliasId != 0 )
            {
                var personAliasEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.PERSON_ALIAS ) ?? 0;
                addSuggestion = false;
                entityIdToBeSavedAsSuggestions.Remove( followedEntityId );

                using ( var followingContext = new RockContext() )
                {
                    var following = new FollowingService( followingContext )
                        .GetByEntityAndPerson( personAliasEntityTypeId, existingFollowingPersonAliasId, followerPersonId )
                        .FirstOrDefault();

                    following.EntityId = followedEntityId;

                    var suggested = new FollowingSuggestedService( followingContext )
                        .GetByEntityAndPerson( personAliasEntityTypeId, existingFollowingPersonAliasId, followerPersonId )
                        .FirstOrDefault();

                    if ( suggested != null )
                    {
                        // Get the Followed Person's Primary PersonAliasId from FollowingSuggestions if it exists, PersonEntitySuggestions is already filtered for EntityType
                        int existingFollowingSuggestedPrimaryPersonAliasId = suggestionTypeComponent.PersonEntitySuggestions
                            .Where( s => s.PersonId == followerPersonId && s.EntityId == followedEntityId )
                            .Select( s => s.EntityId )
                            .FirstOrDefault();

                        if ( existingFollowingSuggestedPrimaryPersonAliasId != 0 )
                        {
                            // Since the Primary PersonAlias is already in FollowingSuggestions then just delete this one as a duplicate
                            new FollowingSuggestedService( followingContext ).Delete( suggested );
                        }
                        else
                        {
                            // Update the outdated PersonAliasId to the primary PersonAliasId which is the followedEntityId
                            suggested.EntityId = followedEntityId;
                        }
                    }

                    followingContext.SaveChanges();
                }
            }
        }

        private bool IsEntityTypePersonAlias( int entityTypeId )
        {
            var personAliasEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.PERSON_ALIAS ) ?? 0;
            return entityTypeId == personAliasEntityTypeId;
        }

        private class SuggestionTypeComponent
        {
            public SuggestionTypeComponent()
            {
            }

            public SuggestionTypeComponent( FollowingSuggestionType followingSuggestionType )
            {
                FollowingSuggestionType = followingSuggestionType;
            }

            public List<PersonEntitySuggestion> PersonEntitySuggestions { get; set; }

            public FollowingSuggestionType FollowingSuggestionType { get; set; }

            public Dictionary<int, List<int>> ExistingFollowings { get; set; }
        }
    }

    /// <summary>
    /// Helper class for following suggestion notifications
    /// </summary>
    [LavaType( "SuggestionType", "Notices" )]
    [DotLiquid.LiquidType( "SuggestionType", "Notices" )]
    public class FollowingSuggestionNotices
    {
        /// <summary>
        /// Gets or sets the type of the suggestion.
        /// </summary>
        /// <value>
        /// The type of the suggestion.
        /// </value>
        public FollowingSuggestionType SuggestionType { get; set; }

        /// <summary>
        /// Gets or sets the notices.
        /// </summary>
        /// <value>
        /// The notices.
        /// </value>
        public List<string> Notices { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FollowingSuggestionNotices"/> class.
        /// </summary>
        /// <param name="suggestionType">Type of the suggestion.</param>
        /// <param name="notices">The notices.</param>
        public FollowingSuggestionNotices( FollowingSuggestionType suggestionType, List<string> notices )
        {
            SuggestionType = suggestionType;
            Notices = notices;
        }
    }
}
