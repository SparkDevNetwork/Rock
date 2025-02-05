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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava;
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Core.Lava.Shortcodes
{
    [TestClass]
    [TestCategory("Core.Lava.Shortcodes")]
    public class GoogleMapShortcodeTests : LavaIntegrationTestBase
    {
        #region GoogleMap

        #region Zoom Parameter

        [TestMethod]
        public void GoogleMapShortcode_WithSingleMarkerAndZoomUnspecified_RendersMapCenteredOnMarkerWithZoomCityLevel()
        {
            var testCase = GetTestCaseForMapWithSingleMarkerWithUnspecifiedZoom();
            TestHelper.AssertRenderTestIsValid( testCase );
        }

        [TestMethod]
        public void GoogleMapShortcode_WithSingleMarkerAndZoomToWorld_RendersMapCenteredOnMarkerWithZoomWorldLevel()
        {
            var testCase = GetTestCaseForMapWithSingleMarkerWithZoomToWorld();
            TestHelper.AssertRenderTestIsValid( testCase );
        }

        [TestMethod]
        public void GoogleMapShortcode_WithMultipleMarkersAndZoomUnspecified_RendersMapShowingAllMarkerWithZoomToFit()
        {
            var testCase = GetTestCaseForMapWithMultipleMarkersAndZoomUnspecified();
            TestHelper.AssertRenderTestIsValid( testCase );
        }

        [TestMethod]
        public void GoogleMapShortcode_WithMultipleMarkersAndZoomToWorld_RendersMapShowingAllMarkersWithZoomWorldLevel()
        {
            var testCase = GetTestCaseForMapWithMultipleMarkersAndZoomToWorld();
            TestHelper.AssertRenderTestIsValid( testCase );
        }

        #endregion

        [TestMethod]
        public void GoogleMapShortcode_ApplicationTestTemplate_CanRender()
        {
            var testCases = new List<LavaTemplateRenderTestCase>();

            testCases.Add( GetTestCaseForMapWithNoMarkers() );
            testCases.Add( GetTestCaseForMapWithSingleMarkerWithUnspecifiedZoom() );
            testCases.Add( GetTestCaseForMapWithSingleMarkerWithZoomToWorld() );
            testCases.Add( GetTestCaseForMapWithMultipleMarkersAndZoomUnspecified() );
            testCases.Add( GetTestCaseForMapWithMultipleMarkersAndZoomToWorld() );

            var testTemplate = TestHelper.BuildRenderTestTemplate( testCases,
                "Google Map Shortcode Tests rev20240605.1" );

            TestHelper.AssertTemplateIsValid( testTemplate, ( LavaRenderParameters ) null );
        }

        private LavaTemplateRenderTestCase GetTestCaseForMapWithNoMarkers()
        {
            var testCase = new LavaTemplateRenderTestCase
            {
                Category = "Test Set 1: Marker Elements",
                Name = "No Markers",
                Description = "Expected: No Map"
            };

            testCase.InputTemplate = @"
{[ googlemap ]}
{[ endgooglemap ]}
";

            // Verify array of marker info.
            testCase.MatchRequirements.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
// create javascript array of marker info
var markersid<guid> = [
        ];
" ) );
            // Verify Map Options.
            testCase.MatchRequirements.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
var mapOptions = {
    zoom: 10,
    scrollwheel: true,
    draggable: true,
    center: centerLatLng,
    mapTypeId: 'roadmap',
    zoomControl: true,
    mapTypeControl: false,
    gestureHandling: 'cooperative',
    streetViewControl: false,
    fullscreenControl: true
}
" ) );

            return testCase;
        }

        private LavaTemplateRenderTestCase GetTestCaseForMapWithSingleMarkerWithUnspecifiedZoom()
        {
            var testCase = new LavaTemplateRenderTestCase
            {
                Category = "Zoom Parameter Tests",
                Name = "Single Marker, Zoom=(unspecified)",
                Description = "Expected: Map centered on single marker, zoomed to city-level"
            };

            testCase.InputTemplate = @"
{[ googlemap ]}
    [[ marker location:'10,100' ]] [[ endmarker ]]
{[ endgooglemap ]}
";

            // Verify array of marker info.
            testCase.MatchRequirements.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
// create javascript array of marker info
var markersid<guid> = [
                [10, 100,'','',''],
        ];
" ) );

            // Verify center marker.
            testCase.MatchRequirements.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
var centerLatLng = new google.maps.LatLng( 10,100 );
" ) );

            // Verify Map Option: Zoom
            testCase.MatchRequirements.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
var mapOptions = {
    zoom: 11,
" ) );

            return testCase;
        }

        private LavaTemplateRenderTestCase GetTestCaseForMapWithSingleMarkerWithZoomToWorld()
        {
            var testCase = new LavaTemplateRenderTestCase
            {
                Category = "Zoom Parameter Tests",
                Name = "Single Marker, Zoom=1 (world)",
                Description = "Expected: Map centered on single marker, zoomed to world-level"
            };

            testCase.InputTemplate = @"
<h3>Single Marker, Zoom=1 (world)</h3>
<p>Expected: Map centered on single marker, zoomed to world-level</p>
<hr>
{[ googlemap zoom:'1' ]}
    [[ marker location:'10,100' ]] [[ endmarker ]]
{[ endgooglemap ]}
";

            // Verify Map Option: Zoom
            testCase.MatchRequirements.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
var mapOptions = {
    zoom: 1,
" ) );

            return testCase;
        }

        private LavaTemplateRenderTestCase GetTestCaseForMapWithMultipleMarkersAndZoomUnspecified()
        {
            var testCase = new LavaTemplateRenderTestCase
            {
                Category = "Zoom Parameter Tests",
                Name = "Multiple Markers, Zoom=(unspecified)",
                Description = "Expected: Map showing all markers, zoomed to fit"
            };

            testCase.InputTemplate = @"
{[googlemap]}
    [[marker location:'10,100' ]] [[endmarker]]
    [[marker location:'11,100' ]] [[endmarker]]
    [[marker location:'12,100' ]] [[endmarker]]
{[endgooglemap]}
";

            // Verify array of marker info.
            testCase.MatchRequirements.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
// create javascript array of marker info
var markersid<guid> = [
            [10, 100,'','',''],
            [11, 100,'','',''],
            [12, 100,'','',''],
        ];
" ) );

            // Verify center marker.
            testCase.MatchRequirements.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
var centerLatLng = new google.maps.LatLng( 10,100 );
" ) );

            // Verify Map Options: Zoom
            testCase.MatchRequirements.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
var mapOptions = {
    zoom: 10,
" ) );

            return testCase;
        }

        private LavaTemplateRenderTestCase GetTestCaseForMapWithMultipleMarkersAndZoomToWorld()
        {
            var testCase = new LavaTemplateRenderTestCase
            {
                Category = "Zoom Parameter Tests",
                Name = "Multiple Markers, Zoom=1 (world)",
                Description = "Expected: Map showing all markers, zoomed to world-level"
            };

            testCase.InputTemplate = @"
{[ googlemap zoom:'1' ]}
    [[ marker location:'10,100' ]] [[ endmarker ]]
    [[ marker location:'11,100' ]] [[ endmarker ]]
    [[ marker location:'12,100' ]] [[ endmarker ]]
{[ endgooglemap ]}
";

            // Verify center marker.
            testCase.MatchRequirements.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
var centerLatLng = new google.maps.LatLng( 10,100 );
" ) );

            // Verify Map Options: Zoom
            testCase.MatchRequirements.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
var mapOptions = {
    zoom: 1,
" ) );

            return testCase;
        }

        #endregion

        #region GoogleStaticMap

        [TestMethod]
        public void GoogleStaticMapShortcode_DocumentationExample_EmitsCorrectHtml()
        {
            var input = @"
{[ googlestaticmap center:'10451 W Palmeras Dr Sun City, AZ 85373-2000' zoom:'12' ]}
{[ endgooglestaticmap ]}
";

            var expectedOutput = @"
<div class=""alert alert-warning"">
    There is no Google API key defined. Please add your key under: 'Admin Tools > General Settings > Global Attributes > Google API Key'.
</div>
<div style=""width: 100%"">
    <img src=""https://maps.googleapis.com/maps/api/staticmap?size=640x320&maptype=roadmap&scale=2&format=png8&zoom=12&center=10451%20W%20Palmeras%20Dr%20Sun%20City%2C%20AZ%2085373-2000&key="" style=""width: 100%"" />
</div>
";

            expectedOutput = expectedOutput.Replace( "``", @"""" );

            TestHelper.AssertTemplateOutput( expectedOutput, input );
        }

        #endregion
    }
}
