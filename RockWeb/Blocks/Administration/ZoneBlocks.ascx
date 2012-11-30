<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ZoneBlocks.ascx.cs" Inherits="RockWeb.Blocks.Administration.ZoneBlocks" %>
<script type="text/javascript">
</script>
<asp:UpdatePanel ID="upPages" runat="server">
<ContentTemplate>

    <asp:HiddenField ID="hfOption" runat="server" Value="Page" />
    <ul id="zone-block-options" class="nav nav-pills" data-pills="pills">
        <li id="liPage" runat="server" ><a href='#<%=divPage.ClientID%>' >Current Page</a></li>
        <li id="liLayout" runat="server" ><a href='#<%=divLayout.ClientID%>'><asp:Literal ID="lAllPages" runat="server"></asp:Literal></a></li>
    </ul>

    <asp:Panel ID="pnlLists" runat="server" CssClass="pill-content">

        <div id="divPage" runat="server" class="pill-pane" >
            <Rock:Grid ID="gPageBlocks" runat="server" AllowPaging="false" EmptyDataText="No Page Blocks Found" RowItemText="block">
                <Columns>
                    <Rock:ReorderField />
                    <asp:BoundField DataField="Name" HeaderText="Name" />
                    <asp:TemplateField HeaderText="Type" >
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container, "DataItem.BlockType.Name") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <Rock:EditField OnClick="gPageBlocks_Edit" />
                    <Rock:DeleteField OnClick="gPageBlocks_Delete" />
                </Columns>
            </Rock:Grid>
        </div>

        <div id="divLayout" runat="server" class="pill-pane" >
            <Rock:Grid ID="gLayoutBlocks" runat="server" AllowPaging="false" EmptyDataText="No Layout Blocks Found">
                <Columns>
                    <Rock:ReorderField />
                    <asp:BoundField DataField="Name" HeaderText="Name" />
                    <asp:TemplateField HeaderText="Type" >
                        <ItemTemplate>
                            <%# DataBinder.Eval(Container, "DataItem.BlockType.Name") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <Rock:EditField OnClick="gLayoutBlocks_Edit" />
                    <Rock:DeleteField OnClick="gLayoutBlocks_Delete" />
                </Columns>
            </Rock:Grid>
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlDetails" runat="server" Visible="false" CssClass="admin-details">

        <asp:HiddenField ID="hfBlockLocation" runat="server" />
        <asp:HiddenField ID="hfBlockId" runat="server" />

        <asp:ValidationSummary ID="vsZoneBlocks" runat="server" CssClass="failureNotification" ValidationGroup="ZoneBlockValidationGroup"/>
        <fieldset>
            <legend><asp:Literal ID="lAction" runat="server"></asp:Literal> Block</legend>
            <Rock:DataTextBox ID="tbBlockName" runat="server" SourceTypeName="Rock.Cms.Block, Rock" PropertyName="Name" />
            <Rock:DataDropDownList ID="ddlBlockType" runat="server" SourceTypeName="Rock.Cms.Block, Rock" PropertyName="BlockTypeId" LabelText="Type" />
        </fieldset>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" onclick="btnSave_Click" />
            <asp:LinkButton id="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </asp:Panel>

    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>

