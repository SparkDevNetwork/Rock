<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ZoneBlocks.ascx.cs" Inherits="RockWeb.Blocks.Cms.ZoneBlocks" %>

<div class='admin-dialog'>
    <Rock:Grid ID="rGrid" runat="server" Width="480px" Height="180px" Title="Zone Blocks" 
    EnableEdit="true" EnableAdd="true" EnableOrdering="true" EnablePaging="true"
    CssClass="data-grid" AsyncEditorLoading="true" style="position:relative" IdColumnName="id">
        <Rock:GridDeleteColumn DataField="id" />
        <Rock:GridColumn DataField="id" Visible="false" />
        <Rock:GridColumn DataField="Name" HeaderText="Name" CanEdit="true" Width="180" />
        <Rock:GridColumn DataField="Block.Name" HeaderText="Type" CanEdit="false" Width="180" />
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

