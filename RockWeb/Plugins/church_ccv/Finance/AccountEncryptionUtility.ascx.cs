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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace RockWeb.Plugins.church_ccv.Finance
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Account Encryption Utility" )]
    [Category( "CCV > Finance" )]
    [Description( "Utility block to encrypt any saved bank account numbers that have been recently synced, but not yet encrypted." )]

    public partial class AccountEncryptionUtility : Rock.Web.UI.RockBlock
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            nbResult.Visible = false;
        }

        #endregion

        #region Events

        protected void lbGo_Click( object sender, EventArgs e )
        {
            try
            {
                int successCount = 0;

                using ( var rockContext = new RockContext() )
                {
                    var service = new FinancialPersonBankAccountService( rockContext );
                    var items = service.Queryable()
                        .Where( a => a.ForeignId != null &&
                            a.ForeignId.StartsWith( "T" ) &&
                            a.AccountNumberSecured != null &&
                            a.AccountNumberSecured == "" )
                        .ToList();

                    foreach ( var item in items )
                    {
                        var parts = item.ForeignId.Substring(1).Split('A');
                        if ( parts.Length >= 2 && 
                            !string.IsNullOrWhiteSpace( parts[0] ) &&
                            !string.IsNullOrWhiteSpace( parts[1] ) )
                        {
                            item.AccountNumberSecured = FinancialPersonBankAccount.EncodeAccountNumber( parts[0], parts[1] );
                            item.AccountNumberMasked = parts[1].Masked();
                            successCount++;

                            if ( successCount % 1000 == 0 )
                            {
                                rockContext.SaveChanges();
                            }
                        }
                    }

                    rockContext.SaveChanges();
                }

                nbResult.NotificationBoxType = NotificationBoxType.Success;
                nbResult.Title = "Success!";
                nbResult.Text = string.Format( "<p>{0:N0} account numbers were succesfully encrypted</p>", successCount );
                nbResult.Visible = true;

            }
            catch ( Exception ex )
            {
                nbResult.NotificationBoxType = NotificationBoxType.Danger;
                nbResult.Title = "Error Occurred";
                nbResult.Text = string.Format( "<p>{0}</p>", ex.Message );
                nbResult.Visible = true;
            }
        }

        #endregion

    }
}