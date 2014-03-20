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
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Business Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of the given business." )]
    public partial class BusinessDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        #region Control Methods
        #endregion

        #region Events
        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
        }

        #endregion
        protected void lbSave_Click( object sender, EventArgs e )
        {

        }
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( tbPhone.Text ) ) )
            {
                var phoneNumber = new PhoneNumber();
                phoneNumber.Number = PhoneNumber.CleanNumber( tbPhone.Text );
            }
        }
}
}