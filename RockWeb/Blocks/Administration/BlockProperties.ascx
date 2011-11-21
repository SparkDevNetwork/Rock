<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BlockProperties.ascx.cs" Inherits="RockWeb.Blocks.Administration.BlockProperties" %>

<asp:PlaceHolder id="phClose" runat="server"></asp:PlaceHolder>

<div class="admin-dialog">

    <fieldset id="fsProperties" runat="server">
        
        <ol id="olProperties" runat="server">
            <li>
                <asp:Label ID="lblBlockName" runat="server" AssociatedControlID="tbBlockName">Name</asp:Label>
                <asp:TextBox ID="tbBlockName" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvBlockName" runat="server" ControlToValidate="tbBlockName" 
                        CssClass="failureNotification" ErrorMessage="Name is required." ToolTip="Name is required." 
                        ValidationGroup="BlockPropertiesValidationGroup">*</asp:RequiredFieldValidator>
            </li>
            <li>
                <asp:Label ID="lblCacheDuration" runat="server" AssociatedControlID="tbCacheDuration">Cache Duration</asp:Label>
                <asp:TextBox ID="tbCacheDuration" runat="server" Text="0"></asp:TextBox>
                <asp:RangeValidator ID="rvCacheDuration" runat="server" ControlToValidate="tbCacheDuration" 
                    MinimumValue="0" MaximumValue="999999999" Type="Integer" CssClass="failureNotification" 
                    ErrorMessage="Cache Duration must be valid number of seconds" ToolTip="Seconds"
                    ValidationGroup="BlockPropertiesValidationGroup">*</asp:RangeValidator>
            </li>
        </ol>

    </fieldset>

    <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click " />

</div>

