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

namespace Rock.ViewModels.Blocks.Engagement.StepProgramDetail
{
    /// <summary>
    ///
    /// </summary>
    public class SeriesBag
    {
        /// <summary>
        /// Gets or sets the label for the series.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the data for the series.
        /// </summary>
        public List<double> Data { get; set; }

        /// <summary>
        /// Gets or sets the color for the series.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the opacity for each data point in the series.
        /// </summary>
        public List<double> Opacity { get; set; }
    }
}
