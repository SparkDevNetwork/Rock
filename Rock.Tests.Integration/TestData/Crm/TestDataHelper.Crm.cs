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
using System.Collections.Generic;
using Rock.Data;
using Rock.Model;
using System.Linq;
using Rock.Web.Cache;
using System;

namespace Rock.Tests.Integration
{
    public static partial class TestDataHelper
    {
        public static partial class Crm
        {
            #region Lookup Functions

            public static Service<T> GetEntityService<T>( RockContext rockContext = null )
                where T : Rock.Data.Entity<T>, new()
            {
                rockContext = rockContext ?? new RockContext();
                var service = Reflection.GetServiceForEntityType( typeof( T ), rockContext );

                return service as Service<T>;
            }

            public static T GetEntityByIdentifierOrThrow<T>( string identifier, RockContext rockContext = null )
                where T : Rock.Data.Entity<T>, new()
            {
                rockContext = rockContext ?? new RockContext();
                var service = GetEntityService<T>( rockContext );
                
                var entity = service.GetByIdentifierOrThrow( identifier );
                return entity;
            }

            public static Site GetInternalSite( RockContext rockContext = null )
            {
                rockContext = GetActiveRockContext( rockContext );
                var siteService = new SiteService( rockContext );

                var internalSite = siteService.Get( SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid() );
                return internalSite;
            }

            public static List<Page> GetInternalSitePages( RockContext rockContext = null )
            {
                rockContext = GetActiveRockContext( rockContext );
                var pageService = new PageService( rockContext );

                var internalSite = GetInternalSite( rockContext );
                var pages = pageService.Queryable()
                    .Where( p => p.Layout != null && p.Layout.SiteId == internalSite.Id )
                    .ToList();

                return pages;
            }

            #endregion

            #region Group Actions

            public class AddGroupArgs
            {
                //public RockContext DataContext { get; set; }
                public bool ReplaceIfExists { get; set; }
                public string ForeignKey { get; set; }
                public string GroupTypeIdentifier { get; set; }
                public string ParentGroupIdentifier { get; set; }
                public string GroupName { get; set; }
                public string GroupGuid { get; set; }
                public List<GroupMember> GroupMembers { get; set; }
                public string CampusIdentifier { get; set; }

            }

            public class AddGroupActionResult
            {
                public Group Group;
                public int AffectedItemCount;
            }

            /// <summary>
            /// Add a new Group.
            /// </summary>
            /// <param name="args"></param>
            /// <returns></returns>
            public static AddGroupActionResult AddGroup( AddGroupArgs args )
            {
                var result = new AddGroupActionResult();
                Group group = null;

                // Use a new context because the Group will be immediately persisted.
                var rockContext = new RockContext();
                rockContext.WrapTransaction( () =>
                {
                    var groupService = new GroupService( rockContext );
                    var groupTypeService = new GroupTypeService( rockContext );
                    var groupType = groupTypeService.Queryable().GetByIdentifierOrThrow( args.GroupTypeIdentifier );

                    // Get optional parent Group.
                    Guid? parentGroupGuid = null;
                    if ( !string.IsNullOrWhiteSpace( args.ParentGroupIdentifier ) )
                    {
                        parentGroupGuid = groupService.Queryable()
                            .GetByIdentifierOrThrow( args.ParentGroupIdentifier )
                            .Guid;
                    }

                    // Get optional Campus.
                    int? campusId = null;
                    if ( !string.IsNullOrWhiteSpace( args.CampusIdentifier ) )
                    {
                        var campusService = new CampusService( rockContext );
                        campusId = campusService.Queryable()
                            .GetByIdentifier( args.CampusIdentifier )
                            .Id;
                    }

                    var groupGuid = args.GroupGuid.AsGuidOrNull();

                    if ( groupGuid != null )
                    {
                        var existingGroup = groupService.Queryable().FirstOrDefault( g => g.Guid == groupGuid );
                        if ( existingGroup != null )
                        {
                            if ( !args.ReplaceIfExists )
                            {
                                return;
                            }
                            DeleteGroup( rockContext, args.GroupGuid );
                            rockContext.SaveChanges();
                        }
                    }

                    group = GroupService.SaveNewGroup( rockContext,
                        groupType.Id,
                        parentGroupGuid,
                        args.GroupName,
                        args.GroupMembers ?? new List<GroupMember>(),
                        campusId,
                        savePersonAttributes: true );

                    group.Guid = args.GroupGuid.AsGuidOrNull() ?? Guid.NewGuid();
                    group.ForeignKey = args.ForeignKey;

                    rockContext.SaveChanges();
                } );

                result.Group = group;
                result.AffectedItemCount = ( group == null ? 0 : 1 );
                return result;
            }

            public static bool DeleteGroup( RockContext rockContext, string groupIdentifier )
            {
                rockContext.WrapTransaction( () =>
                {
                    var groupService = new GroupService( rockContext );
                    var group = groupService.Get( groupIdentifier );

                    groupService.Delete( group, removeFromAuthTables: true );

                    rockContext.SaveChanges();
                } );

                return true;
            }

            #endregion

            #region Group Member Actions

            public class AddGroupMemberArgs
            {
                public static AddGroupMemberArgs New( string groupIdentifier, string personIdentifiers, string groupRoleIdentifier = null, bool replaceIfExists = false )
                {
                    var args = new AddGroupMemberArgs
                    {
                        GroupIdentifier = groupIdentifier,
                        PersonIdentifiers = personIdentifiers,
                        GroupRoleIdentifier = groupRoleIdentifier,
                        ReplaceIfExists = replaceIfExists
                    };

                    args.PersonIdentifiers = personIdentifiers;
                    return args;
                }

                public bool ReplaceIfExists { get; set; }
                public string ForeignKey { get; set; }
                public string GroupIdentifier { get; set; }
                public string PersonIdentifiers { get; set; }
                public string GroupRoleIdentifier { get; set; }
            }

            public static List<GroupMember> AddGroupMembers( RockContext rockContext, AddGroupMemberArgs args )
            {
                var groupMembers = new List<GroupMember>();

                rockContext.WrapTransaction( () =>
                {
                    var groupService = new GroupService( rockContext );
                    var groupRoleService = new GroupService( rockContext );

                    var group = groupService.Get( args.GroupIdentifier );
                    AssertRockEntityIsNotNull( group, args.GroupIdentifier );

                    var groupId = group.Id;
                    var groupGuid = group.Guid;
                    var groupTypeId = group.GroupTypeId;

                    var roleId = args.GroupRoleIdentifier.AsIntegerOrNull() ?? 0;
                    var roleGuid = args.GroupRoleIdentifier.AsGuidOrNull();

                    var groupTypeRoleService = new GroupTypeRoleService( rockContext );
                    var role = groupTypeRoleService.Queryable()
                        .FirstOrDefault( r => ( r.GroupTypeId == groupTypeId )
                            && ( r.Id == roleId || r.Guid == roleGuid || r.Name == args.GroupRoleIdentifier ) );
                    AssertRockEntityIsNotNull( role, args.GroupRoleIdentifier );

                    var personService = new PersonService( rockContext );
                    var groupMemberService = new GroupMemberService( rockContext );

                    var personIdentifierList = args.PersonIdentifiers.SplitDelimitedValues( "," );
                    foreach ( var personIdentifier in personIdentifierList )
                    {
                        var person = personService.Get( personIdentifier );
                        AssertRockEntityIsNotNull( person, personIdentifier );

                        var groupMember = new GroupMember
                        {
                            ForeignKey = args.ForeignKey,
                            GroupId = groupId,
                            GroupTypeId = groupTypeId,
                            PersonId = person.Id,
                            GroupRoleId = role.Id
                        };

                        groupMemberService.Add( groupMember );
                        groupMembers.Add( groupMember );
                    }
                    //rockContext.SaveChanges( disablePrePostProcessing:true );
                } );

                return groupMembers;
            }

            #endregion

            #region Group Requirements

            public class AddGroupRequirementToGroupArgs : AddGroupRequirementArgsBase
            {
                public string GroupIdentifier { get; set; }
            }

            public class AddGroupRequirementToGroupTypeArgs : AddGroupRequirementArgsBase
            {
                public string GroupTypeIdentifier { get; set; }
            }

            public class AddGroupRequirementArgsBase
            {
                public Guid? GroupRequirementGuid { get; set; }

                public bool ReplaceIfExists { get; set; }
                public string ForeignKey { get; set; }

                public string GroupRoleIdentifier { get; set; }
                public string GroupRequirementTypeIdentifier { get; set; }

                public bool MustMeetRequirementToAddMember { get; set; }

                public bool AllowLeadersToOverride { get; set; }

                public AppliesToAgeClassification? AppliesToAgeClassification { get; set; }

                /// <inheritdoc cref="GroupRequirement.AppliesToDataViewId" />
                public string AppliesToDataViewIdentifier { get; set; }
 
                public DueDateType? DueDateRequirementType { get; set; }
                public int? DueDateGroupAttributeId { get; set; }
                public DateTime? DueDate { get; set; }
            }

            public static GroupRequirement AddGroupRequirement( RockContext rockContext, AddGroupRequirementToGroupArgs args )
            {
                var groupRequirement = AddGroupRequirementInternal( rockContext, args, "Group", args.GroupIdentifier );
                return groupRequirement;
            }

            public static GroupRequirement AddGroupTypeRequirement( RockContext rockContext, AddGroupRequirementToGroupTypeArgs args )
            {
                var groupRequirement = AddGroupRequirementInternal( rockContext, args, "GroupType", args.GroupTypeIdentifier );
                return groupRequirement;
            }

            private static GroupRequirement AddGroupRequirementInternal( RockContext rockContext, AddGroupRequirementArgsBase args, string targetEntityType, string targetEntityIdentifier )
            {
                var groupRequirementService = new GroupRequirementService( rockContext );

                GroupRequirement groupRequirement = null;
                if ( args.GroupRequirementGuid != null )
                {
                    groupRequirement = groupRequirementService.Get( args.GroupRequirementGuid.Value );
                    if ( groupRequirement != null && !args.ReplaceIfExists )
                    {
                        throw new Exception( "Item already exists" );
                    }
                }

                if ( groupRequirement == null )
                {
                    groupRequirement = new GroupRequirement();
                    groupRequirement.Guid = Guid.NewGuid();

                    groupRequirementService.Add( groupRequirement );
                }

                // Get the Requirement Type
                var requirementTypeService = new GroupRequirementTypeService( rockContext );
                var groupRequirementType = requirementTypeService.GetByIdentifierOrThrow( args.GroupRequirementTypeIdentifier );

                groupRequirement.GroupRequirementTypeId = groupRequirementType.Id;

                // Set the target of the requirement, either a Group Type or a Group.
                int? groupId = null;
                int? groupTypeId = null;
                if ( targetEntityType == "GroupType" )
                {
                    var groupTypeService = new GroupTypeService( rockContext );
                    var groupType = groupTypeService.GetByIdentifierOrThrow( targetEntityIdentifier );
                    groupTypeId = groupType.Id;

                    groupRequirement.GroupTypeId = groupTypeId;
                }
                else if ( targetEntityType == "Group" )
                {
                    var groupService = new GroupService( rockContext );
                    var group = groupService.GetByIdentifierOrThrow( targetEntityIdentifier );
                    groupId = group.Id;
                    groupTypeId = group.GroupTypeId;

                    groupRequirement.GroupId = groupId;
                }
                else
                {
                    throw new Exception( $"Invalid target entity type. [Value=\"{targetEntityType}\"]" );
                }

                // Get the Group Role
                if ( !string.IsNullOrWhiteSpace( args.GroupRoleIdentifier ) )
                {
                    var groupTypeRoleService = new GroupTypeRoleService( rockContext );
                    var groupRole = groupTypeRoleService.Queryable()
                        .Where( r => r.GroupTypeId == groupTypeId )
                        .GetByIdentifierOrThrow( args.GroupRoleIdentifier );

                    groupRequirement.GroupRoleId = groupRole.Id;
                }

                // Applies To
                if ( args.AppliesToAgeClassification != null )
                {
                    groupRequirement.AppliesToAgeClassification = args.AppliesToAgeClassification.Value;
                }

                if ( !string.IsNullOrWhiteSpace( args.AppliesToDataViewIdentifier ) )
                {
                    var dataService = new DataViewService( rockContext );
                    var dataView = dataService.Queryable().GetByIdentifier( args.AppliesToDataViewIdentifier );
                    groupRequirement.AppliesToDataViewId = dataView.Id;
                }

                // Additional Settings
                groupRequirement.MustMeetRequirementToAddMember = args.MustMeetRequirementToAddMember;
                groupRequirement.AllowLeadersToOverride = args.AllowLeadersToOverride;

                if ( groupRequirementType.DueDateType == DueDateType.ConfiguredDate )
                {
                    groupRequirement.DueDateStaticDate = args.DueDate;
                }

                if ( groupRequirementType.DueDateType == DueDateType.GroupAttribute )
                {
                    // Set this due date attribute if it exists.
                    if ( groupRequirement.GroupId == null )
                    {
                        throw new Exception( "Group Attribute for Due Date is not available for a Group Type requirement." );
                    }

                    var groupDueDateAttributeId = AttributeCache.AllForEntityType<Group>()
                        .Where( a => a.EntityTypeQualifierColumn == "GroupType"
                            && a.EntityTypeQualifierValue == groupRequirement.GroupId.GetValueOrDefault().ToString() )
                        .Select( a=> a.Id )
                        .FirstOrDefault();
                     groupRequirement.DueDateAttributeId = groupDueDateAttributeId;
                }

                // Check if a duplicate group requirement type exists for the same group role.
                var duplicateGroupRequirementsQuery = groupRequirementService.Queryable()
                    .Where( a => a.GroupRequirementTypeId == groupRequirement.GroupRequirementTypeId
                                && a.GroupRoleId == groupRequirement.GroupRoleId
                                && a.Guid != groupRequirement.Guid );

                if ( groupId != null )
                {
                    duplicateGroupRequirementsQuery = duplicateGroupRequirementsQuery.Where( r => r.GroupId == groupId );
                }
                else if ( groupTypeId != null )
                {
                    duplicateGroupRequirementsQuery = duplicateGroupRequirementsQuery.Where( r => r.GroupTypeId == groupTypeId );
                }

                if ( duplicateGroupRequirementsQuery.Any() )
                {
                    throw new Exception( $"This group already has a group requirement of {groupRequirementType.Name} {( groupRequirement.GroupRoleId.HasValue ? "for group role " + groupRequirement.GroupRole.Name : string.Empty )}" );
                }

                //groupRequirementService.Add( groupRequirement );

                return groupRequirement;
            }

            #endregion

            #region Group Requirement Types

            public class AddGroupRequirementTypeArgs
            {
                public bool ReplaceIfExists { get; set; }
                public string ForeignKey { get; set; }
                public string GroupRequirementTypeGuid { get; set; }
                public string Name { get; set; }

                public RequirementCheckType CheckType { get; set; }

                /// <summary>
                /// The filter used to identify Group Members that satisfy this requirement.
                /// For a Data View Check Type, this is the identifier of a Person Data View.
                /// For a SQL Check Type, this is a SQL expression that returns a list of Person Id. 
                /// </summary>
                public string PassFilterValue { get; set; }

                /// <summary>
                /// The filter used to identify Group Members that should have a warning status for this requirement.
                /// For a Data View Check Type, this is the identifier of a Person Data View.
                /// For a SQL Check Type, this is a SQL expression that returns a list of Person Id. 
                /// </summary>
                public string WarningFilterValue { get; set; }

                public string MeetsRequirementLabel { get; set; } = "Pass";
                public string DoesNotMeetRequirementLabel { get; set; } = "Fail";
                public string WarningLabel { get; set; } = "Warning";
            }

            public class AddGroupRequirementTypeActionResult
            {
                public GroupRequirementType GroupRequirementType;
                public int AffectedItemCount;
            }

            /// <summary>
            /// Add a new Group Requirement Type.
            /// </summary>
            /// <param name="args"></param>
            /// <returns></returns>
            public static AddGroupRequirementTypeActionResult AddGroupRequirementType( AddGroupRequirementTypeArgs args )
            {
                var result = new AddGroupRequirementTypeActionResult();
                GroupRequirementType groupRequirementType = null;

                var rockContext = new RockContext();
                rockContext.WrapTransaction( () =>
                {
                    var groupRequirementTypeService = new GroupRequirementTypeService( rockContext );

                    int? meetsRequirementDataViewId = null;
                    int? warningDataViewId = null;
                    string warningSql = null;
                    string meetsRequirementSql = null;

                    if ( args.CheckType == RequirementCheckType.Dataview )
                    {
                        // Get Data Views
                        var dataViewService = new DataViewService( rockContext );

                        if ( !string.IsNullOrWhiteSpace( args.PassFilterValue ) )
                        {
                            meetsRequirementDataViewId = dataViewService.GetByIdentifierOrThrow( args.PassFilterValue ).Id;
                        }
                        if ( !string.IsNullOrWhiteSpace( args.WarningFilterValue ) )
                        {
                            warningDataViewId = dataViewService.GetByIdentifierOrThrow( args.WarningFilterValue ).Id;
                        }
                    }
                    else if ( args.CheckType == RequirementCheckType.Sql )
                    {
                        warningSql = args.WarningFilterValue;
                        meetsRequirementSql = args.PassFilterValue;
                    }

                    var groupRequirementTypeGuid = args.GroupRequirementTypeGuid.AsGuidOrNull();

                    if ( groupRequirementTypeGuid != null )
                    {
                        groupRequirementType = groupRequirementTypeService.Queryable().FirstOrDefault( g => g.Guid == groupRequirementTypeGuid );
                        if ( groupRequirementType != null )
                        {
                            if ( args.ReplaceIfExists )
                            {
                                return;
                            }
                            DeleteGroupRequirementType( rockContext, args.GroupRequirementTypeGuid );
                            rockContext.SaveChanges();
                        }
                    }

                    groupRequirementType = new GroupRequirementType
                    {
                        ForeignKey = args.ForeignKey,
                        Name = args.Name,
                        RequirementCheckType = args.CheckType,
                        NegativeLabel = args.DoesNotMeetRequirementLabel,
                        PositiveLabel = args.MeetsRequirementLabel,
                        DataViewId = meetsRequirementDataViewId,
                        WarningDataViewId = warningDataViewId,
                        SqlExpression = meetsRequirementSql,
                        WarningSqlExpression = warningSql
                    };

                    groupRequirementTypeService.Add( groupRequirementType );

                    groupRequirementType.Guid = args.GroupRequirementTypeGuid.AsGuidOrNull() ?? Guid.NewGuid();
                    groupRequirementType.ForeignKey = args.ForeignKey;

                    rockContext.SaveChanges();
                } );

                result.GroupRequirementType = groupRequirementType;
                result.AffectedItemCount = ( groupRequirementType == null ? 0 : 1 );
                return result;
            }

            public static bool DeleteGroupRequirementType( RockContext rockContext, string groupRequirementTypeIdentifier )
            {
                rockContext.WrapTransaction( () =>
                {
                    var groupRequirementTypeService = new GroupRequirementTypeService( rockContext );
                    var groupRequirementType = groupRequirementTypeService.Get( groupRequirementTypeIdentifier );

                    groupRequirementTypeService.Delete( groupRequirementType );

                    rockContext.SaveChanges();
                } );

                return true;
            }

            #endregion

            #region Person Actions

            /// <summary>
            /// Sets the background check result for the specified Person.
            /// </summary>
            /// <param name="personIdentifier"></param>
            /// <param name="backgroundCheckDate"></param>
            /// <param name="isPass"></param>
            /// <returns></returns>
            public static bool SetPersonBackgroundCheck( string personIdentifier, DateTime? backgroundCheckDate, bool? isPass )
            {
                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );

                var person = personService.GetByIdentifierOrThrow( personIdentifier );

                person.LoadAttributes( rockContext );

                var isChecked = ( backgroundCheckDate != null );

                var checkResult = string.Empty;
                if ( isChecked )
                {
                    checkResult = isPass.GetValueOrDefault( true ) ? "Pass" : "Fail";
                }

                person.SetAttributeValue( "BackgroundChecked", isChecked.ToString() );
                person.SetAttributeValue( "BackgroundCheckDate", backgroundCheckDate );
                person.SetAttributeValue( "BackgroundCheckResult", checkResult );
                person.SaveAttributeValues( rockContext );

                return true;
            }

            /// <summary>
            /// Sets the Connection Status for the specified Person.
            /// </summary>
            /// <param name="personIdentifier"></param>
            /// <param name="connectionStatusIdentifier"></param>
            /// <returns></returns>
            public static bool SetPersonConnectionStatus( string personIdentifier, string connectionStatusIdentifier )
            {
                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );

                var person = personService.GetByIdentifierOrThrow( personIdentifier );

                var definedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid();

                var definedValueService = new DefinedValueService( rockContext );
                var connectionStatusValueQuery = definedValueService.Queryable()
                    .Where( v => v.DefinedType.Guid == definedTypeGuid );

                DefinedValue connectionStatusValue;
                if ( connectionStatusIdentifier.AsGuid() != Guid.Empty
                     || connectionStatusIdentifier.AsIntegerOrNull() != null )
                {
                    connectionStatusValue = connectionStatusValueQuery.GetByIdentifierOrThrow( connectionStatusIdentifier );
                }
                else
                {
                    connectionStatusValue = connectionStatusValueQuery.GetByName( connectionStatusIdentifier, "Value" );
                }

                person.ConnectionStatusValueId = connectionStatusValue.Id;

                rockContext.SaveChanges();

                return true;
            }

            #endregion
        }
    }
}
