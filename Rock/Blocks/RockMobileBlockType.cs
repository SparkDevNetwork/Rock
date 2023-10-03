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

using Rock.Mobile;

namespace Rock.Blocks
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    /// <seealso cref="Rock.Blocks.IRockMobileBlockType" />
    [Obsolete( "Use RockBlockType instead." )]
    [RockObsolete( "1.16.0" )]
    public abstract class RockMobileBlockType : RockBlockType, IRockMobileBlockType
    {
        #region Methods

        /// <summary>
        /// Gets the additional settings defined for this block instance.
        /// </summary>
        /// <returns>An AdditionalBlockSettings object.</returns>
        [Obsolete]
        [RockObsolete( "1.16.0" )]
        public AdditionalBlockSettings GetAdditionalSettings()
        {
            return BlockCache?.AdditionalSettings.FromJsonOrNull<AdditionalBlockSettings>() ?? new AdditionalBlockSettings();
        }

        #endregion
    }
}
