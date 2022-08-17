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
                        <Rock:RockTextBox ID="tbRoute" runat="server" Label="Route" Required="true" Help="The web route you want to deep link to your mobile application. Follows the deep link path prefix."/>
                        <Rock:PagePicker ID="ppMobilePage" runat="server" Label="Mobile Page" Required="true" Help="The mobile page you want the web route to direct to." />
                        <Rock:Toggle ID="tglFallbackType" runat="server" OnCheckedChanged="tglFallbackType_CheckedChanged" Label="Fallback Method" OnText="URL" OffText="Page" ButtonSizeClass="btn-xs" Help="If someone goes to a deep link, and they do not have the mobile application installed or are on a different platform, you can either redirect them to a page directly or to an external URL. The route and query string parameters that were initially passed in will be passed along."/>
                        <Rock:PagePicker ID="ppFallbackPage" runat="server" Label="Fallback Page" PromptForPageRoute="true" Help="The page to fallback to." />
                        <Rock:UrlLinkBox ID="tbFallbackUrl" runat="server" Label="Fallback URL" Help="The URL to fallback to, if you included route paramters (ex: {GroupGuid}) in the route, they will be replaced in the fallback URL, if included." />
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