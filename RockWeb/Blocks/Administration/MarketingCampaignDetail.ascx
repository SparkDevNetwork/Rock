<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarketingCampaignDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.MarketingCampaignDetail" %>

<asp:UpdatePanel ID="upMarketingCampaigns" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfMarketingCampaignId" runat="server" />

            <div id="pnlEditDetails" runat="server" class="well">
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />
                <fieldset>
                    <legend>
                        <asp:Literal ID="lActionTitle" runat="server" /></legend>
                    <div class="row-fluid">
                        <div class="span6">
                            <Rock:DataTextBox ID="tbTitle" runat="server" SourceTypeName="Rock.Model.MarketingCampaign, Rock" PropertyName="Title" />
                            <Rock:PersonPicker ID="ppContactPerson" runat="server" Label="Contact" OnSelectPerson="ppContactPerson_SelectPerson" />
                            <Rock:DataTextBox ID="tbContactEmail" runat="server" SourceTypeName="Rock.Model.MarketingCampaign, Rock" PropertyName="ContactEmail" Label="Contact Email" />
                            <Rock:DataTextBox ID="tbContactPhoneNumber" runat="server" SourceTypeName="Rock.Model.MarketingCampaign, Rock" PropertyName="ContactPhoneNumber" Label="Contact Phone" />
                            <Rock:DataTextBox ID="tbContactFullName" runat="server" SourceTypeName="Rock.Model.MarketingCampaign, Rock" PropertyName="ContactFullName" Label="Contact Name" />
                            <Rock:DataDropDownList ID="ddlEventGroup" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Name" Label="Linked Event" />
                        </div>
                        <div class="span6">
                            <Rock:CampusesPicker ID="cpCampuses" runat="server" />
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

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>

            <fieldset id="fieldsetViewDetails" runat="server">
                <legend>Marketing Campaign - Ads
                </legend>
                <div class="well">
                    <div class="row-fluid">
                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    </div>
                    <div class="row-fluid">
                        <asp:Literal ID="lblMainDetails" runat="server" />
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-mini" OnClick="btnEdit_Click" />
                    </div>
                </div>

            </fieldset>

        </asp:Panel>

        <Rock:NotificationBox ID="nbWarning" runat="server" Title="Warning" NotificationBoxType="Warning" Visible="false" />

        <asp:Panel ID="pnlMarketingCampaignAudiencePicker" runat="server" Visible="false">
            <Rock:DataDropDownList ID="ddlMarketingCampaignAudiences" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.MarketingCampaignAudience, Rock"
                PropertyName="Name" Label="Select Audiences" />
            <asp:HiddenField ID="hfMarketingCampaignAudienceIsPrimary" runat="server" />
            <div class="actions">
                <asp:LinkButton ID="btnAddMarketingCampaignAudience" runat="server" Text="Add" CssClass="btn btn-primary" OnClick="btnAddMarketingCampaignAudience_Click"></asp:LinkButton>
                <asp:LinkButton ID="btnCancelAddMarketingCampaignAudience" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancelAddMarketingCampaignAudience_Click"></asp:LinkButton>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
