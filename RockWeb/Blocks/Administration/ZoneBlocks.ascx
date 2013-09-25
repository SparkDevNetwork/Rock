<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ZoneBlocks.ascx.cs" Inherits="RockWeb.Blocks.Administration.ZoneBlocks" %>
<script type="text/javascript">
</script>
<asp:UpdatePanel ID="upPages" runat="server">
<ContentTemplate>

    <asp:HiddenField ID="hfOption" runat="server" Value="Page" />
    <ul id="zone-block-options" class="nav nav-pills">
        <li id="liPage" runat="server" ><a href='#<%=divPage.ClientID%>'  data-toggle="pill">Current Page</a></li>
        <li id="liLayout" runat="server" ><a href='#<%=divLayout.ClientID%>' data-toggle="pill"><asp:Literal ID="lAllPages" runat="server"></asp:Literal></a></li>
    </ul>

    <asp:Panel ID="pnlLists" runat="server" CssClass="tab-content">

        <div id="divPage" runat="server" class="tab-pane" >
            <Rock:Grid ID="gPageBlocks" runat="server" AllowPaging="false" EmptyDataText="No Page Blocks Found" RowItemText="block" OnRowSelected="gPageBlocks_Edit">
                <Columns>
                    <Rock:ReorderField />
                    <asp:BoundField DataField="Name" HeaderText="Name" />
                    <asp:TemplateField HeaderText="Type" >
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container, "DataItem.BlockType.Name") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <Rock:DeleteField OnClick="gPageBlocks_Delete" />
                </Columns>
            </Rock:Grid>
        </div>

        <div id="divLayout" runat="server" class="tab-pane" >
            <Rock:Grid ID="gLayoutBlocks" runat="server" AllowPaging="false" EmptyDataText="No Layout Blocks Found" OnRowSelected="gLayoutBlocks_Edit">
                <Columns>
                    <Rock:ReorderField />
                    <asp:BoundField DataField="Name" HeaderText="Name" />
                    <asp:TemplateField HeaderText="Type" >
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container, "DataItem.BlockType.Name") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <Rock:DeleteField OnClick="gLayoutBlocks_Delete" />
                </Columns>
            </Rock:Grid>
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlDetails" runat="server" Visible="false" CssClass="admin-details well">

        <asp:HiddenField ID="hfBlockLocation" runat="server" />
        <asp:HiddenField ID="hfBlockId" runat="server" />

        <asp:ValidationSummary ID="vsZoneBlocks" runat="server" CssClass="alert alert-error" ValidationGroup="ZoneBlockValidationGroup"/>
        <fieldset>
            <legend><asp:Literal ID="lAction" runat="server"></asp:Literal> Block</legend>
            <Rock:DataTextBox ID="tbBlockName" runat="server" SourceTypeName="Rock.Model.Block, Rock" PropertyName="Name" Required="true" ValidationGroup="ZoneBlockValidationGroup" CssClass="input-large"/>
            <Rock:DataDropDownList ID="ddlBlockType" runat="server" SourceTypeName="Rock.Model.Block, Rock" PropertyName="BlockTypeId" Label="Type" 
                AutoPostBack="true" OnSelectedIndexChanged="ddlBlockType_SelectedIndexChanged" CssClass="input-large" ValidationGroup="ZoneBlockValidationGroup" />
        </fieldset>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" onclick="btnSave_Click" ValidationGroup="ZoneBlockValidationGroup" />
            <asp:LinkButton id="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </asp:Panel>

    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>

