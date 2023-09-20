<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelItemVimeoSyncAll.ascx.cs" Inherits="RockWeb.Plugins.rocks_kfs.Vimeo.VimeoSyncAll" %>
<asp:UpdatePanel ID="upnlSync" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlVimeoSync" CssClass="panel panel-block" runat="server" Visible="false">
            <div class="panel-heading"><i class="fa fa-vimeo-square" aria-hidden="true"></i>&nbsp;Vimeo Sync</div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-sm-10">
                        <asp:CheckBoxList runat="server" ID="cblSyncOptions" RepeatDirection="Horizontal"></asp:CheckBoxList>
                    </div>
                    <div class="col-sm-2">
                        <Rock:BootstrapButton runat="server" ID="btnVimeoSync" Text="Run Vimeo Sync" OnClick="btnVimeoSync_Click" CssClass="btn btn-primary pull-right" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
