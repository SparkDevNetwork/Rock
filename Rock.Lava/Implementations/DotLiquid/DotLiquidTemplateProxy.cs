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
using DotLiquid;

namespace Rock.Lava.DotLiquid
{
    /// <summary>
    /// A Lava library wrapper for the DotLiquid implementation of a Liquid template.
    /// </summary>
    public class DotLiquidTemplateProxy : LavaTemplateBase
    {
        #region Constructors

        // A parsed DotLiquid template.
        private Template _dotLiquidTemplate;

        public DotLiquidTemplateProxy( Template template )
        {
            _dotLiquidTemplate = template;
        }

        #endregion

        /// <summary>
        /// Get the DotLiquid template instance.
        /// </summary>
        public Template DotLiquidTemplate
        {
            get
            {
                return _dotLiquidTemplate;
            }
        }

        public void Dispose()
        {
            //
        }
    }
}
