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

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Enums.Core;
using Rock.Utility;

/**
 * This file has a TypeScript counterpart in the Obsidian project. If changes
 * are made to any of these tests then those changes must be made to the
 * Obsidian version as well.
 */

namespace Rock.Tests.UnitTests.Rock.Utility
{
    /// <summary>
    /// Tests for Reflection.cs
    /// </summary>
    [TestClass]
    public class RockColorTests
    {
        #region Constructor

        /// <summary>
        /// Named color Constructor should return expected values.
        /// </summary>
        /// <param name="colorName">Name of the color.</param>
        /// <param name="expectedRed">The expected red component value.</param>
        /// <param name="expectedGreen">The expected green component value.</param>
        /// <param name="expectedBlue">The expected blue component value.</param>
        /// <param name="expectedAlpha">The expected alpha component value.</param>
        [TestMethod]
        [DataRow( "red", 255, 0, 0, 1 )]
        [DataRow( "green", 0, 128, 0, 1 )]
        [DataRow( "blue", 0, 0, 255, 1 )]
        [DataRow( "deeppink", 255, 20, 147, 1 )]
        [DataRow( "orange", 255, 165, 0, 1 )]
        [DataRow( "transparent", 0, 0, 0, 0 )]
        public void Constructor_ValidNamedColorIsCorrect( string colorName, double expectedRed, double expectedGreen, double expectedBlue, double expectedAlpha )
        {
            var color = new RockColor( colorName );

            Assert.AreEqual( expectedRed, color.R );
            Assert.AreEqual( expectedGreen, color.G );
            Assert.AreEqual( expectedBlue, color.B );
            Assert.AreEqual( expectedAlpha, color.Alpha );
        }

        /// <summary>
        /// Named color Constructor should return black for invalid names.
        /// </summary>
        [TestMethod]
        public void Constructor_InvalidNamedColorIsBlack()
        {
            var color = new RockColor( "bogon" );

            Assert.AreEqual( 0, color.R );
            Assert.AreEqual( 0, color.G );
            Assert.AreEqual( 0, color.B );
            Assert.AreEqual( 1, color.Alpha );
        }

        /// <summary>
        /// Hex color Constructor should return expected values.
        /// </summary>
        /// <param name="hexColor">Hex value of the color.</param>
        /// <param name="expectedRed">The expected red component value.</param>
        /// <param name="expectedGreen">The expected green component value.</param>
        /// <param name="expectedBlue">The expected blue component value.</param>
        /// <param name="expectedAlpha">The expected alpha component value.</param>
        [TestMethod]
        [DataRow( "#f00", 255, 0, 0, 1 )]
        [DataRow( "#0f04", 0, 255, 0, 0x44 / 255.0 )]
        [DataRow( "#008000", 0, 128, 0, 1 )]
        [DataRow( "#0000ff80", 0, 0, 255, 0x80 / 255.0 )]
        public void Constructor_ValidHexColorIsCorrect( string hexColor, double expectedRed, double expectedGreen, double expectedBlue, double expectedAlpha )
        {
            var color = new RockColor( hexColor );

            Assert.AreEqual( expectedRed, color.R );
            Assert.AreEqual( expectedGreen, color.G );
            Assert.AreEqual( expectedBlue, color.B );
            Assert.AreEqual( expectedAlpha, color.Alpha );
        }

        /// <summary>
        /// Hex color Constructor should return black for invalid values.
        /// </summary>
        /// <param name="hexColor">Name of the color.</param>
        [TestMethod]
        [DataRow( "#" )]
        [DataRow( "#f" )]
        [DataRow( "#ff" )]
        [DataRow( "#fffff" )]
        [DataRow( "#fffffff" )]
        [DataRow( "#fffffffff" )]
        public void Constructor_InvalidHexColorIsBlack( string hexColor )
        {
            var color = new RockColor( hexColor );

            Assert.AreEqual( 0, color.R );
            Assert.AreEqual( 0, color.G );
            Assert.AreEqual( 0, color.B );
            Assert.AreEqual( 1, color.Alpha );
        }

        /// <summary>
        /// RGBA color Constructor should return expected values.
        /// </summary>
        /// <param name="rgbaColor">RGBA string value of the color.</param>
        /// <param name="expectedRed">The expected red component value.</param>
        /// <param name="expectedGreen">The expected green component value.</param>
        /// <param name="expectedBlue">The expected blue component value.</param>
        /// <param name="expectedAlpha">The expected alpha component value.</param>
        [TestMethod]
        [DataRow( "rgba(255, 0, 0, 1)", 255, 0, 0, 1 )]
        [DataRow( "rgba(0, 128, 0, 0)", 0, 128, 0, 0 )]
        [DataRow( "rgba(0, 0, 64, 0.5", 0, 0, 64, 0.5 )]
        [DataRow( "rgba(5, 10, 15, 1)", 5, 10, 15, 1 )]
        public void Constructor_ValidRgbaColorIsCorrect( string rgbaColor, double expectedRed, double expectedGreen, double expectedBlue, double expectedAlpha )
        {
            var color = new RockColor( rgbaColor );

            Assert.AreEqual( expectedRed, color.R );
            Assert.AreEqual( expectedGreen, color.G );
            Assert.AreEqual( expectedBlue, color.B );
            Assert.AreEqual( expectedAlpha, color.Alpha );
        }

        /// <summary>
        /// RGBA color Constructor should return black for invalid values.
        /// </summary>
        /// <param name="rgbaColor">RGBA string value of the color.</param>
        [TestMethod]
        [DataRow( "rgba(255)" )]
        [DataRow( "rgba(255, 255)" )]
        [DataRow( "rgba(255, 255, 255)" )]
        [DataRow( "rgba(255, 255, 255, 255, 255" )]
        public void Constructor_InvalidRgbaColorIsBlack( string rgbaColor )
        {
            var color = new RockColor( rgbaColor );

            Assert.AreEqual( 0, color.R );
            Assert.AreEqual( 0, color.G );
            Assert.AreEqual( 0, color.B );
            Assert.AreEqual( 1, color.Alpha );
        }

        /// <summary>
        /// RGBA color Constructor should normalize values.
        /// </summary>
        /// <param name="rgbaColor">RGBA string value of the color.</param>
        /// <param name="expectedRed">The expected red component value.</param>
        /// <param name="expectedGreen">The expected green component value.</param>
        /// <param name="expectedBlue">The expected blue component value.</param>
        /// <param name="expectedAlpha">The expected alpha component value.</param>
        [TestMethod]
        [DataRow( "rgba(500, -23, 0, -0.4)", 255, 0, 0, 0 )]
        [DataRow( "rgba(0, 0, 0, 23)", 0, 0, 0, 1 )]
        public void Constructor_RgbaNormalizesValues( string rgbaColor, double expectedRed, double expectedGreen, double expectedBlue, double expectedAlpha )
        {
            var color = new RockColor( rgbaColor );

            Assert.AreEqual( expectedRed, color.R );
            Assert.AreEqual( expectedGreen, color.G );
            Assert.AreEqual( expectedBlue, color.B );
            Assert.AreEqual( expectedAlpha, color.Alpha );
        }

        /// <summary>
        /// RGBA color Constructor should handle whitespace.
        /// </summary>
        /// <param name="rgbaColor">RGBA string value of the color.</param>
        /// <param name="expectedRed">The expected red component value.</param>
        /// <param name="expectedGreen">The expected green component value.</param>
        /// <param name="expectedBlue">The expected blue component value.</param>
        /// <param name="expectedAlpha">The expected alpha component value.</param>
        [TestMethod]
        [DataRow( "rgba(1,2,3,0.5)", 1, 2, 3, 0.5 )]
        [DataRow( "rgba(  1,2,3,0.5)", 1, 2, 3, 0.5 )]
        [DataRow( "rgba(1,2,3,0.5  )", 1, 2, 3, 0.5 )]
        [DataRow( "rgba(1,2,  3  ,0.5  )", 1, 2, 3, 0.5 )]
        public void Constructor_RgbaHandlesWhitespace( string rgbaColor, double expectedRed, double expectedGreen, double expectedBlue, double expectedAlpha )
        {
            var color = new RockColor( rgbaColor );

            Assert.AreEqual( expectedRed, color.R );
            Assert.AreEqual( expectedGreen, color.G );
            Assert.AreEqual( expectedBlue, color.B );
            Assert.AreEqual( expectedAlpha, color.Alpha );
        }

        /// <summary>
        /// RGB color Constructor should return expected values.
        /// </summary>
        /// <param name="rgbColor">RGB string value of the color.</param>
        /// <param name="expectedRed">The expected red component value.</param>
        /// <param name="expectedGreen">The expected green component value.</param>
        /// <param name="expectedBlue">The expected blue component value.</param>
        [TestMethod]
        [DataRow( "rgb(255, 0, 0)", 255, 0, 0 )]
        [DataRow( "rgb(0, 128, 0)", 0, 128, 0 )]
        [DataRow( "rgb(0, 0, 64)", 0, 0, 64 )]
        [DataRow( "rgb(5, 10, 15)", 5, 10, 15 )]
        public void Constructor_ValidRgbColorIsCorrect( string rgbColor, double expectedRed, double expectedGreen, double expectedBlue )
        {
            var color = new RockColor( rgbColor );

            Assert.AreEqual( expectedRed, color.R );
            Assert.AreEqual( expectedGreen, color.G );
            Assert.AreEqual( expectedBlue, color.B );
            Assert.AreEqual( 1, color.Alpha );
        }

        /// <summary>
        /// RGB color Constructor should return black for invalid values.
        /// </summary>
        /// <param name="rgbColor">Name of the color.</param>
        [TestMethod]
        [DataRow( "rgb(255)" )]
        [DataRow( "rgb(255, 255)" )]
        [DataRow( "rgb(255, 255, 255, 255" )]
        public void Constructor_InvalidRgbColorIsBlack( string rgbColor )
        {
            var color = new RockColor( rgbColor );

            Assert.AreEqual( 0, color.R );
            Assert.AreEqual( 0, color.G );
            Assert.AreEqual( 0, color.B );
            Assert.AreEqual( 1, color.Alpha );
        }

        /// <summary>
        /// RGB color Constructor should normalize values.
        /// </summary>
        /// <param name="rgbColor">RGB string value of the color.</param>
        /// <param name="expectedRed">The expected red component value.</param>
        /// <param name="expectedGreen">The expected green component value.</param>
        /// <param name="expectedBlue">The expected blue component value.</param>
        [TestMethod]
        [DataRow( "rgb(500, -23, 0)", 255, 0, 0 )]
        public void Constructor_RgbNormalizesValues( string rgbColor, double expectedRed, double expectedGreen, double expectedBlue )
        {
            var color = new RockColor( rgbColor );

            Assert.AreEqual( expectedRed, color.R );
            Assert.AreEqual( expectedGreen, color.G );
            Assert.AreEqual( expectedBlue, color.B );
            Assert.AreEqual( 1, color.Alpha );
        }

        /// <summary>
        /// RGB color Constructor should handle whitespace.
        /// </summary>
        /// <param name="rgbColor">RGB string value of the color.</param>
        /// <param name="expectedRed">The expected red component value.</param>
        /// <param name="expectedGreen">The expected green component value.</param>
        /// <param name="expectedBlue">The expected blue component value.</param>
        [TestMethod]
        [DataRow( "rgb(1,2,3)", 1, 2, 3 )]
        [DataRow( "rgb(  1,2,3)", 1, 2, 3 )]
        [DataRow( "rgb(1,2,3  )", 1, 2, 3 )]
        [DataRow( "rgb(1,  2  ,3)", 1, 2, 3 )]
        public void Constructor_RgbHandlesWhitespace( string rgbColor, double expectedRed, double expectedGreen, double expectedBlue )
        {
            var color = new RockColor( rgbColor );

            Assert.AreEqual( expectedRed, color.R );
            Assert.AreEqual( expectedGreen, color.G );
            Assert.AreEqual( expectedBlue, color.B );
            Assert.AreEqual( 1, color.Alpha );
        }

        #endregion

        #region UpdateHslFromRgb

        /// <summary>
        /// UpdateHslFromRgb method should calculate the HSL values from RGB correctly.
        /// </summary>
        /// <param name="hexColor">Color of the hexadecimal.</param>
        /// <param name="hue">The expected hue.</param>
        /// <param name="saturation">The expected saturation.</param>
        /// <param name="luminosity">The expected luminosity.</param>
        [TestMethod]
        [DataRow( "#ee7725", 24.48, 0.86, 0.54 )]
        [DataRow( "#346137", 124, 0.30, 0.29 )]
        public void UpdateHslFromRgb_CalculatesCorrectly( string hexColor, double hue, double saturation, double luminosity )
        {
            var color = new RockColor( hexColor );

            Assert.AreEqual( hue, Math.Round( color.Hue, 2 ) );
            Assert.AreEqual( saturation, Math.Round( color.Saturation, 2 ) );
            Assert.AreEqual( luminosity, Math.Round( color.Luminosity, 2 ) );
        }

        #endregion

        #region UpdateRgbFromHsl

        /// <summary>
        /// UpdateRgbFromHsl method should calculate the RGB values from HSL correctly.
        /// </summary>
        /// <param name="hue">The expected hue.</param>
        /// <param name="saturation">The expected saturation.</param>
        /// <param name="luminosity">The expected luminosity.</param>
        /// <param name="hexColor">Color of the hexadecimal.</param>
        /// <param name="expectedRed">The expected red component value.</param>
        /// <param name="expectedGreen">The expected green component value.</param>
        /// <param name="expectedBlue">The expected blue component value.</param>
        [TestMethod]
        [DataRow( 24, 0.86, 0.54, 239, 118, 37 )]
        [DataRow( 124, 0.30, 0.29, 52, 96, 55 )]
        public void UpdateRgbFromHsl_CalculatesCorrectly( double hue, double saturation, double luminosity, double expectedRed, double expectedGreen, double expectedBlue )
        {
            var color = new RockColor( "#000000" )
            {
                Hue = hue,
                Saturation = saturation,
                Luminosity = luminosity
            };

            Assert.AreEqual( expectedRed, color.R );
            Assert.AreEqual( expectedGreen, color.G );
            Assert.AreEqual( expectedBlue, color.B );
        }

        #endregion

        #region Luma

        /// <summary>
        /// Validates that the Luma calculation is what it should be.
        /// </summary>
        /// <param name="hexColor">The color as a hex string.</param>
        /// <param name="expectedLuma">The expected luma value.</param>
        [TestMethod]
        [DataRow( "#ee7725", 0.32 )]
        [DataRow( "#245678", 0.08 )]
        [DataRow( "#cccccc", 0.60 )]
        public void Luma_CalculationIsCorrect( string hexColor, double expectedLuma )
        {
            var color = new RockColor( hexColor );

            Assert.AreEqual( expectedLuma, Math.Round( color.Luma, 2 ) );
        }

        #endregion

        #region Hue

        /// <summary>
        /// Validates the Hue property correctly wraps the value when setting
        /// a value over 360.
        /// </summary>
        [TestMethod]
        public void Hue_HugeValueWrapsCorrectly()
        {
            var color = new RockColor( 255, 255, 255 )
            {
                Hue = ( 360 * 4 ) + 30
            };

            Assert.AreEqual( 30, color.Hue );
        }

        /// <summary>
        /// Validates the Hue property correctly wraps the value when setting
        /// a value under 0.
        /// </summary>
        [TestMethod]
        public void Hue_NegativeValueWrapsCorrectly()
        {
            var color = new RockColor( 255, 255, 255 )
            {
                Hue = -30
            };

            Assert.AreEqual( 360 - 30, color.Hue );
        }

        #endregion

        #region ToHex

        /// <summary>
        /// Validates that the output of the ToHex method is correct.
        /// </summary>
        /// <param name="red">The red component value.</param>
        /// <param name="green">The green component value.</param>
        /// <param name="blue">The blue component value.</param>
        /// <param name="alpha">The alpha component value.</param>
        /// <param name="expectedHex">The expected hexadecimal.</param>
        [TestMethod]
        [DataRow( 255, 0, 0, 1, "#ff0000" )]
        [DataRow( 0, 128, 0, 1, "#008000" )]
        [DataRow( 0, 0, 64, 0.5, "#00004080" )]
        public void ToHex_GeneratesExpectedText( int red, int green, int blue, double alpha, string expectedHex )
        {
            var color = new RockColor( red, green, blue, alpha );

            Assert.AreEqual( expectedHex, color.ToHex() );
        }

        #endregion

        #region ToRGBA

        /// <summary>
        /// Validates that the ToRGBA method procudes the correct format.
        /// </summary>
        [TestMethod]
        public void ToRGBA_CorrectlyFormatsColor()
        {
            var color = new RockColor( 100.2, 255, 0, 0.5 );

            Assert.AreEqual( "rgba(100, 255, 0, 0.5)", color.ToRGBA() );
        }

        #endregion

        #region IsLight

        /// <summary>
        /// Validates that the IsLight property returns true for white.
        /// </summary>
        [TestMethod]
        public void IsLight_WhiteReturnsTrue()
        {
            var color = new RockColor( 255, 255, 255 );

            Assert.AreEqual( true, color.IsLight );
        }

        /// <summary>
        /// Validates that the IsLight property returns false for black.
        /// </summary>
        [TestMethod]
        public void IsLight_BlackReturnsFalse()
        {
            var color = new RockColor( 0, 0, 0 );

            Assert.AreEqual( false, color.IsLight );
        }

        #endregion

        #region IsDark

        /// <summary>
        /// Validates that the IsDark property returns true for black.
        /// </summary>
        [TestMethod]
        public void IsDark_BlackReturnsTrue()
        {
            var color = new RockColor( 0, 0, 0 );

            Assert.AreEqual( true, color.IsDark );
        }

        /// <summary>
        /// Validates that the IsDark property returns false for white.
        /// </summary>
        [TestMethod]
        public void IsDark_WhiteReturnsFalse()
        {
            var color = new RockColor( 255, 255, 255 );

            Assert.AreEqual( false, color.IsDark );
        }

        #endregion

        #region CompareTo

        /// <summary>
        /// Validates that the CompareTo method returns 0 when two colors are equal.
        /// </summary>
        [TestMethod]
        public void CompareTo_ReturnsZeroWhenEqual()
        {
            var color1 = new RockColor( 255, 0, 0 );
            var color2 = new RockColor( 255, 0, 0 );

            Assert.AreEqual( 0, color1.CompareTo( color2 ) );
        }

        /// <summary>
        /// Validates that the CompareTo method returns <c>-1</c> when the first
        /// color has higher RGBA values than the second color.
        /// </summary>
        [TestMethod]
        public void CompareTo_ReturnsNegativeOneWhenDarker()
        {
            var color1 = new RockColor( 0, 32, 0 );
            var color2 = new RockColor( 255, 0, 0 );

            Assert.AreEqual( -1, color1.CompareTo( color2 ) );
        }

        /// <summary>
        /// Validates that the CompareTo method returns <c>-1</c> when the first
        /// color has lower RGBA values than the second color.
        /// </summary>
        [TestMethod]
        public void CompareTo_ReturnsOneWhenLighter()
        {
            var color1 = new RockColor( 0, 0, 255 );
            var color2 = new RockColor( 32, 0, 0 );

            Assert.AreEqual( 1, color1.CompareTo( color2 ) );
        }

        #endregion

        #region Lighten

        /// <summary>
        /// Validates that the Lighten method increases the lumonsity
        /// by the expected amount.
        /// </summary>
        [TestMethod]
        public void Lighten_TenIncreasesLuminosityByPointOne()
        {
            var color = new RockColor( 128, 128, 128 );
            var oldLuminosity = color.Luminosity;

            color.Lighten( 10 );

            Assert.AreEqual( oldLuminosity + 0.1, color.Luminosity );
        }

        /// <summary>
        /// Validates that the Lighten method decreases the lumonsity
        /// by the expected amount.
        /// </summary>
        [TestMethod]
        public void Lighten_NegativeTenDecreasesLuminosityByPointOne()
        {
            var color = new RockColor( 128, 128, 128 );
            var oldLuminosity = color.Luminosity;

            color.Lighten( -10 );

            Assert.AreEqual( oldLuminosity - 0.1, color.Luminosity );
        }

        #endregion

        #region Darken

        /// <summary>
        /// Validates that the Darken method decreases the lumonsity
        /// by the expected amount.
        /// </summary>
        [TestMethod]
        public void Darken_TenDecreasesLuminosityByPointOne()
        {
            var color = new RockColor( 128, 128, 128 );
            var oldLuminosity = color.Luminosity;

            color.Darken( 10 );

            Assert.AreEqual( oldLuminosity - 0.1, color.Luminosity );
        }

        /// <summary>
        /// Validates that the Darken method increases the lumonsity
        /// by the expected amount.
        /// </summary>
        [TestMethod]
        public void Darken_NegativeTenIncreasesLuminosityByPointOne()
        {
            var color = new RockColor( 128, 128, 128 );
            var oldLuminosity = color.Luminosity;

            color.Darken( -10 );

            Assert.AreEqual( oldLuminosity + 0.1, color.Luminosity );
        }

        #endregion

        #region Saturate

        /// <summary>
        /// Validates that the Saturate method increases the saturation
        /// by the expected amount.
        /// </summary>
        [TestMethod]
        public void Saturate_TenIncreasesSaturationByPointOne()
        {
            var color = new RockColor( 64, 64, 96 );
            var oldSaturation = color.Saturation;

            color.Saturate( 10 );

            Assert.AreEqual( oldSaturation + 0.1, color.Saturation );
        }

        /// <summary>
        /// Validates that the Saturate method decreases the saturation
        /// by the expected amount.
        /// </summary>
        [TestMethod]
        public void Saturate_NegativeTenDecreasesSaturationByPointOne()
        {
            var color = new RockColor( 64, 64, 96 );
            var oldSaturation = color.Saturation;

            color.Saturate( -10 );

            Assert.AreEqual( oldSaturation - 0.1, color.Saturation );
        }

        #endregion

        #region Desaturate

        /// <summary>
        /// Validates that the Desaturate method decreases the saturation
        /// by the expected amount.
        /// </summary>
        [TestMethod]
        public void Desaturate_TenDecreasesSaturationByPointOne()
        {
            var color = new RockColor( 64, 64, 96 );
            var oldSaturation = color.Saturation;

            color.Desaturate( 10 );

            Assert.AreEqual( oldSaturation - 0.1, color.Saturation );
        }

        /// <summary>
        /// Validates that the Desaturate method increases the saturation
        /// by the expected amount.
        /// </summary>
        [TestMethod]
        public void Desaturate_NegativeTenIncreasesSaturationByPointOne()
        {
            var color = new RockColor( 64, 64, 96 );
            var oldSaturation = color.Saturation;

            color.Desaturate( -10 );

            Assert.AreEqual( oldSaturation + 0.1, color.Saturation );
        }

        #endregion

        #region FadeIn

        /// <summary>
        /// Validates that the FadeIn method increases the alpha
        /// by the expected amount.
        /// </summary>
        [TestMethod]
        public void FadeIn_TenIncreasesAlphaByPointOne()
        {
            var color = new RockColor( 128, 128, 128, 0.5 );
            var oldAlpha = color.Alpha;

            color.FadeIn( 10 );

            Assert.AreEqual( oldAlpha + 0.1, color.Alpha );
        }

        /// <summary>
        /// Validates that the FadeIn method decreases the alpha
        /// by the expected amount.
        /// </summary>
        [TestMethod]
        public void FadeIn_NegativeTenDecreasesAlphaByPointOne()
        {
            var color = new RockColor( 128, 128, 128, 0.5 );
            var oldAlpha = color.Alpha;

            color.FadeIn( -10 );

            Assert.AreEqual( oldAlpha - 0.1, color.Alpha );
        }

        #endregion

        #region FadeOut

        /// <summary>
        /// Validates that the FadeOut method decreases the alpha
        /// by the expected amount.
        /// </summary>
        [TestMethod]
        public void FadeOut_TenDecreasesAlphaByPointOne()
        {
            var color = new RockColor( 128, 128, 128, 0.5 );
            var oldAlpha = color.Alpha;

            color.FadeOut( 10 );

            Assert.AreEqual( oldAlpha - 0.1, color.Alpha );
        }

        /// <summary>
        /// Validates that the FadeOut method increases the alpha
        /// by the expected amount.
        /// </summary>
        [TestMethod]
        public void FadeOut_NegativeTenIncreasesAlphaByPointOne()
        {
            var color = new RockColor( 128, 128, 128, 0.5 );
            var oldAlpha = color.Alpha;

            color.FadeOut( -10 );

            Assert.AreEqual( oldAlpha + 0.1, color.Alpha );
        }

        #endregion

        #region AdjustHueByPercent

        /// <summary>
        /// Validates that the AdjustHueByPercent method increases the hue
        /// by the expected amount.
        /// </summary>
        [TestMethod]
        public void AdjustHueByPercent_TenIncreasesHueByThirtySixDegrees()
        {
            var color = new RockColor( 128, 128, 0 );
            var oldHue = color.Hue;

            color.AdjustHueByPercent( 10 );

            Assert.AreEqual( oldHue + 36, color.Hue );
        }

        /// <summary>
        /// Validates that the AdjustHueByPercent method decreases the hue
        /// by the expected amount.
        /// </summary>
        [TestMethod]
        public void AdjustHueByPercent_NegativeTenDecreasesHueByThirtySixDegrees()
        {
            var color = new RockColor( 128, 128, 0 );
            var oldHue = color.Hue;

            color.AdjustHueByPercent( -10 );

            Assert.AreEqual( oldHue - 36, color.Hue );
        }

        #endregion

        #region AdjustHueByDegrees

        /// <summary>
        /// Validates that the AdjustHueByPercent method increases the hue
        /// by the expected amount.
        /// </summary>
        [TestMethod]
        public void AdjustHueByPercent_TenIncreasesHueByTenDegrees()
        {
            var color = new RockColor( 128, 128, 0 );
            var oldHue = color.Hue;

            color.AdjustHueByDegrees( 10 );

            Assert.AreEqual( oldHue + 10, color.Hue );
        }

        /// <summary>
        /// Validates that the AdjustHueByPercent method decreases the hue
        /// by the expected amount.
        /// </summary>
        [TestMethod]
        public void AdjustHueByPercent_NegativeTenDecreasesHueByTenDegrees()
        {
            var color = new RockColor( 128, 128, 0 );
            var oldHue = color.Hue;

            color.AdjustHueByDegrees( -10 );

            Assert.AreEqual( oldHue - 10, color.Hue );
        }

        #endregion

        #region Tint

        /// <summary>
        /// Validates that the Tint method on a red color produces the expected
        /// tinted color.
        /// </summary>
        [TestMethod]
        public void Tint_RedFiftyPercentProducesExpectedColor()
        {
            var color = new RockColor( 255, 0, 0 );

            color.Tint( 50 );

            Assert.AreEqual( 255, color.R );
            Assert.AreEqual( 127.5, color.G );
            Assert.AreEqual( 127.5, color.B );
        }

        #endregion

        #region Shade

        /// <summary>
        /// Validates that the Shade method on a red color produces the expected
        /// tinted color.
        /// </summary>
        [TestMethod]
        public void Shade_RedFiftyPercentProducesExpectedColor()
        {
            var color = new RockColor( 255, 0, 0 );

            color.Shade( 50 );

            Assert.AreEqual( 127.5, color.R );
            Assert.AreEqual( 0, color.G );
            Assert.AreEqual( 0, color.B );
        }

        #endregion

        #region Grayscale

        /// <summary>
        /// Validates that the Grayscale method only changes saturation.
        /// </summary>
        [TestMethod]
        public void Grayscale_OnlyChangesSaturation()
        {
            var expectedColor = new RockColor( "#ee7725" );
            var color = new RockColor( "#ee7725" );

            color.Grayscale();

            Assert.AreEqual( expectedColor.Hue, color.Hue );
            Assert.AreEqual( 0, color.Saturation );
            Assert.AreEqual( expectedColor.Luminosity, color.Luminosity );
        }

        #endregion

        #region Clone

        /// <summary>
        /// Validates that the Clone method produces a new color instance
        /// that has the same RGBA component values as the original.
        /// </summary>
        [TestMethod]
        public void Clone_ProducesIdenticalColor()
        {
            var expectedColor = new RockColor( "#ee7725" )
            {
                Alpha = 0.25
            };

            var color = expectedColor.Clone();

            Assert.AreEqual( expectedColor.R, color.R );
            Assert.AreEqual( expectedColor.G, color.G );
            Assert.AreEqual( expectedColor.B, color.B );
            Assert.AreEqual( expectedColor.Alpha, color.Alpha );
            Assert.IsFalse( ReferenceEquals( color, expectedColor ) );
        }

        #endregion

        #region CalculateContrastRatio

        /// <summary>
        /// Validates that the CalculateContrastRatio produces the correct values
        /// that match the W3C specifications.
        /// </summary>
        /// <param name="hexForeground">The hexadecimal foreground color.</param>
        /// <param name="hexBackground">The hexadecimal background color.</param>
        /// <param name="expectedRatio">The expected ratio.</param>
        [TestMethod]
        [DataRow( "#404040", "#a8a8a8", 4.36 )]
        [DataRow( "#0000ff", "#ffffff", 8.59 )]
        [DataRow( "#8a8aff", "#ffffff", 2.93 )]
        public void CalculateContrastRatio_ProducesCorrectValue( string hexForeground, string hexBackground, double expectedRatio )
        {
            var foregroundColor = new RockColor( hexForeground );
            var backgroundColor = new RockColor( hexBackground );

            var ratio = RockColor.CalculateContrastRatio( backgroundColor, foregroundColor );

            Assert.AreEqual( expectedRatio, Math.Round( ratio, 2 ) );
        }

        /// <summary>
        /// Validates the the CalculateConstrastRatio produces the same value
        /// regardless of the parameter order.
        /// </summary>
        [TestMethod]
        public void CalculateContrastRatio_OrderDoesNotMatter()
        {
            var color1 = new RockColor( "#000000" );
            var color2 = new RockColor( "#ffffff" );

            var ratio1 = RockColor.CalculateContrastRatio( color1, color2 );
            var ratio2 = RockColor.CalculateContrastRatio( color2, color1 );

            Assert.AreEqual( ratio1, ratio2 );
        }

        #endregion

        #region CalculateColorPair

        /// <summary>
        /// Validates that the CalculateColorPair method produces the correct
        /// paired colors for the input color.
        /// </summary>
        /// <param name="hexColor">Color of the hexadecimal.</param>
        /// <param name="expectedForegroundHex">The expected foreground hexadecimal.</param>
        /// <param name="expectedBackgroundHex">The expected background hexadecimal.</param>
        [TestMethod]
        [DataRow( "#219ff3", "#143952", "#c1e4fb" )]
        [DataRow( "#4caf50", "#145217", "#c1fbc3" )]
        [DataRow( "#cd2bba", "#52144a", "#fbc1f4" )]
        [DataRow( "#cdb6b6", "#521414", "#fbc1c1" )]
        [DataRow( "#8f5252", "#521414", "#fbc1c1" )]
        [DataRow( "#ffffff", "#333333", "#dedede" )]
        public void CalculateColorPair_ProducesCorrectColors( string hexColor, string expectedForegroundHex, string expectedBackgroundHex )
        {
            var color = new RockColor( hexColor );
            var pair = RockColor.CalculateColorPair( color );

            Assert.AreEqual( expectedForegroundHex, pair.ForegroundColor.ToHex() );
            Assert.AreEqual( expectedBackgroundHex, pair.BackgroundColor.ToHex() );
        }

        #endregion

        #region CalculateColorRecipe

        /// <summary>
        /// Validates that the CalculateColorRecipe method produces the expected
        /// colors.
        /// </summary>
        /// <param name="hexColor">Color of the hexadecimal.</param>
        /// <param name="recipe">The recipe.</param>
        /// <param name="expectedHexColor">Expected color of the hexadecimal.</param>
        [TestMethod]
        [DataRow( "#219ff3", ColorRecipe.Lightest, "#c1e4fb" )]
        [DataRow( "#219ff3", ColorRecipe.Light, "#f1f3f4" )]
        [DataRow( "#219ff3", ColorRecipe.Medium, "#97acba" )]
        [DataRow( "#219ff3", ColorRecipe.Dark, "#507a95" )]
        [DataRow( "#219ff3", ColorRecipe.Darkest, "#143952" )]
        [DataRow( "#219ff3", ColorRecipe.Primary, "#a8d3f0" )]
        public void CalculateColorRecipe_ProducesCorrectColor( string hexColor, ColorRecipe recipe, string expectedHexColor )
        {
            var color = new RockColor( hexColor );
            var recipeColor = RockColor.CalculateColorRecipe( color, recipe );

            Assert.AreEqual( expectedHexColor, recipeColor.ToHex() );
        }

        #endregion
    }
}
