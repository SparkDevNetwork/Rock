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
using Rock.DownhillCss.Utility;

namespace Rock.DownhillCss
{
    /// <summary>
    /// A set of utility methods for building and using Downhill
    /// </summary>
    public static class CssUtilities
    {

        // The spacer values are used to adjust the differences in the padding and margin amounts. They
        // will be multipled by a default spacing value (mobile = 10) to determine the actual value for
        // each. The values below are roughly mapped from the Bootstrap 4 values.
        private static decimal[] spacingValues = { 0, .25m, .5m, 1, 2, 3 };

        // TODO: Consider that Tailwind.css has a lot more of these {0,.25,.5,.75,1,1.25,1.5,2,2.5,3,4,5,6,8,10,12,14,16}
        // https://tailwindcss.com/docs/customizing-spacing#default-spacing-scale


        // The sizes of borders that should be generated
        private static int[] borderWidths = { 0, 1, 2, 4 };

        /// <summary>
        /// Dictionary of font sizes
        /// </summary>
        private static Dictionary<string, decimal> fontSizes = new Dictionary<string, decimal>()
        {
            { "xs", .75m },
            { "sm", .875m },
            { "base", 1 },
            { "lg", 1.125m },
            { "xl", 1.25m },
            { "2xl", 1.5m },
            { "3xl", 1.875m },
            { "4xl", 2.25m },
            { "5xl", 3m },
            { "6xl", 4m },
        };

        /// <summary>
        /// Builds the base framework.
        /// </summary>
        /// <returns></returns>
        public static string BuildFramework( DownhillSettings settings )
        {
            // Get application color properties
            PropertyInfo[] applicationColorProperties = typeof( ApplicationColors ).GetProperties();

            StringBuilder frameworkCss = new StringBuilder();

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
            cssStyles = cssStyles.Replace( "?spacing-base", settings.SpacingBase.ToString() );
            cssStyles = cssStyles.Replace( "?font-size-default", settings.FontSizeDefault.ToString() );

            // Text and heading colors
            cssStyles = cssStyles.Replace( "?color-text", settings.TextColor );
            cssStyles = cssStyles.Replace( "?color-heading", settings.HeadingColor );
            cssStyles = cssStyles.Replace( "?color-background", settings.BackgroundColor );

            return cssStyles;
        }

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

        private static void TextSizes( StringBuilder frameworkCss, DownhillSettings settings, PropertyInfo[] applicationColorProperties )
        {
            frameworkCss.AppendLine( "/*" );
            frameworkCss.AppendLine( "// Text Size Utilities" );
            frameworkCss.AppendLine( "*/" );

            foreach ( var size in fontSizes )
            {
                frameworkCss.AppendLine( $".text-{size.Key.ToLower()} {{" );
                frameworkCss.AppendLine( $"    font-size: {size.Value * settings.FontSizeDefault}{settings.FontUnits};" );
                frameworkCss.AppendLine( "}" );
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

        private static void Margins( StringBuilder frameworkCss, DownhillSettings settings, PropertyInfo[] applicationColorProperties )
        {
            int spacingValueCount = spacingValues.Length;

            frameworkCss.AppendLine( "" );
            frameworkCss.AppendLine( "/*" );
            frameworkCss.AppendLine( "// Margin Utilities" );
            frameworkCss.AppendLine( "*/" );

            // m- (all)
            for ( int i = 0; i < spacingValueCount; i++ )
            {
                frameworkCss.AppendLine( $".m-{i} {{" );
                frameworkCss.AppendLine( $"    margin: {spacingValues[i] * settings.SpacingBase}{settings.SpacingUnits};" );
                frameworkCss.AppendLine( "}" );
            }

            // mt- (top)
            for ( int i = 0; i < spacingValueCount; i++ )
            {
                frameworkCss.AppendLine( $".mt-{i} {{" );
                frameworkCss.AppendLine( $"    margin-top: {spacingValues[i] * settings.SpacingBase}{settings.SpacingUnits};" );
                frameworkCss.AppendLine( "}" );
            }

            // mb- (bottom)
            for ( int i = 0; i < spacingValueCount; i++ )
            {
                frameworkCss.AppendLine( $".mb-{i} {{" );
                frameworkCss.AppendLine( $"    margin-bottom: {spacingValues[i] * settings.SpacingBase}{settings.SpacingUnits};" );
                frameworkCss.AppendLine( "}" );
            }

            // ml- (left)
            for ( int i = 0; i < spacingValueCount; i++ )
            {
                frameworkCss.AppendLine( $".ml-{i} {{" );
                frameworkCss.AppendLine( $"    margin-left: {spacingValues[i] * settings.SpacingBase}{settings.SpacingUnits};" );
                frameworkCss.AppendLine( "}" );
            }

            // mr- (right)
            for ( int i = 0; i < spacingValueCount; i++ )
            {
                frameworkCss.AppendLine( $".mr-{i} {{" );
                frameworkCss.AppendLine( $"    margin-right: {spacingValues[i] * settings.SpacingBase}{settings.SpacingUnits};" );
                frameworkCss.AppendLine( "}" );
            }

            // mx- (left and right)
            for ( int i = 0; i < spacingValueCount; i++ )
            {
                frameworkCss.AppendLine( $".mx-{i} {{" );
                frameworkCss.AppendLine( $"    margin-left: {spacingValues[i] * settings.SpacingBase}{settings.SpacingUnits};" );
                frameworkCss.AppendLine( $"    margin-right: {spacingValues[i] * settings.SpacingBase}{settings.SpacingUnits};" );
                frameworkCss.AppendLine( "}" );
            }

            // my- (top and bottom)
            for ( int i = 0; i < spacingValueCount; i++ )
            {
                frameworkCss.AppendLine( $".my-{i} {{" );
                frameworkCss.AppendLine( $"    margin-top: {spacingValues[i] * settings.SpacingBase}{settings.SpacingUnits};" );
                frameworkCss.AppendLine( $"    margin-bottom: {spacingValues[i] * settings.SpacingBase}{settings.SpacingUnits};" );
                frameworkCss.AppendLine( "}" );
            }
        }

        private static void Paddings( StringBuilder frameworkCss, DownhillSettings settings, PropertyInfo[] applicationColorProperties )
        {
            int spacingValueCount = spacingValues.Length;

            frameworkCss.AppendLine( "" );
            frameworkCss.AppendLine( "/*" );
            frameworkCss.AppendLine( "// Padding Utilities" );
            frameworkCss.AppendLine( "*/" );

            // p- (all)
            for ( int i = 0; i < spacingValueCount; i++ )
            {
                frameworkCss.AppendLine( $".p-{i} {{" );
                frameworkCss.AppendLine( $"    padding: {spacingValues[i] * settings.SpacingBase}{settings.SpacingUnits};" );
                frameworkCss.AppendLine( "}" );
            }

            // pt- (top)
            for ( int i = 0; i < spacingValueCount; i++ )
            {
                frameworkCss.AppendLine( $".pt-{i} {{" );
                frameworkCss.AppendLine( $"    padding-top: {spacingValues[i] * settings.SpacingBase}{settings.SpacingUnits};" );
                frameworkCss.AppendLine( "}" );
            }

            // pb- (bottom)
            for ( int i = 0; i < spacingValueCount; i++ )
            {
                frameworkCss.AppendLine( $".pb-{i} {{" );
                frameworkCss.AppendLine( $"    padding-bottom: {spacingValues[i] * settings.SpacingBase}{settings.SpacingUnits};" );
                frameworkCss.AppendLine( "}" );
            }

            // pl- (left)
            for ( int i = 0; i < spacingValueCount; i++ )
            {
                frameworkCss.AppendLine( $".pl-{i} {{" );
                frameworkCss.AppendLine( $"    padding-left: {spacingValues[i] * settings.SpacingBase}{settings.SpacingUnits};" );
                frameworkCss.AppendLine( "}" );
            }

            // pr- (right)
            for ( int i = 0; i < spacingValueCount; i++ )
            {
                frameworkCss.AppendLine( $".pr-{i} {{" );
                frameworkCss.AppendLine( $"    padding-right: {spacingValues[i] * settings.SpacingBase}{settings.SpacingUnits};" );
                frameworkCss.AppendLine( "}" );
            }

            // px- (left and right)
            for ( int i = 0; i < spacingValueCount; i++ )
            {
                frameworkCss.AppendLine( $".px-{i} {{" );
                frameworkCss.AppendLine( $"    padding-left: {spacingValues[i] * settings.SpacingBase}{settings.SpacingUnits};" );
                frameworkCss.AppendLine( $"    padding-right: {spacingValues[i] * settings.SpacingBase}{settings.SpacingUnits};" );
                frameworkCss.AppendLine( "}" );
            }

            // py- (top and bottom)
            for ( int i = 0; i < spacingValueCount; i++ )
            {
                frameworkCss.AppendLine( $".py-{i} {{" );
                frameworkCss.AppendLine( $"    padding-top: {spacingValues[i] * settings.SpacingBase}{settings.SpacingUnits};" );
                frameworkCss.AppendLine( $"    padding-bottom: {spacingValues[i] * settings.SpacingBase}{settings.SpacingUnits};" );
                frameworkCss.AppendLine( "}" );
            }
        }

        private static void BorderWidths( StringBuilder frameworkCss, DownhillSettings settings, PropertyInfo[] applicationColorProperties )
        {
            int spacingValueCount = spacingValues.Length;
            int borderWidthCount = borderWidths.Length;

            frameworkCss.AppendLine( "" );
            frameworkCss.AppendLine( "/*" );
            frameworkCss.AppendLine( "// Border Widths" );
            frameworkCss.AppendLine( "*/" );

            for ( int i = 0; i < spacingValueCount; i++ )
            {
                if ( spacingValues[i] == 1 )
                {
                    // When the unit is 1 then we drop the unit from the class name .border (not .border-1)
                    // This is the pattern of both Bootstrap and Tailwind
                    // Tailwind and Bootstrap deviate here from t vs top. Bootstrap uses the full 'top' but since
                    // Tailwind uses t AND Bootstrap used t for margins and padding, decided to use just t.

                    // Xamarin Forms 4.0 does not allow for separate widths on borders
                    // https://github.com/xamarin/Xamarin.Forms/blob/4.3.0/Xamarin.Forms.Core/Properties/AssemblyInfo.cs

                    // border (all)
                    frameworkCss.AppendLine( $".border {{" );
                    frameworkCss.AppendLine( $"    border-width: {spacingValues[i]}{settings.BorderUnits};" );
                    frameworkCss.AppendLine( "}" );
                }
                else
                {
                    // border- (all)
                    frameworkCss.AppendLine( $".border-{i} {{" );
                    frameworkCss.AppendLine( $"    border-width: {spacingValues[i]}{settings.BorderUnits};" );
                    frameworkCss.AppendLine( "}" );
                }
            }
        }

        private static string baseStylesWeb = @"";

        private static string baseStylesMobile = @"
/* Resets */
^label {
    font-size: default;
    color: ?color-text;
}

.heading1 {
    color: ?color-heading;
    font-style: bold;
    font-size: 34;
    margin-bottom: 0;
    line-height: 1;
}

.heading2 {
    color: ?color-heading;
    font-style: bold;
    font-size: title;
    line-height: 1;
}

.heading3 {
    color: ?color-heading;
    font-style: bold;
    font-size: subtitle;
    line-height: 1.05;
}

.heading4 {
    color: ?color-heading;
    font-style: bold;
    font-size: default;
    line-height: 1.1;
}

.heading5, .heading6 {
    color: ?color-heading;
    font-style: bold;
    font-size: small;
    line-height: 1.25;
}

.link{
    color: ?color-primary;
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

/* Button Sizes */
.btn.btn-lg {
    font-size: large;
    padding: 20;
}

.btn.btn-sm {
    font-size: micro;
    height: 35;
}

/* Text Weights */
.text-bold {
    font-style: bold;
}

.text-italic {
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
.text-default {
    font-size: default;
    color: ?color-text;
}

.text-micro {
    font-size: micro;
    color: ?color-text;
}

.text-small {
    font-size: small;
    color: ?color-text;
}

.text-medium {
    font-size: medium;
    color: ?color-text;
}

.text-large {
    font-size: large;
    color: ?color-text;
}

.text-title {
    font-size: title;
    color: ?color-text;
}

.text-subtitle {
    font-size: subtitle;
    color: ?color-text;
}

.text-header {
    font-size: header;
    color: ?color-text;
}

.text-caption {
    font-size: caption;
    color: ?color-text;
}

.text-body {
    font-size: body;
    color: ?color-text;
}

/* Body Styles */
.body {
    font-size: default;
    color: ?color-text;
    line-height: 1.15;
    margin-bottom: 12;
}

.body-small {
    font-size: small;
    color: ?color-text;
    line-height: 1.25;
    margin-bottom: 12;
}

.body-micro {
    font-size: micro;
    color: ?color-text;
    line-height: 1.25;
    margin-bottom: 8;
}

.body-large {
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
    border-radius: 2;
}

.rounded {
    border-radius: 6;
}

.rounded-lg {
    border-radius: 10;
}

.rounded-full {
    border-radius: 1000;
}

/* Toggle Button CSS */
.toggle-button {
    border-radius: 0;
    border-color: ?color-primary;
    background-color: initial;
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

.calendar-filter-panel {
    margin-bottom: 5;
}

/* Calendar Classes */
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
    margin-bottom: 4;
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
fieldstack {
    border-radius: 0;
    border-color: ?color-secondary;
    border-width: 1;
    margin-bottom: 12;
}

/* Form Fields  */
formfield {
    padding: 12 12 12 6;
}

formfield .required-indicator {
    margin-right: 4;
}

";
    }
}