// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Text;
using System.Threading.Tasks;
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
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            Guid? groupGuid = dataMap.GetString( "EligibleFollowers" ).AsGuidOrNull();
            Guid? systemEmailGuid = dataMap.GetString( "EmailTemplate" ).AsGuidOrNull();

            if ( groupGuid.HasValue && systemEmailGuid.HasValue )
            {
                var rockContext = new RockContext();

                var followingService = new FollowingService( rockContext );
                var followingSuggestedService = new FollowingSuggestedService( rockContext );

                // The people who are eligible to get following suggestions based on the group type setting for this job
                var eligiblePersonIds = new GroupMemberService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( m =>
                        m.Group != null &&
                        m.Group.Guid.Equals( groupGuid.Value ) &&
                        m.GroupMemberStatus == GroupMemberStatus.Active &&
                        m.Person != null &&
                        m.Person.Email != null &&
                        m.Person.Email != "" )
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

                    var exceptionMsgs = new List<string>();

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
                                var suggestionEntityType = EntityTypeCache.Read( suggestionComponent.FollowedType );
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

                                        // Get a list of the suggested followers
                                        var suggestedFollowerPersonIds = personEntitySuggestions
                                            .Select( s => s.PersonId )
                                            .Distinct()
                                            .ToList();

                                        // Read all the existing suggestions for this type and the returned followers
                                        var existingSuggestions = followingSuggestedService
                                            .Queryable( "PersonAlias" )
                                            .Where( s =>
                                                s.SuggestionTypeId == suggestionType.Id &&
                                                suggestedFollowerPersonIds.Contains( s.PersonAlias.PersonId ) )
                                            .ToList();

                                        // Get the existing followings for any of the follower personids returned by suggestion type
                                        var existingFollowings = followingService
                                            .Queryable( "PersonAlias" )
                                            .Where( f =>
                                                f.EntityTypeId == entityTypeId &&
                                                suggestedFollowerPersonIds.Contains( f.PersonAlias.PersonId ) )
                                            .ToList();

                                        // Look  through the returned suggestions
                                        foreach ( var personEntitySuggestion in personEntitySuggestions )
                                        {
                                            // Make sure person isn't already following this entity
                                            if ( !existingFollowings
                                                .Any( f =>
                                                    f.PersonAlias.PersonId == personEntitySuggestion.PersonId &&
                                                    f.EntityId == personEntitySuggestion.EntityId ) )
                                            {
                                                // If this person had a primary alias id
                                                if ( primaryAliasIds.ContainsKey( personEntitySuggestion.PersonId ) )
                                                {
                                                    entityIds.Add( personEntitySuggestion.EntityId );

                                                    // Look for existing suggestion for this person and entity
                                                    var suggestion = existingSuggestions
                                                        .Where( s =>
                                                            s.PersonAlias.PersonId == personEntitySuggestion.PersonId &&
                                                            s.EntityId == personEntitySuggestion.EntityId )
                                                        .OrderByDescending( s => s.StatusChangedDateTime )
                                                        .FirstOrDefault();

                                                    // If not found, add one
                                                    if ( suggestion == null )
                                                    {
                                                        suggestion = new FollowingSuggested();
                                                        suggestion.EntityTypeId = entityTypeId;
                                                        suggestion.EntityId = personEntitySuggestion.EntityId;
                                                        suggestion.PersonAliasId = primaryAliasIds[personEntitySuggestion.PersonId];
                                                        suggestion.SuggestionTypeId = suggestionType.Id;
                                                        suggestion.Status = FollowingSuggestedStatus.PendingNotification;
                                                        suggestion.StatusChangedDateTime = timestamp;
                                                        followingSuggestedService.Add( suggestion );
                                                    }
                                                    else
                                                    {
                                                        // If found, and it has not been ignored, and it's time to promote again, update the promote date
                                                        if ( suggestion.Status != FollowingSuggestedStatus.Ignored &&
                                                            (
                                                                !suggestionType.ReminderDays.HasValue ||
                                                                !suggestion.LastPromotedDateTime.HasValue ||
                                                                suggestion.LastPromotedDateTime.Value.AddDays( suggestionType.ReminderDays.Value ) <= timestamp
                                                            ) )
                                                        {
                                                            suggestion.StatusChangedDateTime = timestamp;
                                                            suggestion.Status = FollowingSuggestedStatus.PendingNotification;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        // Save the suggestions for this type
                                        rockContext.SaveChanges();

                                    }

                                    // If any entities are being suggested for this type, query database for thema and save to dictionary
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
                                                var entityList = entityQry.Where( q => entityIds.Contains( q.Id ) ).ToList();
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

                    var allSuggestions = followingSuggestedService
                        .Queryable( "PersonAlias" )
                        .Where( s => s.Status == FollowingSuggestedStatus.PendingNotification )
                        .ToList();

                    var suggestionPersonIds = allSuggestions
                        .Where( s => eligiblePersonIds.Contains( s.PersonAlias.PersonId ) )
                        .Select( s => s.PersonAlias.PersonId )
                        .Distinct()
                        .ToList();

                    var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );

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
                                    var notices = new List<string>();

                                    foreach ( var personSuggestion in allSuggestions
                                        .Where( s =>
                                            s.PersonAlias.PersonId == person.Id &&
                                            s.SuggestionTypeId == suggestionType.Id )
                                        .ToList() )
                                    {
                                        if ( suggestedEntities[suggestionType.Id].ContainsKey( personSuggestion.EntityId ) )
                                        {
                                            personSuggestion.LastPromotedDateTime = timestamp;
                                            personSuggestion.Status = FollowingSuggestedStatus.Suggested;
                                            notices.Add( component.FormatEntityNotification( suggestionType, suggestedEntities[suggestionType.Id][personSuggestion.EntityId] ) );
                                        }
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
                                var recipients = new List<RecipientData>();
                                var mergeFields = new Dictionary<string, object>();
                                mergeFields.Add( "Person", person );
                                mergeFields.Add( "Suggestions", personSuggestionNotices );
                                recipients.Add( new RecipientData( person.Email, mergeFields ) );
                                Email.Send( systemEmailGuid.Value, recipients, appRoot );
                            }

                            rockContext.SaveChanges();
                        }
                        catch ( Exception ex )
                        {
                            exceptionMsgs.Add( string.Format( "An exception occurred sending suggestions to '{0}':{1}    {2}", person.FullName, Environment.NewLine, ex.Messages().AsDelimited( Environment.NewLine + "   " ) ) );
                            ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
                        }
                    }

                    if ( exceptionMsgs.Any() )
                    {
                        throw new Exception( "One or more exceptions occurred calculating suggestions..." + Environment.NewLine + exceptionMsgs.AsDelimited( Environment.NewLine ) );
                    }
                }
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
