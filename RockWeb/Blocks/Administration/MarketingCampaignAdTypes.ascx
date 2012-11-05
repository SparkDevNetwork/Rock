<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarketingCampaignAdTypes.ascx.cs" Inherits="MarketingCampaignAdTypes" %>

<asp:UpdatePanel ID="upMarketingCampaignAdType" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" runat="server">
            <Rock:NotificationBox ID="nbGridWarning" runat="server" Title="Warning" NotificationBoxType="Warning" Visible="false" />
            <Rock:Grid ID="gMarketingCampaignAdType" runat="server" AllowSorting="true">
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                    <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                    <Rock:EditField OnClick="gMarketingCampaignAdType_Edit" />
                    <Rock:DeleteField OnClick="gMarketingCampaignAdType_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">

            <asp:HiddenField ID="hfMarketingCampaignAdTypeId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="failureNotification" />

            <fieldset>
                <legend>
                    <i id="iconIsSystem" runat="server" class="icon-eye-open"></i>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>
                <div class="row-fluid">
                    <div class="span6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Cms.MarketingCampaignAdType, Rock" PropertyName="Name" />
                        <Rock:LabeledDropDownList ID="ddlDateRangeType" runat="server" LabelText="Date Range Type" />
                    </div>
                    <div class="span6">
                        <Rock:Grid ID="gMarketingCampaignAdAttributeTypes" runat="server" AllowPaging="false" ShowActionExcelExport="false">
                            <Columns>
                                <asp:BoundField DataField="Value" HeaderText="Attribute Types" />
                                <Rock:DeleteField OnClick="gMarketingCampaignAdAttributeType_Delete" />
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

    </ContentTemplate>
</asp:UpdatePanel>
