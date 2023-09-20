<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckoutSuccess1.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Checkin.CheckoutSuccess1" %>
<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="maWarning" runat="server" />

        <div class="checkin-header">
            <h1>
                <asp:Literal ID="lTitle" runat="server"></asp:Literal></h1>
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
                <asp:LinkButton CssClass="btn btn-primary btn-done" ID="lbDone" runat="server" OnClick="lbDone_Click" Text="Done" />
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
