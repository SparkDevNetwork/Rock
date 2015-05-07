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
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IDX_GroupRequirementTypeGroup", IsUnique = true, Order = 0 )]
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the group requirement type identifier.
        /// </summary>
        /// <value>
        /// The group requirement type identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IDX_GroupRequirementTypeGroup", IsUnique = true, Order = 1 )]
        public int GroupRequirementTypeId { get; set; }

        /// <summary>
        /// The specific GroupRoleId that this requirement is for. NULL means this requirement applies to all roles.
        /// </summary>
        /// <value>
        /// The group role identifier.
        /// </value>
        [DataMember]
        [Index( "IDX_GroupRequirementTypeGroup", IsUnique = true, Order = 2 )]
        public int? GroupRoleId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        public virtual Group Group { get; set; }

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
            if ( this.GroupRequirementType != null && this.Group != null )
            {
                return string.Format( "{0}|{1}", this.GroupRequirementType, this.Group );
            }
            else
            {
                return base.ToString();
            }
        }

        /// <summary>
        /// Returns a list of each person and their GroupRequiremnt status for this group requirement
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="personQry">A qry containing the people whose requirements should be checked</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <returns></returns>
        public IEnumerable<PersonGroupRequirementStatus> PersonQueryableMeetsGroupRequirement( RockContext rockContext, IQueryable<Person> personQry, int? groupRoleId )
        {
            if ( ( this.GroupRoleId != null ) && ( groupRoleId != null ) && ( this.GroupRoleId != groupRoleId ) )
            {
                // if this GroupRequirement is for a specific role, the groupRole we are checking for is something different
                var result = personQry.ToList().Select( a =>
                    new PersonGroupRequirementStatus
                    {
                        PersonId = a.Id,
                        GroupRequirement = this,
                        MeetsGroupRequirement = MeetsGroupRequirement.NotApplicable
                    } );

                return result;
            }

            if ( this.GroupRequirementType.RequirementCheckType == RequirementCheckType.Dataview )
            {
                if ( this.GroupRequirementType.DataViewId.HasValue )
                {
                    var errorMessages = new List<string>();
                    var personService = new PersonService( rockContext );
                    var paramExpression = personService.ParameterExpression;
                    var dataViewWhereExpression = this.GroupRequirementType.DataView.GetExpression( personService, paramExpression, out errorMessages );
                    var dataViewQry = personService.Get( paramExpression, dataViewWhereExpression );
                    if ( dataViewQry != null )
                    {
                        var join = personQry.Join(
                            dataViewQry.DefaultIfEmpty(),
                            pr => pr.Id,
                            pq => pq.Id,
                            ( pr, pq ) =>
                            new
                            {
                                PersonId = pq.Id,
                                Included = pr != null
                            } );

                        var result = join.ToList().Select( a =>
                            new PersonGroupRequirementStatus
                            {
                                PersonId = a.PersonId,
                                GroupRequirement = this,
                                MeetsGroupRequirement = a.Included ? MeetsGroupRequirement.Meets : MeetsGroupRequirement.NotMet
                            } );

                        return result;
                    }
                }
            }
            else if ( this.GroupRequirementType.RequirementCheckType == RequirementCheckType.Sql )
            {
                string formattedSql = this.GroupRequirementType.SqlExpression.ResolveMergeFields( this.GroupRequirementType.GetMergeObjects( this.Group ) );
                try
                {
                    var tableResult = DbService.GetDataTable( formattedSql, System.Data.CommandType.Text, null );
                    if ( tableResult.Columns.Count > 0 )
                    {
                        var personIds = tableResult.Rows.OfType<System.Data.DataRow>().Select( r => Convert.ToInt32( r[0] ) );

                        var result = personQry.Select( a => a.Id ).ToList().Select( a => new PersonGroupRequirementStatus
                            {
                                PersonId = a,
                                GroupRequirement = this,
                                MeetsGroupRequirement = personIds.Contains( a ) ? MeetsGroupRequirement.Meets : MeetsGroupRequirement.NotMet
                            } );

                        return result;
                    }
                }
                catch (Exception ex)
                {
                    // Exception occurred (probably due to bad SQL)
                    ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
                    
                    var result = personQry.Select( a => a.Id ).ToList().Select( a => new PersonGroupRequirementStatus
                    {
                        PersonId = a,
                        GroupRequirement = this,
                        MeetsGroupRequirement = MeetsGroupRequirement.Error
                    } );

                    return result;
                }
            }
            else
            {
                // manual
                var result = personQry.ToList().Select( a =>
                    new PersonGroupRequirementStatus
                    {
                        PersonId = a.Id,
                        GroupRequirement = this,
                        MeetsGroupRequirement = MeetsGroupRequirement.ManualCheckRequired
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
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <returns></returns>
        public MeetsGroupRequirement PersonMeetsGroupRequirement( int personId, int? groupRoleId )
        {
            var rockContext = new RockContext();
            var personQuery = new PersonService( rockContext ).Queryable().Where( a => a.Id == personId );
            var result = this.PersonQueryableMeetsGroupRequirement( rockContext, personQuery, groupRoleId ).FirstOrDefault();
            if ( result == null )
            {
                // no result. probably because personId was zero
                return MeetsGroupRequirement.NotMet;
            }
            else
            {
                return result.MeetsGroupRequirement;
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
        /// The Requirement doesn't apply for the GroupRole we are checking against
        /// </summary>
        NotApplicable,

        /// <summary>
        /// Person must be added to Group first, then requirement must be manually checked
        /// </summary>
        ManualCheckRequired,

        /// <summary>
        /// The Requirement calculation resulted in an exception
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
            this.HasRequired( a => a.Group ).WithMany( a => a.GroupRequirements ).HasForeignKey( a => a.GroupId ).WillCascadeOnDelete( false );

            this.HasRequired( a => a.GroupRequirementType ).WithMany().HasForeignKey( a => a.GroupRequirementTypeId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.GroupRole ).WithMany().HasForeignKey( a => a.GroupRoleId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}
