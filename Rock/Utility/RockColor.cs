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
using System.Globalization;
using System.Linq;

namespace Rock.Utility
{
    /// <summary>
    /// A utility class for color manipulations
    /// </summary>
    /// 9/30/2018 - JME
    /// A good piece of this class came from the color class of dotLess. Several other
    /// resources were used to create the algorthms used. For instance the conversion
    /// between HSL and RGB. Becuase of that it is a bit of frankencode, but it is
    /// working and I did put in quite a bit of effort to clean up the code to our
    /// standards and document it as best as possible.
    public class RockColor 
    {
        #region Private Members
        private static readonly Dictionary<string, int> Html4Colors = new Dictionary<string, int>
    {
        {"aliceblue", 0xf0f8ff},
        {"antiquewhite", 0xfaebd7},
        {"aqua", 0x00ffff},
        {"aquamarine", 0x7fffd4},
        {"azure", 0xf0ffff},
        {"beige", 0xf5f5dc},
        {"bisque", 0xffe4c4},
        {"black", 0x000000},
        {"blanchedalmond", 0xffebcd},
        {"blue", 0x0000ff},
        {"blueviolet", 0x8a2be2},
        {"brown", 0xa52a2a},
        {"burlywood", 0xdeb887},
        {"cadetblue", 0x5f9ea0},
        {"chartreuse", 0x7fff00},
        {"chocolate", 0xd2691e},
        {"coral", 0xff7f50},
        {"cornflowerblue", 0x6495ed},
        {"cornsilk", 0xfff8dc},
        {"crimson", 0xdc143c},
        {"cyan", 0x00ffff},
        {"darkblue", 0x00008b},
        {"darkcyan", 0x008b8b},
        {"darkgoldenrod", 0xb8860b},
        {"darkgray", 0xa9a9a9},
        {"darkgrey", 0xa9a9a9},
        {"darkgreen", 0x006400},
        {"darkkhaki", 0xbdb76b},
        {"darkmagenta", 0x8b008b},
        {"darkolivegreen", 0x556b2f},
        {"darkorange", 0xff8c00},
        {"darkorchid", 0x9932cc},
        {"darkred", 0x8b0000},
        {"darksalmon", 0xe9967a},
        {"darkseagreen", 0x8fbc8f},
        {"darkslateblue", 0x483d8b},
        {"darkslategray", 0x2f4f4f},
        {"darkslategrey", 0x2f4f4f},
        {"darkturquoise", 0x00ced1},
        {"darkviolet", 0x9400d3},
        {"deeppink", 0xff1493},
        {"deepskyblue", 0x00bfff},
        {"dimgray", 0x696969},
        {"dimgrey", 0x696969},
        {"dodgerblue", 0x1e90ff},
        {"firebrick", 0xb22222},
        {"floralwhite", 0xfffaf0},
        {"forestgreen", 0x228b22},
        {"fuchsia", 0xff00ff},
        {"gainsboro", 0xdcdcdc},
        {"ghostwhite", 0xf8f8ff},
        {"gold", 0xffd700},
        {"goldenrod", 0xdaa520},
        {"gray", 0x808080},
        {"grey", 0x808080},
        {"green", 0x008000},
        {"greenyellow", 0xadff2f},
        {"honeydew", 0xf0fff0},
        {"hotpink", 0xff69b4},
        {"indianred", 0xcd5c5c},
        {"indigo", 0x4b0082},
        {"ivory", 0xfffff0},
        {"khaki", 0xf0e68c},
        {"lavender", 0xe6e6fa},
        {"lavenderblush", 0xfff0f5},
        {"lawngreen", 0x7cfc00},
        {"lemonchiffon", 0xfffacd},
        {"lightblue", 0xadd8e6},
        {"lightcoral", 0xf08080},
        {"lightcyan", 0xe0ffff},
        {"lightgoldenrodyellow", 0xfafad2},
        {"lightgray", 0xd3d3d3},
        {"lightgrey", 0xd3d3d3},
        {"lightgreen", 0x90ee90},
        {"lightpink", 0xffb6c1},
        {"lightsalmon", 0xffa07a},
        {"lightseagreen", 0x20b2aa},
        {"lightskyblue", 0x87cefa},
        {"lightslategray", 0x778899},
        {"lightslategrey", 0x778899},
        {"lightsteelblue", 0xb0c4de},
        {"lightyellow", 0xffffe0},
        {"lime", 0x00ff00},
        {"limegreen", 0x32cd32},
        {"linen", 0xfaf0e6},
        {"magenta", 0xff00ff},
        {"maroon", 0x800000},
        {"mediumaquamarine", 0x66cdaa},
        {"mediumblue", 0x0000cd},
        {"mediumorchid", 0xba55d3},
        {"mediumpurple", 0x9370d8},
        {"mediumseagreen", 0x3cb371},
        {"mediumslateblue", 0x7b68ee},
        {"mediumspringgreen", 0x00fa9a},
        {"mediumturquoise", 0x48d1cc},
        {"mediumvioletred", 0xc71585},
        {"midnightblue", 0x191970},
        {"mintcream", 0xf5fffa},
        {"mistyrose", 0xffe4e1},
        {"moccasin", 0xffe4b5},
        {"navajowhite", 0xffdead},
        {"navy", 0x000080},
        {"oldlace", 0xfdf5e6},
        {"olive", 0x808000},
        {"olivedrab", 0x6b8e23},
        {"orange", 0xffa500},
        {"orangered", 0xff4500},
        {"orchid", 0xda70d6},
        {"palegoldenrod", 0xeee8aa},
        {"palegreen", 0x98fb98},
        {"paleturquoise", 0xafeeee},
        {"palevioletred", 0xd87093},
        {"papayawhip", 0xffefd5},
        {"peachpuff", 0xffdab9},
        {"peru", 0xcd853f},
        {"pink", 0xffc0cb},
        {"plum", 0xdda0dd},
        {"powderblue", 0xb0e0e6},
        {"purple", 0x800080},
        {"red", 0xff0000},
        {"rosybrown", 0xbc8f8f},
        {"royalblue", 0x4169e1},
        {"saddlebrown", 0x8b4513},
        {"salmon", 0xfa8072},
        {"sandybrown", 0xf4a460},
        {"seagreen", 0x2e8b57},
        {"seashell", 0xfff5ee},
        {"sienna", 0xa0522d},
        {"silver", 0xc0c0c0},
        {"skyblue", 0x87ceeb},
        {"slateblue", 0x6a5acd},
        {"slategray", 0x708090},
        {"slategrey", 0x708090},
        {"snow", 0xfffafa},
        {"springgreen", 0x00ff7f},
        {"steelblue", 0x4682b4},
        {"tan", 0xd2b48c},
        {"teal", 0x008080},
        {"thistle", 0xd8bfd8},
        {"tomato", 0xff6347},
        {"turquoise", 0x40e0d0},
        {"violet", 0xee82ee},
        {"wheat", 0xf5deb3},
        {"white", 0xffffff},
        {"whitesmoke", 0xf5f5f5},
        {"yellow", 0xffff00},
        {"yellowgreen", 0x9acd32}
    };

        private static readonly Dictionary<int, string> Html4ColorsReverse =
            Html4Colors.GroupBy( kvp => kvp.Value ).ToDictionary( g => g.Key, g => g.First().Key );

        private string _text;
        #endregion

        #region Static Methods
        /// <summary>
        /// Gets a color from the specified keyword or hexadecimal.
        /// </summary>
        /// <param name="keywordOrHex">The keyword or hexadecimal.</param>
        /// <returns>Rock Color</returns>
        public static RockColor From( string keywordOrHex )
        {
            return GetColorFromKeyword( keywordOrHex ) ?? FromHex( keywordOrHex );
        }

        /// <summary>
        /// Gets the color from keyword.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <returns>Rock Color</returns>
        public static RockColor GetColorFromKeyword( string keyword )
        {
            if ( keyword == "transparent" )
            {
                return new RockColor( 0, 0, 0, 0.0, keyword );
            }

            int rgb;
            if ( Html4Colors.TryGetValue( keyword, out rgb ) )
            {
                var r = ( rgb >> 16 ) & 0xFF;
                var g = ( rgb >> 8 ) & 0xFF;
                var b = rgb & 0xFF;
                return new RockColor( r, g, b, 1.0, keyword );
            }

            return null;
        }

        /// <summary>
        /// Gets the keyword from the provided RGB value.
        /// </summary>
        /// <param name="rgb">The RGB.</param>
        /// <returns></returns>
        public static string GetKeyword( int[] rgb )
        {
            var color = ( rgb[0] << 16 ) + ( rgb[1] << 8 ) + rgb[2];

            string keyword;
            if ( Html4ColorsReverse.TryGetValue( color, out keyword ) )
            {
                return keyword;
            }

            return null;
        }
        
        /// <summary>
        /// Gets Rock Color from the provided hexadecimal.
        /// </summary>
        /// <param name="hex">The hexadecimal.</param>
        /// <returns></returns>
        public static RockColor FromHex( string hex )
        {
            hex = hex.TrimStart( '#' );
            double[] rgb;
            var alpha = 1.0;
            var text = '#' + hex;

            if ( hex.Length == 8 )
            {
                rgb = ParseRgb( hex.Substring( 2 ) );
                alpha = Parse( hex.Substring( 0, 2 ) ) / 255.0;
            }
            else if ( hex.Length == 6 )
            {
                rgb = ParseRgb( hex );
            }
            else
            {
                rgb = hex.ToCharArray().Select( c => Parse( "" + c + c ) ).ToArray();
            }

            return new RockColor( rgb, alpha, text );
        }
        #endregion 

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RockColor"/> class.
        /// </summary>
        /// <param name="color">The color as an integer.</param>
        public RockColor( int color )
        {
            RGB = new double[3];

            this.B = color & 0xff;
            color >>= 8;
            this.G = color & 0xff;
            color >>= 8;
            this.R = color & 0xff;
            this.Alpha = 1;

            UpdateHslFromRgb();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockColor" /> class.
        /// </summary>
        /// <param name="color">The color in hexidecimal or rgba format.</param>
        public RockColor( string color )
        {
            // #ee7625
            if ( color.StartsWith( "#" ) )
            {
                ParseHexString( color );
                return;
            }

            // rgba(255, 99, 71, 0.5)
            if ( color.StartsWith( "rgba" ) )
            {
                color = color.Replace(" ","").Replace( "rgba(", "" ).Replace( ")", "" );
                var parts = color.Split( ',' );
                if (parts.Length == 4 )
                {
                    this.R = parts[0].Trim().AsDouble();
                    this.G = parts[1].Trim().AsDouble();
                    this.B = parts[2].Trim().AsDouble();
                    this.Alpha = parts[3].Trim().AsDouble();
                }
            }

            // rgb(255, 99, 71)
            if ( color.StartsWith( "rgb" ) )
            {
                color = color.Replace( " ", "" ).Replace( "rgb(", "" ).Replace( ")", "" );

                var parts = color.Split( ',' );
                if ( parts.Length == 3 )
                {
                    this.R = parts[0].Trim().AsDouble();
                    this.G = parts[1].Trim().AsDouble();
                    this.B = parts[2].Trim().AsDouble();
                }
            }

            // Othewise assume that the color is a named color 'blue'
            var namedColor = RockColor.GetColorFromKeyword( color );

            if ( namedColor != null )
            {
                this.R = namedColor.R;
                this.G = namedColor.G;
                this.B = namedColor.B;
                this.Alpha = namedColor.Alpha; // For the case of 'transparent'
            }

            UpdateHslFromRgb();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockColor"/> class.
        /// </summary>
        /// <param name="r">The Red value.</param>
        /// <param name="g">The Green value.</param>
        /// <param name="b">The Blue value.</param>
        public RockColor( double r, double g, double b ) : this( r, g, b, 1 )
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockColor"/> class.
        /// </summary>
        /// <param name="r">The Red value.</param>
        /// <param name="g">The Green value.</param>
        /// <param name="b">The Blue value.</param>
        /// <param name="alpha">The Alpha channel value.</param>
        public RockColor( double r, double g, double b, double alpha ) : this( new[] { r, g, b }, alpha )
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockColor"/> class.
        /// </summary>
        /// <param name="rgb">The RGB components as an array.</param>
        public RockColor( double[] rgb ) : this( rgb, 1 )
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockColor"/> class.
        /// </summary>
        /// <param name="rgb">The RGB components as an array.</param>
        /// <param name="alpha">The Alpha channel value.</param>
        public RockColor( double[] rgb, double alpha ) : this( rgb, alpha, null )
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockColor"/> class.
        /// </summary>
        /// <param name="rgb">The RGB components as an array.</param>
        /// <param name="alpha">The Alpha channel value.</param>
        /// <param name="text">The name for the color.</param>
        public RockColor( double[] rgb, double alpha, string text )
        {
            RGB = rgb.Select( c => this.Normalize( c, 255.0 ) ).ToArray();
            Alpha = this.Normalize( alpha, 1.0 );
            _text = text;

            UpdateHslFromRgb();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockColor" /> class.
        /// </summary>
        /// <param name="red">The Red value.</param>
        /// <param name="green">The Green value.</param>
        /// <param name="blue">The Blue value.</param>
        /// <param name="alpha">The Alpha channel value.</param>
        /// <param name="text">The name for the color.</param>
        public RockColor( double red, double green, double blue, double alpha = 1.0, string text = null )
            : this( new[] { red, green, blue }, alpha, text )
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// The RGB value
        /// </summary>
        public readonly double[] RGB = new double[3];

        private double _alpha = 1;

        private double _hue;
        private double _saturation;
        private double _luminosity;

        /// <summary>
        /// Gets or sets the alpha level.
        /// </summary>
        /// <value>
        /// The alpha level.
        /// </value>
        public double Alpha
        {
            get
            {
                return Normalize( _alpha, 1 );
            }
            set
            {
                if ( value > 100 )
                {
                    _alpha = 100;
                    return;
                }

                if ( value < 0 )
                {
                    _alpha = 0;
                    return;
                }

                _alpha = value;
            }
        }

        /// <summary>
        /// Gets or sets the Red value.
        /// </summary>
        /// <value>
        /// The r.
        /// </value>
        public double R
        {
            get { return RGB[0]; }
            set {
                RGB[0] = value;
                UpdateHslFromRgb();
            }
        }

        /// <summary>
        /// Gets or sets the Green value.
        /// </summary>
        /// <value>
        /// The g.
        /// </value>
        public double G
        {
            get { return RGB[1]; }
            set {
                RGB[1] = value;
                UpdateHslFromRgb();
            }
        }

        /// <summary>
        /// Gets or sets the Blue value.
        /// </summary>
        /// <value>
        /// The b.
        /// </value>
        public double B
        {
            get { return RGB[2]; }
            set {
                RGB[2] = value;
                UpdateHslFromRgb();
            }
        }

        /// <summary>
        /// Calculates the luma value based on the <a href="http://www.w3.org/TR/2008/REC-WCAG20-20081211/#relativeluminancedef">W3 Standard</a>
        /// </summary>
        /// <value>
        /// The luma value for the current color
        /// </value>
        public double Luma
        {
            get
            {
                var linearR = R / 255;
                var linearG = G / 255;
                var linearB = B / 255;

                var red = TransformLinearToSrbg( linearR );
                var green = TransformLinearToSrbg( linearG );
                var blue = TransformLinearToSrbg( linearB );

                return 0.2126 * red + 0.7152 * green + 0.0722 * blue;
            }
        }

        /// <summary>
        /// Gets or sets the hue.
        /// </summary>
        /// <value>
        /// The hue of the color.
        /// </value>
        public double Hue
        {
            get { return _hue; }
            set {
                _hue = value;

                NormalizeHslValues();
                UpdateRgbFromHsl();
            }
        }

        /// <summary>
        /// Gets or sets the saturation.
        /// </summary>
        /// <value>
        /// The saturation of the color.
        /// </value>
        public double Saturation
        {
            get { return _saturation; }
            set {
                _saturation =  value;

                NormalizeHslValues();
                UpdateRgbFromHsl();
            }
        }

        /// <summary>
        /// Gets or sets the luminosity.
        /// </summary>
        /// <value>
        /// The luminosity of the color.
        /// </value>
        public double Luminosity
        {
            get { return _luminosity; }
            set {
                _luminosity = value;

                NormalizeHslValues();
                UpdateRgbFromHsl();
            }
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Lightens the color by the provided percentage.
        /// </summary>
        /// <param name="percentage">The percentage.</param>
        public void Lighten( int percentage )
        {
            Luminosity = Luminosity + ( ( double ) percentage / ( double ) 100 );
        }

        /// <summary>
        /// Darkens the color by the provided percentage.
        /// </summary>
        /// <param name="percentage">The percentage.</param>
        public void Darken( int percentage )
        {
            Luminosity = Luminosity - ( ( double ) percentage / ( double ) 100 );
        }

        /// <summary>
        /// Saturates the color by the provided percentage.
        /// </summary>
        /// <param name="percentage">The percentage.</param>
        public void Saturate( int percentage )
        {
            Saturation = Saturation + ( ( double ) percentage / ( double ) 100 );
        }

        /// <summary>
        /// Desaturates the color by the provided percentage.
        /// </summary>
        /// <param name="percentage">The percentage.</param>
        public void Desaturate( int percentage )
        {
            Saturation = Saturation - ( ( double ) percentage / ( double ) 100 );
        }

        /// <summary>
        /// Decreases the opacity level by the given percentage. This makes the color less transparent (opaque).
        /// </summary>
        /// <param name="percentage">The percentage.</param>
        public void FadeIn( int percentage )
        {
            Alpha = _alpha + ( ( double ) percentage / ( double ) 100 );
        }

        /// <summary>
        /// Increases the opacity level by the given percentage. This makes the color more transparent.
        /// </summary>
        /// <param name="percentage">The percentage.</param>
        public void FadeOut( int percentage )
        {
            Alpha = _alpha - ( ( double ) percentage / ( double ) 100 );
        }

        /// <summary>
        /// Adjusts the hue by a the specified percentage.
        /// </summary>
        /// <param name="percentage">The percentage.</param>
        public void AdjustHueByPercent( int percentage )
        {
            Hue = _hue + ( 360 * ( ( double ) percentage / ( double ) 100 ) );
        }

        /// <summary>
        /// Adjusts the hue by a certain degree value.
        /// </summary>
        /// <param name="degrees">The degrees.</param>
        public void AdjustHueByDegrees( int degrees )
        {
            Hue = Hue + degrees;
        }

        /// <summary>
        /// Tints the specified percentage amount (mixes the color with white).
        /// </summary>
        /// <param name="percentageAmount">The percentage amount.</param>
        public void Tint( int percentageAmount )
        {
            Mix( new RockColor( "#ffffff" ), percentageAmount );
        }

        /// <summary>
        /// Shades the specified percentage amount (mixes the color with black).
        /// </summary>
        /// <param name="percentageAmount">The percentage amount.</param>
        public void Shade( int percentageAmount )
        {
            Mix( new RockColor( "#000000" ), percentageAmount );
        }

        /// <summary>
        /// Mixes the specified color into the current color with an optional percentage amount.
        /// </summary>
        /// <param name="mixColor">Color of the mix.</param>
        /// <param name="percentageAmount">The percentage amount.</param>
        public void Mix( RockColor mixColor, int percentageAmount = 50 )
        {
            var amount = ( double ) percentageAmount / ( double ) 100;

            R = ( double ) ( ( mixColor.R * amount ) + this.R * ( 1 - amount ) );
            G = ( double ) ( ( mixColor.G * amount ) + this.G * ( 1 - amount ) );
            B = ( double ) ( ( mixColor.B * amount ) + this.B * ( 1 - amount ) ); 
        }

        /// <summary>
        /// Turns the color to it's greyscale value.
        /// </summary>
        public void Grayscale()
        {
            Saturate( -100 );
        }
    #endregion

    #region Private Methods

    /// <summary>
    /// Parses the hexadecimal string to the RGB components.
    /// </summary>
    /// <param name="hex">The hexadecimal.</param>
    private void ParseHexString( string hex )
    {
        hex = hex.TrimStart( '#' );
        double[] rgb;
        var alpha = 1.0;
        var text = '#' + hex;

        if ( hex.Length == 8 )
        {
            rgb = ParseRgb( hex.Substring( 2 ) );
            alpha = Parse( hex.Substring( 0, 2 ) ) / 255.0;
        }
        else if ( hex.Length == 6 )
        {
            rgb = ParseRgb( hex );
        }
        else
        {
            rgb = hex.ToCharArray().Select( c => Parse( "" + c + c ) ).ToArray();
        }

        R = rgb[0];
        G = rgb[1];
        B = rgb[2];
        Alpha = alpha;

        _text = text;
    }

    /// <summary>
    /// Normalizes the specified value providing a max value and assuming 0 for the min.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="max">The maximum.</param>
    /// <returns></returns>
    private double Normalize( double value, double max )
    {
        return this.Normalize( value, 0d, max );
    }

    /// <summary>
    /// Normalizes the specified value using min and maxes.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="min">The minimum.</param>
    /// <param name="max">The maximum.</param>
    /// <returns></returns>
    private double Normalize( double value, double min, double max )
    {
        return value < min ? min : value > max ? max : value;
    }

    /// <summary>
    /// Updates the HSL values of the color from the RGB values.
    /// </summary>
    private void UpdateHslFromRgb()
        {
            // Convert RGB to a 0.0 to 1.0 range.
            double double_r = RGB[0] / 255.0;
            double double_g = RGB[1] / 255.0;
            double double_b = RGB[2] / 255.0;

            // Get the maximum and minimum RGB components.
            double max = double_r;
            if ( max < double_g )
                max = double_g;
            if ( max < double_b )
                max = double_b;

            double min = double_r;
            if ( min > double_g )
                min = double_g;
            if ( min > double_b )
                min = double_b;

            double diff = max - min;
            _luminosity = ( max + min ) / 2;
            if ( Math.Abs( diff ) < 0.00001 )
            {
                _saturation = 0;
                _hue = 0;  // H is really undefined.
            }
            else
            {
                if ( _luminosity <= 0.5 )
                    _saturation = diff / ( max + min );
                else
                    _saturation = diff / ( 2 - max - min );

                double r_dist = ( max - double_r ) / diff;
                double g_dist = ( max - double_g ) / diff;
                double b_dist = ( max - double_b ) / diff;

                if ( double_r == max )
                    _hue = b_dist - g_dist;
                else if ( double_g == max )
                    _hue = 2 + r_dist - b_dist;
                else
                    _hue = 4 + g_dist - r_dist;

                _hue = _hue * 60;
                if ( _hue < 0 )
                    _hue += 360;
            }

            // ensure valid values
            NormalizeHslValues();
        }

        /// <summary>
        /// Normalizes the HSL values to ensure valid values.
        /// </summary>
        private void NormalizeHslValues()
        {
            _luminosity = this.Normalize( _luminosity, 1 );
            _saturation = this.Normalize( _saturation, 1 );
            _hue = this.Normalize( _hue, 360 );
        }

        /// <summary>
        /// Updates the RGB values of the color from the HSL values.
        /// </summary>
        private void UpdateRgbFromHsl()
        {
            double p2;
            if ( _luminosity <= 0.5 )
            {
                p2 = _luminosity * ( 1 + _saturation );
            }
            else
            {
                p2 = _luminosity + _saturation - _luminosity * _saturation;
            }

            double p1 = 2 * _luminosity - p2;
            double double_r, double_g, double_b;
            if ( _saturation == 0 )
            {
                double_r = _luminosity;
                double_g = _luminosity;
                double_b = _luminosity;
            }
            else
            {
                double_r = QqhToRgb( p1, p2, _hue + 120 );
                double_g = QqhToRgb( p1, p2, _hue );
                double_b = QqhToRgb( p1, p2, _hue - 120 );
            }

            // Convert RGB to the 0 to 255 range.
            RGB[0] = ( int ) ( double_r * 255.0 );
            RGB[1] = ( int ) ( double_g * 255.0 );
            RGB[2] = ( int ) ( double_b * 255.0 );
        }

        /// <summary>
        /// QQHs to RGB helper for HSL to RGB.
        /// </summary>
        /// <param name="q1">The q1.</param>
        /// <param name="q2">The q2.</param>
        /// <param name="hue">The hue.</param>
        /// <returns></returns>
        private static double QqhToRgb( double q1, double q2, double hue )
        {
            if ( hue > 360 )
            {
                hue -= 360;
            }
            else if ( hue < 0 )
            {
                hue += 360;
            }

            if ( hue < 60 )
            {
                return q1 + ( q2 - q1 ) * hue / 60;
            }
            if ( hue < 180 )
            {
                return q2;
            }
            if ( hue < 240 )
            {
                return q1 + ( q2 - q1 ) * ( 240 - hue ) / 60;
            }
            return q1;
        }

        /// <summary>
        /// Transforms the linear to SRBG. Formula derivation decscribed <a href="http://en.wikipedia.org/wiki/SRGB#Theory_of_the_transformation">here</a>
        /// </summary>
        /// <param name="linearChannel">The linear channel, for example R/255</param>
        /// <returns>The sRBG value for the given channel</returns>
        private double TransformLinearToSrbg( double linearChannel )
        {
            const double decodingGamma = 2.4;
            const double phi = 12.92;
            const double alpha = .055;
            return ( linearChannel <= 0.03928 ) ? linearChannel / phi : Math.Pow( ( ( linearChannel + alpha ) / ( 1 + alpha ) ), decodingGamma );
        }

        /// <summary>
        /// Parses a hex string to RGB.
        /// </summary>
        /// <param name="hex">The hexadecimal.</param>
        /// <returns>Array of RGB values</returns>
        private static double[] ParseRgb( string hex )
        {
            return Enumerable.Range( 0, 3 )
                .Select( i => hex.Substring( i * 2, 2 ) )
                .Select( Parse )
                .ToArray();
        }

        /// <summary>
        /// Parses the specified hexadecimal.
        /// </summary>
        /// <param name="hex">The hexadecimal.</param>
        /// <returns></returns>
        private static double Parse( string hex )
        {
            return int.Parse( hex, NumberStyles.HexNumber );
        }

        /// <summary>
        /// Gets the hexadecimal string.
        /// </summary>
        /// <param name="rgb">The RGB.</param>
        /// <returns></returns>
        private string GetHexString( IEnumerable<int> rgb )
        {
            return '#' + rgb.Select( i => i.ToString( "x2" ) ).JoinStrings( "" );
        }

        /// <summary>
        /// Converts to int.
        /// </summary>
        /// <param name="rgb">The RGB.</param>
        /// <returns></returns>
        private List<int> ConvertToInt( IEnumerable<double> rgb )
        {
            return rgb.Select( d => ( int ) Math.Round( d, MidpointRounding.AwayFromZero ) ).ToList();
        }

        #endregion

        #region Helper Methods
        /// <summary>
        ///  Returns in the IE ARGB format e.g ##FF001122 = rgba(0x00, 0x11, 0x22, 1)
        /// </summary>
        /// <returns></returns>
        public string ToArgb()
        {
            var values = new[] { Alpha * 255 }.Concat( RGB );
            var argb = ConvertToInt( values );
            return GetHexString( argb );
        }

        /// <summary>
        /// To the rgba format (e.g. rgba( 201, 76, 76, 0.6)).
        /// </summary>
        /// <returns></returns>
        public string ToRGBA()
        {
            return $"rgba( {RGB[0]}, {RGB[1]}, {RGB[2]}, {Alpha} )";
        }

        /// <summary>
        /// Returns the hexadecimal version of the color (#ee7625).
        /// </summary>
        /// <returns>Hexadecimal version of the color.</returns>
        public string ToHex()
        {
            var argb = ConvertToInt( RGB );
            return GetHexString( argb );
        }

        /// <summary>
        /// Compares two RockColors.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public int CompareTo( object obj )
        {
            var col = obj as RockColor;

            if ( col == null )
            {
                return -1;
            }

            if ( col.R == R && col.G == G && col.B == B && col.Alpha == Alpha )
            {
                return 0;
            }

            return ( ( ( 256 * 3 ) - ( col.R + col.G + col.B ) ) * col.Alpha ) < ( ( ( 256 * 3 ) - ( R + G + B ) ) * Alpha ) ? 1 : -1;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="RockColor"/> to <see cref="System.Drawing.Color"/>.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        /// <exception cref="ArgumentNullException">color</exception>
        public static explicit operator System.Drawing.Color( RockColor color )
        {
            if ( color == null )
            {
                throw new ArgumentNullException( "color" );
            }

            return System.Drawing.Color.FromArgb( ( int ) Math.Round( color.Alpha * 255d ), ( int ) color.R, ( int ) color.G, ( int ) color.B );
        }

        #endregion
    }
    
}