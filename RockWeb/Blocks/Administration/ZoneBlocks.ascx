<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ZoneBlocks.ascx.cs" Inherits="RockWeb.Blocks.Administration.ZoneBlocks" %>

<asp:UpdatePanel ID="upPages" runat="server">
<ContentTemplate>

    <h3>Layout Blocks</h3>
    <Rock:Grid ID="gLayoutBlocks" runat="server">
        <Columns>
            <Rock:ReorderField />
            <asp:BoundField DataField="Name" HeaderText="Name" />
            <asp:TemplateField HeaderText="Type" >
                <ItemTemplate>
                    <%# DataBinder.Eval(Container, "DataItem.Block.Name") %>
                </ItemTemplate>
            </asp:TemplateField>
            <Rock:EditField OnClick="gLayoutBlocks_Edit" />
            <Rock:DeleteField OnClick="gLayoutBlocks_Delete" />
        </Columns>
    </Rock:Grid>

    <h3>Page Blocks</h3>
    <Rock:Grid ID="gPageBlocks" runat="server">
        <Columns>
            <Rock:ReorderField />
            <asp:BoundField DataField="Name" HeaderText="Name" />
            <asp:TemplateField HeaderText="Type" >
                <ItemTemplate>
                    <%# DataBinder.Eval(Container, "DataItem.Block.Name") %>
                </ItemTemplate>
            </asp:TemplateField>
            <Rock:EditField OnClick="gPageBlocks_Edit" />
            <Rock:DeleteField OnClick="gPageBlocks_Delete" />
        </Columns>
    </Rock:Grid>

    <asp:Panel ID="pnlDetails" runat="server" Visible="false" CssClass="admin-details">

        <asp:HiddenField ID="hfBlockLocation" runat="server" />
        <asp:HiddenField ID="hfBlockInstanceId" runat="server" />
        <asp:ValidationSummary ID="vsZoneBlocks" runat="server" CssClass="failureNotification" ValidationGroup="ZoneBlockValidationGroup"/>
        <fieldset>
            <legend>Zone Blocks</legend>
            <ol>
                <li>
                    <asp:Label ID="lblBlockName" runat="server" AssociatedControlID="tbBlockName">Name</asp:Label>
                    <asp:TextBox ID="tbBlockName" runat="server"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvBlockName" runat="server" ControlToValidate="tbBlockName" 
                            CssClass="failureNotification" ErrorMessage="Block Name is required." ToolTip="Block Name is required." 
                            ValidationGroup="ZoneBlockValidationGroup">*</asp:RequiredFieldValidator>
                </li>
                <li>
                    <asp:Label ID="lblLayout" runat="server" AssociatedControlID="ddlBlockType">Type</asp:Label>
                    <asp:DropDownList ID="ddlBlockType" runat="server"></asp:DropDownList>
                </li>
            </ol>
        </fieldset>
        <br />
        <asp:Button id="btnCancel" runat="server" Text="Cancel" CausesValidation="false" OnClick="btnCancel_Click" />
        <asp:Button ID="btnSave" runat="server" Text="Save" ValidationGroup="ZoneBlockValidationGroup" CssClass="button" onclick="btnSave_Click" />

    </asp:Panel>

    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>

