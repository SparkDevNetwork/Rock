using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace RockWeb.Plugins.org_lakepointe.Groups
{
    [DisplayName("Group Attribute Bulk Update")]
    [Category("LPC > Groups")]
    [Description("Tool to bulk update group attribute values for a group and child groups.")]

    public partial class GroupAttributeBulkUpdate : RockBlock
    {

        #region Fields
        RockContext _context = null;

        Group _baseGroup = null;
        #endregion

        #region Properties
        private List<AttributeCache> AttributeList { get; set; }
        private int BaseGroupID { get; set; }
        private Group BaseGroup
        {
            get
            {
                if (_baseGroup == null && BaseGroupID > 0)
                {
                    _baseGroup = new GroupService(_context).Get(BaseGroupID);
                }
                return _baseGroup;
            }
            set
            {
                _baseGroup = value;
                BaseGroupID = value.Id;
            }
        }
        List<string> SelectedFields { get; set; }

        private Dictionary<int, string> UpdatedAttributes { get; set; }

        #endregion

        #region Base Control Methods
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            SelectedFields = new List<string>();
            if (_context == null)
            {
                _context = new RockContext();
            }

            // Adapted from /Blocks/CRM/BulkUpdate.ascx.cs Ln:180
            var scriptBuilder = new System.Text.StringBuilder();
            scriptBuilder.AppendLine("$(document).ready(function() {");
            scriptBuilder.AppendLine("  $('span.js-select-item').click(function() {");
            scriptBuilder.AppendLine("      var selectIcon = $(this).children('i'); ");
            scriptBuilder.AppendLine("      var formGroup = $(this).parent().parent().children('.attributeField').first().children('.form-group');");
            scriptBuilder.AppendLine("      formGroup.toggleClass('bulk-item-selected');");
            scriptBuilder.AppendLine("      var enabled = formGroup.hasClass('bulk-item-selected');");
            scriptBuilder.AppendLine("      selectIcon.toggleClass('fa-check-circle-o', enabled);");
            scriptBuilder.AppendLine("      selectIcon.toggleClass('fa-circle-o', !enabled);");
            scriptBuilder.Append("\n\n");

            scriptBuilder.Append(
@"        // Checkboxes needs special handling
        var checkboxes = formGroup.find(':checkbox');
        if ( checkboxes.length ) {
            $(checkboxes).each(function() {
                if (this.nodeName === 'INPUT' ) {
                    $(this).toggleClass('aspNetDisabled', !enabled);
                    $(this).prop('disabled', !enabled);
                    $(this).closest('label').toggleClass('text-muted', !enabled);
                    $(this).closest('.form-group').toggleClass('bulk-item-selected', enabled);
                }
            });
        }");
            scriptBuilder.Append("\n\n");
            scriptBuilder.Append(
@"      // Enable/Disable the controls
        formGroup.find('.form-control').each( function() {

            $(this).toggleClass('aspNetDisabled', !enabled);
            $(this).prop('disabled', !enabled);
        });");
            scriptBuilder.Append("\n\n");
            scriptBuilder.Append(
@"        // Update the hidden field with the client id of each selected control, (if client id ends with '_hf' as in the case of multi-select attributes, strip the ending '_hf').
        var newValue = '';
        $('div.bulk-item-selected').each(function( index ) {
            $(this).find('[id]').each(function() {
                var re = /_hf$/;
                var ctrlId = $(this).prop('id').replace(re, '');
                newValue += ctrlId + '|';
            });
        });");
            scriptBuilder.Append("\n");
            scriptBuilder.AppendFormat(
"       $('#{0}').val(newValue);\n\n", hfSelectedFields.ClientID);
            scriptBuilder.Append("\n\n");
            scriptBuilder.AppendLine("  });");
            scriptBuilder.AppendLine("});");

            ScriptManager.RegisterStartupScript(upMain, upMain.GetType(), "select-items" + DateTime.Now.Ticks, scriptBuilder.ToString(), true);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                BaseGroupID = PageParameter("GroupId").AsInteger();
                LoadGroup();
            }
            else
            {
                LoadGroupAttributes(false);
            }

        }

        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);

            if (ViewState["BaseGroupId"] != null)
            {
                BaseGroupID = (int)ViewState["BaseGroupId"];

                string selectedItemsValue = Request.Form[hfSelectedFields.UniqueID];
                if (!string.IsNullOrWhiteSpace(selectedItemsValue))
                {
                    SelectedFields = selectedItemsValue.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                else
                {
                    SelectedFields = new List<string>();
                }
            }

            if (ViewState["UpdatedAttributes"] != null)
            {
                UpdatedAttributes = (Dictionary<int, string>)ViewState["UpdatedAttributes"];
            }
        }

        protected override object SaveViewState()
        {
            ViewState["BaseGroupId"] = BaseGroupID;
            ViewState["UpdatedAttributes"] = UpdatedAttributes;
            return base.SaveViewState();
        }
        #endregion

        #region Events
        protected void btnBack_Click(object sender, EventArgs e)
        {
            pnlGroupInfo.Visible = true;
            pnlConfirm.Visible = false;
        }

        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            int groupsUpdated = UpdateGroupAttributes();
            string message = string.Format("<p>You have successfully updated {0} {1} in {2} {3}.",
                    UpdatedAttributes.Count,
                    "attribute".PluralizeIf(UpdatedAttributes.Count != 1),
                    groupsUpdated,
                    "group".PluralizeIf(groupsUpdated != 1));

            SetNotification("Success", message, NotificationBoxType.Success);

            SelectedFields.Clear();
            UpdatedAttributes.Clear();
            LoadGroupAttributes(true);

            pnlGroupInfo.Visible = true;
            pnlConfirm.Visible = false;           
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            SelectedFields.Clear();
            cbUpdateChildGroups.Checked = false;
            LoadGroupAttributes(true);
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            GetUpdatedAttributes();
            if (UpdatedAttributes.Count > 0)
            {
                LoadConfirmationPanel();
                pnlGroupInfo.Visible = false;
                pnlConfirm.Visible = true;
            }
        }

        #endregion

        #region Methods

        private void GetUpdatedAttributes()
        {
            if (BaseGroup == null)
            {
                return;
            }

            if (UpdatedAttributes == null)
            {
                UpdatedAttributes = new Dictionary<int, string>();
            }

            var groupEntityType = EntityTypeCache.Get(Rock.SystemGuid.EntityType.GROUP.AsGuid(), _context);
            var attributeService = new AttributeService(_context);

            var attributes = new List<Rock.Model.Attribute>();
            attributes.AddRange(
                attributeService.GetByEntityTypeQualifier(groupEntityType.Id, "GroupTypeId", BaseGroup.GroupTypeId.ToString(), false)
                .OrderBy(a => a.Order)
                .ThenBy(a => a.Name)
                .ToList());

            var inheritedGroupType = BaseGroup.GroupType.InheritedGroupType;

            while (inheritedGroupType != null)
            {
                attributes.AddRange(
                    attributeService.GetByEntityTypeQualifier(groupEntityType.Id, "GroupTypeId", inheritedGroupType.Id.ToString(), false)
                    .OrderBy(a => a.Order)
                    .ThenBy(a => a.Name)
                    .ToList());

                inheritedGroupType = inheritedGroupType.InheritedGroupType;
            }

            foreach (var attribute in attributes)
            {
                if (!attribute.IsAuthorized(Authorization.EDIT, CurrentPerson))
                {
                    continue;
                }
                var attributeCache = AttributeCache.Get(attribute.Id, _context);
                HtmlGenericControl divParent = (HtmlGenericControl)phAttributes.FindControl(string.Format("attribute_div_{0}", attribute.Id));

                if (divParent != null)
                {
                    HtmlGenericControl divAttribute = (HtmlGenericControl)divParent.Controls[0].Controls[1];
                    Control attributeControl = divAttribute.FindControl(string.Format("attribute_field_{0}", attribute.Id));

                    if (attributeControl != null && SelectedFields.Contains(attributeControl.ClientID))
                    {
                        string newValue = attributeCache.FieldType.Field.GetEditValue(attributeControl, attributeCache.QualifierValues);

                        if (UpdatedAttributes.ContainsKey(attributeCache.Id))
                        {
                            UpdatedAttributes[attributeCache.Id] = newValue;
                        }
                        else
                        {
                            UpdatedAttributes.Add(attributeCache.Id, newValue);
                        }
                    }
                    else
                    {
                        if (UpdatedAttributes.ContainsKey(attributeCache.Id))
                        {
                            UpdatedAttributes.Remove(attributeCache.Id);
                        }
                    }

                }

            }
        }

        private void LoadConfirmationPanel()
        {
            phConfirm.Controls.Clear();
            int affectedGroups = 0;
            if (cbUpdateChildGroups.Checked)
            {
                affectedGroups = new GroupService(_context).GetAllDescendentGroups(BaseGroup.Id, false)
                    .Count() + 1;
            }
            else
            {
                affectedGroups = 1;
            }

            Literal lWarning = new Literal();
            lWarning.ID = "lWarning";
            lWarning.Text = string.Format("<p>You are about to make the following updates to up to {0} {1}.</p>",
                affectedGroups, "Group".PluralizeIf(affectedGroups != 1));
            phConfirm.Controls.Add(lWarning);

            foreach (var attributeUpdate in UpdatedAttributes)
            {
                var attributeCache = AttributeCache.Get(attributeUpdate.Key, _context);
                RockLiteral lAttribute = new RockLiteral();
                lAttribute.ID = string.Format("attribute_confirm_{0}", attributeCache.Id);
                lAttribute.Label = attributeCache.Name;
                lAttribute.Text = attributeUpdate.Value.IsNotNullOrWhiteSpace() ? attributeUpdate.Value : "(empty)";
                phConfirm.Controls.Add(lAttribute);
            }
        }

        private void LoadGroup()
        {
            if (BaseGroup == null)
            {
                pnlGroupInfo.Visible = false;
                return;
            }

            lGroupName.Text = BaseGroup.Name;
            lGroupType.Text = BaseGroup.GroupType.Name;
            LoadGroupAttributes(true);

            pnlGroupInfo.Visible = true;
        }

        private void LoadGroupAttributes(bool setValues)
        {
            if (BaseGroup == null)
            {
                return;
            }

            var groupEntityType = EntityTypeCache.Get(Rock.SystemGuid.EntityType.GROUP.AsGuid(), _context);
            var attributeService = new AttributeService(_context);
            int attributeCount = 0;

            phAttributes.Controls.Clear();


            var attributes = new List<Rock.Model.Attribute>();
            attributes.AddRange(
                attributeService.GetByEntityTypeQualifier(groupEntityType.Id, "GroupTypeId", BaseGroup.GroupTypeId.ToString(), false)
                .OrderBy(a => a.Order)
                .ThenBy(a => a.Name)
                .ToList());

            var inheritedGroupType = BaseGroup.GroupType.InheritedGroupType;

            while (inheritedGroupType != null)
            {
                attributes.AddRange(
                    attributeService.GetByEntityTypeQualifier(groupEntityType.Id, "GroupTypeId", inheritedGroupType.Id.ToString(), false)
                    .OrderBy(a => a.Order)
                    .ThenBy(a => a.Name)
                    .ToList());

                inheritedGroupType = inheritedGroupType.InheritedGroupType;
            }

            BaseGroup.LoadAttributes(_context);
            foreach (var attribute in attributes)
            {
                attributeCount++;
                if (attribute.IsAuthorized(Authorization.EDIT, CurrentPerson))
                {
                    //This logic adapted from  /Blocks/CRM/BulkUpdate.ascx.cs Ln:1774
                    var attributeCache = AttributeCache.Get(attribute);
                    var value = setValues ? attributeCache.GetAttributeValue(attributeCache.Key) : string.Empty;
                    var div0 = new HtmlGenericControl("div");
                    div0.ID = string.Format("attribute_div_{0}", attribute.Id);
                    div0.AddCssClass("row attribute-row");
                    phAttributes.Controls.Add(div0);
                    var divParent = new HtmlGenericControl("div");
                    divParent.AddCssClass("col-xs-12");
                    div0.Controls.Add(divParent);
                    var div1 = new HtmlGenericControl("div");
                    div1.AddCssClass("selectItem");
                    divParent.Controls.Add(div1);
                    var div2 = new HtmlGenericControl("div");
                    div2.AddCssClass("attributeField");
                    divParent.Controls.Add(div2);

                    var clientId = string.Format("{0}attribute_field_{1}", phAttributes.ClientID.Replace(phAttributes.ID, ""), attribute.Id);
                    var controlEnabled = SelectedFields.Contains(clientId, StringComparer.OrdinalIgnoreCase);
                    var iconClass = controlEnabled ? @"fa fa-check-circle-o" : @"fa fa-circle-o";
                    var literal = new Literal();
                    literal.Text = string.Format("<span class=\"js-select-item\"><i class=\"{0}\"></i>&nbsp;</span>", iconClass);
                    div1.Controls.Add(literal);

                    var control = attributeCache.AddControl(div2.Controls, value, string.Empty, setValues, true, false);

                    if (!(control is RockCheckBox) && !(control is PersonPicker) && !(control is ItemPicker))
                    {
                        var webControl = control as WebControl;
                        if (webControl != null)
                        {
                            webControl.Enabled = controlEnabled;
                        }

                    }
                }
            }

            if (attributeCount == 0)
            {
                var div1 = new HtmlGenericContainer("div");
                phAttributes.Controls.Add(div1);
                div1.AddCssClass("col-sm-12");
                var nb = new NotificationBox();
                nb.Title = "No Attributes Found";
                nb.Text = "The selected group does not have any attributes associated with it.";
                nb.ID = "nbNoAttributes";
                nb.NotificationBoxType = NotificationBoxType.Info;
                nb.Visible = true;
                div1.Controls.Add(nb);

                pnlGroupInfoFooter.Visible = false;
            }
            else
            {
                pnlGroupInfoFooter.Visible = true;
            }
        }

        private void SetNotification(string title, string message, NotificationBoxType boxType)
        {
            nbGroupAttributeUpdate.Title = title;
            nbGroupAttributeUpdate.Text = message;
            nbGroupAttributeUpdate.NotificationBoxType = boxType;

            if (title.IsNullOrWhiteSpace() && message.IsNullOrWhiteSpace())
            {
                nbGroupAttributeUpdate.Visible = false;
            }
            else
            {
                nbGroupAttributeUpdate.Visible = true;
            }
        }

        private int UpdateGroupAttributes()
        {
            var groupsUpdated = 0;
            var groupIdsToUpdate = new List<int>();
            groupIdsToUpdate.Add(BaseGroup.Id);

            if (cbUpdateChildGroups.Checked)
            {
                groupIdsToUpdate.AddRange(
                    new GroupService(_context).GetAllDescendentGroupIds(BaseGroup.Id, false));
            }

            var attributes = new List<AttributeCache>();

            foreach (var a in UpdatedAttributes)
            {
                attributes.Add(AttributeCache.Get(a.Key, _context));
            }


            foreach (var groupId in groupIdsToUpdate)
            {
                using (var groupContext = new RockContext())
                {
                    var group = new GroupService(groupContext).Get(groupId);

                    if (!group.IsAuthorized(Authorization.EDIT, CurrentPerson))
                    {
                        continue;
                    }

                    group.LoadAttributes(groupContext);
                    bool attributesUpdated = false;
                    foreach (var a in attributes)
                    {
                        var currentValue = group.GetAttributeValue(a.Key);
                        if (!currentValue.Equals(UpdatedAttributes[a.Id]))
                        {
                            group.SetAttributeValue(a.Key, UpdatedAttributes[a.Id]);
                            attributesUpdated = true;
                        }

                    }

                    if (attributesUpdated)
                    {
                        group.SaveAttributeValues(groupContext);
                    }

                    groupsUpdated++;
                }
            }

            return groupsUpdated;
        }

        #endregion

    }
}