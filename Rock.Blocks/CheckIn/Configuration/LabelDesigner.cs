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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.CheckIn.v2.Labels;
using Rock.Enums.CheckIn.Labels;
using Rock.Model;
using Rock.ViewModels.Blocks.CheckIn.Configuration.LabelDesigner;
using Rock.ViewModels.CheckIn.Labels;

namespace Rock.Blocks.CheckIn.Configuration
{
    /// <summary>
    /// Designs a check-in label with a nice drag and drop experience.
    /// </summary>

    [DisplayName( "Label Designer" )]
    [Category( "Check-in > Configuration" )]
    [Description( "Designs a check-in label with a nice drag and drop experience." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "3f477b52-6062-4af4-abb7-b8c153f6242a" )]
    [Rock.SystemGuid.BlockTypeGuid( "8c4ad18f-9f81-4145-8ad0-ab90e451d0d6" )]
    public class LabelDesigner : RockBlockType
    {
        public override object GetObsidianBlockInitialization()
        {
            var personLabelSources = FieldDataSources.GetDataSources( LabelType.Person )
                .Select( p => ToDataSourceBag( p.Value ) )
                .ToList();

            return new
            {
                PersonLabelSources = personLabelSources
            };
        }

        private DataSourceBag ToDataSourceBag( FieldDataSource dataSource )
        {
            return new DataSourceBag
            {
                Key = dataSource.Key,
                Name = dataSource.Name,
                TextSubType = dataSource.TextSubType,
                Category = dataSource.Category,
                SupportedComparisionTypes = ( int ) dataSource.SupportedComparisionTypes,
                CustomFields = dataSource.CustomFields,
                FormatterOptions = dataSource.Formatter?.Options ?? new List<DataFormatterOptionBag>()
            };
        }
    }
}
