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
using System.Linq;

using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class StateDropDownList : RockDropDownList
    {
        private bool _rebindRequired = false;

        /// <summary>
        /// Display an abbreviated state name
        /// </summary>
        public bool UseAbbreviation
        {
            get { return ViewState["UseAbbreviation"] as bool? ?? false; }
            set 
            {
                _rebindRequired = ( ViewState["UseAbbreviation"] as bool? ?? false ) != value;
                ViewState["UseAbbreviation"] = value; 
            }
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.DataValueField = "Id";
            this.DataTextField = UseAbbreviation ? "Id" : "Value";

            _rebindRequired = false;
            var definedType = DefinedTypeCache.Get( new Guid( SystemGuid.DefinedType.LOCATION_ADDRESS_STATE ) );
            var stateList = definedType.DefinedValues.OrderBy( v => v.Order ).Select( v => new { Id = v.Value, Value = v.Description } ).ToList();
            this.DataSource = stateList;

            // make sure it isn't set to a selected value that doesn't exist and default to Org State if nothing is set
            var selectedValue = _cachedSelectedValue ?? this.DefaultSelectedValue;

            if ( !stateList.Any( a => a.Id == selectedValue ) )
            {
                this.SelectedValue = null;
            }
            else
            {
                this.SelectedValue = selectedValue;
            }

            this.DataBind();
        }

        /// <summary>
        /// Gets the default selected value which is the Organizations Address's State
        /// </summary>
        /// <value>
        /// The default selected value.
        /// </value>
        public string DefaultSelectedValue
        {
            get
            {
                return GlobalAttributesCache.Get().OrganizationState;
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( System.Web.UI.HtmlTextWriter writer )
        {
            if ( _rebindRequired )
            {
                string value = this.SelectedValue;

                this.DataTextField = UseAbbreviation ? "Id" : "Value";
                this.DataBind();

                var li = this.Items.FindByValue( value );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }

            base.RenderControl( writer );
        }

        
        /// <summary>
        /// Gets the value of the selected item in the list control, or selects the item in the list control that contains the specified value.
        /// </summary>
        /// <returns>The value of the selected item in the list control. The default is an empty string ("").</returns>
        public override string SelectedValue
        {
            get
            {
                return base.SelectedValue;
            }
            set
            {
                // keep track of the selected value that they want to set to prevent having it bind with a value that doesn't exist
                _cachedSelectedValue = value;
                base.SelectedValue = value;
            }
        }

        private string _cachedSelectedValue;
    }
}
