// <copyright>
// Copyright by BEMA Software Services
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
using System.Web.UI.WebControls;

using com.bemaservices.RoomManagement.Model;
using com.bemaservices.RoomManagement.ReportTemplates;
using Rock;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace com.bemaservices.RoomManagement.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class ReportTemplatePicker : RockDropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportTemplatePicker" /> class.
        /// </summary>
        public ReportTemplatePicker()
            : base()
        {
            Label = "Report Template Type";
        }

        /// <summary>
        /// Gets or sets the report templates.
        /// </summary>
        /// <value>
        /// The report templates.
        /// </value>
        public List<EntityTypeCache> ReportTemplates
        {
            set
            {
                this.Items.Clear();
                this.Items.Add( new ListItem() );

                foreach ( EntityTypeCache reportTemplate in value )
                {
                    this.Items.Add( new ListItem( reportTemplate.FriendlyName, reportTemplate.Id.ToString() ) );
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected report template identifier.
        /// </summary>
        /// <value>
        /// The selected report template identifier.
        /// </value>
        public int? SelectedReportTemplateId
        {
            get
            {
                return this.SelectedValueAsInt();
            }

            set
            {
                int id = value.HasValue ? value.Value : 0;
                var li = this.Items.FindByValue( id.ToString() );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }
        }
    }
}