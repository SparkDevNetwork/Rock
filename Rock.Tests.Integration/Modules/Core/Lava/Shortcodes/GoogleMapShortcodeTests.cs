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
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.Shortcodes
{
    [TestClass]
    [TestCategory("Core.Lava.Shortcodes")]
    public class GoogleMapShortcodeTests : LavaIntegrationTestBase
    {
        #region GoogleMap

        /// <summary>
        /// Verifies that the common elements of this shortcode are rendered correctly.
        /// </summary>
        [TestMethod]
        public void GoogleMapShortcode_DefaultOptions_ContainsExpectedOutputElements()
        {
            var input = @"
{[ googlemap ]}
    [[ marker location:'33.640705,-112.280198' ]] [[ endmarker ]]
{[ endgooglemap ]}
";

            var expectedMatches = new List<LavaTestOutputMatchRequirement>();

            // API Key warning.
            expectedMatches.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
<div class=""alert alert-warning"">
    There is no Google API key defined. Please add your key under: 'Admin Tools > General Settings > Global Attributes > Google API Key'.
</div>
" ) );


            // Styling
            expectedMatches.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
<script src='https://maps.googleapis.com/maps/api/js?key=' type='text/javascript'></script>

<style>

.id<<guid>> {
    width: 100%;
}

#map-container-id<<guid>> {
    position: relative;
}

#id<<guid>> {
    height: 600px;
    overflow: hidden;
    padding-bottom: 22.25%;
    padding-top: 30px;
    position: relative;
}

</style>

<div class=""map-container id<<guid>>"">
    <div id=""map-container-id<<guid>>""></div>
	<div id=""id<<guid>>""></div>
</div>
" ) );

            // Verify Javascript to initialize map.
            expectedMatches.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
//Set Map
function initializeid<<guid>>() {
    var bounds = new google.maps.LatLngBounds();
        
            var centerLatLng = new google.maps.LatLng( 33.640705,-112.280198 );
    if ( isNaN( centerLatLng.lat() ) || isNaN( centerLatLng.lng() ) ) {
        centerLatLng = null;
    };
                
    var mapOptions = {
        zoom: 11,
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

    var map = new google.maps.Map(document.getElementById('id<<guid>>'), mapOptions);
    var infoWindow = new google.maps.InfoWindow(), marker, i;
        
    // place each marker on the map
    for( i = 0; i < markersid<<guid>>.length; i++ ) {
        var position = new google.maps.LatLng(markersid<<guid>>[i][0], markersid<<guid>>[i][1]);
        bounds.extend(position);
        marker = new google.maps.Marker({
            position: position,
            map: map,
            animation: null,
            title: markersid<<guid>>[i][2],
            icon: markersid<<guid>>[i][4]
        });

        // Add info window to marker
        google.maps.event.addListener(marker, 'click', (function(marker, i) {
            return function() {
                if (markersid<<guid>>[i][3] != ''){
                    infoWindow.setContent(markersid<<guid>>[i][3]);
                    infoWindow.open(map, marker);
                }
            }
        }) (marker, i));
    }
" ) );

            // Resize function.
            expectedMatches.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
// Center the map to fit all markers on the screen

//Resize Function
google.maps.event.addDomListener(window, ""resize"", function() {
	var center = map.getCenter();
    if ( center ) {
        google.maps.event.trigger(map, ""resize"");
        map.setCenter(center);
    }
});
" ) );

            // Window Initialization
            expectedMatches.Add( LavaTestOutputMatchRequirement.NewContainsText( @"
    google.maps.event.addDomListener(window, 'load', initializeid<<guid>>);
" ) );

            var options = new LavaTestRenderOptions { Wildcards = new List<string> { "<<guid>>" } };

            TestHelper.AssertTemplateOutput( expectedMatches, input, options );
        }

        [TestMethod]
        public void GoogleMapShortcode_WithNoMarkerElements_RendersEmptyMapPlaceholder()
        {
            var testCase = GetTestCaseForMapWithNoMarkers();
            TestHelper.AssertRenderTestIsValid( testCase );
        }

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

            TestHelper.AssertTemplateIsValid( testTemplate );
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
