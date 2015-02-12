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

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Kiosk Transaction Entry" )]
    [Category( "Finance" )]
    [Description( "Block used to process giving from a kiosk." )]

    #region Block Attributes
    [ComponentField( "Rock.Financial.GatewayContainer, Rock", "Credit Card Gateway", "The payment gateway to use for Credit Card transactions", true, "", "", 0, "CCGateway" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE, "Source", "The Financial Source Type to use when creating transactions", false, false,
        Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_KIOSK, "", 1 )]
    [AccountsField( "Accounts", "Accounts to allow giving. This list will be filtered by campus context when displayed.", true, "", "", 1 )]
    [TextField( "Batch Name Prefix", "The prefix to add to the financial batch.", true, "Kiosk Giving", "", 2 )]
    [LinkedPage( "Homepage", "Homepage of the kiosk.", true, "", "", 2 )]
    [PersonField("Anonymous Person", "Person in the database to assign anonymous giving to.", true, "", "", 3)]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Connection Status", "The connection status to use when creating a new individual.", true, false, Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PARTICIPANT, "", 4 )]
    #endregion

    public partial class KioskTransactionEntry : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                // added for your convenience
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        protected void lbSearchNext_Click( object sender, EventArgs e )
        {
            if ( tbPhone.Text.Length < 4 )
            {
                nbSearch.Text = "You must enter at least 4 numbers of your phone.";
                nbSearch.NotificationBoxType = NotificationBoxType.Warning;
            }
            else
            {
                // todo search for families
                HidePanels();
                pnlFamilySelect.Visible = true;
            }
        }

        protected void lbSearchCancel_Click( object sender, EventArgs e )
        {
            GoHome();
        }

        protected void lbFamilySelectCancel_Click( object sender, EventArgs e )
        {
            GoHome();
        }

        protected void lbFamilySelectBack_Click( object sender, EventArgs e )
        {
            HidePanels();
            pnlSearch.Visible = true;
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        #endregion

        #region Methods

        // hides all panels
        private void HidePanels()
        {
            pnlSearch.Visible = false;
            pnlFamilySelect.Visible = false;
            pnlRegister.Visible = false;
            pnlAccountEntry.Visible = false;
            pnlSwipe.Visible = false;
            pnlReceipt.Visible = false;

            // clear out specific notification blocks that are used for validation
            nbSearch.Text = string.Empty;
        }

        // redirects to the homepage
        private void GoHome()
        {
            NavigateToLinkedPage( "Homepage" );
        }

        #endregion

        
}
}