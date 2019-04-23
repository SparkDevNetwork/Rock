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
using System.Data.Entity;
using System.Linq;
using System.Reflection;

using Quartz;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Follow;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Calculates, saves, and notifies followers of all the active following suggestions
    /// </summary>
    [SystemEmailField( "Following Suggestion Notification Email Template", required: true, order: 0, key: "EmailTemplate" )]
    [SecurityRoleField( "Eligible Followers", "The group that contains individuals who should receive following suggestions", true, order: 1 )]
    [DisallowConcurrentExecution]
    public class SendFollowingSuggestions : IJob
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

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            var exceptionMsgs = new List<string>();

            JobDataMap dataMap = context.JobDetail.JobDataMap;
            Guid? groupGuid = dataMap.GetString( "EligibleFollowers" ).AsGuidOrNull();
            Guid? systemEmailGuid = dataMap.GetString( "EmailTemplate" ).AsGuidOrNull();
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
                            .ForEach( a => primaryAliasIds.AddOrIgnore( a.PersonId, a.Id ) );

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

                                    // Get the entitytype for this suggestion type
                                    var suggestionEntityType = EntityTypeCache.Get( suggestionComponent.FollowedType );
                                    if ( suggestionEntityType != null )
                                    {
                                        var entityIds = new List<int>();

                                        // Call the components method to return all of it's suggestions
                                        var personEntitySuggestions = suggestionComponent.GetSuggestions( suggestionType, followerPersonIds );

                                        // If any suggestions were returned by the component
                                        if ( personEntitySuggestions.Any() )
                                        {
                                            int entityTypeId = suggestionEntityType.Id;
                                            string reasonNote = suggestionType.ReasonNote;

                                            // Get the existing followings for any of the followers 
                                            var existingFollowings = new Dictionary<int, List<int>>();
                                            foreach( var following in followingService.Queryable( "PersonAlias" ).AsNoTracking()
                                                .Where( f =>
                                                    f.EntityTypeId == entityTypeId &&
                                                    followerPersonIds.Contains( f.PersonAlias.PersonId ) ) )
                                            {
                                                existingFollowings.AddOrIgnore( following.PersonAlias.PersonId, new List<int>() );
                                                existingFollowings[ following.PersonAlias.PersonId].Add( following.EntityId );
                                            }

                                            // Loop through each follower
                                            foreach ( var followerPersonId in personEntitySuggestions
                                                .Select( s => s.PersonId )
                                                .Distinct() )
                                            {

                                                using ( var suggestionContext = new RockContext() )
                                                {
                                                    var followingSuggestedService = new FollowingSuggestedService( suggestionContext );

                                                    // Read all the existing suggestions for this type and the returned followers
                                                    var existingSuggestions = followingSuggestedService
                                                        .Queryable( "PersonAlias" )
                                                        .Where( s =>
                                                            s.SuggestionTypeId == suggestionType.Id &&
                                                            s.PersonAlias.PersonId == followerPersonId )
                                                        .ToList();

                                                    // Look  through the returned suggestions
                                                    foreach ( var followedEntityId in personEntitySuggestions
                                                        .Where( s => s.PersonId == followerPersonId )
                                                        .Select( s => s.EntityId ) )
                                                    {
                                                        // Make sure person isn't already following this entity
                                                        if ( !existingFollowings.ContainsKey( followerPersonId )
                                                            || !existingFollowings[followerPersonId].Contains( followedEntityId ) )
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

                                                                // If not found, add one
                                                                if ( suggestion == null )
                                                                {
                                                                    suggestion = new FollowingSuggested();
                                                                    suggestion.EntityTypeId = entityTypeId;
                                                                    suggestion.EntityId = followedEntityId;
                                                                    suggestion.PersonAliasId = primaryAliasIds[followerPersonId];
                                                                    suggestion.SuggestionTypeId = suggestionType.Id;
                                                                    suggestion.Status = FollowingSuggestedStatus.PendingNotification;
                                                                    suggestion.StatusChangedDateTime = timestamp;
                                                                    followingSuggestedService.Add( suggestion );
                                                                }
                                                                else
                                                                {
                                                                    // If found, and it has not been ignored, and it's time to promote again, update the promote date
                                                                    if ( suggestion.Status != FollowingSuggestedStatus.Ignored &&
                                                                        suggestionType.ReminderDays.HasValue &&
                                                                        (
                                                                            !suggestion.LastPromotedDateTime.HasValue ||
                                                                            suggestion.LastPromotedDateTime.Value.AddDays( suggestionType.ReminderDays.Value ) <= timestamp
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

                        var appRoot = GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot", rockContext );

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
                                    emailMessage.AddRecipient( new RecipientData( person.Email, mergeFields ) );
                                    var errors = new List<string>();
                                    emailMessage.Send(out errors);
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

            context.Result = string.Format( "A total of {0} following suggestions sent to {1} people", followingSuggestionsSuggestionsTotal, followingSuggestionsEmailsSent );

            if ( exceptionMsgs.Any() )
            {
                throw new Exception( "One or more exceptions occurred calculating suggestions..." + Environment.NewLine + exceptionMsgs.AsDelimited( Environment.NewLine ) );
            }

        }
    }

    /// <summary>
    /// Helper class for following suggestion notifications
    /// </summary>
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
