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
using System.ComponentModel;

namespace Rock.Model
{
    /// <summary>
    /// Identifies a UTM dimension captured on an interaction (utm_source, utm_medium, etc.).
    /// </summary>
    [Enums.EnumDomain( "Cms" )]
    public enum UtmDimension
    {
        /// <summary>
        /// The UTM source (utm_source).
        /// </summary>
        [Description( "Sources" )]
        Source = 0,

        /// <summary>
        /// The UTM medium (utm_medium).
        /// </summary>
        [Description( "Mediums" )]
        Medium = 1,

        /// <summary>
        /// The UTM campaign (utm_campaign).
        /// </summary>
        [Description( "Campaigns" )]
        Campaign = 2,

        /// <summary>
        /// The UTM term (utm_term).
        /// </summary>
        [Description( "Terms" )]
        Term = 3,

        /// <summary>
        /// The UTM content (utm_content).
        /// </summary>
        [Description( "Content" )]
        Content = 4
    }
}
