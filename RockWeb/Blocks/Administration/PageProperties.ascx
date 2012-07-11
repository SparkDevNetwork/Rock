<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageProperties.ascx.cs" Inherits="RockWeb.Blocks.Administration.PageProperties" %>

<asp:UpdatePanel id="upPanel" runat="server">
<ContentTemplate>
 
    <asp:PlaceHolder ID="phContent" runat="server">

        <ul class="nav nav-pills" >
            <asp:Repeater ID="rptProperties" runat="server" >
                <ItemTemplate >
                    <li class='<%# GetTabClass(Container.DataItem) %>'>
                        <asp:LinkButton ID="lbProperty" runat="server" Text='<%# Container.DataItem %>' OnClick="lbProperty_Click">
                        </asp:LinkButton> 
                    </li>
                </ItemTemplate>
            </asp:Repeater>
        </ul>

        <div class="tabContent" >

        <p><asp:Literal ID="lPropertyNote" runat="server"></asp:Literal></p>
            
        <asp:Panel ID="pnlBasicProperty" runat="server" Visible="true" >
            <fieldset>
                <Rock:DataDropDownList ID="ddlParentPage" runat="server" LabelText="Parent Page" SourceTypeName="Rock.CMS.Page, Rock" PropertyName="ParentPageId"/>
                <Rock:DataTextBox ID="tbPageName" runat="server" SourceTypeName="Rock.CMS.Page, Rock" PropertyName="Name"/>
                <Rock:DataTextBox ID="tbPageTitle" runat="server" SourceTypeName="Rock.CMS.Page, Rock" PropertyName="Title"/>
                <Rock:DataDropDownList ID="ddlLayout" runat="server" SourceTypeName="Rock.CMS.Page, Rock" PropertyName="Layout"/>
                <Rock:DataTextBox ID="tbDescription" runat="server" TextMode="MultiLine" Rows="3" SourceTypeName="Rock.CMS.Page, Rock" PropertyName="Description" />
            </fieldset>
        </asp:Panel>

         <asp:Panel ID="pnlMenuDisplay" runat="server" Visible="false" >
            <fieldset>
                <Rock:DataDropDownList ID="ddlMenuWhen" runat="server" LabelText="Display When" SourceTypeName="Rock.CMS.Page, Rock" PropertyName="DisplayInNavWhen"/>
                <Rock:LabeledCheckBox ID="cbMenuDescription" runat="server" LabelText="Show Description"/>
                <Rock:LabeledCheckBox ID="cbMenuIcon" runat="server" LabelText="Show Icon"/>
                <Rock:LabeledCheckBox ID="cbMenuChildPages" runat="server" LabelText="Show Child Pages"/>
            </fieldset>
        </asp:Panel>

        <asp:Panel ID="pnlAdvancedSettings" runat="server" Visible="false" >
            <fieldset>
                <Rock:LabeledCheckBox ID="cbRequiresEncryption" runat="server" LabelText="Force SSL"/>
                <Rock:LabeledCheckBox ID="cbEnableViewState" runat="server" LabelText="Enable ViewState"/>
                <Rock:LabeledCheckBox ID="cbIncludeAdminFooter" runat="server" LabelText="Allow Configuration"/>
                <Rock:DataTextBox ID="tbCacheDuration" runat="server" LabelText="Cache Duration" SourceTypeName="Rock.CMS.Page, Rock" PropertyName="OutputCacheDuration"/>
            </fieldset>
        </asp:Panel>

        <asp:Panel ID="pnlRoutes" runat="server" Visible="false" >
            <fieldset>                
                <Rock:DataTextBox ID="tbPageRoute" runat="server" LabelText="Page Routes" SourceTypeName="Rock.CMS.Page, Rock" PropertyName="PageRoutes"  />
            </fieldset>
        </asp:Panel>

        <placeholder id="phAttributes" runat="server"></placeholder>

    </asp:PlaceHolder>

    <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="alert-message block-massage error"/>
    
</ContentTemplate>
</asp:UpdatePanel>



