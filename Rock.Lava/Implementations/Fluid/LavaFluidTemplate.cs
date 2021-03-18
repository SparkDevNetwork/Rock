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
using Fluid;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// An implementation of a FluidTemplate that stores some additional information about the parsed template.
    /// </summary>
    internal class LavaFluidTemplate : BaseFluidTemplate<LavaFluidTemplate>
    {
        /// <summary>
        /// The text of the Lava source template.
        /// </summary>
        public string SourceDocument { get; set; }

        /// <summary>
        /// A collection of elements parsed from the source document.
        /// </summary>
        public List<FluidParsedTemplateElement> Elements { get; set; }
    }
}
