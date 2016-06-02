// <copyright>
// Copyright by Central Christian Church
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

namespace RockWeb.Plugins.com_centralaz.Prayer
{
    /// <summary>
    /// A block for people to follow-up on their prayer requests.
    /// </summary>
    [DisplayName( "Prayer Response" )]
    [Category( "com_centralaz > Prayer" )]
    [Description( "Block for people who have requested prayer to submit an answer to said prayer." )]
    [IntegerField( "Expires After (Days)", "Number of days until the request will expire (only applies when auto-approved is enabled).", false, 14, "Features", 4, "ExpireDays" )]
    [IntegerField( "Maximum Answer Length", "Maximum character length for prayer answers.", true, 4000 )]
    [CodeEditorField( "Description", "Lava template to use to display information about the block", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"<div>
  We are encouraged to be able to pray for you. If you feel as though your prayer has been answered, feel welcome to fill out just how it has been answered. Otherwise, click the 'Extend Request' button for the prayer request to be extended another week.
  </br>
  </br>
  First Name: {{PrayerRequest.FirstName}}
  </br>
  Last Name: {{PrayerRequest.LastName}}
  </br>
  Email Address: {{PrayerRequest.Email}}
  </br>
  Prayer Category: {{PrayerRequest.Category.Name}}
  </br>
  </br>
  Request: {{PrayerRequest.Text}}
  </br>
  </br>

</div>
", "", 2 )]
    [CodeEditorField( "Error", "Lava template to use to display an error message.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"<div>
<h3>Error</h3>
Either the prayer you are looking to update has expired, or you do not have the right credentials for it. If you would like us to continue praying for you, we welcome you to fill out another prayer request.
</div>", "", 2 )]
    [CodeEditorField( "Date Extended Message", "Lava template to use to display a success message.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"<div>
Our team will continue praying for your request.
</div>", "", 2 )]
    [CodeEditorField( "Answer Submitted Message", "Lava template to use to display a success message.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"<div>
Thank you for sharing God's answer to your prayer with us.
</div>", "", 2 )]
    public partial class PrayerResponse : Rock.Web.UI.RockBlock
    {

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
                Guid? guid = PageParameter( "Guid" ).AsGuidOrNull();
                if ( guid != null )
                {
                    PrayerRequest prayerRequest = new PrayerRequestService( new RockContext() ).Get( guid.Value );
                    if ( prayerRequest != null )
                    {
                        byte[] b = System.Text.Encoding.UTF8.GetBytes( prayerRequest.Email );
                        string encodedEmail = Convert.ToBase64String( b );

                        if ( encodedEmail == PageParameter( "Key" )
                            && ( ( prayerRequest.ExpirationDate != null
                                && prayerRequest.ExpirationDate.Value.AddDays( GetAttributeValue( "ExpireDays" ).AsInteger() ) >= DateTime.Now )
                            || prayerRequest.ExpirationDate == null )
                            )
                        {
                            if ( prayerRequest.ExpirationDate == null )
                            {
                                lbExtend.Visible = false;
                            }

                            string template = GetAttributeValue( "Description" );
                            lDescription.Text = template.ResolveMergeFields( GetMergeFields( prayerRequest ) );
                        }
                        else
                        {
                            pnlView.Visible = false;
                            pnlResponse.Visible = true;
                            string template = GetAttributeValue( "Error" );
                            lResponse.Text = template.ResolveMergeFields( GetMergeFields( prayerRequest ) );
                        }
                    }
                }
                else
                {
                    pnlView.Visible = false;
                    pnlResponse.Visible = true;
                    string template = GetAttributeValue( "Error" );
                    lResponse.Text = template.ResolveMergeFields( GetMergeFields() );
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            int maxChar = GetAttributeValue( "MaximumAnswerLength" ).AsInteger();
            if ( dtbAnswer.Text.Length > maxChar )
            {
                nbWarning.Text = String.Format( "Please keep response shorter than {0} characters long.", maxChar );
                nbWarning.Visible = true;
            }
            else
            {
                nbWarning.Visible = false;
                RockContext rockContext = new RockContext();
                Guid guid = PageParameter( "Guid" ).AsGuid();
                PrayerRequest prayerRequest = new PrayerRequestService( rockContext ).Get( guid );
                prayerRequest.Answer = dtbAnswer.Text;
                rockContext.SaveChanges();

                string template = GetAttributeValue( "AnswerSubmittedMessage" );
                lResponse.Text = template.ResolveMergeFields( GetMergeFields( prayerRequest ) );
                pnlView.Visible = false;
                pnlResponse.Visible = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbExtend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbExtend_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            Guid guid = PageParameter( "Guid" ).AsGuid();
            PrayerRequest prayerRequest = new PrayerRequestService( rockContext ).Get( guid );
            prayerRequest.ExpirationDate = DateTime.Now.AddDays( GetAttributeValue( "ExpireDays" ).AsInteger() );
            rockContext.SaveChanges();

            string template = GetAttributeValue( "DateExtendedMessage" );
            lResponse.Text = template.ResolveMergeFields( GetMergeFields( prayerRequest ) );
            pnlView.Visible = false;
            pnlResponse.Visible = true;
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

        /// <summary>
        /// Gets the merge fields.
        /// </summary>
        /// <param name="prayerRequest">The prayer request.</param>
        /// <returns></returns>
        private Dictionary<string, object> GetMergeFields( PrayerRequest prayerRequest = null )
        {
            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "CurrentPerson", CurrentPerson );
            if ( prayerRequest != null )
            {
                mergeFields.Add( "PrayerRequest", prayerRequest );
            }
            return mergeFields;
        }
        #endregion

    }
}