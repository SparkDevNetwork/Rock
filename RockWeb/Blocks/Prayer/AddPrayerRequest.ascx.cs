//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
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
    [IntegerField( "Category Id", "The id of a top level category. This controls which categories are shown when entering new prayer requests.", false, -1, "Filtering", 1, "GroupCategoryId" )]

    [BooleanField( "Enable Auto Approve", "If enabled, prayer requests are automatically approved; otherwise they must be approved by an admin before they can be seen by the prayer team.", true, "Behavior", 0 )]
    [BooleanField( "Default Allow Comments Setting", "Controls the default setting for 'Allow Comments' on new prayer requests.", false, "Behavior", 1 )]
    [BooleanField( "Enable Urgent Flag", "If enabled, requestors will be able to flag prayer requests as urgent.", false, "Behavior", 2 )]
    [BooleanField( "Enable Comments Flag", "If enabled, requestors will be able set whether or not they want to allow comments on their requests.", false, "Behavior", 3 )]
    [BooleanField( "Enable Public Display Flag", "If enabled, requestors will be able set whether or not they want their request displayed on the public website.", false, "Behavior", 4 )]
    [IntegerField( "Character Limit", "If set to something other than 0, this will limit the number of characters allowed when entering a new prayer request.", false, 250, "Behavior", 5 )]

    [BooleanField( "Navigate To Parent On Save", "If enabled, on successful save control will redirect back to the parent page.", false, "On Save", 10 )]
    [TextField("Save Success Text", "Some text (or HTML) to display to the requester upon successful save.", false, "<p>Thank you allowing us to pray for you.</p>", "On Save", 11 )]
    
    public partial class AddPrayerRequest : RockBlock
    {
        #region Private BlockType Attributes
        int _groupCategoryId = -1;
        protected int? _prayerRequestEntityTypeId = null;
        protected bool enableUrgentFlag = false;
        protected bool enableCommentsFlag = false;
        protected bool enablePublicDisplayFlag = false;
        #endregion

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            enableUrgentFlag = GetAttributeValue( "EnableUrgentFlag" ).AsBoolean();
            enableCommentsFlag = GetAttributeValue( "EnableCommentsFlag" ).AsBoolean();
            enablePublicDisplayFlag = GetAttributeValue( "EnablePublicDisplayFlag" ).AsBoolean();
            litSaveSuccess.Text = GetAttributeValue( "SaveSuccessText" );

            RockPage.AddScriptLink( Page, ResolveUrl( "~/Scripts/bootstrap-limit.js" ) );

            Int32.TryParse( GetAttributeValue( "GroupCategoryId" ), out _groupCategoryId );
            Type type = new PrayerRequest().GetType();
            _prayerRequestEntityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( type.FullName );

            int charLimit;
            if ( Int32.TryParse( GetAttributeValue( "CharacterLimit" ), out charLimit ) && charLimit > 0 )
            {
                string script = string.Format( @"
$(document).ready(function() {{
    $('#{0}').limit({{maxChars: {1}, counter:'#{2}', normalClass:'badge', warningClass:'badge-warning', overLimitClass: 'badge-important'}});

    $('#{0}').on('cross', function(){{
        $('#{3}').addClass('disabled');
        $('#{2}').addClass('label-important');
    }})
    $('#{0}').on('uncross', function(){{
        $('#{3}').removeClass('disabled')
        $('#{2}').removeClass('label-important');
    }})
}});
", txtRequest.ClientID, charLimit, lblCount.ClientID, btnSave.ClientID );
                ScriptManager.RegisterStartupScript( this.Page, this.GetType(), string.Format( "limit-{0}", this.ClientID ), script, true );
            }

            BindCategories();
        }

        /// <summary>
        /// Bind the category selector to the correct set of categories.
        /// </summary>
        private void BindCategories()
        {
            rblCategory.DataSource = new CategoryService().GetByEntityTypeId( _prayerRequestEntityTypeId ).Where( c => c.Id == _groupCategoryId || c.ParentCategoryId == _groupCategoryId ).AsQueryable().ToList();
            rblCategory.DataTextField = "Name";
            rblCategory.DataValueField = "Id";
            rblCategory.DataBind();
        }

        /// <summary>
        /// Handles the Click event to save the prayer request.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            SaveRequest();
        }


        /// <summary>
        /// Saves the prayer request.
        /// </summary>
        private void SaveRequest()
        {
            bool isAutoApproved = GetAttributeValue( "EnableAutoApprove" ).AsBoolean();
            bool defaultAllowComments = GetAttributeValue( "DefaultAllowCommentsSetting" ).AsBoolean();

            PrayerRequest prayerRequest = new PrayerRequest { Id = 0, IsActive = true, IsApproved = isAutoApproved, AllowComments = defaultAllowComments };

            PrayerRequestService prayerRequestService = new PrayerRequestService();
            prayerRequestService.Add( prayerRequest, CurrentPersonId );
            prayerRequest.EnteredDate = DateTime.Now;
            
            // If changing from NOT approved to approved, record who and when
            if ( isAutoApproved )
            {
                prayerRequest.ApprovedByPersonId = CurrentPerson.Id;
                prayerRequest.ApprovedOnDate = DateTime.Now;
            }

            // Now record all the bits...
            prayerRequest.CategoryId = rblCategory.SelectedValueAsInt();
            prayerRequest.FirstName = tbFirstName.Text;
            prayerRequest.LastName = tbLastName.Text;
            prayerRequest.Text = txtRequest.Text;

            if ( enableUrgentFlag )
            {
                prayerRequest.IsUrgent = cbIsUrgent.Checked;
            }

            if ( enableCommentsFlag )
            {
                prayerRequest.AllowComments = cbAllowComments.Checked;
            }

            if ( enablePublicDisplayFlag )
            {
                prayerRequest.IsPublic = cbAllowPublicDisplay.Checked;
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

            prayerRequestService.Save( prayerRequest, CurrentPersonId );

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
        /// Set up the form for another request from the same person.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnAddAnother_Click( object sender, EventArgs e )
        {
            pnlForm.Visible = true;
            pnlReceipt.Visible = false;
            txtRequest.Text = "";
            //rblCategory.SelectedIndex = -1;
            txtRequest.Focus();
        }
}
}