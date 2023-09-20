<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MinPassSettings.ascx.cs" Inherits="RockWeb.Plugins.com_mineCartStudio.MinePass.MinPassSettings" %>

<asp:UpdatePanel ID="upnlAccounts" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlEdit" runat="server">
            <div class="well">
                <div class="row">
                    <div class="col-md-6">
                        <div class="center-block" style="max-width: 200px; text-align: center;">
                            <asp:Image ID="imgMineCartLogo" CssClass="img-responsive" runat="server" ImageUrl="~/Plugins/com_mineCartStudio/MinePass/Assets/Images/minecart.svg" />
                        </div>
                        The Mine Pass service requires an API key from Mine Cart Studio. You can sign-up for a 
                        key on <a href="https://www.minecart.com/minepass">the Mine Pass website</a>.
                    </div>

                    <div class="col-md-6">
                        <div class="margin-t-xl">
                            <Rock:RockTextBox ID="txtApiKey" runat="server" Label="Mine Pass API Key" />
                            <asp:LinkButton ID="btnSaveApiKey" runat="server" CssClass="btn btn-primary btn-xs" Text="Save" OnClick="btnSaveApiKey_Click" />
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlStatus" runat="server">
            <div class="well">
                Mine Pass Service: <span class="label label-success">Active</span>

                <div class="pull-right">
                    <asp:LinkButton ID="btnEditSettings" runat="server" CssClass="btn btn-default btn-xs" Text="Edit Settings" OnClick="btnEditSettings_Click" />
                </div>
            </div>                 
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
