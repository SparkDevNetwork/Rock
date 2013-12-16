<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AuditInformationDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.AuditInformationDetail" %>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" Visible="true" CssClass="panel panel-default">
            <div class="panel-body">

                <div class="banner"><h1><asp:Literal ID="lActionTitle" runat="server"></asp:Literal></h1></div>
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                
                <fieldset id="fieldsetViewDetails" runat="server">
                    <div class="row">
                        <div class="col-md-12">
                            <asp:Literal ID="lblDetails" runat="server" />
                        </div>                        
                    </div>
                </fieldset>                   

            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
