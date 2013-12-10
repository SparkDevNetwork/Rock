<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RestControllerDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.RestControllerDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <div class="banner">
                <h1><asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
            </div>

            <fieldset>

                <p class="description">
                    <asp:Literal ID="lDescription" runat="server" /></p>

                <div class="row">
                    <asp:Literal ID="lblMainDetails" runat="server" />
                </div>

                <h4>API Actions</h4>

                <Rock:Grid ID="gApiActions" runat="server" AllowSorting="true" DataKeyNames="ID" DisplayType="Light">
                    <Columns>
                        <asp:BoundField DataField="HttpMethod.Method" HeaderText="Method" SortExpression="HttpMethod.Method" />
                        <asp:BoundField DataField="RelativePath" HeaderText="Relative Path" SortExpression="RelativePath" />
                    </Columns>
                </Rock:Grid>
            </fieldset>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
