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

            // Apply Reset First
            frameworkCss.Append( resetStylesMobile );

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

            // Build Margin Utilties
            Margins( frameworkCss, settings, applicationColorProperties );

            // Build Padding Utilties
            Paddings( frameworkCss, settings, applicationColorProperties );

            // Build Border Width Utilities
            BorderWidths( frameworkCss, settings, applicationColorProperties ); /* somewhat mobile specific now */

            // Add base styles that are not generated
            if ( settings.Platform == DownhillPlatform.Mobile )
            {
                frameworkCss.Append( baseStylesMobile );
            }
            else
            {
                frameworkCss.Append( baseStylesWeb );
            }

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

            var originalColor = RockColor.FromHex( color );
            var mixColor = RockColor.FromHex( mixcolor );

            var mixPercent = ( int ) ( ( Math.Abs( level ) * .08m ) * 100 );

            originalColor.Mix( mixColor, mixPercent );

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

        private static string resetStylesMobile = @"
/*
    Resets
    -----------------------------------------------------------
*/

/* Fixes frame backgrounds from being black while in dark mode */

^editor {
    background-color: transparent;
    color: ?color-text;
}

^frame {
    background-color: transparent;
}

NavigationPage {
    -rock-status-bar-text: dark;
}

^contentpage {
    background-color: ?color-background;
}

^label {
    font-size: default;
    color: ?color-text;
}
";

        private static string baseStylesMobile = @"
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
    font-size: title;
    line-height: 1;
}

.h3 {
    color: ?color-heading;
    font-style: bold;
    font-size: subtitle;
    line-height: 1.05;
}

.h4 {
    color: ?color-heading;
    font-style: bold;
    font-size: default;
    line-height: 1.1;
}

.h5, .h6 {
    color: ?color-heading;
    font-style: bold;
    font-size: small;
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
    font-size: default;
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
    font-size: default;
    line-height: 1;
}

/* Body Styles */
.paragraph {
    font-size: default;
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

/* Divider */
.divider {
    background-color: ?color-gray-400;
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

/* Forms Styling */
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

/* Buttons */
.btn {
    border-radius: ?radius-base;
    padding-left: 16;
    padding-right: 16;
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
    height: 100;
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
    margin-top: 32;
    text-align: center;
}

.calendar-events-day {
    text-align: left;
    margin-bottom: 8;
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

/* Modals */
.modal-header {
    background-color: ?color-gray-400;
}

.modal-title {  
}

.modal-close {
    opacity: 0.5;
}

/* Forms Styles */
^borderlessentry,
^datepicker,
^checkbox, 
^picker,
^entry, 
^switch {
    color: ?color-text;
    font-size: default;
}

/* Field Titles */
fieldgroupheader {
   
}

fieldgroupheader .title,
formfield .title {
    color: ?color-text;
    font-style: bold;
    font-size: default;
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
";
        #endregion
    }
}