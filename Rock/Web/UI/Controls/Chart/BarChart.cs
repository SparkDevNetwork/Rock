﻿// <copyright>
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

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class BarChart : FlotChart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BarChart"/> class.
        /// </summary>
        public BarChart()
        {
            this.Options.series = new SeriesOptions( true, false, false );
            double oneDay = new TimeSpan( 1, 0, 0, 0 ).TotalMilliseconds;
            this.BarWidth = oneDay;
        }

        /// <summary>
        /// Gets or sets the width of the bar
        /// Note that if the xaxis is time, BarWidth should be in milliseconds
        /// </summary>
        /// <value>
        /// The width of the bar.
        /// </value>
        public double? BarWidth
        {
            get
            {
                return this.Options.series.bars.barWidth;
            }

            set
            {
                this.Options.series.bars.barWidth = value;
            }
        }
    }

}
