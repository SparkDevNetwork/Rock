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

using Rock.Attribute;
using Rock.Model;
using Rock.SystemKey;
using Rock.Web;

namespace Rock.Blocks.Administration
{
    /// <summary>
    /// Presents a panel that allows the administrator to choose the default experience after a fresh Rock install.
    /// </summary>
    [DisplayName( "Choose Experience Mode" )]
    [Category( "Administration" )]
    [Description( "Presents a panel that allows the administrator to choose the default experience after a fresh Rock install." )]
    [IconCssClass( "ti ti-compass" )]

    [SystemGuid.EntityTypeGuid( "3733a532-9f34-418c-8edb-ef07d6112907" )]
    [SystemGuid.BlockTypeGuid( "47418bf9-ddf0-4c12-92a0-667a3c1209d2" )]
    internal class ChooseExperienceMode : RockBlockType
    {
        #region Block Actions

        /// <summary>
        /// Saves the specified experience mode setting and deletes the current block.
        /// </summary>
        /// <returns>A <see cref="BlockActionResult"/> indicating the result of the save operation.</returns>
        [BlockAction]
        public BlockActionResult SaveExperienceMode( bool trailblazerMode )
        {
            SystemSettings.SetValue( SystemSetting.TRAILBLAZER_MODE, trailblazerMode.ToString() );

            var blockService = new BlockService( RockContext );
            var block = blockService.Get( this.BlockId );

            blockService.Delete( block );

            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }
}
