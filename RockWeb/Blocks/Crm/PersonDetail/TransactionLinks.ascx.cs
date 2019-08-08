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
using System.ComponentModel;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Transaction Links
    /// </summary>
    [DisplayName( "Person Transaction Links" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Block for displaying links to add and schedule transactions for a person." )]

    #region Block Attributes

    [LinkedPage(
        name: "Add Transaction Page",
        description: "",
        required: true,
        defaultValue: Rock.SystemGuid.Page.ADD_TRANSACTION,
        category: "",
        order: 0,
        key: AttributeKey.AddTransactionPage )]

    [LinkedPage(
        name: "Text to Give Settings Page",
        description: "",
        required: false,
        defaultValue: "",
        category: "",
        order: 0,
        key: AttributeKey.TextToGiveSettingsPage )]

    [BooleanField(
        name: "Is Secondary Block",
        description: "Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.",
        defaultValue: false,
        category: "",
        order: 1,
        key: AttributeKey.IsSecondaryBlock )]

    [IntegerField(
        name: "Person Token Expire Minutes",
        description: "The number of minutes the person token for the transaction is valid after it is issued.",
        required: true,
        defaultValue: 60,
        category: "",
        order: 2,
        key: AttributeKey.PersonTokenExpireMinutes )]

    [IntegerField(
        name: "Person Token Usage Limit",
        description: "The maximum number of times the person token for the transaction can be used.",
        required: false,
        defaultValue: 1,
        category: "",
        order: 3,
        key: AttributeKey.PersonTokenUsageLimit )]

    #endregion Block Attributes

    public partial class TransactionLinks : PersonBlock, ISecondaryBlock
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        protected static class AttributeKey
        {
            /// <summary>
            /// The add transaction page
            /// </summary>
            public const string AddTransactionPage = "AddTransactionPage";

            /// <summary>
            /// The is secondary block
            /// </summary>
            public const string IsSecondaryBlock = "IsSecondaryBlock";

            /// <summary>
            /// The person token expire minutes
            /// </summary>
            public const string PersonTokenExpireMinutes = "PersonTokenExpireMinutes";

            /// <summary>
            /// The person token usage limit
            /// </summary>
            public const string PersonTokenUsageLimit = "PersonTokenUsageLimit";

            /// <summary>
            /// The text to give settings page
            /// </summary>
            public const string TextToGiveSettingsPage = "TextToGiveSettingsPage";
        }

        #endregion Keys

        #region Base Control Methods

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

            if ( !IsPostBack )
            {
                if ( GetAttributeValue( AttributeKey.TextToGiveSettingsPage ).IsNullOrWhiteSpace() )
                {
                    btnTextToGiveSettings.Visible = false;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            pnlContent.Visible = ( Person != null && Person.Id != 0 );
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            if ( GetAttributeValue( AttributeKey.TextToGiveSettingsPage ).IsNullOrWhiteSpace() )
            {
                btnTextToGiveSettings.Visible = false;
            }
            else
            {
                btnTextToGiveSettings.Visible = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnAddTransaction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddTransaction_Click( object sender, EventArgs e )
        {
            var addTransactionPage = new Rock.Web.PageReference( this.GetAttributeValue( AttributeKey.AddTransactionPage ) );
            if ( addTransactionPage != null )
            {
                // create a limited-use personkey that will last long enough for them to go thru all the 'postbacks' while posting a transaction
                var personKey = this.Person.GetImpersonationToken( RockDateTime.Now.AddMinutes( this.GetAttributeValue( AttributeKey.PersonTokenExpireMinutes ).AsIntegerOrNull() ?? 60 ), this.GetAttributeValue( AttributeKey.PersonTokenUsageLimit ).AsIntegerOrNull(), addTransactionPage.PageId );
                Response.Redirect( string.Format( "~/AddTransaction?Person={0}", personKey ) );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnTextToGiveSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnTextToGiveSettings_Click( object sender, EventArgs e )
        {
            var settingsPage = new Rock.Web.PageReference( GetAttributeValue( AttributeKey.TextToGiveSettingsPage ) );

            if ( settingsPage != null && Person != null )
            {
                NavigateToLinkedPage( AttributeKey.TextToGiveSettingsPage, new Dictionary<string, string> { { "PersonId", Person.Id.ToString() } } );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnAddScheduledTransaction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddScheduledTransaction_Click( object sender, EventArgs e )
        {
            // just send them the same page as Add Transaction page since you can add a scheduled transaction there either way
            btnAddTransaction_Click( sender, e );
        }

        #endregion

        #region Secondary Block

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            if ( this.GetAttributeValue( AttributeKey.IsSecondaryBlock ).AsBooleanOrNull() ?? false )
            {
                // hide the entire block
                this.Visible = visible;
            }
        }

        #endregion Secondary Block
    }
}

