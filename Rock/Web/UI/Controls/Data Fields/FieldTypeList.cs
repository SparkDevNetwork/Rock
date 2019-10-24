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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A composite control that renders a label, dropdownlist, and datavalidation control for a specific field of a data model
    /// </summary>
    [ToolboxData( "<{0}:FieldTypeList runat=server></{0}:FieldTypeList>" )]
    [RockObsolete( "1.7" )]
    [Obsolete("Use FieldTypePicker instead", true )]
    public class FieldTypeList : DataDropDownList
    {
        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            this.Items.Clear();
            this.Items.Add( new ListItem() );
            foreach ( var item in FieldTypeCache.All() )
            {
                this.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
            }
        }
    }
}