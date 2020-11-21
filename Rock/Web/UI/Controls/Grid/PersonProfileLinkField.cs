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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// <see cref="Grid"/> Column for linking to a profile
    /// </summary>
    public class PersonProfileLinkField : HyperLinkField
    {
        /// <summary>
        /// Gets or sets the linked page attribute key.
        /// </summary>
        /// <value>
        /// The linked page attribute key.
        /// </value>
        public string LinkedPageAttributeKey
        {
            get
            {
                return ViewState["LinkedPageAttributeKey"] as string;
            }
            set
            {
                ViewState["LinkedPageAttributeKey"] = value;
            }
        }

        /// <summary>
        /// Performs basic instance initialization for a data control field.
        /// </summary>
        /// <param name="sortingEnabled">A value that indicates whether the control supports the sorting of columns of data.</param>
        /// <param name="control">The data control that owns the <see cref="T:System.Web.UI.WebControls.DataControlField"/>.</param>
        /// <returns>
        /// Always returns false.
        /// </returns>
        public override bool Initialize( bool sortingEnabled, Control control )
        {
            var grid = control as Grid;
            var block = grid?.RockBlock();

            // This control must be within a grid with a person ID field and must have a attribute key
            if ( block == null || LinkedPageAttributeKey.IsNullOrWhiteSpace() || grid.PersonIdField.IsNullOrWhiteSpace() )
            {
                Visible = false;
                return base.Initialize( sortingEnabled, control );
            }

            var linkedPageUrl = block.LinkedPageUrl( LinkedPageAttributeKey, new Dictionary<string, string> { { "PersonId", "{0}" } } );

            // If there is no link, then hide the control
            if ( linkedPageUrl.IsNullOrWhiteSpace() )
            {
                Visible = false;
                return base.Initialize( sortingEnabled, control );
            }

            Visible = true;
            ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            HeaderStyle.CssClass = "grid-columncommand";
            ItemStyle.CssClass = "grid-columncommand";
            ControlStyle.CssClass = "btn btn-default btn-sm";
            DataNavigateUrlFields = new[] { grid.PersonIdField };

            DataNavigateUrlFormatString = linkedPageUrl;
            DataTextFormatString = "<i class='fa fa-user'></i>";
            DataTextField = grid.PersonIdField;

            return base.Initialize( sortingEnabled, control );
        }
    }
}