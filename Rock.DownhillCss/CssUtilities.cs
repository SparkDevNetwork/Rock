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
using Rock.DownhillCss.Utility;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Rock.DownhillCss
{
    /// <summary>
    /// This class is used to define styles for Rock applications that run on other platforms.
    /// It follows the provided <see cref="DownhillSettings"/> to generate a large amount
    /// of utility classes (CSS) that can be used to style an application.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This utility was primarily written for Rock Mobile, which runs on .NET MAUI (https://github.com/dotnet/maui).
    ///         It may not be perfect for all platforms without additional work.
    ///     </para>
    /// </remarks>
    public static class CssUtilities
    {
        /// <summary>
        /// The internal instance of the CSS utilities.
        /// </summary>
        private static CssUtilitiesInternal Instance { get; set; }

        /// <summary>
        /// Builds the Downhill CSS framework.\
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string BuildFramework( DownhillSettings settings )
        {
            if ( Instance == null )
            {
                Instance = new CssUtilitiesInternal( settings );
            }
            else
            {
                Instance.Settings = settings;
            }

            return Instance.BuildFramework();
        }

        /// <summary>
        /// Builds the Downhill CSS framework with additional CSS.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="additionalCss"></param>
        /// <returns></returns>
        public static string BuildFramework( DownhillSettings settings, string additionalCss )
        {
            Instance = new CssUtilitiesInternal( settings, additionalCss );
            return Instance.BuildFramework();
        }

        /// <summary>
        /// The internal class for the CSS utilities.
        /// </summary>
        internal class CssUtilitiesInternal
        {
            #region Properties

            /// <summary>
            /// The settings for the Downhill framework.
            /// </summary>
            public DownhillSettings Settings { get; internal set; }

            /// <summary>
            /// The platform for the Downhill framework. Retrieved from the settings.
            /// </summary>
            protected DownhillPlatform Platform => Settings.Platform;

            #endregion

            #region Fields

            /// <summary>
            /// The CSS builder.
            /// </summary>
            private StringBuilder _cssBuilder;

            /// <summary>
            /// Additional CSS to parse.
            /// </summary>
            private string _additionalCss;

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="CssUtilities"/> class.
            /// </summary>
            /// <param name="settings"></param>
            /// <param name="additionalCss"></param>
            public CssUtilitiesInternal( DownhillSettings settings, string additionalCss = "" )
            {
                Settings = settings;
                _cssBuilder = new StringBuilder();
                _additionalCss = additionalCss;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Builds the CSS framework for the specified settings.
            /// </summary>
            /// <returns></returns>
            public string BuildFramework()
            {
                if ( Platform == DownhillPlatform.Mobile )
                {
                    if ( Settings.MobileStyleFramework == MobileStyleFramework.Legacy || Settings.MobileStyleFramework == MobileStyleFramework.Blended )
                    {
                        _cssBuilder.Append( legacyStylesMobile );
                    }
                    else if ( Settings.MobileStyleFramework == MobileStyleFramework.Standard || Settings.MobileStyleFramework == MobileStyleFramework.Blended )
                    {
                        _cssBuilder.Append( baseStylesMobile );
                    }
                }
                else
                {
                    _cssBuilder.Append( baseStylesWeb );
                }

                BuildTypographyUtilities();
                BuildColorUtilities();
                BuildSpacingUtilities();
                BuildSizeUtilities();
                BuildBorderWidthUtilities();

                if ( !string.IsNullOrWhiteSpace( _additionalCss ) )
                {
                    _cssBuilder.Append( _additionalCss );
                }

                return ParseCss();
            }

            /// <summary>
            /// Parses the CSS by replacing '?' variables.
            /// </summary>
            /// <returns></returns>
            public string ParseCss()
            {
                var cssStyles = _cssBuilder.ToString();

                // Replace application colors
                PropertyInfo[] applicationColorProperties = typeof( ApplicationColors ).GetProperties();
                foreach ( PropertyInfo colorProperty in applicationColorProperties )
                {
                    var value = colorProperty.GetValue( Settings.ApplicationColors ).ToString();

                    var colorName = colorProperty.Name;

                    // Background
                    cssStyles = cssStyles.Replace( $"?color-{colorName.ToLower()}-background", MixThemeColor( value, -10 ) );

                    // Border
                    cssStyles = cssStyles.Replace( $"?color-{colorName.ToLower()}-border", MixThemeColor( value, -9 ) );

                    // Text
                    cssStyles = cssStyles.Replace( $"?color-{colorName.ToLower()}-text", MixThemeColor( value, 6 ) );

                    // If warning then also make a set for validation
                    if ( colorName == "Warning" )
                    {
                        // Background
                        cssStyles = cssStyles.Replace( $"?color-validation-background", MixThemeColor( value, -10 ) );

                        // Border
                        cssStyles = cssStyles.Replace( $"?color-validation-border", MixThemeColor( value, -9 ) );

                        // Text
                        cssStyles = cssStyles.Replace( $"?color-validation-text", MixThemeColor( value, 6 ) );
                    }

                    // In the future, we could add a setting
                    // to disable tailwind colors. For now,
                    // just always include them.
                    if ( Settings.SupplyTailwindCss || true )
                    {
                        foreach ( var color in ColorPalette.ColorMaps )
                        {
                            foreach ( var saturatedColor in color.ColorSaturations )
                            {
                                cssStyles = ReplaceCssVariable( cssStyles, $"?color-{color.Color.ToLower()}-{saturatedColor.Intensity}", saturatedColor.ColorValue.ToLower() );
                            }
                        }
                    }

                    // Split the property names by capitalization
                    // and join them with a hyphen.
                    var colorNameHyphenated = GetHyphenatedPropertyName( colorName );

                    // Ex: ?color-interface-strongest
                    cssStyles = ReplaceCssVariable( cssStyles, $"?color-{colorNameHyphenated}", value );
                }

                cssStyles = ParseLegacyCss( cssStyles );

                foreach ( var extraCss in Settings.AdditionalCssToParse )
                {
                    var key = extraCss.Key;
                    var value = extraCss.Value;

                    cssStyles = ReplaceCssVariable( cssStyles, key, value );
                }

                return cssStyles;
            }

            /// <summary>
            /// Replaces the specified CSS variable with the value.
            /// </summary>
            /// <param name="cssStyles"></param>
            /// <param name="name"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            private string ReplaceCssVariable( string cssStyles, string name, string value )
            {
                if ( string.IsNullOrWhiteSpace( name ) || string.IsNullOrWhiteSpace( value ) )
                {
                    return cssStyles;
                }

                if ( !name.StartsWith( "?" ) )
                {
                    name = $"?{name}";
                }

                name = Regex.Escape( name.Substring( 1 ) );

                // Use regex to match the ? variable and replace it with the value.
                // Add negative lookahead to ensure the variable is not followed by '-'.
                return Regex.Replace( cssStyles, $@"\?{name}\b(?!-)", value );
            }

            /// <summary>
            /// Creates the CSS for the alert component.
            /// </summary>
            /// <param name="colorName"></param>
            /// <param name="frameworkCss"></param>
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

                if ( originalColor.Alpha < 1 )
                {
                    return originalColor.ToRGBA();
                }

                return originalColor.ToHex();
            }

            /// <summary>
            /// Replaces the legacy colors with the old values from settings.
            /// </summary>
            /// <param name="cssStyles"></param>
            /// <returns></returns>
            private string ParseLegacyCss( string cssStyles )
            {
                if ( Settings.MobileStyleFramework == MobileStyleFramework.Legacy || Settings.MobileStyleFramework == MobileStyleFramework.Blended )
                {
                    // Text and heading colors
                    cssStyles = cssStyles.Replace( "?color-text", Settings.TextColor );
                    cssStyles = cssStyles.Replace( "?color-heading", Settings.HeadingColor );
                    cssStyles = cssStyles.Replace( "?color-background", Settings.BackgroundColor );
                }

                if ( Settings.Platform == DownhillPlatform.Mobile )
                {
                    // Most Xamarin.Forms controls only support integer values for border-radius.
                    cssStyles = cssStyles.Replace( "?radius-base", ( ( int ) Math.Floor( Settings.RadiusBase ) ).ToString() );
                }
                else
                {
                    cssStyles = cssStyles.Replace( "?radius-base", Settings.RadiusBase.ToString() );
                }

                cssStyles = cssStyles.Replace( "?font-size-default", Settings.FontSizeDefault.ToString() );

                return cssStyles;
            }

            /// <summary>
            /// Adds utility classes for all of our named text styles.
            /// Such as .title, .caption, .title2 etc.
            /// </summary>
            private void BuildTypographyUtilities()
            {
                foreach ( var namedTextStyle in NamedTextStyle.AppleStyles )
                {
                    _cssBuilder.AppendLine( $"/* {namedTextStyle.Name} */" );

                    AddUtilityClass( string.Empty, namedTextStyle.Name.ToLower(), new Dictionary<string, string>
                    {
                        ["font-size"] = namedTextStyle.Size.ToString(),
                        ["line-height"] = namedTextStyle.LineHeight.ToString()
                    } );
                }

                AddUtilityClass( string.Empty, "bold", new Dictionary<string, string>
                {
                    ["font-style"] = "bold"
                } );

                AddUtilityClass( string.Empty, "italic", new Dictionary<string, string>
                {
                    ["font-style"] = "italic"
                } );
            }

            /// <summary>
            /// Adds utility classes for all of the application colors.
            /// Such as .bg-x, .text-x, .border-x, etc.
            /// </summary>
            private void BuildColorUtilities()
            {
                var defaultColors = Settings.ApplicationColors;
                var flippedColors = FlipColors( defaultColors );
                var colorProperties = typeof( ApplicationColors ).GetProperties();

                AddColorUtilityClasses( defaultColors, string.Empty );
                AddColorUtilityClasses( flippedColors, "dark-mode" );

                if ( Settings.SupplyTailwindCss || true )
                {
                    foreach ( var color in ColorPalette.ColorMaps )
                    {
                        foreach ( var saturatedColor in color.ColorSaturations )
                        {
                            AddUtilityClass( "bg", $"{color.Color.ToLower()}-{saturatedColor.Intensity}", new Dictionary<string, string>
                            {
                                ["background-color"] = saturatedColor.ColorValue.ToLower()
                            } );

                            AddUtilityClass( "text", $"{color.Color.ToLower()}-{saturatedColor.Intensity}", new Dictionary<string, string>
                            {
                                ["color"] = saturatedColor.ColorValue.ToLower()
                            } );

                            AddUtilityClass( "border", $"{color.Color.ToLower()}-{saturatedColor.Intensity}", new Dictionary<string, string>
                            {
                                ["border-color"] = saturatedColor.ColorValue.ToLower()
                            } );
                        }
                    }
                }

                void AddColorUtilityClasses( ApplicationColors colors, string prefix )
                {
                    // Colors
                    foreach ( var color in colorProperties )
                    {
                        var colorName = GetHyphenatedPropertyName( color.Name );
                        var colorValue = color.GetValue( colors ).ToString();

                        // There is kind of some unique logic to manage all states of compatibility for
                        // the downhill generated.
                        // If the Style Framework is set to Legacy, we want to skip any new colors.
                        // If the Style Framework is set to Standard, we want to skip any legacy colors.
                        // If the Style Framework is set to Blended, we want to include all colors.
                        if ( Settings.MobileStyleFramework == MobileStyleFramework.Legacy )
                        {
                            var obsoleteAttribute = color.GetCustomAttribute<ObsoleteAttribute>();

                            // Skip all NON obsolete attributes.
                            if ( obsoleteAttribute == null )
                            {
                                continue;
                            }

                            CreateAlertByColor( colorName, _cssBuilder );
                        }
                        else if( Settings.MobileStyleFramework == MobileStyleFramework.Blended )
                        {
                            CreateAlertByColor( colorName, _cssBuilder );
                        }
                        else if ( Settings.MobileStyleFramework == MobileStyleFramework.Standard )
                        {
                            var obsoleteAttribute = color.GetCustomAttribute<ObsoleteAttribute>();

                            // Skip all obsolete attributes.
                            if ( obsoleteAttribute != null )
                            {
                                continue;
                            }
                        }

                        _cssBuilder.AppendLine( $"/* Color: {colorName} {prefix} */" );

                        // Background Color
                        var bgClass = string.IsNullOrWhiteSpace( prefix ) ? "bg" : $"{prefix} .bg";
                        AddUtilityClass( bgClass, colorName, new Dictionary<string, string>
                        {
                            ["background-color"] = colorValue
                        } );

                        // Text Color
                        var textClass = string.IsNullOrWhiteSpace( prefix ) ? "text" : $"{prefix} .text";
                        AddUtilityClass( textClass, colorName, new Dictionary<string, string>
                        {
                            ["color"] = colorValue
                        } );

                        // Border Color
                        var borderClass = string.IsNullOrWhiteSpace( prefix ) ? "border" : $"{prefix} .border";
                        AddUtilityClass( borderClass, colorName, new Dictionary<string, string>
                        {
                            ["border-color"] = colorValue
                        } );
                    }

                    // Create one more for validation
                    CreateAlertByColor( "validation", _cssBuilder );
                }
            }

            /// <summary>
            /// Adds utility classes for all of the spacing values.
            /// This includes margin, padding and spacing.
            /// </summary>
            private void BuildSpacingUtilities()
            {
                var spacings = Settings.SpacingValues;

                foreach ( var spacing in spacings )
                {
                    _cssBuilder.AppendLine( $"/* Spacing Unit: {spacing.Value} */" );

                    // m-
                    AddUtilityClass( "m", spacing.Key.ToString(), new Dictionary<string, string>
                    {
                        ["margin"] = spacing.Value
                    } );

                    // ml-
                    AddUtilityClass( "ml", spacing.Key.ToString(), new Dictionary<string, string>
                    {
                        ["margin-left"] = spacing.Value
                    } );

                    // mt-
                    AddUtilityClass( "mt", spacing.Key.ToString(), new Dictionary<string, string>
                    {
                        ["margin-top"] = spacing.Value
                    } );

                    // mr-
                    AddUtilityClass( "mr", spacing.Key.ToString(), new Dictionary<string, string>
                    {
                        ["margin-right"] = spacing.Value
                    } );

                    // mb-
                    AddUtilityClass( "mb", spacing.Key.ToString(), new Dictionary<string, string>
                    {
                        ["margin-bottom"] = spacing.Value
                    } );

                    // my-
                    AddUtilityClass( "my", spacing.Key.ToString(), new Dictionary<string, string>
                    {
                        ["margin-top"] = spacing.Value,
                        ["margin-bottom"] = spacing.Value
                    } );

                    // mx-
                    AddUtilityClass( "mx", spacing.Key.ToString(), new Dictionary<string, string>
                    {
                        ["margin-left"] = spacing.Value,
                        ["margin-right"] = spacing.Value
                    } );

                    // p-
                    AddUtilityClass( "p", spacing.Key.ToString(), new Dictionary<string, string>
                    {
                        ["padding"] = spacing.Value
                    } );

                    // pl-
                    AddUtilityClass( "pl", spacing.Key.ToString(), new Dictionary<string, string>
                    {
                        ["padding-left"] = spacing.Value
                    } );

                    // pt-
                    AddUtilityClass( "pt", spacing.Key.ToString(), new Dictionary<string, string>
                    {
                        ["padding-top"] = spacing.Value
                    } );

                    // pr-
                    AddUtilityClass( "pr", spacing.Key.ToString(), new Dictionary<string, string>
                    {
                        ["padding-right"] = spacing.Value
                    } );

                    // pb-
                    AddUtilityClass( "pb", spacing.Key.ToString(), new Dictionary<string, string>
                    {
                        ["padding-bottom"] = spacing.Value
                    } );

                    // py-
                    AddUtilityClass( "py", spacing.Key.ToString(), new Dictionary<string, string>
                    {
                        ["padding-top"] = spacing.Value,
                        ["padding-bottom"] = spacing.Value
                    } );

                    // px-
                    AddUtilityClass( "px", spacing.Key.ToString(), new Dictionary<string, string>
                    {
                        ["padding-left"] = spacing.Value,
                        ["padding-right"] = spacing.Value
                    } );

                    var useStandardStyles = Settings.MobileStyleFramework == MobileStyleFramework.Standard || Settings.MobileStyleFramework == MobileStyleFramework.Blended;

                    // We only want to include these styles
                    // if we aren't in legacy mode.
                    // In the shell, what used to be defined in CSS
                    // is now defined with a spacing- class, so we want
                    // to make sure that does not conflict with any legacy
                    // styles.
                    if( useStandardStyles )
                    {
                        // spacing-
                        AddUtilityClass( "spacing", spacing.Key.ToString(), new Dictionary<string, string>
                        {
                            ["-maui-spacing"] = spacing.Value
                        } );

                        // gap-
                        AddUtilityClass( "gap", spacing.Key.ToString(), new Dictionary<string, string>
                        {
                            ["row-gap"] = spacing.Value,
                            ["column-gap"] = spacing.Value,
                            ["-rock-responsive-layout-row-spacing"] = spacing.Value,
                            ["-rock-responsive-layout-column-spacing"] = spacing.Value
                        } );

                        // gap-row-
                        AddUtilityClass( "gap-row", spacing.Key.ToString(), new Dictionary<string, string>
                        {
                            ["row-gap"] = spacing.Value,
                            ["-rock-responsive-layout-row-spacing"] = spacing.Value
                        }
                        );

                        // gap-column-
                        AddUtilityClass( "gap-column", spacing.Key.ToString(), new Dictionary<string, string>
                        {
                            ["column-gap"] = spacing.Value,
                            ["-rock-responsive-layout-column-spacing"] = spacing.Value
                        } );
                    }
                }
            }

            /// <summary>
            /// Adds utility classes for all of the border widths.
            /// </summary>
            private void BuildBorderWidthUtilities()
            {
                foreach ( var width in Settings.BorderWidths )
                {
                    // We also want to supply a .border class that will apply a 1px border.
                    if ( width == 1 )
                    {
                        AddUtilityClass( string.Empty, "border", new Dictionary<string, string>
                        {
                            ["border-width"] = $"{width}{Settings.BorderUnits}"
                        } );
                    }

                    AddUtilityClass( "border", width.ToString(), new Dictionary<string, string>
                    {
                        ["border-width"] = $"{width}{Settings.BorderUnits}"
                    } );
                }
            }

            /// <summary>
            /// Adds utility classes for all of the size values.
            /// Such as h-{value} and w-{value}.
            /// </summary>
            private void BuildSizeUtilities()
            {
                foreach ( var size in Settings.SpacingValues )
                {
                    AddUtilityClass( "h", size.Key.ToString(), new Dictionary<string, string>
                    {
                        ["height"] = size.Value
                    } );

                    AddUtilityClass( "w", size.Key.ToString(), new Dictionary<string, string>
                    {
                        ["width"] = size.Value
                    } );
                }
            }

            /// <summary>
            /// Adds a utility class to the specified CSS.
            /// </summary>
            /// <param name="classPrefix">The class prefix, essentially whatever comes before the utility name. For ex: "bg" would be used here for a class like '.bg-blue'.</param>
            /// <param name="className">The name of the class. This is typically the specific color/spacing unit.</param>
            /// <param name="propertyValues">The properties to set for this utility class.</param>
            private void AddUtilityClass( string classPrefix, string className, Dictionary<string, string> propertyValues )
            {
                string classSelector = className;
                if ( !string.IsNullOrEmpty( classPrefix ) )
                {
                    classSelector = $"{classPrefix}-{className}";
                }

                _cssBuilder.AppendLine( $".{classSelector} {{" );
                foreach ( var property in propertyValues )
                {
                    _cssBuilder.AppendLine( $"    {property.Key}: {property.Value};" );
                }
                _cssBuilder.AppendLine( "}" );
                _cssBuilder.AppendLine( $"" );
            }

            /// <summary>
            /// Flips the application colors to the other theme.
            /// </summary>
            /// <param name="colors"></param>
            /// <returns></returns>
            private static ApplicationColors FlipColors( ApplicationColors colors )
            {
                var flippedColors = new ApplicationColors
                {
                    // Soft = Strong & Strong = Soft
                    BrandSoft = colors.BrandStrong,
                    BrandStrong = colors.BrandSoft,

                    SuccessSoft = colors.SuccessStrong,
                    SuccessStrong = colors.SuccessSoft,

                    WarningSoft = colors.WarningStrong,
                    WarningStrong = colors.WarningSoft,

                    DangerSoft = colors.DangerStrong,
                    DangerStrong = colors.DangerSoft,

                    PrimaryStrong = colors.PrimarySoft,
                    PrimarySoft = colors.PrimaryStrong,

                    InfoStrong = colors.InfoSoft,
                    InfoSoft = colors.InfoStrong,

                    SecondarySoft = colors.SecondaryStrong,
                    SecondaryStrong = colors.SecondarySoft,

                    InterfaceStrongest = colors.InterfaceSoftest,
                    InterfaceStronger = colors.InterfaceSofter,
                    InterfaceStrong = colors.InterfaceSoft,
                    InterfaceMedium = colors.InterfaceMedium,
                    InterfaceSoft = colors.InterfaceStrong,

                    // These two are an exception
                    // to the traditional swap, we want
                    // the background to be pure black on dark mode.
                    InterfaceSofter = colors.InterfaceStrongest,
                    InterfaceSoftest = colors.InterfaceStronger
                };

                return flippedColors;
            }

            /// <summary>
            /// Takes a property name (in PascalCase) and returns a hyphenated version of the name in lowercase.
            /// For example, "InterfaceStrongest" would return "interface-strongest".
            /// </summary>
            /// <param name="propertyName"></param>
            /// <returns></returns>
            private string GetHyphenatedPropertyName( string propertyName )
            {
                return string.Join( "-", Regex.Split( propertyName, @"(?<!^)(?=[A-Z])" ) ).ToLower();
            }

            #endregion

            #region Platform Base Styles

            private static string baseStylesMobile = @"/* Fixes frame backgrounds from being black while in dark mode */
^editor {
    background-color: transparent;
}

^frame {
    background-color: transparent;
}

^radiobutton {
    background-color: transparent;
}

^border {
    background-color: transparent;
}

^Page {
    -rock-status-bar-text: light;
}

^ContentPage {
    background-color: ?color-interface-softer;
}

.dark-mode ^ContentPage {
    background-color: ?color-interface-strongest;
}

^label {
    font-size: ?font-size-default;
    color: ?color-interface-medium;
}

icon {
    color: ?color-interface-medium;
}

.link {
    color: ?color-primary-strong;
}

/*** Buttons ***/
/* Primary */
.btn.btn-primary {
    background-color: ?color-primary-strong;
    color: ?color-interface-softest;
}

/* Success */
.btn.btn-success {
    background-color: ?color-success-strong;
    color: ?color-interface-softest;
}

/* Info */
.btn.btn-info {
    background-color: ?color-info-strong;
    color: ?color-interface-softest;
}

/* Warning */
.btn.btn-warning {
    background-color: ?color-warning-strong;
    color: ?color-interface-softest;
}

/* Danger */
.btn.btn-danger {
    background-color: ?color-danger-strong;
    color: ?color-interface-softest;
}


/* Dark */
.btn.btn-dark {
    color: ?color-interface-soft;
    background-color: ?color-interface-strong;
}

/* Light */
.btn.btn-light {
    color: ?color-interface-strong;
    background-color: ?color-interface-soft;
}

/* Secondary */
.btn.btn-secondary {
    color: ?color-interface-softest;
    background-color: ?color-secondary-strong;
}

/* Brand */
.btn.btn-brand {
    color: ?color-interface-softest;
    background-color: ?color-brand-strong;
}

/* Default */
.btn.btn-default {
    color: ?color-primary-strong;
    border-color: ?color-primary-strong;
    border-width: 1;
    background-color: transparent;
}

/* Link Buttons */

/* Link primary */
.btn.btn-link {
    color: ?color-primary-strong;
    border-width: 0;
    background-color: transparent;
}

/* Link secondary */
.btn.btn-link-secondary {
    color: ?color-secondary-strong;
    border-width: 0;
    background-color: transparent;
}

/* Link success */
.btn.btn-link-success {
    color: ?color-success-strong;
    border-width: 0;
    background-color: transparent;
}

/* Link danger */
.btn.btn-link-danger {
    color: ?color-danger-strong;
    border-width: 0;
    background-color: transparent;
}

/* Link warning */
.btn.btn-link-warning {
    color: ?color-warning-strong;
    border-width: 0;
    background-color: transparent;
}

/* Link info */
.btn.btn-link-info {
    color: ?color-info-strong;
    border-width: 0;
    background-color: transparent;
}

/* Link light */
.btn.btn-link-light {
    color: ?color-interface-soft;
    border-width: 0;
    background-color: transparent;
}

/* Link dark */
.btn.btn-link-dark {
    color: ?color-interface-strong;
    border-width: 0;
    background-color: transparent;
}

/* Link brand */
.btn.btn-link-brand {
    color: ?color-brand-strong;
    border-width: 0;
    background-color: transparent;
}

/* Outline Buttons */

/* Outline primary */
.btn.btn-outline-primary {
    color: ?color-primary-strong;
    border-color: ?color-primary-strong;
    border-width: 1;
    background-color: transparent;
}

/* Outline secondary */
.btn.btn-outline-secondary {
    color: ?color-secondary-strong;
    border-color: ?color-secondary-strong;
    border-width: 1;
    background-color: transparent;
}

/* Outline success */
.btn.btn-outline-success {
    color: ?color-success-strong;
    border-color: ?color-success-strong;
    border-width: 1;
    background-color: transparent;
}

/* Outline danger */
.btn.btn-outline-danger {
    color: ?color-danger-strong;
    border-color: ?color-danger-strong;
    border-width: 1;
    background-color: transparent;
}

/* Outline warning */
.btn.btn-outline-warning {
    color: ?color-warning-strong;
    border-color: ?color-warning-strong;
    border-width: 1;
    background-color: transparent;
}

/* Outline info */
.btn.btn-outline-info {
    color: ?color-info-strong;
    border-color: ?color-info-strong;
    border-width: 1;
    background-color: transparent;
}

/* Outline light */
.btn.btn-outline-light {
    color: ?color-interface-medium;
    border-color: ?color-interface-soft;
    border-width: 1;
    background-color: transparent;
}

/* Outline dark */
.btn.btn-outline-dark {
    color: ?color-interface-strong;
    border-color: ?color-interface-strong;
    border-width: 1;
    background-color: transparent;
}

/* Outline brand */
.btn.btn-outline-brand {
    color: ?color-brand-strong;
    border-color: ?color-brand-strong;
    border-width: 1;
    background-color: transparent;
}

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

/* Button Sizes */
.btn {
    font-size: 17;
    padding: 16;
    font-style: bold;
    border-radius: 8;
}

.btn.btn-sm {
    font-size: 17;
    padding: 8 16;
    font-style: bold;
}

.btn.btn-lg {
    font-size: 17;
    padding: 24 16;
    font-style: bold;
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

.text {
    font-size: ?font-size-default;
    color: ?color-interface-medium;
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
    text-align: end;
}

.text-left {
    text-align: start;
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

/* Text Alignment */
.text-center {
    text-align: center;
}

.text-right {
    text-align: end;
}

.text-left {
    text-align: start;
}

.text-start {
    text-align: start;
}

.text-end {
    text-align: end;
}

 /* Fields */
fieldgroupheader .required-indicator {
    margin-top: 2;    
}

.required-indicator {
    color: transparent;
    width: 6;
    height: 6;
    border-radius: 3;
}

fieldgroupheader.required .required-indicator,
formfield.required .required-indicator,
.unlabeled.required {
    color: ?color-danger-strong;
}

.dark-mode fieldgroupheader.required .required-indicator,
.dark-mode formfield.required .required-indicator  {
    color: ?color-danger-soft;
}

^picker,
^datepicker,
^personpicker {
    -rock-picker-placeholder-color: ?color-interface-medium;
    rock-placeholder-text-color: ?color-interface-medium;
}

^borderlessentry,
^datepicker,
^picker,
^entry,
^editor,
^personpicker {
    color: ?color-interface-strong;
    rock-placeholder-text-color: ?color-interface-medium;
}

.dark-mode ^borderlessentry,
.dark-mode ^picker,
.dark-mode ^editor,
.dark-mode ^datepicker,
.dark-mode ^entry {
    color: ?color-interface-soft;
}

^TabbedShell {
    -maui-bar-background-color: ?color-interface-softest;
    -rock-unselected-tab-color: ?color-interface-medium;
    -rock-selected-tab-color: ?color-primary-strong;
}

^TabbedShell.dark-mode {
    -maui-bar-background-color: ?color-interface-stronger;
    -rock-selected-tab-color: ?color-primary-strong;
}

.navigation-bar {
    -maui-bar-background-color: ?color-interface-softest;
    -maui-bar-text-color: ?color-interface-stronger;
}

.dark-mode .navigation-bar {
    -maui-bar-background-color: ?color-interface-stronger;
    -maui-bar-text-color: ?color-interface-softer;
}

/*** Cover Sheets ***/
.cover-sheet .cover-sheet-title {
  color: ?color-interface-stronger;
  font-style: bold;
  font-size: 17;
}

.dark-mode.cover-sheet .cover-sheet-title {
  color: ?color-interface-softer;
}

.cover-sheet {
    -maui-bar-background-color: ?color-interface-softest;
    -maui-bar-text-color: ?color-interface-stronger;
}

.dark-mode.cover-sheet {
  -maui-bar-background-color: ?color-interface-stronger;
  -maui-bar-text-color: ?color-interface-softer;
}

.cover-sheet .cover-sheet-toolbar-button {
  padding: 0;
  margin: 0;
  background-color: transparent;
  color: ?color-primary-strong;
  font-size: 17;
}

.dark-mode.cover-sheet .cover-sheet-toolbar-button {
  color: ?color-primary-soft;
}

.android.cover-sheet .cover-sheet-toolbar-button {
  margin: 12, 0;    
}

/* Divider */
.divider {
    background-color: ?color-interface-soft;
    height: 1;
}

.dark-mode .divider {
    background-color: ?color-interface-strong;
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

^MessageBubble {
    -rock-outbound-background-color: ?color-info-strong;
    -rock-outbound-text-color: ?color-interface-softest;
    -rock-inbound-background-color: ?color-interface-softest;
    -rock-inbound-text-color: ?color-interface-strong;
}

.dark-mode ^MessageBubble {
    -rock-outbound-background-color: ?color-info-soft;
    -rock-outbound-text-color: ?color-interface-stronger;
    -rock-inbound-background-color: ?color-interface-stronger;
    -rock-inbound-text-color: ?color-interface-soft;
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
}";
            private static string baseStylesWeb = @"";

            private static string legacyStylesMobile = @"/*
/*
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

.alert {
    margin: 0 0 12 0;
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
^fieldstack, .form-field-stack {
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
}";
            #endregion
        }
    }
}