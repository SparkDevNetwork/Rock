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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.BulkImport;
using Rock.ViewModels.Utility;

namespace Rock.Blocks.BulkImport
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "CSV Import" )]
    [Category( "CSV Import" )]
    [Description( "Block to import data into Rock using the CSV files." )]
    [IconCssClass( "fa fa-file-csv" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "A25E1EBB-A21B-4283-AFD8-E5D7C4CA8757" )]
    [Rock.SystemGuid.BlockTypeGuid( "362C679C-9A7F-4A2B-9BB0-8683824BE892" )]
    public class CsvImport : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            // TODO
            public const string Todo = "Todo";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CsvImportBlockBox();

            box.PreviousSourceDescriptions = new PersonService( new RockContext() )
                    .GetForeignKeys()
                    .Select( foreignKey => new ListItemBag { Value = foreignKey, Text = foreignKey } )
                    .ToList();

            return box;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the bag that describes the grid data to be displayed in the block.
        /// </summary>
        /// <returns>An action result that contains the grid data.</returns>
        [BlockAction]
        public virtual BlockActionResult ImportData()
        {
            var rockContext = new RockContext();

            var bag = new // ImportResultBag
            {
            };

            return ActionOk( bag );
        }

        #endregion
    }
}
