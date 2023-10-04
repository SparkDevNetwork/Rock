using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;


namespace RockWeb.Plugins.org_lakepointe.Communications
{
    [DisplayName("Subscriptions")]
    [Category("LPC > Communications")]
    [Description("Allows a user to set their Content Channel Based Subscriptions.")]
    [CodeEditorField("Header Text", description: "Block header text.", mode: CodeEditorMode.Lava, theme: CodeEditorTheme.Rock, height: 200, required: false, order:0, key:"HeaderText")]
    [LavaCommandsField("Lava Commands", description:"The allowed lava commands.", required:false, order:1, key:"LavaCommands")]
    [LinkedPage("Person Profile Page", description:"Person Profile page to redirect the user to if their email is inactive or not found.", required:true, order:2, key:"PersonProfilePage")]
    public partial class DevotionalSignup : RockBlock
    {
        string devotionalGroupGuidString = "d3ac166d-0157-4542-b150-c6bc5c54d895";
        string lpNewsGroupGuidString = "355e936b-d57e-4e3f-a310-4c6f4bcea411";
        string lpeNewsGroupGuidString = "2d6138d2-bf65-41cd-aad5-7d7664ab745d";
        RockContext _context;
        bool invalidEmail = false;

        #region Base Contol Methods
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            _context = new RockContext();

            this.BlockUpdated += DevotionalSignup_BlockUpdated; ;
            this.AddConfigurationUpdateTrigger(upMain);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ShowNotificationBox(String.Empty, NotificationBoxType.Info);

            if (!Page.IsPostBack)
            {
                LoadHeader();
                LoadDetails();
            }
        }

        #endregion

        #region Events

        private void DevotionalSignup_BlockUpdated(object sender, EventArgs e)
        {
            LoadHeader();
            LoadDetails();
        }

        protected void rptSubscriptionGroups_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var group = e.Item.DataItem as Rock.Model.Group;
            var groupMemberService = new GroupMemberService(_context);
            if(group != null)
            {
                var hfGroupId = e.Item.FindControl("hfGroupId") as HiddenField;
                hfGroupId.Value = group.Id.ToString();
                var cbSubscriptionGroup = e.Item.FindControl("cbSubscriptionGroup") as RockCheckBox;

                var groupMember = groupMemberService.GetByGroupIdAndPersonId(group.Id, CurrentPerson.Id)
                    .Where(gm => gm.GroupMemberStatus == GroupMemberStatus.Active)
                    .FirstOrDefault();

                cbSubscriptionGroup.Text = group.Name;
                cbSubscriptionGroup.Checked = groupMember != null && !invalidEmail;                         
            }
        }

        protected void cbSubscriptionGroup_CheckedChanged(object sender, EventArgs e)
        {
            var item = (sender as RockCheckBox).BindingContainer as RepeaterItem;

            var hfGroupId = item.FindControl("hfGroupId") as HiddenField;
            var cbGroup = item.FindControl("cbSubscriptionGroup") as RockCheckBox;
            if (hfGroupId != null)
            {
                SaveChanges(hfGroupId.Value.AsInteger(), cbGroup.Checked);
            }

            LoadDetails();

        }
        #endregion

        #region Methods
        private void LoadDetails()
        {
            ShowNotificationBox(String.Empty, NotificationBoxType.Info);
            pnlSubscriptions.Visible = false;

            if (CurrentPerson == null)
            {
                ShowNotificationBox("Please login to your profile to update your email subscription.", NotificationBoxType.Info);
                return;
            }

            var person = new PersonService(_context).Get(CurrentPerson.Guid);
            var profilePageUrl = LinkedPageRoute("PersonProfilePage");

            if (person.Email.IsNullOrWhiteSpace())
            {

                var message = string.Format("Please visit your Person Profile page (<a href='{0}'>here</a>) to verify your email address in order to subscribe.", profilePageUrl);

                ShowNotificationBox(message, NotificationBoxType.Warning);

                return;
            }

            if (!person.IsEmailActive)
            {
                var message = string.Format("Messages will be sent to {0}. If this is incorrect please visit your <a href='{1}'>Person Profile</a> to update your email.", person.Email, profilePageUrl);
                ShowNotificationBox(message, NotificationBoxType.Info);
                invalidEmail = true;
            }
            else
            {
                switch (person.EmailPreference)
                {
                    case EmailPreference.NoMassEmails:
                        ShowNotificationBox("<strong>Note:</strong> Signing up for an email subscription will update your email preferences to allow bulk email from Lakepointe Church.", NotificationBoxType.Info);
                        invalidEmail = true;
                        break;
                    case EmailPreference.DoNotEmail:
                        ShowNotificationBox("<strong>Note:</strong> Signing up for an email subscription will update your email preferences to allow bulk email from Lakepointe Church.", NotificationBoxType.Info);
                        invalidEmail = true;
                        break;
                    default:
                        invalidEmail = false;
                        break;
                }
            }

            LoadGroups();

        }
        private void LoadGroups()
        {
            List<Guid> groupGuids = new List<Guid>();
            groupGuids.AddRange(new Guid[] { devotionalGroupGuidString.AsGuid(), lpNewsGroupGuidString.AsGuid(), lpeNewsGroupGuidString.AsGuid() });
            var groups = new GroupService(_context).Queryable().AsNoTracking()
                .Where(g => groupGuids.Contains(g.Guid))
                .Where(g => g.IsActive && !g.IsArchived)
                .OrderBy(g => g.Name)
                .ToList();

            rptSubscriptionGroups.DataSource = groups;
            rptSubscriptionGroups.DataBind();

            if (groups.Count > 0)
            {
                pnlSubscriptions.Visible = true;
            }
            else
            {
                ShowNotificationBox("No Subscription Groups currently available.", NotificationBoxType.Info);
            }
        }

        private void LoadHeader()
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false });

            lHeader.Text = GetAttributeValue("HeaderText").ResolveMergeFields(mergeFields, GetAttributeValue("EnabledLavaCommands"));
        }

        private void SaveChanges(int groupId, bool isActive)
        {
            if (groupId <= 0)
            {
                return;
            }

            var groupMemberContext = new RockContext();
            var person = new PersonService(groupMemberContext).Get(CurrentPerson.Guid);
            var group = new GroupService(groupMemberContext).Get(groupId);

            if (person.EmailPreference != EmailPreference.EmailAllowed)
            {
                person.EmailPreference = EmailPreference.EmailAllowed;
            }

            if (!person.IsEmailActive && person.Email.IsNotNullOrWhiteSpace())
            {
                person.IsEmailActive = true;
            }

            var groupMemberService = new GroupMemberService(groupMemberContext);
            var groupMember = groupMemberService.GetByGroupIdAndPersonId(groupId, person.Id, false).SingleOrDefault();

            if (groupMember != null)
            {
                if (isActive)
                {
                    if (groupMember.IsArchived)
                    {
                        groupMemberService.AddOrRestoreGroupMember(group, person.Id, group.GroupType.DefaultGroupRoleId.Value);
                    }

                    if (groupMember.GroupMemberStatus != GroupMemberStatus.Active)
                    {
                        groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                    }
                }
                else
                {
                    groupMemberService.Delete(groupMember);
                }
            }
            else
            {
                if (isActive)
                {
                    groupMemberService.AddOrRestoreGroupMember(group, person.Id, group.GroupType.DefaultGroupRoleId.Value);
                }
            }

            groupMemberContext.SaveChanges();

        }

        private void ShowNotificationBox(string message, NotificationBoxType boxType)
        {
            nbSubscriptions.Text = message;
            nbSubscriptions.NotificationBoxType = boxType;
            nbSubscriptions.Visible = message.IsNotNullOrWhiteSpace();

        }

        #endregion


    }
}