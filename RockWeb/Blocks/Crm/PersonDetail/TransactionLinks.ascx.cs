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
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

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
        "Add Transaction Page",
        Key = AttributeKey.AddTransactionPage,
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.Page.ADD_TRANSACTION,
        Order = 0 )]

    [LinkedPage(
        "Text to Give Settings Page",
        Key = AttributeKey.TextToGiveSettingsPage,
        IsRequired = false,
        Order = 1 )]

    [BooleanField(
        "Is Secondary Block",
        Key = AttributeKey.IsSecondaryBlock,
        Description = "Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.",
        DefaultBooleanValue = false,
        Order = 2 )]

    [IntegerField(
        "Person Token Expire Minutes",
        Key = AttributeKey.PersonTokenExpireMinutes,
        Description = "The number of minutes the person token for the transaction is valid after it is issued.",
        IsRequired = true,
        DefaultIntegerValue = 60,
        Order = 3 )]

    [IntegerField(
        "Person Token Usage Limit",
        Key = AttributeKey.PersonTokenUsageLimit,
        Description = "The maximum number of times the person token for the transaction can be used.",
        IsRequired = false,
        DefaultIntegerValue = 1,
        Order = 4 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "2BB707AC-F29A-44DF-A103-7454077509B4" )]
    public partial class TransactionLinks : PersonBlock, ISecondaryBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string AddTransactionPage = "AddTransactionPage";
            public const string IsSecondaryBlock = "IsSecondaryBlock";
            public const string PersonTokenExpireMinutes = "PersonTokenExpireMinutes";
            public const string PersonTokenUsageLimit = "PersonTokenUsageLimit";
            public const string TextToGiveSettingsPage = "TextToGiveSettingsPage";
        }

        #endregion Attribute Keys

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
            pnlContent.Visible = ( Person != null && Person.Id != 0 );

            base.OnLoad( e );
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
                if ( !this.Person.IsPersonTokenUsageAllowed() )
                {
                    mdWarningAlert.Show( $"Due to their protection profile level you cannot add a transaction on behalf of this person.", ModalAlertType.Warning );
                    return;
                }

                // create a limited-use personkey that will last long enough for them to go thru all the 'postbacks' while posting a transaction
                var personKey = this.Person.GetImpersonationToken(
                        RockDateTime.Now.AddMinutes( this.GetAttributeValue( AttributeKey.PersonTokenExpireMinutes ).AsIntegerOrNull() ?? 60 ),
                        this.GetAttributeValue( AttributeKey.PersonTokenUsageLimit ).AsIntegerOrNull(),
                        addTransactionPage.PageId );

                if ( personKey.IsNotNullOrWhiteSpace() )
                {
                    addTransactionPage.QueryString["Person"] = personKey;
                    Response.Redirect( addTransactionPage.BuildUrl() );
                }
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