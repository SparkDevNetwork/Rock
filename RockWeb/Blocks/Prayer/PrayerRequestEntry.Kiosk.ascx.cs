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
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Prayer
{
    [DisplayName( "Prayer Request Entry - Kiosk" )]
    [Category( "Prayer" )]
    [Description( "Allows prayer requests to be added from a kiosk." )]

    // Category Selection
    [CategoryField( "Category Selection", "A top level category. This controls which categories the person can choose from when entering their prayer request.", false, "Rock.Model.PrayerRequest", "", "", false, "", "Category Selection", 1, "GroupCategoryId" )]
    [CategoryField( "Default Category", "If categories are not being shown, choose a default category to use for all new prayer requests.", false, "Rock.Model.PrayerRequest", "", "", false, "4B2D88F5-6E45-4B4B-8776-11118C8E8269", "Category Selection", 2, "DefaultCategory" )]
    
    // Features
    [BooleanField( "Enable Auto Approve", "If enabled, prayer requests are automatically approved; otherwise they must be approved by an admin before they can be seen by the prayer team.", true, "Features", 3 )]
    [IntegerField( "Expires After (Days)", "Number of days until the request will expire (only applies when auto-approved is enabled).", false, 14, "Features", 4, "ExpireDays" )]
    [BooleanField( "Default Allow Comments Setting", "This is the default setting for the 'Allow Comments' on prayer requests. If you enable the 'Comments Flag' below, the requestor can override this default setting.", true, "Features", 5 )]
    [BooleanField( "Enable Urgent Flag", "If enabled, requestors will be able to flag prayer requests as urgent.", false, "Features", 6 )]
    [BooleanField( "Enable Comments Flag", "If enabled, requestors will be able set whether or not they want to allow comments on their requests.", false, "Features", 7 )]
    [BooleanField( "Enable Public Display Flag", "If enabled, requestors will be able set whether or not they want their request displayed on the public website.", false, "Features", 8 )]
    [IntegerField( "Character Limit", "If set to something other than 0, this will limit the number of characters allowed when entering a new prayer request.", false, 250, "Features", 9 )]
    
    // On Save Behavior
    [BooleanField( "Navigate To Parent On Save", "If enabled, on successful save control will redirect back to the parent page.", false, "On Save Behavior", 10 )]
    [CodeEditorField( "Save Success Text", "Text to display upon successful save. (Only applies if not navigating to parent page on save.) <span class='tip tip-html'>", CodeEditorMode.Html, CodeEditorTheme.Rock, 200, false, "<p>Thank you for allowing us to pray for you.</p>", "On Save Behavior", 11 )]

    [LinkedPage( "Homepage", "Homepage of the kiosk.", true, "", "", 12 )]
    public partial class PrayerRequestEntryKiosk : RockBlock
    {
        #region Properties
        public int? PrayerRequestEntityTypeId { get; private set; }
        
        // note: the ascx uses these for rendering logic
        public bool EnableUrgentFlag { get; private set; }
        public bool EnableCommentsFlag { get; private set; }
        public bool EnablePublicDisplayFlag { get; private set; }

        #endregion

        #region Base Control Methods
        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( !IsPostBack )
            {
                // hide cancel buttons if no homepage defined
                if ( string.IsNullOrWhiteSpace( GetAttributeValue( "Homepage" ) ) )
                {
                    lbPrayerCancel.Visible = false;
                }
            }

            this.EnableUrgentFlag = GetAttributeValue( "EnableUrgentFlag" ).AsBoolean();
            this.EnableCommentsFlag = GetAttributeValue( "EnableCommentsFlag" ).AsBoolean();
            this.EnablePublicDisplayFlag = GetAttributeValue( "EnablePublicDisplayFlag" ).AsBoolean();
            nbMessage.Text = GetAttributeValue( "SaveSuccessText" );

            RockPage.AddScriptLink( Page, ResolveUrl( "~/Scripts/bootstrap-limit.js" ) );
            var categoryGuid = GetAttributeValue( "GroupCategoryId" );
            if ( ! string.IsNullOrEmpty( categoryGuid ) )
            {
                BindCategories( categoryGuid );
            }
            else
            {
                bddlCategory.Visible = false;
            }

            Type type = new PrayerRequest().GetType();
            this.PrayerRequestEntityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( type.FullName );

            int charLimit = GetAttributeValue( "CharacterLimit" ).AsInteger();
            if ( charLimit > 0 )
            {
                dtbRequest.Placeholder = string.Format( "Please pray that... (up to {0} characters)", charLimit );
                string scriptFormat = @"
    function SetCharacterLimit() {{
        $('#{0}').limit({{maxChars: {1}, counter:'#{2}', normalClass:'badge', warningClass:'badge-warning', overLimitClass: 'badge-danger'}});

        $('#{0}').on('cross', function(){{
            $('#{3}').prop('disabled', true);
        }});
        $('#{0}').on('uncross', function(){{
            $('#{3}').prop('disabled', false);
        }});
    }};
    $(document).ready(function () {{ SetCharacterLimit(); }});
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(SetCharacterLimit);
";
                string script = string.Format(scriptFormat , dtbRequest.ClientID, charLimit, lblCount.ClientID, lbSave.ClientID );
                ScriptManager.RegisterStartupScript( this.Page, this.GetType(), string.Format( "limit-{0}", this.ClientID ), script, true );
            }
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( ! Page.IsPostBack )
            {
                if ( CurrentPerson != null )
                {
                    dtbFirstName.Text = CurrentPerson.FirstName;
                    dtbLastName.Text = CurrentPerson.LastName;
                    dtbEmail.Text = CurrentPerson.Email;
                }
            }
        }

        #endregion

        #region Events

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event to save the prayer request.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( ! IsValid() )
            {
                return;
            }

            bool isAutoApproved = GetAttributeValue( "EnableAutoApprove" ).AsBoolean();
            bool defaultAllowComments = GetAttributeValue( "DefaultAllowCommentsSetting" ).AsBoolean();

            PrayerRequest prayerRequest = new PrayerRequest { Id = 0, IsActive = true, IsApproved = isAutoApproved, AllowComments = defaultAllowComments };

            var rockContext = new RockContext();
            PrayerRequestService prayerRequestService = new PrayerRequestService( rockContext );
            prayerRequestService.Add( prayerRequest );
            prayerRequest.EnteredDateTime = RockDateTime.Now;

            if ( isAutoApproved )
            {
                prayerRequest.ApprovedByPersonAliasId = CurrentPersonAliasId;
                prayerRequest.ApprovedOnDateTime = RockDateTime.Now;
                var expireDays = Convert.ToDouble( GetAttributeValue( "ExpireDays" ) );
                prayerRequest.ExpirationDate = RockDateTime.Now.AddDays( expireDays );
            }

            // Now record all the bits...
            int? categoryId = bddlCategory.SelectedValueAsInt();
            Guid defaultCategoryGuid = GetAttributeValue( "DefaultCategory" ).AsGuid();
            if ( categoryId == null && !defaultCategoryGuid.IsEmpty() )
            {
                var category = new CategoryService( rockContext ).Get( defaultCategoryGuid );
                categoryId = category.Id;
            }

            prayerRequest.CategoryId = categoryId;
            prayerRequest.RequestedByPersonAliasId = CurrentPersonAliasId;
            prayerRequest.FirstName = dtbFirstName.Text;
            prayerRequest.LastName = dtbLastName.Text;
            prayerRequest.Email = dtbEmail.Text;
            prayerRequest.Text = dtbRequest.Text;
            
            if ( this.EnableUrgentFlag )
            {
                prayerRequest.IsUrgent = cbIsUrgent.Checked;
            }
            else
            {
                prayerRequest.IsUrgent = false;
            }

            if ( this.EnableCommentsFlag )
            {
                prayerRequest.AllowComments = cbAllowComments.Checked;
            }

            if ( this.EnablePublicDisplayFlag )
            {
                prayerRequest.IsPublic = cbAllowPublicDisplay.Checked;
            }
            else
            {
                prayerRequest.IsPublic = false;
            }

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !prayerRequest.IsValid )
            {
                // field controls render error messages
                return;
            }

            rockContext.SaveChanges();

            bool isNavigateToParent = GetAttributeValue( "NavigateToParentOnSave" ).AsBoolean();

            if ( isNavigateToParent )
            {
                NavigateToParentPage();
            }
            else
            {
                pnlForm.Visible = false;
                pnlReceipt.Visible = true;
            }
        }

        /// <summary>
        /// When cancel clicked send them to the homepage
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbPrayerCancel_Click( object sender, EventArgs e )
        {
            GoHome();
        }

        /// <summary>
        /// When done button clicked send them to the homepage
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDone_Click( object sender, EventArgs e )
        {
            GoHome();
        }

        #endregion

        #region Methods

        // redirects to the homepage
        private void GoHome()
        {
            NavigateToLinkedPage( "Homepage" );
        }

        /// <summary>
        /// Bind the category selector to the correct set of categories.
        /// </summary>
        private void BindCategories( string categoryGuid )
        {
            Guid guid = new Guid( categoryGuid );

            bddlCategory.DataSource = new CategoryService( new RockContext() ).GetByEntityTypeId( this.PrayerRequestEntityTypeId ).Where( c => c.Guid == guid ||
                ( c.ParentCategory != null && c.ParentCategory.Guid == guid ) ).AsQueryable().ToList();
            bddlCategory.DataTextField = "Name";
            bddlCategory.DataValueField = "Id";
            bddlCategory.DataBind();
        }

        /// <summary>
        /// Returns true if the form is valid; false otherwise.
        /// </summary>
        /// <returns></returns>
        private bool IsValid()
        {
            IEnumerable<string> errors = Enumerable.Empty<string>();

            // Check length in case the client side js didn't
            int charLimit = GetAttributeValue( "CharacterLimit" ).AsInteger();
            if ( charLimit > 0  && dtbRequest.Text.Length > charLimit )
            {
                errors = errors.Concat( new[] { string.Format( "Whoops. Would you mind reducing the length of your prayer request to {0} characters?", charLimit ) } );
            }

            if ( errors != null && errors.Count() > 0 )
            {
                nbWarningMessage.Visible = true;
                nbWarningMessage.Text = errors.Aggregate( new StringBuilder( "<ul>" ), ( sb, s ) => sb.AppendFormat( "<li>{0}</li>", s ) ).Append( "</ul>" ).ToString();
                return false;
            }
            else
            {
                nbWarningMessage.Visible = false;
                return true;
            }
        }

        #endregion
    }
}