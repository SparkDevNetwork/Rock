<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelItemVimeoSync.ascx.cs" Inherits="RockWeb.Plugins.rocks_kfs.Vimeo.VimeoSync" %>
<asp:UpdatePanel ID="upnlSync" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlVimeoSync" CssClass="panel panel-block" runat="server" Visible="false">
            <div class="panel-heading"><i class="fa fa-vimeo-square" aria-hidden="true"></i>&nbsp;Vimeo Sync</div>
            <div class="panel-body">
                <asp:Panel ID="pnlNewDetails" runat="server" Visible="false" class="row">
                    <div class="col-sm-2">
                        <Rock:RockTextBox runat="server" ID="tbVimeoId" TextMode="Number" Label="Vimeo Id" ValidationGroup="VimeoSync" Required="true" />
                    </div>
                    <div class="col-sm-2">
                        <Rock:DatePicker ID="dpStart" runat="server" Label="Start" ValidationGroup="VimeoSync" Required="true" />
                    </div>
                </asp:Panel>
                <div class="row">
                    <div class="col-sm-10">
                        <asp:CheckBoxList runat="server" ID="cblSyncOptions" RepeatDirection="Horizontal"></asp:CheckBoxList>
                    </div>
                    <div class="col-sm-2">
                        <Rock:BootstrapButton runat="server" ID="btnVimeoSync" Text="Run Vimeo Sync" OnClick="btnVimeoSync_Click" CssClass="btn btn-primary pull-right" ValidationGroup="VimeoSync" />
                    </div>
                </div>
                <div class="row">
                    <br />
                    <asp:Literal runat="server" ID="litPreview" Visible="false"></asp:Literal>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
