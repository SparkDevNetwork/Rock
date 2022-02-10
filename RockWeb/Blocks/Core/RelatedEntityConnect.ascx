<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RelatedEntityConnect.ascx.cs" Inherits="RockWeb.Blocks.Core.RelatedEntityConnect" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><asp:Literal ID="lIcon" runat="server" /> <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            
            <div class="panel-body">

                <Rock:NotificationBox id="nbMessages" runat="server" NotificationBoxType="Warning" />

                <asp:Panel ID="pnlEdit" runat="server">
                    <asp:Literal ID="lHeaderContent" runat="server" />


                    <Rock:NumberUpDown ID="nudQuantity" runat="server" Label="Quantity" />

                    <Rock:RockTextBox ID="txtNotes" runat="server" Label="Notes" TextMode="MultiLine" Rows="3" />

                    <Rock:AttributeValuesContainer ID="avcAttributes" runat="server" NumberOfColumns="2" />

                    <div class="actions">
                        <asp:LinkButton id="btnSave" runat="server" Text="Save" CssClass="btn btn-primary   " OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" />
                    </div>

                </asp:Panel>
                
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>


