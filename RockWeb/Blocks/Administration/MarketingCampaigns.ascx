<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarketingCampaigns.ascx.cs" Inherits="MarketingCampaigns" %>

<asp:UpdatePanel ID="upMarketingCampaigns" runat="server">
    <Triggers>
        <asp:PostBackTrigger ControlID="gMarketingCampaigns" />
    </Triggers>
    <ContentTemplate>

        <asp:Panel ID="pnlList" runat="server">
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:Grid ID="gMarketingCampaigns" runat="server" AllowSorting="true" OnEditRow="gMarketingCampaigns_Edit">
                <Columns>
                    <asp:BoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                    <asp:BoundField DataField="EventGroup.Name" HeaderText="Event Group" SortExpression="EventGroup.Name" />
                    <asp:BoundField DataField="ContactFullName" HeaderText="Contact" SortExpression="ContactFullName" />
                    <Rock:DeleteField OnClick="gMarketingCampaigns_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">

            <asp:HiddenField ID="hfMarketingCampaignId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="failureNotification" />

            <fieldset id="fieldsetEditDetails" runat="server">
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server" /></legend>
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
                        <Rock:CampusPicker ID="cpCampuses" runat="server" />
                        <Rock:Grid ID="gMarketingCampaignAudiencesPrimary" runat="server" DisplayType="Light">
                            <Columns>
                                <asp:BoundField DataField="Name" HeaderText="Primary Audience" />
                                <Rock:DeleteField OnClick="gMarketingCampaignAudiences_Delete" />
                            </Columns>
                        </Rock:Grid>
                        <Rock:Grid ID="gMarketingCampaignAudiencesSecondary" runat="server" DisplayType="Light">
                            <Columns>
                                <asp:BoundField DataField="Name" HeaderText="Secondary Audience" />
                                <Rock:DeleteField OnClick="gMarketingCampaignAudiences_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </fieldset>

            <div class="actions" id="pnlEditDetailsActions" runat="server">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

            <fieldset id="fieldsetViewDetails" runat="server">
                <legend>Marketing Campaign - Ads
                </legend>
                <div class="well">
                    <div class="row-fluid">
                        <asp:Literal ID="lblMainDetails" runat="server" />
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-mini" OnClick="btnEdit_Click" />
                    </div>
                </div>

            </fieldset>

            <div id="pnlMarketingCampaignAds" runat="server" class="">
                <Rock:Grid ID="gMarketingCampaignAds" runat="server" DisplayType="Full" OnEditRow="gMarketingCampaignAds_Edit">
                    <Columns>
                        <asp:BoundField DataField="MarketingCampaignAdType.Name" HeaderText="Ad Type" />
                        <Rock:DateField DataField="StartDate" HeaderText="Date" />
                        <Rock:EnumField DataField="MarketingCampaignAdStatus" HeaderText="Approval Status" />
                        <asp:BoundField DataField="Priority" HeaderText="Priority" />
                        <Rock:DeleteField OnClick="gMarketingCampaignAds_Delete" />
                    </Columns>
                </Rock:Grid>
            </div>

        </asp:Panel>

        <Rock:NotificationBox ID="nbWarning" runat="server" Title="Warning" NotificationBoxType="Warning" Visible="false" />

        <asp:Panel ID="pnlMarketingCampaignAudiencePicker" runat="server" Visible="false">
            <Rock:DataDropDownList ID="ddlMarketingCampaignAudiences" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Cms.MarketingCampaignAudienceDto, Rock"
                PropertyName="Name" LabelText="Select Audiences" />
            <asp:HiddenField ID="hfMarketingCampaignAudienceIsPrimary" runat="server" />
            <div class="actions">
                <asp:LinkButton ID="btnAddMarketingCampaignAudience" runat="server" Text="Add" CssClass="btn btn-primary" OnClick="btnAddMarketingCampaignAudience_Click"></asp:LinkButton>
                <asp:LinkButton ID="btnCancelAddMarketingCampaignAudience" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancelAddMarketingCampaignAudience_Click"></asp:LinkButton>
            </div>
        </asp:Panel>

        <!-- Ad Details Controls -->
        <asp:Panel ID="pnlMarketingCampaignAdEditor" runat="server" Visible="false">
            <asp:HiddenField ID="hfMarketingCampaignAdId" runat="server" />
            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitleAd" runat="server" />
                </legend>
                <div class="row-fluid">
                    <div class="span6">
                        <Rock:DataDropDownList ID="ddlMarketingCampaignAdType" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Cms.MarketingCampaignAdTypeDto, Rock" PropertyName="Name"
                            LabelText="Ad Type" AutoPostBack="true" OnSelectedIndexChanged="ddlMarketingCampaignAdType_SelectedIndexChanged" />
                        <asp:UpdatePanel ID="upAdApproval" runat="server">
                            <ContentTemplate>
                                <Rock:LabeledText ID="ltMarketingCampaignAdStatus" runat="server" LabelText="Approval Status" />
                                <asp:HiddenField ID="hfMarketingCampaignAdStatus" runat="server" />
                                <Rock:LabeledText ID="ltMarketingCampaignAdStatusPerson" runat="server" LabelText="by" />
                                <asp:HiddenField ID="hfMarketingCampaignAdStatusPersonId" runat="server" />
                                <asp:Button ID="btnApproveAd" runat="server" OnClick="btnApproveAd_Click" class="btn btn-primary btn-mini" Text="Approve" />
                                <asp:Button ID="btnDenyAd" runat="server" OnClick="btnDenyAd_Click" class="btn btn-mini" Text="Deny" />
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>

                    <div class="span6">
                        <Rock:DateTimePicker ID="tbAdDateRangeStartDate" runat="server" SourceTypeName="Rock.Cms.MarketingCampaignAd, Rock" PropertyName="StartDate" LabelText="Start Date" DatePickerType="Date" Required="true" />
                        <Rock:DateTimePicker ID="tbAdDateRangeEndDate" runat="server" SourceTypeName="Rock.Cms.MarketingCampaignAd, Rock" PropertyName="EndDate" LabelText="End Date" DatePickerType="Date" Required="true" />
                        <Rock:DataTextBox ID="tbUrl" runat="server" SourceTypeName="Rock.Cms.MarketingCampaignAd, Rock" PropertyName="Url" LabelText="Url" />
                        <Rock:DataTextBox ID="tbPriority" runat="server" SourceTypeName="Rock.Cms.MarketingCampaignAd, Rock" PropertyName="Priority" LabelText="Priority" />
                    </div>
                </div>

                <div class="attributes">
                    <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                </div>
                <div class="actions">
                    <asp:LinkButton ID="btnSaveAd" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSaveAd_Click"></asp:LinkButton>
                    <asp:LinkButton ID="btnCancelAd" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancelAd_Click"></asp:LinkButton>
                </div>
            </fieldset>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
