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
using Rock.Field;
using Rock.Field.Types;
using Rock.Model;
using Rock.Reporting;
using Rock.ViewModels.Reporting;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2.Labels
{
    internal static class FieldSourceHelper
    {
        public static IReadOnlyDictionary<string, FieldDataSource> GetDataSources( LabelType labelType )
        {
            var dataSources = new List<FieldDataSource>();

            if ( labelType == LabelType.Person )
            {
                dataSources = GetPersonLabelDataSources();
            }

            return dataSources.ToDictionary( d => d.Key, d => d );
        }

        public static IReadOnlyList<FieldFilterSourceBag> GetFilterSources( LabelType labelType )
        {
            if ( labelType == LabelType.Person )
            {
                return GetPersonLabelFilterSources();
            }

            return new List<FieldFilterSourceBag>();
        }

        #region Person Label

        /// <summary>
        /// Gets all data sources for a <see cref="LabelType.Person"/> label.
        /// </summary>
        /// <returns>A list of data sources.</returns>
        public static List<FieldDataSource> GetPersonLabelDataSources()
        {
            return GetPersonLabelAttendeeInfoSources()
                .Concat( GetPersonLabelCheckInInfoSources() )
                .Concat( GetPersonLabelAchievementInfoSources() )
                .DistinctBy( ds => ds.Key )
                .ToList();
        }

        /// <summary>
        /// Gets the attendee information data sources for a person label.
        /// </summary>
        /// <returns>A list of field data sources.</returns>
        private static List<FieldDataSource> GetPersonLabelAttendeeInfoSources()
        {
            return GetStandardPersonAttendeeInfoSources<PersonLabelData>();
        }

        /// <summary>
        /// Gets the check-in information data sources for a person label.
        /// </summary>
        /// <returns>A list of field data sources.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0028:Simplify collection initialization", Justification = "Because the list of options is so long it is more clear to use the Add() method." )]
        private static List<FieldDataSource> GetPersonLabelCheckInInfoSources()
        {
            var dataSources = new List<FieldDataSource>();

            dataSources.Add( new MultiValueFieldDataSource<PersonLabelData>
            {
                Key = "056a1d46-de60-413d-bf90-3e8d62128730",
                Name = "Area Name",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValuesFunc = ( source, field, printRequest ) => source.PersonAttendance.Select( a => a.Area.Name )
            } );

            dataSources.Add( new SingleValueFieldDataSource<PersonLabelData>
            {
                Key = "e654e589-f4f6-4946-b973-49743eb637f4",
                Name = "Check-in Time",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValueFunc = ( source, field, printRequest ) => source.PersonAttendance.Min( a => a.StartDateTime )
            } );

            dataSources.Add( new SingleValueFieldDataSource<PersonLabelData>
            {
                Key = "9d96337a-61b2-48c6-b3df-dec5b4ec8584",
                Name = "Current Time",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValueFunc = ( source, field, printRequest ) => RockDateTime.Now
            } );

            dataSources.Add( new MultiValueFieldDataSource<PersonLabelData>
            {
                Key = "15e42323-9f41-4258-b49f-4f206df37fad",
                Name = "Group Name",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValuesFunc = ( source, field, printRequest ) => source.PersonAttendance.Select( a => a.Group.Name )
            } );

            dataSources.Add( new MultiValueFieldDataSource<PersonLabelData>
            {
                Key = "4389a134-cc1e-47e4-b435-fee0ad050cf0",
                Name = "Group Role Name",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValuesFunc = ( source, field, printRequest ) => source.PersonAttendance
                    .SelectMany( a => a.GroupMembers )
                    .Select( gm => GroupTypeCache.Get( gm.GroupTypeId ).Roles.FirstOrDefault( r => r.Id == gm.GroupRoleId )?.Name )
                    .Where( n => n != null )
                    .FirstOrDefault()
                    ?? string.Empty
            } );

            dataSources.Add( new MultiValueFieldDataSource<PersonLabelData>
            {
                Key = "d390728a-2886-49c1-a580-c84995bb5fb2",
                Name = "Location Name",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValuesFunc = ( source, field, printRequest ) => source.PersonAttendance.Select( a => a.Location.Name )
            } );

            dataSources.Add( new MultiValueFieldDataSource<PersonLabelData>
            {
                Key = "331038d5-c959-4a37-9978-10c2a3a851b1",
                Name = "Schedule Name",
                TextSubType = TextFieldSubType.CheckInInfo,
                Category = "Common",
                ValuesFunc = ( source, field, printRequest ) => source.PersonAttendance.Select( a => a.Schedule.Name )
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
                ValueFunc = ( source, field, printRequest ) => source.PersonAttendance.Select( a => a.SecurityCode ).FirstOrDefault() ?? string.Empty
            } );

            return dataSources;
        }

        /// <summary>
        /// Gets the achievement information data sources for a person label.
        /// </summary>
        /// <returns>A list of field data sources.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0028:Simplify collection initialization", Justification = "Because the list of options is so long it is more clear to use the Add() method." )]
        private static List<FieldDataSource> GetPersonLabelAchievementInfoSources()
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

        private static List<FieldFilterSourceBag> GetPersonLabelFilterSources()
        {
            var filterSources = new List<FieldFilterSourceBag>();

            foreach ( var entityField in EntityHelper.GetEntityFields( typeof( Person ) ) )
            {
                if ( entityField.FieldKind == FieldKind.Attribute )
                {
                    var filterSource = new FieldFilterSourceBag
                    {
                        Guid = Guid.NewGuid(),
                        Type = Enums.Reporting.FieldFilterSourceType.Attribute,
                        Category = "Attributes",
                        Attribute = PublicAttributeHelper.GetPublicAttributeForEdit( AttributeCache.Get( entityField.AttributeGuid.Value ) )
                    };

                    filterSources.Add( filterSource );
                }
                else
                {
                    var privateConfigValues = entityField.FieldConfig.ToDictionary( c => c.Key, c => c.Value.Value );
                    var configValues = entityField.FieldType.Field.GetPublicConfigurationValues( privateConfigValues, ConfigurationValueUsage.Edit, string.Empty );

                    var filterSource = new FieldFilterSourceBag
                    {
                        Guid = Guid.NewGuid(),
                        Type = Enums.Reporting.FieldFilterSourceType.Property,
                        Category = entityField.IsPreviewable ? "Common" : "Person Properties",
                        Property = new FieldFilterPropertyBag
                        {
                            Title = entityField.Title,
                            Name = entityField.Name,
                            FieldTypeGuid = entityField.FieldType.ControlFieldTypeGuid,
                            ConfigurationValues = configValues
                        }
                    };

                    filterSources.Add( filterSource );
                }
            }

            return filterSources;
        }

        #endregion

        /// <summary>
        /// Gets the attendee information data sources for a label that has a
        /// single Person property.
        /// </summary>
        /// <returns>A list of field data sources.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0028:Simplify collection initialization", Justification = "Because the list of options is so long it is more clear to use the Add() method." )]
        private static List<FieldDataSource> GetStandardPersonAttendeeInfoSources<TLabelData>()
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
            var excludedKeys = new string[]
            {
                "person.gender",
                "person.age"
            };

            return dataSources
                .Concat( personDataSources.Where( ds => !excludedKeys.Contains( ds.Key ) ) )
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
            var keysToMakeCommon = new List<string>
            {
                "person.nickname",
                "person.lastname",
                "person.daysuntilbirthday",
                $"attribute:person:{SystemGuid.Attribute.PERSON_LEGAL_NOTE.ToLower()}",
                $"attribute:person:{SystemGuid.Attribute.PERSON_ALLERGY.ToLower()}"
            };

            var dataSources = new List<FieldDataSource>();
            var entityFields = EntityHelper.GetEntityFields( typeof( Person ), true, false );

            foreach ( var entityField in entityFields )
            {
                FieldDataSource dataSource = null;

                if ( entityField.FieldKind == FieldKind.Property )
                {
                    dataSource = GetPropertyDataSource<TLabelData>( entityField, "person", data => data.Person );
                }
                else
                {
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
                }

                if ( dataSource != null )
                {
                    dataSource.TextSubType = TextFieldSubType.AttendeeInfo;

                    if ( keysToMakeCommon.Contains( dataSource.Key ) )
                    {
                        dataSource.Category = "Common";
                    }

                    dataSources.Add( dataSource );
                }
            }

            return dataSources;
        }

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
    }
}
