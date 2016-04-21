﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Drawing;

namespace Rock
{
    /// <summary>
    ///
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Returns the number of digits following the decimal place. 5.68 would return 2. 17.9998 would return 4.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static int GetDecimalPrecision( this decimal value )
        {
            return BitConverter.GetBytes( decimal.GetBits( value )[3] )[2];
        }

        /// <summary>
        /// Changes the color brightness.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="correctionFactor">The correction factor.</param>
        /// <returns></returns>
        public static Color ChangeColorBrightness( this Color color, float correctionFactor )
        {
            float red = (float)color.R;
            float green = (float)color.G;
            float blue = (float)color.B;

            if ( correctionFactor < 0 )
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return Color.FromArgb( color.A, (int)red, (int)green, (int)blue );
        }

        /// <summary>
        /// To the HTML.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        public static string ToHtml( this Color color )
        {
            return ColorTranslator.ToHtml( color );
        }

    }
}