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
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Utility.Settings.DataAutomation;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to update people/families based on the Data Automation settings.
    /// </summary>
    [DisallowConcurrentExecution]
    public class RunDataAutomation : IJob
    {
        #region Private Fields

        /// <summary>
        /// The reactivate people settings
        /// </summary>
        private ReactivatePeople _reactivateSettings = new ReactivatePeople();

        /// <summary>
        /// The inactivate people settings
        /// </summary>
        private InactivatePeople _inactivateSettings = new InactivatePeople();

        /// <summary>
        /// The campus settings
        /// </summary>
        private UpdateFamilyCampus _campusSettings = new UpdateFamilyCampus();

        #endregion Private Fields

        #region Constructor

        /// <summary> 
        /// Empty constructor for job initilization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public RunDataAutomation()
        {
            _reactivateSettings = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_REACTIVATE_PEOPLE ).FromJsonOrNull<ReactivatePeople>();
            _inactivateSettings = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_INACTIVATE_PEOPLE ).FromJsonOrNull<InactivatePeople>();
            _campusSettings = Rock.Web.SystemSettings.GetValue( SystemSetting.DATA_AUTOMATION_UPDATE_FAMILY_CAMPUS ).FromJsonOrNull<UpdateFamilyCampus>();
        }

        #endregion Constructor

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Execute( IJobExecutionContext context )
        {
            List<Exception> dataAutomationSettingException = new List<Exception>();
            if ( _reactivateSettings != null && _reactivateSettings.IsEnabled )
            {
                ProcessReactivateSetting();
            }

            if ( _inactivateSettings != null && _inactivateSettings.IsEnabled )
            {
                ProcessInactivateSetting();
            }

            if ( _campusSettings != null && _campusSettings.IsEnabled )
            {
                var familyGroupTypeId = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Id;

                List<Group> families = null;

                using ( RockContext rockContext = new RockContext() )
                {
                    families = new GroupMemberService( rockContext )
                        .Queryable( "Group.Members.Person" )
                        .Where( m => m.Group.GroupTypeId == familyGroupTypeId )
                        .OrderBy( m => m.GroupOrder ?? int.MaxValue )
                        .DistinctBy( a => a.GroupId )
                        .Select( m => m.Group )
                        .ToList();

                }

                if ( _campusSettings.IsIgnoreIfManualUpdateEnabled )
                {
                    var personEntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Person ) ).Id;
                    using ( RockContext rockContext = new RockContext() )
                    {
                        var startPeriod = RockDateTime.Now.AddDays( -_campusSettings.IgnoreIfManualUpdatePeriod );
                        var familiesWithManualUpdate = new HistoryService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( m => m.EntityTypeId == personEntityTypeId &&
                                m.Summary.Contains( "Modified <span class='field name'>Campus</span>" ) &&
                                m.CreatedDateTime >= startPeriod && m.RelatedEntityId.HasValue )
                            .Select( a => a.RelatedEntityId.Value )
                            .ToList();

                        families.RemoveAll( a => familiesWithManualUpdate.Contains( a.Id ) );
                    }
                }


                foreach ( var family in families )
                {
                    int? attendanceCampusId = null;
                    int? givingCampusId = null;
                    int? currentCampusId = family.CampusId;
                    var personIds = family.Members.Select( a => a.PersonId ).ToList();

                    if ( _campusSettings.IsMostFamilyGivingEnabled || _campusSettings.IsMostFamilyAttendanceEnabled )
                    {
                        using ( RockContext rockContext = new RockContext() )
                        {
                            if ( _campusSettings.IsMostFamilyAttendanceEnabled )
                            {
                                var startPeriod = RockDateTime.Now.AddDays( -_campusSettings.MostFamilyAttendancePeriod );
                                attendanceCampusId = new AttendanceService( rockContext )
                                                      .Queryable().AsNoTracking().Where( a => a.StartDateTime >= startPeriod && a.CampusId.HasValue && a.DidAttend == true && personIds.Contains( a.PersonAlias.PersonId ) )
                                                      .GroupBy( a => a.CampusId )
                                                      .OrderByDescending( a => a.Count() )
                                                      .Select( a => a.Key )
                                                      .FirstOrDefault();
                                if ( attendanceCampusId == currentCampusId )
                                {
                                    attendanceCampusId = null;
                                }

                                if ( _campusSettings.IsIgnoreCampusChangesEnabled && attendanceCampusId.HasValue && currentCampusId.HasValue )
                                {
                                    if ( _campusSettings.IgnoreCampusChanges.Where( a => a.FromCampus == currentCampusId.Value && a.ToCampus == attendanceCampusId.Value && a.BasedOn == CampusCriteria.UseAttendance ).Any() )
                                    {
                                        attendanceCampusId = null;
                                    }

                                }
                            }
                        }

                        if ( _campusSettings.IsMostFamilyGivingEnabled )
                        {
                            var startPeriod = RockDateTime.Now.AddDays( -_campusSettings.MostFamilyAttendancePeriod );
                            using ( RockContext rockContext = new RockContext() )
                            {
                                givingCampusId = new FinancialTransactionDetailService( rockContext )
                                .Queryable().AsNoTracking()
                                .Where( a => a.Transaction.TransactionDateTime >= startPeriod &&
                                    a.Transaction.TransactionDateTime.HasValue &&
                                    a.Transaction.AuthorizedPersonAliasId.HasValue &&
                                    personIds.Contains( a.Transaction.AuthorizedPersonAlias.PersonId )
                                    && a.Account.CampusId.HasValue )
                                .GroupBy( a => a.Account.CampusId )
                                .OrderByDescending( a => a.Select( b => b.Transaction ).Distinct().Count() )
                                .Select( a => a.Key )
                                .FirstOrDefault();
                            }
                            if ( givingCampusId == currentCampusId )
                            {
                                givingCampusId = null;
                            }

                            if ( _campusSettings.IsIgnoreCampusChangesEnabled && givingCampusId.HasValue && currentCampusId.HasValue )
                            {
                                if ( _campusSettings.IgnoreCampusChanges.Where( a => a.FromCampus == currentCampusId.Value && a.ToCampus == givingCampusId.Value && a.BasedOn == CampusCriteria.UseGiving ).Any() )
                                {
                                    givingCampusId = null;
                                }

                            }
                        }

                        if ( attendanceCampusId.HasValue && givingCampusId.HasValue && attendanceCampusId.Value != givingCampusId.Value )
                        {
                            switch ( _campusSettings.MostAttendanceOrGiving )
                            {
                                case CampusCriteria.UseGiving:
                                    {
                                        attendanceCampusId = null;
                                    }
                                    break;
                                case CampusCriteria.UseAttendance:
                                    {
                                        givingCampusId = null;
                                    }
                                    break;
                                case CampusCriteria.Ignore:
                                default:
                                    {
                                        attendanceCampusId = null;
                                        givingCampusId = null;
                                    }
                                    break;
                            }
                        }

                        int? updateCampusId = attendanceCampusId ?? givingCampusId;
                        if ( updateCampusId.HasValue )
                        {
                            using ( RockContext rockContext = new RockContext() )
                            {
                                var familyGroup = new GroupService( rockContext ).Get( family.Id );
                                familyGroup.CampusId = updateCampusId;

                                var changes = new List<string>();
                                string oldCampusName = currentCampusId.HasValue ? CampusCache.Read( currentCampusId.Value ).Name : string.Empty;
                                changes.Add( $"Modifed Campus from <span class='field-value'>{oldCampusName}</span> to <span class'field-value'>{CampusCache.Read( updateCampusId.Value ).Name}</span> due to data automation job." );
                                HistoryService.AddChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                                   personIds.FirstOrDefault(), changes,family.Name,typeof(Group),family.Id );

                                rockContext.SaveChanges();
                            }
                        }

                    }
                }
            }

        }

        /// <summary>
        /// Update families on reactivate settings.
        /// </summary>
        private void ProcessReactivateSetting()
        {
            var familyGroupTypeId = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Id;

            var values = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON.AsGuid() )
                            .DefinedValues
                            .Where( a => a.AttributeValues.ContainsKey( "AllowAutomatedReactivation" ) &&
                            a.AttributeValues["AllowAutomatedReactivation"].Value.AsBoolean() )
                            .Select( a => a.Id ).ToList();
            var inactiveStatusId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;

            List<Group> familiesWithInactivePerson = null;
            List<Person> allMemberofFamilies = null;

            using ( RockContext rockContext = new RockContext() )
            {
                familiesWithInactivePerson = new GroupMemberService( rockContext )
                    .Queryable( "Group.Members" )
                   .Where( m => m.Group.GroupTypeId == familyGroupTypeId &&
                         m.Person.RecordStatusValueId.HasValue &&
                         m.Person.RecordStatusValueId == inactiveStatusId &&
                         m.Person.RecordStatusReasonValueId.HasValue &&
                         values.Contains( m.Person.RecordStatusReasonValueId.Value ) )
                    .OrderBy( m => m.GroupOrder ?? int.MaxValue )
                    .DistinctBy( a => a.GroupId )
                    .Select( m => m.Group )
                    .ToList();

                allMemberofFamilies = familiesWithInactivePerson
                                        .SelectMany( a => a.Members.Select( b => b.Person ) )
                                        .ToList();

                if ( allMemberofFamilies.Count == 0 )
                {
                    return;
                }
            }


            List<int> qualifiedPersonIds = new List<int>();

            if ( _reactivateSettings.IsLastContributionEnabled )
            {
                List<int> fulfilledPersonIds = CheckLastContribution( allMemberofFamilies, _reactivateSettings.LastContributionPeriod );
                if ( fulfilledPersonIds != null && fulfilledPersonIds.Count > 0 )
                {
                    allMemberofFamilies.RemoveAll( a => fulfilledPersonIds.Contains( a.Id ) );
                    qualifiedPersonIds.AddRange( fulfilledPersonIds );
                }
            }

            if ( _reactivateSettings.IsAttendanceInGroupTypeEnabled && allMemberofFamilies.Count > 0 && _reactivateSettings.AttendanceInGroupType.Count > 0 )
            {
                List<int> fulfilledPersonIds = CheckAttendanceInGroupType( allMemberofFamilies, _reactivateSettings.AttendanceInGroupTypeDays, _reactivateSettings.AttendanceInGroupType );

                if ( fulfilledPersonIds != null && fulfilledPersonIds.Count > 0 )
                {
                    allMemberofFamilies.RemoveAll( a => fulfilledPersonIds.Contains( a.Id ) );
                    qualifiedPersonIds.AddRange( fulfilledPersonIds );
                }
            }

            if ( _reactivateSettings.IsAttendanceInServiceGroupEnabled && allMemberofFamilies.Count > 0 )
            {
                List<int> fulfilledPersonIds = CheckAttendanceInServiceGroup( allMemberofFamilies, _reactivateSettings.AttendanceInServiceGroupPeriod );

                if ( fulfilledPersonIds != null && fulfilledPersonIds.Count > 0 )
                {
                    allMemberofFamilies.RemoveAll( a => fulfilledPersonIds.Contains( a.Id ) );
                    qualifiedPersonIds.AddRange( fulfilledPersonIds );
                }
            }

            if ( _reactivateSettings.IsPrayerRequestEnabled && allMemberofFamilies.Count > 0 )
            {
                List<int> fulfilledPersonIds = CheckPrayerRequest( allMemberofFamilies, _reactivateSettings.PrayerRequestPeriod );

                if ( fulfilledPersonIds != null && fulfilledPersonIds.Count > 0 )
                {
                    allMemberofFamilies.RemoveAll( a => fulfilledPersonIds.Contains( a.Id ) );
                    qualifiedPersonIds.AddRange( fulfilledPersonIds );
                }
            }

            if ( _reactivateSettings.IsPersonAttributesEnabled && _reactivateSettings.PersonAttributes.Count > 0 && allMemberofFamilies.Count > 0 )
            {
                List<int> fulfilledPersonIds = CheckPersonAttribute( allMemberofFamilies, _reactivateSettings.PersonAttributesDays, _reactivateSettings.PersonAttributes );

                if ( fulfilledPersonIds != null && fulfilledPersonIds.Count > 0 )
                {
                    allMemberofFamilies.RemoveAll( a => fulfilledPersonIds.Contains( a.Id ) );
                    qualifiedPersonIds.AddRange( fulfilledPersonIds );

                }
            }

            if ( _reactivateSettings.IsIncludeDataViewEnabled && !string.IsNullOrEmpty( _reactivateSettings.IncludeDataView ) && allMemberofFamilies.Count > 0 )
            {
                List<int> fulfilledPersonIds = CheckInDataView( allMemberofFamilies, _reactivateSettings.IncludeDataView.AsInteger() );
                if ( fulfilledPersonIds != null && fulfilledPersonIds.Count > 0 )
                {
                    allMemberofFamilies.RemoveAll( a => fulfilledPersonIds.Contains( a.Id ) );
                    qualifiedPersonIds.AddRange( fulfilledPersonIds );
                }
            }

            if ( _reactivateSettings.IsInteractionsEnabled && allMemberofFamilies.Count > 0 && _reactivateSettings.Interactions != null && _reactivateSettings.Interactions.Count > 0 )
            {
                foreach ( var interaction in _reactivateSettings.Interactions.Where( a => a.IsInteractionTypeEnabled ) )
                {
                    List<int> fulfilledPersonIds = CheckInteraction( allMemberofFamilies, interaction );

                    if ( fulfilledPersonIds != null && fulfilledPersonIds.Count > 0 )
                    {
                        allMemberofFamilies.RemoveAll( a => fulfilledPersonIds.Contains( a.Id ) );
                        qualifiedPersonIds.AddRange( fulfilledPersonIds );

                    }
                }
            }

            if ( _reactivateSettings.IsExcludeDataViewEnabled && !string.IsNullOrEmpty( _reactivateSettings.ExcludeDataView ) && qualifiedPersonIds.Count > 0 )
            {
                using ( RockContext rockContext = new RockContext() )
                {
                    var dataView = new DataViewService( rockContext ).Get( _reactivateSettings.ExcludeDataView.AsInteger() );
                    if ( dataView != null )
                    {
                        List<string> errorMessages = new List<string>();
                        var qry = dataView.GetQuery( null, null, out errorMessages );
                        if ( qry != null )
                        {
                            var fulfilledPersonIds = qry.Where( e => qualifiedPersonIds.Contains( e.Id ) )
                                  .Select( e => e.Id )
                                  .ToList();
                            qualifiedPersonIds.RemoveAll( a => fulfilledPersonIds.Contains( a ) );
                        }
                    }
                }
            }


            //For all the qualified Person, get their family and reactivate inactive members
            if ( qualifiedPersonIds.Count > 0 )
            {
                var activeStatusId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;

                var qualifiedInactiveMembers = familiesWithInactivePerson.Where( a => a.Members.Any( b => qualifiedPersonIds.Contains( b.PersonId ) ) )
                                .SelectMany( a => a.Members.Select( b => b.Person ) )
                                .Where( m => m.RecordStatusValueId.HasValue &&
                                m.RecordStatusValueId == inactiveStatusId &&
                                m.RecordStatusReasonValueId.HasValue &&
                                values.Contains( m.RecordStatusReasonValueId.Value ) );

                foreach ( var person in qualifiedInactiveMembers )
                {
                    using ( RockContext rockContext = new RockContext() )
                    {
                        var personService = new PersonService( rockContext );
                        var inactivePerson = personService.Get( person.Id );
                        inactivePerson.RecordStatusValueId = activeStatusId;
                        inactivePerson.RecordStatusReasonValueId = null;

                        var changes = new List<string>();
                        changes.Add( $"Modifed Record Status from <span class='field-value'>{DefinedValueCache.GetName( person.RecordStatusValueId )}</span> to <span class'field-value'>{DefinedValueCache.GetName( activeStatusId )}</span> due to data automation job." );
                        HistoryService.AddChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON.AsGuid(),
                           person.Id, changes );
                        rockContext.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Update families on inactivate settings.
        /// </summary>
        private void ProcessInactivateSetting()
        {
            var familyGroupTypeId = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Id;

            var activeStatusId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;

            List<Group> familiesWithActivePerson = null;

            using ( RockContext rockContext = new RockContext() )
            {
                familiesWithActivePerson = new GroupMemberService( rockContext )
                    .Queryable( "Group.Members.Person" )
                   .Where( m => m.Group.GroupTypeId == familyGroupTypeId &&
                         m.Person.RecordStatusValueId.HasValue &&
                         m.Person.RecordStatusValueId == activeStatusId )
                    .OrderBy( m => m.GroupOrder ?? int.MaxValue )
                    .DistinctBy( a => a.GroupId )
                    .Select( m => m.Group )
                    .ToList();
            }

            if ( _inactivateSettings.IsNoLastContributionEnabled && familiesWithActivePerson.Count > 0 )
            {
                var allMemberofFamilies = GetMemberOfFamilies( familiesWithActivePerson );
                List<int> nonfulfilledPersonIds = CheckLastContribution( allMemberofFamilies, _inactivateSettings.NoLastContributionPeriod );

                if ( nonfulfilledPersonIds != null && nonfulfilledPersonIds.Count > 0 )
                {
                    familiesWithActivePerson.RemoveAll( a => a.Members.Where( b => nonfulfilledPersonIds.Contains( b.PersonId ) ).Any() );
                }
            }

            if ( _inactivateSettings.IsNoAttendanceInGroupTypeEnabled && familiesWithActivePerson.Count > 0 && _inactivateSettings.AttendanceInGroupType.Count > 0 )
            {
                var allMemberofFamilies = GetMemberOfFamilies( familiesWithActivePerson );
                List<int> nonfulfilledPersonIds = CheckAttendanceInGroupType( allMemberofFamilies, _inactivateSettings.NoAttendanceInGroupTypeDays, _inactivateSettings.AttendanceInGroupType );

                if ( nonfulfilledPersonIds != null && nonfulfilledPersonIds.Count > 0 )
                {
                    familiesWithActivePerson.RemoveAll( a => a.Members.Where( b => nonfulfilledPersonIds.Contains( b.PersonId ) ).Any() );
                }
            }

            if ( _inactivateSettings.IsNoAttendanceInServiceGroupEnabled && familiesWithActivePerson.Count > 0 )
            {
                var allMemberofFamilies = GetMemberOfFamilies( familiesWithActivePerson );
                List<int> nonfulfilledPersonIds = CheckAttendanceInServiceGroup( allMemberofFamilies, _inactivateSettings.NoAttendanceInServiceGroupPeriod );

                if ( nonfulfilledPersonIds != null && nonfulfilledPersonIds.Count > 0 )
                {
                    familiesWithActivePerson.RemoveAll( a => a.Members.Where( b => nonfulfilledPersonIds.Contains( b.PersonId ) ).Any() );
                }
            }

            if ( _inactivateSettings.IsNoPrayerRequestEnabled && familiesWithActivePerson.Count > 0 )
            {
                var allMemberofFamilies = GetMemberOfFamilies( familiesWithActivePerson );
                List<int> nonfulfilledPersonIds = CheckPrayerRequest( allMemberofFamilies, _inactivateSettings.NoPrayerRequestPeriod );

                if ( nonfulfilledPersonIds != null && nonfulfilledPersonIds.Count > 0 )
                {
                    familiesWithActivePerson.RemoveAll( a => a.Members.Where( b => nonfulfilledPersonIds.Contains( b.PersonId ) ).Any() );
                }
            }

            if ( _inactivateSettings.IsNoPersonAttributesEnabled && _inactivateSettings.PersonAttributes.Count > 0 && familiesWithActivePerson.Count > 0 )
            {
                var allMemberofFamilies = GetMemberOfFamilies( familiesWithActivePerson );
                List<int> nonfulfilledPersonIds = CheckPersonAttribute( allMemberofFamilies, _inactivateSettings.NoPersonAttributesDays, _inactivateSettings.PersonAttributes );

                if ( nonfulfilledPersonIds != null && nonfulfilledPersonIds.Count > 0 )
                {
                    familiesWithActivePerson.RemoveAll( a => a.Members.Where( b => nonfulfilledPersonIds.Contains( b.PersonId ) ).Any() );

                }
            }

            if ( _inactivateSettings.IsNotInDataviewEnabled && !string.IsNullOrEmpty( _inactivateSettings.NotInDataview ) && familiesWithActivePerson.Count > 0 )
            {
                var allMemberofFamilies = GetMemberOfFamilies( familiesWithActivePerson );
                List<int> nonfulfilledPersonIds = CheckInDataView( allMemberofFamilies, _inactivateSettings.NotInDataview.AsInteger() );

                if ( nonfulfilledPersonIds != null && nonfulfilledPersonIds.Count > 0 )
                {
                    familiesWithActivePerson.RemoveAll( a => a.Members.Where( b => nonfulfilledPersonIds.Contains( b.PersonId ) ).Any() );
                }
            }

            if ( _inactivateSettings.IsNoInteractionsEnabled && familiesWithActivePerson.Count > 0 && _inactivateSettings.NoInteractions != null && _inactivateSettings.NoInteractions.Count > 0 )
            {
                foreach ( var interaction in _inactivateSettings.NoInteractions.Where( a => a.IsInteractionTypeEnabled ) )
                {
                    var allMemberofFamilies = GetMemberOfFamilies( familiesWithActivePerson );
                    List<int> nonfulfilledPersonIds = CheckInteraction( allMemberofFamilies, interaction );

                    if ( nonfulfilledPersonIds != null && nonfulfilledPersonIds.Count > 0 )
                    {
                        familiesWithActivePerson.RemoveAll( a => a.Members.Where( b => nonfulfilledPersonIds.Contains( b.PersonId ) ).Any() );
                    }
                }
            }

            //For all the families, get their family and inactivate active members
            if ( familiesWithActivePerson.Count > 0 )
            {
                var inActiveStatusId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;

                var activeMembers = familiesWithActivePerson
                                .SelectMany( a => a.Members.Select( b => b.Person ) )
                                .Where( m => m.RecordStatusValueId.HasValue &&
                                m.RecordStatusValueId == activeStatusId );

                foreach ( var person in activeMembers )
                {
                    using ( RockContext rockContext = new RockContext() )
                    {
                        var personService = new PersonService( rockContext );
                        var inactivePerson = personService.Get( person.Id );
                        inactivePerson.RecordStatusValueId = inActiveStatusId;
                        inactivePerson.RecordStatusReasonValueId = null;

                        var changes = new List<string>();
                        changes.Add( $"Modifed Record Status from <span class='field-value'>{DefinedValueCache.GetName( person.RecordStatusValueId )}</span> to <span class'field-value'>{DefinedValueCache.GetName( inActiveStatusId )}</span> due to data automation job." );
                        HistoryService.AddChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON.AsGuid(),
                           person.Id, changes );
                        rockContext.SaveChanges();
                    }
                }
            }
        }

        private List<int> CheckInDataView( List<Person> allMemberofFamilies, int dataviewId )
        {
            var fulfilledPersonIds = new List<int>();
            var personIds = allMemberofFamilies.Select( a => a.Id ).ToList();
            using ( RockContext rockContext = new RockContext() )
            {
                var dataView = new DataViewService( rockContext ).Get( dataviewId );
                if ( dataView != null )
                {
                    List<string> errorMessages = new List<string>();
                    var qry = dataView.GetQuery( null, null, out errorMessages );
                    if ( qry != null )
                    {
                        fulfilledPersonIds = qry.Where( e => personIds.Contains( e.Id ) )
                                   .Select( e => e.Id )
                                   .ToList();

                    }
                }
            }

            return fulfilledPersonIds;
        }

        private List<int> CheckInteraction( List<Person> allMemberofFamilies, InteractionItem interaction )
        {
            List<int> fulfilledPersonIds = new List<int>();
            var personIds = allMemberofFamilies.Select( a => a.Id ).ToList();
            var interactionStartPeriod = RockDateTime.Now.AddDays( -interaction.LastInteractionDays );
            using ( RockContext rockContext = new RockContext() )
            {
                fulfilledPersonIds = new InteractionService( rockContext )
                   .Queryable()
                   .AsNoTracking()
                   .Where( a => a.InteractionDateTime >= interactionStartPeriod &&
                        a.InteractionComponent.Channel.Guid == interaction.Guid &&
                        personIds.Contains( a.PersonAlias.PersonId ) )
                   .Select( a => a.PersonAlias.PersonId )
                   .ToList();
            }

            return fulfilledPersonIds;
        }

        private List<int> CheckPrayerRequest( List<Person> allMemberofFamilies, int periodInDays )
        {
            var prayerRequestStartDate = RockDateTime.Now.AddDays( -periodInDays );
            var personIds = allMemberofFamilies.Select( a => a.Id ).ToList();
            List<int> fulfilledPersonIds = new List<int>();
            using ( RockContext rockContext = new RockContext() )
            {
                fulfilledPersonIds = new PrayerRequestService( rockContext )
                    .Queryable()
                    .Where( a =>
                          a.EnteredDateTime >= prayerRequestStartDate &&
                          personIds.Contains( a.RequestedByPersonAlias.PersonId ) )
                      .Select( a => a.RequestedByPersonAlias.PersonId )
                      .Distinct()
                      .ToList();
            }
            return fulfilledPersonIds;
        }

        private List<int> CheckPersonAttribute( List<Person> allMemberofFamilies, int periodInDays, List<int> attributeIds )
        {
            var attributeModifiedStartDate = RockDateTime.Now.AddDays( -periodInDays );
            var personIds = allMemberofFamilies.Select( a => a.Id ).ToList();
            List<int> fulfilledPersonIds = new List<int>();

            using ( RockContext rockContext = new RockContext() )
            {
                fulfilledPersonIds = new AttributeValueService( rockContext )
                    .Queryable()
                    .Where( a =>
                          a.ModifiedDateTime.HasValue && a.ModifiedDateTime >= attributeModifiedStartDate &&
                          attributeIds.Contains( a.AttributeId ) &&
                          a.EntityId.HasValue && personIds.Contains( a.EntityId.Value ))
                      .Select( a => a.EntityId.Value )
                      .Distinct()
                      .ToList();
            }
            return fulfilledPersonIds;
        }

        private List<int> CheckAttendanceInServiceGroup( List<Person> allMemberofFamilies, int periodInDays )
        {
            var attendanceStartDate = RockDateTime.Now.AddDays( -periodInDays );
            var personIds = allMemberofFamilies.Select( a => a.Id ).ToList();
            List<int> fulfilledPersonIds = new List<int>();
            using ( RockContext rockContext = new RockContext() )
            {
                fulfilledPersonIds = new AttendanceService( rockContext )
                    .Queryable()
                    .Where( a =>
                          a.Group.GroupType.AttendanceCountsAsWeekendService &&
                          a.StartDateTime >= attendanceStartDate &&
                          personIds.Contains( a.PersonAlias.PersonId ) )
                      .Select( a => a.PersonAlias.PersonId )
                      .Distinct()
                      .ToList();
            }
            return fulfilledPersonIds;
        }

        private List<int> CheckAttendanceInGroupType( List<Person> allMemberofFamilies, int periodInDays, List<int> groupTypes )
        {
            var attendanceStartDate = RockDateTime.Now.AddDays( -periodInDays );
            var personIds = allMemberofFamilies.Select( a => a.Id ).ToList();
            List<int> fulfilledPersonIds = new List<int>();

            using ( RockContext rockContext = new RockContext() )
            {
                fulfilledPersonIds = new AttendanceService( rockContext )
                    .Queryable()
                    .Where( a =>
                        groupTypes.Contains( a.Group.GroupTypeId ) &&
                        a.StartDateTime >= attendanceStartDate &&
                        personIds.Contains( a.PersonAlias.PersonId ) )
                    .Select( a => a.PersonAlias.PersonId )
                    .Distinct()
                    .ToList();
            }

            return fulfilledPersonIds;
        }

        private List<int> CheckLastContribution( List<Person> allMemberofFamilies, int periodInDays )
        {
            int transactionTypeContributionId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ).Id;
            var contributionStartDate = RockDateTime.Now.AddDays( -periodInDays );

            var personIds = allMemberofFamilies.Select( a => a.Id ).ToList();
            List<int> fulfilledPersonIds = new List<int>();
            using ( RockContext rockContext = new RockContext() )
            {
                fulfilledPersonIds = new FinancialTransactionService( rockContext ).Queryable()
                        .Where( a => a.TransactionTypeValueId == transactionTypeContributionId &&
                             a.AuthorizedPersonAliasId.HasValue && personIds.Contains( a.AuthorizedPersonAlias.PersonId ) &&
                             a.SundayDate >= contributionStartDate )
                        .Select( a => a.AuthorizedPersonAlias.PersonId )
                        .Distinct()
                        .ToList();
            }
            return fulfilledPersonIds;
        }

        private static List<Person> GetMemberOfFamilies( List<Group> familiesWithActivePerson )
        {
            return familiesWithActivePerson
                                .SelectMany( a => a.Members.Select( b => b.Person ) )
                                .ToList();
        }
    }
}
