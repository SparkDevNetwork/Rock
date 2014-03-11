<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Success.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Success" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="row checkin-header">
        <div class="col-md-12">
            <h1>Checked-in</h1>
        </div>
    </div>


    <div class="row checkin-body">
        <div class="col-md-12">
            
            <ol class="checkin-summary checkin-body-container">
                <asp:PlaceHolder ID="phResults" runat="server"></asp:PlaceHolder>
            </ol>
        </div>
    </div>


    <div class="checkin-footer">   
        <div class="checkin-actions">
            <asp:LinkButton CssClass="btn btn-primary" ID="lbDone" runat="server" OnClick="lbDone_Click" Text="Done" />
            <asp:LinkButton CssClass="btn btn-default" ID="lbAnother" runat="server" OnClick="lbAnother_Click" Text="Another Person" />
         </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
