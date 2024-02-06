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
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Http.TestLibrary;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Controllers;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Performance.Modules.Crm.Person
{
    [TestClass]
    public class PersonPickerControllerTests : DatabaseTestsBase
    {
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

        [TestMethod]
        public void NameSearch()
        {
            SearchWithRestController( "name", _nameSearchStrings );
        }

        [TestMethod]
        public void Linq_Street1Search()
        {
            SearchAddressWithLinq( _addressSearchStrings );
        }

        [TestMethod]
        public void AddressSearchSimple()
        {
            SearchWithRestController( "address", _addressSearchStrings, includeDetails: false );
            SearchWithRestController( "address", _addressSearchStrings, includeDetails: false );
        }

        [TestMethod]
        public void PersonPickerController_AddressSearchWithDetail()
        {
            SearchWithRestController( "address", _addressSearchStrings, includeDetails: true );
            SearchWithRestController( "address", _addressSearchStrings, includeDetails: true );
        }

        [TestMethod]
        public void AddressSearchWithManyMatchesAndIncludeDetails_ReturnsResult()
        {
            SearchWithRestController( "address",
                new List<string> { "Main" },
                includeDetails: true );
        }

        [TestMethod]
        public void NameSearchForPersonWithManyAliases_IsPerformant()
        {
            // This search tests for a significant performance issue that previously occurred
            // when the search result included the default merge target person "Spam Deleted".
            SearchWithRestController( "name",
                new List<string> { "Warmup", "Deleted, Spam" },
                includeDetails: true );
        }

        [TestMethod]
        public void TestSearchMany()
        {
            // Pre-load the Countries Defined Type because it adds a substantial delay to the first search.
            var dtc = DefinedTypeCache.Get( new Guid( SystemGuid.DefinedType.LOCATION_COUNTRIES ) );
            dtc.GetDefinedValueFromValue( "?" );

            SearchWithRestController( "name",
                new List<string> { "Leigh", "Airdo", "Deleted, Spam", "Cummings", "Deleted2, Spam2", "Drotning" },
                includeDetails: true );
        }

        public void SearchWithRestController( string searchField, List<string> searchStrings, bool includeDetails = true )
        {
            for ( int i = 0; i < searchStrings.Count; i++ )
            {
                var nameSearchText = string.Empty;
                var addressSearchText = string.Empty;
                var searchText = searchStrings[i];

                if ( searchField == "name" )
                {
                    nameSearchText = searchText;
                }
                else if ( searchField == "address" )
                {
                    addressSearchText = searchText;
                }
                else
                {
                    throw new Exception( "Unknown search field." );
                }

                using ( var simulator = GetHttpSimulator() )
                {
                    using ( var request = simulator.SimulateRequest( new Uri( "http://www.rocksolidchurch.com/" ) ) )
                    {
                        TestHelper.StartTimer( $"** Search {i}: [{searchField}]='{searchText}'" );
                        var controller = new PeopleController();
                        var result = controller.Search( name: nameSearchText,
                            includeDetails: includeDetails,
                            address: addressSearchText );
                        TestHelper.EndTimer( $"** Search {i}: [{searchField}]='{searchText}'" );

                        var resultItems = result.ToList();

                        // Verify that we have at least 1 match.
                        if ( resultItems.Count == 0 )
                        {
                            Debug.WriteLine( $"WARNING: Search \"{searchText}\" returned no items." );
                        }
                    }
                }
            }
        }

        public void SearchAddressWithLinq( List<string> searchStrings )
        {
            var simulator = GetHttpSimulator();

            for ( int i = 0; i < searchStrings.Count; i++ )
            {
                var searchText = searchStrings[i];

                var dataContext = new RockContext();
                var locationService = new LocationService( dataContext );
                var items = locationService.Queryable()
                    .AsNoTracking()
                    .Where( x => x.Street1.Contains( searchText ) )
                    .ToList();

                // Verify that we have at least 1 match.
                if ( items.Count == 0 )
                {
                    Debug.WriteLine( $"WARNING: Search \"{searchText}\" returned no items." );
                }
            }
        }

        private HttpSimulator _simulator = null;
        private string _webContentFolder = null;

        private string GetWebContentFolder()
        {
            if ( _webContentFolder == null )
            {
                var codeBaseUrl = new Uri( System.Reflection.Assembly.GetExecutingAssembly().CodeBase );
                var codeBasePath = Uri.UnescapeDataString( codeBaseUrl.AbsolutePath );
                var dirPath = System.IO.Path.GetDirectoryName( codeBasePath );
                _webContentFolder = System.IO.Path.Combine( dirPath, "Content" );
            }
            return _webContentFolder;
        }

        private HttpSimulator GetHttpSimulator()
        {
            _simulator = new HttpSimulator( "/", GetWebContentFolder() );
            _simulator.DebugWriter = TextWriter.Null;
            return _simulator;
        }
    }
}
