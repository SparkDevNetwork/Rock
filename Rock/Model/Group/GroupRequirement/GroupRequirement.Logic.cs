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

using Rock.Data;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rock.Model
{
    public partial class GroupRequirement
    {
        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( this.GroupRequirementType != null )
            {
                if ( this.Group != null )
                {
                    return string.Format( "{0}|{1}", this.GroupRequirementType, this.Group );
                }
                else if ( this.GroupType != null )
                {
                    return string.Format( "{0}|{1}", this.GroupRequirementType, this.GroupType );
                }
            }

            return base.ToString();
        }

        /// <summary>
        /// Gets the parent security authority for this GroupRequirement.
        /// </summary>
        /// <value>
        /// The parent security authority for this GroupRequirement.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.GroupRequirementType != null ? this.GroupRequirementType : base.ParentAuthority;
            }
        }

        /// <summary>
        /// Returns a list of each person and their GroupRequirement status for this group requirement
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="personQry">The person qry.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">No dataview assigned to Group Requirement Type: " + this.GroupRequirementType.Name</exception>
        public IEnumerable<PersonGroupRequirementStatus> PersonQueryableMeetsGroupRequirement( RockContext rockContext, IQueryable<Person> personQry, int groupId, int? groupRoleId )
        {
            return PersonQueryableMeetsGroupRequirement( rockContext, personQry, groupId, groupRoleId, null );
        }

        /// <summary>
        /// Returns the status of this Group Requirement for the specified Group and for each person in the supplied query.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="personQry">The person qry.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <param name="dataViewCache">An optional cache that stores Data View results to improve the efficiency of repeated calls to this method in the same processing action.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">No dataview assigned to Group Requirement Type: " + this.GroupRequirementType.Name</exception>
        internal IEnumerable<PersonGroupRequirementStatus> PersonQueryableMeetsGroupRequirement( RockContext rockContext, IQueryable<Person> personQry, int groupId, int? groupRoleId, DataViewResultsCache dataViewCache )
        {
            if ( ( this.GroupRoleId != null ) && ( groupRoleId != null ) && ( this.GroupRoleId != groupRoleId ) )
            {
                // If this GroupRequirement is for a specific group role, and the groupRole we are comparing is not the same role.
                var result = personQry.Select( p => p.Id ).ToList().Select( a =>
                     new PersonGroupRequirementStatus
                     {
                         PersonId = a,
                         GroupRequirement = this,
                         MeetsGroupRequirement = MeetsGroupRequirement.NotApplicable
                     } );

                return result;
            }

            // Create a list of statuses so that not applicable status results can be included.
            List<PersonGroupRequirementStatus> statusResults = new List<PersonGroupRequirementStatus>();

            // If this GroupRequirement is not for 'All' age classifications, then calculate those statuses.
            if ( this.AppliesToAgeClassification != AppliesToAgeClassification.All )
            {
                // If the person's age classification we are comparing is not the one applied to this requirement,
                // then mark that person's requirement status as 'Not Applicable'.
                var notApplicablePersonQry = personQry.Where( p => ( int ) p.AgeClassification != ( int ) this.AppliesToAgeClassification );
                var results = notApplicablePersonQry
                    .Select( p => p.Id )
                    .ToList()
                    .Select( a =>
                    new PersonGroupRequirementStatus
                    {
                        PersonId = a,
                        GroupRequirement = this,
                        MeetsGroupRequirement = MeetsGroupRequirement.NotApplicable
                    } );

                if ( results != null )
                {
                    statusResults.AddRange( results );
                }

                personQry = personQry.Where( p => ( int ) p.AgeClassification == ( int ) this.AppliesToAgeClassification );
            }

            if ( this.AppliesToDataViewId.HasValue )
            {
                // If the Group Requirement has a Data View it applies to, apply it here.
                IQueryable<int> appliesToDataViewPersonIds = null;

                if ( dataViewCache != null )
                {
                    // Get the Data View result from the supplied cache.
                    appliesToDataViewPersonIds = dataViewCache.GetDataViewResultQueryable( this.AppliesToDataViewId.Value, rockContext );
                }
                else
                { 
                    // Get the Data View result.
                    var appliesToDataViewPersonService = new PersonService( rockContext );
                    var appliesToDataViewParamExpression = appliesToDataViewPersonService.ParameterExpression;
                    var appliesToDataViewWhereExpression = this.AppliesToDataView.GetExpression( appliesToDataViewPersonService, appliesToDataViewParamExpression );
                    appliesToDataViewPersonIds = appliesToDataViewPersonService.Get( appliesToDataViewParamExpression, appliesToDataViewWhereExpression ).Select( p => p.Id );
                }

                // For all people in the supplied person query that are not represented in the "Applies To" query,
                // add a "Not Applicable" status result.
                var notApplicablePersonQry = personQry.Where( p => !appliesToDataViewPersonIds.Contains( p.Id ) );
                
                var results = notApplicablePersonQry.Select( p => p.Id ).ToList()
                    .Select( a =>
                        new PersonGroupRequirementStatus
                        {
                            PersonId = a,
                            GroupRequirement = this,
                            MeetsGroupRequirement = MeetsGroupRequirement.NotApplicable
                        } );

                if ( results != null )
                {
                    statusResults.AddRange( results );
                }

                personQry = personQry.Where( p => appliesToDataViewPersonIds.Contains( p.Id ) );
                
            }

            var attributeValueService = new AttributeValueService( rockContext );

            if ( this.GroupRequirementType.RequirementCheckType == RequirementCheckType.Dataview )
            {
                var personService = new PersonService( rockContext );
                var paramExpression = personService.ParameterExpression;

                IQueryable<int> warningDataViewPersonIdQuery = null;
                if ( this.GroupRequirementType.WarningDataViewId.HasValue )
                {
                    if ( dataViewCache != null )
                    {
                        // Get the Data View result from the supplied cache.
                        warningDataViewPersonIdQuery = dataViewCache.GetDataViewResultQueryable( this.GroupRequirementType.WarningDataViewId.Value, rockContext );
                    }
                    else
                    {
                        // Get the Data View result.
                        var warningDataViewWhereExpression = this.GroupRequirementType.WarningDataView.GetExpression( personService, paramExpression );
                        warningDataViewPersonIdQuery = personService.Get( paramExpression, warningDataViewWhereExpression ).Where( a => personQry.Any( p => p.Id == a.Id ) ).Select( a => a.Id );
                    }
                }

                if ( this.GroupRequirementType.DataViewId.HasValue )
                {
                    var groupRequirementTypeDataViewId = this.GroupRequirementType.DataViewId.Value;

                    IQueryable<int> groupRequirementTypePersonIdQuery;
                    if ( dataViewCache != null )
                    {
                        // Get the Data View result from the supplied cache.
                        groupRequirementTypePersonIdQuery = dataViewCache.GetDataViewResultQueryable( groupRequirementTypeDataViewId, rockContext );
                    }
                    else
                    {
                        // Get the Data View result.
                        var dataViewWhereExpression = this.GroupRequirementType.DataView.GetExpression( personService, paramExpression );
                        groupRequirementTypePersonIdQuery = personService.Get( paramExpression, dataViewWhereExpression ).Select( p => p.Id );
                    }

                    var candidatePersonIdList = personQry.Select( p => p.Id )
                        .ToHashSet();

                    var personWithRequirementsList = candidatePersonIdList.Intersect( groupRequirementTypePersonIdQuery )
                        .Select( p => new { PersonId = p, Included = true } )
                        .Union( candidatePersonIdList.Except( groupRequirementTypePersonIdQuery ).Select( p => new { PersonId = p, Included = false } ) )
                        .ToList();

                    var result = personWithRequirementsList.Select( a =>
                    {
                        GroupMemberRequirementService groupMemberRequirementService = new GroupMemberRequirementService( rockContext );
                            
                        // Get the nullable group member requirement ID based on the PersonId, GroupRequirementId, GroupId, and GroupRoleId.
                        int? groupMemberRequirementId = groupMemberRequirementService.GetIdByPersonIdRequirementIdGroupIdGroupRoleId( a.PersonId, this.Id, groupId, groupRoleId );
                        var personGroupRequirementStatus = new PersonGroupRequirementStatus
                        {
                            PersonId = a.PersonId,
                            GroupRequirement = this,
                            GroupMemberRequirementId = groupMemberRequirementId,
                        };

                        var hasWarning = warningDataViewPersonIdQuery?.Contains( a.PersonId ) == true;

                        if ( a.Included )
                        {
                            if ( hasWarning )
                            {
                                personGroupRequirementStatus.MeetsGroupRequirement = MeetsGroupRequirement.MeetsWithWarning;
                            }
                            else
                            {
                                personGroupRequirementStatus.MeetsGroupRequirement = MeetsGroupRequirement.Meets;
                                personGroupRequirementStatus.RequirementWarningDateTime = null;
                            }
                        }
                        else
                        {
                            var possibleDueDate = CalculateGroupMemberRequirementDueDate(
                                this.GroupRequirementType.DueDateType,
                                this.GroupRequirementType.DueDateOffsetInDays,
                                this.DueDateStaticDate,
                                this.DueDateAttributeId.HasValue ? new AttributeValueService( rockContext ).GetByAttributeIdAndEntityId( this.DueDateAttributeId.Value, this.GroupId )?.Value.AsDateTime() ?? null : null,
                                new GroupService( rockContext ).Get( groupId ).Members.Where( m => m.PersonId == a.PersonId && m.GroupRoleId == groupRoleId ).Select( m => m.DateTimeAdded ).DefaultIfEmpty( null ).FirstOrDefault() );

                            bool isRequirementDue = possibleDueDate.HasValue ? possibleDueDate <= RockDateTime.Now : true;

                            if ( !isRequirementDue )
                            {
                                personGroupRequirementStatus.MeetsGroupRequirement = MeetsGroupRequirement.MeetsWithWarning;
                            }
                            else
                            {
                                personGroupRequirementStatus.MeetsGroupRequirement = MeetsGroupRequirement.NotMet;
                            }
                        }

                        return personGroupRequirementStatus;
                    } );

                    if ( result != null )
                    {
                        statusResults.AddRange( result );
                    }

                    return statusResults;
                }
                else
                {
                    var personWithIdRequirements = personQry.Select( p => p.Id );
                    GroupMemberRequirementService groupMemberRequirementService = new GroupMemberRequirementService( rockContext );
                    var result = personWithIdRequirements.ToList().Select( a =>
                    {
                        var personGroupRequirementStatus = new PersonGroupRequirementStatus
                        {
                            PersonId = a,
                            GroupRequirement = this,
                            MeetsGroupRequirement = warningDataViewPersonIdQuery?.Contains( a ) == true ? MeetsGroupRequirement.MeetsWithWarning : MeetsGroupRequirement.Meets,
                            GroupMemberRequirementId = groupMemberRequirementService.GetIdByPersonIdRequirementIdGroupIdGroupRoleId( a, this.Id, groupId, groupRoleId ),
                        };

                        return personGroupRequirementStatus;
                    } );

                    if ( result != null )
                    {
                        statusResults.AddRange( result );
                    }

                    return statusResults;
                }
            }
            else if ( this.GroupRequirementType.RequirementCheckType == RequirementCheckType.Sql )
            {
                // if requirement set on GroupType, this.Group is null
                var targetGroup = this.Group ?? new GroupService( rockContext ).Get( groupId );
                var personQryIdList = personQry.Select( a => a.Id ).ToList();
                Person personMergeField = null;
                if ( personQryIdList.Count == 1 )
                {
                    var personId = personQryIdList[0];
                    personMergeField = new PersonService( rockContext ).GetNoTracking( personId );
                }

                string formattedSql = this.GroupRequirementType.SqlExpression.ResolveMergeFields( this.GroupRequirementType.GetMergeObjects( targetGroup, personMergeField ) );
                string warningFormattedSql = this.GroupRequirementType.WarningSqlExpression.ResolveMergeFields( this.GroupRequirementType.GetMergeObjects( targetGroup, personMergeField ) );
                try
                {
                    var tableResult = DbService.GetDataTable( formattedSql, System.Data.CommandType.Text, null );
                    if ( tableResult.Columns.Count > 0 )
                    {
                        IEnumerable<int> personIds = tableResult.Rows.OfType<System.Data.DataRow>().Select( r => Convert.ToInt32( r[0] ) );
                        IEnumerable<int> warningPersonIds = null;

                        // if a Warning SQL was specified, get a list of PersonIds that should have a warning with their status
                        if ( !string.IsNullOrWhiteSpace( warningFormattedSql ) )
                        {
                            var warningTableResult = DbService.GetDataTable( warningFormattedSql, System.Data.CommandType.Text, null );
                            if ( warningTableResult.Columns.Count > 0 )
                            {
                                warningPersonIds = warningTableResult.Rows.OfType<System.Data.DataRow>().Select( r => Convert.ToInt32( r[0] ) );
                            }
                        }

                        var result = personQryIdList.Select( a =>
                        {
                            var possibleDueDate = CalculateGroupMemberRequirementDueDate(
                            this.GroupRequirementType.DueDateType,
                            this.GroupRequirementType.DueDateOffsetInDays,
                            this.DueDateStaticDate,
                            this.DueDateAttributeId.HasValue ? new AttributeValueService( rockContext ).GetByAttributeIdAndEntityId( this.DueDateAttributeId.Value, this.GroupId )?.Value.AsDateTime() ?? null : null,
                            new GroupService( rockContext ).Get( groupId ).Members.Where( m => m.PersonId == a && m.GroupRoleId == groupRoleId ).Select( m => m.DateTimeAdded ).DefaultIfEmpty( null ).FirstOrDefault() );
                            bool isRequirementDue = possibleDueDate.HasValue ? possibleDueDate <= RockDateTime.Now : true;
                            var personGroupRequirementStatus = new PersonGroupRequirementStatus
                            {
                                PersonId = a,
                                GroupRequirement = this,
                                MeetsGroupRequirement = personIds.Contains( a )
                                      ? ( ( warningPersonIds != null && warningPersonIds.Contains( a ) )
                                          ? MeetsGroupRequirement.MeetsWithWarning
                                          : MeetsGroupRequirement.Meets
                                          )
                                      : isRequirementDue ? MeetsGroupRequirement.NotMet : MeetsGroupRequirement.MeetsWithWarning,
                            };
                            return personGroupRequirementStatus;
                        } );

                        if ( result != null )
                        {
                            statusResults.AddRange( result );
                        }

                        return statusResults;
                    }
                }
                catch ( Exception ex )
                {
                    // Exception occurred (probably due to bad SQL)
                    ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );

                    var result = personQry.Select( a => a.Id ).ToList().Select( a => new PersonGroupRequirementStatus
                    {
                        PersonId = a,
                        GroupRequirement = this,
                        MeetsGroupRequirement = MeetsGroupRequirement.Error,
                        CalculationException = ex
                    } );

                    if ( result != null )
                    {
                        statusResults.AddRange( result );
                    }

                    return statusResults;
                }
            }
            else
            {
                // manual
                GroupMemberRequirementService groupMemberRequirementService = new GroupMemberRequirementService( rockContext );
                var groupMemberRequirementQry = groupMemberRequirementService.Queryable().Where( a => a.GroupMember.GroupId == groupId && a.GroupRequirementId == this.Id );
                var groupMemberRequirementWithMetDateTimeQry = groupMemberRequirementQry.Where( a => a.RequirementMetDateTime.HasValue );

                var result = personQry.ToList().Select( a =>
                {
                    var possibleDueDate = CalculateGroupMemberRequirementDueDate(
                            this.GroupRequirementType.DueDateType,
                            this.GroupRequirementType.DueDateOffsetInDays,
                            this.DueDateStaticDate,
                            this.DueDateAttributeId.HasValue ? new AttributeValueService( rockContext ).GetByAttributeIdAndEntityId( this.DueDateAttributeId.Value, this.GroupId )?.Value.AsDateTime() ?? null : null,
                            new GroupService( rockContext )
                            .Get( groupId ).Members.Where( m => m.PersonId == a.Id && m.GroupRoleId == groupRoleId ).Select( m => m.DateTimeAdded ).DefaultIfEmpty( null ).FirstOrDefault() );

                    return new PersonGroupRequirementStatus
                    {
                        PersonId = a.Id,
                        GroupRequirement = this,
                        GroupMemberRequirementId = groupMemberRequirementService.GetIdByPersonIdRequirementIdGroupIdGroupRoleId( a.Id, this.Id, groupId, groupRoleId ),
                        RequirementDueDate = possibleDueDate,
                        MeetsGroupRequirement = groupMemberRequirementWithMetDateTimeQry.Any( r => r.GroupMember.PersonId == a.Id ) ? MeetsGroupRequirement.Meets
                        : possibleDueDate.HasValue ? possibleDueDate > RockDateTime.Now ? MeetsGroupRequirement.MeetsWithWarning : MeetsGroupRequirement.NotMet : MeetsGroupRequirement.NotMet
                    };
                } );

                if ( result != null )
                {
                    statusResults.AddRange( result );
                }

                return statusResults;
            }

            // This shouldn't happen, since a requirement must have a dataview, SQL or manual origin.
            return null;
        }

        /// <summary>
        /// Check if the Person meets the group requirement for the role
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <returns></returns>
        [RockObsolete( "1.14" )]
        [Obsolete( "Does not pass the RockContext into subsequent method calls.  Use PersonMeetsGroupRequirement( RockContext rockContext...) instead.", false )]
        public PersonGroupRequirementStatus PersonMeetsGroupRequirement( int personId, int groupId, int? groupRoleId )
        {
            return PersonMeetsGroupRequirement( null, personId, groupId, groupRoleId );
        }

        /// <summary>
        /// Check if the Person meets the group requirement for the role.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <returns></returns>
        public PersonGroupRequirementStatus PersonMeetsGroupRequirement( RockContext rockContext, int personId, int groupId, int? groupRoleId )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            var personQuery = new PersonService( rockContext ).Queryable().Where( a => a.Id == personId );
            var result = this.PersonQueryableMeetsGroupRequirement( rockContext, personQuery, groupId, groupRoleId ).FirstOrDefault();
            if ( result == null )
            {
                // no result. probably because personId was zero
                return new PersonGroupRequirementStatus
                {
                    GroupRequirement = this,
                    MeetsGroupRequirement = MeetsGroupRequirement.NotMet,
                    PersonId = personId
                };
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// Updates the group member requirement result.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="meetsGroupRequirement">The meets group requirement.</param>
        public void UpdateGroupMemberRequirementResult( RockContext rockContext, int personId, int groupId, MeetsGroupRequirement meetsGroupRequirement )
        {
            GroupRequirement groupRequirement = this;
            var currentDateTime = RockDateTime.Now;
            GroupMemberRequirementService groupMemberRequirementService = new GroupMemberRequirementService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );
            var groupMemberQry = groupMemberService.Queryable( true ).Where( a => a.PersonId == personId && a.GroupId == groupId );

            if ( groupRequirement.GroupId.HasValue )
            {
                groupMemberQry = groupMemberQry.Where( g => g.GroupId == groupRequirement.GroupId );
            }
            else if ( groupRequirement.GroupTypeId.HasValue )
            {
                groupMemberQry = groupMemberQry.Where( g => g.Group.GroupTypeId == groupRequirement.GroupTypeId );
            }
            else
            {
                // shouldn't happen, but grouprequirement doesn't have a groupId or a GroupTypeId
                return;
            }

            if ( this.GroupRoleId != null )
            {
                groupMemberQry = groupMemberQry.Where( a => a.GroupRoleId == this.GroupRoleId );
            }

            if ( this.AppliesToAgeClassification == AppliesToAgeClassification.Adults )
            {
                groupMemberQry = groupMemberQry.Where( a => a.Person.AgeClassification == AgeClassification.Adult );
            }
            else if ( this.AppliesToAgeClassification == AppliesToAgeClassification.Children )
            {
                groupMemberQry = groupMemberQry.Where( a => a.Person.AgeClassification == AgeClassification.Child );
            }

            // just in case the same person is in the same group multiple times, get a list of the groupMember records for this person
            foreach ( var groupMemberId in groupMemberQry.Select( a => a.Id ) )
            {
                var groupMemberRequirement = groupMemberRequirementService.Queryable().Where( a => a.GroupMemberId == groupMemberId && a.GroupRequirementId == groupRequirement.Id ).FirstOrDefault();
                if ( groupMemberRequirement == null )
                {
                    groupMemberRequirement = new GroupMemberRequirement();
                    groupMemberRequirement.GroupMemberId = groupMemberId;
                    groupMemberRequirement.GroupRequirementId = groupRequirement.Id;
                    groupMemberRequirementService.Add( groupMemberRequirement );
                }

                groupMemberRequirement.LastRequirementCheckDateTime = currentDateTime;

                if ( meetsGroupRequirement == MeetsGroupRequirement.Meets )
                {
                    // They meet the requirement so update the Requirement Met Date/Time.
                    groupMemberRequirement.RequirementMetDateTime = currentDateTime;
                    groupMemberRequirement.RequirementFailDateTime = null;
                    groupMemberRequirement.RequirementWarningDateTime = null;
                }
                else if ( meetsGroupRequirement == MeetsGroupRequirement.MeetsWithWarning )
                {
                    if ( !groupMemberRequirement.RequirementWarningDateTime.HasValue )
                    {
                        // They have a warning for the requirement, and didn't have a warning already.
                        groupMemberRequirement.RequirementWarningDateTime = currentDateTime;
                    }
                }
                else
                {
                    // They don't meet the requirement so set the Requirement Met and the Warning Date/Time to null.
                    groupMemberRequirement.RequirementMetDateTime = null;
                    groupMemberRequirement.RequirementWarningDateTime = null;
                    groupMemberRequirement.RequirementFailDateTime = currentDateTime;
                }
            }
        }

        /// <summary>
        /// Calculates the due date if the properties allow it.
        /// </summary>
        /// <param name="dueDateType"></param>
        /// <param name="dueDateOffsetInDays"></param>
        /// <param name="dueDateStaticDate"></param>
        /// <param name="dueDateFromGroupAttribute"></param>
        /// <param name="dueDateGroupMemberAdded"></param>
        /// <returns></returns>
        public DateTime? CalculateGroupMemberRequirementDueDate( DueDateType dueDateType, int? dueDateOffsetInDays, DateTime? dueDateStaticDate, DateTime? dueDateFromGroupAttribute, DateTime? dueDateGroupMemberAdded )
        {
            switch ( dueDateType )
            {
                case DueDateType.ConfiguredDate:
                    if ( dueDateStaticDate.HasValue )
                    {
                        return dueDateStaticDate.Value;
                    }

                    return null;
                case DueDateType.GroupAttribute:
                    if ( dueDateFromGroupAttribute.HasValue )
                    {
                        return dueDateFromGroupAttribute.Value.AddDays( dueDateOffsetInDays.HasValue ? dueDateOffsetInDays.Value : 0 );
                    }

                    return null;
                case DueDateType.DaysAfterJoining:
                    if ( dueDateGroupMemberAdded.HasValue )
                    {
                        return dueDateGroupMemberAdded.Value.AddDays( dueDateOffsetInDays.HasValue ? dueDateOffsetInDays.Value : 0 );
                    }

                    return null;

                case DueDateType.Immediate:
                default:
                    return null;
            }
        }

        #endregion
    }
}
