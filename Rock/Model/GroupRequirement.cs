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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    ///
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupRequirement" )]
    [DataContract]
    public partial class GroupRequirement : Model<GroupRequirement>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember]
        [Index( "IDX_GroupRequirementTypeGroup", IsUnique = true, Order = 0 )]
        [IgnoreCanDelete]
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the group type identifier.
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        [DataMember]
        [Index( "IDX_GroupRequirementTypeGroup", IsUnique = true, Order = 1 )]
        [IgnoreCanDelete]
        public int? GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the group requirement type identifier.
        /// </summary>
        /// <value>
        /// The group requirement type identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IDX_GroupRequirementTypeGroup", IsUnique = true, Order = 2 )]
        public int GroupRequirementTypeId { get; set; }

        /// <summary>
        /// The specific GroupRoleId that this requirement is for. NULL means this requirement applies to all roles.
        /// </summary>
        /// <value>
        /// The group role identifier.
        /// </value>
        [DataMember]
        [Index( "IDX_GroupRequirementTypeGroup", IsUnique = true, Order = 3 )]
        public int? GroupRoleId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a member must meet this requirement before adding (only applies to DataView and SQL RequirementCheckType)
        /// </summary>
        /// <value>
        /// <c>true</c> if [must meet requirement to add member]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool MustMeetRequirementToAddMember { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [LavaInclude]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the type of the group.
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        [LavaInclude]
        public virtual GroupType GroupType { get; set; }

        /// <summary>
        /// Gets or sets the type of the group requirement.
        /// </summary>
        /// <value>
        /// The type of the group requirement.
        /// </value>
        [DataMember]
        public virtual GroupRequirementType GroupRequirementType { get; set; }

        /// <summary>
        /// The specific Group Role that this requirement is for. NULL means this requirement applies to all roles.
        /// </summary>
        /// <value>
        /// The group type role.
        /// </value>
        [LavaInclude]
        public virtual GroupTypeRole GroupRole { get; set; }

        #endregion

        #region Public Methods

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
        /// Returns a list of each person and their GroupRequiremnt status for this group requirement
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
                // if this GroupRequirement is for a specific role, the groupRole we are checking for is something different
                var result = personQry.Select( p => p.Id ).ToList().Select( a =>
                     new PersonGroupRequirementStatus
                     {
                         PersonId = Id,
                         GroupRequirement = this,
                         MeetsGroupRequirement = MeetsGroupRequirement.NotApplicable
                     } );

                return result;
            }

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
                            var personGroupRequirementStatus = new PersonGroupRequirementStatus
                            {
                                PersonId = a.PersonId,
                                GroupRequirement = this
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
                                }
                            }
                            else
                            {
                                personGroupRequirementStatus.MeetsGroupRequirement = MeetsGroupRequirement.NotMet;
                            }

                            return personGroupRequirementStatus;
                        }
                        );

                        return result;
                    }
                }
                else
                {
                    var personWithIdRequirements = personQry.Select( p => p.Id );

                    var result = personWithIdRequirements.ToList().Select( a =>
                            new PersonGroupRequirementStatus
                            {
                                PersonId = a,
                                GroupRequirement = this,
                                MeetsGroupRequirement = warningDataViewPersonIdList.Contains( a ) == true ? MeetsGroupRequirement.MeetsWithWarning : MeetsGroupRequirement.Meets
                            } );

                    return result;

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

                        var result = personQryIdList.Select( a => new PersonGroupRequirementStatus
                        {
                            PersonId = a,
                            GroupRequirement = this,
                            MeetsGroupRequirement = personIds.Contains( a )
                                    ? ( ( warningPersonIds != null && warningPersonIds.Contains( a ) )
                                        ? MeetsGroupRequirement.MeetsWithWarning
                                        : MeetsGroupRequirement.Meets
                                        )
                                    : MeetsGroupRequirement.NotMet,
                        } );

                        return result;
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

                    return result;
                }
            }
            else
            {
                // manual
                var groupMemberRequirementQry = new GroupMemberRequirementService( rockContext ).Queryable().Where( a => a.GroupMember.GroupId == groupId && a.GroupRequirementId == this.Id && a.RequirementMetDateTime.HasValue );

                var result = personQry.ToList().Select( a =>
                    new PersonGroupRequirementStatus
                    {
                        PersonId = a.Id,
                        GroupRequirement = this,
                        MeetsGroupRequirement = groupMemberRequirementQry.Any( r => r.GroupMember.PersonId == a.Id ) ? MeetsGroupRequirement.Meets : MeetsGroupRequirement.NotMet
                    } );

                return result;
            }

            // shouldn't happen
            return null;
        }
     

        /// <summary>
        /// Check if the Person meets the group requirement for the role
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <returns></returns>
        public PersonGroupRequirementStatus PersonMeetsGroupRequirement( int personId, int groupId, int? groupRoleId )
        {
            var rockContext = new RockContext();
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

                if ( ( meetsGroupRequirement == MeetsGroupRequirement.Meets ) || ( meetsGroupRequirement == MeetsGroupRequirement.MeetsWithWarning ) )
                {
                    // they meet the requirement so update the Requirement Met Date/Time
                    groupMemberRequirement.RequirementMetDateTime = currentDateTime;
                    groupMemberRequirement.RequirementFailDateTime = null;
                }
                else
                {
                    // they don't meet the requirement so set the Requirement Met Date/Time to null
                    groupMemberRequirement.RequirementMetDateTime = null;
                    groupMemberRequirement.RequirementFailDateTime = currentDateTime;
                }

                if ( meetsGroupRequirement == MeetsGroupRequirement.MeetsWithWarning )
                {
                    if ( !groupMemberRequirement.RequirementWarningDateTime.HasValue )
                    {
                        // they have a warning for the requirement, and didn't have a warning already
                        groupMemberRequirement.RequirementWarningDateTime = currentDateTime;
                    }
                }
                else
                {
                    // no warning, so set to null
                    groupMemberRequirement.RequirementWarningDateTime = null;
                }
            }
        }

        #endregion
    }

    #region enum

    /// <summary>
    ///
    /// </summary>
    public enum MeetsGroupRequirement
    {
        /// <summary>
        ///  Meets requirements
        /// </summary>
        Meets,

        /// <summary>
        /// Doesn't meet requirements
        /// </summary>
        NotMet,

        /// <summary>
        /// The meets with warning
        /// </summary>
        MeetsWithWarning,

        /// <summary>
        /// The Requirement doesn't apply for the GroupRole we are checking against
        /// </summary>
        NotApplicable,

        /// <summary>
        /// The Requirement calculation resulted in an exception <see cref="GroupRequirementStatus.CalculationException"/>
        /// </summary>
        Error
    }

    #endregion

    #region GroupRequirement classes

    /// <summary>
    ///
    /// </summary>
    public class PersonGroupRequirementStatus : GroupRequirementStatus
    {
        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int PersonId { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class GroupRequirementStatus
    {
        /// <summary>
        /// Gets or sets the group requirement.
        /// </summary>
        /// <value>
        /// The group requirement.
        /// </value>
        public GroupRequirement GroupRequirement { get; set; }

        /// <summary>
        /// Gets or sets the meets group requirement.
        /// </summary>
        /// <value>
        /// The meets group requirement.
        /// </value>
        public MeetsGroupRequirement MeetsGroupRequirement { get; set; }

        /// <summary>
        /// Gets or sets the requirement warning date time.
        /// </summary>
        /// <value>
        /// The requirement warning date time.
        /// </value>
        public DateTime? RequirementWarningDateTime { get; set; }

        /// <summary>
        /// Gets or sets the last requirement check date time.
        /// </summary>
        /// <value>
        /// The last requirement check date time.
        /// </value>
        public DateTime? LastRequirementCheckDateTime { get; set; }

        /// <summary>
        /// Gets or sets the calculation exception.
        /// </summary>
        /// <value>
        /// The calculation exception.
        /// </value>
        public Exception CalculationException { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0}:{1}", this.GroupRequirement, this.MeetsGroupRequirement );
        }
    }

    #endregion

    #region Entity Configuration

    /// <summary>
    ///
    /// </summary>
    public partial class GroupRequirementConfiguration : EntityTypeConfiguration<GroupRequirement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupRequirementConfiguration"/> class.
        /// </summary>
        public GroupRequirementConfiguration()
        {
            // NOTE: would be nice if this would cascade delete, but doing so results in a "may cause cycles or multiple cascade paths" error
            this.HasOptional( a => a.Group ).WithMany( g => g.GroupRequirements ).HasForeignKey( a => a.GroupId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.GroupType ).WithMany( gt => gt.GroupRequirements ).HasForeignKey( a => a.GroupTypeId ).WillCascadeOnDelete( false );

            this.HasRequired( a => a.GroupRequirementType ).WithMany().HasForeignKey( a => a.GroupRequirementTypeId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.GroupRole ).WithMany().HasForeignKey( a => a.GroupRoleId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}
