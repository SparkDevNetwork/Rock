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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// User control for editing a system email
    /// </summary>
    [DisplayName( "Unsubscribe" )]
    [Category( "Communication" )]
    [Description( "Allows user to unsubscribe from email communications." )]

    [TextField("No Mass Emails Text", "Text to display for the 'No Mass Emails' option.", false, "I am am still involved with {{ GlobalAttribute.OrganizationName }}, but do not wish to receive mass emails (personal emails are fine).")]
    [TextField( "No Emails Text", "Text to display for the 'No Emails' option.", false, "I am still involved with {{ GlobalAttribute.OrganizationName }}, do not want to receive emails of any kind." )]
    [TextField( "Not Involved Text", "Text to display for the 'Not Involved' option.", false, " I am no longer involved with {{ GlobalAttribute.OrganizationName }}." )]
    [TextField( "Success Text", "Text to display after user submits selection.", false, "<h4>Thank-You</h4>We have saved your email preference." )]
    [TextField( "Reasons to Exclude", "A delimited list of the Inactive Reasons to exclude from Reason list", false, "No Activity,Deceased")]
    public partial class Unsubscribe : RockBlock
    {
         
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var mergeObjects = GlobalAttributesCache.GetMergeFields( CurrentPerson );
            rblEmailPreference.Items.Add( new ListItem( GetAttributeValue( "NoMassEmailsText" ).ResolveMergeFields(mergeObjects), "0" ) );
            rblEmailPreference.Items.Add( new ListItem( GetAttributeValue( "NoEmailsText" ).ResolveMergeFields( mergeObjects ), "1" ) );
            rblEmailPreference.Items.Add( new ListItem( GetAttributeValue( "NotInvolvedText" ).ResolveMergeFields( mergeObjects ), "2" ) );
            rblEmailPreference.SelectedIndex = 0;

            nbSuccess.Text = GetAttributeValue( "SuccessText" ).ResolveMergeFields( mergeObjects );

            LoadDropdowns();

            string script = string.Format( @"
    $(""input[id^='{0}'"").click(function () {{
        if ($(this).val() == '2') {{
            $('#{1}').slideDown('fast');
        }} else {{
            $('#{1}').slideUp('fast');
        }}
    }});    
", rblEmailPreference.ClientID, divNotInvolved.ClientID );

            ScriptManager.RegisterStartupScript( rblEmailPreference, rblEmailPreference.GetType(), "toggle-preference" + this.BlockId.ToString(), script, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Page.IsPostBack )
            {
                divNotInvolved.Attributes["Style"] = rblEmailPreference.SelectedValue == "2" ? "display:block" : "display:none";
            }
        }
        
        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnSubmit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSubmit_Click( object sender, EventArgs e )
        {
            var key = PageParameter( "Person" );
            if ( !string.IsNullOrWhiteSpace( key ) )
            {
                var service = new PersonService();
                var person = service.GetByUrlEncodedKey( PageParameter( "Person" ) );
                if ( person != null )
                {
                    switch ( rblEmailPreference.SelectedValue )
                    {
                        case "0":
                            {
                                person.EmailPreference = EmailPreference.NoMassEmails;
                                break;
                            }
                        case "1":
                            {
                                person.EmailPreference = EmailPreference.DoNotEmail;
                                break;
                            }
                        case "2":
                            {
                                person.EmailPreference = EmailPreference.DoNotEmail;
                                person.RecordStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE ).Id;
                                person.RecordStatusReasonValueId = ddlInactiveReason.SelectedValue.AsInteger().Value;
                                person.SystemNote = tbInactiveNote.Text;
                                break;
                            }
                    }

                    service.Save( person, CurrentPersonAlias );

                    nbSuccess.Title = string.Empty;
                    nbSuccess.NotificationBoxType = NotificationBoxType.Success;
                    nbSuccess.Visible = true;
                    return;
                }
            }

            nbSuccess.Title = "Sorry";
            nbSuccess.Text = "But, we're not really sure who you are.";
            nbSuccess.NotificationBoxType = NotificationBoxType.Danger;
            nbSuccess.Visible = true;
        }

        #endregion

        #region Methods

        private void LoadDropdowns()
        {
            var excludeReasons = GetAttributeValue( "ReasonstoExclude" ).SplitDelimitedValues( false ).ToList();
            var ds = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON.AsGuid() ).DefinedValues
                .Where( v => !excludeReasons.Contains(v.Name, StringComparer.OrdinalIgnoreCase))
                .Select( v => new
                {
                    v.Name,
                    v.Id
                } );


            ddlInactiveReason.SelectedIndex = -1;
            ddlInactiveReason.DataSource = ds;
            ddlInactiveReason.DataTextField = "Name";
            ddlInactiveReason.DataValueField = "Id";
            ddlInactiveReason.DataBind();
        }
        
        #endregion

    }
}