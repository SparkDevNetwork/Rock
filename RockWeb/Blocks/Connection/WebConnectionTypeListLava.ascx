<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WebConnectionTypeListLava.ascx.cs" Inherits="RockWeb.Blocks.Connection.WebConnectionTypeListLava" %>

<asp:UpdatePanel ID="upConnectionSelectLava" runat="server">
    <ContentTemplate>
        <!-- Content -->
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel-body">
                <div class="row">
                    <div class="col-xs-12">
                        <asp:Literal ID="lContent" runat="server"></asp:Literal>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
