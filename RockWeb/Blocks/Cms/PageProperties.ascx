<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageProperties.ascx.cs" Inherits="RockWeb.Blocks.Cms.PageProperties" %>

<asp:PlaceHolder id="phClose" runat="server"></asp:PlaceHolder>

<div class="admin-dialog">

    <fieldset id="fsProperties" runat="server">
        
        <ol id="olProperties" runat="server">
            <li>
                <asp:Label ID="lblPageName" runat="server" AssociatedControlID="tbPageName">Name</asp:Label>
                <asp:TextBox ID="tbPageName" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvPageName" runat="server" ControlToValidate="tbPageName" 
                        CssClass="failureNotification" ErrorMessage="Name is required." ToolTip="Name is required." 
                        ValidationGroup="PagesValidationGroup">*</asp:RequiredFieldValidator>
            </li>
            <li>
                <asp:Label ID="lblPageTitle" runat="server" AssociatedControlID="tbPageTitle">Title</asp:Label>
                <asp:TextBox ID="tbPageTitle" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvPageTitle" runat="server" ControlToValidate="tbPageTitle" 
                        CssClass="failureNotification" ErrorMessage="Title is required." ToolTip="Title is required." 
                        ValidationGroup="PagesValidationGroup">*</asp:RequiredFieldValidator>
            </li>
            <li>
                <asp:Label ID="lblLayout" runat="server" AssociatedControlID="ddlLayout">Layout</asp:Label>
                <asp:DropDownList ID="ddlLayout" runat="server"></asp:DropDownList>
            </li>
            <li>
                <asp:Label ID="lblMenuWhen" runat="server" AssociatedControlID="ddlMenuWhen">Display in Menu When</asp:Label>
                <asp:DropDownList ID="ddlMenuWhen" runat="server"></asp:DropDownList>
            </li>
            <li>
                <asp:Label ID="lblMenuDescription" runat="server" AssociatedControlID="cbMenuDescription">Display Description in Menu</asp:Label>
                <asp:CheckBox ID="cbMenuDescription" runat="server" />
            </li>
            <li>
                <asp:Label ID="lblMenuIcon" runat="server" AssociatedControlID="cbMenuIcon">Display Icon in Menu</asp:Label>
                <asp:CheckBox ID="cbMenuIcon" runat="server" />
            </li>
            <li>
                <asp:Label ID="lblMenuChildPages" runat="server" AssociatedControlID="cbMenuChildPages">Display Child Pages in Menu</asp:Label>
                <asp:CheckBox ID="cbMenuChildPages" runat="server" />
            </li>
            <li>
                <asp:Label ID="lblRequiresEncryption" runat="server" AssociatedControlID="cbRequiresEncryption">Requires Encryption</asp:Label>
                <asp:CheckBox ID="cbRequiresEncryption" runat="server" />
            </li>
            <li>
                <asp:Label ID="lblEnableViewState" runat="server" AssociatedControlID="cbEnableViewState">Enable ViewState</asp:Label>
                <asp:CheckBox ID="cbEnableViewState" runat="server" />
            </li>
            <li>
                <asp:Label ID="lblIncludeAdminFooter" runat="server" AssociatedControlID="cbIncludeAdminFooter">Include Admin Footer</asp:Label>
                <asp:CheckBox ID="cbIncludeAdminFooter" runat="server" />
            </li>
            <li>
                <asp:Label ID="lblCacheDuration" runat="server" AssociatedControlID="tbCacheDuration">Cache Duration</asp:Label>
                <asp:TextBox ID="tbCacheDuration" runat="server" Text="0"></asp:TextBox>
                <asp:RangeValidator ID="rvCacheDuration" runat="server" ControlToValidate="tbCacheDuration" 
                    MinimumValue="0" MaximumValue="999999999" Type="Integer" CssClass="failureNotification" 
                    ErrorMessage="Cache Duration must be valid number of seconds" ToolTip="Seconds"
                    ValidationGroup="PagesValidationGroup">*</asp:RangeValidator>
            </li>
            <li>
                <asp:Label ID="lblDescription" runat="server" AssociatedControlID="tbDescription">Description</asp:Label>
                <asp:TextBox ID="tbDescription" runat="server"></asp:TextBox>
            </li>
        </ol>

    </fieldset>

    <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click " />

</div>

