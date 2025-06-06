using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

using Rock.Data;
using Rock.Enums.Core.AI.Agent;
using Rock.Field.Types;
using Rock.Model;
using Rock.Net;
using Rock.SystemGuid;
using Rock.Web.Cache;
using Rock.Workflow.Action;

using Group = Rock.Model.Group;
using Person = Rock.Model.Person;

namespace Rock.AI.Agent.Skills
{
    [Description( "Used for managing groups, such as adding or removing members, and getting group information." )]
    [AgentSkillGuid( "489e96d7-c66c-4683-b76b-92fbfda372f4" )]
    internal class GroupManagerSkill : IRockAiSkill
    {
        private AgentRequestContext _requestContext;
        private RockContext _rockContext;

        public GroupManagerSkill( AgentRequestContext requestContext, RockContext rockContext, ILogger<GroupManagerSkill> logger )
        {
            _requestContext = requestContext;
            _rockContext = rockContext;
        }

        #region Native Functions

        [KernelFunction( "GroupMemberOperations" )]
        [Description( @"Finds a group of people of a specific group type for a person.

This function is designed to be called multiple times in a progressive flow:

🔍 Phase 1 — Group Type Discovery:
- If the user mentions joining or finding a group, team, etc, and no specific group type is known yet, call this function with groupTypeId = null.
- The function will return a list of valid group types for you to match against.

🔍 Phase 2 — Filter Discovery:
- Once groupTypeId is known, call this function again with searchFilters = null.
- This call will return the list of valid filters for that group type. You must use this list in the final call.
- Do NOT guess or invent filter keys or values.

🔍 Phase 3 — Group Search:
- Call this function again with both groupTypeId and valid searchFilters to retrieve actual group results.

🛑 Do NOT:
- Guess or hardcode group type IDs or the search filter keys.
- Skip this function if information is incomplete — it is designed to provide what’s needed.

This function may be called 2–3 times per user request and is responsible for guiding the user and the model through the group selection process.
" )]
        [AgentFunctionGuid( "468688c4-86ad-401f-ab46-ac0875de2452" )]
        public string GroupMemberOperations( Kernel kernel, int personId, int groupId,
            [Description( "The operation to preform (Add|Update|Delete)." )] GroupMemberOperation operation,
            [Description( "The role ID. Pass null if the correct value is not yet known. Do not guess." )] int? groupMemberRoleId = null )
        {
            var test = _requestContext.ChatHistory.FirstOrDefault().Content;
            var currentPerson = RockRequestContextAccessor.Current.CurrentPerson;

            // Get the person and group to add them to

            var person = new PersonService( _rockContext ).Get( personId );

            if ( person == null )
            {
                return "We could not find the person you're looking to add to the group.";
            }

            var group = new GroupService( _rockContext ).Get( groupId );

            if ( group == null )
            {
                return "We could not find the group to add them to.";
            }

            // If delete process the action, we don't need to worry about the role
            if ( operation == GroupMemberOperation.Delete )
            {
                return DeletePersonFromGroup( person, group );
            }

            // Get the role that we're suppose to use.

            // If no role was provided return a list of roles
            if ( groupMemberRoleId == null || groupMemberRoleId == 0 )
            {
                var validValues = group.GroupType.Roles.Select( r => new
                    {
                        RoleId = r.Id,
                        RoleName = r.Name,
                        r.IsLeader,
                        IsDefaultRole = r.Id == group.GroupType.DefaultGroupRoleId
                    } ).ToList().ToJson();
                
                return $"The groups valid roles are: {validValues}. Use this list to find the best match to the request. If no role was specified by the user then call the GroupMemberOperations using the default role. If there is no default role then ask what role they would like to use.";
            }

            // Check that the provided role is in the selected group
            if ( !group.GroupType.Roles.Any( r => r.Id == groupMemberRoleId ) )
            {
                var validValues = group.GroupType.Roles.Select( r => new { r.Id, r.Name } ).ToList().ToJson();
                return $"The role provided is not valid for a group of type {group.GroupType.Name}. Valid values: {validValues}";
            }

            // Get the name of the group role
            var groupMemberRole = GroupTypeRoleCache.Get( groupMemberRoleId.Value );

            // Check that the person is not already in the group with that role
            var groupMemberService = new GroupMemberService( _rockContext );
            var groupMember = groupMemberService.Queryable()
                .Where( m => m.GroupId == groupId && m.PersonId == personId ).FirstOrDefault();

            if ( groupMember != null && groupMember.GroupRoleId == groupMemberRoleId )
            {
                return $"{person.NickName} is already a {groupMember.GroupRole.Name} in the {group.Name} group.";
            }

            // Add or Update the person
            if ( operation == GroupMemberOperation.Add )
            {
                return AddPersonToGroup( person, group, groupMemberRole, groupMember );
            }
            else
            {
                return UpdatePersonInGroup( groupMember, groupMemberRole );
            }
        }

        [KernelFunction( "GroupFinder" )]
        [Description( @"
Finds a group of a specific group type for a person.

This function supports a two-phase flow:

🔍 Phase 1 — Discovery:
- If the groupTypeId is not known, call this function with groupTypeId = null.
- If searchFilters are not known or have not been returned from a previous call, call this function with searchFilters = null.
- Passing null for either is expected and encouraged for discovery.
- The function will respond with a list of valid group types and/or search filters for the group type.

✅ Phase 2 — Filtered Search:
- Once a valid groupTypeId and searchFilters are known, call this function again to retrieve matching groups.

🚫 Do NOT make up or guess values:
- Do not pass searchFilters or groupTypeId unless they were explicitly provided by the user or returned from this function.
- If you're unsure about the valid filters or group type, start by calling this function with null values.
" )]
        [AgentFunctionGuid( "34335be4-bf15-07b2-48c8-fac18be8bc46" )]
        public string GroupFinder( int personId, int? groupTypeId = null,
        [Description(
            @"
A list of group filters used to refine the search.

You MUST pass null for this parameter if you have not already received the valid filters from this function for the selected groupTypeId.

Do NOT guess, infer, or create filters yourself. This function will return the correct options to use.
Each group type has unique filters, so this must always be looked up before sending values.
" )]
        string searchFilters = null, GroupFilterSortOrder? sortOrder = GroupFilterSortOrder.None )
        {
            // Ensure we have the correct group types.
            if ( groupTypeId == null || groupTypeId <= 0 )
            {
                var groupTypeReturn = GroupTypeCache.All().Select( t => new
                {
                    t.Id,
                    t.Name,
                    t.GroupTypePurposeValue?.Value
                } ).ToList().ToJson();

                return $@"Below is a list of valid group types you can choose from.  
                        Use this list to select the best match based on the user's request, then call the GroupFinder function again with the selected groupTypeId.

                        Once the group type is known, GroupFinder will return the valid criteria you can use to build the search filters.  
                        ⚠️ Do not guess or make up filter values — the search will only work with filters provided by the function.
                            {groupTypeReturn}";

            }

            var groupType = GroupTypeCache.Get( groupTypeId.Value );

            // Ensure we have the correct search filters
            if ( searchFilters == null )
            {
                var searchFilterConfiguration = GetGroupFinderSearchFilters( groupType);

                var searchFilterConfigurationResults = searchFilterConfiguration.ToJson();

                return $@"Below is the search filter configuration for this group type. Determine which are appropriate for the request
                        and recall this GroupFinder function with the appropriate configuration. Use the Value property as the FilterValue An array of group filters used to refine the search. Each item should be an object with the following shape:

{{
  ""GroupFilterType"": ""Attribute"" | ""Property"" | ""Demographic"",
  ""FilterKey"": string,        // The name of the attribute, property, or demographic field to filter on
  ""FilterValue"": string,      // The value to match against (e.g., 'Couples', 'true', '25')
  ""GroupFilterMatchType"": ""Equals"" | ""Contains"" | ""GreaterThan"" | ""LessThan""
}}

Example:
[
  {{
    ""GroupFilterType"": ""Attribute"",
    ""FilterKey"": ""Topic"",
    ""FilterValue"": ""123"",
    ""GroupFilterMatchType"": ""Equals""
  }},
  {{
    ""GroupFilterType"": ""Attribute"",
    ""FilterKey"": ""ChildFriendly"",
    ""FilterValue"": ""true"",
    ""GroupFilterMatchType"": ""Equals""
  }}
]
                        {searchFilterConfiguration.ToJson()}";
            }

            return @"The best matching groups are below:
                Ted Decker's Group Id: 26
                Bill Marble's Group Id: 56";
        }
        

        #endregion

        
    

        #region Private Methods

        private List<GroupFinderSearchFilter> GetGroupFinderSearchFilters( GroupTypeCache groupType )
        {
            var searchFilters = new List<GroupFinderSearchFilter>();

            // Group Name
            searchFilters.Add( new GroupFinderSearchFilter
            {
                GroupFilterSearchFilterType = GroupFilterSearchFilterType.Property.ToString(),
                FilterKey = "Name",
                FilterMatchTypes = "Contains,StartsWith"
            } );

            // Weekly Schedule
            if ( groupType.AllowedScheduleTypes == ScheduleType.Weekly )
            {
                searchFilters.Add( new GroupFinderSearchFilter
                {
                    GroupFilterSearchFilterType = GroupFilterSearchFilterType.Schedule.ToString(),
                    FilterKey = "Weekly",
                    FilterMatchTypes = "Equals",
                    ValidValues = "Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday"
                } );
            }

            // Age
            searchFilters.Add( new GroupFinderSearchFilter
            {
                GroupFilterSearchFilterType = GroupFilterSearchFilterType.Demographic.ToString(),
                FilterKey = "Age"
            } );

            // Group Attributes
            var group = new Rock.Model.Group();
            group.GroupTypeId = groupType.Id;
            group.LoadAttributes();

            foreach ( var groupAttribute in group.Attributes )
            {
                var fieldTypeGuid = groupAttribute.Value.FieldType.Guid;

                // Determine match types based on the attribute's field type
                var matchTypes = string.Empty;

                if ( fieldTypeGuid == Rock.SystemGuid.FieldType.TEXT.AsGuid() ||
                     fieldTypeGuid == Rock.SystemGuid.FieldType.MEMO.AsGuid() )
                {
                    matchTypes = "Contains,Equals";
                }
                else if ( fieldTypeGuid == Rock.SystemGuid.FieldType.INTEGER.AsGuid() ||
                          fieldTypeGuid == Rock.SystemGuid.FieldType.DECIMAL.AsGuid() ||
                          fieldTypeGuid == Rock.SystemGuid.FieldType.DATE_TIME.AsGuid() ||
                          fieldTypeGuid == Rock.SystemGuid.FieldType.DATE.AsGuid() )
                {
                    matchTypes = "Contains,Equals";
                }
                else if ( fieldTypeGuid == Rock.SystemGuid.FieldType.SINGLE_SELECT.AsGuid() ||
                          fieldTypeGuid == Rock.SystemGuid.FieldType.DEFINED_VALUE.AsGuid() ||
                          fieldTypeGuid == Rock.SystemGuid.FieldType.BOOLEAN.AsGuid() ||
                          fieldTypeGuid == Rock.SystemGuid.FieldType.CAMPUS.AsGuid() )
                {
                    matchTypes = "Equals";
                }
                else
                {
                    matchTypes = "Contains,Equals";
                }

                // Determine the valid values for the attribute
                var validValues = string.Empty;

                // Single-select
                if ( fieldTypeGuid == Rock.SystemGuid.FieldType.SINGLE_SELECT.AsGuid() )
                {
                    var singleSelectField = ( SelectSingleFieldType ) groupAttribute.Value.FieldType.Field;

                    var configurationValues = Rock.Field.Helper.GetConfiguredValues( groupAttribute.Value.ConfigurationValues );
                }
                // Campus
                else if ( fieldTypeGuid == Rock.SystemGuid.FieldType.CAMPUS.AsGuid() )
                {
                    validValues = CampusCache.All().Select( c => new { Value = c.Id, Description = c.Name } ).ToList().ToJson();
                }
                // Defined Value
                else if ( fieldTypeGuid == Rock.SystemGuid.FieldType.DEFINED_VALUE.AsGuid() )
                {
                    var definedTypeId = groupAttribute.Value.ConfigurationValues.GetValueOrNull( "definedtype" ).AsIntegerOrNull();
                    var allMultiple = groupAttribute.Value.ConfigurationValues.GetValueOrNull( "allowmultiple" ).AsBoolean();
                    var usesDescription = groupAttribute.Value.ConfigurationValues.GetValueOrNull( "displaydescription" ).AsBoolean();

                    if ( !definedTypeId.HasValue )
                    {
                        validValues = string.Empty;
                    }

                    var definedType = DefinedTypeCache.Get( definedTypeId.Value );

                    validValues = DefinedTypeCache.Get( definedTypeId.Value )
                        .DefinedValues.Select( v => new { Value = v.Id, Description = usesDescription ? v.Description : v.Value } )
                        .ToList().ToJson();
                }
                // Boolean
                else if ( fieldTypeGuid == Rock.SystemGuid.FieldType.BOOLEAN.AsGuid() )
                {
                    validValues = "True,False";
                }

                // Return search filter
                searchFilters.Add( new GroupFinderSearchFilter
                {
                    GroupFilterSearchFilterType = GroupFilterSearchFilterType.Attribute.ToString(),
                    FilterKey = groupAttribute.Key,
                    FilterMatchTypes = matchTypes,
                    ValidValues = validValues
                } );

            }

            return searchFilters;
        }

        /// <summary>
        /// Adds a person to the group.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="group"></param>
        /// <param name="groupTypeRole"></param>
        /// <param name="groupMember"></param>
        /// <returns></returns>
        private string AddPersonToGroup( Person person, Group group, GroupTypeRoleCache groupTypeRole, GroupMember groupMember = null )
        {
            if( groupMember != null )
            {
                return $"{person.NickName} is already in the group with the role of {groupMember.GroupRole.Name}. Would you like for me to change their role?";
            }

            var groupMemberService = new GroupMemberService( _rockContext );
            groupMember = new GroupMember();
            groupMemberService.Add( groupMember );
            groupMember.PersonId = person.Id;
            groupMember.GroupId = group.Id;
            groupMember.GroupRoleId = groupTypeRole.Id;

            _rockContext.SaveChanges();

            return $"{person.NickName} has been added as a {groupTypeRole.Name} to {group.Name}.";
        }

        /// <summary>
        /// Updates the group member in the group.
        /// </summary>
        /// <param name="groupMember"></param>
        /// <param name="groupTypeRole"></param>
        /// <returns></returns>
        private string UpdatePersonInGroup( GroupMember groupMember, GroupTypeRoleCache groupTypeRole )
        {
            if ( groupMember == null )
            {
                return "The individual is not part of the group, so no update was made. Ask if they would like to add them instead.";
            }

            groupMember.GroupRoleId = groupTypeRole.Id;

            _rockContext.SaveChanges();

            return $"The individual was updated to the {groupTypeRole.Name} role.";
        }

        /// <summary>
        /// Deletes the person from a group. A person could have multiple roles in the group. They will be removed from each.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        private string DeletePersonFromGroup( Person person, Group group )
        {
            var groupMemberService = new GroupMemberService( _rockContext );

            var groupMembers = groupMemberService.Queryable().Where( gm => gm.PersonId == person.Id && gm.GroupId == group.Id )
                .ToList();

            foreach( var groupMember in groupMembers )
            {
                groupMemberService.Delete( groupMember );
            }

            _rockContext.SaveChanges();

            return $"{person.NickName} has been removed from the group {group.Name}";
        }

        public List<AgentFunction> GetSemanticFunctions()
        {
            return new List<AgentFunction>();
        }

        #endregion

        #region POCOs
        private class GroupFinderSearchFilter
        {
            public string GroupFilterSearchFilterType { get; set; }
            public string FilterKey { get; set; }
            public string ValidValues { get; set; }
            public string FilterMatchTypes { get; set; }
        }
        #endregion

        #region Enums

        public enum GroupMemberOperation { Add, Update, Delete }

        internal enum GroupFilterSearchFilterType { Property, Attribute, Schedule, Demographic, Distance }

        internal enum GroupFilterSortOrder { None, NearestGroup, GroupName }

        #endregion

    }
}
