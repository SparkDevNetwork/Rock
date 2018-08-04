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
using System.Collections.Generic;
using Rock;
using Rock.Attribute;
using Rock.Web.UI;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Web.UI.RockBlock" />
    /// <seealso cref="Rock.Web.UI.IPickerBlock" />
    [ColorField( "Header Text" )]
    public partial class PickSomething : RockBlock, IPickerBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            lHeaderText.Text = this.PickerSettings["HeaderText"];
        }

        #region IPickerBlock

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>
        /// The selected value.
        /// </value>
        public string SelectedValue
        {
            get
            {
                return ddlPickSomething.SelectedValue;
            }

            set
            {
                ddlPickSomething.SetValue( value );
            }
        }

        /// <summary>
        /// Gets the selected text.
        /// </summary>
        /// <value>
        /// The selected text.
        /// </value>
        public string GetSelectedText( string selectedValue )
        {
            var item = ddlPickSomething.Items.FindByValue( selectedValue );
            if ( item != null )
            {
                return item.Text;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this control should be shown in a modal
        /// </summary>
        /// <value>
        /// <c>true</c> if [show in modal]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInModal
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// The picker settings
        /// </summary>
        private Dictionary<string, string> _pickerSettings = new Dictionary<string, string>()
        {
            { "HeaderText", "Hello World" }
        };

        /// <summary>
        /// The picker settings.
        /// </summary>
        /// <value>
        /// The picker settings.
        /// </value>
        public Dictionary<string, string> PickerSettings
        {
            get
            {
                return _pickerSettings;
            }
        }

        #endregion

        /// <summary>
        /// Occurs when [select item].
        /// </summary>
        public event EventHandler SelectItem;

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlPickSomething control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlPickSomething_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( SelectItem != null )
            {
                SelectItem( this, e );
            }
        }
    }
}