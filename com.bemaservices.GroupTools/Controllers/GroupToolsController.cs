// <copyright>
// Copyright by BEMA Software Services
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Web.Http;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Web.Cache;

namespace com.bemaservices.GroupTools.Controllers
{
    /// <summary>
    /// The controller class for the GroupTools
    /// </summary>
    [RoutePrefix( "api/com_bemaservices/GroupTools" )]

    public class GroupToolsController : Rock.Rest.ApiControllerBase
    {
        static ConcurrentDictionary<string, (IQueryable<GroupInformation> data, DateTime expire)> GroupsInformationV6Cache = new ConcurrentDictionary<string, (IQueryable<GroupInformation> data, DateTime expire)>();
        static ConcurrentDictionary<string, (GroupFinderInformation data, DateTime expire)> GroupFinderInformationV6Cache = new ConcurrentDictionary<string, (GroupFinderInformation data, DateTime expire)>();

        #region API Calls
        /// <summary>
        /// Version 6 of the endpoint used to get groups information. This iteration has a 5-minute cache on all results that don't include Off-Campus or In-Person life groups due to group capacities needing to be up-to-date.
        /// </summary>
        [Authenticate, Secured]
        [System.Web.Http.Route( "GetGroupsInformationV6" )]
        public IQueryable<GroupInformation> GetGroupsInformationV6(
            string groupTypeIds = "",
            string campusIds = "",
            string parentGroupIds = "",
            string meetingDays = "",
            string categoryIds = "",
            string age = "",
            int? offset = null,
            int? limit = null,
            int characterLimit = 150,
            string keywords = "",
            bool showPrivateGroups = false,
            bool showInactiveGroups = false,
            string optionalFilterAttributeKey = null,
            string optionalFilterIds = null,
            string alternateCategoryKey = null,
            string secondaryCategoryAttributeKey = null,
            string secondaryCategoryFilterIds = null,
            bool includeGroupsWithoutCampus = false,
            bool limitByCapacity = false,
            bool includePendingMembersInGroupCapacity = false,
            bool calculateCapacityByPerson = false,
            bool includeGroupsWithoutCapacity = false,
            string requestedAttributeIds = "",
            string defaultColor = "#428bca",
            string groupTypeDefinedValueId = "",
            string groupIds = "",
            bool skipLeaders = false,
            bool skipMeetingLocations = false,
            bool skipGroupMemberCount = false,
            bool skipLifeStage = false,
            bool skipSchedule = false,
            bool skipCategories = false )
        {
            string key = "";

            // Skip cache if groups are Off-Campus (they have a capacity that needs to remain up-to-date)
            bool skipCache = false;

            // groupTypeDefinedValueId is blank so groupTypeIds will be used - Check if group type is Off-Campus
            if ( groupTypeDefinedValueId.IsNullOrWhiteSpace() && groupTypeIds.Contains( "607" ) )
            {
                skipCache = true;
            }
            // groupTypeDefinedValueId is not blank so groupTypeDefinedValueId will be used - Check if category is Off-Campus or In-Person
            if ( groupTypeDefinedValueId.IsNotNullOrWhiteSpace() )
            {
                if ( groupTypeDefinedValueId == "6502" || groupTypeDefinedValueId == "6503" )
                {
                    skipCache = true;
                }
            }

            if ( skipCache == false )
            {
                key = $"{groupTypeDefinedValueId}-{skipMeetingLocations}-{groupTypeIds}-{limit}-{offset}-{campusIds}-{meetingDays}-{categoryIds}-{keywords}-{secondaryCategoryAttributeKey}-{secondaryCategoryFilterIds}";

                if ( GroupsInformationV6Cache.ContainsKey( key ) )
                {
                    var cachedData = GroupsInformationV6Cache[key];
                    if ( cachedData.expire < DateTime.Now )
                    {
                        GroupsInformationV6Cache.TryRemove( key, out _ );
                    }
                    else
                    {
                        return cachedData.data;
                    }
                }
            }

            var currentPerson = GetPerson();

            var rockContext = new RockContext();
            var groupInfoList = new List<GroupInformation>();
            var definedValueService = new DefinedValueService( rockContext );
            int entityTypeId = EntityTypeCache.GetId( typeof( Group ) ) ?? 0;

            var qryOptions = new GroupQueryOptions
            {
                GroupTypeIds = groupTypeIds,
                GroupIds = groupIds,
                CampusIds = campusIds,
                ParentGroupIds = parentGroupIds,
                MeetingDays = meetingDays,
                Age = age,
                Keywords = keywords,

                CategoryKey = alternateCategoryKey.IsNotNullOrWhiteSpace() ? alternateCategoryKey : "Category",
                CategoryIds = categoryIds,
                OptionalFilterAttributeKey = optionalFilterAttributeKey,
                OptionalFilterIds = optionalFilterIds,
                SecondaryCategoryAttributeKey = secondaryCategoryAttributeKey,
                SecondaryCategoryFilterIds = secondaryCategoryFilterIds,

                ShowPrivateGroups = showPrivateGroups,
                ShowInactiveGroups = showInactiveGroups,
                IncludeGroupsWithoutCampus = includeGroupsWithoutCampus,
                LimitByCapacity = limitByCapacity,
                IncludePendingMembersInGroupCapacity = includePendingMembersInGroupCapacity,
                CalculateCapacityByPerson = calculateCapacityByPerson,
                IncludeGroupsWithoutCapacity = includeGroupsWithoutCapacity,

                SkipLeaders = skipLeaders,
                SkipMeetingLocations = skipMeetingLocations,
                SkipGroupMemberCount = skipGroupMemberCount,
                SkipLifeStage = skipLifeStage,
                SkipSchedule = skipSchedule,
                SkipCategories = skipCategories
            };

            if ( groupTypeDefinedValueId.IsNotNullOrWhiteSpace() )
            {
                var definedValueId = groupTypeDefinedValueId.AsInteger();
                var definedValue = definedValueService.Get( definedValueId );
                if ( definedValue != null )
                {
                    definedValue.LoadAttributes();
                    var definedValueGroupTypes = definedValue.GetAttributeValue( "GroupTypes" );
                    var definedValueSortByGroupTypeOrder = definedValue.GetAttributeValue( "PrimarySortByType" ).AsBoolean();
                    if ( definedValueGroupTypes.IsNotNullOrWhiteSpace() && definedValueGroupTypes.SplitDelimitedValues().AsIntegerList().Any() )
                    {
                        qryOptions.GroupTypeIds = definedValueGroupTypes;
                    }

                    if ( definedValueSortByGroupTypeOrder )
                    {
                        qryOptions.SortByGroupTypeOrder = true;
                    }
                }
            }

            IQueryable<Group> qry = FilterGroups( rockContext, currentPerson, qryOptions );

            var parameters = new Dictionary<string, object>();
            parameters.Add( "GroupTypeIds", qryOptions.GroupTypeIds );
            parameters.Add( "CategoryKey", qryOptions.CategoryKey );
            parameters.Add( "SortByGroupTypeOrder", qryOptions.SortByGroupTypeOrder ? 1 : 0 );
            parameters.Add( "GroupIds", qry.Select( g => g.Id ).ToList().AsDelimited( "," ) );
            var query = @" Create table #CategoryAttributeIdTable (CategoryId Int)
                                Insert into #CategoryAttributeIdTable
                                Select a.Id 
                                from Attribute a 
                                Join EntityType et on et.Id = a.EntityTypeId 
                                Where [Key] = @CategoryKey 
                                and et.Name = 'Rock.Model.Group';

                        With GroupTypeOrder as (
                        Select *, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) as GroupTypeOrder from ufnUtility_CsvToTable(@GroupTypeIds)
                        )
                        Select g.Id
	                        , g.Name
                            , Case When @SortByGroupTypeOrder = 1 then gto.GroupTypeOrder else 0 end as GroupTypeSortOrder
	                        , Case When av.Value like '%99BC9586-3C23-4BE4-BEB3-2285FBBCD1C9%' then 1 else 0 end as IsFeatured
	                        , Case When av.Value like '%3F3EFCE6-8DEB-4F3E-A6F8-E9E6FE3A1852%' then 1 else 0 end as IsNew
                        From [Group] g
                        Join GroupTypeOrder gto on gto.Item = g.GroupTypeId
                        Left Join AttributeValue av on av.EntityId = g.Id and av.AttributeId in (Select CategoryId from #CategoryAttributeIdTable)
                        Where g.Id in (Select value from string_split(@GroupIds,','))
                        Order By GroupTypeSortOrder, IsFeatured desc, IsNew desc, g.Name ";

            var sortingTableQry = DbService.GetDataTable( query, CommandType.Text, parameters, 30 ).AsEnumerable().AsQueryable();

            if ( offset.HasValue )
            {
                sortingTableQry = sortingTableQry.Skip( offset.Value );
            }

            if ( limit.HasValue )
            {
                sortingTableQry = sortingTableQry.Take( limit.Value );
            }

            var sortingTableList = sortingTableQry.Select( s => new GroupSortInfo
            {
                GroupId = s["Id"].ToStringSafe().AsInteger(),
                SortOrder = s["GroupTypeSortOrder"].ToStringSafe().AsInteger(),
                IsFeatured = s["IsFeatured"].ToStringSafe().AsBoolean( false ),
                IsNew = s["IsNew"].ToStringSafe().AsBoolean( false ),
            } ).ToList();

            var selectedGroupIds = sortingTableList.Select( s => s.GroupId ).ToList();

            qry = qry.Where( g => selectedGroupIds.Contains( g.Id ) );

            var requestedAttributeIdList = requestedAttributeIds.SplitDelimitedValues().AsIntegerList();
            Dictionary<string, AttributeInformation> requestedAttributeList = new Dictionary<string, AttributeInformation>();
            Dictionary<string, AttributeCache> attributeCacheList = new Dictionary<string, AttributeCache>();
            foreach ( var requestedAttributeId in requestedAttributeIdList )
            {
                var requestedAttribute = GetAttributeInformation( currentPerson, requestedAttributeId );
                if ( requestedAttribute.Key != null )
                {
                    requestedAttributeList.AddOrReplace( requestedAttribute.Key, requestedAttribute );
                    attributeCacheList.AddOrReplace( requestedAttribute.Key, AttributeCache.Get( requestedAttribute.Id ) );
                }
            }

            foreach ( var groupQryObject in qry.ToList() )
            {
                var group = groupQryObject;
                var matchingQry = sortingTableList.Where( s => s.GroupId == group.Id ).FirstOrDefault();

                var groupInfo = new GroupInformation();
                groupInfo.Id = group.Id;
                groupInfo.Guid = group.Guid;
                groupInfo.GroupTypeId = group.GroupTypeId;
                groupInfo.GroupTypeSortOrder = matchingQry != null ? matchingQry.SortOrder : 0;
                groupInfo.CampusId = group.CampusId;
                groupInfo.Campus = group.CampusId.HasValue ? group.Campus.Name : "";
                groupInfo.Name = group.Name;
                groupInfo.Capacity = group.GroupCapacity;

                groupInfo.Description = group.Description.Truncate( characterLimit );
                groupInfo.Color = defaultColor;

                if ( !qryOptions.SkipLeaders )
                {
                    groupInfo.GroupLeaders = GetGroupLeaders( group );
                }

                if ( !qryOptions.SkipMeetingLocations )
                {
                    groupInfo.MeetingLocations = GetMeetingLocations( group );
                }

                if ( !qryOptions.SkipGroupMemberCount )
                {
                    groupInfo.GroupMemberCount = GetGroupMemberCount( qryOptions, group );
                }

                if ( !qryOptions.SkipSchedule )
                {
                    BuildGroupSchedule( group, groupInfo );
                }


                if ( !qryOptions.SkipLifeStage || !qryOptions.SkipCategories || requestedAttributeIdList.Any() )
                {
                    group.LoadAttributes();

                    if ( requestedAttributeIdList.Any() )
                    {
                        groupInfo.AttributeValues = GetRequestedAttributeValues( requestedAttributeIds, group, requestedAttributeList, attributeCacheList );
                    }

                    if ( !qryOptions.SkipCategories )
                    {
                        BuildCategories( definedValueService, qryOptions.CategoryKey, group, groupInfo );
                    }

                    if ( !qryOptions.SkipLifeStage )
                    {
                        groupInfo.LifeStage = GetGroupLifeStage( group );
                    }

                }

                groupInfoList.Add( groupInfo );
            }

            var groupInfoListQry = groupInfoList.AsQueryable().OrderBy( g => qryOptions.SortByGroupTypeOrder );
            if ( qryOptions.SortByGroupTypeOrder )
            {
                foreach ( var groupTypeId in qryOptions.GroupTypeIds.SplitDelimitedValues().AsIntegerList() )
                {
                    groupInfoListQry = groupInfoListQry.ThenByDescending( g => g.GroupTypeId == groupTypeId );
                }
            }

            groupInfoListQry = groupInfoListQry
                .OrderBy( g => g.GroupTypeSortOrder )
                .ThenByDescending( g => g.Category == "Featured" )
                .ThenByDescending( g => g.Category == "New" )
                .ThenBy( g => g.Name );

            if ( skipCache == false )
            {
                GroupsInformationV6Cache[key] = (groupInfoListQry.AsQueryable(), DateTime.Now.AddMinutes( 5 ));
            }

            return groupInfoListQry.AsQueryable();
        }

        /// <summary>
        /// Version 6 of the endpoint used to get the group finder information. This iteration has a 5-minute cache on all results that don't include Off-Campus or In-Person life groups due to group capacities needing to be up-to-date.
        /// </summary>
        [Authenticate, Secured]
        [System.Web.Http.Route( "GetGroupFinderInformationV6" )]
        public GroupFinderInformation GetGroupFinderInformationV6(
            string groupTypeIds = "",
            string campusIds = "",
            string parentGroupIds = "",
            string meetingDays = "",
            string categoryIds = "",
            string age = "",
            int? offset = null,
            int? limit = null,
            int characterLimit = 150,
            string keywords = "",
            bool showPrivateGroups = false,
            bool showInactiveGroups = false,
            string optionalFilterAttributeKey = null,
            string optionalFilterIds = null,
            string alternateCategoryKey = null,
            string secondaryCategoryAttributeKey = null,
            string secondaryCategoryFilterIds = null,
            bool includeGroupsWithoutCampus = false,
            bool limitByCapacity = false,
            bool includePendingMembersInGroupCapacity = false,
            bool includeGroupsWithoutCapacity = false,
            string groupTypeDefinedValueId = "" )
        {
            string key = "";

            // Skip cache if groups are Off-Campus (they have a capacity that needs to remain up-to-date)
            bool skipCache = false;

            // groupTypeDefinedValueId is blank so groupTypeIds will be used - Check if group type is Off-Campus
            if ( groupTypeDefinedValueId.IsNullOrWhiteSpace() && groupTypeIds.Contains( "607" ) )
            {
                skipCache = true;
            }
            // groupTypeDefinedValueId is not blank so groupTypeDefinedValueId will be used - Check if category is Off-Campus or In-Person
            if ( groupTypeDefinedValueId.IsNotNullOrWhiteSpace() )
            {
                if ( groupTypeDefinedValueId == "6502" || groupTypeDefinedValueId == "6503" )
                {
                    skipCache = true;
                }
            }

            if ( skipCache == false )
            {
                key = $"{groupTypeDefinedValueId}-{groupTypeIds}-{campusIds}-{meetingDays}-{categoryIds}-{keywords}-{offset}-{limit}-{secondaryCategoryAttributeKey}-{secondaryCategoryFilterIds}";

                if ( GroupFinderInformationV6Cache.ContainsKey( key ) )
                {
                    var cachedData = GroupFinderInformationV6Cache[key];
                    if ( cachedData.expire < DateTime.Now )
                    {
                        GroupFinderInformationV6Cache.TryRemove( key, out _ );
                    }
                    else
                    {
                        return cachedData.data;
                    }
                }
            }

            var groupFinderInfo = new GroupFinderInformation();
            groupFinderInfo.Limit = limit;
            groupFinderInfo.Offset = offset;

            var currentPerson = GetPerson();

            var rockContext = new RockContext();
            var groupInfoList = new List<GroupInformation>();
            var definedValueService = new DefinedValueService( rockContext );
            int entityTypeId = EntityTypeCache.GetId( typeof( Group ) ) ?? 0;

            var qryOptions = new GroupQueryOptions
            {
                GroupTypeIds = groupTypeIds,
                CampusIds = campusIds,
                ParentGroupIds = parentGroupIds,
                MeetingDays = meetingDays,
                Age = age,
                Keywords = keywords,

                CategoryKey = alternateCategoryKey.IsNotNullOrWhiteSpace() ? alternateCategoryKey : "Category",
                CategoryIds = categoryIds,
                OptionalFilterAttributeKey = optionalFilterAttributeKey,
                OptionalFilterIds = optionalFilterIds,
                SecondaryCategoryAttributeKey = secondaryCategoryAttributeKey,
                SecondaryCategoryFilterIds = secondaryCategoryFilterIds,

                ShowPrivateGroups = showPrivateGroups,
                ShowInactiveGroups = showInactiveGroups,
                IncludeGroupsWithoutCampus = includeGroupsWithoutCampus,
                LimitByCapacity = limitByCapacity,
                IncludePendingMembersInGroupCapacity = includePendingMembersInGroupCapacity,
                IncludeGroupsWithoutCapacity = includeGroupsWithoutCapacity
            };

            if ( groupTypeDefinedValueId.IsNotNullOrWhiteSpace() )
            {
                var definedValueId = groupTypeDefinedValueId.AsInteger();
                var definedValue = definedValueService.Get( definedValueId );
                if ( definedValue != null )
                {
                    definedValue.LoadAttributes();
                    var definedValueGroupTypes = definedValue.GetAttributeValue( "GroupTypes" );
                    var definedValueSortByGroupTypeOrder = definedValue.GetAttributeValue( "PrimarySortByType" ).AsBoolean();
                    if ( definedValueGroupTypes.IsNotNullOrWhiteSpace() && definedValueGroupTypes.SplitDelimitedValues().AsIntegerList().Any() )
                    {
                        qryOptions.GroupTypeIds = definedValueGroupTypes;
                    }

                    if ( definedValueSortByGroupTypeOrder )
                    {
                        qryOptions.SortByGroupTypeOrder = true;
                    }
                }
            }

            IQueryable<Group> qry = FilterGroups( rockContext, currentPerson, qryOptions );

            groupFinderInfo.GroupIds = qry.Select( g => g.Id ).ToList();
            groupFinderInfo.TotalCount = groupFinderInfo.GroupIds.Count();
            groupFinderInfo.GroupIdList = groupFinderInfo.GroupIds.AsDelimited( "," );

            var parameters = new Dictionary<string, object>();
            parameters.Add( "CategoryKey", qryOptions.CategoryKey );
            parameters.Add( "SecondaryCategoryKey", qryOptions.SecondaryCategoryAttributeKey );
            parameters.Add( "GroupIds", groupFinderInfo.GroupIdList );
            var query = @"      Create table #CategoryAttributeIdTable (CategoryId Int)
                                Insert into #CategoryAttributeIdTable
                                Select a.Id 
                                from Attribute a 
                                Join EntityType et on et.Id = a.EntityTypeId 
                                Where [Key] = @CategoryKey 
                                and et.Name = 'Rock.Model.Group'

                                Create Table #SecondaryCategoryAttributeIdTable (CategoryId int)
                                Insert into #SecondaryCategoryAttributeIdTable
                                Select a.Id 
                                from Attribute a 
                                Join EntityType et on et.Id = a.EntityTypeId 
                                Where [Key] = @SecondaryCategoryKey 
                                and et.Name = 'Rock.Model.Group'

                                Select g.Id
		                                ,s.WeeklyDayOfWeek
		                                ,dv1.Guid as CategoryOption
		                                ,dv2.Guid as SecondaryCategoryOption
                                From [Group] g
                                Left Join Schedule s on g.ScheduleId = s.Id
                                Left Join AttributeValue av1 on av1.EntityId = g.Id and av1.AttributeId in (Select CategoryId from #CategoryAttributeIdTable)
								Left Join DefinedValue dv1 on dv1.Guid in (Select try_cast(value as Uniqueidentifier) From String_Split(av1.Value, ','))
                                Left Join AttributeValue av2 on av2.EntityId = g.Id and av2.AttributeId in (Select CategoryId from #SecondaryCategoryAttributeIdTable)
                                Left Join DefinedValue dv2 on dv2.Guid in (Select try_cast(value as Uniqueidentifier) From String_Split(av2.Value, ','))
                                Where g.Id in (Select value from string_split(@GroupIds,','))";

            var optionsQry = DbService.GetDataTable( query, CommandType.Text, parameters, 30 ).AsEnumerable().Select( s => new GroupOptionInfo
            {
                GroupId = s["Id"].ToStringSafe().AsInteger(),
                WeeklyDayOfWeek = s["WeeklyDayOfWeek"].ToStringSafe().AsIntegerOrNull(),
                CategoryOption = s["CategoryOption"].ToStringSafe(),
                SecondaryCategoryOption = s["SecondaryCategoryOption"].ToStringSafe(),
            } );

            groupFinderInfo.MeetingDayOptions = optionsQry.Where( g => g.WeeklyDayOfWeek.HasValue ).Select( g => g.WeeklyDayOfWeek.Value ).Distinct().ToList().Select( w => new FilterOption
            {
                Text = ( ( DayOfWeek ) w ).ConvertToString( true ),
                Value = w.ToString()
            } ).ToList();

            var categoryGuids = optionsQry.Where( g => g.CategoryOption != null ).Select( g => g.CategoryOption ).Distinct().ToList().AsGuidList();
            groupFinderInfo.CategoryOptions = definedValueService.GetByGuids( categoryGuids ).OrderBy( dv => dv.Order ).Select( dv => new FilterOption
            {
                Text = dv.Value,
                Value = dv.Id.ToString()
            } ).ToList();

            var secondaryCategoryGuids = optionsQry.Where( g => g.SecondaryCategoryOption != null ).Select( g => g.SecondaryCategoryOption ).Distinct().ToList().AsGuidList();
            groupFinderInfo.SecondaryCategoryOptions = definedValueService.GetByGuids( secondaryCategoryGuids ).OrderBy( dv => dv.Order ).Select( dv => new FilterOption
            {
                Text = dv.Value,
                Value = dv.Id.ToString()
            } ).ToList();

            if ( skipCache == false )
            {
                GroupFinderInformationV6Cache[key] = (groupFinderInfo, DateTime.Now.AddMinutes( 5 ));
            }

            return groupFinderInfo;
        }

        [Authenticate, Secured]
        [System.Web.Http.Route( "GetGroupsInformationV4" )]
        public IQueryable<GroupInformation> GetGroupsInformationV4(
             string groupTypeIds = "",
            string campusIds = "",
            string parentGroupIds = "",
            string meetingDays = "",
            string categoryIds = "",
            string age = "",
            int? offset = null,
            int? limit = null,
            int characterLimit = 150,
            string keywords = "",
            bool showPrivateGroups = false,
            bool showInactiveGroups = false,
            string optionalFilterAttributeKey = null,
            string optionalFilterIds = null,
            string alternateCategoryKey = null,
            string secondaryCategoryAttributeKey = null,
            string secondaryCategoryFilterIds = null,
            bool includeGroupsWithoutCampus = false,
            bool limitByCapacity = false,
            bool includePendingMembersInGroupCapacity = false,
            bool calculateCapacityByPerson = false,
            bool includeGroupsWithoutCapacity = false,
            string requestedAttributeIds = "",
            string defaultColor = "#428bca",
            string groupTypeDefinedValueId = "",
            string groupIds = "",
            bool skipLeaders = false,
            bool skipMeetingLocations = false,
            bool skipGroupMemberCount = false,
            bool skipLifeStage = false,
            bool skipSchedule = false,
            bool skipCategories = false )
        {
            var currentPerson = GetPerson();

            var rockContext = new RockContext();
            var groupInfoList = new List<GroupInformation>();
            var definedValueService = new DefinedValueService( rockContext );
            int entityTypeId = EntityTypeCache.GetId( typeof( Group ) ) ?? 0;

            var qryOptions = new GroupQueryOptions
            {
                GroupTypeIds = groupTypeIds,
                GroupIds = groupIds,
                CampusIds = campusIds,
                ParentGroupIds = parentGroupIds,
                MeetingDays = meetingDays,
                Age = age,
                Keywords = keywords,

                CategoryKey = alternateCategoryKey.IsNotNullOrWhiteSpace() ? alternateCategoryKey : "Category",
                CategoryIds = categoryIds,
                OptionalFilterAttributeKey = optionalFilterAttributeKey,
                OptionalFilterIds = optionalFilterIds,
                SecondaryCategoryAttributeKey = secondaryCategoryAttributeKey,
                SecondaryCategoryFilterIds = secondaryCategoryFilterIds,

                ShowPrivateGroups = showPrivateGroups,
                ShowInactiveGroups = showInactiveGroups,
                IncludeGroupsWithoutCampus = includeGroupsWithoutCampus,
                LimitByCapacity = limitByCapacity,
                IncludePendingMembersInGroupCapacity = includePendingMembersInGroupCapacity,
                CalculateCapacityByPerson = calculateCapacityByPerson,
                IncludeGroupsWithoutCapacity = includeGroupsWithoutCapacity,

                SkipLeaders = skipLeaders,
                SkipMeetingLocations = skipMeetingLocations,
                SkipGroupMemberCount = skipGroupMemberCount,
                SkipLifeStage = skipLifeStage,
                SkipSchedule = skipSchedule,
                SkipCategories = skipCategories
            };

            if ( groupTypeDefinedValueId.IsNotNullOrWhiteSpace() )
            {
                var definedValueId = groupTypeDefinedValueId.AsInteger();
                var definedValue = definedValueService.Get( definedValueId );
                if ( definedValue != null )
                {
                    definedValue.LoadAttributes();
                    var definedValueGroupTypes = definedValue.GetAttributeValue( "GroupTypes" );
                    var definedValueSortByGroupTypeOrder = definedValue.GetAttributeValue( "PrimarySortByType" ).AsBoolean();
                    if ( definedValueGroupTypes.IsNotNullOrWhiteSpace() && definedValueGroupTypes.SplitDelimitedValues().AsIntegerList().Any() )
                    {
                        qryOptions.GroupTypeIds = definedValueGroupTypes;
                    }

                    if ( definedValueSortByGroupTypeOrder )
                    {
                        qryOptions.SortByGroupTypeOrder = true;
                    }
                }
            }

            IQueryable<Group> qry = FilterGroups( rockContext, currentPerson, qryOptions );

            var parameters = new Dictionary<string, object>();
            parameters.Add( "GroupTypeIds", qryOptions.GroupTypeIds );
            parameters.Add( "CategoryKey", qryOptions.CategoryKey );
            parameters.Add( "SortByGroupTypeOrder", qryOptions.SortByGroupTypeOrder ? 1 : 0 );
            parameters.Add( "GroupIds", qry.Select( g => g.Id ).ToList().AsDelimited( "," ) );
            var query = @" Create table #CategoryAttributeIdTable (CategoryId Int)
                                Insert into #CategoryAttributeIdTable
                                Select a.Id 
                                from Attribute a 
                                Join EntityType et on et.Id = a.EntityTypeId 
                                Where [Key] = @CategoryKey 
                                and et.Name = 'Rock.Model.Group';

                        With GroupTypeOrder as (
                        Select *, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) as GroupTypeOrder from ufnUtility_CsvToTable(@GroupTypeIds)
                        )
                        Select g.Id
	                        , g.Name
                            , Case When @SortByGroupTypeOrder = 1 then gto.GroupTypeOrder else 0 end as GroupTypeSortOrder
	                        , Case When av.Value like '%99BC9586-3C23-4BE4-BEB3-2285FBBCD1C9%' then 1 else 0 end as IsFeatured
	                        , Case When av.Value like '%3F3EFCE6-8DEB-4F3E-A6F8-E9E6FE3A1852%' then 1 else 0 end as IsNew
                        From [Group] g
                        Join GroupTypeOrder gto on gto.Item = g.GroupTypeId
                        Left Join AttributeValue av on av.EntityId = g.Id and av.AttributeId in (Select CategoryId from #CategoryAttributeIdTable)
                        Where g.Id in (Select value from string_split(@GroupIds,','))
                        Order By GroupTypeSortOrder, IsFeatured desc, IsNew desc, g.Name ";

            var sortingTableQry = DbService.GetDataTable( query, CommandType.Text, parameters, 30 ).AsEnumerable().AsQueryable();

            if ( offset.HasValue )
            {
                sortingTableQry = sortingTableQry.Skip( offset.Value );
            }

            if ( limit.HasValue )
            {
                sortingTableQry = sortingTableQry.Take( limit.Value );
            }

            var sortingTableList = sortingTableQry.Select( s => new GroupSortInfo
            {
                GroupId = s["Id"].ToStringSafe().AsInteger(),
                SortOrder = s["GroupTypeSortOrder"].ToStringSafe().AsInteger(),
                IsFeatured = s["IsFeatured"].ToStringSafe().AsBoolean( false ),
                IsNew = s["IsNew"].ToStringSafe().AsBoolean( false ),
            } ).ToList();

            var selectedGroupIds = sortingTableList.Select( s => s.GroupId ).ToList();

            qry = qry.Where( g => selectedGroupIds.Contains( g.Id ) );

            var requestedAttributeIdList = requestedAttributeIds.SplitDelimitedValues().AsIntegerList();
            Dictionary<string, AttributeInformation> requestedAttributeList = new Dictionary<string, AttributeInformation>();
            Dictionary<string, AttributeCache> attributeCacheList = new Dictionary<string, AttributeCache>();
            foreach ( var requestedAttributeId in requestedAttributeIdList )
            {
                var requestedAttribute = GetAttributeInformation( currentPerson, requestedAttributeId );
                if ( requestedAttribute.Key != null )
                {
                    requestedAttributeList.AddOrReplace( requestedAttribute.Key, requestedAttribute );
                    attributeCacheList.AddOrReplace( requestedAttribute.Key, AttributeCache.Get( requestedAttribute.Id ) );
                }
            }

            foreach ( var groupQryObject in qry.ToList() )
            {
                var group = groupQryObject;
                var matchingQry = sortingTableList.Where( s => s.GroupId == group.Id ).FirstOrDefault();

                var groupInfo = new GroupInformation();
                groupInfo.Id = group.Id;
                groupInfo.Guid = group.Guid;
                groupInfo.GroupTypeId = group.GroupTypeId;
                groupInfo.GroupTypeSortOrder = matchingQry != null ? matchingQry.SortOrder : 0;
                groupInfo.CampusId = group.CampusId;
                groupInfo.Campus = group.CampusId.HasValue ? group.Campus.Name : "";
                groupInfo.Name = group.Name;
                groupInfo.Capacity = group.GroupCapacity;

                groupInfo.Description = group.Description.Truncate( characterLimit );
                groupInfo.Color = defaultColor;

                if ( !qryOptions.SkipLeaders )
                {
                    groupInfo.GroupLeaders = GetGroupLeaders( group );
                }

                if ( !qryOptions.SkipMeetingLocations )
                {
                    groupInfo.MeetingLocations = GetMeetingLocations( group );
                }

                if ( !qryOptions.SkipGroupMemberCount )
                {
                    groupInfo.GroupMemberCount = GetGroupMemberCount( qryOptions, group );
                }

                if ( !qryOptions.SkipSchedule )
                {
                    BuildGroupSchedule( group, groupInfo );
                }


                if ( !qryOptions.SkipLifeStage || !qryOptions.SkipCategories || requestedAttributeIdList.Any() )
                {
                    group.LoadAttributes();

                    if ( requestedAttributeIdList.Any() )
                    {
                        groupInfo.AttributeValues = GetRequestedAttributeValues( requestedAttributeIds, group, requestedAttributeList, attributeCacheList );
                    }

                    if ( !qryOptions.SkipCategories )
                    {
                        BuildCategories( definedValueService, qryOptions.CategoryKey, group, groupInfo );
                    }

                    if ( !qryOptions.SkipLifeStage )
                    {
                        groupInfo.LifeStage = GetGroupLifeStage( group );
                    }

                }

                groupInfoList.Add( groupInfo );
            }

            var groupInfoListQry = groupInfoList.AsQueryable().OrderBy( g => qryOptions.SortByGroupTypeOrder );
            if ( qryOptions.SortByGroupTypeOrder )
            {
                foreach ( var groupTypeId in qryOptions.GroupTypeIds.SplitDelimitedValues().AsIntegerList() )
                {
                    groupInfoListQry = groupInfoListQry.ThenByDescending( g => g.GroupTypeId == groupTypeId );
                }
            }

            groupInfoListQry = groupInfoListQry
                .OrderBy( g => g.GroupTypeSortOrder )
                .ThenByDescending( g => g.Category == "Featured" )
                .ThenByDescending( g => g.Category == "New" )
                .ThenBy( g => g.Name );


            return groupInfoListQry.AsQueryable(); ;
        }

        [Authenticate, Secured]
        [System.Web.Http.Route( "GetGroupFinderInformationV4" )]
        public GroupFinderInformation GetGroupFinderInformationV4(
             string groupTypeIds = "",
            string campusIds = "",
            string parentGroupIds = "",
            string meetingDays = "",
            string categoryIds = "",
            string age = "",
            int? offset = null,
            int? limit = null,
            int characterLimit = 150,
            string keywords = "",
            bool showPrivateGroups = false,
            bool showInactiveGroups = false,
            string optionalFilterAttributeKey = null,
            string optionalFilterIds = null,
            string alternateCategoryKey = null,
            string secondaryCategoryAttributeKey = null,
            string secondaryCategoryFilterIds = null,
            bool includeGroupsWithoutCampus = false,
            bool limitByCapacity = false,
            bool includePendingMembersInGroupCapacity = false,
            bool includeGroupsWithoutCapacity = false,
            string groupTypeDefinedValueId = "" )
        {
            var groupFinderInfo = new GroupFinderInformation();
            groupFinderInfo.Limit = limit;
            groupFinderInfo.Offset = offset;

            var currentPerson = GetPerson();

            var rockContext = new RockContext();
            var groupInfoList = new List<GroupInformation>();
            var definedValueService = new DefinedValueService( rockContext );
            int entityTypeId = EntityTypeCache.GetId( typeof( Group ) ) ?? 0;

            var qryOptions = new GroupQueryOptions
            {
                GroupTypeIds = groupTypeIds,
                CampusIds = campusIds,
                ParentGroupIds = parentGroupIds,
                MeetingDays = meetingDays,
                Age = age,
                Keywords = keywords,

                CategoryKey = alternateCategoryKey.IsNotNullOrWhiteSpace() ? alternateCategoryKey : "Category",
                CategoryIds = categoryIds,
                OptionalFilterAttributeKey = optionalFilterAttributeKey,
                OptionalFilterIds = optionalFilterIds,
                SecondaryCategoryAttributeKey = secondaryCategoryAttributeKey,
                SecondaryCategoryFilterIds = secondaryCategoryFilterIds,

                ShowPrivateGroups = showPrivateGroups,
                ShowInactiveGroups = showInactiveGroups,
                IncludeGroupsWithoutCampus = includeGroupsWithoutCampus,
                LimitByCapacity = limitByCapacity,
                IncludePendingMembersInGroupCapacity = includePendingMembersInGroupCapacity,
                IncludeGroupsWithoutCapacity = includeGroupsWithoutCapacity
            };

            if ( groupTypeDefinedValueId.IsNotNullOrWhiteSpace() )
            {
                var definedValueId = groupTypeDefinedValueId.AsInteger();
                var definedValue = definedValueService.Get( definedValueId );
                if ( definedValue != null )
                {
                    definedValue.LoadAttributes();
                    var definedValueGroupTypes = definedValue.GetAttributeValue( "GroupTypes" );
                    var definedValueSortByGroupTypeOrder = definedValue.GetAttributeValue( "PrimarySortByType" ).AsBoolean();
                    if ( definedValueGroupTypes.IsNotNullOrWhiteSpace() && definedValueGroupTypes.SplitDelimitedValues().AsIntegerList().Any() )
                    {
                        qryOptions.GroupTypeIds = definedValueGroupTypes;
                    }

                    if ( definedValueSortByGroupTypeOrder )
                    {
                        qryOptions.SortByGroupTypeOrder = true;
                    }
                }
            }

            IQueryable<Group> qry = FilterGroups( rockContext, currentPerson, qryOptions );

            groupFinderInfo.GroupIds = qry.Select( g => g.Id ).ToList();
            groupFinderInfo.TotalCount = groupFinderInfo.GroupIds.Count();
            groupFinderInfo.GroupIdList = groupFinderInfo.GroupIds.AsDelimited( "," );

            var parameters = new Dictionary<string, object>();
            parameters.Add( "CategoryKey", qryOptions.CategoryKey );
            parameters.Add( "SecondaryCategoryKey", qryOptions.SecondaryCategoryAttributeKey );
            parameters.Add( "GroupIds", groupFinderInfo.GroupIdList );
            var query = @"      Create table #CategoryAttributeIdTable (CategoryId Int)
                                Insert into #CategoryAttributeIdTable
                                Select a.Id 
                                from Attribute a 
                                Join EntityType et on et.Id = a.EntityTypeId 
                                Where [Key] = @CategoryKey 
                                and et.Name = 'Rock.Model.Group'

                                Create Table #SecondaryCategoryAttributeIdTable (CategoryId int)
                                Insert into #SecondaryCategoryAttributeIdTable
                                Select a.Id 
                                from Attribute a 
                                Join EntityType et on et.Id = a.EntityTypeId 
                                Where [Key] = @SecondaryCategoryKey 
                                and et.Name = 'Rock.Model.Group'

                                Select g.Id
		                                ,s.WeeklyDayOfWeek
		                                ,dv1.Guid as CategoryOption
		                                ,dv2.Guid as SecondaryCategoryOption
                                From [Group] g
                                Left Join Schedule s on g.ScheduleId = s.Id
                                Left Join AttributeValue av1 on av1.EntityId = g.Id and av1.AttributeId in (Select CategoryId from #CategoryAttributeIdTable)
								Left Join DefinedValue dv1 on dv1.Guid in (Select try_cast(value as Uniqueidentifier) From String_Split(av1.Value, ','))
                                Left Join AttributeValue av2 on av2.EntityId = g.Id and av2.AttributeId in (Select CategoryId from #SecondaryCategoryAttributeIdTable)
                                Left Join DefinedValue dv2 on dv2.Guid in (Select try_cast(value as Uniqueidentifier) From String_Split(av2.Value, ','))
                                Where g.Id in (Select value from string_split(@GroupIds,','))";

            var optionsQry = DbService.GetDataTable( query, CommandType.Text, parameters, 30 ).AsEnumerable().Select( s => new GroupOptionInfo
            {
                GroupId = s["Id"].ToStringSafe().AsInteger(),
                WeeklyDayOfWeek = s["WeeklyDayOfWeek"].ToStringSafe().AsIntegerOrNull(),
                CategoryOption = s["CategoryOption"].ToStringSafe(),
                SecondaryCategoryOption = s["SecondaryCategoryOption"].ToStringSafe(),
            } );

            groupFinderInfo.MeetingDayOptions = optionsQry.Where( g => g.WeeklyDayOfWeek.HasValue ).Select( g => g.WeeklyDayOfWeek.Value ).Distinct().ToList().Select( w => new FilterOption
            {
                Text = ( ( DayOfWeek ) w ).ConvertToString( true ),
                Value = w.ToString()
            } ).ToList();

            var categoryGuids = optionsQry.Where( g => g.CategoryOption != null ).Select( g => g.CategoryOption ).Distinct().ToList().AsGuidList();
            groupFinderInfo.CategoryOptions = definedValueService.GetByGuids( categoryGuids ).OrderBy( dv => dv.Order ).Select( dv => new FilterOption
            {
                Text = dv.Value,
                Value = dv.Id.ToString()
            } ).ToList();

            var secondaryCategoryGuids = optionsQry.Where( g => g.SecondaryCategoryOption != null ).Select( g => g.SecondaryCategoryOption ).Distinct().ToList().AsGuidList();
            groupFinderInfo.SecondaryCategoryOptions = definedValueService.GetByGuids( secondaryCategoryGuids ).OrderBy( dv => dv.Order ).Select( dv => new FilterOption
            {
                Text = dv.Value,
                Value = dv.Id.ToString()
            } ).ToList();

            return groupFinderInfo;
        }

        [Authenticate, Secured]
        [System.Web.Http.Route( "GetGroupsInformationV3" )]
        public GroupFinderInformation GetGroupsInformationV3(
             string groupTypeIds = "",
            string campusIds = "",
            string parentGroupIds = "",
            string meetingDays = "",
            string categoryIds = "",
            string age = "",
            int? offset = null,
            int? limit = null,
            int characterLimit = 150,
            string keywords = "",
            bool showPrivateGroups = false,
            bool showInactiveGroups = false,
            string optionalFilterAttributeKey = null,
            string optionalFilterIds = null,
            string alternateCategoryKey = null,
            string secondaryCategoryAttributeKey = null,
            string secondaryCategoryFilterIds = null,
            bool includeGroupsWithoutCampus = false,
            bool limitByCapacity = false,
            bool includePendingMembersInGroupCapacity = false,
            bool includeGroupsWithoutCapacity = false,
            string requestedAttributeIds = "",
            string defaultColor = "#428bca",
            string groupTypeDefinedValueId = "" )
        {
            var groupFinderInfo = new GroupFinderInformation();
            groupFinderInfo.Limit = limit;
            groupFinderInfo.Offset = offset;

            var currentPerson = GetPerson();

            var rockContext = new RockContext();
            var groupInfoList = new List<GroupInformation>();
            var definedValueService = new DefinedValueService( rockContext );
            int entityTypeId = EntityTypeCache.GetId( typeof( Group ) ) ?? 0;

            var qryOptions = new GroupQueryOptions
            {
                GroupTypeIds = groupTypeIds,
                CampusIds = campusIds,
                ParentGroupIds = parentGroupIds,
                MeetingDays = meetingDays,
                Age = age,
                Keywords = keywords,

                CategoryKey = alternateCategoryKey.IsNotNullOrWhiteSpace() ? alternateCategoryKey : "Category",
                CategoryIds = categoryIds,
                OptionalFilterAttributeKey = optionalFilterAttributeKey,
                OptionalFilterIds = optionalFilterIds,
                SecondaryCategoryAttributeKey = secondaryCategoryAttributeKey,
                SecondaryCategoryFilterIds = secondaryCategoryFilterIds,

                ShowPrivateGroups = showPrivateGroups,
                ShowInactiveGroups = showInactiveGroups,
                IncludeGroupsWithoutCampus = includeGroupsWithoutCampus,
                LimitByCapacity = limitByCapacity,
                IncludePendingMembersInGroupCapacity = includePendingMembersInGroupCapacity,
                IncludeGroupsWithoutCapacity = includeGroupsWithoutCapacity
            };

            if ( groupTypeDefinedValueId.IsNotNullOrWhiteSpace() )
            {
                var definedValueId = groupTypeDefinedValueId.AsInteger();
                var definedValue = definedValueService.Get( definedValueId );
                if ( definedValue != null )
                {
                    definedValue.LoadAttributes();
                    var definedValueGroupTypes = definedValue.GetAttributeValue( "GroupTypes" );
                    var definedValueSortByGroupTypeOrder = definedValue.GetAttributeValue( "PrimarySortByType" ).AsBoolean();
                    if ( definedValueGroupTypes.IsNotNullOrWhiteSpace() && definedValueGroupTypes.SplitDelimitedValues().AsIntegerList().Any() )
                    {
                        qryOptions.GroupTypeIds = definedValueGroupTypes;
                    }

                    if ( definedValueSortByGroupTypeOrder )
                    {
                        qryOptions.SortByGroupTypeOrder = true;
                    }

                }
            }

            IQueryable<Group> qry = FilterGroups( rockContext, currentPerson, qryOptions );

            var categoryValues = new AttributeValueService( rockContext ).Queryable().AsNoTracking()
                .Where( a => a.Attribute.Key == qryOptions.CategoryKey )
                .Where( a => a.Attribute.EntityTypeId == entityTypeId );

            var secondaryCategoryValues = new AttributeValueService( rockContext ).Queryable().AsNoTracking()
                .Where( a => a.Attribute.Key == qryOptions.SecondaryCategoryAttributeKey )
                .Where( a => a.Attribute.EntityTypeId == entityTypeId );

            var groupQry = from g in qry
                           join av in categoryValues on g.Id equals av.EntityId into gav
                           from x in gav.DefaultIfEmpty()
                           join sav in secondaryCategoryValues on g.Id equals sav.EntityId into gsav
                           from sx in gsav.DefaultIfEmpty()
                           select new
                           {
                               Group = g,
                               Categories = x.Value.ToUpper(),
                               SecondaryCategories = sx.Value.ToUpper()
                           };

            var orderedGroupQry = groupQry.OrderBy( g => qryOptions.SortByGroupTypeOrder );
            if ( qryOptions.SortByGroupTypeOrder )
            {
                foreach ( var groupTypeId in qryOptions.GroupTypeIds.SplitDelimitedValues().AsIntegerList() )
                {
                    orderedGroupQry = orderedGroupQry.ThenByDescending( g => g.Group.GroupTypeId == groupTypeId );
                }
            }

            groupQry = orderedGroupQry.ThenByDescending( g => g.Categories.Contains( "99BC9586-3C23-4BE4-BEB3-2285FBBCD1C9" ) )
             .ThenByDescending( g => g.Categories.Contains( "3F3EFCE6-8DEB-4F3E-A6F8-E9E6FE3A1852" ) )
             .ThenBy( g => g.Group.Name )
                .AsQueryable();

            groupFinderInfo.TotalCount = qry.Count();
            groupFinderInfo.MeetingDayOptions = qry.Where( g => g.Schedule.WeeklyDayOfWeek.HasValue ).Select( g => g.Schedule.WeeklyDayOfWeek.Value ).Distinct().ToList().Select( w => new FilterOption
            {
                Text = w.ConvertToString( true ),
                Value = w.ConvertToInt().ToString()
            } ).ToList();

            var categoryGuids = groupQry.Where( g => g.Categories != null ).Select( g => g.Categories ).Distinct().ToList().SelectMany( c => c.ToStringSafe().Split( ',' ) ).AsGuidList();
            groupFinderInfo.CategoryOptions = definedValueService.GetByGuids( categoryGuids ).Select( dv => new FilterOption
            {
                Text = dv.Value,
                Value = dv.Id.ToString()
            } ).ToList();

            var secondaryCategoryGuids = groupQry.Where( g => g.SecondaryCategories != null ).Select( g => g.SecondaryCategories ).Distinct().ToList().SelectMany( c => c.ToStringSafe().Split( ',' ) ).AsGuidList();
            groupFinderInfo.SecondaryCategoryOptions = definedValueService.GetByGuids( secondaryCategoryGuids ).Select( dv => new FilterOption
            {
                Text = dv.Value,
                Value = dv.Id.ToString()
            } ).ToList();

            if ( offset.HasValue )
            {
                groupQry = groupQry.Skip( offset.Value );
            }

            if ( limit.HasValue )
            {
                groupQry = groupQry.Take( limit.Value );
            }

            var requestedAttributeIdList = requestedAttributeIds.SplitDelimitedValues().AsIntegerList();
            Dictionary<string, AttributeInformation> requestedAttributeList = new Dictionary<string, AttributeInformation>();
            Dictionary<string, AttributeCache> attributeCacheList = new Dictionary<string, AttributeCache>();
            foreach ( var requestedAttributeId in requestedAttributeIdList )
            {
                var requestedAttribute = GetAttributeInformation( currentPerson, requestedAttributeId );
                if ( requestedAttribute.Key != null )
                {
                    requestedAttributeList.AddOrReplace( requestedAttribute.Key, requestedAttribute );
                    attributeCacheList.AddOrReplace( requestedAttribute.Key, AttributeCache.Get( requestedAttribute.Id ) );
                }
            }

            var groupQryList = groupQry.ToList();
            foreach ( var groupQryObject in groupQryList )
            {
                var group = groupQryObject.Group;
                group.LoadAttributes();

                var groupInfo = new GroupInformation();
                groupInfo.Id = group.Id;
                groupInfo.Guid = group.Guid;
                groupInfo.GroupTypeId = group.GroupTypeId;
                groupInfo.CampusId = group.CampusId;
                groupInfo.Campus = group.CampusId.HasValue ? group.Campus.Name : "";
                groupInfo.Name = group.Name;
                groupInfo.Capacity = group.GroupCapacity;

                groupInfo.Description = group.Description.Truncate( characterLimit );
                groupInfo.Color = defaultColor;

                groupInfo.GroupLeaders = GetGroupLeaders( group );
                groupInfo.MeetingLocations = GetMeetingLocations( group );
                groupInfo.GroupMemberCount = GetGroupMemberCount( qryOptions, group );
                groupInfo.AttributeValues = GetRequestedAttributeValues( requestedAttributeIds, group, requestedAttributeList, attributeCacheList );
                groupInfo.LifeStage = GetGroupLifeStage( group );

                BuildGroupSchedule( group, groupInfo );
                BuildCategories( definedValueService, qryOptions.CategoryKey, group, groupInfo );

                groupInfoList.Add( groupInfo );
            }

            var groupInfoListQry = groupInfoList.AsQueryable().OrderBy( g => qryOptions.SortByGroupTypeOrder );
            if ( qryOptions.SortByGroupTypeOrder )
            {
                foreach ( var groupTypeId in qryOptions.GroupTypeIds.SplitDelimitedValues().AsIntegerList() )
                {
                    groupInfoListQry = groupInfoListQry.ThenByDescending( g => g.GroupTypeId == groupTypeId );
                }
            }

            groupInfoListQry = groupInfoListQry.ThenByDescending( g => g.Category == "Featured" )
                .ThenByDescending( g => g.Category == "New" )
                .ThenBy( g => g.Name );

            groupFinderInfo.Groups = groupInfoListQry.ToList();

            return groupFinderInfo;
        }

        /// <summary>
        /// Gets the groups information.
        /// </summary>
        /// <param name="groupTypeIds">The group type ids.</param>
        /// <param name="campusIds">The campus ids.</param>
        /// <param name="parentGroupIds">The parent group ids.</param>
        /// <param name="meetingDays">The meeting days.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="showPrivateGroups">if set to <c>true</c> [show private groups].</param>
        /// <param name="showInactiveGroups">if set to <c>true</c> [show inactive groups].</param>
        /// <param name="limitByCapacity">if set to <c>true</c> [limit by capacity].</param>
        /// <param name="includePendingMembersInGroupCapacity">if set to <c>true</c> [include pending members in group capacity].</param>
        /// <param name="requestedAttributeIds">The requested attribute ids.</param>
        /// <returns>IQueryable&lt;GroupInformation&gt;.</returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "GetGroupsInformation" )]
        public IQueryable<GroupInformation> GetGroupsInformationV2(
          string groupTypeIds = "",
          string campusIds = "",
          string parentGroupIds = "",
          string meetingDays = "",
          int? offset = null,
          int? limit = null,
          bool showPrivateGroups = false,
          bool showInactiveGroups = false,
            bool limitByCapacity = false,
          bool includePendingMembersInGroupCapacity = false,
          string requestedAttributeIds = "" )
        {
            var currentPerson = GetPerson();
            var rockContext = new RockContext();
            var groupInfoList = new List<GroupInformation>();
            var definedValueService = new DefinedValueService( rockContext );
            int entityTypeId = EntityTypeCache.GetId( typeof( Group ) ) ?? 0;

            var qryOptions = new GroupQueryOptions
            {
                GroupTypeIds = groupTypeIds,
                CampusIds = campusIds,
                ParentGroupIds = parentGroupIds,
                MeetingDays = meetingDays,

                ShowPrivateGroups = showPrivateGroups,
                ShowInactiveGroups = showInactiveGroups,
                LimitByCapacity = limitByCapacity,
                IncludePendingMembersInGroupCapacity = includePendingMembersInGroupCapacity,
            };

            IQueryable<Group> qry = FilterGroups( rockContext, currentPerson, qryOptions );

            if ( offset.HasValue )
            {
                qry = qry.Skip( offset.Value );
            }

            if ( limit.HasValue )
            {
                qry = qry.Take( limit.Value );
            }

            var requestedAttributeIdList = requestedAttributeIds.SplitDelimitedValues().AsIntegerList();
            Dictionary<string, AttributeInformation> requestedAttributeList = new Dictionary<string, AttributeInformation>();
            Dictionary<string, AttributeCache> attributeCacheList = new Dictionary<string, AttributeCache>();
            foreach ( var requestedAttributeId in requestedAttributeIdList )
            {
                var requestedAttribute = GetAttributeInformation( currentPerson, requestedAttributeId );
                if ( requestedAttribute.Key != null )
                {
                    requestedAttributeList.AddOrReplace( requestedAttribute.Key, requestedAttribute );
                    attributeCacheList.AddOrReplace( requestedAttribute.Key, AttributeCache.Get( requestedAttribute.Id ) );
                }
            }

            foreach ( var group in qry.ToList() )
            {
                group.LoadAttributes();

                var groupInfo = new GroupInformation();
                groupInfo.Id = group.Id;
                groupInfo.Guid = group.Guid;
                groupInfo.GroupTypeId = group.GroupTypeId;
                groupInfo.CampusId = group.CampusId;
                groupInfo.Campus = group.CampusId.HasValue ? group.Campus.Name : "";
                groupInfo.Name = group.Name;
                groupInfo.Description = group.Description;
                groupInfo.Capacity = group.GroupCapacity;

                groupInfo.GroupLeaders = GetGroupLeaders( group );
                groupInfo.MeetingLocations = GetMeetingLocations( group );
                groupInfo.GroupMemberCount = GetGroupMemberCount( qryOptions, group );
                groupInfo.AttributeValues = GetRequestedAttributeValues( requestedAttributeIds, group, requestedAttributeList, attributeCacheList );

                BuildGroupSchedule( group, groupInfo );

                groupInfoList.Add( groupInfo );
            }

            var groupInfoQry = groupInfoList.AsQueryable()
                .OrderByDescending( g => g.Name )
                .AsQueryable();

            return groupInfoQry;
        }

        /// <summary>
        /// Gets the groups member count.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <returns>System.Nullable&lt;System.Int32&gt;.</returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "GetGroupsMemberCount" )]
        public int? GetGroupsMemberCount(
          int? groupId = null )
        {
            var currentPerson = GetPerson();
            int? memberCount = null;

            if ( groupId.HasValue )
            {
                var rockContext = new RockContext();
                var groupService = new GroupService( rockContext );

                var group = groupService.Get( groupId.Value );
                if ( group != null )
                {
                    if ( group.IsAuthorized( Authorization.VIEW, currentPerson ) )
                    {
                        memberCount = group.ActiveMembers().Count();
                    }
                }
            }

            return memberCount;
        }

        /// <summary>
        /// Gets the attribute information.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <returns>AttributeInformation.</returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "GetAttributeInformation" )]
        public AttributeInformation GetAttributeInformation(
          int? attributeId = null )
        {
            var currentPerson = GetPerson();
            var attributeInformation = new AttributeInformation();

            if ( attributeId.HasValue )
            {
                attributeInformation = GetAttributeInformation( currentPerson, attributeId.Value );
            }

            return attributeInformation;
        }

        /// <summary>
        /// Gets the attributes information.
        /// </summary>
        /// <param name="attributeIds">The attribute ids.</param>
        /// <returns>List&lt;AttributeInformation&gt;.</returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "GetAttributeInformationList" )]
        public List<AttributeInformation> GetAttributesInformation(
          string attributeIds = "" )
        {
            var currentPerson = GetPerson();
            var attributeInformationList = new List<AttributeInformation>();

            var attributeIdList = attributeIds.SplitDelimitedValues().AsIntegerList();
            foreach ( var attributeId in attributeIdList )
            {
                var attributeInformation = GetAttributeInformation( currentPerson, attributeId );
                attributeInformationList.Add( attributeInformation );
            }

            return attributeInformationList;
        }

        /// <summary>
        /// Gets the groups.
        /// </summary>
        /// <param name="groupTypeIds">The group type ids.</param>
        /// <param name="campusIds">The campus ids.</param>
        /// <param name="parentGroupIds">The parent group ids.</param>
        /// <param name="meetingDays">The meeting days.</param>
        /// <param name="categoryIds">The category ids.</param>
        /// <param name="age">The age.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="characterLimit">The character limit.</param>
        /// <param name="keywords">The keywords.</param>
        /// <param name="showPrivateGroups">if set to <c>true</c> [show private groups].</param>
        /// <param name="showInactiveGroups">if set to <c>true</c> [show inactive groups].</param>
        /// <param name="optionalFilterAttributeKey">The optional filter attribute key.</param>
        /// <param name="optionalFilterIds">The optional filter ids.</param>
        /// <param name="alternateCategoryKey">The alternate category key.</param>
        /// <param name="secondaryCategoryAttributeKey">The secondary category attribute key.</param>
        /// <param name="secondaryCategoryFilterIds">The secondary category filter ids.</param>
        /// <param name="includeGroupsWithoutCampus">if set to <c>true</c> [include groups without campus].</param>
        /// <param name="limitByCapacity">if set to <c>true</c> [limit by capacity].</param>
        /// <param name="includePendingMembersInGroupCapacity">if set to <c>true</c> [include pending members in group capacity].</param>
        /// <param name="requestedAttributeIds">The requested attribute ids.</param>
        /// <param name="defaultColor">The default color.</param>
        /// <returns>IQueryable&lt;GroupInformation&gt;.</returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "GetGroups" )]
        public IQueryable<GroupInformation> GetGroupsV1(
            string groupTypeIds = "",
            string campusIds = "",
            string parentGroupIds = "",
            string meetingDays = "",
            string categoryIds = "",
            string age = "",
            int? offset = null,
            int? limit = null,
            int characterLimit = 150,
            string keywords = "",
            bool showPrivateGroups = false,
            bool showInactiveGroups = false,
            string optionalFilterAttributeKey = null,
            string optionalFilterIds = null,
            string alternateCategoryKey = null,
            string secondaryCategoryAttributeKey = null,
            string secondaryCategoryFilterIds = null,
            bool includeGroupsWithoutCampus = false,
            bool limitByCapacity = false,
            bool includePendingMembersInGroupCapacity = false,
            string requestedAttributeIds = "",
            string defaultColor = "#428bca" )
        {
            var currentPerson = GetPerson();

            var rockContext = new RockContext();
            var groupInfoList = new List<GroupInformation>();
            var definedValueService = new DefinedValueService( rockContext );
            int entityTypeId = EntityTypeCache.GetId( typeof( Group ) ) ?? 0;

            var qryOptions = new GroupQueryOptions
            {
                GroupTypeIds = groupTypeIds,
                CampusIds = campusIds,
                ParentGroupIds = parentGroupIds,
                MeetingDays = meetingDays,
                Age = age,
                Keywords = keywords,

                CategoryKey = alternateCategoryKey.IsNotNullOrWhiteSpace() ? alternateCategoryKey : "Category",
                CategoryIds = categoryIds,
                OptionalFilterAttributeKey = optionalFilterAttributeKey,
                OptionalFilterIds = optionalFilterIds,
                SecondaryCategoryAttributeKey = secondaryCategoryAttributeKey,
                SecondaryCategoryFilterIds = secondaryCategoryFilterIds,

                ShowPrivateGroups = showPrivateGroups,
                ShowInactiveGroups = showInactiveGroups,
                IncludeGroupsWithoutCampus = includeGroupsWithoutCampus,
                LimitByCapacity = limitByCapacity,
                IncludePendingMembersInGroupCapacity = includePendingMembersInGroupCapacity
            };

            IQueryable<Group> qry = FilterGroups( rockContext, currentPerson, qryOptions );

            var categoryValues = new AttributeValueService( rockContext ).Queryable().AsNoTracking()
                .Where( a => a.Attribute.Key == qryOptions.CategoryKey )
                .Where( a => a.Attribute.EntityTypeId == entityTypeId );

            var groupQry = from g in qry
                           join av in categoryValues on g.Id equals av.EntityId into gav
                           from x in gav.DefaultIfEmpty()
                           select new
                           {
                               Group = g,
                               Categories = x.Value.ToUpper()
                           };

            groupQry = groupQry.OrderByDescending( g => g.Categories.Contains( "99BC9586-3C23-4BE4-BEB3-2285FBBCD1C9" ) )
                    .ThenByDescending( g => g.Categories.Contains( "3F3EFCE6-8DEB-4F3E-A6F8-E9E6FE3A1852" ) )
                .ThenBy( g => g.Group.Name );

            if ( offset.HasValue )
            {
                groupQry = groupQry.Skip( offset.Value );
            }

            if ( limit.HasValue )
            {
                groupQry = groupQry.Take( limit.Value );
            }

            var requestedAttributeIdList = requestedAttributeIds.SplitDelimitedValues().AsIntegerList();
            Dictionary<string, AttributeInformation> requestedAttributeList = new Dictionary<string, AttributeInformation>();
            Dictionary<string, AttributeCache> attributeCacheList = new Dictionary<string, AttributeCache>();
            foreach ( var requestedAttributeId in requestedAttributeIdList )
            {
                var requestedAttribute = GetAttributeInformation( currentPerson, requestedAttributeId );
                if ( requestedAttribute.Key != null )
                {
                    requestedAttributeList.AddOrReplace( requestedAttribute.Key, requestedAttribute );
                    attributeCacheList.AddOrReplace( requestedAttribute.Key, AttributeCache.Get( requestedAttribute.Id ) );
                }
            }

            foreach ( var group in groupQry.Select( g => g.Group ).ToList() )
            {
                group.LoadAttributes();

                var groupInfo = new GroupInformation();
                groupInfo.Id = group.Id;
                groupInfo.Guid = group.Guid;
                groupInfo.GroupTypeId = group.GroupTypeId;
                groupInfo.CampusId = group.CampusId;
                groupInfo.Campus = group.CampusId.HasValue ? group.Campus.Name : "";
                groupInfo.Name = group.Name;
                groupInfo.Capacity = group.GroupCapacity;

                groupInfo.Description = group.Description.Truncate( characterLimit );
                groupInfo.Color = defaultColor;

                groupInfo.GroupLeaders = GetGroupLeaders( group );
                groupInfo.MeetingLocations = GetMeetingLocations( group );
                groupInfo.GroupMemberCount = GetGroupMemberCount( qryOptions, group );
                groupInfo.AttributeValues = GetRequestedAttributeValues( requestedAttributeIds, group, requestedAttributeList, attributeCacheList );
                groupInfo.LifeStage = GetGroupLifeStage( group );

                BuildGroupSchedule( group, groupInfo );
                BuildCategories( definedValueService, qryOptions.CategoryKey, group, groupInfo );

                groupInfoList.Add( groupInfo );
            }

            var groupInfoQry = groupInfoList.AsQueryable()
                .OrderByDescending( g => g.Category == "Featured" )
                .ThenByDescending( g => g.Category == "New" )
                .ThenBy( g => g.Name )
                .AsQueryable();

            return groupInfoQry;
        }

        /// <summary>
        /// Gets the group count.
        /// </summary>
        /// <param name="groupTypeIds">The group type ids.</param>
        /// <param name="campusIds">The campus ids.</param>
        /// <param name="parentGroupIds">The parent group ids.</param>
        /// <param name="meetingDays">The meeting days.</param>
        /// <param name="categoryIds">The category ids.</param>
        /// <param name="age">The age.</param>
        /// <param name="keywords">The keywords.</param>
        /// <param name="showPrivateGroups">if set to <c>true</c> [show private groups].</param>
        /// <param name="showInactiveGroups">if set to <c>true</c> [show inactive groups].</param>
        /// <param name="optionalFilterAttributeKey">The optional filter attribute key.</param>
        /// <param name="optionalFilterIds">The optional filter ids.</param>
        /// <param name="alternateCategoryKey">The alternate category key.</param>
        /// <param name="secondaryCategoryAttributeKey">The secondary category attribute key.</param>
        /// <param name="secondaryCategoryFilterIds">The secondary category filter ids.</param>
        /// <param name="includeGroupsWithoutCampus">if set to <c>true</c> [include groups without campus].</param>
        /// <param name="limitByCapacity">if set to <c>true</c> [limit by capacity].</param>
        /// <param name="includePendingMembersInGroupCapacity">if set to <c>true</c> [include pending members in group capacity].</param>
        /// <returns>System.Int32.</returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "GetGroupCount" )]
        public int GetGroupCount(
            string groupTypeIds = "",
            string campusIds = "",
            string parentGroupIds = "",
            string meetingDays = "",
            string categoryIds = "",
            string age = "",
            string keywords = "",
            bool showPrivateGroups = false,
            bool showInactiveGroups = false,
            string optionalFilterAttributeKey = null,
            string optionalFilterIds = null,
            string alternateCategoryKey = null,
            string secondaryCategoryAttributeKey = null,
            string secondaryCategoryFilterIds = null,
            bool includeGroupsWithoutCampus = false,
            bool limitByCapacity = false,
            bool includePendingMembersInGroupCapacity = false,
            bool includeGroupsWithoutCapacity = false )
        {
            var currentPerson = GetPerson();

            var qryOptions = new GroupQueryOptions
            {
                GroupTypeIds = groupTypeIds,
                CampusIds = campusIds,
                ParentGroupIds = parentGroupIds,
                MeetingDays = meetingDays,
                Age = age,
                Keywords = keywords,

                CategoryKey = alternateCategoryKey.IsNotNullOrWhiteSpace() ? alternateCategoryKey : "Category",
                CategoryIds = categoryIds,
                OptionalFilterAttributeKey = optionalFilterAttributeKey,
                OptionalFilterIds = optionalFilterIds,
                SecondaryCategoryAttributeKey = secondaryCategoryAttributeKey,
                SecondaryCategoryFilterIds = secondaryCategoryFilterIds,

                ShowPrivateGroups = showPrivateGroups,
                ShowInactiveGroups = showInactiveGroups,
                IncludeGroupsWithoutCampus = includeGroupsWithoutCampus,
                LimitByCapacity = limitByCapacity,
                IncludePendingMembersInGroupCapacity = includePendingMembersInGroupCapacity,
                IncludeGroupsWithoutCapacity = includeGroupsWithoutCapacity
            };

            IQueryable<Group> qry = FilterGroups( new RockContext(), currentPerson, qryOptions );

            return qry.Count();
        }

        #endregion API Calls

        #region Internal Methods
        /// <summary>
        /// Gets the attribute information.
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="requestedAttributeId">The requested attribute identifier.</param>
        /// <returns>AttributeInformation.</returns>
        private AttributeInformation GetAttributeInformation( Person currentPerson, int requestedAttributeId )
        {
            var attributeInformation = new AttributeInformation();

            var attributeCache = AttributeCache.Get( requestedAttributeId );
            if ( attributeCache != null )
            {
                if ( attributeCache.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    attributeInformation.Id = attributeCache.Id;
                    attributeInformation.Name = attributeCache.Name;
                    attributeInformation.Key = attributeCache.Key;
                    attributeInformation.IsPublic = attributeCache.IsPublic;
                    attributeInformation.FieldType = attributeCache.FieldType.Name;

                    attributeInformation.Options = new List<FilterOption>();
                    var configurationValues = attributeCache.QualifierValues;
                    if ( attributeCache.FieldType.Guid == Rock.SystemGuid.FieldType.DEFINED_VALUE.AsGuid() )
                    {
                        int? definedTypeId = configurationValues != null && configurationValues.ContainsKey( "definedtype" ) ? configurationValues["definedtype"].Value.AsIntegerOrNull() : null;
                        bool useDescription = configurationValues != null && configurationValues.ContainsKey( "displaydescription" ) && configurationValues["displaydescription"].Value.AsBoolean();
                        var dt = DefinedTypeCache.Get( definedTypeId.Value );
                        var definedValuesList = dt?.DefinedValues
                            .Where( a => a.IsActive )
                            .OrderBy( v => v.Order ).ThenBy( v => v.Value ).ToList();

                        if ( definedValuesList != null && definedValuesList.Any() )
                        {
                            foreach ( var definedValue in definedValuesList )
                            {
                                var filterOption = new FilterOption();
                                filterOption.Text = useDescription && definedValue.Description.IsNotNullOrWhiteSpace() ? definedValue.Description : definedValue.Value;
                                filterOption.Value = definedValue.Guid.ToString();
                                attributeInformation.Options.Add( filterOption );
                            }
                        }
                    }
                    else if ( attributeCache.FieldType.Guid == Rock.SystemGuid.FieldType.BOOLEAN.AsGuid() )
                    {
                        string trueText = "Yes";
                        var trueQualifierValue = attributeCache.QualifierValues["truetext"];
                        if ( trueQualifierValue != null )
                        {
                            trueText = trueQualifierValue.Value.ToStringOrDefault( "Yes" );
                        }

                        string falseText = "No";
                        var falseQualifierValue = attributeCache.QualifierValues["falsetext"];
                        if ( falseQualifierValue != null )
                        {
                            falseText = falseQualifierValue.Value.ToStringOrDefault( "No" );
                        }

                        var trueOption = new FilterOption();
                        trueOption.Text = trueText;
                        trueOption.Value = "True";
                        attributeInformation.Options.Add( trueOption );

                        var falseOption = new FilterOption();
                        falseOption.Text = falseText;
                        falseOption.Value = "False";
                        attributeInformation.Options.Add( falseOption );
                    }
                    else
                    {
                        var configuredValues = Rock.Field.Helper.GetConfiguredValues( configurationValues ); // I am not sure what needs to be done here.
                        foreach ( var configuredValue in configuredValues )
                        {
                            var filterOption = new FilterOption();
                            filterOption.Text = configuredValue.Key;
                            filterOption.Value = configuredValue.Value;
                            attributeInformation.Options.Add( filterOption );
                        }
                    }
                }
            }

            return attributeInformation;
        }

        /// <summary>
        /// Builds the categories.
        /// </summary>
        /// <param name="definedValueService">The defined value service.</param>
        /// <param name="categoryKey">The category key.</param>
        /// <param name="group">The group.</param>
        /// <param name="groupInfo">The group information.</param>
        private static void BuildCategories( DefinedValueService definedValueService, string categoryKey, Group group, GroupInformation groupInfo )
        {
            var categoryGuids = group.GetAttributeValue( categoryKey ).SplitDelimitedValues().AsGuidList();
            if ( categoryGuids.Any() )
            {
                var categories = definedValueService.GetByGuids( categoryGuids );
                if ( categories.Any() )
                {
                    var primaryCategory = categories.OrderBy( c => c.Order ).First();
                    groupInfo.Category = primaryCategory.Value;
                    groupInfo.Color = GetCategoryColor( primaryCategory );

                    List<CategoryColor> secondaryCategories = new List<CategoryColor>();
                    foreach ( var category in categories.Where( c => c.Id != primaryCategory.Id ).OrderBy( c => c.Order ).ToList() )
                    {
                        var secondaryCategory = new CategoryColor();
                        secondaryCategory.Category = category.Value;
                        secondaryCategory.Color = GetCategoryColor( category );
                        secondaryCategories.Add( secondaryCategory );
                    }

                    groupInfo.SecondaryCategories = secondaryCategories;
                }
            }
        }

        /// <summary>
        /// Builds the group schedule.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="groupInfo">The group information.</param>
        private static void BuildGroupSchedule( Group group, GroupInformation groupInfo )
        {
            if ( group.Schedule != null )
            {
                var schedule = group.Schedule;
                groupInfo.Frequency = "Custom";

                if ( schedule.WeeklyDayOfWeek.HasValue && schedule.WeeklyTimeOfDay.HasValue )
                {
                    groupInfo.Frequency = "Weekly";
                    groupInfo.DayOfWeek = schedule.WeeklyDayOfWeek.Value.ConvertToString().Substring( 0, 3 );

                    if ( schedule.WeeklyTimeOfDay.HasValue )
                    {
                        groupInfo.TimeOfDay = schedule.WeeklyTimeOfDay.Value.ToTimeString();
                    }
                }
                else
                {
                    var calendarEvent = schedule.GetICalEvent();
                    if ( calendarEvent != null && calendarEvent.Start != null )
                    {
                        string startTimeText = calendarEvent.Start.Value.TimeOfDay.ToTimeString();
                        groupInfo.TimeOfDay = startTimeText;

                        if ( calendarEvent.RecurrenceRules.Any() )
                        {
                            // some type of recurring schedule

                            var rrule = calendarEvent.RecurrenceRules[0];
                            if ( rrule.Interval == 1 )
                            {
                                groupInfo.Frequency = rrule.Frequency.ToString();
                            }
                            if ( rrule.Interval == 2 )
                            {
                                groupInfo.Frequency = string.Format( "Bi{0}", rrule.Frequency.ToString().ToLower() );
                            }

                            if ( rrule.ByDay.Count == 1 )
                            {
                                var byDay = rrule.ByDay.First();
                                groupInfo.DayOfWeek = byDay.DayOfWeek.ConvertToString().Substring( 0, 3 );
                            }
                            else
                            {
                                groupInfo.DayOfWeek = "Varies";
                            }
                        }
                        else
                        {
                            if ( calendarEvent.RecurrenceDates.Any() )
                            {
                                var dates = calendarEvent.RecurrenceDates.SelectMany( d => d.Select( d1 => d1.StartTime ) );
                                var weekDays = dates.Select( d => d.DayOfWeek ).Distinct().ToList();

                                weekDays.Add( calendarEvent.Start.Value.DayOfWeek, true );

                                if ( weekDays.Count() == 1 )
                                {
                                    groupInfo.DayOfWeek = weekDays.First().ConvertToString().Substring( 0, 3 );
                                }
                                else
                                {
                                    groupInfo.DayOfWeek = "Varies";
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                groupInfo.DayOfWeek = "N/A";
                groupInfo.Frequency = "No Schedule";
            }
        }

        /// <summary>
        /// Gets the group life stage.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns>System.String.</returns>
        private static string GetGroupLifeStage( Group group )
        {
            var groupInfoLifeStage = "";
            var maximumAge = group.GetAttributeValue( "MaximumAge" ).AsIntegerOrNull();
            var minimumAge = group.GetAttributeValue( "MinimumAge" ).AsIntegerOrNull();
            if ( minimumAge.HasValue || maximumAge.HasValue )
            {
                if ( !minimumAge.HasValue )
                {
                    groupInfoLifeStage = String.Format( "{0} & Under", maximumAge.Value );
                }
                else
                {
                    if ( !maximumAge.HasValue )
                    {
                        groupInfoLifeStage = String.Format( "{0} & Up", minimumAge.Value );
                    }
                    else
                    {
                        groupInfoLifeStage = String.Format( "{0} - {1}", minimumAge.Value, maximumAge.Value );
                    }
                }
            }
            else
            {
                groupInfoLifeStage = "";
            }

            return groupInfoLifeStage;
        }

        /// <summary>
        /// Gets the group member count.
        /// </summary>
        /// <param name="includePendingMembersInGroupCapacity">if set to <c>true</c> [include pending members in group capacity].</param>
        /// <param name="group">The group.</param>
        /// <returns>System.Int32.</returns>
        private static int GetGroupMemberCount( GroupQueryOptions groupQueryOptions, Group group )
        {
            var groupInfoMemberCount = 0;
            var groupMembers = group.Members.AsEnumerable();
            if ( groupQueryOptions.IncludePendingMembersInGroupCapacity )
            {
                groupMembers = groupMembers.Where( gm => gm.GroupMemberStatus != GroupMemberStatus.Inactive && gm.IsArchived == false );
            }
            else
            {
                groupMembers = groupMembers.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active );
            }

            if ( groupQueryOptions.CalculateCapacityByPerson )
            {
                groupInfoMemberCount = groupMembers.Select( gm => gm.PersonId ).Distinct().Count();
            }
            else
            {
                groupInfoMemberCount = groupMembers.Count();
            }

            return groupInfoMemberCount;
        }

        /// <summary>
        /// Gets the meeting locations.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns>List&lt;LocationInformation&gt;.</returns>
        private static List<LocationInformation> GetMeetingLocations( Group group )
        {
            var groupInfoMeetingLocations = new List<LocationInformation>();
            foreach ( var meetingLocation in group.GroupLocations )
            {
                var locationInfo = new LocationInformation();
                var location = meetingLocation.Location;
                if ( location != null )
                {
                    locationInfo.LocationId = location.Id;
                    locationInfo.Name = location.Name;
                    locationInfo.Street1 = location.Street1;
                    locationInfo.Street2 = location.Street2;
                    locationInfo.City = location.City;
                    locationInfo.County = location.County;
                    locationInfo.State = location.State;
                    locationInfo.PostalCode = location.PostalCode;
                    locationInfo.Country = location.Country;
                    if ( location.GeoPoint != null )
                    {
                        locationInfo.Point = new MapCoordinate( location.GeoPoint.Latitude, location.GeoPoint.Longitude );
                        locationInfo.Point.Latitude = locationInfo.Point.Latitude != null ? Convert.ToDouble( locationInfo.Point.Latitude.Value.ToString( "#.###" ) ) : ( double? ) null;
                        locationInfo.Point.Longitude = locationInfo.Point.Longitude != null ? Convert.ToDouble( locationInfo.Point.Longitude.Value.ToString( "#.###" ) ) : ( double? ) null;
                    }

                    groupInfoMeetingLocations.Add( locationInfo );
                }
            }

            return groupInfoMeetingLocations;
        }

        /// <summary>
        /// Gets the group leaders.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns>List&lt;GroupMemberInformation&gt;.</returns>
        private static List<GroupMemberInformation> GetGroupLeaders( Group group )
        {
            var groupInfoGroupLeaders = new List<GroupMemberInformation>();
            foreach ( var member in group.Members )
            {
                if ( member.GroupRole.IsLeader )
                {
                    var groupMemberInfo = new GroupMemberInformation();
                    var groupMemberPerson = member.Person;

                    groupMemberInfo.FirstName = groupMemberPerson.FirstName;
                    groupMemberInfo.NickName = groupMemberPerson.NickName;
                    groupMemberInfo.LastName = groupMemberPerson.LastName;
                    groupMemberInfo.Email = groupMemberPerson.Email;
                    groupMemberInfo.Role = member.GroupRole.Name;
                    groupMemberInfo.RoleId = member.GroupRoleId;
                    groupInfoGroupLeaders.Add( groupMemberInfo );
                }
            }

            return groupInfoGroupLeaders;
        }

        /// <summary>
        /// Gets the requested attribute values.
        /// </summary>
        /// <param name="requestedAttributeIds">The requested attribute ids.</param>
        /// <param name="group">The group.</param>
        /// <param name="requestedAttributeList">The requested attribute list.</param>
        /// <param name="attributeCacheList">The attribute cache list.</param>
        /// <returns>Dictionary&lt;System.String, AttributeValueInformation&gt;.</returns>
        private Dictionary<string, AttributeValueInformation> GetRequestedAttributeValues( string requestedAttributeIds, Group group, Dictionary<string, AttributeInformation> requestedAttributeList, Dictionary<string, AttributeCache> attributeCacheList )
        {

            var groupInfoAttributeValues = new Dictionary<string, AttributeValueInformation>();
            foreach ( var requestedAttribute in requestedAttributeList )
            {
                if ( attributeCacheList.ContainsKey( requestedAttribute.Key ) )
                {
                    var attributeCache = attributeCacheList[requestedAttribute.Key];
                    var field = attributeCache.FieldType.Field;

                    var attributeValue = new AttributeValueInformation();
                    attributeValue.Attribute = requestedAttribute.Value;
                    attributeValue.AttributeId = requestedAttribute.Value.Id;
                    attributeValue.AttributeKey = requestedAttribute.Value.Key;

                    attributeValue.RawValue = group.GetAttributeValue( attributeValue.AttributeKey );
                    attributeValue.FormattedValue = field.FormatValue( null, attributeCache.EntityTypeId, group.Id, attributeValue.RawValue, attributeCache.QualifierValues, false );

                    groupInfoAttributeValues.Add( requestedAttribute.Key, attributeValue );
                }
            }

            return groupInfoAttributeValues;
        }

        /// <summary>
        /// Gets the color of the category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns>System.String.</returns>
        private static string GetCategoryColor( DefinedValue category )
        {
            category.LoadAttributes();
            var rawValue = category.GetAttributeValue( "Color" );
            var colorString = string.Empty;
            if ( rawValue.IsNotNullOrWhiteSpace() )
            {
                if ( rawValue.Contains( "rgb" ) )
                {
                    var colorList = rawValue.Replace( "rgb(", "" ).Replace( ")", "" ).SplitDelimitedValues().AsIntegerList();
                    if ( colorList.Count == 3 )
                    {
                        Color myColor = Color.FromArgb( colorList[0], colorList[1], colorList[2] );
                        string hex = myColor.R.ToString( "X2" ) + myColor.G.ToString( "X2" ) + myColor.B.ToString( "X2" );

                        colorString = "#" + hex;
                    }
                }
                else
                {
                    colorString = rawValue;
                }
            }

            return colorString;
        }

        /// <summary>
        /// Filters the groups.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="groupTypeIds">The group type ids.</param>
        /// <param name="campusIds">The campus ids.</param>
        /// <param name="parentGroupIds">The parent group ids.</param>
        /// <param name="meetingDays">The meeting days.</param>
        /// <param name="categoryIds">The category ids.</param>
        /// <param name="age">The age.</param>
        /// <param name="keywords">The keywords.</param>
        /// <param name="showPrivateGroups">if set to <c>true</c> [show private groups].</param>
        /// <param name="showInactiveGroups">if set to <c>true</c> [show inactive groups].</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="optionalFilterAttributeKey">The optional filter attribute key.</param>
        /// <param name="optionalFilterIds">The optional filter ids.</param>
        /// <param name="alternateCategoryKey">The alternate category key.</param>
        /// <param name="secondaryCategoryAttributeKey">The secondary category attribute key.</param>
        /// <param name="secondaryCategoryFilterIds">The secondary category filter ids.</param>
        /// <param name="includeGroupsWithoutCampus">if set to <c>true</c> [include groups without campus].</param>
        /// <param name="limitByCapacity">if set to <c>true</c> [limit by capacity].</param>
        /// <param name="includePendingMembersInGroupCapacity">if set to <c>true</c> [include pending members in group capacity].</param>
        /// <returns>IQueryable&lt;Group&gt;.</returns>
        private static IQueryable<Group> FilterGroups(
            RockContext rockContext,
             Person person,
            GroupQueryOptions qryOptions )
        {
            var authorizedGroupTypeIds = new GroupTypeService( rockContext ).Queryable()
               .ToList().Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, person ) ).Select( a => a.Id ).ToList();

            var groupService = new GroupService( rockContext );
            var definedValueService = new DefinedValueService( rockContext );
            var qry = groupService.Queryable().AsNoTracking();

            var groupTypeIdList = qryOptions.GroupTypeIds.SplitDelimitedValues().AsIntegerList();
            var groupIdList = qryOptions.GroupIds.SplitDelimitedValues().AsIntegerList();
            var campusIdList = qryOptions.CampusIds.SplitDelimitedValues().AsIntegerList();
            var parentGroupIdList = qryOptions.ParentGroupIds.SplitDelimitedValues().AsIntegerList();
            var meetingDayIntegerList = qryOptions.MeetingDays.SplitDelimitedValues().AsIntegerList();
            var meetingDayList = meetingDayIntegerList.Where( i => i >= 0 && i <= 6 ).Select( i => i.ToString().ConvertToEnum<DayOfWeek>() ).ToList();
            var includeNonWeeklySchedules = meetingDayIntegerList.Where( i => i > 6 ).Any();
            var categoryIdList = qryOptions.CategoryIds.SplitDelimitedValues().AsIntegerList();

            qry = qry.Where( g => authorizedGroupTypeIds.Contains( g.GroupTypeId ) );

            if ( groupTypeIdList.Any() )
            {
                qry = qry.Where( g => groupTypeIdList.Contains( g.GroupTypeId ) );
            }

            // After filtering on security, if group ids were passed in, skip the remaining filtering and return those groups. 
            if ( groupIdList.Any() )
            {
                return qry.Where( g => groupIdList.Contains( g.Id ) );
            }

            if ( !qryOptions.ShowPrivateGroups )
            {
                qry = qry.Where( g => g.IsPublic );
            }

            if ( !qryOptions.ShowInactiveGroups )
            {
                qry = qry.Where( g => g.IsActive );
            }

            if ( qryOptions.LimitByCapacity )
            {
                qry = qry.Where( g => ( qryOptions.IncludeGroupsWithoutCapacity && g.GroupCapacity == null ) || g.GroupCapacity > g.Members.Where( gm => gm.IsArchived == false &&
                      (
                         gm.GroupMemberStatus == GroupMemberStatus.Active || ( qryOptions.IncludePendingMembersInGroupCapacity && gm.GroupMemberStatus == GroupMemberStatus.Pending )
                      )
                    ).Count() );
            }

            if ( campusIdList.Any() )
            {
                qry = qry.Where( g =>
                    ( g.CampusId.HasValue && campusIdList.Contains( g.CampusId.Value ) ) ||
                    ( !g.CampusId.HasValue && qryOptions.IncludeGroupsWithoutCampus )
                    );
            }

            if ( parentGroupIdList.Any() )
            {
                var descendantIds = new List<int>();
                foreach ( var parentGroupId in parentGroupIdList )
                {
                    descendantIds.AddRange( groupService.GetAllDescendentGroupIds( parentGroupId, true ) );
                }

                qry = qry.Where( g => descendantIds.Contains( g.Id ) );
            }

            if ( meetingDayList.Any() || includeNonWeeklySchedules )
            {
                qry = qry.Where( g => g.Schedule != null );

                if ( meetingDayList.Any() && !includeNonWeeklySchedules )
                {
                    qry = qry.Where( g => g.Schedule.WeeklyDayOfWeek != null && meetingDayList.Contains( g.Schedule.WeeklyDayOfWeek.Value ) );
                }

                if ( !meetingDayList.Any() && includeNonWeeklySchedules )
                {
                    qry = qry.Where( g => g.Schedule.iCalendarContent != null && g.Schedule.iCalendarContent != string.Empty );
                }

                if ( meetingDayList.Any() && includeNonWeeklySchedules )
                {
                    qry = qry.Where( g =>
                                        ( g.Schedule.WeeklyDayOfWeek != null && meetingDayList.Contains( g.Schedule.WeeklyDayOfWeek.Value ) ) ||
                                        ( g.Schedule.iCalendarContent != null && g.Schedule.iCalendarContent != string.Empty )
                                    );
                }
            }

            if ( categoryIdList.Any() )
            {
                var categoryList = definedValueService.GetByIds( categoryIdList ).Select( c => c.Guid.ToString().ToUpper() ).ToList();
                qry = qry.WhereAttributeValue( rockContext, av => av.Attribute.Key == qryOptions.CategoryKey && categoryList.Any( c => av.Value.ToUpper().Contains( c ) ) );
            }

            if ( qryOptions.OptionalFilterAttributeKey.IsNotNullOrWhiteSpace() && qryOptions.OptionalFilterIds.IsNotNullOrWhiteSpace() )
            {
                var optionalFilterIdList = qryOptions.OptionalFilterIds.SplitDelimitedValues().AsIntegerList();
                if ( optionalFilterIdList.Any() )
                {
                    var optionalFilterList = definedValueService.GetByIds( optionalFilterIdList ).Select( c => c.Guid.ToString().ToUpper() ).ToList();
                    qry = qry.WhereAttributeValue( rockContext, av => av.Attribute.Key == qryOptions.OptionalFilterAttributeKey && optionalFilterList.Any( c => av.Value.ToUpper().Contains( c ) ) );
                }
            }

            if ( qryOptions.SecondaryCategoryAttributeKey.IsNotNullOrWhiteSpace() && qryOptions.SecondaryCategoryFilterIds.IsNotNullOrWhiteSpace() )
            {
                var sceondaryCategoryIdList = qryOptions.SecondaryCategoryFilterIds.SplitDelimitedValues().AsIntegerList();
                if ( sceondaryCategoryIdList.Any() )
                {
                    var secondaryCategoryList = definedValueService.GetByIds( sceondaryCategoryIdList ).Select( c => c.Guid.ToString().ToUpper() ).ToList();
                    qry = qry.WhereAttributeValue( rockContext, av => av.Attribute.Key == qryOptions.SecondaryCategoryAttributeKey && secondaryCategoryList.Any( c => av.Value.ToUpper().Contains( c ) ) );
                }
            }

            var ageInt = qryOptions.Age.AsIntegerOrNull();
            if ( ageInt.HasValue )
            {
                int entityTypeId = EntityTypeCache.GetId( typeof( Group ) ) ?? 0;

                var excludedMinimumAgeIds = new AttributeValueService( rockContext ).Queryable().AsNoTracking()
                    .Where( a => a.Attribute.Key == "MinimumAge" )
                    .Where( a => a.Attribute.EntityTypeId == entityTypeId )
                    .Where( a => a.ValueAsNumeric.HasValue && a.ValueAsNumeric > ageInt.Value )
                    .Select( a => a.EntityId );

                var excludedMaximumAgeIds = new AttributeValueService( rockContext ).Queryable().AsNoTracking()
                    .Where( a => a.Attribute.Key == "MaximumAge" )
                    .Where( a => a.Attribute.EntityTypeId == entityTypeId )
                    .Where( a => a.ValueAsNumeric.HasValue && a.ValueAsNumeric < ageInt.Value )
                    .Select( a => a.EntityId );

                qry = qry.Where( g => !excludedMaximumAgeIds.Contains( g.Id ) && !excludedMinimumAgeIds.Contains( g.Id ) );
            }

            if ( qryOptions.Keywords.IsNotNullOrWhiteSpace() )
            {
                qry = qry.Where( g => g.Description.Contains( qryOptions.Keywords ) || g.Name.Contains( qryOptions.Keywords ) );
            }

            return qry;
        }

        #endregion Internal Methods      
    }

    public class GroupQueryOptions
    {
        public string GroupTypeIds { get; set; } = "";
        public string GroupIds { get; set; } = "";
        public bool SortByGroupTypeOrder { get; set; } = false;
        public string CampusIds { get; set; } = "";
        public string ParentGroupIds { get; set; } = "";
        public string MeetingDays { get; set; } = "";
        public string CategoryIds { get; set; } = "";
        public string Age { get; set; } = "";
        public string Keywords { get; set; } = "";
        public bool ShowPrivateGroups { get; set; } = false;
        public bool ShowInactiveGroups { get; set; } = false;
        public string OptionalFilterAttributeKey { get; set; } = "";
        public string OptionalFilterIds { get; set; } = "";
        public string SecondaryCategoryAttributeKey { get; set; } = "";
        public string SecondaryCategoryFilterIds { get; set; } = "";
        public bool IncludeGroupsWithoutCampus { get; set; } = false;
        public bool LimitByCapacity { get; set; } = false;
        public bool IncludePendingMembersInGroupCapacity { get; set; } = false;
        public bool CalculateCapacityByPerson { get; set; } = false;
        public bool IncludeGroupsWithoutCapacity { get; set; } = false;
        public string CategoryKey { get; set; } = "Category";
        public bool SkipLeaders { get; set; } = false;
        public bool SkipMeetingLocations { get; set; } = false;
        public bool SkipGroupMemberCount { get; set; } = false;
        public bool SkipLifeStage { get; set; } = false;
        public bool SkipSchedule { get; set; } = false;
        public bool SkipCategories { get; set; } = false;
    }

    public class GroupFinderInformation
    {
        public int TotalCount { get; set; }
        public int? Limit { get; set; }
        public int? Offset { get; set; }
        public List<FilterOption> MeetingDayOptions { get; set; }
        public List<FilterOption> CategoryOptions { get; set; }
        public List<FilterOption> SecondaryCategoryOptions { get; set; }
        public List<GroupInformation> Groups { get; set; }
        public List<int> GroupIds { get; set; }
        public string GroupIdList { get; set; }

    }

    /// <summary>
    /// A class to store group data to be returned by the API
    /// </summary>
    public class GroupInformation
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>The unique identifier.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the group type identifier.
        /// </summary>
        /// <value>The group type identifier.</value>
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the group type sort order.
        /// </summary>
        /// <value>The group type sort order.</value>
        public int GroupTypeSortOrder { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>The campus identifier.</value>
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>The campus.</value>
        public string Campus { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the day of week.
        /// </summary>
        /// <value>The day of week.</value>
        public string DayOfWeek { get; set; }

        /// <summary>
        /// Gets or sets the time of day.
        /// </summary>
        /// <value>The time of day.</value>
        public string TimeOfDay { get; set; }

        /// <summary>
        /// Gets or sets the frequency.
        /// </summary>
        /// <value>The frequency.</value>
        public string Frequency { get; set; }

        /// <summary>
        /// Gets or sets the friendly schedule text.
        /// </summary>
        /// <value>The friendly schedule text.</value>
        public string FriendlyScheduleText { get; set; }

        /// <summary>
        /// Gets or sets the life stage.
        /// </summary>
        /// <value>The life stage.</value>
        public string LifeStage { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>The category.</value>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the secondary categories.
        /// </summary>
        /// <value>The secondary categories.</value>
        public List<CategoryColor> SecondaryCategories { get; set; }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the capacity.
        /// </summary>
        /// <value>The capacity.</value>
        public int? Capacity { get; set; }

        /// <summary>
        /// Gets or sets the group member count.
        /// </summary>
        /// <value>The group member count.</value>
        public int? GroupMemberCount { get; set; }

        /// <summary>
        /// Gets or sets the attribute values.
        /// </summary>
        /// <value>The attribute values.</value>
        public Dictionary<string, AttributeValueInformation> AttributeValues { get; set; }
        /// <summary>
        /// Gets or sets the group leaders.
        /// </summary>
        /// <value>The group leaders.</value>
        public List<GroupMemberInformation> GroupLeaders { get; set; }
        /// <summary>
        /// Gets or sets the meeting locations.
        /// </summary>
        /// <value>The meeting locations.</value>
        public List<LocationInformation> MeetingLocations { get; set; }
    }

    /// <summary>
    /// Class CategoryColor.
    /// </summary>
    public class CategoryColor
    {
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>The category.</value>
        public string Category { get; set; }
        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        public string Color { get; set; }
    }

    /// <summary>
    /// Class AttributeValueInformation.
    /// </summary>
    public class AttributeValueInformation
    {
        /// <summary>
        /// Gets or sets the attribute identifier.
        /// </summary>
        /// <value>The attribute identifier.</value>
        public int AttributeId { get; set; }
        /// <summary>
        /// Gets or sets the attribute key.
        /// </summary>
        /// <value>The attribute key.</value>
        public string AttributeKey { get; set; }
        /// <summary>
        /// Gets or sets the attribute.
        /// </summary>
        /// <value>The attribute.</value>
        public AttributeInformation Attribute { get; set; }
        /// <summary>
        /// Gets or sets the formatted value.
        /// </summary>
        /// <value>The formatted value.</value>
        public string FormattedValue { get; set; }

        /// <summary>
        /// Gets or sets the raw value.
        /// </summary>
        /// <value>The raw value.</value>
        public string RawValue { get; set; }
    }
    /// <summary>
    /// Class FilterOption.
    /// </summary>
    public class FilterOption
    {
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text { get; set; }
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; set; }
    }

    public class GroupTypeSortOrder
    {
        public int GroupTypeId { get; set; }
        public int SortOrder { get; set; }
    }
    public class GroupSortInfo
    {
        public int GroupId { get; set; }
        public int SortOrder { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsNew { get; set; }
    }
    public class GroupOptionInfo
    {
        public int GroupId { get; set; }
        public int? WeeklyDayOfWeek { get; set; }
        public string CategoryOption { get; set; }
        public string SecondaryCategoryOption { get; set; }
    }

    /// <summary>
    /// Class GroupMemberInformation.
    /// </summary>
    public class GroupMemberInformation
    {
        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>The first name.</value>
        public string FirstName { get; set; }
        /// <summary>
        /// Gets or sets the name of the nick.
        /// </summary>
        /// <value>The name of the nick.</value>
        public string NickName { get; set; }
        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>The last name.</value>
        public string LastName { get; set; }
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>The email.</value>
        public string Email { get; set; }
        /// <summary>
        /// Gets or sets the role.
        /// </summary>
        /// <value>The role.</value>
        public string Role { get; set; }
        /// <summary>
        /// Gets or sets the role identifier.
        /// </summary>
        /// <value>The role identifier.</value>
        public int? RoleId { get; set; }
    }

    /// <summary>
    /// Class LocationInformation.
    /// </summary>
    public class LocationInformation
    {
        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>The location identifier.</value>
        public int LocationId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the street1.
        /// </summary>
        /// <value>The street1.</value>
        public string Street1 { get; set; }
        /// <summary>
        /// Gets or sets the street2.
        /// </summary>
        /// <value>The street2.</value>
        public string Street2 { get; set; }
        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>The city.</value>
        public string City { get; set; }
        /// <summary>
        /// Gets or sets the county.
        /// </summary>
        /// <value>The county.</value>
        public string County { get; set; }
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        public string State { get; set; }
        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>The country.</value>
        public string Country { get; set; }
        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        /// <value>The postal code.</value>
        public string PostalCode { get; set; }
        public MapCoordinate Point { get; set; }
    }

    public class MapCoordinate
    {
        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        public double? Longitude { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapCoordinate"/> class.
        /// </summary>
        public MapCoordinate()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapCoordinate"/> class.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        public MapCoordinate( double? latitude, double? longitude )
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }

    /// <summary>
    /// Class AttributeInformation.
    /// </summary>
    public class AttributeInformation
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>The key.</value>
        public string Key { get; set; }
        /// <summary>
        /// Gets or sets the type of the field.
        /// </summary>
        /// <value>The type of the field.</value>
        public string FieldType { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is public.
        /// </summary>
        /// <value><c>true</c> if this instance is public; otherwise, <c>false</c>.</value>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>The options.</value>
        public List<FilterOption> Options { get; set; }
    }
}
