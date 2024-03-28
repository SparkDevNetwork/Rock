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

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Performance.Modules.Crm.Person;

namespace Rock.Tests.Performance.BenchmarkRunners
{
    [TestClass]
    [Ignore( "Benchmarks must be run in Release mode, this should be updated to only run via CLI." )]
    public class BenchmarkRunners
    {
        [TestMethod]
        public void PersonPickerController_PerformanceForMultipleQueries()
        {
            var summary = BenchmarkRunner.Run<PersonSearchBenchmarks>();
        }
    }

    [HtmlExporter]
    [PlainExporter]
    [MinColumn, MaxColumn]
    [Outliers( Perfolizer.Mathematics.OutlierDetection.OutlierMode.DontRemove )]
    [MinIterationCount( 100 )]
    [MaxIterationCount( 101 )]
    public class PersonSearchBenchmarks
    {
        private static PersonPickerControllerTests _tests = new PersonPickerControllerTests();

        private static List<string> _nameSearchStrings = new List<string>
            {
                // Search strings for Sample Database.
                "Decker", "Greggs", "Jackson", "Jones", "Lowe", "Marble", "Miller", "Peterson", "Simmons", "Tucker", "Webb"

                // Search strings for Spark Site.
                //"Leigh", "Airdo", "Cummings", "Drotning", "Edmiston", "Hazelbaker", "Kishor", "Pena", "Peterson", "Zimmerman"
            };
        private static List<string> _addressSearchStrings = new List<string>
            {
                // Search strings for Sample Database.
                "18th", "30th", "Bloomfield", "Harmont", "Shangri La", "Lupine", "Eugie", "Curnow", "33rd"

                // Search strings for Spark Database.
                //"18th", "30th", "Main", "Street", "Route", "Drive", "Box", "Walnut", "33rd"
                //"24654 N Lake Pleasant Pkwy # 103-218"
            };

        [Benchmark( Description = "Linq", Baseline = true )]
        public void AddressSearchLinq()
        {
            _tests.SearchAddressWithLinq( _addressSearchStrings );
        }

        [Benchmark( Description = "Name_NoDetails" )]
        public void NameSearchNoDetails()
        {
            _tests.SearchWithRestController( "name", _nameSearchStrings, includeDetails: false );
        }

        [Benchmark( Description = "Name_WithDetails" )]
        public void NameSearchWithDetails()
        {
            _tests.SearchWithRestController( "name", _nameSearchStrings, includeDetails: true );
        }

        [Benchmark( Description = "Address_NoDetails" )]
        public void AddressSearchNoDetails()
        {
            _tests.SearchWithRestController( "address", _addressSearchStrings, includeDetails: false );
        }

        [Benchmark( Description = "Address_WithDetails" )]
        public void AddressSearchWithDetails()
        {
            _tests.SearchWithRestController( "address", _addressSearchStrings, includeDetails: true );
        }
    }
}
