<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarketingCampaigns.ascx.cs" Inherits="MarketingCampaigns" %>

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
                        <Rock:Grid ID="gMarketingCampaignAds" runat="server" AllowPaging="false" ShowActionExcelExport="false">
                            <Columns>
                                <asp:BoundField DataField="Value.Name" HeaderText="Ad Type" />
                                <asp:BoundField DataField="Value.DateText" HeaderText="Date" />
                                <asp:BoundField DataField="Value.StatusText" HeaderText="Approval Status" />
                                <asp:BoundField DataField="Value.Priority" HeaderText="Priority" />
                                <Rock:EditField OnClick="gMarketingCampaignAds_Edit" />
                                <Rock:DeleteField OnClick="gMarketingCampaignAds_Delete" />
                            </Columns>
                        </Rock:Grid>
                        <Rock:Grid ID="gMarketingCampaignAudiences" runat="server" AllowPaging="false" ShowActionExcelExport="false">
                            <Columns>
                                <asp:BoundField DataField="Value.AudienceTypeValueName" HeaderText="Audience" />
                                <Rock:BoolField DataField="Value.IsPrimary" HeaderText="Primary" />
                                <Rock:DeleteField OnClick="gMarketingCampaignAudiences_Delete" />
                            </Columns>
                        </Rock:Grid>
                        <Rock:Grid ID="gCampuses" runat="server" AllowPaging="false" ShowActionExcelExport="false">
                            <Columns>
                                <asp:BoundField DataField="Value" HeaderText="Campus" />
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

        <asp:Panel ID="pnlMarketingCampaignAudiencePicker" runat="server" Visible="false">
            <Rock:DataDropDownList ID="ddlMarketingCampaignAudiences" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Cms.MarketingCampaignAudience, Rock" PropertyName="Name" LabelText="Select Audiences" />
            <Rock:LabeledCheckBox ID="ckMarketingCampaignAudienceIsPrimary" runat="server" LabelText="Primary Audience" />

            <div class="actions">
                <asp:LinkButton ID="btnAddMarketingCampaignAudience" runat="server" Text="Add" CssClass="btn primary" OnClick="btnAddMarketingCampaignAudience_Click"></asp:LinkButton>
                <asp:LinkButton ID="btnCancelAddMarketingCampaignAudience" runat="server" Text="Cancel" CssClass="btn secondary" OnClick="btnCancelAddMarketingCampaignAudience_Click"></asp:LinkButton>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlCampusPicker" runat="server" Visible="false">
            <Rock:DataDropDownList ID="ddlCampus" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Crm.Campus, Rock" PropertyName="Name" LabelText="Select Campus" />

            <div class="actions">
                <asp:LinkButton ID="btnAddCampus" runat="server" Text="Add" CssClass="btn primary" OnClick="btnAddCampus_Click"></asp:LinkButton>
                <asp:LinkButton ID="btnCancelAddCampus" runat="server" Text="Cancel" CssClass="btn secondary" OnClick="btnCancelAddCampus_Click"></asp:LinkButton>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlMarketingCampaignAdEditor" runat="server" Visible="false">
            <Rock:LabeledTextBox ID="tbCampaignTitle" runat="server" LabelText="Campaign Title" ReadOnly="true" />
            <Rock:DataDropDownList ID="ddlMarketingCampaignAdType" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Cms.MarketingCampaignAdType, Rock" PropertyName="Name" LabelText="Ad Type" />
            <Rock:DataTextBox ID="tbPriority" runat="server" SourceTypeName="Rock.Cms.MarketingCampaignAd, Rock" PropertyName="Priority" LabelText="Priority" />
            <Rock:LabeledDropDownList ID="ddlMarketingCampaignAdStatus" runat="server" LabelText="Approval Status" />
            <!-- ToDo: Better Person picker -->
            <Rock:DataDropDownList ID="ddlMarketingCampaignAdStatusPerson" runat="server" DataTextField="Fullname" DataValueField="Id" SourceTypeName="Rock.Crm.Person, Rock" PropertyName="FullName" LabelText="Contact" AutoPostBack="true" OnSelectedIndexChanged="ddlContactPerson_SelectedIndexChanged" />
            <Rock:DataTextBox ID="tbStartDate" runat="server" SourceTypeName="Rock.Cms.MarketingCampaignAd, Rock" PropertyName="StartDate" LabelText="Start Date" />
            <Rock:DataTextBox ID="tbEndDate" runat="server" SourceTypeName="Rock.Cms.MarketingCampaignAd, Rock" PropertyName="EndDate" LabelText="End Date" />
            <Rock:DataTextBox ID="tbUrl" runat="server" SourceTypeName="Rock.Cms.MarketingCampaignAd, Rock" PropertyName="Url" LabelText="Url" />
            <Rock:LabeledTextBox ID="tbSummaryText" runat="server" LabelText="Summary Text" />
            <Rock:CKEditorControl ID="tbDetailEditor" runat="server" />
            <Rock:ImageSelector ID="imgSummaryImage" runat="server" ImageId="0" />
            <Rock:ImageSelector ID="imgDetailImage" runat="server" ImageId="0" />

            <div class="actions">
                <asp:LinkButton ID="btnSaveAd" runat="server" Text="Add" CssClass="btn primary" OnClick="btnSaveAd_Click"></asp:LinkButton>
                <asp:LinkButton ID="btnCancelAd" runat="server" Text="Cancel" CssClass="btn secondary" OnClick="btnCancelAd_Click"></asp:LinkButton>
            </div>
        </asp:Panel>

        <Rock:NotificationBox ID="nbWarning" runat="server" Title="Warning" NotificationBoxType="Warning" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
