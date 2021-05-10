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
using Fluid.Parser;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// A Lava library wrapper for the Fluid implementation of a Liquid template.
    /// </summary>
    internal class FluidTemplateProxy : LavaTemplateBase
    {
        #region Constructors

        private FluidTemplate _template;

        public FluidTemplateProxy( FluidTemplate template )
        {
            _template = template;
        }

        #endregion

        /// <summary>
        /// Get the Fluid template instance.
        /// </summary>
        public FluidTemplate FluidTemplate
        {
            get
            {
                return _template;
            }
        }
    }
}
