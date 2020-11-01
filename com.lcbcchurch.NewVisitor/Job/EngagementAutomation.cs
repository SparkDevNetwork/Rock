using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using com.lcbcchurch.NewVisitor.Settings;
using com.lcbcchurch.NewVisitor.SystemKey;
using Newtonsoft.Json;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;

namespace com.lcbcchurch.NewVisitor.Job
{
    /// <summary>
    /// Job to update engagement scoring based on the Engagement Automation settings.
    /// Engagement Automation tasks are tasks that update the engagement related attribute of person.
    /// </summary>

    #region Attributes

    [InteractionChannelField(
        "Interaction Channel",
        Description = "The Interaction channel to use.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.InteractionChannel )]
    [BooleanField(
        "Enable Updating Family Campus",
        Description = "When enabled, the family's campus will be updated if any adults in the family have attendance at the campus.",
        DefaultBooleanValue = true,
        Order = 1,
        Key = AttributeKey.EnableUpdatingFamilyCampus
        )]
    [DisallowConcurrentExecution]

    #endregion Attributes
    public class EngagementAutomation : IJob
    {
        private const string USER_PREFERENCE_KEY_ENGAGEMENT_SCORE = "lcbc_EngagementScoreResults";
        private const string SOURCE_OF_CHANGE = "Engagement Automation";
        private HttpContext _httpContext = null;

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string InteractionChannel = "InteractionChannel";
            public const string EnableUpdatingFamilyCampus = "EnableUpdatingFamilyCampus";
        }

        #endregion Attribute Keys

        #region Constructor

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public EngagementAutomation()
        {
        }

        #endregion Constructor

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Execute( IJobExecutionContext context )
        {
            _httpContext = HttpContext.Current;
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            string engagementScoringResult = CalculateEngagementScore( context );
            var message = new StringBuilder();
            message.AppendLine( $@"Engagement Scoring: {engagementScoringResult}" );

            if ( dataMap.GetString( AttributeKey.EnableUpdatingFamilyCampus ).AsBoolean() )
            {
                string updateFamilyResult = UpdateFamilyCampus( context );
                message.AppendLine( $@"Update Family Campus: {updateFamilyResult}" );
            }

            context.UpdateLastStatusMessage( message.ToString() );
        }

        #region Helper Methods

        /// <summary>
        /// Updates the family campus.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Could not determine the 'Family' group type.</exception>
        private string UpdateFamilyCampus( IJobExecutionContext context )
        {
            try
            {
                context.UpdateLastStatusMessage( $"Processing campus updates." );

                var setting = Rock.Web.SystemSettings.GetValue( SystemSetting.LCBC_ENGAGEMENTSCORING_CONFIGURATION ).FromJsonOrNull<EngagementAutomationSetting>();

                var isValid = setting.BeginDateAttributeGuid.HasValue && setting.ScoreAttributeGuid.HasValue && setting.ScoringItems.Any();
                if ( !isValid )
                {
                    throw new Exception( "Engagement Scoring Settings are not correctly configured." );
                }

                var rockContext = new RockContext();

                var periodInDays = setting.WeeksInEngagementWindow * 7;
                var personIds = GetEligiblePeople( setting.BeginDateAttributeGuid.Value, periodInDays, rockContext );

                if ( !personIds.Any() )
                {
                    return $"No person found eligible for engagement scoring;";
                }

                var persons = new PersonService( rockContext ).GetByIds( personIds );
                var changeFamilyCampusList = new Dictionary<int, int>();

                var groupTypeFamily = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
                var adultRoleId = groupTypeFamily.Roles.First( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;

                foreach ( var person in persons )
                {
                    var family = person.GetFamily( rockContext );
                    if ( family == null )
                    {
                        continue;
                    }

                    var familyAdultPersonIds = family.Members.Where( a => a.GroupRoleId == adultRoleId ).Select( a => a.PersonId ).ToList();
                    var attendanceCampus = new AttendanceService( rockContext )
                                    .Queryable().AsNoTracking()
                                    .Where( a =>
                                        a.DidAttend == true &&
                                        a.CampusId.HasValue &&
                                        familyAdultPersonIds.Contains( a.PersonAlias.PersonId ) )
                                    .OrderByDescending( a => a.StartDateTime )
                                    .FirstOrDefault();


                    if ( attendanceCampus != null && attendanceCampus.CampusId != family.CampusId )
                    {
                        changeFamilyCampusList.AddOrReplace( family.Id, attendanceCampus.CampusId.Value );
                    }
                }

                // Counters for displaying results
                int recordsProcessed = 0;
                int recordsUpdated = 0;
                int totalRecords = changeFamilyCampusList.Count();

                // Loop through each family
                foreach ( var familyId in changeFamilyCampusList.Keys )
                {
                    try
                    {
                        // Update the status on every 100th record
                        if ( recordsProcessed % 100 == 0 )
                        {
                            context.UpdateLastStatusMessage( $"Processing campus updates: {recordsProcessed:N0} of {totalRecords:N0} families processed; campus has been updated for {recordsUpdated:N0} of them." );
                        }

                        recordsProcessed++;

                        // Using a new rockcontext for each one (to improve performance)
                        var updateRockContext = new RockContext();
                        updateRockContext.SourceOfChange = SOURCE_OF_CHANGE;

                        // Get the family
                        var groupService = new GroupService( updateRockContext );
                        var family = groupService.Get( familyId );
                        // Update the campus
                        family.CampusId = changeFamilyCampusList[familyId];
                        updateRockContext.SaveChanges();

                        // Since we just successfully saved the change, increment the update counter
                        recordsUpdated++;
                    }
                    catch ( Exception ex )
                    {
                        // Log exception and keep on trucking.
                        ExceptionLogService.LogException( new Exception( $"Exception occurred trying to update campus for GroupId:{familyId}.", ex ), _httpContext );
                    }
                }

                // Format the result message
                return $"{recordsProcessed:N0} families were processed; campus was updated for {recordsUpdated:N0} of them.";
            }
            catch ( Exception ex )
            {
                // Log exception and return the exception messages.
                ExceptionLogService.LogException( ex, _httpContext );
                return ex.Messages().AsDelimited( "; " );
            }
        }

        /// <summary>
        /// calculate the engagement score.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private string CalculateEngagementScore( IJobExecutionContext context )
        {
            try
            {
                context.UpdateLastStatusMessage( $"Processing engagement scoring." );

                var setting = Rock.Web.SystemSettings.GetValue( SystemSetting.LCBC_ENGAGEMENTSCORING_CONFIGURATION ).FromJsonOrNull<EngagementAutomationSetting>();

                var isValid = setting.BeginDateAttributeGuid.HasValue && setting.ScoreAttributeGuid.HasValue && setting.ScoringItems.Any();
                if ( !isValid )
                {
                    throw new Exception( "Engagement Scoring Settings are not correctly configured." );
                }

                var rockContext = new RockContext();

                var periodInDays = setting.WeeksInEngagementWindow * 7;
                var personIds = GetEligiblePeople( setting.BeginDateAttributeGuid.Value, periodInDays, rockContext );

                if ( !personIds.Any() )
                {
                    return $"No person found eligible for engagement scoring;";
                }

                // Loop through each person
                JobDataMap dataMap = context.JobDetail.JobDataMap;
                Guid interactionChannelGuid = dataMap.GetString( "InteractionChannel" ).AsGuid();
                var interactionChannel = InteractionChannelCache.Get( interactionChannelGuid );
                var scoringItemDetails = new List<ScoringItemDetail>();
                foreach ( var item in setting.ScoringItems )
                {
                    var interactionComponent = GetComponent( interactionChannel, item );
                    List<int> entityIds = new List<int>();
                    int entityTypeId = default( int );

                    switch ( item.Type )
                    {
                        case ScoringItemType.GivenToAnAccount:
                            {
                                entityTypeId = EntityTypeCache.Get<FinancialAccount>().Id;
                                entityIds = new FinancialAccountService( rockContext ).GetListByGuids( item.EntityItemsGuid ).Select( a => a.Id ).ToList();
                            }
                            break;
                        case ScoringItemType.AttendanceInGroupOfType:
                        case ScoringItemType.AttendanceInGroupOfTypeCumulative:
                        case ScoringItemType.MemberOfGroupType:
                            {
                                entityTypeId = EntityTypeCache.Get<GroupType>().Id;
                                entityIds = new List<int>() { GroupTypeCache.Get( item.EntityItemsGuid.FirstOrDefault() ).Id };
                            }
                            break;
                        case ScoringItemType.InDataView:
                            {
                                entityTypeId = EntityTypeCache.Get<DataView>().Id;
                                entityIds = new DataViewService( rockContext ).GetListByGuids( item.EntityItemsGuid ).Select( a => a.Id ).ToList();
                            }
                            break;
                        case ScoringItemType.PersonAttribute:
                        case ScoringItemType.MemberOfGroupWithGroupTypeHavingAnAttribute:
                        default:
                            {
                                entityTypeId = EntityTypeCache.Get<Rock.Model.Attribute>().Id;
                                entityIds = new List<int>() { AttributeCache.Get( item.EntityItemsGuid.FirstOrDefault() ).Id };
                            }
                            break;
                    }
                    var scoringItem = new ScoringItemDetail()
                    {
                        ComponentId = interactionComponent.Id,
                        Guid = item.Guid,
                        EntityTypeId = entityTypeId,
                        EntityIds = entityIds,
                        Name = item.Name
                    };
                    scoringItemDetails.Add( scoringItem );
                }

                var scoringGuids = scoringItemDetails.Select( a => a.Guid ).ToList();
                // Get the distinct person ids
                personIds = personIds.Distinct().ToList();
                var persons = new PersonService( rockContext ).GetByIds( personIds );
                persons.LoadAttributes();
                List<EngagementScoreResult> engagementScoreResults = new List<EngagementScoreResult>();
                foreach ( var person in persons )
                {
                    var engagementScoreResult = new EngagementScoreResult()
                    {
                        PersonId = person.Id
                    };
                    var userPreference = PersonService.GetUserPreference( person, USER_PREFERENCE_KEY_ENGAGEMENT_SCORE );
                    if ( string.IsNullOrWhiteSpace( userPreference ) )
                    {
                        engagementScoreResult.ItemScores = new List<ItemScore>();
                    }
                    else
                    {
                        engagementScoreResult.ItemScores = JsonConvert.DeserializeObject<List<ItemScore>>( userPreference )
                                                         .Where( a => scoringGuids.Contains( a.ScoringItemId ) )
                                                         .ToList();
                    }
                    engagementScoreResults.Add( engagementScoreResult );
                }

                GetScoreBasedonType( setting, rockContext, personIds, engagementScoreResults );

                // Counter for displaying results
                int recordsProcessed = 0;
                int recordsUpdated = 0;

                var attributeCache = AttributeCache.Get( setting.ScoreAttributeGuid.Value );
                foreach ( var person in persons )
                {
                    recordsProcessed++;
                    try
                    {
                        var updateRockContext = new RockContext();
                        var interactionService = new InteractionService( updateRockContext );
                        updateRockContext.SourceOfChange = SOURCE_OF_CHANGE;
                        var engagementScoreResult = engagementScoreResults.Single( a => a.PersonId == person.Id );
                        var totalScore = engagementScoreResult.ItemScores.Select( a => a.Score ).DefaultIfEmpty( 0 ).Sum();
                        person.SetAttributeValue( attributeCache.Key, totalScore );
                        person.SaveAttributeValues( updateRockContext );
                        PersonService.SaveUserPreference( person, USER_PREFERENCE_KEY_ENGAGEMENT_SCORE, engagementScoreResult.ItemScores.ToJson() );
                        foreach ( var item in engagementScoreResult.ItemScores.Where( a => a.Score > 0 && a.IsChanged ) )
                        {
                            var scoringItemDetail = scoringItemDetails.First( a => a.Guid == item.ScoringItemId );
                            var interaction = interactionService
                                .AddInteraction( scoringItemDetail.ComponentId, null, "Scored", item.Score.ToString(), person.PrimaryAliasId, item.Date.Value, null, null, null, null, null, null );
                            interaction.InteractionSummary = scoringItemDetail.Name;
                            interaction.RelatedEntityId = scoringItemDetail.EntityIds.FirstOrDefault();
                            interaction.RelatedEntityTypeId = scoringItemDetail.EntityTypeId;
                        }
                        updateRockContext.SaveChanges();
                        recordsUpdated++;
                    }
                    catch ( Exception ex )
                    {
                        // Log exception and keep on trucking.
                        ExceptionLogService.LogException( new Exception( $"Exception occurred trying to update engagement score PersonId:{person.Id}.", ex ), _httpContext );
                    }
                }
                // Format the result message
                return $"{recordsProcessed:N0} people were processed; {recordsUpdated:N0} people were updated;";
            }
            catch ( Exception ex )
            {
                // Log exception and return the exception messages.
                ExceptionLogService.LogException( ex, _httpContext );
                return ex.Messages().AsDelimited( "; " );
            }
        }

        /// <summary>
        /// Update the score for each scoring items
        /// </summary>
        /// <param name="setting">The engagement automation setting.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="targetPersonIds">The target person identifiers.</param>
        /// <param name="engagementScoreResults">The engagement score results.</param>
        /// <returns></returns>
        private void GetScoreBasedonType( EngagementAutomationSetting setting, RockContext rockContext, List<int> targetPersonIds, List<EngagementScoreResult> engagementScoreResults )
        {
            var periodInDays = setting.WeeksInEngagementWindow * 7;
            foreach ( var scoringItem in setting.ScoringItems )
            {
                bool attendanceInGroupOfTypeCumulative = scoringItem.Type == ScoringItemType.AttendanceInGroupOfTypeCumulative;

                var nonScorePersonIds = engagementScoreResults
                                            .Where( a => attendanceInGroupOfTypeCumulative || !a.ItemScores.Any( b => b.ScoringItemId == scoringItem.Guid && b.Score == scoringItem.Score ) )
                                            .Select( a => a.PersonId )
                                            .ToList();

                switch ( scoringItem.Type )
                {
                    case ScoringItemType.AttendanceInGroupOfType:
                        {
                            var groupType = GroupTypeCache.Get( scoringItem.EntityItemsGuid.FirstOrDefault() );
                            var attendedPersonIds = GetPeopleWhoAttendedGroupType( groupType.Id, periodInDays, nonScorePersonIds, rockContext );
                            UpdatePersonsScore( engagementScoreResults, scoringItem, nonScorePersonIds, attendedPersonIds );
                        }
                        break;
                    case ScoringItemType.AttendanceInGroupOfTypeCumulative:
                        {
                            var groupType = GroupTypeCache.Get( scoringItem.EntityItemsGuid.FirstOrDefault() );
                            var personIdsWithAttendance = GetPeopleTotalAttendanceForGroupType( groupType.Id, periodInDays, targetPersonIds, rockContext );
                            foreach ( var personId in targetPersonIds )
                            {
                                var engagementScoreResult = engagementScoreResults.Single( a => a.PersonId == personId );
                                var itemScore = engagementScoreResult.ItemScores.FirstOrDefault( a => a.ScoringItemId == scoringItem.Guid );
                                if ( itemScore == null )
                                {
                                    itemScore = new ItemScore()
                                    {
                                        ScoringItemId = scoringItem.Guid
                                    };
                                    engagementScoreResult.ItemScores.Add( itemScore );
                                }

                                var score = default( int );
                                if ( personIdsWithAttendance.ContainsKey( personId ) )
                                {
                                    score = scoringItem.Score * personIdsWithAttendance[personId].Count;
                                }
                                if ( personIdsWithAttendance.ContainsKey( personId ) && score != itemScore.Score )
                                {
                                    itemScore.Score = score;
                                    itemScore.Date = personIdsWithAttendance[personId].OrderBy( a => a ).FirstOrDefault();
                                    itemScore.IsChanged = true;
                                }
                            }
                        }
                        break;
                    case ScoringItemType.MemberOfGroupType:
                        {
                            var groupType = GroupTypeCache.Get( scoringItem.EntityItemsGuid.FirstOrDefault() );
                            var activePersonIds = GetPeopleWhoAreActiveMemberOfGroupType( groupType.Id, periodInDays, nonScorePersonIds, rockContext );
                            UpdatePersonsScore( engagementScoreResults, scoringItem, nonScorePersonIds, activePersonIds );
                        }
                        break;
                    case ScoringItemType.PersonAttribute:
                        {
                            var attribute = AttributeCache.Get( scoringItem.EntityItemsGuid.FirstOrDefault() );
                            var activePersonIds = GetPeopleWithNewPersonAttributeValue( attribute.Id, periodInDays, nonScorePersonIds, rockContext );
                            UpdatePersonsScore( engagementScoreResults, scoringItem, nonScorePersonIds, activePersonIds );
                        }
                        break;
                    case ScoringItemType.MemberOfGroupWithGroupTypeHavingAnAttribute:
                        {
                            var attribute = AttributeCache.Get( scoringItem.EntityItemsGuid.FirstOrDefault() );
                            var activePersonIds = GetPeopleWithGroupTypeAttributeValue( attribute, scoringItem.EntityItemQualifierValue, periodInDays, nonScorePersonIds, rockContext );
                            UpdatePersonsScore( engagementScoreResults, scoringItem, nonScorePersonIds, activePersonIds );
                        }
                        break;
                    case ScoringItemType.GivenToAnAccount:
                        {
                            var contributionPersonIds = GetPeopleWhoContributed( scoringItem.EntityItemsGuid, periodInDays, nonScorePersonIds, rockContext );
                            UpdatePersonsScore( engagementScoreResults, scoringItem, nonScorePersonIds, contributionPersonIds );
                        }
                        break;
                    case ScoringItemType.InDataView:
                        {
                            var inDataViewPersonIds = GetPeopleWhoAreInDataView( scoringItem.EntityItemsGuid.FirstOrDefault(), periodInDays, nonScorePersonIds, rockContext );
                            UpdatePersonsScore( engagementScoreResults, scoringItem, nonScorePersonIds, inDataViewPersonIds.ToDictionary( a => a, b => RockDateTime.Now ) );
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Update the persons score
        /// </summary>
        /// <param name="engagementScoreResults">The engagement scoring result.</param>
        /// <param name="scoringItem">The scoring item.</param>
        /// <param name="targetPersonIds">The target person identifiers.</param>
        /// <param name="achieverPersonIds">The achiever person identifiers.</param>
        /// <returns></returns>
        private static void UpdatePersonsScore( List<EngagementScoreResult> engagementScoreResults, ScoringItem scoringItem, List<int> targetPersonIds, Dictionary<int, DateTime> achieverPersonIds )
        {
            foreach ( var personId in targetPersonIds )
            {
                var engagementScoreResult = engagementScoreResults.Single( a => a.PersonId == personId );
                var itemScore = engagementScoreResult.ItemScores.FirstOrDefault( a => a.ScoringItemId == scoringItem.Guid );
                if ( itemScore == null )
                {
                    itemScore = new ItemScore()
                    {
                        ScoringItemId = scoringItem.Guid
                    };
                    engagementScoreResult.ItemScores.Add( itemScore );
                }

                if ( achieverPersonIds.ContainsKey( personId ) )
                {
                    itemScore.Score = scoringItem.Score;
                    itemScore.Date = achieverPersonIds[personId];
                    itemScore.IsChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets the people who are eligible for scoring.
        /// </summary>
        /// <param name="engagementBeginDateAttributeGuid">The engagement end attribute guid.</param>
        /// <param name="periodInDays">The period in days.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<int> GetEligiblePeople( Guid engagementBeginDateAttributeGuid, int periodInDays, RockContext rockContext )
        {
            var personEntityTypeId = EntityTypeCache.Get( typeof( Person ) ).Id;
            var currentDate = RockDateTime.Now.Date;
            var beginDate = currentDate.AddDays( -periodInDays );
            var tomorrow = currentDate.AddDays( 1 );
            var attribute = AttributeCache.Get( engagementBeginDateAttributeGuid );
            var qry = new AttributeValueService( rockContext )
                .Queryable().AsNoTracking()
                .Where( a =>
                    a.AttributeId == attribute.Id &&
                    a.ValueAsDateTime.HasValue &&
                    a.ValueAsDateTime.Value >= beginDate &&
                    a.ValueAsDateTime.Value < tomorrow &&
                    a.Attribute.EntityTypeId == personEntityTypeId &&
                    a.EntityId.HasValue );

            return qry
                .Select( a => a.EntityId.Value )
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Gets the type of the people who attended group.
        /// </summary>
        /// <param name="groupTypeId">The group type id.</param>
        /// <param name="personIds">The person ids.</param>
        /// <param name="periodInDays">The period in days.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Dictionary<int, DateTime> GetPeopleWhoAttendedGroupType( int groupTypeId, int periodInDays, List<int> personIds, RockContext rockContext )
        {
            var currentDate = RockDateTime.Now.Date;
            var tomorrowDate = currentDate.AddDays( 1 );
            var startDate = currentDate.AddDays( -periodInDays );

            var qry = new AttendanceService( rockContext )
                .Queryable().AsNoTracking()
                .Where( a =>
                    a.Occurrence.Group != null &&
                    a.Occurrence.Group.GroupTypeId == groupTypeId &&
                    a.StartDateTime >= startDate &&
                    a.StartDateTime < tomorrowDate &&
                    a.DidAttend.HasValue &&
                    a.DidAttend.Value == true &&
                    a.PersonAlias != null &&
                    personIds.Contains( a.PersonAlias.PersonId ) );

            return qry
                .Select( a => new { a.PersonAlias.PersonId, a.StartDateTime } )
                .ToList()
                .GroupBy( a => a.PersonId )
                .ToDictionary( a => a.Key, b => b.Select( a => a.StartDateTime ).OrderBy( a => a ).FirstOrDefault() );
        }

        /// <summary>
        /// Gets the type of the people who are active member of group.
        /// </summary>
        /// <param name="groupTypeId">The group type id.</param>
        /// <param name="periodInDays">The period in days.</param>
        /// <param name="personIds">The person ids.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Dictionary<int, DateTime> GetPeopleWhoAreActiveMemberOfGroupType( int groupTypeId, int periodInDays, List<int> personIds, RockContext rockContext )
        {
            var currentDate = RockDateTime.Now.Date;
            var tomorrowDate = currentDate.AddDays( 1 );
            var startDate = currentDate.AddDays( -periodInDays );

            var qry = new GroupMemberService( rockContext )
                .Queryable().AsNoTracking()
                .Where( a =>
                    a.Group.GroupTypeId == groupTypeId &&
                    a.CreatedDateTime >= startDate &&
                    a.CreatedDateTime < tomorrowDate &&
                    a.GroupMemberStatus == GroupMemberStatus.Active &&
                    personIds.Contains( a.PersonId ) );

            return qry
                .Select( a => new { a.PersonId, a.CreatedDateTime.Value } )
                .ToList()
                .GroupBy( a => a.PersonId )
                .ToDictionary( a => a.Key, b => b.Select( a => a.Value ).OrderBy( a => a ).FirstOrDefault() );
        }

        /// <summary>
        /// Gets the people total attendance who attended group with grouptypeId.
        /// </summary>
        /// <param name="groupTypeId">The group type id.</param>
        /// <param name="periodInDays">The period in days.</param>
        /// <param name="personIds">The person ids.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Dictionary<int, List<DateTime>> GetPeopleTotalAttendanceForGroupType( int groupTypeId, int periodInDays, List<int> personIds, RockContext rockContext )
        {
            var currentDate = RockDateTime.Now.Date;
            var tomorrowDate = currentDate.AddDays( 1 );
            var startDate = currentDate.AddDays( -periodInDays );

            var qry = new AttendanceService( rockContext )
                .Queryable().AsNoTracking()
                .Where( a =>
                    a.Occurrence.Group != null &&
                    a.Occurrence.Group.GroupTypeId == groupTypeId &&
                    a.StartDateTime >= startDate &&
                    a.StartDateTime < tomorrowDate &&
                    a.DidAttend.HasValue &&
                    a.DidAttend.Value == true &&
                    a.PersonAlias != null &&
                    personIds.Contains( a.PersonAlias.PersonId ) );

            return qry
                .GroupBy( a => a.PersonAlias.PersonId )
                .ToDictionary( a => a.Key, b => b.Select( a => a.StartDateTime ).ToList() );
        }

        /// <summary>
        /// Gets the people with new person attribute value updates.
        /// </summary>
        /// <param name="attribute">The attribute id.</param>
        /// <param name="periodInDays">The period in days.</param>
        /// <param name="personIds">The person ids.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Dictionary<int, DateTime> GetPeopleWithGroupTypeAttributeValue( AttributeCache attribute, string entityQualifierValue, int periodInDays, List<int> personIds, RockContext rockContext )
        {
            var currentDate = RockDateTime.Now.Date;
            var tomorrowDate = currentDate.AddDays( 1 );
            var startDate = currentDate.AddDays( -periodInDays );

            var groupTypeEntityTypeId = EntityTypeCache.Get( typeof( GroupType ) ).Id;
            var groupTypeService = new GroupTypeService( rockContext );
            var qry = groupTypeService.Queryable().AsNoTracking();

            List<int> groupTypeIds = new List<int>();
            var entityField = EntityHelper.GetEntityFieldForAttribute( attribute, false );
            if ( entityField != null )
            {
                var values = JsonConvert.DeserializeObject<List<string>>( entityQualifierValue );
                var parameterExpression = groupTypeService.ParameterExpression;
                var attributeExpression = Rock.Utility.ExpressionHelper.GetAttributeExpression( groupTypeService, parameterExpression, entityField, values );
                qry = qry.Where( parameterExpression, attributeExpression );
                groupTypeIds = qry.Select( a => a.Id ).ToList();
            }

            var personQry = new GroupMemberService( rockContext )
               .Queryable().AsNoTracking()
               .Where( a =>
                   groupTypeIds.Contains( a.Group.GroupTypeId ) &&
                   a.CreatedDateTime >= startDate &&
                   a.CreatedDateTime < tomorrowDate &&
                   a.GroupMemberStatus == GroupMemberStatus.Active );

            return personQry
                .Select( a => new { a.PersonId, a.CreatedDateTime.Value } )
                .ToList()
                .Where( a => personIds.Contains( a.PersonId ) )
                .GroupBy( a => a.PersonId )
                .ToDictionary( a => a.Key, b => b.Select( a => a.Value ).OrderBy( a => a ).FirstOrDefault() );
        }


        /// <summary>
        /// Gets the people with new person attribute value updates.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        /// <param name="periodInDays">The period in days.</param>
        /// <param name="personIds">The person ids.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Dictionary<int, DateTime> GetPeopleWithNewPersonAttributeValue( int attributeId, int periodInDays, List<int> personIds, RockContext rockContext )
        {
            var currentDate = RockDateTime.Now.Date;
            var tomorrowDate = currentDate.AddDays( 1 );
            var startDate = currentDate.AddDays( -periodInDays );

            var personEntityTypeId = EntityTypeCache.Get( typeof( Person ) ).Id;

            var qry = new AttributeValueService( rockContext )
                .Queryable().AsNoTracking()
                .Where( a =>
                    a.CreatedDateTime.HasValue &&
                    a.CreatedDateTime.Value >= startDate &&
                    a.CreatedDateTime.Value < tomorrowDate &&
                    a.Value != null &&
                    a.Value != string.Empty &&
                    a.Attribute.EntityTypeId == personEntityTypeId &&
                    a.AttributeId == attributeId &&
                    a.EntityId.HasValue &&
                    personIds.Contains( a.EntityId.Value ) );

            return qry
                .Select( a => new { PersonId = a.EntityId.Value, CreatedDateTime = a.CreatedDateTime.Value } )
                .ToList()
                .GroupBy( a => a.PersonId )
                .ToDictionary( a => a.Key, b => b.Select( a => a.CreatedDateTime ).OrderBy( a => a ).FirstOrDefault() );
        }

        /// <summary>
        /// Gets the people who are in dataview.
        /// </summary>
        /// <param name="accountsGuid">The account guid.</param>
        /// <param name="periodInDays">The period in days.</param>
        /// <param name="personIds">The person ids.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<int> GetPeopleWhoAreInDataView( Guid dataViewGuid, int periodInDays, List<int> personIds, RockContext rockContext )
        {
            var currentDate = RockDateTime.Now.Date;
            var tomorrowDate = currentDate.AddDays( 1 );
            var startDate = currentDate.AddDays( -periodInDays );

            var dataView = new DataViewService( rockContext ).Get( dataViewGuid );
            if ( dataView != null )
            {
                var errors = new List<string>();
                return dataView.GetQuery( null, 30, out errors )
                    .Where( a => personIds.Contains( a.Id ) )
                    .Select( a => a.Id )
                    .ToList();
            }
            else
            {
                return new List<int>();
            }
        }

        /// <summary>
        /// Gets the people with who contributed.
        /// </summary>
        /// <param name="accountsGuid">The account guid.</param>
        /// <param name="periodInDays">The period in days.</param>
        /// <param name="personIds">The person ids.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Dictionary<int, DateTime> GetPeopleWhoContributed( List<Guid> accountsGuid, int periodInDays, List<int> personIds, RockContext rockContext )
        {
            var currentDate = RockDateTime.Now.Date;
            var tomorrowDate = currentDate.AddDays( 1 );
            var startDate = currentDate.AddDays( -periodInDays );

            var accounts = new FinancialAccountService( rockContext ).GetListByGuids( accountsGuid );
            List<int> accountIds = new List<int>();
            foreach ( var item in accounts )
            {
                accountIds.Add( item.Id );
                var childAccountIds = item.ChildAccounts.Select( a => a.Id ).ToList();
                if ( childAccountIds.Any() )
                {
                    accountIds.AddRange( childAccountIds );
                }
            }

            int transactionTypeContributionId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ).Id;

            var personGroupIdPair = new PersonService( rockContext ).Queryable()
                                 .Where( p => personIds.Contains( p.Id ) )
                                 .ToDictionary( a => a.Id, b => b.GivingGroupId );


            var financialTransactionQry = new FinancialTransactionDetailService( rockContext ).Queryable()
                                    .Where( a => a.Transaction.TransactionTypeValueId == transactionTypeContributionId &&
                                    a.Transaction.AuthorizedPersonAliasId.HasValue &&
                                    a.Transaction.TransactionDateTime >= startDate &&
                                    a.Transaction.TransactionDateTime < tomorrowDate &&
                                    a.Amount > 0 &&
                                    accountIds.Contains( a.AccountId ) );

            //non giving group persons
            var nonGivingPersonIds = personGroupIdPair.Where( a => !a.Value.HasValue ).Select( a => a.Key ).ToList();
            var nonGivingPersonIdsWithDate = financialTransactionQry
                                    .Where( a => nonGivingPersonIds.Contains( a.Transaction.AuthorizedPersonAlias.PersonId ) )
                                    .Select( a => new { a.Transaction.AuthorizedPersonAlias.PersonId, TransactionDateTime = a.Transaction.TransactionDateTime.Value } )
                                    .ToList()
                                    .GroupBy( a => a.PersonId )
                                    .ToDictionary( a => a.Key, b => b.Select( a => a.TransactionDateTime ).OrderBy( a => a ).FirstOrDefault() );


            var nonGivingGroupIds = personGroupIdPair.Where( a => a.Value.HasValue ).Select( a => a.Value ).ToList();
            var nonGivingGroupIdsWithDate = financialTransactionQry
                                    .Where( a => nonGivingGroupIds.Contains( a.Transaction.AuthorizedPersonAlias.Person.GivingGroupId ) )
                                    .Select( a => new { GivingGroupId = a.Transaction.AuthorizedPersonAlias.Person.GivingGroupId.Value, TransactionDateTime = a.Transaction.TransactionDateTime.Value } )
                                    .ToList()
                                    .GroupBy( a => a.GivingGroupId )
                                    .ToDictionary( a => a.Key, b => b.Select( a => a.TransactionDateTime ).OrderBy( a => a ).FirstOrDefault() );

            foreach ( var key in nonGivingGroupIdsWithDate.Keys )
            {
                var personId = personGroupIdPair.Where( a => a.Value == key ).Select( a => a.Key ).First();
                nonGivingPersonIdsWithDate.AddOrReplace( personId, nonGivingGroupIdsWithDate[key] );
            }

            return nonGivingPersonIdsWithDate;
        }

        /// <summary>
        /// Gets the component.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="scoringItem">The scoringItem.</param>
        /// <returns></returns>
        private InteractionComponentCache GetComponent( InteractionChannelCache channel, ScoringItem scoringItem )
        {
            var interactionComponentIds = new InteractionComponentService( new RockContext() )
                .GetByChannelId( channel.Id )
                .Select( v => v.Id )
                .ToList();

            var components = new List<InteractionComponentCache>();

            foreach ( var id in interactionComponentIds )
            {
                var componentCache = InteractionComponentCache.Get( id );
                if ( componentCache != null )
                {
                    components.Add( componentCache );
                }
            }

            // Find by Name
            var component = components.FirstOrDefault( c => c.Name.Equals( scoringItem.Name, StringComparison.OrdinalIgnoreCase ) );
            if ( component != null )
            {
                return component;
            }

            // If still no match, and we have a name, create a new channel
            using ( var newRockContext = new RockContext() )
            {
                var interactionComponent = new InteractionComponent();
                interactionComponent.Name = scoringItem.Name;
                interactionComponent.ComponentData = scoringItem.IconCssClass;
                interactionComponent.InteractionChannelId = channel.Id;
                new InteractionComponentService( newRockContext ).Add( interactionComponent );
                newRockContext.SaveChanges();

                return InteractionComponentCache.Get( interactionComponent.Id );
            }
        }
        #endregion Helper Methods

        #region Helper Classes

        /// <summary>
        /// Helper class for engagement scoring result
        /// </summary>
        public class EngagementScoreResult
        {
            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the item scores.
            /// </summary>
            /// <value>
            /// The item scores.
            /// </value>
            public List<ItemScore> ItemScores { get; set; }
        }

        /// <summary>
        /// Item Score
        /// </summary>
        public class ItemScore
        {
            /// <summary>
            /// Gets or sets the scoring item identifier.
            /// </summary>
            /// <value>
            /// The scoring item identifier.
            /// </value>
            public Guid ScoringItemId { get; set; }

            /// <summary>
            /// Gets or sets the date.
            /// </summary>
            /// <value>
            /// The date.
            /// </value>
            public DateTime? Date { get; set; }

            /// <summary>
            /// Gets or sets the score.
            /// </summary>
            /// <value>
            /// The score.
            /// </value>
            public int Score { get; set; }

            /// <summary>
            /// Gets or sets the IsChanged.
            /// </summary>
            /// <value>
            /// The IsChanged.
            /// </value>
            [JsonIgnore]
            public bool IsChanged { get; set; }
        }

        /// <summary>
        /// Helper class to temporarily keep all the scoring item detail
        /// </summary>
        public class ScoringItemDetail
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the component identifier.
            /// </summary>
            /// <value>
            /// The component identifier.
            /// </value>
            public int ComponentId { get; set; }

            /// <summary>
            /// Gets or sets the entity type identifier.
            /// </summary>
            /// <value>
            /// The entity type identifier.
            /// </value>
            public int EntityTypeId { get; set; }

            /// <summary>
            /// Gets or sets the entity identifier.
            /// </summary>
            /// <value>
            /// The entity identifier.
            /// </value>
            public List<int> EntityIds { get; set; }
        }

        #endregion
    }
}