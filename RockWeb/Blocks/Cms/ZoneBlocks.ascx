<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ZoneBlocks.ascx.cs" Inherits="RockWeb.Blocks.Cms.ZoneBlocks" %>

<div class='admin-dialog'>
    <Rock:Grid ID="rGrid" runat="server" EnableOrdering="true" DataKeyNames="id">
        <Columns>
            <asp:BoundField DataField="Name" HeaderText="Name" ItemStyle-Width="180px" />
            <asp:TemplateField HeaderText="Type" ItemStyle-Width="180px">
                <ItemTemplate>
                <%# DataBinder.Eval(Container, "DataItem.Block.Name") %>
                </ItemTemplate>
            </asp:TemplateField>
            <Rock:DeleteField />
        </Columns>
    </Rock:Grid>
    <div class="admin-details" style="display:none">
        <span class="failureNotification">
            <asp:Literal ID="FailureText" runat="server"></asp:Literal>
        </span>
        <asp:ValidationSummary ID="ZoneBlockValidationSummary" runat="server" CssClass="failureNotification" 
                ValidationGroup="ZoneBlockValidationGroup"/>
        <fieldset>
            <legend>Add Block</legend>
            <ol>
                <li>
                    <asp:Label ID="BlockNameLabel" runat="server" AssociatedControlID="BlockName">Name</asp:Label>
                    <asp:TextBox ID="BlockName" runat="server"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="BlockNameRequired" runat="server" ControlToValidate="BlockName" 
                            CssClass="failureNotification" ErrorMessage="Block Name is required." ToolTip="Block Name is required." 
                            ValidationGroup="ZoneBlockValidationGroup">*</asp:RequiredFieldValidator>
                </li>
                <li>
                    <asp:Label ID="BlockTypeLabel" runat="server" AssociatedControlID="BlockType">Type</asp:Label>
                    <asp:DropDownList ID="BlockType" runat="server"></asp:DropDownList>
                </li>
            </ol>
        </fieldset>
        <br />
        <asp:Button ID="AddButton" runat="server" CommandName="Save" Text="Add" 
            ValidationGroup="ZoneBlockValidationGroup" CssClass="button" 
            onclick="AddButton_Click" />
    </div>
</div>

