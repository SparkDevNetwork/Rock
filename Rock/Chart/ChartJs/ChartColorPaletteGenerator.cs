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
using System.Linq;
using Rock.Utility;

namespace Rock.Chart
{
    #region Helper Classes and Interfaces

    /// <summary>
    /// Generates a palette of colors that are suitable for use with a chart or graph.
    /// </summary>
    public class ChartColorPaletteGenerator
    {
        private static List<RockColor> _preferredChartColors = null;

        /// <summary>
        /// Gets a set of colors that offer maximum contrast for charts having less then 10 data sets.
        /// This provides the best result for most charts.
        /// </summary>
        /// <returns></returns>
        public static List<RockColor> GetPreferredChartColors()
        {
            if ( _preferredChartColors == null )
            {
                _preferredChartColors = new List<RockColor>();

                // Add the preferred colors that offer maximum contrast for charts having less then
                // 10 data sets. This provides the best result for most charts.
                foreach ( var color in ChartJsConstants.Colors.DefaultPalette )
                {
                    _preferredChartColors.Add( RockColor.FromHex( color ) );
                }

                // Add the remaining colors in the Html 4 color palette.
                // This caters for charts with a large number of data sets.
                foreach ( var hexColor in RockColor.Html4ColorNameMap.Values )
                {
                    var newColor = new RockColor( hexColor );
                    if ( !_preferredChartColors.Contains( newColor ) )
                    {
                        _preferredChartColors.Add( newColor );
                    }
                }
            }

            return _preferredChartColors;
        }

        #region Constructors

        /// <summary>
        /// Create a new instance for the default color palette.
        /// </summary>
        public ChartColorPaletteGenerator()
        {
        }

        /// <summary>
        /// Create a new instance using the provided set of colors.
        /// </summary>
        /// <param name="baseColors"></param>
        public ChartColorPaletteGenerator( List<RockColor> baseColors )
        {
            BaseColors = baseColors;
        }

        #endregion

        /// <summary>
        /// A collection of HTML Colors that represent the default palette for datasets in the chart.
        /// Colors are selected in order from this list for each dataset that does not have a specified color.
        /// </summary>
        public List<RockColor> BaseColors { get; set; }

        private Queue<RockColor> _colorQueue = null;
        private int _lightenPercentage = 0;

        /// <summary>
        /// Creates a queue of colors to be used as the palette for the chart datasets.
        /// </summary>
        /// <returns></returns>
        private void GenerateColorQueue()
        {
            _colorQueue = new Queue<RockColor>();

            var availableColors = this.BaseColors ?? ChartColorPaletteGenerator.GetPreferredChartColors();
            foreach ( var color in availableColors )
            {
                var queueColor = color.Clone();
                if ( _lightenPercentage != 0 )
                {
                    queueColor.Lighten( _lightenPercentage );
                }

                _colorQueue.Enqueue( queueColor );
            }
        }

        /// <summary>
        /// Get the next set of colors in the palette.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<RockColor> GetNextColors( int count )
        {
            var colors = new List<RockColor>();
            for ( int i = 0; i < count; i++ )
            {
                colors.Add( GetNextColor() );
            }
            return colors;
        }

        /// <summary>
        /// Get the next color in the palette.
        /// </summary>
        /// <returns></returns>
        public RockColor GetNextColor()
        {
            if ( _colorQueue == null
                 || _colorQueue.Count == 0 )
            {
                if ( _colorQueue != null )
                {
                    // We have exhausted the queue, so create another queue using lighter shades of the available colors.
                    _lightenPercentage += 25;
                    if ( _lightenPercentage > 100 )
                    {
                        _lightenPercentage = 0;
                    }
                }

                GenerateColorQueue();
            }

            return _colorQueue.Dequeue();
        }
    }

    #endregion
}