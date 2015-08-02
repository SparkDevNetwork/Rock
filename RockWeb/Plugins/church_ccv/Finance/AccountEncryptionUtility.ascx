<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountEncryptionUtility.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Finance.AccountEncryptionUtility" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-lock"></i> Encrypt Account Numbers</h1>
            </div>
            <div class="panel-body">

                    <h4>Encrypt Saved Bank Account</h4>
                    <p>This block will encrypt any new bank account numbers that have been synced to Rock from an external source and not yet encrypted:</p>

                    <div class="actions">
                        <asp:LinkButton ID="lbGo" runat="server" Text="Go!" CssClass="btn btn-default" OnClick="lbGo_Click" />
                    </div>

                    <Rock:NotificationBox ID="nbResult" runat="server" Visible="false" CssClass="margin-t-md" />

                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
