<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttributeCategories.ascx.cs" Inherits="RockWeb.Blocks.Administration.AttributeCategories" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" runat="server">

            <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                <Rock:LabeledDropDownList ID="ddlEntityType" runat="server" LabelText="Entity Type" />
            </Rock:GridFilter>
            <Rock:Grid ID="rGrid" runat="server" AllowSorting="true" RowItemText="setting" OnRowSelected="rGrid_Edit">
                <Columns>
                    <asp:BoundField DataField="Id" HeaderText="Id" SortExpression="Id" />
                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                    <asp:TemplateField>
                        <HeaderTemplate>Entity Type</HeaderTemplate>
                        <ItemTemplate>
                            <asp:Literal ID="lEntityType" runat="server"></asp:Literal>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <Rock:DeleteField OnClick="rGrid_Delete" />
                </Columns>
            </Rock:Grid>

        </asp:Panel>

        <Rock:ModalDialog ID="modalDetails" runat="server" Title="Category">
            <Content>
                <asp:HiddenField ID="hfIdValue" runat="server" />
                <fieldset>
                    <Rock:LabeledTextBox ID="tbName" runat="server" LabelText="Name" />
                </fieldset>
            </Content>
        </Rock:ModalDialog>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
