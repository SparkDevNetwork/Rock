<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarketingCampaignAdTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.MarketingCampaignAdTypeDetail" %>

<asp:UpdatePanel ID="upMarketingCampaignAdType" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" Visible="false">

            <asp:HiddenField ID="hfMarketingCampaignAdTypeId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />

            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row-fluid">
                    <div class="span6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.MarketingCampaignAdType, Rock" PropertyName="Name" />
                        <Rock:RockDropDownList ID="ddlDateRangeType" runat="server" Label="Date Range Type" />
                    </div>
                    <div class="span6">
                        <Rock:Grid ID="gMarketingCampaignAdAttributeTypes" runat="server" AllowPaging="false" DisplayType="Light">
                            <Columns>
                                <asp:BoundField DataField="Name" HeaderText="Attribute Types" />
                                <Rock:EditField OnClick="gMarketingCampaignAdAttributeType_Edit" />
                                <Rock:DeleteField OnClick="gMarketingCampaignAdAttributeType_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlAdTypeAttribute" runat="server" Visible="false">
            <Rock:AttributeEditor ID="edtAdTypeAttributes" runat="server" OnSaveClick="btnSaveAttribute_Click" OnCancelClick="btnCancelAttribute_Click" />
        </asp:Panel>

        <Rock:NotificationBox ID="nbWarning" runat="server" Title="Warning" NotificationBoxType="Warning" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
