<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FormTemplateList.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.FormBuilder.FormTemplateList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdAlert" runat="server" />

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfFormTemplates" runat="server">
                        <Rock:RockDropDownList ID="ddlIsActive" runat="server" Label="Active">
                            <asp:ListItem Text="" Value="" />
                            <asp:ListItem Text="Yes" Value="Yes" />
                            <asp:ListItem Text="No" Value="No" />
                        </Rock:RockDropDownList>
                        <asp:PlaceHolder ID="phAttributeFilters" runat="server" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gFormTemplates" runat="server" AllowSorting="true" OnRowSelected="gFormTemplates_RowSelected">
                        <Columns>
                            <Rock:SelectField />
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                            <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField OnClick="gFormTemplates_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>