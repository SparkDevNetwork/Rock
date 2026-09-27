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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava;
using Rock.Tests.Lava.Shared;

namespace Rock.Tests.Integration.Core.Lava.Shortcodes
{
    [TestClass]
    [TestCategory( "Core.Lava.Shortcodes" )]
    public class GoogleMapShortcodeTests : LavaIntegrationTestBase
    {
        #region Templates

        /*
            The googlemap shortcode emits a block of Javascript whose marker array
            and map options are the only parts these tests are about. The templates
            are named constants rather than inline literals because the last test in
            this class renders all of them together, as a single document that can
            be pasted into a Rock instance to check the maps by eye.
        */

        private const string MapWithNoMarkersTemplate = @"
{[ googlemap ]}
{[ endgooglemap ]}
";

        private const string MapWithSingleMarkerAndUnspecifiedZoomTemplate = @"
{[ googlemap ]}
    [[ marker location:'10,100' ]] [[ endmarker ]]
{[ endgooglemap ]}
";

        private const string MapWithSingleMarkerAndZoomToWorldTemplate = @"
{[ googlemap zoom:'1' ]}
    [[ marker location:'10,100' ]] [[ endmarker ]]
{[ endgooglemap ]}
";

        private const string MapWithMultipleMarkersAndUnspecifiedZoomTemplate = @"
{[ googlemap ]}
    [[ marker location:'10,100' ]] [[ endmarker ]]
    [[ marker location:'11,100' ]] [[ endmarker ]]
    [[ marker location:'12,100' ]] [[ endmarker ]]
{[ endgooglemap ]}
";

        private const string MapWithMultipleMarkersAndZoomToWorldTemplate = @"
{[ googlemap zoom:'1' ]}
    [[ marker location:'10,100' ]] [[ endmarker ]]
    [[ marker location:'11,100' ]] [[ endmarker ]]
    [[ marker location:'12,100' ]] [[ endmarker ]]
{[ endgooglemap ]}
";

        #endregion Templates

        #region GoogleMap: Marker Elements

        [TestMethod]
        public void GoogleMapShortcode_WithNoMarkers_RendersEmptyMarkerArray()
        {
            var expectedFragments = new[]
            {
                // The marker array is emitted, but empty.
                @"
// create javascript array of marker info
var markersid<guid> = [
        ];
",
                @"
var mapOptions = {
    zoom: 10,
    center: centerLatLng,
    mapTypeId: 'roadmap',
    zoomControl: true,
    mapTypeControl: false,
    cameraControl: false,
    gestureHandling: 'cooperative',
    streetViewControl: false,
    fullscreenControl: true
}
"
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, MapWithNoMarkersTemplate );

                LavaAssert.ContainsAllIgnoringWhitespace( expectedFragments, output, "<guid>" );
            } );
        }

        #endregion GoogleMap: Marker Elements

        #region GoogleMap: Zoom Parameter

        [TestMethod]
        public void GoogleMapShortcode_WithSingleMarkerAndZoomUnspecified_RendersMapCenteredOnMarkerWithZoomCityLevel()
        {
            var expectedFragments = new[]
            {
                @"
// create javascript array of marker info
var markersid<guid> = [
                [10, 100,'','',''],
        ];
",
                @"
var centerLatLng = new google.maps.LatLng( 10,100 );
",
                // A single marker zooms to city level when no zoom is given.
                @"
var mapOptions = {
    zoom: 11,
"
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, MapWithSingleMarkerAndUnspecifiedZoomTemplate );

                LavaAssert.ContainsAllIgnoringWhitespace( expectedFragments, output, "<guid>" );
            } );
        }

        [TestMethod]
        public void GoogleMapShortcode_WithSingleMarkerAndZoomToWorld_RendersMapCenteredOnMarkerWithZoomWorldLevel()
        {
            var expectedFragments = new[]
            {
                @"
var mapOptions = {
    zoom: 1,
"
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, MapWithSingleMarkerAndZoomToWorldTemplate );

                LavaAssert.ContainsAllIgnoringWhitespace( expectedFragments, output );
            } );
        }

        [TestMethod]
        public void GoogleMapShortcode_WithMultipleMarkersAndZoomUnspecified_RendersMapShowingAllMarkerWithZoomToFit()
        {
            var expectedFragments = new[]
            {
                @"
// create javascript array of marker info
var markersid<guid> = [
            [10, 100,'','',''],
            [11, 100,'','',''],
            [12, 100,'','',''],
        ];
",
                // The map centers on the first marker and zooms to fit the rest.
                @"
var centerLatLng = new google.maps.LatLng( 10,100 );
",
                @"
var mapOptions = {
    zoom: 10,
"
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, MapWithMultipleMarkersAndUnspecifiedZoomTemplate );

                LavaAssert.ContainsAllIgnoringWhitespace( expectedFragments, output, "<guid>" );
            } );
        }

        [TestMethod]
        public void GoogleMapShortcode_WithMultipleMarkersAndZoomToWorld_RendersMapShowingAllMarkersWithZoomWorldLevel()
        {
            var expectedFragments = new[]
            {
                @"
var centerLatLng = new google.maps.LatLng( 10,100 );
",
                @"
var mapOptions = {
    zoom: 1,
"
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, MapWithMultipleMarkersAndZoomToWorldTemplate );

                LavaAssert.ContainsAllIgnoringWhitespace( expectedFragments, output );
            } );
        }

        #endregion GoogleMap: Zoom Parameter

        #region GoogleMap: Combined Document

        /// <summary>
        /// Renders every googlemap example in this class as a single document.
        /// </summary>
        /// <remarks>
        /// The assertions above each cover a fragment of the emitted script. This
        /// renders the same templates together, so that the output can be pasted
        /// into a Rock page to confirm the maps actually draw, and so that a
        /// template which only renders in isolation is caught.
        /// </remarks>
        [TestMethod]
        public void GoogleMapShortcode_ApplicationTestTemplate_CanRender()
        {
            var documentTemplate = string.Join( "\n", new[]
            {
                "<h1>Google Map Shortcode Tests rev20240605.1</h1>",
                "<h3>No Markers</h3><p>Expected: No Map</p><hr>",
                MapWithNoMarkersTemplate,
                "<h3>Single Marker, Zoom=(unspecified)</h3><p>Expected: Map centered on single marker, zoomed to city-level</p><hr>",
                MapWithSingleMarkerAndUnspecifiedZoomTemplate,
                "<h3>Single Marker, Zoom=1 (world)</h3><p>Expected: Map centered on single marker, zoomed to world-level</p><hr>",
                MapWithSingleMarkerAndZoomToWorldTemplate,
                "<h3>Multiple Markers, Zoom=(unspecified)</h3><p>Expected: Map showing all markers, zoomed to fit</p><hr>",
                MapWithMultipleMarkersAndUnspecifiedZoomTemplate,
                "<h3>Multiple Markers, Zoom=1 (world)</h3><p>Expected: Map showing all markers, zoomed to world-level</p><hr>",
                MapWithMultipleMarkersAndZoomToWorldTemplate
            } );

            var options = new LavaRenderOptions
            {
                ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.Throw
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var result = LavaRenderTestHelper.RenderResult( engine, documentTemplate, options );

                Assert.IsFalse( result.HasErrors, $"Document template reported an error. [Error={result.Error?.Message}]" );
            } );
        }

        #endregion GoogleMap: Combined Document

        #region GoogleStaticMap

        [TestMethod]
        public void GoogleStaticMapShortcode_DocumentationExample_EmitsCorrectHtml()
        {
            var inputTemplate = @"
{[ googlestaticmap center:'10451 W Palmeras Dr Sun City, AZ 85373-2000' zoom:'12' ]}
{[ endgooglestaticmap ]}
";

            var expectedFragments = new[]
            {
                // No Google API key is configured for the test environment, so the
                // shortcode renders its warning alongside the image.
                @"
<div class=""alert alert-warning"">
    There is no Google API key defined. Please add your key under: 'Admin Tools > General Settings > Global Attributes > Google API Key'.
</div>
",
                @"<img src=""https://maps.googleapis.com/maps/api/staticmap?size=640x320&maptype=roadmap&scale=2&format=png8&zoom=12&center=10451%20W%20Palmeras%20Dr%20Sun%20City%2C%20AZ%2085373-2000&key="" style=""width: 100%"" />"
            };

            LavaRenderTestHelper.ExecuteForActiveEngines( engine =>
            {
                var output = LavaRenderTestHelper.Render( engine, inputTemplate );

                LavaAssert.ContainsAllIgnoringWhitespace( expectedFragments, output );
            } );
        }

        #endregion GoogleStaticMap
    }
}
