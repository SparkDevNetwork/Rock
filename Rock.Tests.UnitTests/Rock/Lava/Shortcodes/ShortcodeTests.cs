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
using System.Diagnostics;
using System.Text.RegularExpressions;
using Rock.Lava;
using Rock.Lava.Shortcodes;
using Xunit;

namespace Rock.Tests.Rock.Lava.Fluid
{
    [Collection( LavaFiltersTestCollectionFixture.Name )]
    public class ShortcodeTests
    {
        ILavaTemplate _template;
        TestHelper _helper;

        #region Constructors

        public ShortcodeTests( DynamicTemplateFixture fixture )
        {
            _template = fixture.GetTemplate();

            _helper = new TestHelper( _template );
        }

        #endregion

        /// <summary>
        /// Verify the output of a Lava Shortcode Tag implemented using standard Liquid syntax (Example: {% shortcode 'youtube', ... %}).
        /// </summary>
        [Fact]
        public void ShortcodeDynamicTag_StandardTagSyntax_ProducesExpectedOutput()
        {
            var engine = _template.LavaEngine;

            SendEngineInfoToDebug( engine );

            // Register the shortcode definition.
            var shortcode = new global::Rock.Lava.Fluid.FluidDynamicShortcodeTag();

            shortcode.Initialize( "youtube", Constants.Shortcode.YoutubeMarkup, Constants.Shortcode.YoutubeParameters.SplitDelimitedValues( "," ) );

            engine.RegisterShortcode( shortcode );

            // Render a test template using the shortcode.
            var result = _helper.GetTemplateOutput( "{% youtube id:'8kpHK4YIwY4', showinfo:'false', controls:'false' %}" );

            AssertLavaTemplateOutputEquivalent( Constants.Shortcode.YoutubeOutput, result );
        }

        /// <summary>
        /// Verify the output of a Lava Shortcode Tag implemented using standard Liquid syntax (Example: {% shortcode 'youtube', ... %}).
        /// </summary>
        [Fact]
        public void ShortcodeDynamicBlock_StandardBlockSyntax_ProducesExpectedOutput()
        {
            var engine = _template.LavaEngine;

            SendEngineInfoToDebug( engine );

            // Register the shortcode definition.
            var shortcode = new global::Rock.Lava.Fluid.FluidDynamicShortcodeBlock();

            shortcode.Initialize( "parallax", Constants.Shortcode.ParallaxMarkup, Constants.Shortcode.ParallaxParameters.SplitDelimitedValues( "," ) );

            engine.RegisterShortcode( shortcode );

            // Render a test template using the shortcode.
            var testBlock = @"
{% parallax image:'http://cdn.wonderfulengineering.com/wp-content/uploads/2014/09/star-wars-wallpaper-4.jpg'@@speed:'0.2'@@height:'400px'@@position:'top'@@contentpadding:'20px' %}
<h1>Hello World</h1>
{% endparallax %}
";

            var result = _helper.GetTemplateOutput( testBlock );

            AssertLavaTemplateOutputEquivalent( Constants.Shortcode.ParallaxOutput, result );
        }

        private void AssertLavaTemplateOutputEquivalent(string expectedOutput, string actualOutput)
        {
            // Replace any instance-specific unique identifiers in the output.
            actualOutput = ReplaceGuidWithText( actualOutput, "uniqueid" );
            expectedOutput = ReplaceGuidWithText( expectedOutput, "uniqueid" );

            // Remove whitespace.
            actualOutput = Regex.Replace( actualOutput, @"\s+", "" );            
            expectedOutput = Regex.Replace( expectedOutput, @"\s+", "" );

            Assert.Equal( expectedOutput, actualOutput );
        }

        /// <summary>
        /// Verify the output of a Lava Shortcode Tag.
        /// </summary>
        [Fact]
        public void ShortcodeDynamicTag_LavaSyntax_ProducesString()
        {
            var engine = _template.LavaEngine;

            SendEngineInfoToDebug( engine );

            // Register the shortcode definition.
            var shortcode = new global::Rock.Lava.Fluid.FluidDynamicShortcodeBlock();

            shortcode.Initialize( "youtube", Constants.Shortcode.YoutubeMarkup, Constants.Shortcode.YoutubeParameters.SplitDelimitedValues( "," ) );

            engine.RegisterShortcode( shortcode );

            // Render a test template using the shortcode.
            var result = _helper.GetTemplateOutput( "{[ youtube id:'8kpHK4YIwY4' showinfo:'false' controls:'false' ]}" );

            AssertLavaTemplateOutputEquivalent( Constants.Shortcode.YoutubeOutput, result );
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void ShortcodeDynamicBlock_ParallaxWithLavaSyntax_ProducesString()
        {
            SendEngineInfoToDebug( _template.LavaEngine );

            var engine = _template.LavaEngine;

            // Define the factory method that is used to create new instances of the shortcode.
            IRockShortcode ShortcodeFactoryMethod( string name )
            {
                var shortcode = new global::Rock.Lava.Fluid.FluidDynamicShortcodeBlock();

                shortcode.Markup = Constants.Shortcode.ParallaxMarkup;
                shortcode.Parameters = Constants.Shortcode.ParallaxParameters;
                shortcode.EnabledLavaCommands = "";

                return shortcode;
            };

            engine.RegisterShortcode( "parallax", ShortcodeFactoryMethod );

            var testBlock = @"
{[ parallax image:'http://cdn.wonderfulengineering.com/wp-content/uploads/2014/09/star-wars-wallpaper-4.jpg' speed:'0.2' height:'400px' position:'top' contentpadding:'20px' ]}
<h1>Hello World</h1>
{[ endparallax ]}
";
            var result = _helper.GetTemplateOutput( testBlock );

            var expectedOutput = Constants.Shortcode.ParallaxOutput;

            AssertLavaTemplateOutputEquivalent( Constants.Shortcode.ParallaxOutput, result );
        }

        #region Helper methods

        private string ReplaceGuidWithText( string input, string replacementText )
        {
            var result = Regex.Replace( input, @"[({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?", replacementText, RegexOptions.IgnoreCase );

            return result;
        }

        private void SendEngineInfoToDebug( ILavaEngine engine )
        {
            Debug.WriteLine( $"Lava Framework: {engine.FrameworkName}" );
        }

        #endregion
    }

    public static class Constants
    {
        public static class Shortcode
        {
            public const string YoutubeMarkup = @"
{% assign wrapperId = uniqueid %}

{% assign parts = id | Split:'/' %}
{% assign id = parts | Last %}
{% assign parts = id | Split:'=' %}
{% assign id = parts | Last | Trim %}

{% assign url = 'https://www.youtube.com/embed/' | Append:id | Append:'?rel=0' %}

{% assign showinfo = showinfo | AsBoolean %}
{% assign controls = controls | AsBoolean %}
{% assign autoplay = autoplay | AsBoolean %}

{% if showinfo %}
    {% assign url = url | Append:'&showinfo=1' %}
{% else %}
    {% assign url = url | Append:'&showinfo=0' %}
{% endif %}

{% if controls %}
    {% assign url = url | Append:'&controls=1' %}
{% else %}
    {% assign url = url | Append:'&controls=0' %}
{% endif %}

{% if autoplay %}
    {% assign url = url | Append:'&autoplay=1' %}
{% else %}
    {% assign url = url | Append:'&autoplay=0' %}
{% endif %}

<style>

#{{ wrapperId }} {
    width: {{ width }};
}

.embed-container { 
    position: relative; 
    padding-bottom: 56.25%; 
    height: 0; 
    overflow: hidden; 
    max-width: 100%; } 
.embed-container iframe, 
.embed-container object, 
.embed-container embed { position: absolute; top: 0; left: 0; width: 100%; height: 100%; }
</style>

<div id='{{ wrapperId }}'>
    <div class='embed-container'><iframe src='{{ url }}' frameborder='0' allowfullscreen></iframe></div>
</div>
";

            public const string YoutubeOutput = @"
<style>

#id-9fbb63b1-d5c2-4db0-80ca-76a2a43c31ea {
    width: ;
}

.embed-container { 
    position: relative; 
    padding-bottom: 56.25%; 
    height: 0; 
    overflow: hidden; 
    max-width: 100%; } 
.embed-container iframe, 
.embed-container object, 
.embed-container embed { position: absolute; top: 0; left: 0; width: 100%; height: 100%; }
</style>

<div id='id-9fbb63b1-d5c2-4db0-80ca-76a2a43c31ea'>
    <div class='embed-container'><iframe src='https://www.youtube.com/embed/8kpHK4YIwY4?rel=0&showinfo=0&controls=0&autoplay=0' frameborder='0' allowfullscreen></iframe></div>
</div>
";

            public const string YoutubeParameters = "id,showinfo,controls,autoplay,width";

            public const string ParallaxMarkup = @"
{{ 'https://cdnjs.cloudflare.com/ajax/libs/jarallax/1.11.1/jarallax.min.js' | AddScriptLink }}
{% if videourl != '' -%}
    {{ 'https://cdnjs.cloudflare.com/ajax/libs/jarallax/1.11.1/jarallax-video.min.js' | AddScriptLink }}
{% endif -%}

{% assign id = uniqueid -%} 
{% assign bodyZindex = zindex | Plus:1 -%}

{% assign speed = speed | AsInteger %}

{% if speed > 0 -%}
    {% assign speed = speed | Times:'.01' -%}
    {% assign speed = speed | Plus:'1' -%}
{% elseif speed == 0 -%}
    {% assign speed = 1 -%}
{% else -%}
    {% assign speed = speed | Times:'.02' -%}
    {% assign speed = speed | Plus:'1' -%}
{% endif -%}


 
{% if videourl != '' -%}
    <div id='{{ id }}' class='jarallax' data-jarallax-video='{{ videourl }}'
            data-type='{{ type }}'
            data-speed='{{ speed | Format:'0' }}'
            data-img-position='{{ position }}'
            data-object-position='{{ position }}'
            data-background-position='{{ position }}'
            data-zindex='{{ bodyZindex }}'
            data-no-android='{{ noandroid }}'
            data-no-ios='{{ noios }}'>
{% else -%}
    <div id = '{{ id }}' data-jarallax class='jarallax' data-type='{{ type }}' data-speed='{{ speed }}' data-img-position='{{ position }}' data-object-position='{{ position }}' data-background-position='{{ position }}' data-zindex='{{ bodyZindex }}' data-no-android='{{ noandroid }}' data-no-ios='{{ noios }}'>
        <img class='jarallax-img' src='{{ image }}' alt=''>
{% endif -%}

        {% if blockContent != '' -%}
            <div class='parallax-content'>
                {{ blockContent }}
            </div>
        {% else -%}
            {{ blockContent }}
        {% endif -%}
    </div>

/* stylesheet */
#{{ id }} {
    /* eventually going to change the height using media queries with mixins using sass, and then include only the classes I want for certain parallaxes */
    min-height: {{ height }};
    background: transparent;
    position: relative;
    z-index: 0;
}

#{{ id }} .jarallax-img {
    position: absolute;
    object-fit: cover;
    /* support for plugin https://github.com/bfred-it/object-fit-images */
    font-family: 'object-fit: cover;';
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: -1;
}

#{{ id }} .parallax-content{
    display: inline-block;
    margin: {{ contentpadding }};
    color: {{ contentcolor }};
    text-align: {{ contentalign }};
	width: 100%;
}
/* endstylesheet */
";

            public const string ParallaxOutput = @"
https://cdnjs.cloudflare.com/ajax/libs/jarallax/1.11.1/jarallax.min.js
https://cdnjs.cloudflare.com/ajax/libs/jarallax/1.11.1/jarallax-video.min.js
<div id='id-uniqueid' class='jarallax' data-jarallax-video=''
        data-type=''
        data-speed='1'
        data-img-position='top'
        data-object-position='top'
        data-background-position='top'
        data-zindex='1'
        data-no-android=''
        data-no-ios=''>

    <div class='parallax-content'>
        <h1>Hello World</h1>
    </div>
</div>

/* stylesheet */
#id-uniqueid {
    /* eventually going to change the height using media queries with mixins using sass, and then include only the classes I want for certain parallaxes */
    min-height: 400px;
    background: transparent;
    position: relative;
    z-index: 0;
}

#id-uniqueid .jarallax-img {
    position: absolute;
    object-fit: cover;
    /* support for plugin https://github.com/bfred-it/object-fit-images */
    font-family: 'object-fit: cover;';
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: -1;
}

#id-uniqueid .parallax-content{
    display: inline-block;
    margin: 20px;
    color: ;
    text-align: ;
	width: 100%;
}
/* endstylesheet */
";

            public const string ParallaxParameters = "image,height,videourl,speed,zindex,position,contentpadding,contentcolor,contentalign,noios,noandroid";
        }
    }
}

