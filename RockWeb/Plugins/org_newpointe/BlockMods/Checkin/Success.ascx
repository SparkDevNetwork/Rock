<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Success.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.BlockMods.CheckIn.Success" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="checkin-header">
        <h1>Checked-in</h1>
    </div>

    <div class="checkin-body">
        
        <div class="checkin-scroll-panel">
            <div class="scroller">           
                <ol class="checkin-summary checkin-body-container">
                    <asp:PlaceHolder ID="phResults" runat="server"></asp:PlaceHolder>
                </ol>

            </div>
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
