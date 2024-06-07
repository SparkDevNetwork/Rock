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
using System.Data.Entity;
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
                var appliesToDataViewPersonService = new PersonService( rockContext );
                var appliesToDataViewParamExpression = appliesToDataViewPersonService.ParameterExpression;
                var appliesToDataViewWhereExpression = this.AppliesToDataView.GetExpression( appliesToDataViewPersonService, appliesToDataViewParamExpression );
                var appliesToDataViewPersonIds = appliesToDataViewPersonService.Get( appliesToDataViewParamExpression, appliesToDataViewWhereExpression ).Select( p => p.Id );

                // If the dataview does not contain anyone in the person query, give the member a "not applicable" status.
                var notApplicablePersonQry = personQry.Where( p => !appliesToDataViewPersonIds.Contains( p.Id ) );
                {
                    var results = notApplicablePersonQry.Select( p => p.Id ).ToList().Select( a =>
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
            }

            var attributeValueService = new AttributeValueService( rockContext );

            if ( this.GroupRequirementType.RequirementCheckType == RequirementCheckType.Dataview )
            {
                var personService = new PersonService( rockContext );
                var paramExpression = personService.ParameterExpression;
                List<int> warningDataViewPersonIdList = null;
                if ( this.GroupRequirementType.WarningDataViewId.HasValue )
                {
                    var warningDataViewWhereExpression = this.GroupRequirementType.WarningDataView.GetExpression( personService, paramExpression );
                    warningDataViewPersonIdList = personService.Get( paramExpression, warningDataViewWhereExpression ).Where( a => personQry.Any( p => p.Id == a.Id ) ).Select( a => a.Id ).ToList();
                }

                if ( this.GroupRequirementType.DataViewId.HasValue )
                {
                    var dataViewWhereExpression = this.GroupRequirementType.DataView.GetExpression( personService, paramExpression );
                    var dataViewQry = personService.Get( paramExpression, dataViewWhereExpression );
                    if ( dataViewQry != null )
                    {
                        var personWithRequirementsQuery = from p in personQry
                                                          join d in dataViewQry on p.Id equals d.Id into oj
                                                          from d in oj.DefaultIfEmpty()
                                                          select new { PersonId = p.Id, Included = d != null };

                        var personWithRequirementsList = personWithRequirementsQuery.Select( p => new { PersonId = p.PersonId, Included = p.Included } ).ToList();

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

                            var hasWarning = warningDataViewPersonIdList?.Contains( a.PersonId ) == true;

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
                            MeetsGroupRequirement = warningDataViewPersonIdList?.Contains( a ) == true ? MeetsGroupRequirement.MeetsWithWarning : MeetsGroupRequirement.Meets,
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
        /// Returns a list of each person and their group requirement status for each group requirement, using cached data view results when possible.
        /// <para>
        /// IMPORTANT: This method should only be used for batch processes, where it's understood that the underlying data view
        /// results (i.e. to determine if a person currently meets a given group requirement) might be slightly out of date (likely
        /// by no more than 5 minutes). If you need to be 100% certain whether a person meets group requirements in this exact moment,
        /// call the <see cref="PersonQueryableMeetsGroupRequirement(RockContext, IQueryable{Person}, int, int?)"/> method instead.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="personQry">The person qry.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <param name="shouldLoadGroupMemberRequirementIds">Whether the existing group member requirement identifier - if any - should be loaded for each status.</param>
        /// <returns>A list of each person and their group requirement status for each group requirement.</returns>
        internal IEnumerable<PersonGroupRequirementStatus> PersonQueryableMeetsGroupRequirementUsingDataViewCache( RockContext rockContext, IQueryable<Person> personQry, int groupId, int? groupRoleId, bool shouldLoadGroupMemberRequirementIds = false )
        {
            if ( ( this.GroupRoleId != null ) && ( groupRoleId != null ) && ( this.GroupRoleId != groupRoleId ) )
            {
                // If this GroupRequirement is for a specific group role, and the groupRole we are comparing is not the same role,
                // return a "Not Applicable" status for all candidates.
                var notApplicableStatuses = personQry.Select( p => p.Id ).ToList().Select( id =>
                     new PersonGroupRequirementStatus
                     {
                         PersonId = id,
                         GroupRequirement = this,
                         MeetsGroupRequirement = MeetsGroupRequirement.NotApplicable
                     } );

                return notApplicableStatuses;
            }

            // Create a list of statuses so that not applicable status results can be included.
            List<PersonGroupRequirementStatus> statusResults = new List<PersonGroupRequirementStatus>();

            // When comparing against cached data view results, we'll eagerly-load the candidate person IDs
            // at the first appropriate spot below, then continue to refine and compare against this same
            // in-memory list of IDs throughout the method.
            HashSet<int> candidatePersonIds = null;

            // If this GroupRequirement is not for "All" age classifications, separate the candidates into
            // groups of people who do fit the age classification and those who don't.
            if ( this.AppliesToAgeClassification != AppliesToAgeClassification.All )
            {
                var candidatePeopleWithAgeClassifications = personQry.Select( p => new
                {
                    p.Id,
                    p.AgeClassification
                } )
                .ToList();

                // If the person's age classification we are comparing is not the one applied to this requirement,
                // then mark that person's requirement status as "Not Applicable".
                var notApplicableStatusResults = candidatePeopleWithAgeClassifications
                    .Where( p => ( int ) p.AgeClassification != ( int ) this.AppliesToAgeClassification )
                    .Select( p => new PersonGroupRequirementStatus
                    {
                        PersonId = p.Id,
                        GroupRequirement = this,
                        MeetsGroupRequirement = MeetsGroupRequirement.NotApplicable
                    } )
                    .ToList();

                if ( notApplicableStatusResults.Any() )
                {
                    statusResults.AddRange( notApplicableStatusResults );
                }

                // Filter down to only those candidates who fit the age classification.
                candidatePersonIds = candidatePeopleWithAgeClassifications
                    .Where( p => ( int ) p.AgeClassification == ( int ) this.AppliesToAgeClassification )
                    .Select( p => p.Id )
                    .ToHashSet();

                // If none of the candidates fit the age classification, return the current list of statuses.
                if ( !candidatePersonIds.Any() )
                {
                    return statusResults;
                }
            }

            // If the Group Requirement has a Data View it applies to, apply it here.
            if ( this.AppliesToDataViewId.HasValue )
            {
                var appliesToDataViewCache = DataViewCache.Get( this.AppliesToDataViewId.Value );
                var appliesToDataViewPersonIds = appliesToDataViewCache.GetVolatileEntityIds().ToHashSet();

                // If we haven't yet queried the database for the candidate person IDs, do so now.
                if ( candidatePersonIds == null )
                {
                    candidatePersonIds = personQry.Select( a => a.Id ).ToHashSet();
                }

                // For all candidates that are not represented in the "Applies To" data view, add a "Not Applicable" status result.
                var notApplicableStatusResults = candidatePersonIds
                    .Except( appliesToDataViewPersonIds )
                    .Select( id => new PersonGroupRequirementStatus
                    {
                        PersonId = id,
                        GroupRequirement = this,
                        MeetsGroupRequirement = MeetsGroupRequirement.NotApplicable
                    } )
                    .ToList();

                if ( notApplicableStatusResults.Any() )
                {
                    statusResults.AddRange( notApplicableStatusResults );
                }

                // Filter down to only those candidates in the "Applies To" data view.
                candidatePersonIds = candidatePersonIds.Intersect( appliesToDataViewPersonIds ).ToHashSet();

                // If none of the candidates are in the "Applies To" data view, return the current list of statuses.
                if ( !candidatePersonIds.Any() )
                {
                    return statusResults;
                }
            }

            // If we haven't yet queried the database for the candidate person IDs, do so now.
            if ( candidatePersonIds == null )
            {
                candidatePersonIds = personQry.Select( p => p.Id ).ToHashSet();
            }

            // If we don't have any candidates, return the empty list of statuses.
            if ( !candidatePersonIds.Any() )
            {
                return statusResults;
            }

            // If this requirement's due date is based on a group attribute value, get it now, to be used when looping below.
            DateTime? dueDateFromGroupAttribute = null;
            if ( this.GroupRequirementType.DueDateType == DueDateType.GroupAttribute && this.DueDateAttributeId.HasValue )
            {
                dueDateFromGroupAttribute = new AttributeValueService( rockContext )
                    .GetByAttributeIdAndEntityId( this.DueDateAttributeId.Value, this.GroupId )
                    ?.Value.AsDateTime() ?? null;
            }

            // Local function to get the date time a person was added to the group, if this requirement's due date is
            // based on a member's days after joining. This is a possible opportunity for further performance improvement
            // within this method; maybe we can get these date time values in bulk, when querying for the candidate person
            // IDs above, rather than making a separate, per-person query when looping over the candidates.
            var groupMemberService = new GroupMemberService( rockContext );
            DateTime? GetGroupMemberDateTimeAdded( int personId )
            {
                if ( this.GroupRequirementType.DueDateType != DueDateType.DaysAfterJoining )
                {
                    return null;
                }

                return groupMemberService.Queryable()
                    .Where( gm =>
                        gm.GroupId == groupId
                        && gm.PersonId == personId
                        && gm.GroupRoleId == groupRoleId
                    )
                    .Select( gm => gm.DateTimeAdded )
                    .DefaultIfEmpty( null )
                    .FirstOrDefault();
            }

            var groupMemberRequirementService = new GroupMemberRequirementService( rockContext );

            // Loop through each candidate and determine:
            //  1) if they already meet the requirement with no warnings;
            //  2) if they already meet the requirement, but are also in a warning state;
            //  3) if they don't meet the requirement, but the due date hasn't yet passed (which puts them in a warning state);
            //  4) if they simply don't meet the requirement.

            if ( this.GroupRequirementType.RequirementCheckType == RequirementCheckType.Dataview )
            {
                // If a warning data view was specified, get a list of person IDs that should have a warning with their status.
                HashSet<int> warningDataViewPersonIds = null;
                if ( this.GroupRequirementType.WarningDataViewId.HasValue )
                {
                    var warningDataViewCache = DataViewCache.Get( this.GroupRequirementType.WarningDataViewId.Value );
                    warningDataViewPersonIds = warningDataViewCache.GetVolatileEntityIds().ToHashSet();
                }

                if ( this.GroupRequirementType.DataViewId.HasValue )
                {
                    var groupRequirementDataViewCache = DataViewCache.Get( this.GroupRequirementType.DataViewId.Value );
                    var groupRequirementDataViewPersonIds = groupRequirementDataViewCache.GetVolatileEntityIds().ToHashSet();

                    var dataViewStatusResults = candidatePersonIds.Select( personId =>
                    {
                        int? groupMemberRequirementId = null;
                        if ( shouldLoadGroupMemberRequirementIds )
                        {
                            groupMemberRequirementId = groupMemberRequirementService.GetIdByPersonIdRequirementIdGroupIdGroupRoleId( personId, this.Id, groupId, groupRoleId );
                        }

                        var personGroupRequirementStatus = new PersonGroupRequirementStatus
                        {
                            PersonId = personId,
                            GroupRequirement = this,
                            GroupMemberRequirementId = groupMemberRequirementId
                        };

                        if ( groupRequirementDataViewPersonIds.Contains( personId ) )
                        {
                            // This person meets the requirement, but they might still be in a warning state.
                            if ( warningDataViewPersonIds?.Contains( personId ) == true )
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
                            // This person doesn't meet the requirement. Perform some final checks based on due date to determine
                            // if they should be reported as meeting with warning, or simply not meeting the requirement.
                            var isRequirementDue = true;
                            if ( this.GroupRequirementType.DueDateType != DueDateType.Immediate )
                            {
                                var possibleDueDate = CalculateGroupMemberRequirementDueDate(
                                    this.GroupRequirementType.DueDateType,
                                    this.GroupRequirementType.DueDateOffsetInDays,
                                    this.DueDateStaticDate,
                                    dueDateFromGroupAttribute,
                                    GetGroupMemberDateTimeAdded( personId )
                                );

                                personGroupRequirementStatus.RequirementDueDate = possibleDueDate;

                                isRequirementDue = !possibleDueDate.HasValue || possibleDueDate <= RockDateTime.Now;
                            }

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
                    } ).ToList();

                    if ( dataViewStatusResults.Any() )
                    {
                        statusResults.AddRange( dataViewStatusResults );
                    }

                    return statusResults;
                }
                else
                {
                    // Even though this requirement is supposed to be based on a data view, the data view's ID hasn't
                    // been defined. Perform a simple comparison against the warning data view - if defined - instead.
                    var dataViewStatusResults = candidatePersonIds.Select( personId =>
                    {
                        int? groupMemberRequirementId = null;
                        if ( shouldLoadGroupMemberRequirementIds )
                        {
                            groupMemberRequirementId = groupMemberRequirementService.GetIdByPersonIdRequirementIdGroupIdGroupRoleId( personId, this.Id, groupId, groupRoleId );
                        }

                        var personGroupRequirementStatus = new PersonGroupRequirementStatus
                        {
                            PersonId = personId,
                            GroupRequirement = this,
                            GroupMemberRequirementId = groupMemberRequirementId
                        };

                        if ( warningDataViewPersonIds?.Contains( personId ) == true )
                        {
                            personGroupRequirementStatus.MeetsGroupRequirement = MeetsGroupRequirement.MeetsWithWarning;
                        }
                        else
                        {
                            personGroupRequirementStatus.MeetsGroupRequirement = MeetsGroupRequirement.Meets;
                            personGroupRequirementStatus.RequirementWarningDateTime = null;
                        }

                        return personGroupRequirementStatus;
                    } ).ToList();

                    if ( dataViewStatusResults.Any() )
                    {
                        statusResults.AddRange( dataViewStatusResults );
                    }

                    return statusResults;
                }
            }
            else if ( this.GroupRequirementType.RequirementCheckType == RequirementCheckType.Sql )
            {
                // If requirement is set on GroupType, this.Group is null.
                var targetGroup = this.Group ?? new GroupService( rockContext ).GetNoTracking( groupId );

                Person personMergeField = null;
                if ( candidatePersonIds.Count == 1 )
                {
                    var personId = candidatePersonIds.First();
                    personMergeField = new PersonService( rockContext ).GetNoTracking( personId );
                }

                string formattedSql = this.GroupRequirementType.SqlExpression.ResolveMergeFields( this.GroupRequirementType.GetMergeObjects( targetGroup, personMergeField ) );
                string warningFormattedSql = this.GroupRequirementType.WarningSqlExpression.ResolveMergeFields( this.GroupRequirementType.GetMergeObjects( targetGroup, personMergeField ) );

                try
                {
                    var tableResult = DbService.GetDataTable( formattedSql, System.Data.CommandType.Text, null );
                    if ( tableResult.Columns.Count > 0 )
                    {
                        var groupRequirementSqlPersonIds = tableResult.Rows.OfType<System.Data.DataRow>().Select( r => Convert.ToInt32( r[0] ) ).ToHashSet();

                        // If a warning SQL expression was specified, get a list of person IDs that should have a warning with their status.
                        HashSet<int> warningSqlPersonIds = null;
                        if ( !string.IsNullOrWhiteSpace( warningFormattedSql ) )
                        {
                            var warningTableResult = DbService.GetDataTable( warningFormattedSql, System.Data.CommandType.Text, null );
                            if ( warningTableResult.Columns.Count > 0 )
                            {
                                warningSqlPersonIds = warningTableResult.Rows.OfType<System.Data.DataRow>().Select( r => Convert.ToInt32( r[0] ) ).ToHashSet();
                            }
                        }

                        var sqlStatusResults = candidatePersonIds.Select( personId =>
                        {
                            int? groupMemberRequirementId = null;
                            if ( shouldLoadGroupMemberRequirementIds )
                            {
                                groupMemberRequirementId = groupMemberRequirementService.GetIdByPersonIdRequirementIdGroupIdGroupRoleId( personId, this.Id, groupId, groupRoleId );
                            }

                            var personGroupRequirementStatus = new PersonGroupRequirementStatus
                            {
                                PersonId = personId,
                                GroupRequirement = this,
                                GroupMemberRequirementId = groupMemberRequirementId
                            };

                            if ( groupRequirementSqlPersonIds.Contains( personId ) )
                            {
                                // This person meets the requirement, but they might still be in a warning state.
                                if ( warningSqlPersonIds?.Contains( personId ) == true )
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
                                // This person doesn't meet the requirement. Perform some final checks based on due date to determine
                                // if they should be reported as meeting with warning, or simply not meeting the requirement.
                                var isRequirementDue = true;
                                if ( this.GroupRequirementType.DueDateType != DueDateType.Immediate )
                                {
                                    var possibleDueDate = CalculateGroupMemberRequirementDueDate(
                                        this.GroupRequirementType.DueDateType,
                                        this.GroupRequirementType.DueDateOffsetInDays,
                                        this.DueDateStaticDate,
                                        dueDateFromGroupAttribute,
                                        GetGroupMemberDateTimeAdded( personId )
                                    );

                                    personGroupRequirementStatus.RequirementDueDate = possibleDueDate;

                                    isRequirementDue = !possibleDueDate.HasValue || possibleDueDate <= RockDateTime.Now;
                                }

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
                        } ).ToList();

                        if ( sqlStatusResults.Any() )
                        {
                            statusResults.AddRange( sqlStatusResults );
                        }

                        return statusResults;
                    }
                }
                catch ( Exception ex )
                {
                    // Exception occurred (probably due to bad SQL).
                    ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );

                    var sqlStatusResults = candidatePersonIds.Select( personId => new PersonGroupRequirementStatus
                    {
                        PersonId = personId,
                        GroupRequirement = this,
                        MeetsGroupRequirement = MeetsGroupRequirement.Error,
                        CalculationException = ex
                    } ).ToList();

                    if ( sqlStatusResults.Any() )
                    {
                        statusResults.AddRange( sqlStatusResults );
                    }

                    return statusResults;
                }
            }
            else // Manual
            {
                // This is a possible opportunity for further performance improvement within this method. Maybe we
                // can get these values in bulk, when querying for the candidate person IDs above, rather than
                // making a separate query here.
                var groupMemberRequirements = groupMemberRequirementService
                    .Queryable()
                    .AsNoTracking()
                    .Where( gmr =>
                        gmr.GroupMember.GroupId == groupId
                        && gmr.GroupRequirementId == this.Id
                    )
                    .Select( gmr => new
                    {
                        GroupMemberRequirementId = gmr.Id,
                        gmr.GroupMember.PersonId,
                        gmr.RequirementMetDateTime
                    } )
                    .ToList();

                var manualStatusResults = candidatePersonIds.Select( personId =>
                {
                    var groupMemberRequirement = groupMemberRequirements.FirstOrDefault( gmr => gmr.PersonId == personId );

                    var personGroupRequirementStatus = new PersonGroupRequirementStatus
                    {
                        PersonId = personId,
                        GroupRequirement = this,
                        GroupMemberRequirementId = shouldLoadGroupMemberRequirementIds ? groupMemberRequirement?.GroupMemberRequirementId : null
                    };

                    var requirementMetDateTime = groupMemberRequirement?.RequirementMetDateTime;
                    if ( requirementMetDateTime.HasValue )
                    {
                        personGroupRequirementStatus.MeetsGroupRequirement = MeetsGroupRequirement.Meets;
                        return personGroupRequirementStatus;
                    }

                    // This person doesn't meet the requirement. Perform some final checks based on due date to determine
                    // if they should be reported as meeting with warning, or simply not meeting the requirement.
                    var isRequirementDue = true;
                    if ( this.GroupRequirementType.DueDateType != DueDateType.Immediate )
                    {
                        var possibleDueDate = CalculateGroupMemberRequirementDueDate(
                            this.GroupRequirementType.DueDateType,
                            this.GroupRequirementType.DueDateOffsetInDays,
                            this.DueDateStaticDate,
                            dueDateFromGroupAttribute,
                            GetGroupMemberDateTimeAdded( personId )
                        );

                        personGroupRequirementStatus.RequirementDueDate = possibleDueDate;

                        isRequirementDue = !possibleDueDate.HasValue || possibleDueDate <= RockDateTime.Now;
                    }

                    if ( !isRequirementDue )
                    {
                        personGroupRequirementStatus.MeetsGroupRequirement = MeetsGroupRequirement.MeetsWithWarning;
                    }
                    else
                    {
                        personGroupRequirementStatus.MeetsGroupRequirement = MeetsGroupRequirement.NotMet;
                    }

                    return personGroupRequirementStatus;
                } ).ToList();

                if ( manualStatusResults.Any() )
                {
                    statusResults.AddRange( manualStatusResults );
                }

                return statusResults;
            }

            // This shouldn't happen, since a requirement must have a data view, SQL or manual origin.
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

            if ( personId > 0 && groupId > 0 )
            {
                var metRequirement = new GroupMemberRequirementService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .FirstOrDefault( gmr =>
                        gmr.GroupRequirementId == this.Id
                        && gmr.GroupMember.GroupId == groupId
                        && gmr.GroupMember.PersonId == personId
                        && (
                            gmr.RequirementMetDateTime.HasValue
                            || gmr.WasManuallyCompleted
                            || gmr.WasOverridden
                        )
                        && (
                            !gmr.GroupRequirement.GroupRoleId.HasValue
                            || (
                                groupRoleId.HasValue
                                && gmr.GroupRequirement.GroupRoleId == groupRoleId.Value
                            )
                        )
                    );

                if ( metRequirement != null )
                {
                    return new PersonGroupRequirementStatus
                    {
                        GroupRequirement = this,
                        MeetsGroupRequirement = MeetsGroupRequirement.Meets,
                        GroupMemberRequirementId = metRequirement.Id,
                        PersonId = personId
                    };
                }
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
        /// Updates the group member requirement result (does not save the context).
        /// <para>
        /// If the group member requirement in question does not already exist in the database, a new instance
        /// will be created as a proxy and added to the rock context, so navigation properties (lazy loading)
        /// will work for any code that subsequently loads this same entity instance when using the same rock
        /// context. It's up to the caller of this method to save changes.
        /// </para>
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
                    // Create the group member requirement as a proxy so navigation properties
                    // work for subsequent retrieval of this instance using the same rock context.
                    groupMemberRequirement = rockContext.Set<GroupMemberRequirement>().Create();
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
