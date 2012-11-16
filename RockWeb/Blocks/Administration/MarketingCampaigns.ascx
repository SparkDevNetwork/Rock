<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarketingCampaigns.ascx.cs" Inherits="MarketingCampaigns" %>

<script type="text/javascript">
    $(document).ready(function () {
        // 
        function ConfigureDatePickers(sender, args) {
            $("#<%=tbAdDateRangeStartDate.ClientID%>").kendoDatePicker();
            $("#<%=tbAdDateRangeEndDate.ClientID%>").kendoDatePicker();
        }

        // create DatePicker from input HTML element on initial page load
        ConfigureDatePickers(null, null);

        // create DatePicker from input HTML element on Ajax Request
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(ConfigureDatePickers);
    });
</script>
<asp:UpdatePanel ID="upMarketingCampaigns" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlList" runat="server">
            <Rock:NotificationBox ID="nbGridWarning" runat="server" Title="Warning" NotificationBoxType="Warning" Visible="false" />
            <Rock:Grid ID="gMarketingCampaigns" runat="server" AllowSorting="true">
                <Columns>
                    <asp:BoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                    <asp:BoundField DataField="EventGroup.Name" HeaderText="Event Group" SortExpression="EventGroup.Name" />
                    <asp:BoundField DataField="ContactFullName" HeaderText="Contact" SortExpression="ContactFullName" />
                    <Rock:EditField OnClick="gMarketingCampaigns_Edit" />
                    <Rock:DeleteField OnClick="gMarketingCampaigns_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">

            <asp:HiddenField ID="hfMarketingCampaignId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="failureNotification" />

            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>
                <div class="row-fluid">
                    <div class="span6">
                        <Rock:DataTextBox ID="tbTitle" runat="server" SourceTypeName="Rock.Cms.MarketingCampaign, Rock" PropertyName="Title" />
                        <!-- ToDo: Better Person picker -->
                        <Rock:DataDropDownList ID="ddlContactPerson" runat="server" DataTextField="Fullname" DataValueField="Id" SourceTypeName="Rock.Crm.Person, Rock" PropertyName="FullName" LabelText="Contact" AutoPostBack="true" OnSelectedIndexChanged="ddlContactPerson_SelectedIndexChanged" />
                        <Rock:DataTextBox ID="tbContactEmail" runat="server" SourceTypeName="Rock.Cms.MarketingCampaign, Rock" PropertyName="ContactEmail" LabelText="Contact Email" />
                        <Rock:DataTextBox ID="tbContactPhoneNumber" runat="server" SourceTypeName="Rock.Cms.MarketingCampaign, Rock" PropertyName="ContactPhoneNumber" LabelText="Contact Phone" />
                        <Rock:DataTextBox ID="tbContactFullName" runat="server" SourceTypeName="Rock.Cms.MarketingCampaign, Rock" PropertyName="ContactFullName" LabelText="Contact Name" />
                        <Rock:DataDropDownList ID="ddlEventGroup" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Crm.Group, Rock" PropertyName="Name" LabelText="Event Group" />
                    </div>
                    <div class="span6">
                        <Rock:Grid ID="gMarketingCampaignAds" runat="server" DisplayType="Light">
                            <Columns>
                                <asp:BoundField DataField="Name" HeaderText="Ad Type" />
                                <Rock:DateField DataField="StartDate" HeaderText="Date" />
                                <Rock:EnumField DataField="MarketingCampaignAdStatus" HeaderText="Approval Status" />
                                <asp:BoundField DataField="Priority" HeaderText="Priority" />
                                <Rock:EditField OnClick="gMarketingCampaignAds_Edit" />
                                <Rock:DeleteField OnClick="gMarketingCampaignAds_Delete" />
                            </Columns>
                        </Rock:Grid>
                        <Rock:Grid ID="gMarketingCampaignAudiences" runat="server" DisplayType="Light">
                            <Columns>
                                <asp:BoundField DataField="Name" HeaderText="Audience" />
                                <Rock:BoolField DataField="IsPrimary" HeaderText="Primary" />
                                <Rock:DeleteField OnClick="gMarketingCampaignAudiences_Delete" />
                            </Columns>
                        </Rock:Grid>
                        <Rock:Grid ID="gCampuses" runat="server" DisplayType="Light">
                            <Columns>
                                <asp:BoundField DataField="Name" HeaderText="Campus" />
                                <Rock:DeleteField OnClick="gCampus_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn secondary" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>

        <Rock:NotificationBox ID="nbWarning" runat="server" Title="Warning" NotificationBoxType="Warning" Visible="false" />

        <asp:Panel ID="pnlMarketingCampaignAudiencePicker" runat="server" Visible="false">
            <Rock:DataDropDownList ID="ddlMarketingCampaignAudiences" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Cms.MarketingCampaignAudienceDto, Rock"
                PropertyName="Name" LabelText="Select Audiences" />
            <Rock:LabeledCheckBox ID="ckMarketingCampaignAudienceIsPrimary" runat="server" LabelText="Primary Audience" />

            <div class="actions">
                <asp:LinkButton ID="btnAddMarketingCampaignAudience" runat="server" Text="Add" CssClass="btn primary" OnClick="btnAddMarketingCampaignAudience_Click"></asp:LinkButton>
                <asp:LinkButton ID="btnCancelAddMarketingCampaignAudience" runat="server" Text="Cancel" CssClass="btn secondary" OnClick="btnCancelAddMarketingCampaignAudience_Click"></asp:LinkButton>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlCampusPicker" runat="server" Visible="false">
            <Rock:DataDropDownList ID="ddlCampus" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Crm.Campus, Rock"
                PropertyName="Name" LabelText="Select Campus" />

            <div class="actions">
                <asp:LinkButton ID="btnAddCampus" runat="server" Text="Add" CssClass="btn primary" OnClick="btnAddCampus_Click"></asp:LinkButton>
                <asp:LinkButton ID="btnCancelAddCampus" runat="server" Text="Cancel" CssClass="btn secondary" OnClick="btnCancelAddCampus_Click"></asp:LinkButton>
            </div>
        </asp:Panel>

        <!-- Ad Details Controls -->
        <asp:Panel ID="pnlMarketingCampaignAdEditor" runat="server" Visible="false">
            <asp:HiddenField ID="hfMarketingCampaignAdGuid" runat="server" />
            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitleAd" runat="server" />
                </legend>
                <div class="row-fluid">
                    <div class="span6">
                        <Rock:DataDropDownList ID="ddlMarketingCampaignAdType" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Cms.MarketingCampaignAdTypeDto, Rock" PropertyName="Name"
                            LabelText="Ad Type" AutoPostBack="true" OnSelectedIndexChanged="ddlMarketingCampaignAdType_SelectedIndexChanged" />
                        <Rock:LabeledDropDownList ID="ddlMarketingCampaignAdStatus" runat="server" LabelText="Approval Status" />
                        <!-- ToDo: Better Person picker -->
                        <Rock:DataDropDownList ID="ddlMarketingCampaignAdStatusPerson" runat="server" DataTextField="Fullname"
                            DataValueField="Id" SourceTypeName="Rock.Crm.Person, Rock" PropertyName="FullName" LabelText="Approver" />
                    </div>

                    <div class="span6">
                        <Rock:DataTextBox ID="tbAdDateRangeStartDate" runat="server" SourceTypeName="Rock.Cms.MarketingCampaignAd, Rock" PropertyName="StartDate" LabelText="Start Date" />
                        <Rock:DataTextBox ID="tbAdDateRangeEndDate" runat="server" SourceTypeName="Rock.Cms.MarketingCampaignAd, Rock" PropertyName="EndDate" LabelText="End Date" />
                        <Rock:DataTextBox ID="tbUrl" runat="server" SourceTypeName="Rock.Cms.MarketingCampaignAd, Rock" PropertyName="Url" LabelText="Url" />
                        <Rock:DataTextBox ID="tbPriority" runat="server" SourceTypeName="Rock.Cms.MarketingCampaignAd, Rock" PropertyName="Priority" LabelText="Priority" />
                    </div>
                </div>

                <div class="attributes">
                    <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                </div>
                <div class="actions">
                    <asp:LinkButton ID="btnSaveAd" runat="server" Text="Save" CssClass="btn primary" OnClick="btnSaveAd_Click"></asp:LinkButton>
                    <asp:LinkButton ID="btnCancelAd" runat="server" Text="Cancel" CssClass="btn secondary" OnClick="btnCancelAd_Click"></asp:LinkButton>
                </div>
            </fieldset>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
