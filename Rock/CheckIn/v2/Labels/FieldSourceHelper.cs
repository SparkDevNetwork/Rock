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
using System.Linq;

using Rock.Attribute;
using Rock.CheckIn.v2.Labels.Formatters;
using Rock.Enums.CheckIn.Labels;
using Rock.Enums.Reporting;
using Rock.Field;
using Rock.Field.Types;
using Rock.Model;
using Rock.Reporting;
using Rock.ViewModels.Reporting;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2.Labels
{
    /// <summary>
    /// Provides all the methods for accessing the field source data. This
    /// includes the data sources, the filter sources, and the
    /// <see cref="EntityField"/> objects used to support converting the rule
    /// bags between public and private format.
    /// </summary>
    internal static class FieldSourceHelper
    {
        /// <summary>
        /// The person property names that should be made common so they are
        /// easily accessible when picking a data source or filter.
        /// </summary>
        private static readonly HashSet<string> PersonPropertyNamesToMakeCommon = new HashSet<string>
        {
            nameof( Person.NickName ),
            nameof( Person.LastName ),
            nameof( Person.DaysUntilBirthday ),
            nameof( Person.Gender )
        };

        /// <summary>
        /// The person attributes that should be made common so they are easily
        /// accessible when picking a data source or filter.
        /// </summary>
        private static readonly HashSet<Guid> PersonAttributesToMakeCommon = new HashSet<Guid>
        {
            SystemGuid.Attribute.PERSON_LEGAL_NOTE.AsGuid(),
            SystemGuid.Attribute.PERSON_ALLERGY.AsGuid()
        };

        /// <summary>
        /// The person property names that should be excluded from data sources.
        /// These are excluded either because we don't want them or because we
        /// are going to provide a custom version of them.
        /// </summary>
        private static readonly HashSet<string> PersonPropertyNamesToExcludeFromDataSources = new HashSet<string>
        {
            nameof( Person.Gender ),
            nameof( Person.Age )
        };

        /// <summary>
        /// The person property names that should be excluded from filter sources.
        /// These are excluded either because we don't want them or because we
        /// are going to provide a custom version of them.
        /// </summary>
        private static readonly HashSet<string> PersonPropertyNamesToExcludeFromFilterSources = new HashSet<string>
        {
            nameof( Person.Age )
        };

        /// <summary>
        /// The person attributes that should be excluded from data sources.
        /// These are excluded either because we don't want them or because we
        /// are going to provide a custom version of them.
        /// </summary>
        private static readonly HashSet<Guid> PersonAttributesToExcludeFromDataSources = new HashSet<Guid>();

        /// <summary>
        /// Retrieves the data sources for the specified label type.
        /// </summary>
        /// <param name="labelType">The type of label for which to retrieve data sources.</param>
        /// <returns>A read-only list containing the data sources for the specified label type.</returns>
        public static IReadOnlyList<FieldDataSource> GetDataSources( LabelType labelType )
        {
            if ( labelType == LabelType.Family )
            {
                return GetFamilyLabelDataSources();
            }
            else if ( labelType == LabelType.Person )
            {
                return GetPersonLabelDataSources();
            }
            else if ( labelType == LabelType.Attendance )
            {
                return GetAttendanceLabelDataSources();
            }
            else if ( labelType == LabelType.Checkout )
            {
                return GetCheckoutLabelDataSources();
            }

            return new List<FieldDataSource>();
        }

        /// <summary>
        /// Retrieves the filter sources for the specified label type.
        /// </summary>
        /// <param name="labelType">The type of label for which to retrieve filter sources.</param>
        /// <returns>A read-only list containing the filter sources for the specified label type.</returns>
        public static IReadOnlyList<FieldFilterSourceBag> GetFilterSources( LabelType labelType )
        {
            if ( labelType == LabelType.Family )
            {
                return GetFamilyLabelFilterSources();
            }
            else if ( labelType == LabelType.Person )
            {
                return GetPersonLabelFilterSources();
            }
            else if ( labelType == LabelType.Attendance )
            {
                return GetAttendanceLabelFilterSources();
            }
            else if ( labelType == LabelType.Checkout )
            {
                return GetCheckoutLabelFilterSources();
            }

            return new List<FieldFilterSourceBag>();
        }

        /// <summary>
        /// Gets the data source dictionary for the label type. This will be
        /// cached for short periods of time to improve performance.
        /// </summary>
        /// <param name="labelType">The type of label for which to retrieve data sources.</param>
        /// <returns>A dictionary of data sources whose key is the data source key.</returns>
        public static IReadOnlyDictionary<string, FieldDataSource> GetCachedDataSources( LabelType labelType )
        {
            return RockCache.GetOrAddExisting( $"{typeof( FieldSourceHelper )}:DataSources:{labelType}", () =>
            {
                var dataSources = GetDataSources( labelType );
                var sourceDictionary = new Dictionary<string, FieldDataSource>( dataSources.Count );

                foreach ( var dataSource in dataSources )
                {
                    sourceDictionary.TryAdd( dataSource.Key, dataSource );
                }

                return sourceDictionary;
            } ) as Dictionary<string, FieldDataSource>;
        }

        #region Person Label

        /// <summary>
        /// Gets all data sources for a <see cref="LabelType.Person"/> label.
        /// </summary>
        /// <returns>A list of data sources.</returns>
        public static List<FieldDataSource> GetPersonLabelDataSources()
        {
            return GetPersonLabelAttendeeInfoDataSources()
                .Concat( GetPersonLabelCheckInInfoDataSources() )
                .Concat( GetPersonLabelAchievementInfoDataSources() )
                .DistinctBy( ds => ds.Key )
                .ToList();
        }

        /// <summary>
        /// Gets the attendee information data sources for a person label.
        /// </summary>
        /// <returns>A list of field data sources.</returns>
        private static List<FieldDataSource> GetPersonLabelAttendeeInfoDataSources()
        {
            return GetStandardPersonAttendeeInfoDataSources<PersonLabelData>();
        }

        /// <summary>
        /// Gets the check-in information data sources for a person label.
        /// </summary>
        /// <returns>A list of field data sources.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0028:Simplify collection initialization", Justification = "Because the list of options is so long it is more clear to use the Add() method." )]
        private static List<FieldDataSource> GetPersonLabelCheckInInfoDataSources()
        {
            var dataSources = new List<FieldDataSource>();

            dataSources.Add( new MultiValueFieldDataSource<PersonLabelData>
            {
                Key = "056a1d46-de60-413d-bf90-3e8d62128730",
                Name = "Area Name",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValuesFunc = ( source, field, printRequest ) => source.AreaNames
            } );

            dataSources.Add( new SingleValueFieldDataSource<PersonLabelData>
            {
                Key = "e654e589-f4f6-4946-b973-49743eb637f4",
                Name = "Check-in Time",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValueFunc = ( source, field, printRequest ) => source.CheckInTime
            } );

            dataSources.Add( new SingleValueFieldDataSource<PersonLabelData>
            {
                Key = "9d96337a-61b2-48c6-b3df-dec5b4ec8584",
                Name = "Current Time",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValueFunc = ( source, field, printRequest ) => source.CurrentTime
            } );

            dataSources.Add( new MultiValueFieldDataSource<PersonLabelData>
            {
                Key = "15e42323-9f41-4258-b49f-4f206df37fad",
                Name = "Group Name",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValuesFunc = ( source, field, printRequest ) => source.GroupNames
            } );

            dataSources.Add( new MultiValueFieldDataSource<PersonLabelData>
            {
                Key = "4389a134-cc1e-47e4-b435-fee0ad050cf0",
                Name = "Group Role Name",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValuesFunc = ( source, field, printRequest ) => source.GroupRoleNames
            } );

            dataSources.Add( new MultiValueFieldDataSource<PersonLabelData>
            {
                Key = "d390728a-2886-49c1-a580-c84995bb5fb2",
                Name = "Location Name",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValuesFunc = ( source, field, printRequest ) => source.LocationNames
            } );

            dataSources.Add( new MultiValueFieldDataSource<PersonLabelData>
            {
                Key = "331038d5-c959-4a37-9978-10c2a3a851b1",
                Name = "Schedule Name",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValuesFunc = ( source, field, printRequest ) => source.ScheduleNames
            } );

            dataSources.Add( new MultiValueFieldDataSource<PersonLabelData>
            {
                Key = "f8693f9e-0163-44ca-a5e8-396a4547cadf",
                Name = "Schedule Time",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValuesFunc = ( source, field, printRequest ) => source.PersonAttendance.Select( a => a.Schedule.GetNextCheckInStartTime( a.StartDateTime ) )
            } );

            dataSources.Add( new SingleValueFieldDataSource<PersonLabelData>
            {
                Key = "42961db8-eee8-401c-b8df-f13a20cac31b",
                Name = "Security Code",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValueFunc = ( source, field, printRequest ) => source.SecurityCode
            } );

            return dataSources;
        }

        /// <summary>
        /// Gets the achievement information data sources for a person label.
        /// </summary>
        /// <returns>A list of field data sources.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0028:Simplify collection initialization", Justification = "Because the list of options is so long it is more clear to use the Add() method." )]
        private static List<FieldDataSource> GetPersonLabelAchievementInfoDataSources()
        {
            var dataSources = new List<FieldDataSource>();

            dataSources.Add( new MultiValueFieldDataSource<PersonLabelData>
            {
                Key = "14ee31ed-aa52-4840-bc2e-5af39fd0d620",
                Name = "Just Completed Achievements",
                TextSubType = TextFieldSubType.AchievementInfo,
                Category = "Common",
                ValuesFunc = ( source, field, printRequest ) => source.JustCompletedAchievements
            } );

            dataSources.Add( new MultiValueFieldDataSource<PersonLabelData>
            {
                Key = "e4cb1548-0e69-4a7d-ab4f-32b70bd4d066",
                Name = "In Progress Achievements",
                TextSubType = TextFieldSubType.AchievementInfo,
                Category = "Common",
                ValuesFunc = ( source, field, printRequest ) => source.InProgressAchievements
            } );

            dataSources.Add( new MultiValueFieldDataSource<PersonLabelData>
            {
                Key = "4d1db5c1-7f6c-48db-8c5b-39aeabff64b1",
                Name = "Previously Completed Achievements",
                TextSubType = TextFieldSubType.AchievementInfo,
                Category = "Common",
                ValuesFunc = ( source, field, printRequest ) => source.PreviouslyCompletedAchievements
            } );

            return dataSources;
        }

        /// <summary>
        /// Gets the filter sources that will be used with <see cref="LabelType.Person"/>.
        /// </summary>
        /// <returns>A list of <see cref="FieldFilterSourceBag"/> objects that represent the filtering options.</returns>
        public static List<FieldFilterSourceBag> GetPersonLabelFilterSources()
        {
            var filterSources = GetPersonFilterSources( nameof( PersonLabelData.Person ) );

            // Add in the IsFirstTime filter.
            filterSources.Add( CreateBooleanPropertyFilter(
                propertyName: nameof( PersonLabelData.IsFirstTime ),
                category: "Common" ) );

            // Add in the Check-in Info filters.
            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( PersonLabelData.AreaNames ),
                category: "Check-in Info" ) );

            filterSources.Add( CreateDateTimePropertyFilter(
                propertyName: nameof( PersonLabelData.CheckInTime ),
                category: "Check-in Info" ) );

            filterSources.Add( CreateDateTimePropertyFilter(
                propertyName: nameof( PersonLabelData.CurrentTime ),
                category: "Check-in Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( PersonLabelData.GroupNames ),
                category: "Check-in Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( PersonLabelData.GroupRoleNames ),
                category: "Check-in Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( PersonLabelData.LocationNames ),
                category: "Check-in Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( PersonLabelData.ScheduleNames ),
                category: "Check-in Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( PersonLabelData.SecurityCode ),
                category: "Check-in Info" ) );

            // Add in the Achievement Info filters.
            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( PersonLabelData.InProgressAchievements ),
                category: "Achievement Info" ) );

            filterSources.Add( CreateIntegerPropertyFilter(
                propertyName: nameof( PersonLabelData.InProgressAchievementIds ),
                category: "Achievement Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( PersonLabelData.JustCompletedAchievements ),
                category: "Achievement Info" ) );

            filterSources.Add( CreateIntegerPropertyFilter(
                propertyName: nameof( PersonLabelData.JustCompletedAchievementIds ),
                category: "Achievement Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( PersonLabelData.PreviouslyCompletedAchievements ),
                category: "Achievement Info" ) );

            filterSources.Add( CreateIntegerPropertyFilter(
                propertyName: nameof( PersonLabelData.PreviouslyCompletedAchievementIds ),
                category: "Achievement Info" ) );

            return filterSources;
        }

        #endregion

        #region Attendance Label

        /// <summary>
        /// Gets all data sources for a <see cref="LabelType.Attendance"/> label.
        /// </summary>
        /// <returns>A list of data sources.</returns>
        public static List<FieldDataSource> GetAttendanceLabelDataSources()
        {
            return GetAttendanceLabelAttendeeInfoDataSources()
                .Concat( GetAttendanceLabelCheckInInfoDataSources() )
                .Concat( GetAttendanceLabelAchievementInfoDataSources() )
                .DistinctBy( ds => ds.Key )
                .ToList();
        }

        /// <summary>
        /// Gets the attendee information data sources for an attendance label.
        /// </summary>
        /// <returns>A list of field data sources.</returns>
        private static List<FieldDataSource> GetAttendanceLabelAttendeeInfoDataSources()
        {
            return GetStandardPersonAttendeeInfoDataSources<PersonLabelData>();
        }

        /// <summary>
        /// Gets the check-in information data sources for an attendance label.
        /// </summary>
        /// <returns>A list of field data sources.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0028:Simplify collection initialization", Justification = "Because the list of options is so long it is more clear to use the Add() method." )]
        private static List<FieldDataSource> GetAttendanceLabelCheckInInfoDataSources()
        {
            var dataSources = new List<FieldDataSource>();

            dataSources.Add( new SingleValueFieldDataSource<AttendanceLabelData>
            {
                Key = "6b58e4d4-1cd7-4908-abe6-ba0ff070f95c",
                Name = "Area Name",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValueFunc = ( source, field, printRequest ) => source.Attendance.Area.Name
            } );

            dataSources.Add( new SingleValueFieldDataSource<AttendanceLabelData>
            {
                Key = "f69b47f6-40f0-4cf3-93f8-9a7f49c400e7",
                Name = "Check-in Time",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValueFunc = ( source, field, printRequest ) => source.Attendance.StartDateTime
            } );

            dataSources.Add( new SingleValueFieldDataSource<AttendanceLabelData>
            {
                Key = "d3f07ec5-4444-4d20-adc3-4f979e8a29cb",
                Name = "Current Time",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValueFunc = ( source, field, printRequest ) => RockDateTime.Now
            } );

            dataSources.Add( new SingleValueFieldDataSource<AttendanceLabelData>
            {
                Key = "d7baf5ab-3b5a-4304-a29c-a3d3a8de4c6c",
                Name = "Group Name",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValueFunc = ( source, field, printRequest ) => source.Attendance.Group.Name
            } );

            dataSources.Add( new SingleValueFieldDataSource<AttendanceLabelData>
            {
                Key = "55c0d92b-792a-48b1-97d1-00048156043c",
                Name = "Group Role Name",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValueFunc = ( source, field, printRequest ) => source.Attendance
                    .GroupMembers
                    .Select( gm => GroupTypeCache.Get( gm.GroupTypeId, printRequest.RockContext )
                        ?.Roles
                        .FirstOrDefault( r => r.Id == gm.GroupRoleId )
                        ?.Name )
                    .Where( n => n != null )
                    .FirstOrDefault()
                    ?? string.Empty
            } );

            dataSources.Add( new SingleValueFieldDataSource<AttendanceLabelData>
            {
                Key = "602af35f-2bbd-4147-ae2c-1123478a30ee",
                Name = "Location Name",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValueFunc = ( source, field, printRequest ) => source.Attendance.Location.Name
            } );

            dataSources.Add( new SingleValueFieldDataSource<AttendanceLabelData>
            {
                Key = "b6fd6684-187e-4bc3-a85c-b25d1367c914",
                Name = "Schedule Name",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValueFunc = ( source, field, printRequest ) => source.Attendance.Schedule.Name
            } );

            dataSources.Add( new SingleValueFieldDataSource<AttendanceLabelData>
            {
                Key = "1e01aa11-7171-4124-bdaf-e316ca34390b",
                Name = "Schedule Time",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValueFunc = ( source, field, printRequest ) => source.Attendance.Schedule.GetNextCheckInStartTime( source.Attendance.StartDateTime )
            } );

            dataSources.Add( new SingleValueFieldDataSource<AttendanceLabelData>
            {
                Key = "5bfa4351-3f18-4ec8-be29-18e4aa44323d",
                Name = "Security Code",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValueFunc = ( source, field, printRequest ) => source.Attendance.SecurityCode
            } );

            return dataSources;
        }

        /// <summary>
        /// Gets the achievement information data sources for an attendance label.
        /// </summary>
        /// <returns>A list of field data sources.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0028:Simplify collection initialization", Justification = "Because the list of options is so long it is more clear to use the Add() method." )]
        private static List<FieldDataSource> GetAttendanceLabelAchievementInfoDataSources()
        {
            var dataSources = new List<FieldDataSource>();

            dataSources.Add( new MultiValueFieldDataSource<AttendanceLabelData>
            {
                Key = "fca51f2b-44ab-44e0-929a-3f5cd43368ea",
                Name = "Just Completed Achievements",
                TextSubType = TextFieldSubType.AchievementInfo,
                Category = "Common",
                ValuesFunc = ( source, field, printRequest ) => source.JustCompletedAchievements
            } );

            dataSources.Add( new MultiValueFieldDataSource<AttendanceLabelData>
            {
                Key = "92c258a0-d7ff-4f26-a670-07b80eb422b4",
                Name = "In Progress Achievements",
                TextSubType = TextFieldSubType.AchievementInfo,
                Category = "Common",
                ValuesFunc = ( source, field, printRequest ) => source.InProgressAchievements
            } );

            dataSources.Add( new MultiValueFieldDataSource<AttendanceLabelData>
            {
                Key = "c3281631-1949-480b-9732-5978678e7551",
                Name = "Previously Completed Achievements",
                TextSubType = TextFieldSubType.AchievementInfo,
                Category = "Common",
                ValuesFunc = ( source, field, printRequest ) => source.PreviouslyCompletedAchievements
            } );

            return dataSources;
        }

        /// <summary>
        /// Gets the filter sources that will be used with <see cref="LabelType.Attendance"/>.
        /// </summary>
        /// <returns>A list of <see cref="FieldFilterSourceBag"/> objects that represent the filtering options.</returns>
        public static List<FieldFilterSourceBag> GetAttendanceLabelFilterSources()
        {
            var filterSources = GetPersonFilterSources( nameof( AttendanceLabelData.Person ) );

            // Add in the IsFirstTime filter.
            filterSources.Add( CreateBooleanPropertyFilter(
                propertyName: nameof( AttendanceLabelData.Attendance.IsFirstTime ),
                path: nameof( AttendanceLabelData.Attendance ),
                category: "Common" ) );

            // Add in the Check-in Info filters.
            filterSources.Add( CreateStringPropertyFilter(
                title: "Area Name",
                propertyName: nameof( AttendanceLabelData.Attendance.Area.Name ),
                path: $"{nameof( AttendanceLabelData.Attendance )}.{nameof( AttendanceLabelData.Attendance.Area )}",
                category: "Check-in Info" ) );

            filterSources.Add( CreateDateTimePropertyFilter(
                propertyName: nameof( AttendanceLabelData.CheckInTime ),
                category: "Check-in Info" ) );

            filterSources.Add( CreateDateTimePropertyFilter(
                propertyName: nameof( AttendanceLabelData.CurrentTime ),
                category: "Check-in Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                title: "Group Name",
                propertyName: nameof( AttendanceLabelData.Attendance.Group.Name ),
                path: $"{nameof( AttendanceLabelData.Attendance )}.{nameof( AttendanceLabelData.Attendance.Group )}",
                category: "Check-in Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( AttendanceLabelData.GroupRoleNames ),
                category: "Check-in Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                title: "Location Name",
                propertyName: nameof( AttendanceLabelData.Attendance.Location.Name ),
                path: $"{nameof( AttendanceLabelData.Attendance )}.{nameof( AttendanceLabelData.Attendance.Location )}",
                category: "Check-in Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                title: "Schedule Name",
                propertyName: nameof( AttendanceLabelData.Attendance.Schedule.Name ),
                path: $"{nameof( AttendanceLabelData.Attendance )}.{nameof( AttendanceLabelData.Attendance.Schedule )}",
                category: "Check-in Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( AttendanceLabelData.Attendance.SecurityCode ),
                path: nameof( AttendanceLabelData.Attendance ),
                category: "Check-in Info" ) );

            // Add in the Achievement Info filters.
            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( AttendanceLabelData.InProgressAchievements ),
                category: "Achievement Info" ) );

            filterSources.Add( CreateIntegerPropertyFilter(
                propertyName: nameof( AttendanceLabelData.InProgressAchievementIds ),
                category: "Achievement Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( AttendanceLabelData.JustCompletedAchievements ),
                category: "Achievement Info" ) );

            filterSources.Add( CreateIntegerPropertyFilter(
                propertyName: nameof( AttendanceLabelData.JustCompletedAchievementIds ),
                category: "Achievement Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( AttendanceLabelData.PreviouslyCompletedAchievements ),
                category: "Achievement Info" ) );

            filterSources.Add( CreateIntegerPropertyFilter(
                propertyName: nameof( AttendanceLabelData.PreviouslyCompletedAchievementIds ),
                category: "Achievement Info" ) );

            return filterSources;
        }

        #endregion

        #region Family Label

        /// <summary>
        /// Gets all data sources for a <see cref="LabelType.Family"/> label.
        /// </summary>
        /// <returns>A list of data sources.</returns>
        public static List<FieldDataSource> GetFamilyLabelDataSources()
        {
            return GetFamilyLabelAttendeeInfoDataSources()
                .Concat( GetFamilyLabelCheckInInfoDataSources() )
                .Concat( GetFamilyLabelAchievementInfoDataSources() )
                .DistinctBy( ds => ds.Key )
                .ToList();
        }

        /// <summary>
        /// Gets the attendee information data sources for a family label.
        /// </summary>
        /// <returns>A list of field data sources.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0028:Simplify collection initialization", Justification = "Because the list of options is so long it is more clear to use the Add() method." )]
        private static List<FieldDataSource> GetFamilyLabelAttendeeInfoDataSources()
        {
            var dataSources = new List<FieldDataSource>();

            dataSources.Add( new MultiValueFieldDataSource<FamilyLabelData>
            {
                Key = "4f87ee6a-5ccf-46ea-b3b3-20d079f6f7eb",
                Name = "Nick Name",
                TextSubType = TextFieldSubType.AttendeeInfo,
                Category = "Common",
                ValuesFunc = ( source, field, printRequest ) => source.AllAttendance
                    .Select( a => a.Person )
                    .DistinctBy( a => a.Id )
                    .Select( a => a.NickName )
            } );

            dataSources.Add( new MultiValueFieldDataSource<FamilyLabelData>
            {
                Key = "4e7ef55c-5a25-4332-9f49-56e9443c8aac",
                Name = "Full Name",
                TextSubType = TextFieldSubType.AttendeeInfo,
                Category = "Common",
                Formatter = FullNameDataFormatter.Instance,
                ValuesFunc = ( source, field, printRequest ) => source.AllAttendance
                    .Select( a => a.Person )
            } );

            return dataSources;
        }

        /// <summary>
        /// Gets the check-in information data sources for a family label.
        /// </summary>
        /// <returns>A list of field data sources.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0028:Simplify collection initialization", Justification = "Because the list of options is so long it is more clear to use the Add() method." )]
        private static List<FieldDataSource> GetFamilyLabelCheckInInfoDataSources()
        {
            var dataSources = new List<FieldDataSource>();

            dataSources.Add( new SingleValueFieldDataSource<FamilyLabelData>
            {
                Key = "7bae9b91-3653-4796-9698-c01b2d3b5049",
                Name = "Check-in Time",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                Formatter = DateDataFormatter.Instance,
                ValueFunc = ( source, field, printRequest ) => source.AllAttendance
                    .Min( a => a.StartDateTime )
            } );

            dataSources.Add( new SingleValueFieldDataSource<FamilyLabelData>
            {
                Key = "2309804e-e2dc-43b8-b5f4-ce78ced088b3",
                Name = "Current Time",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                Formatter = DateDataFormatter.Instance,
                ValueFunc = ( source, field, printRequest ) => RockDateTime.Now
            } );

            dataSources.Add( new MultiValueFieldDataSource<FamilyLabelData>
            {
                Key = "c62d2d38-4c0e-490d-81d9-4b2ac88d25ad",
                Name = "Security Codes",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                Formatter = SecurityCodeAndNameDataFormatter.Instance,
                ValuesFunc = ( source, field, printRequest ) => source.AllAttendance
            } );

            dataSources.Add( new MultiValueFieldDataSource<FamilyLabelData>
            {
                Key = "2f63ab4e-38ec-4704-be9c-426825bc3bf5",
                Name = "Check-in Details",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                Formatter = CheckInDetailDataFormatter.Instance,
                ValuesFunc = ( source, field, printRequest ) => source.AllAttendance
            } );

            return dataSources;
        }

        /// <summary>
        /// Gets the achievement information data sources for a family label.
        /// </summary>
        /// <returns>A list of field data sources.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0028:Simplify collection initialization", Justification = "Because the list of options is so long it is more clear to use the Add() method." )]
        private static List<FieldDataSource> GetFamilyLabelAchievementInfoDataSources()
        {
            var dataSources = new List<FieldDataSource>();

            dataSources.Add( new MultiValueFieldDataSource<FamilyLabelData>
            {
                Key = "fca51f2b-44ab-44e0-929a-3f5cd43368ea",
                Name = "Just Completed Achievements",
                TextSubType = TextFieldSubType.AchievementInfo,
                Category = "Common",
                ValuesFunc = ( source, field, printRequest ) => source.JustCompletedAchievements
            } );

            return dataSources;
        }

        /// <summary>
        /// Gets the filter sources that will be used with <see cref="LabelType.Family"/>.
        /// </summary>
        /// <returns>A list of <see cref="FieldFilterSourceBag"/> objects that represent the filtering options.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0028:Simplify collection initialization", Justification = "Because the list of options is so long it is more clear to use the Add() method." )]
        public static List<FieldFilterSourceBag> GetFamilyLabelFilterSources()
        {
            var filterSources = new List<FieldFilterSourceBag>();

            // Add in the Attendee Info filters.
            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( FamilyLabelData.NickNames ),
                category: "Attendee Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( FamilyLabelData.FirstNames ),
                category: "Attendee Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( FamilyLabelData.LastNames ),
                category: "Attendee Info" ) );

            // Add in the Check-in Info filters.
            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( FamilyLabelData.AreaNames ),
                category: "Check-in Info" ) );

            filterSources.Add( CreateDateTimePropertyFilter(
                propertyName: nameof( FamilyLabelData.CheckInTime ),
                category: "Check-in Info" ) );

            filterSources.Add( CreateDateTimePropertyFilter(
                propertyName: nameof( FamilyLabelData.CurrentTime ),
                category: "Check-in Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( FamilyLabelData.GroupNames ),
                category: "Check-in Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( FamilyLabelData.LocationNames ),
                category: "Check-in Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( FamilyLabelData.ScheduleNames ),
                category: "Check-in Info" ) );

            // Add in the Achievement Info filters.
            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( FamilyLabelData.JustCompletedAchievements ),
                category: "Achievement Info" ) );

            filterSources.Add( CreateIntegerPropertyFilter(
                propertyName: nameof( FamilyLabelData.JustCompletedAchievementIds ),
                category: "Achievement Info" ) );

            return filterSources;
        }

        #endregion

        #region Checkout Label

        /// <summary>
        /// Gets all data sources for a <see cref="LabelType.Checkout"/> label.
        /// </summary>
        /// <returns>A list of data sources.</returns>
        public static List<FieldDataSource> GetCheckoutLabelDataSources()
        {
            // Use the same information as attendance label except for achievement
            // information, since we don't have that data now.
            return GetAttendanceLabelAttendeeInfoDataSources()
                .Concat( GetAttendanceLabelCheckInInfoDataSources() )
                .DistinctBy( ds => ds.Key )
                .ToList();
        }

        /// <summary>
        /// Gets the filter sources that will be used with <see cref="LabelType.Checkout"/>.
        /// </summary>
        /// <returns>A list of <see cref="FieldFilterSourceBag"/> objects that represent the filtering options.</returns>
        public static List<FieldFilterSourceBag> GetCheckoutLabelFilterSources()
        {
            var filterSources = GetPersonFilterSources( nameof( CheckoutLabelData.Person ) );

            // Add in the IsFirstTime filter.
            filterSources.Add( CreateBooleanPropertyFilter(
                propertyName: nameof( CheckoutLabelData.Attendance.IsFirstTime ),
                path: nameof( CheckoutLabelData.Attendance ),
                category: "Common" ) );

            // Add in the Check-in Info filters.
            filterSources.Add( CreateStringPropertyFilter(
                title: "Area Name",
                propertyName: nameof( CheckoutLabelData.Attendance.Area.Name ),
                path: $"{nameof( CheckoutLabelData.Attendance )}.{nameof( CheckoutLabelData.Attendance.Area )}",
                category: "Check-in Info" ) );

            filterSources.Add( CreateDateTimePropertyFilter(
                propertyName: nameof( CheckoutLabelData.CheckInTime ),
                category: "Check-in Info" ) );

            filterSources.Add( CreateDateTimePropertyFilter(
                propertyName: nameof( CheckoutLabelData.CurrentTime ),
                category: "Check-in Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                title: "Group Name",
                propertyName: nameof( CheckoutLabelData.Attendance.Group.Name ),
                path: $"{nameof( CheckoutLabelData.Attendance )}.{nameof( CheckoutLabelData.Attendance.Group )}",
                category: "Check-in Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( AttendanceLabelData.GroupRoleNames ),
                category: "Check-in Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                title: "Location Name",
                propertyName: nameof( CheckoutLabelData.Attendance.Location.Name ),
                path: $"{nameof( CheckoutLabelData.Attendance )}.{nameof( CheckoutLabelData.Attendance.Location )}",
                category: "Check-in Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                title: "Schedule Name",
                propertyName: nameof( CheckoutLabelData.Attendance.Schedule.Name ),
                path: $"{nameof( CheckoutLabelData.Attendance )}.{nameof( CheckoutLabelData.Attendance.Schedule )}",
                category: "Check-in Info" ) );

            filterSources.Add( CreateStringPropertyFilter(
                propertyName: nameof( CheckoutLabelData.Attendance.SecurityCode ),
                path: nameof( CheckoutLabelData.Attendance ),
                category: "Check-in Info" ) );

            return filterSources;
        }

        #endregion

        /// <summary>
        /// Gets the attendee information data sources for a label that has a
        /// single Person property.
        /// </summary>
        /// <returns>A list of field data sources.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0028:Simplify collection initialization", Justification = "Because the list of options is so long it is more clear to use the Add() method." )]
        private static List<FieldDataSource> GetStandardPersonAttendeeInfoDataSources<TLabelData>()
            where TLabelData : ILabelDataHasPerson
        {
            var dataSources = new List<FieldDataSource>();

            dataSources.Add( new SingleValueFieldDataSource<TLabelData>
            {
                Key = "ea92317c-65b9-4d8e-b4c4-7fc7ab9ad932",
                Name = "Full Name",
                TextSubType = TextFieldSubType.AttendeeInfo,
                Category = "Common",
                Formatter = FullNameDataFormatter.Instance,
                ValueFunc = ( source, field, printRequest ) => source.Person
            } );

            dataSources.Add( new SingleValueFieldDataSource<TLabelData>
            {
                Key = "1c1f5c30-5b23-484f-9993-f60d07952b33",
                Name = "Gender",
                TextSubType = TextFieldSubType.AttendeeInfo,
                Category = "Common",
                Formatter = GenderDataFormatter.Instance,
                ValueFunc = ( source, field, printRequest ) => source.Person.Gender
            } );

            dataSources.Add( new SingleValueFieldDataSource<TLabelData>
            {
                Key = "c37608a7-9a93-4eb7-b045-208117575533",
                Name = "Grade Offset",
                TextSubType = TextFieldSubType.AttendeeInfo,
                Category = "Common",
                ValueFunc = ( source, field, printRequest ) => source.Person.GradeOffset
            } );

            dataSources.Add( new SingleValueFieldDataSource<TLabelData>
            {
                Key = "ae113ac5-a0b3-4225-be33-d82bb139077e",
                Name = "Grade Formatted",
                TextSubType = TextFieldSubType.AttendeeInfo,
                Category = "Common",
                Formatter = GradeDataFormatter.Instance,
                ValueFunc = ( source, field, printRequest ) => source.Person.GraduationYear
            } );

            dataSources.Add( new SingleValueFieldDataSource<TLabelData>
            {
                Key = "d07f698e-9c3b-4330-a82e-be45a64b813d",
                Name = "Age",
                TextSubType = TextFieldSubType.AttendeeInfo,
                Category = "Common",
                Formatter = PersonAgeDataFormatter.Instance,
                ValueFunc = ( source, field, printRequest ) => source.Person.AgePrecise
            } );

            dataSources.Add( new SingleValueFieldDataSource<TLabelData>
            {
                Key = "10a7d224-d0e7-4620-b52b-cf34e7b5e4ca",
                Name = "Birthday Day Of Week",
                TextSubType = TextFieldSubType.AttendeeInfo,
                Category = "Common",
                Formatter = WeekdayDateDataFormatter.Instance,
                ValueFunc = ( source, field, printRequest ) => source.Person.ThisYearsBirthdate
            } );

            var personDataSources = GetPersonDataSources<TLabelData>();

            return dataSources
                .Concat( personDataSources.Where( ds => !PersonPropertyNamesToExcludeFromDataSources.Contains( ds.Key ) ) )
                .ToList();
        }

        /// <summary>
        /// Gets all the data source objects for a Person object on the label
        /// data.
        /// </summary>
        /// <typeparam name="TLabelData">The type of label data expected.</typeparam>
        /// <returns>A list of field data sources.</returns>
        private static List<FieldDataSource> GetPersonDataSources<TLabelData>()
            where TLabelData : ILabelDataHasPerson
        {
            var dataSources = new List<FieldDataSource>();
            var entityFields = EntityHelper.GetEntityFields( typeof( Person ), true, false );

            foreach ( var entityField in entityFields )
            {
                FieldDataSource dataSource = null;

                if ( entityField.FieldKind == FieldKind.Property )
                {
                    if ( PersonPropertyNamesToExcludeFromDataSources.Contains( entityField.PropertyInfo.Name ) )
                    {
                        continue;
                    }

                    dataSource = GetPropertyDataSource<TLabelData>( entityField, "person", data => data.Person );

                    if ( PersonPropertyNamesToMakeCommon.Contains( entityField.PropertyInfo.Name ) )
                    {
                        dataSource.Category = "Common";
                    }
                }
                else
                {
                    if ( PersonAttributesToExcludeFromDataSources.Contains( entityField.AttributeGuid.Value ) )
                    {
                        continue;
                    }

                    var attributeCache = AttributeCache.Get( entityField.AttributeGuid.Value );

                    if ( entityField.FieldType.Field is DateFieldType || entityField.FieldType.Field is DateTimeFieldType )
                    {
                        dataSource = new DateAttributeFieldDataSource<TLabelData>( attributeCache, "person", data => data.Person );
                    }
                    else
                    {
                        dataSource = new SingleValueFieldDataSource<TLabelData>
                        {
                            Key = $"attribute:person:{entityField.AttributeGuid}",
                            Name = entityField.Title,
                            Category = "Attributes",
                            ValueFunc = ( source, field, printRequest ) => source.Person.GetAttributeValue( entityField.AttributeGuid.Value )
                        };
                    }

                    if ( PersonAttributesToMakeCommon.Contains( entityField.AttributeGuid.Value ) )
                    {
                        dataSource.Category = "Common";
                    }
                }

                dataSource.TextSubType = TextFieldSubType.AttendeeInfo;

                dataSources.Add( dataSource );
            }

            return dataSources;
        }

        /// <summary>
        /// Retrieves a list of all the filter sources for a person object.
        /// </summary>
        /// <param name="filterPath">The path to the person object.</param>
        /// <returns>A list of <see cref="FieldFilterSourceBag"/> objects that represent all the filtering objects.</returns>
        public static List<FieldFilterSourceBag> GetPersonFilterSources( string filterPath )
        {
            var filterSources = new List<FieldFilterSourceBag>();

            foreach ( var entityField in EntityHelper.GetEntityFields( typeof( Person ) ) )
            {
                FieldFilterSourceBag filterSource;

                if ( entityField.FieldKind == FieldKind.Property )
                {
                    if ( PersonPropertyNamesToExcludeFromFilterSources.Contains( entityField.PropertyInfo.Name ) )
                    {
                        continue;
                    }

                    var privateConfigValues = entityField.FieldConfig.ToDictionary( c => c.Key, c => c.Value.Value );
                    var configValues = entityField.FieldType.Field.GetPublicConfigurationValues( privateConfigValues, ConfigurationValueUsage.Edit, string.Empty );

                    filterSource = new FieldFilterSourceBag
                    {
                        Guid = Guid.NewGuid(),
                        Type = FieldFilterSourceType.Property,
                        Path = filterPath,
                        Category = "Person Properties",
                        Property = new FieldFilterPublicPropertyBag
                        {
                            Title = entityField.Title,
                            Name = entityField.Name,
                            FieldTypeGuid = entityField.FieldType.ControlFieldTypeGuid,
                            ConfigurationValues = configValues
                        }
                    };

                    if ( PersonPropertyNamesToMakeCommon.Contains( entityField.PropertyInfo.Name ) )
                    {
                        filterSource.Category = "Common";
                    }
                }
                else
                {
                    filterSource = new FieldFilterSourceBag
                    {
                        Guid = Guid.NewGuid(),
                        Type = FieldFilterSourceType.Attribute,
                        Path = filterPath,
                        Category = "Person Attributes",
                        Attribute = PublicAttributeHelper.GetPublicAttributeForEdit( AttributeCache.Get( entityField.AttributeGuid.Value ) )
                    };

                    if ( PersonAttributesToMakeCommon.Contains( entityField.AttributeGuid.Value ) )
                    {
                        filterSource.Category = "Common";
                    }
                }

                filterSources.Add( filterSource );
            }

            // Add in the Age filter, which will actually use AgePrecise.
            filterSources.Add( CreateDecimalPropertyFilter(
                propertyName: nameof( Person.AgePrecise ),
                title: "Age",
                path: filterPath,
                category: "Common" ) );

            // Add in the GradeOffset filter.
            filterSources.Add( CreateIntegerPropertyFilter(
                propertyName: nameof( Person.GradeOffset ),
                path: filterPath,
                category: "Common" ) );

            return filterSources;
        }

        // TODO: Probably change this to a class.
        private static FieldDataSource GetPropertyDataSource<TLabelData>( EntityField entityField, string propertyPath, Func<TLabelData, object> propertySelector )
        {
            var dataSource = new SingleValueFieldDataSource<TLabelData>
            {
                Key = $"{propertyPath}.{entityField.Name.ToLower()}",
                Name = entityField.Title,
                Category = "Properties",
                ValueFunc = ( source, field, printRequest ) => propertySelector( source ).GetPropertyValue( entityField.Name )
            };

            // Special handling for defined values, we want to display the
            // text value rather than the integer value.
            if ( entityField.FieldType.Guid == SystemGuid.FieldType.DEFINED_VALUE.AsGuid() )
            {
                dataSource.ValueFunc = ( source, field, printRequest ) =>
                {
                    var definedValueId = ( int? ) propertySelector( source ).GetPropertyValue( entityField.Name );

                    if ( !definedValueId.HasValue )
                    {
                        return string.Empty;
                    }

                    return DefinedValueCache.Get( definedValueId.Value )?.Value ?? string.Empty;
                };
            }

            // Set any custom formatters based on property type.
            if ( entityField.PropertyType == typeof( DateTime ) || entityField.PropertyType == typeof( DateTime? ) )
            {
                dataSource.Formatter = DateDataFormatter.Instance;
            }

            return dataSource;
        }

        /// <summary>
        /// Creates a boolean property filter based on the provided parameters.
        /// </summary>
        /// <param name="propertyName">The name of the property to use in the filter.</param>
        /// <param name="path">The path to the property (optional).</param>
        /// <param name="category">The category of the property (optional).</param>
        /// <param name="title">The title of the property (optional).</param>
        /// <returns>A new instance of <see cref="FieldFilterSourceBag"/> that represents the filtering options.</returns>
        private static FieldFilterSourceBag CreateBooleanPropertyFilter( string propertyName, string path = null, string category = null, string title = null )
        {
            return new FieldFilterSourceBag
            {
                Guid = Guid.NewGuid(),
                Type = FieldFilterSourceType.Property,
                Path = path,
                Category = category,
                Property = new FieldFilterPublicPropertyBag
                {
                    Title = title ?? propertyName.SplitCase(),
                    Name = propertyName,
                    FieldTypeGuid = SystemGuid.FieldType.BOOLEAN.AsGuid(),
                    ConfigurationValues = new Dictionary<string, string>()
                }
            };
        }

        /// <summary>
        /// Creates a filter for an integer property using the specified parameters.
        /// </summary>
        /// <param name="propertyName">The name of the property to filter.</param>
        /// <param name="path">The path to the property.</param>
        /// <param name="category">The category of the property.</param>
        /// <param name="title">The title of the property filter.</param>
        /// <returns>A new instance of <see cref="FieldFilterSourceBag"/> that represents the filtering options.</returns>
        private static FieldFilterSourceBag CreateIntegerPropertyFilter( string propertyName, string path = null, string category = null, string title = null )
        {
            return new FieldFilterSourceBag
            {
                Guid = Guid.NewGuid(),
                Type = FieldFilterSourceType.Property,
                Path = path,
                Category = category,
                Property = new FieldFilterPublicPropertyBag
                {
                    Title = title ?? propertyName.SplitCase(),
                    Name = propertyName,
                    FieldTypeGuid = SystemGuid.FieldType.INTEGER.AsGuid(),
                    ConfigurationValues = new Dictionary<string, string>()
                }
            };
        }

        /// <summary>
        /// Creates a filter for a decimal property using the specified parameters.
        /// </summary>
        /// <param name="propertyName">The name of the property to filter.</param>
        /// <param name="path">The path of the property (optional).</param>
        /// <param name="category">The category of the property (optional).</param>
        /// <param name="title">The title of the property (optional).</param>
        /// <returns>A new instance of <see cref="FieldFilterSourceBag"/> that represents the filtering options.</returns>
        private static FieldFilterSourceBag CreateDecimalPropertyFilter( string propertyName, string path = null, string category = null, string title = null )
        {
            return new FieldFilterSourceBag
            {
                Guid = Guid.NewGuid(),
                Type = FieldFilterSourceType.Property,
                Path = path,
                Category = category,
                Property = new FieldFilterPublicPropertyBag
                {
                    Title = title ?? propertyName.SplitCase(),
                    Name = propertyName,
                    FieldTypeGuid = SystemGuid.FieldType.DECIMAL.AsGuid(),
                    ConfigurationValues = new Dictionary<string, string>()
                }
            };
        }

        /// <summary>
        /// Creates a filter for the string property using the specified parameters.
        /// </summary>
        /// <param name="propertyName">The name of the property to filter.</param>
        /// <param name="path">The path to the property (optional).</param>
        /// <param name="category">The category of the property (optional).</param>
        /// <param name="title">The title of the property (optional).</param>
        /// <returns>A new instance of <see cref="FieldFilterSourceBag"/> that represents the filtering options.</returns>
        private static FieldFilterSourceBag CreateStringPropertyFilter( string propertyName, string path = null, string category = null, string title = null )
        {
            return new FieldFilterSourceBag
            {
                Guid = Guid.NewGuid(),
                Type = FieldFilterSourceType.Property,
                Path = path,
                Category = category,
                Property = new FieldFilterPublicPropertyBag
                {
                    Title = title ?? propertyName.SplitCase(),
                    Name = propertyName,
                    FieldTypeGuid = SystemGuid.FieldType.TEXT.AsGuid(),
                    ConfigurationValues = new Dictionary<string, string>()
                }
            };
        }

        /// <summary>
        /// Creates a filter for the DateTime property using the specified parameters.
        /// </summary>
        /// <param name="propertyName">The name of the property to filter.</param>
        /// <param name="path">The path to the property (optional).</param>
        /// <param name="category">The category of the property (optional).</param>
        /// <param name="title">The title of the property (optional).</param>
        /// <returns>A new instance of <see cref="FieldFilterSourceBag"/> that represents the filtering options.</returns>
        private static FieldFilterSourceBag CreateDateTimePropertyFilter( string propertyName, string path = null, string category = null, string title = null )
        {
            return new FieldFilterSourceBag
            {
                Guid = Guid.NewGuid(),
                Type = FieldFilterSourceType.Property,
                Path = path,
                Category = category,
                Property = new FieldFilterPublicPropertyBag
                {
                    Title = title ?? propertyName.SplitCase(),
                    Name = propertyName,
                    FieldTypeGuid = SystemGuid.FieldType.DATE_TIME.AsGuid(),
                    ConfigurationValues = new Dictionary<string, string>()
                }
            };
        }

        /// <summary>
        /// Gets the entity field that will handle filtering value conversion
        /// for the rule when it is being processed for the given label type.
        /// </summary>
        /// <param name="labelType">The type of label that identifies the type of data the rule will process.</param>
        /// <param name="rule">The rule that the <see cref="EntityField"/> is being requested for.</param>
        /// <returns>An instance of <see cref="EntityField"/> or <c>null</c> if the rule is not valid.</returns>
        public static EntityField GetEntityFieldForRule( LabelType labelType, FieldFilterRuleBag rule )
        {
            if ( rule.SourceType == FieldFilterSourceType.Attribute && rule.AttributeGuid.HasValue )
            {
                var attribute = AttributeCache.Get( rule.AttributeGuid.Value );

                if ( attribute == null )
                {
                    return null;
                }

                return EntityHelper.GetEntityFieldForAttribute( attribute );
            }
            else if ( rule.SourceType == FieldFilterSourceType.Property && rule.PropertyName.IsNotNullOrWhiteSpace() )
            {
                Type type = null;

                if ( labelType == LabelType.Family )
                {
                    type = typeof( FamilyLabelData );
                }
                else if ( labelType == LabelType.Person )
                {
                    type = typeof( PersonLabelData );
                }
                else if ( labelType == LabelType.Attendance )
                {
                    type = typeof( AttendanceLabelData );
                }
                else if ( labelType == LabelType.Checkout )
                {
                    type = typeof( CheckoutLabelData );
                }

                // This could also check a whitelist of allowed paths to make sure
                // they are not building filters to things they shouldn't, but since
                // the UI to create these is administrative only we can skip that
                // additional check for now.
                if ( rule.Path.IsNotNullOrWhiteSpace() )
                {
                    var pathComponents = rule.Path.Split( '.' );

                    for ( int i = 0; i < pathComponents.Length && type != null; i++ )
                    {
                        type = type.GetProperty( pathComponents[i] )?.PropertyType;
                    }
                }

                var property = type?.GetProperty( rule.PropertyName );

                if ( property == null )
                {
                    return null;
                }

                var entityField = EntityHelper.GetEntityFieldForProperty( property );

                if ( entityField.FieldType == null )
                {
                    if ( typeof( ICollection<string> ).IsAssignableFrom( property.PropertyType ) )
                    {
                        entityField.FieldType = FieldTypeCache.Get( SystemGuid.FieldType.TEXT );
                    }
                    else if ( property.PropertyType == typeof( double ) || property.PropertyType == typeof( double? ) )
                    {
                        entityField.FieldType = FieldTypeCache.Get( SystemGuid.FieldType.DECIMAL );
                    }
                }

                return entityField;
            }

            return null;
        }
    }
}
