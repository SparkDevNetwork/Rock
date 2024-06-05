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

        private static List<FieldDataSource> GetPersonLabelDataSources()
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
                    dataSource = GetPropertyDataSource<PersonLabelData>( entityField, "person", data => data.Person );
                }
                else
                {
                    var attributeCache = AttributeCache.Get( entityField.AttributeGuid.Value );

                    if ( entityField.FieldType.Field is DateFieldType || entityField.FieldType.Field is DateTimeFieldType )
                    {
                        dataSource = new DateAttributeFieldDataSource<PersonLabelData>( attributeCache, "person", data => data.Person );
                    }
                    else
                    {
                        dataSource = new SingleValueFieldDataSource<PersonLabelData>
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

            var customDataSources = GetCustomPersonLabelSources();
            var customKeys = customDataSources.Select( ds => ds.Key ).ToList();

            dataSources.RemoveAll( ds => customKeys.Contains( ds.Key ) );
            dataSources.AddRange( customDataSources );

            return dataSources;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0028:Simplify collection initialization", Justification = "Because the list of options is so long it is more clear to use the Add() method." )]
        private static List<FieldDataSource> GetCustomPersonLabelSources()
        {
            var dataSources = new List<FieldDataSource>();

            dataSources.Add( new SingleValueFieldDataSource<PersonLabelData>
            {
                Key = "person.fullname",
                Name = "Full Name",
                TextSubType = TextFieldSubType.AttendeeInfo,
                Category = "Common",
                Formatter = FullNameDataFormatter.Instance,
                ValueFunc = ( source, field, printRequest ) => source.Person
            } );

            dataSources.Add( new SingleValueFieldDataSource<PersonLabelData>
            {
                Key = "person.gender",
                Name = "Gender",
                TextSubType = TextFieldSubType.AttendeeInfo,
                Category = "Common",
                Formatter = GenderDataFormatter.Instance,
                ValueFunc = ( source, field, printRequest ) => source.Person.Gender
            } );

            dataSources.Add( new SingleValueFieldDataSource<PersonLabelData>
            {
                Key = "person.gradeoffset",
                Name = "Grade Offset",
                TextSubType = TextFieldSubType.AttendeeInfo,
                Category = "Common",
                ValueFunc = ( source, field, printRequest ) => source.Person.GradeOffset
            } );

            dataSources.Add( new SingleValueFieldDataSource<PersonLabelData>
            {
                Key = "person.gradeformatted",
                Name = "Grade Formatted",
                TextSubType = TextFieldSubType.AttendeeInfo,
                Category = "Common",
                Formatter = GradeDataFormatter.Instance,
                ValueFunc = ( source, field, printRequest ) => source.Person.GraduationYear
            } );

            dataSources.Add( new SingleValueFieldDataSource<PersonLabelData>
            {
                Key = "person.age",
                Name = "Age",
                TextSubType = TextFieldSubType.AttendeeInfo,
                Category = "Common",
                Formatter = PersonAgeDataFormatter.Instance,
                ValueFunc = ( source, field, printRequest ) => source.Person.AgePrecise
            } );

            dataSources.Add( new SingleValueFieldDataSource<PersonLabelData>
            {
                Key = "person.birthdaydayofweek",
                Name = "Birthday Day Of Week",
                TextSubType = TextFieldSubType.AttendeeInfo,
                Category = "Common",
                Formatter = WeekdayDateDataFormatter.Instance,
                ValueFunc = ( source, field, printRequest ) => source.Person.ThisYearsBirthdate
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
