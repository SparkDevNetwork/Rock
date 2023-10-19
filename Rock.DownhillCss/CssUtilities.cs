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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Rock.DownhillCss.Utility;

namespace Rock.DownhillCss
{
    /*
     * FUTURE IDEAS
     * 1. Add settings the DownhillSettings to control whether to include pallete colors in:
     *    + Backgrounds
     *    + Borders
     *    + Tex
     *    + etc.
    */

    /// <summary>
    /// A set of utility methods for building and using Downhill
    /// </summary>
    public static class CssUtilities
    {
        /// <summary>
        /// Builds the base framework.
        /// </summary>
        /// <returns></returns>
        public static string BuildFramework( DownhillSettings settings )
        {
            // Get application color properties by reading the property names from the ApplicationColors class
            PropertyInfo[] applicationColorProperties = typeof( ApplicationColors ).GetProperties();

            StringBuilder frameworkCss = new StringBuilder();

            // Apply Reset & Base CSS First - It is import that this is first so that the utility classes outrank the base CSS
            if (settings.Platform == DownhillPlatform.Mobile)
            {
                frameworkCss.Append( baseStylesMobile );
            }
            else
            {
                frameworkCss.Append( baseStylesWeb );
            }

            // Alerts
            AlertStyles( frameworkCss, settings, applicationColorProperties ); /* somewhat mobile specific now */

            // Text sizes
            TextSizes( frameworkCss, settings, applicationColorProperties );

            // Text colors
            TextColors( frameworkCss, settings, applicationColorProperties );

            // Background colors
            BackgroundColors( frameworkCss, settings, applicationColorProperties );

            // Build border color utilities
            BorderColors( frameworkCss, settings, applicationColorProperties );

            // Build Margin Utilities
            Margins( frameworkCss, settings, applicationColorProperties );

            // Build Padding Utilities
            Paddings( frameworkCss, settings, applicationColorProperties );

            // Build Border Width Utilities
            BorderWidths( frameworkCss, settings, applicationColorProperties ); /* somewhat mobile specific now */

            

            return CssUtilities.ParseCss( frameworkCss.ToString(), settings );
        }

        /// <summary>
        /// Parses custom CSS to replace Downhill variables.
        /// </summary>
        /// <param name="baseStyles">The custom CSS.</param>
        /// <returns></returns>
        public static string ParseCss( string cssStyles, DownhillSettings settings )
        {
            // Variables are prefixed with ? (e.g. ?orange-100) to allow for using pre-processors such as Sass or Less

            // Replace application colors
            PropertyInfo[] applicationColorProperties = typeof( ApplicationColors ).GetProperties();
            foreach ( PropertyInfo colorProperty in applicationColorProperties )
            {
                var value = colorProperty.GetValue( settings.ApplicationColors ).ToString();

                // Replace application color deviations (backgrounds/borders/notification text colors)
                // These are based off of the bootstrap recipes

                // Background
                cssStyles = cssStyles.Replace( $"?color-{colorProperty.Name.ToLower()}-background", MixThemeColor( value, -10 ) );

                // Border
                cssStyles = cssStyles.Replace( $"?color-{colorProperty.Name.ToLower()}-border", MixThemeColor( value, -9 ) );

                // Text
                cssStyles = cssStyles.Replace( $"?color-{colorProperty.Name.ToLower()}-text", MixThemeColor( value, 6 ) );

                var selector = $"?color-{colorProperty.Name.ToLower()}";
                cssStyles = cssStyles.Replace( selector, value );

                // If warning then also make a set for validation
                if ( colorProperty.Name == "Warning" )
                {
                    // Background
                    cssStyles = cssStyles.Replace( $"?color-validation-background", MixThemeColor( value, -10 ) );

                    // Border
                    cssStyles = cssStyles.Replace( $"?color-validation-border", MixThemeColor( value, -9 ) );

                    // Text
                    cssStyles = cssStyles.Replace( $"?color-validation-text", MixThemeColor( value, 6 ) );
                }
            }

            // Replace palette colors
            foreach ( var color in ColorPalette.ColorMaps )
            {
                foreach ( var colorSaturation in color.ColorSaturations )
                {
                    var selector = $"?color-{color.Color.ToLower()}-{colorSaturation.Intensity}";
                    var value = colorSaturation.ColorValue;

                    cssStyles = cssStyles.Replace( selector, value );
                }
            }

            // Run a few other replaces across the CSS
            if ( settings.Platform == DownhillPlatform.Mobile )
            {
                // Most Xamarin.Forms controls only support integer values for border-radius.
                cssStyles = cssStyles.Replace( "?radius-base", ( ( int ) Math.Floor( settings.RadiusBase ) ).ToString() );
            }
            else
            {
                cssStyles = cssStyles.Replace( "?radius-base", settings.RadiusBase.ToString() );
            }
            cssStyles = cssStyles.Replace( "?font-size-default", settings.FontSizeDefault.ToString() );

            // Text and heading colors
            cssStyles = cssStyles.Replace( "?color-text", settings.TextColor );
            cssStyles = cssStyles.Replace( "?color-heading", settings.HeadingColor );
            cssStyles = cssStyles.Replace( "?color-background", settings.BackgroundColor );

            foreach ( var extraCss in settings.AdditionalCssToParse )
            {
                var key = extraCss.Key;
                var value = extraCss.Value;

                cssStyles = cssStyles.Replace( key, value );
            }

            // Note for future... Xamarin Forms doesn't like minified CSS (at least that that's created with the minified method below)
            return cssStyles;
        }

        #region Alerts
        private static void AlertStyles( StringBuilder frameworkCss, DownhillSettings settings, PropertyInfo[] applicationColorProperties )
        {
            frameworkCss.AppendLine( "" );
            frameworkCss.AppendLine( $"/* Alert Styles */" );

            // Base alert styles
            frameworkCss.AppendLine( $".alert {{" );
            frameworkCss.AppendLine( $"    margin: 0 0 12{settings.SpacingUnits} 0;" );
            frameworkCss.AppendLine( "}" );

            // Color specific styles
            foreach ( PropertyInfo colorProperty in applicationColorProperties )
            {
                var colorName = colorProperty.Name.ToLower();
                var colorValue = colorProperty.GetValue( settings.ApplicationColors ).ToString();

                CreateAlertByColor( colorProperty.Name.ToLower(), frameworkCss );
            }

            // Create one more for validation
            CreateAlertByColor( "validation", frameworkCss );
        }

        private static void CreateAlertByColor( string colorName, StringBuilder frameworkCss )
        {
            frameworkCss.AppendLine( $".alert.alert-{colorName} {{" );
            frameworkCss.AppendLine( $"    border-color: ?color-{colorName.ToLower()}-border;" );
            frameworkCss.AppendLine( $"    background-color: ?color-{colorName.ToLower()}-background;" );
            frameworkCss.AppendLine( $"    color: ?color-{colorName.ToLower()}-text;" );
            frameworkCss.AppendLine( "}" );

            frameworkCss.AppendLine( $".alert.alert-{colorName} .alert-heading {{" );
            frameworkCss.AppendLine( $"    color: ?color-{colorName.ToLower()}-text;" );
            frameworkCss.AppendLine( "}" );

            frameworkCss.AppendLine( $".alert.alert-{colorName} .alert-message {{" );
            frameworkCss.AppendLine( $"    color: ?color-{colorName.ToLower()}-text;" );
            frameworkCss.AppendLine( "}" );
        }
        #endregion

        #region Text
        private static void TextSizes( StringBuilder frameworkCss, DownhillSettings settings, PropertyInfo[] applicationColorProperties )
        {
            // Mobile won't be using the text sizes. Instead it will use named sizes
            if (settings.Platform == DownhillPlatform.Mobile)
            {
                return;
            }

            frameworkCss.AppendLine("/*");
            frameworkCss.AppendLine("// Text Size Utilities");
            frameworkCss.AppendLine("*/");

            foreach (var size in settings.FontSizes)
            {
                frameworkCss.AppendLine($".text-{size.Key.ToLower()} {{");
                frameworkCss.AppendLine($"    font-size: {size.Value * settings.FontSizeDefault}{settings.FontUnits};");
                frameworkCss.AppendLine("}");
            }
            
        }

        private static void TextColors( StringBuilder frameworkCss, DownhillSettings settings, PropertyInfo[] applicationColorProperties )
        {
            // Build text colors
            frameworkCss.AppendLine( "/*" );
            frameworkCss.AppendLine( "// Text Color Utilities" );
            frameworkCss.AppendLine( "*/" );

            // Application colors
            frameworkCss.AppendLine( "" );
            frameworkCss.AppendLine( $"/* Application Colors */" );
            foreach ( PropertyInfo colorProperty in applicationColorProperties )
            {
                var colorName = colorProperty.Name.ToLower();
                var colorValue = colorProperty.GetValue( settings.ApplicationColors ).ToString();

                frameworkCss.AppendLine( $".text-{colorName} {{" );
                frameworkCss.AppendLine( $"    color: {colorValue.ToLower()};" );
                frameworkCss.AppendLine( "}" );
            }

            // Palette colors
            foreach ( var color in ColorPalette.ColorMaps )
            {
                frameworkCss.AppendLine( "" );
                frameworkCss.AppendLine( $"/* {color.Color} */" );

                foreach ( var colorSaturation in color.ColorSaturations )
                {
                    frameworkCss.AppendLine( $".text-{color.Color.ToLower()}-{colorSaturation.Intensity} {{" );
                    frameworkCss.AppendLine( $"    color: {colorSaturation.ColorValue.ToLower()};" );
                    frameworkCss.AppendLine( "}" );
                }
            }
        }
        #endregion

        #region Backgrounds
        private static void BackgroundColors( StringBuilder frameworkCss, DownhillSettings settings, PropertyInfo[] applicationColorProperties )
        {
            // Build background color utilities
            frameworkCss.AppendLine( "/*" );
            frameworkCss.AppendLine( "// Background Color Utilities" );
            frameworkCss.AppendLine( "*/" );

            // Application colors
            frameworkCss.AppendLine( "" );
            frameworkCss.AppendLine( $"/* Application Colors */" );
            foreach ( PropertyInfo colorProperty in applicationColorProperties )
            {
                var colorName = colorProperty.Name.ToLower();
                var colorValue = colorProperty.GetValue( settings.ApplicationColors ).ToString();

                frameworkCss.AppendLine( $".bg-{colorName} {{" );
                frameworkCss.AppendLine( $"    background-color: {colorValue.ToLower()};" );
                frameworkCss.AppendLine( "}" );
            }

            // Palette colors
            foreach ( var color in ColorPalette.ColorMaps )
            {
                frameworkCss.AppendLine( "" );
                frameworkCss.AppendLine( $"/* {color.Color} */" );

                foreach ( var colorSaturation in color.ColorSaturations )
                {
                    frameworkCss.AppendLine( $".bg-{color.Color.ToLower()}-{colorSaturation.Intensity} {{" );
                    frameworkCss.AppendLine( $"    background-color: {colorSaturation.ColorValue.ToLower()};" );
                    frameworkCss.AppendLine( "}" );
                }
            }
        }
        #endregion

        #region Spacing - Margins and Padding
        private static void Margins( StringBuilder frameworkCss, DownhillSettings settings, PropertyInfo[] applicationColorProperties )
        {
            var spacingValues = settings.SpacingValues;

            frameworkCss.AppendLine( "" );
            frameworkCss.AppendLine( "/*" );
            frameworkCss.AppendLine( "// Margin Utilities" );
            frameworkCss.AppendLine( "*/" );

            // m- (all)
            foreach( var value in spacingValues )
            {
                frameworkCss.AppendLine( $".m-{value.Key} {{" );
                frameworkCss.AppendLine( $"    margin: {value.Value};" );
                frameworkCss.AppendLine( "}" );
            }

            // mt- (top)
            foreach ( var value in spacingValues )
            {
                frameworkCss.AppendLine( $".mt-{value.Key} {{" );
                frameworkCss.AppendLine( $"    margin-top: {value.Value};" );
                frameworkCss.AppendLine( "}" );
            }

            // mb- (bottom)
            foreach ( var value in spacingValues )
            {
                frameworkCss.AppendLine( $".mb-{value.Key} {{" );
                frameworkCss.AppendLine( $"    margin-bottom: {value.Value};" );
                frameworkCss.AppendLine( "}" );
            }

            // ml- (left)
            foreach ( var value in spacingValues )
            {
                frameworkCss.AppendLine( $".ml-{value.Key} {{" );
                frameworkCss.AppendLine( $"    margin-left: {value.Value};" );
                frameworkCss.AppendLine( "}" );
            }

            // mr- (right)
            foreach ( var value in spacingValues )
            {
                frameworkCss.AppendLine( $".mr-{value.Key} {{" );
                frameworkCss.AppendLine( $"    margin-right: {value.Value};" );
                frameworkCss.AppendLine( "}" );
            }

            // mx- (left and right)
            foreach ( var value in spacingValues )
            {
                frameworkCss.AppendLine( $".mx-{value.Key} {{" );
                frameworkCss.AppendLine( $"    margin-right: {value.Value};" );
                frameworkCss.AppendLine( $"    margin-left: {value.Value};" );
                frameworkCss.AppendLine( "}" );
            }

            // my- (top and bottom)
            foreach ( var value in spacingValues )
            {
                frameworkCss.AppendLine( $".my-{value.Key} {{" );
                frameworkCss.AppendLine( $"    margin-top: {value.Value};" );
                frameworkCss.AppendLine( $"    margin-bottom: {value.Value};" );
                frameworkCss.AppendLine( "}" );
            }
        }

        private static void Paddings( StringBuilder frameworkCss, DownhillSettings settings, PropertyInfo[] applicationColorProperties )
        {
            var spacingValues = settings.SpacingValues;

            frameworkCss.AppendLine( "" );
            frameworkCss.AppendLine( "/*" );
            frameworkCss.AppendLine( "// Padding Utilities" );
            frameworkCss.AppendLine( "*/" );

            // p- (all)
            foreach ( var value in spacingValues )
            {
                frameworkCss.AppendLine( $".p-{value.Key} {{" );
                frameworkCss.AppendLine( $"    padding: {value.Value};" );
                frameworkCss.AppendLine( "}" );
            }

            // pt- (top)
            foreach ( var value in spacingValues )
            {
                frameworkCss.AppendLine( $".pt-{value.Key} {{" );
                frameworkCss.AppendLine( $"    padding-top: {value.Value};" );
                frameworkCss.AppendLine( "}" );
            }

            // pb- (bottom)
            foreach ( var value in spacingValues )
            {
                frameworkCss.AppendLine( $".pb-{value.Key} {{" );
                frameworkCss.AppendLine( $"    padding-bottom: {value.Value};" );
                frameworkCss.AppendLine( "}" );
            }

            // pl- (left)
            foreach ( var value in spacingValues )
            {
                frameworkCss.AppendLine( $".pl-{value.Key} {{" );
                frameworkCss.AppendLine( $"    padding-left: {value.Value};" );
                frameworkCss.AppendLine( "}" );
            }

            // pr- (right)
            foreach ( var value in spacingValues )
            {
                frameworkCss.AppendLine( $".pr-{value.Key} {{" );
                frameworkCss.AppendLine( $"    padding-right: {value.Value};" );
                frameworkCss.AppendLine( "}" );
            }

            // px- (left and right)
            foreach ( var value in spacingValues )
            {
                frameworkCss.AppendLine( $".px-{value.Key} {{" );
                frameworkCss.AppendLine( $"    padding-left: {value.Value};" );
                frameworkCss.AppendLine( $"    padding-right: {value.Value};" );
                frameworkCss.AppendLine( "}" );
            }

            // py- (top and bottom)
            foreach ( var value in spacingValues )
            {
                frameworkCss.AppendLine( $".py-{value.Key} {{" );
                frameworkCss.AppendLine( $"    padding-top: {value.Value};" );
                frameworkCss.AppendLine( $"    padding-bottom: {value.Value};" );
                frameworkCss.AppendLine( "}" );
            }
        }
        #endregion

        #region Borders
        private static void BorderWidths( StringBuilder frameworkCss, DownhillSettings settings, PropertyInfo[] applicationColorProperties )
        {
            var borderWidths = settings.BorderWidths;

            int borderWidthCount = borderWidths.Count;

            frameworkCss.AppendLine( "" );
            frameworkCss.AppendLine( "/*" );
            frameworkCss.AppendLine( "// Border Widths" );
            frameworkCss.AppendLine( "*/" );

            for ( int i = 0; i < borderWidthCount; i++ )
            {
                if ( borderWidths[i] == 1 )
                {
                    // When the unit is 1 then we drop the unit from the class name .border (not .border-1)
                    // This is the pattern of both Bootstrap and Tailwind
                    // Tailwind and Bootstrap deviate here from t vs top. Bootstrap uses the full 'top' but since
                    // Tailwind uses t AND Bootstrap used t for margins and padding, decided to use just t.

                    // Xamarin Forms 4.0 does not allow for separate widths on borders
                    // https://github.com/xamarin/Xamarin.Forms/blob/4.3.0/Xamarin.Forms.Core/Properties/AssemblyInfo.cs

                    // border (all)
                    frameworkCss.AppendLine( $".border {{" );
                    frameworkCss.AppendLine( $"    border-width: {borderWidths[i]}{settings.BorderUnits};" );
                    frameworkCss.AppendLine( "}" );
                }
                else
                {
                    // border- (all)
                    frameworkCss.AppendLine( $".border-{i} {{" );
                    frameworkCss.AppendLine( $"    border-width: {borderWidths[i]}{settings.BorderUnits};" );
                    frameworkCss.AppendLine( "}" );
                }
            }
        }

        private static void BorderColors( StringBuilder frameworkCss, DownhillSettings settings, PropertyInfo[] applicationColorProperties )
        {
            frameworkCss.AppendLine( "/*" );
            frameworkCss.AppendLine( "// Border Color Utilities" );
            frameworkCss.AppendLine( "*/" );

            // Application colors
            frameworkCss.AppendLine( "" );
            frameworkCss.AppendLine( $"/* Application Colors */" );
            foreach ( PropertyInfo colorProperty in applicationColorProperties )
            {
                var colorName = colorProperty.Name.ToLower();
                var colorValue = colorProperty.GetValue( settings.ApplicationColors ).ToString();

                frameworkCss.AppendLine( $".border-{colorName} {{" );
                frameworkCss.AppendLine( $"    border-color: {colorValue.ToLower()};" );
                frameworkCss.AppendLine( "}" );
            }

            // Palette colors
            foreach ( var color in ColorPalette.ColorMaps )
            {
                frameworkCss.AppendLine( "" );
                frameworkCss.AppendLine( $"/* {color.Color} */" );

                foreach ( var colorSaturation in color.ColorSaturations )
                {
                    frameworkCss.AppendLine( $".border-{color.Color.ToLower()}-{colorSaturation.Intensity} {{" );
                    frameworkCss.AppendLine( $"    border-color: {colorSaturation.ColorValue.ToLower()};" );
                    frameworkCss.AppendLine( "}" );
                }
            }
        }
        #endregion

        #region Private Helpers

        /// <summary>
        /// Mixes the color of the theme.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        private static string MixThemeColor( string color, decimal level )
        {
            var mixcolor = "#ffffff";
            if ( level > 0 )
            {
                mixcolor = "#000000";
            }

            var originalColor = new RockColor( color );
            
            var mixColor = RockColor.FromHex( mixcolor );

            var mixPercent = ( int ) ( ( Math.Abs( level ) * .08m ) * 100 );

            originalColor.Mix( mixColor, mixPercent );

            if( originalColor.Alpha < 1 )
            {
                return originalColor.ToRGBA();
            }

            return originalColor.ToHex();
        }

        /// <summary>
        /// Minifies the CSS.
        /// </summary>
        /// <param name="css">The CSS.</param>
        /// <returns></returns>
        private static string MinifyCss( string css )
        {
            css = Regex.Replace( css, @"[a-zA-Z]+#", "#" );
            css = Regex.Replace( css, @"[\n\r]+\s*", string.Empty );
            css = Regex.Replace( css, @"\s+", " " );
            css = Regex.Replace( css, @"\s?([:,;{}])\s?", "$1" );
            css = css.Replace( ";}", "}" );
            css = Regex.Replace( css, @"([\s:]0)(px|pt|%|em)", "$1" );

            // Remove comments from CSS
            css = Regex.Replace( css, @"/\*[\d\D]*?\*/", string.Empty );

            return css;
         
        }
        #endregion

        #region Platform Base Styles
        private static string baseStylesWeb = @"";

        private static string baseStylesMobile = @"/*
Resets
-----------------------------------------------------------
*/

/* Fixes frame backgrounds from being black while in dark mode */

^editor {
    background-color: transparent;
    color: ?color-text;
    margin: -5, -10;
}

^frame {
    background-color: transparent;
}

^radiobutton {
    background-color: transparent;
}

^Page {
    -rock-status-bar-text: light;
}

^contentpage {
    background-color: ?color-background;
}

^label {
    font-size: ?font-size-default;
    color: ?color-text;
}

icon {
    color: ?color-text;
}

/*
    Utility Classes
    -----------------------------------------------------------
*/

.list-item {
    padding-bottom: 12;
}

.h1 {
    color: ?color-heading;
    font-style: bold;
    font-size: 34;
    margin-bottom: 0;
    line-height: 1;
}

.h2 {
    color: ?color-heading;
    font-style: bold;
    font-size: 28;
    line-height: 1;
}

.h3 {
    color: ?color-heading;
    font-style: bold;
    font-size: 22;
    line-height: 1.05;
}

.h4 {
    color: ?color-heading;
    font-style: bold;
    font-size: 16;
    line-height: 1.1;
}

.h5, .h6 {
    color: ?color-heading;
    font-style: bold;
    font-size: 13;
    line-height: 1.25;
}

.link{
    color: ?color-primary;
}

/* Class for styling code that matches Gitbook */
.code {
    background-color: #183055;
    color: #e6ecf1;
    padding: 16;
    font-size: 12;
}

/* Text Weights */
.font-weight-bold {
    font-style: bold;
}

.font-italic {
    font-style: italic;
}

/* Visibility Classes */
.visible {
    visibility: visible;
}

.invisible {
    visibility: hidden;
}

.collapse {
    visibility: collapse;
}

/* Text Named Sizes */
.text {
    font-size: ?font-size-default;
    color: ?color-text;
}

.text-xs {
    font-size: micro;
}

.text-sm {
    font-size: small;
}

.text-md {
    font-size: medium;
}

.text-lg {
    font-size: large;
}

.text-title {
    font-size: title;
    color: ?color-text;
}

.text-subtitle {
    font-size: subtitle;
}

.text-caption {
    font-size: caption;
}

.text-body {
    font-size: body;
}

.title {
    font-style: bold;
    font-size: ?font-size-default;
    line-height: 1;
}

/* Body Styles */
.paragraph {
    font-size: ?font-size-default;
    color: ?color-text;
    line-height: 1.15;
    margin-bottom: 24;
}

.paragraph-sm {
    font-size: small;
    color: ?color-text;
    line-height: 1.25;
    margin-bottom: 12;
}

.paragraph-xs {
    font-size: micro;
    color: ?color-text;
    line-height: 1.25;
    margin-bottom: 8;
}

.paragraph-lg {
    font-size: large;
    color: ?color-text;
    line-height: 1;
    margin-bottom: 16;
}

/* Text Decoration */
.text-underline {
    text-decoration: underline;
}

.text-strikethrough {
    text-decoration: strikethrough;
}

.text-linethrough {
    text-decoration: line-through;
}

/* Opacity */
.o-00, .o-0 {
    opacity: 0;
}

.o-10 {
    opacity: .1;
}

.o-20 {
    opacity: .2;
}

.o-30 {
    opacity: .3;
}

.o-40 {
    opacity: .4;
}

.o-50 {
    opacity: .5;
}

.o-60 {
    opacity: .6;
}

.o-70 {
    opacity: .7;
}

.o-80 {
    opacity: .8;
}

.o-90 {
    opacity: .9;
}

/* Leading */
.leading-none {
    line-height: 1;
}

.leading-tight {
    line-height: 1.1;
}

.leading-snug {
    line-height: 1.2;
}

.leading-normal {
    line-height: 1.25;
}

.leading-relaxed {
    line-height: 1.4;
}

.leading-loose {
    line-height: 1.6;
}

/* Text Alignment */
.text-center {
    text-align: center;
}

.text-right {
    text-align: right;
}

.text-left {
    text-align: left;
}

.text-start {
    text-align: start;
}

.text-end {
    text-align: end;
}

/* Border Radius */
.rounded-none {
    border-radius: 0;
}

.rounded-sm {
    border-radius: 4;
}

.rounded {
    border-radius: 8;
}

.rounded-lg {
    border-radius: 16;
}

.rounded-full {
    border-radius: 1000;
}

/*
    Control CSS
    -----------------------------------------------------------
*/
/* MobileInsertMark - Used by Mobile Shell to insert it's own standard control CSS */

/* Flyout Styling */

.flyout-menu ^listview {
    background-color: ?color-brand;
}

.flyout-menu ^boxview {
    background-color: #fff;
    opacity: 0.4;
}

.flyout-menu-item {
    font-size: 21;
}

/* Countdown */
.countdown-field {
    width: 32;
}

.countdown-field-value,
.countdown-separator-value,
.countdown-complete-message {
    font-size: 24;
    font-style: bold;
    color: ?color-text;
}

.countdown-separator-value {
    color: ?color-gray-500;
}

.countdown-field-label {
    font-size: 12;
    color: ?color-gray-500;
}

.countdown-complete .countdown-field-value,
.countdown-complete .countdown-separator-value {}

.less-than-day .countdown-field-value {}
.less-than-hour {}
.less-than-15-min {}
.less-than-5-min{}

/* Modal */
.modal {
    background-color: #ffffff;
    padding: 0;
    margin: 48 16;
    border-radius: 8;
}

.modal-anchor-top {
    margin: 0 0 48 0;
    corner-radius: 0 8;
}

.modal-anchor-bottom {
    margin: 48 0 0 0;
    corner-radius: 8 0;
}

.modal-header {
    background-color: ?color-gray-400;
    padding: 16;
}

.modal-body {
    padding: 16;
    background-color: #ffffff;
}

.modal-anchor-top .modal-body {
    padding-top: 32;
}

.modal-anchor-bottom .modal-body {
    padding-bottom: 32;
}

.modal-close,
.modal-title {
    color: #ffffff; 
}

.modal-title {
    line-height: 0;
    margin: 0;
    padding: 0;
}

.modal-close {
    opacity: 0.5;
}


/* Divider */
.divider {
    background-color: ?color-gray-200;
    height: 1;
}

.divider-thick {
    height: 2;
}

.divider-thicker {
    height: 4;
}

.divider-thickest {
    height: 8;
}

/* Buttons */
.btn {
    border-radius: ?radius-base;
    padding: 14 16;
}

.btn.btn-primary {
    background-color: ?color-primary;
    color: #ffffff;
}

.btn.btn-success {
    background-color: ?color-success;
    color: #ffffff;
}

.btn.btn-info {
    background-color: ?color-info;
    color: #ffffff;
}

.btn.btn-warning {
    background-color: ?color-warning;
    color: #ffffff;
}

.btn.btn-danger {
    background-color: ?color-danger;
    color: #ffffff;
}

.btn.btn-dark {
    color: #ffffff;
    background-color: ?color-dark;
}

.btn.btn-light {
    color: ?color-text;
    background-color: ?color-light;
}

.btn.btn-secondary {
    color: #ffffff;
    background-color: ?color-secondary;
}

.btn.btn-brand {
    color: #ffffff;
    background-color: ?color-brand;
}

.btn.btn-default {
    color: ?color-primary;
    border-color: ?color-primary;
    border-width: 1;
    background-color: transparent;
}

.btn.btn-link {
    color: ?color-primary;
    border-width: 0;
    background-color: transparent;
}

.btn.btn-link-secondary {
    color: ?color-secondary;
    border-width: 0;
    background-color: transparent;
}

.btn.btn-link-success {
    color: ?color-success;
    border-width: 0;
    background-color: transparent;
}

.btn.btn-link-danger {
    color: ?color-danger;
    border-width: 0;
    background-color: transparent;
}

.btn.btn-link-warning {
    color: ?color-warning;
    border-width: 0;
    background-color: transparent;
}

.btn.btn-link-info {
    color: ?color-info;
    border-width: 0;
    background-color: transparent;
}

.btn.btn-link-light {
    color: ?color-text;
    border-width: 0;
    background-color: transparent;
}

.btn.btn-link-dark {
    color: ?color-dark;
    border-width: 0;
    background-color: transparent;
}

.btn.btn-link-brand {
    color: ?color-brand;
    border-width: 0;
    background-color: transparent;
}

.btn.btn-outline-primary {
    color: ?color-primary;
    border-color: ?color-primary;
    border-width: 1;
    background-color: transparent;
}

.btn.btn-outline-secondary {
    color: ?color-secondary;
    border-color: ?color-secondary;
    border-width: 1;
    background-color: transparent;
}

.btn.btn-outline-success {
    color: ?color-success;
    border-color: ?color-success;
    border-width: 1;
    background-color: transparent;
}

.btn.btn-outline-danger {
    color: ?color-danger;
    border-color: ?color-danger;
    border-width: 1;
    background-color: transparent;
}

.btn.btn-outline-warning {
    color: ?color-warning;
    border-color: ?color-warning;
    border-width: 1;
    background-color: transparent;
}

.btn.btn-outline-info {
    color: ?color-info;
    border-color: ?color-info;
    border-width: 1;
    background-color: transparent;
}

.btn.btn-outline-light {
    color: ?color-text;
    border-color: ?color-light;
    border-width: 1;
    background-color: transparent;
}

.btn.btn-outline-dark {
    color: ?color-dark;
    border-color: ?color-dark;
    border-width: 1;
    background-color: transparent;
}

.btn.btn-outline-brand {
    color: ?color-brand;
    border-color: ?color-brand;
    border-width: 1;
    background-color: transparent;
}


/* Button Sizes */
.btn.btn-lg {
    font-size: large;
    padding: 20;
}

/* Once clients have updated to mobile v2 'height: 35' needs to be replaced with 'padding: 11 12'; */
.btn.btn-sm {
    font-size: micro;
    height: 35;
}

/* Toggle Button CSS */
.toggle-button {
    border-radius: 0;
    border-color: ?color-primary;
    background-color: transparent;
    padding: 9 12 12 12;
}

.toggle-button .title {
    color: ?color-primary;
    font-size: large;
}

.toggle-button .append-text {
    color: ?color-primary;
    font-size: small;
}

.toggle-button .icon {
    margin: 3 0 0 0;
    color: ?color-primary;
    font-size: large;
}

.toggle-button.checked {
    background-color: ?color-primary;
}

.toggle-button.checked .title {
    color: white;
}

.toggle-button.checked .append-text {
    color: white;
}

.toggle-button.checked .icon {
    color: white;
}

/*
    Block CSS
    -----------------------------------------------------------
*/

/* Note Editor */
.noteeditor {
    border-color: ?color-text;
    padding: 8;
    background-color: ?color-gray-100;
    margin-top: 12;
    margin-bottom: 12;
}

.noteeditor ^texteditor {
    min-height: 100;
    color: ?color-text;
    margin: 0;
    font-size: small;
}

.noteeditor-label {
    font-size: 11;
}

/* Hero Block */
.hero .hero-title {
    font-size: 24;
    color: white;
    -rock-text-shadow: 2 2 4 black;
}

.hero .hero-subtitle {
    font-size: 18;
    color: white;
    -rock-text-shadow: 2 2 4 black;
}

.tablet .hero .hero-title {
    font-size: 36;
}

.tablet .hero .hero-subtitle {
    font-size: 28;
}


/* My Prayer Requests */
.block-my-prayer-requests .prayer-request-list {
    -xf-spacing: 40;
}

.block-my-prayer-requests .prayer-header {
    margin-bottom: 20;
}

.block-my-prayer-requests .actions {
    margin-top: 20;
}

.block-my-prayer-requests .answer-header {
    font-size: 14;
    font-style: bold;
    margin-top: 8;
}

.block-my-prayer-requests .answer-text {
    font-size: 14;
}


/* Answer To Prayer */
.block-answer-to-prayer .prayer-header {
    margin-bottom: 20;
}

.block-answer-to-prayer .save-button {
    margin-top: 20;
}


/* Calendar Classes */
.calendar-filter-panel {
    margin-bottom: 5;
}

.calendar-filter {
    padding: 8;
    border-radius: ?radius-base;
    background-color: ?color-gray-200;
}

.calendar-filter label,
.calendar-filter icon {
    color: ?color-text;
    vertical-align: center;
}
.calendar-filter icon {
    font-size: small;
    margin-right: 6;
}

.calendar-toolbar {
    padding: 12 16;
    border-radius: ?radius-base;
    background-color: ?color-primary;
    margin-top: 0;
}
.calendar-toolbar .calendar-toolbar-currentmonth {
    font-style: bold;
    color: #ffffff;
}
.calendar-toolbar .calendar-toolbar-adjacentmonth {
    color: rgba(255,255,255,0.5);
}

.calendar-monthcalendar {
    margin-bottom: 32;
}

.calendar-header {
    font-style: bold;
}

.calendar-day {
    background-color: initial;
}
.calendar-day-current {
    background-color: ?color-gray-200;
}
.calendar-day-current .calendar-day-title {
    color: ?color-text;
}

.calendar-day-adjacent .calendar-day-title {
    color: ?color-gray-400;
}

.calendar-events-heading {
    margin-top: 0;
    text-align: center;
    margin-bottom: 16;
}

.calendar-events-day {
    color: ?color-heading;
    font-style: bold;
    font-size: 16;
    line-height: 1.1;
    margin-bottom: 0;
}

.calendar-event {
    padding: 12;
    border-radius: ?radius-base;
    background-color: ?color-gray-200;
    margin-bottom: 24;
}

.calendar-event-summary {
    padding: 0;
    background-color: ?color-gray-200;
}

.calendar-event-title {
    font-style: bold;
    font-size: 16;
}

.calendar-event-text {
    font-size: small;
}

.calendar-event-audience,
.calendar-event-campus {
    font-size: small;
    color: #888888;
}

.calendar-list-navigation {
    margin-bottom: 16;
}

.previous-month,
.next-month {
    padding: 4 12 0;
    font-size: 24;
    opacity: 0.8;
}

.next-month {
    padding-right: 0;
}

/* Forms Styles */

.form-group {
    margin: 0 0 12 0;
}

.form-group .form-group-title {
    margin: 0 0 5 0;
    color: ?color-primary;
    font-size: 12;
}

.form-field {
    padding: 12;
    color: #282828;
}
^borderlessentry,
^datepicker,
^checkbox, 
^picker,
^entry, 
^switch,
^editor {
    color: ?color-text;
    font-size: ?font-size-default;
}

^literal {
    line-height: 1.15;
    margin-bottom: 16;
}

^editor {
    margin: -5, -10;
}

/* Field Titles */
fieldgroupheader {
   
}

fieldgroupheader .title,
formfield .title {
    color: ?color-text;
    font-style: bold;
    font-size: ?font-size-default;
}

fieldgroupheader.error .title,
formfield.error .title {
    color: ?color-danger;
}

formfield .title {
    margin-right: 12;
    line-height: 1;
}

/* Required Indicator */
fieldgroupheader .required-indicator,
formfield .required-indicator {
    color: transparent;
    width: 4;
    height: 4;
    border-radius: 2;
}

fieldgroupheader.required .required-indicator,
formfield.required .required-indicator {
    color: ?color-danger;
}

/* Field Stacks */
^fieldstack {
    border-radius: 0;
    border-color: ?color-secondary;
    border-width: 1;
    margin-top: 4;
    margin-bottom: 12;
}

/* Form Fields  */
formfield {
    padding: 12 12 12 6;
}

fieldcontainer > .no-fieldstack {
    margin-bottom: 12;
}

formfield .required-indicator {
    margin-right: 4;
}

/* Cards */
.card {
    margin-bottom: 24;
}

.card-container {
   padding: 0; 
   -xf-spacing: 0;
}

.card-content {
    -xf-spacing: 0;
}

.card-inline .card-content,
.card-contained .card-content {
    padding: 16;
}

.card-block .card-content {
    padding-top: 16;
}

.card-image {
    margin: 0;
}

.card-tagline {
    font-style: normal;
    font-size: 14;
}

.card-title {
    margin: 0;
}

.card-descriptions {
    margin-bottom: 8;
}

.card-tagline,
.card-description-left, 
.card-description-right {
    opacity: .7;
}


.card-additionalcontent .paragraph {
    margin-bottom: 0;
    margin-top: 12;
}

.card-inline .card-tagline,
.card-inline .card-title,
.card-inline .card-description-left,
.card-inline .card-description-right,
.card-inline .card-additionalcontent .paragraph {
    color: #ffffff;
}

/* HTML Parser CSS */
^grid.ordered-list,
^grid.unordered-list {
    margin-bottom: 24;
}

^grid.ordered-list ^grid.ordered-list,
^grid.ordered-list ^grid.unordered-list,
^grid.unordered-list ^grid.ordered-list,
^grid.unordered-list ^grid.unordered-list {
    margin-bottom: 0;
}


/* RadioButton */
^radiobutton {
  color: ?color-text;
}


/*** Prayer Card Block ***/
.prayer-card-container {
    border-color: #a6a6a6;
    border-radius: 0;
    padding: 12 24;
    margin: 0 0 18 0;
}

.prayer-card-container .prayer-card-name {
    font-size: ?shell-font-scale(24);
    font-style: bold;
    color: #1d1d1d;
}

.prayer-card-container .prayer-card-category {
    background-color: #009ce3;
    padding: 5;
}
    .prayer-card-container .prayer-card-category Label {
        font-size: ?shell-font-scale(12);
        color: #ffffff;
    }

.prayer-card-container .prayer-card-text {
    margin: 12 0 0 0;
}


/*** Group Finder Block ***/
.group-finder-container {
    -xf-spacing: 0;
}

.group-finder-container .group-finder-search-button {
    margin: 0 0 30 0;
}

.group-finder-container .group-finder-filter-button {
    padding: 4 12;
    background-color: #f5f5f5;
    color: #767676;
    border-color: #b9b9b9;
    border-width: 1;
    border-radius: 4;
}

.group-finder-container .group-finder-filter-button.active {
    background-color: #007acc;
    color: #fff;
}

.group-finder-container .group-primary-content {
    -xf-spacing: 3;
}

.group-finder-container .group-meeting-day {
    color: #007aff;
}

.group-finder-container .group-name {
    font-size: ?shell-font-scale(24);
    font-style: bold;
    color: #1d1d1d;
}

.group-finder-container .group-meeting-time {
    color: #999999;
}

.group-finder-container .group-topic {
    color: #999999;
}

.group-finder-container .group-distance {
    color: #999999;
}

.group-finder-container .group-more-icon {
    color: #999999;
}


/*** Connection Type List block ***/
.connection-type-list-layout .connection-type {
    border-color: #e7e7e7;
    border-radius: ?radius-base;
    padding: 12;
    margin: 0, 0, 0, 12;
}

.connection-type-list-layout .connection-type-icon {
    font-size: 36;
    margin: 0, 0, 10, 0;
}

.connection-type-list-layout .connection-type-name {
    font-style: bold;
}




/*** Connection Opportunity List block ***/
.connection-opportunity-list-layout .connection-opportunities {
    margin: 0, 12, 0, 0;
}

.connection-opportunity-list-layout .connection-opportunity {
    border-color: #e7e7e7;
    border-radius: ?radius-base;
    padding: 12;
    margin: 0, 0, 0, 12;
}

.connection-opportunity-list-layout .connection-opportunity-icon {
    font-size: 36;
    margin: 0, 0, 10, 0;
}

.connection-opportunity-list-layout .connection-opportunity-name {
    font-style: bold;
}

.connection-opportunity-list-layout .filter-button {
    padding: 4 12;
    background-color: #f5f5f5;
    color: #767676;
    border-color: #b9b9b9;
    border-width: 1;
    border-radius: 4;
}

.connection-opportunity-list-layout .filter-button.active {
    background-color: #007acc;
    color: #fff;
}


/*** Connection Request List block ***/
.connection-request-list-layout .connection-requests {
    margin: 0, 12, 0, 0;
}

.connection-request-list-layout .connection-request {
    border-color: #e7e7e7;
    border-radius: ?radius-base;
    padding: 12;
    margin: 0, 0, 0, 12;
}

.connection-request-list-layout .connection-request-image {
    width: 35;
    margin: 0, 0, 10, 0;
}

.connection-request-list-layout .connection-request-name {
    font-style: bold;
}

.connection-request-list-layout .connection-request-date {
    font-size: ?shell-font-scale(12);
}

.connection-request-list-layout .filter-button {
    padding: 4 12;
    background-color: #f5f5f5;
    color: #767676;
    border-color: #b9b9b9;
    border-width: 1;
    border-radius: 4;
}

.connection-request-list-layout .filter-button.active {
    background-color: #007acc;
    color: #fff;
}

/*** Connection Request Detail Block ***/
.connection-request-detail-content {
    -xf-spacing: 0;
}

.connection-request-detail-content .status-pill-layout {
    margin: 0, 0, 0, 20;
}

.connection-request-detail-content .status-pill-layout ^tag {
    margin: 0, 0, 6, 0;
}

.connection-request-detail-content .person-photo {
    width: 80;
    height: 80;
    margin: 0, 0, 12, 0;
}

.connection-request-detail-content .person-name {
    font-style: bold;
    font-size: 22;
}

.connection-request-detail-content .person-detail {
    margin: 0, 0, 0, 20;
}

.connection-request-detail-content .person-contact-buttons {
    margin: 0, 8, 0, 0;
}

^ContactButton.contact-button,
^VerticalIconButton.contact-button {
  padding: 4;
  width: 44;
  height: 32;
  background-color: #e3e3e3;
  border-radius: 6;
}

^ContactButton .contact-button-icon,
^VerticalIconButton .contact-button-icon {
    font-size: 14;
    color: ?color-primary;
}

^ContactButton .contact-button-text,
^VerticalIconButton .contact-button-text {
    font-size: 11;
    color: ?color-primary;
}

.contact-button.is-followed {
  background-color: ?color-primary;
}

^VerticalIconButton.contact-button.is-followed .contact-button-text,
^VerticalIconButton.contact-button.is-followed .contact-button-icon {
  color: white;
}

^VerticalIconButton.contact-button-disabled {
    opacity: 0.4;
}

^VerticalIconButton.contact-button-enabled {
    opacity: 1.0;
}

^VerticalIconButton.contact-button-disabled .contact-button-icon,
^VerticalIconButton.contact-button-disabled .contact-button-text {
  color: ?color-primary-100;
}

.connection-request-detail-content .request-details {
    margin: 0, 0, 0, 0;
}

.connection-request-detail-content .request-attributes {
}

.connection-request-detail-content .workflow-actions {
    margin: 0, 0, 0, 4;
}

.connection-request-detail-content .workflow-action-button {
    font-size: 12;
    color: ?color-text;
    padding: 12, 0, 12, 0;
    margin: 0, 0, 8, 8;
    height: 24;
    border-width: 1;
    border-radius: 12;
    border-color: #999999;
    background-color: transparent;
}

.connection-request-detail-content .group-requirements {
    margin: 0, 0, 0, 0;
}

.connection-request-detail-content .group-manual-requirement {
    margin: 0, 0, 0, 8;
}

.connection-request-detail-content .request-activities > ^Divider {
    margin: 0, 12, 0, 0;
}

.connection-request-detail-content .request-activities > .title {
    margin: 0, 6, 0, 6;
}

.connection-request-detail-content .request-activity {
    border-color: #e7e7e7;
    border-radius: ?radius-base;
    padding: 12;
    margin: 0, 0, 0, 12;
}

.connection-request-detail-content .request-activity.related-activity {
    background-color: #e0e0e0;
    opacity: 0.7;
}

.connection-request-detail-content .activity-image {
    width: 35;
}

.connection-request-detail-content .activity-connector-name {
    font-style: bold;
}

.connection-request-detail-content .activity-date {
    font-size: ?shell-font-scale(12);
}


/*** Group Member Edit block ***/
.group-member-edit-layout .save-button {
    margin: 0, 24, 0, 0;
}


/*** Search block ***/
.search-layout .search-field .search-button {
    margin: 0, 4, 0, 12;
    padding: 24, 0, 24, 0;
}

.search-layout .search-results {
    margin: 0, 12, 0, 0;
}

.search-layout .search-result-content {
    padding: 0, 8, 0, 8;
}

.search-layout .search-result-image {
    width: 35;
    margin: 0, 0, 10, 0;
}

.search-layout .search-result-name {
    font-style: bold;
}

.search-layout .search-result-text {
    font-size: ?shell-font-scale(14);
}

.search-layout .search-result-detail-arrow {
    color: #a5a5a5;
    font-size: ?shell-font-scale(20);
    margin: 10, 0, 0, 0;
}

.search-layout .show-more-button {
    margin: 0, 12, 0, 0;
}

/*** Group Schedule Toolbox Block ***/

.schedule-toolbox-container .detail-title
{
    font-size: ?shell-font-scale(18);
    font-style: bold;
}

.schedule-toolbox-confirmations-container .confirmed-text {
    color: green;
}

.schedule-toolbox-confirmations-container .declined-text {
    color: red;
}

.schedule-preference-container .preferences-container {
    padding: 16;
}

.schedule-preference-container .assignments-container {
    padding: 16;
}

.schedule-signup .field-container {
    padding: 16;
}

.schedule-signup .signups-container {
    padding: 12;
}

/*** Communication Entry Block ***/
.block-communication-entry .communication-entry-layout {
    spacing: 0;
}

.block-communication-entry .recipients-container {
    background-color: white;
}

/* Recipient View */
.recipient-container {
    padding: 8, 0;
    -xf-spacing: 8;
}

.recipients-layout {
    -xf-spacing: 0;
}

.block-communication-entry .recipients-icon {
    color: ?color-primary;
    font-size: 18;
}

.recipient-container .recipient-image {
    height: 50;
    width: 50;
}

.recipient-name-and-communication {
    -xf-spacing: 0;
}

.recipient-container .swipe-to-remove-detail {
    padding: 8;
}

.block-communication-entry .recipient-name {
    font-size: 17;
    font-style: bold;
}

.block-communication-entry .success-layout {
    -xf-spacing: 16;
}

.block-communication-entry .swipe-to-remove-detail {
    padding: 8;
}

.block-communication-entry .failed-recipients-layout {
    -xf-spacing: 0;
}

.block-communication-entry .failed-recipient-item {
    height: 30;
}
/*** Reminder Blocks ***/
.reminder-type-item-layout {
  -xf-spacing:0;
}

.reminder-type-item {
  padding: 8;
}

.reminder-type-item .reminder-type-info-layout {
  -xf-spacing: 0;
}

.reminder-type-item .reminder-type-count-label-and-icon-layout {
  -xf-spacing: 8;
}

.reminder-type-item .reminder-icon-frame {
  width: 48;
  height: 48;
  border-radius: 24;
  padding: 0;
}

.reminder-type-item .reminder-icon-frame ^Icon {
  color: white;
  font-size: 22;
}

.filtered-reminder-card {
  padding: 16;
  border-color: #c2c1be;
  border-width: 1;
  border-radius: 8;
}

.filtered-reminder-card ^stacklayout {
  -xf-spacing: 0;
}

.filtered-reminder-card .icon-frame {
  width: 64;
  height: 64;
  border-radius: 32;
  padding: 0;
}

.filtered-reminder-card .icon-frame ^Icon {
  font-size: 28;
}

/* I want this to also apply a background color to
the .filter-card-icon class */
.filtered-reminder-card .reminders-all, .reminders-all .icon-frame { 
  background-color: #E2D8D8;
}

.filtered-reminder-card .reminders-due, .reminders-due .icon-frame  {
  background-color: #FBC0C0;
}

.filtered-reminder-card .reminders-future, .reminders-future .icon-frame  {
  background-color: #C0E5FB;
}

.filtered-reminder-card .reminders-completed, .reminders-completed .icon-frame {
  background-color: #C0FBE7;
}

.reminder-dashboard-layout {
  -xf-spacing: 0;
}

.add-reminder-icon-frame { 
  background-color: #009CE3;
  width: 24;
  height: 24;
  border-radius: 12;
  padding: 0;
}

.add-reminder-icon-frame ^Icon {
  font-size: 16;
  color: white;
}

.reminder-date {
  color: #a3a0a0;
}

.reminder-type-detail {
  color: #a3a0a0;
}

.reminder-past-due {
  color: #f35c5c;
}

.reminder-item-frame {
  padding: 0;
}

.reminders-list-layout {
  -xf-spacing: 0;
}

.reminder-type-frame {
  padding: 0;
}

/* Person Profile Blocks */
.block-personprofile .block-panel .panel-layout, .block-personprofile .block-panel .items-layout {
  -xf-spacing: 0;
}

.block-attributevalues .block-panel .panel-layout, .block-attributevalues .block-panel .items-layout  {
  -xf-spacing: 0;
}

.block-personprofile .block-panel .block-panel-name-label {
  font-size: 14;
}

.block-personprofile .block-panel .items-frame,
.block-attribute-values .block-panel .items-frame {
  background-color: white;
  border-radius: 8;
  margin: 0;
}

.block-personprofile .block-panel .item-frame,
.block-attribute-values .block-panel .item-frame {
  padding: 0;
}

.block-personprofile .block-panel .items-frame .item-flex-layout,
.block-attributevalues .block-panel .items-frame .item-flex-layout {
  padding: 0, 8;
}

.item-value {
  color: black;
}

.value-action-item {
  color: ?color-primary;
}

.value-no-action-item {
  color: #9e9ea0;
}

.phone-number-field-container ^SwitchList .option-layout,
.phone-number-field-container ^SwitchList ^Divider {
  padding: 0, 8;
  margin: 0;
}

.person-profile-email-edit-sheet.email-field-container ^SwitchList .option-layout,
.person-profile-email-edit-sheet.email-field-container ^SwitchList ^Divider {
  padding: 0, 8;
  margin: 0;
}
";
        #endregion
    }
}