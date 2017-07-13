<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EntityAttributeValues.ascx.cs" Inherits="RockWeb.Blocks.Utility.EntityAttributeValues" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block" Visible="false">
            <div class="panel-heading"><i class="fa fa-list-ul"></i> <asp:Literal ID="ltTitle" runat="server"></asp:Literal></div>
            <div class="panel-body">
                <asp:Panel ID="pnlView" runat="server">
                    <asp:PlaceHolder ID="phAttributes" runat="server" />

                    <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-primary" Text="Edit" OnClick="btnEdit_Click"></asp:LinkButton>
                </asp:Panel>

                <asp:Panel ID="pnlEdit" runat="server" Visible="false">
                    <asp:PlaceHolder ID="phEditAttributes" runat="server"></asp:PlaceHolder>

                    <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="btnSave_Click"></asp:LinkButton>
                    <asp:LinkButton ID="btnCancel" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="btnCancel_Click"></asp:LinkButton>
                </asp:Panel>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>