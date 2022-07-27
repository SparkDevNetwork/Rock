<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MobileDeepLinkDetail.ascx.cs" Inherits="RockWeb.Blocks.Mobile.MobileDeepLinkDetail" %>
<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" />

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h3 class="panel-title">Deep Links</h3>
            </div>

            <div class="panel-body">
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbRoute" runat="server" Label="Route" Required="true" />
                        <Rock:PagePicker ID="ppMobilePage" runat="server" Label="Mobile Page" Required="true" />
                        <Rock:Toggle ID="tglFallbackType" runat="server" OnCheckedChanged="tglFallbackType_CheckedChanged" Label="Fallback Page" OnText="URL" OffText="Page" />
                        <Rock:PagePicker ID="ppFallbackPage" runat="server" Label="Page" />
                        <Rock:UrlLinkBox ID="tbFallbackUrl" runat="server" Label="URL" />
                    </div>
                </div>
                <div class="actions">
                    <asp:LinkButton ID="lbSave" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="lbSave_Click" />
                    <asp:LinkButton ID="lbCancel" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="lbCancel_Click" CausesValidation="false" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>