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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Rock.Enums.CheckIn.Labels;
using Rock.Reporting;

namespace Rock.CheckIn.v2.Labels
{
    internal static class FieldDataSources
    {
        private static readonly ConcurrentDictionary<LabelType, Dictionary<string, FieldDataSource>> _sources = new ConcurrentDictionary<LabelType, Dictionary<string, FieldDataSource>>();

        public static IReadOnlyDictionary<string, FieldDataSource> GetDataSources( LabelType labelType )
        {
            return _sources.GetOrAdd( labelType, lt =>
            {
                var dataSources = new List<FieldDataSource>();

                if ( lt == LabelType.Person )
                {
                    dataSources.Add( new SingleValueFieldDataSource<PersonLabelData>
                    {
                        Key = "person.birthdate",
                        Name = "Birth Date",
                        TextSubType = TextFieldSubType.AttendeeInfo,
                        SupportedComparisionTypes = ComparisonHelper.DateFilterComparisonTypes,
                        Category = "Common",
                        Formatter = DateDataFormatter.Instance,
                        ComparisonValueFunc = ( source, field, printRequest ) => source.Person.BirthDate,
                        ValueFunc = ( source, field, printRequest ) => source.Person.BirthDate
                    } );

                    dataSources.Add( new SingleValueFieldDataSource<PersonLabelData>
                    {
                        Key = "person.id",
                        Name = "Id",
                        TextSubType = TextFieldSubType.AttendeeInfo,
                        SupportedComparisionTypes = ComparisonHelper.NumericFilterComparisonTypesRequired,
                        Category = "Properties",
                        ComparisonValueFunc = ( source, field, printRequest ) => source.Person.Id,
                        ValueFunc = ( source, field, printRequest ) => source.Person.Id
                    } );
                }

                return dataSources.ToDictionary( d => d.Key, d => d );
            } );
        }
    }
}
