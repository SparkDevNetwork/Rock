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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Web.UI;

namespace RockWeb.Blocks.Security
{
    [DisplayName( "Rest User Detail" )]
    [Category( "Security" )]
    [Description( "Displays the details of the given REST API User." )]
    public partial class RestUserDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                var rockContext = new RockContext();
                string itemId = PageParameter( "restUserId" );

                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "restUserId", int.Parse( itemId ) );
                }
                else
                {
                    //pnlDetails.Visible = false;
                }
            }
        }

        #endregion Control Methods

        #region Events

        #endregion Events

        #region Internal Methods

        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            lTitle.Text = ActionTitle.Edit( "Rest User" ).FormatAsHtmlTitle();
        }

        #endregion Internal Methods
        protected void lbSave_Click( object sender, EventArgs e )
        {

        }
        protected void lbCancel_Click( object sender, EventArgs e )
        {

        }
        protected void lbGenerate_Click( object sender, EventArgs e )
        {

        }
}
}