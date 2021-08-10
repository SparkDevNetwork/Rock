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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Rock.Utility;

namespace Rock.Lava.Filters
{
    public static partial class TemplateFilters
    {
        /// <summary>
        /// Lightens the color by the specified percentage amount.
        /// </summary>
        /// <param name="input">The input color.</param>
        /// <param name="amount">The percentage to change.</param>
        /// <returns></returns>
        public static string Lighten( string input, string amount )
        {
            var color = new RockColor( input );
            color.Lighten( CleanColorAmount( amount ) );

            return GetColorString( color, input );
        }

        /// <summary>
        /// Darkens the color by the provided percentage amount.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static string Darken( string input, string amount )
        {
            var color = new RockColor( input );
            color.Darken( CleanColorAmount( amount ) );

            return GetColorString( color, input );
        }

        /// <summary>
        /// Saturates the color by the provided percentage amount.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static string Saturate( string input, string amount )
        {
            var color = new RockColor( input );
            color.Saturate( CleanColorAmount( amount ) );

            // return the color in a format that matched the input
            if ( input.StartsWith( "#" ) )
            {
                return color.ToHex();
            }

            return color.ToRGBA();
        }

        /// <summary>
        /// Desaturates the color by the provided percentage amount.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static string Desaturate( string input, string amount )
        {
            var color = new RockColor( input );
            color.Desaturate( CleanColorAmount( amount ) );

            return GetColorString( color, input );
        }

        /// <summary>
        /// Decreases the opacity level by the given percentage. This makes the color less transparent (opaque).
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static string FadeIn( string input, string amount )
        {
            var color = new RockColor( input );
            color.FadeIn( CleanColorAmount( amount ) );

            return GetColorString( color, input );
        }

        /// <summary>
        /// Increases the opacity level by the given percentage. This makes the color more transparent.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static string FadeOut( string input, string amount )
        {
            var color = new RockColor( input );
            color.FadeOut( CleanColorAmount( amount ) );

            return GetColorString( color, input );
        }

        /// <summary>
        /// Adjusts the hue by the specificed percentage (10%) or degrees (10deg).
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static string AdjustHue( string input, string amount )
        {
            amount = amount.Trim();

            // Adjust by percent
            if ( amount.EndsWith( "%" ) )
            {
                var color = new RockColor( input );
                color.AdjustHueByPercent( CleanColorAmount( amount ) );

                return GetColorString( color, input );
            }

            // Adjust by degrees
            if ( amount.EndsWith( "deg" ) )
            {
                var color = new RockColor( input );
                color.AdjustHueByDegrees( CleanColorAmount( amount, "deg" ) );

                return GetColorString( color, input );
            }

            // They didn't provide a valid amount so give back the original color
            return input;

        }

        /// <summary>
        /// Tints (adds white) the specified color by the specified amount.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static string Tint( string input, string amount )
        {
            var color = new RockColor( input );
            color.Tint( CleanColorAmount( amount ) );

            return GetColorString( color, input );
        }

        /// <summary>
        /// Shades (adds black) the specified color by the specified amount.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static string Shade( string input, string amount )
        {
            var color = new RockColor( input );
            color.Shade( CleanColorAmount( amount ) );

            return GetColorString( color, input );
        }

        /// <summary>
        /// Mixes the specified color with the input color with the given amount.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="mixColorInput">The mix color input.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public static string Mix( string input, string mixColorInput, string amount )
        {
            var color = new RockColor( input );
            var mixColor = new RockColor( mixColorInput );

            color.Mix( mixColor, CleanColorAmount( amount ) );

            return GetColorString( color, input );
        }

        /// <summary>
        /// Returns the color in greyscale.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string Grayscale( string input )
        {
            var color = new RockColor( input );
            color.Grayscale();

            return GetColorString( color, input );
        }

        /// <summary>
        /// Returns the amount string as a proper int for the color functions.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="unit">The unit.</param>
        /// <returns></returns>
        private static int CleanColorAmount( string amount, string unit = "%" )
        {
            amount = amount.Replace( unit, "" ).Trim();

            var amountAsInt = amount.AsIntegerOrNull();

            if ( !amountAsInt.HasValue )
            {
                return 0;
            }

            return amountAsInt.Value;
        }

        /// <summary>
        /// Determines the proper return value of the color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        private static string GetColorString( RockColor color, string input )
        {
            if ( color.Alpha != 1 )
            {
                return color.ToRGBA();
            }

            if ( input.StartsWith( "#" ) )
            {
                return color.ToHex();
            }

            return color.ToRGBA();
        }
    }
}
