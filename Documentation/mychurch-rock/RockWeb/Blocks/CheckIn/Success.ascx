<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Success.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Success" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="row-fluid checkin-header">
        <div class="span12">
            <h1>You Are Checked-in</h1>
        </div>
    </div>


    <div class="row-fluid checkin-body">
        <div class="span12">
            
            <ol class="checkin-summary checkin-body-container">
                <asp:PlaceHolder ID="phResults" runat="server"></asp:PlaceHolder>
            </ol>
        </div>
    </div>


    <div class="row-fluid checkin-footer">   
        <div class="checkin-actions">
            <asp:LinkButton CssClass="btn btn-primary" ID="lbDone" runat="server" OnClick="lbDone_Click" Text="Done" />
            <asp:LinkButton CssClass="btn btn-secondary" ID="lbAnother" runat="server" OnClick="lbAnother_Click" Text="Another Person" />
         </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
