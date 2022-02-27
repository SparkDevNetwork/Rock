<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Success.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Success" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

   

    
    <div class="row">
        <div class="checkin-header panel-header">
            <h1><asp:Literal ID="lTitle" runat="server" /></h1>
        </div>
    </div>

    <div class="row">
        <div class="checkin-body panel-body">
            <div class="checkin-scroll-panel">
                <div class="scroller">
                    <asp:Literal ID="lCheckinResultsHtml" runat="server" />
                    <asp:Literal ID="lCheckinQRCodeHtml" runat="server" />
                </div>
            </div>
        </div>
        <br><br>
    </div>

    <div class="row mt-4">
        <div class="panel-body">   
            <div class="checkin-actions">
                <div class="text-center">
                    <asp:LinkButton CssClass="btn btn-primary" ID="lbDone" runat="server" OnClick="lbDone_Click" Text="Done" />
                    <asp:LinkButton CssClass="btn btn-default" ID="lbAnother" runat="server" OnClick="lbAnother_Click" Text="Another Person" />
                </div>
             </div>
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
