// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A composite control that renders a label, dropdownlist, and datavalidation control for a specific field of a data model
    /// </summary>
    [ToolboxData( "<{0}:FieldTypeList runat=server></{0}:FieldTypeList>" )]
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
            foreach ( var item in Rock.Web.Cache.FieldTypeCache.All() )
            {
                this.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
            }
        }
    }
}