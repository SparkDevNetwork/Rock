using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using OpenXmlPowerTools;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;


namespace RockWeb.Plugins.org_lakepointe.CheckIn
{
    [DisplayName("Acknowledgement Accept")]
    [Category("LPC > Check-in")]
    [Description("Displays acknowledgements for the user to accept prior to Check-in")]

    [LinkedPage("Next Page (Family Check-in)", "", false, "", "", 5, "FamilyNextPage")]

    [TextField("Title", "Title to display", false, "", "Text", 7)]
    [TextField("Caption", "", false, "Please Review", "Text", 8)]
    [TextField("Accept Button Text", "The text displayed on the accept button", false, "Accept", "Text", 9)]

    [TextField("Acknowledgement Attribute Key", "The key of the Check-in Configuration attribute that contains the attribute message. This overrides the \"Acknowledgement Message\" attribute value.", false, "", "Text", 10)]
    [CodeEditorField("Acknowledgement Message", "The lava template for acknowledgemessage. Overriden by the Acknowledgement Attribute Key", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, true, "", "Text", 10)]
    //[LavaCommandsField("Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, "", "Text")]

    public partial class CheckinAcknowledgements : CheckInBlock
    {
        private GroupTypeCache mSelectedGroupType = null;

        public GroupTypeCache SelectedGroupType
        {
            get
            {
                if (mSelectedGroupType == null)
                {
                    LoadCheckinConfigGroupType();
                }
                return mSelectedGroupType;
            }
        }

        #region Base Control Methods
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            RockPage.AddScriptLink("~/Scripts/CheckinClient/checkin-core.js");

            var bodyTag = this.Page.Master.FindControl("bodyTag") as HtmlGenericControl;

            if (bodyTag != null)
            {
                bodyTag.AddCssClass("checkin-acknowledgementreview-bg");
            }

            if (CurrentWorkflow == null || CurrentCheckInState == null)
            {
                NavigateToHomePage();
                return;
            }
            if (!Page.IsPostBack)
            {
                if (UserBackedUp)
                {
                    GoBack();
                    return;
                }

                if (!ShowAcknowledgement())
                {
                    ProcessSelection(maWarning);
                }
                else
                {
                    BuildAcknowledgement(); 
                }
            }
        }
        #endregion

        #region Events
        protected void lbAcknowledge_Click(object sender, EventArgs e)
        {
            ProcessSelection(maWarning, false);
        }

        protected void lbBack_Click(object sender, EventArgs e)
        {
            GoBack();
        }
        #endregion 

        protected void lbCancel_Click(object sender, EventArgs e)
        {
            CancelCheckin();
        }

        #region Methods

        private void BuildAcknowledgement()
        {
            ltitle.Text = GetAttributeValue("Title");
            lCaption.Text = GetAttributeValue("Caption");

            var message = String.Empty;
            var ackKey = GetAttributeValue("AcknowledgementAttributeKey");

            if (ackKey.IsNotNullOrWhiteSpace())
            {
                message = SelectedGroupType.GetAttributeValue(ackKey);
            }

            if (message.IsNotNullOrWhiteSpace())
            {
                lAcknowledgement.Text = message;
            }
            else
            {
                lAcknowledgement.Text = GetAttributeValue("AcknowledgementMessage");
            }
           
            lbAcknowledge.Text = GetAttributeValue("AcceptButtonText");
        }

        private void LoadCheckinConfigGroupType()
        {
            mSelectedGroupType = GroupTypeCache.Get(CurrentCheckInState.CheckinTypeId ?? 0);
        }

        private bool ShowAcknowledgement()
        {
            if (SelectedGroupType != null)
            {
                return SelectedGroupType.GetAttributeValue("ShowAcknowledgementScreen").AsBoolean();
            }

            return false;
        }

        protected override void NavigateToNextPage(Dictionary<string, string> queryParams, bool validateSelectionRequired)
        {
            string pageAttributeKey = "NextPage";
            if( CurrentCheckInType != null &&
                CurrentCheckInType.TypeOfCheckin == TypeOfCheckin.Family &&
                !string.IsNullOrWhiteSpace(LinkedPageUrl("FamilyNextPage")))
            {
                pageAttributeKey = "FamilyNextPage";
            }
            queryParams = CheckForOverride(queryParams);
            NavigateToLinkedPage(pageAttributeKey, queryParams);
        }

        #endregion
    }
}