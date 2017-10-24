<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ZoneBlocks.ascx.cs" Inherits="RockWeb.Blocks.Administration.ZoneBlocks" %>
<script type="text/javascript">
</script>
<asp:UpdatePanel ID="upPages" runat="server">
<ContentTemplate>

    <asp:HiddenField ID="hfOption" runat="server" Value="Page" />
    <ul id="zone-block-options" class="nav nav-pills margin-b-md">
        <li id="liPage" runat="server" ><a href='#<%=divPage.ClientID%>'  data-toggle="pill">Page</a></li>
        <li id="liLayout" runat="server" ><a href='#<%=divLayout.ClientID%>' data-toggle="pill"><asp:Literal ID="lAllPagesForLayout" runat="server"></asp:Literal></a></li>
        <li id="liSite" runat="server" ><a href='#<%=divSite.ClientID%>' data-toggle="pill"><asp:Literal ID="lAllPagesOnSite" runat="server"></asp:Literal></a></li>
    </ul>

    <asp:Panel ID="pnlLists" runat="server" CssClass="tab-content">

        <div id="divPage" runat="server" class="tab-pane" >
            
            <div class="grid">
                <Rock:Grid ID="gPageBlocks" runat="server" AllowPaging="false" EmptyDataText="No Page Blocks Found" RowItemText="block" OnRowSelected="gPageBlocks_Edit">
                    <Columns>
                        <Rock:ReorderField />
                        <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                        <Rock:RockTemplateField HeaderText="Type" >
                            <ItemTemplate>
                                <%# Eval("BlockTypeName") %><br />
                                <small><%# Eval("BlockTypePath") %></small>
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                        <Rock:DeleteField OnClick="gPageBlocks_Delete" />
                    </Columns>
                </Rock:Grid>
            </div>

        </div>

        <div id="divLayout" runat="server" class="tab-pane" >
            
            <div class="grid">
                <Rock:Grid ID="gLayoutBlocks" runat="server" AllowPaging="false" EmptyDataText="No Layout Blocks Found" OnRowSelected="gLayoutBlocks_Edit">
                    <Columns>
                        <Rock:ReorderField />
                        <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                        <Rock:RockTemplateField HeaderText="Type" >
                            <ItemTemplate>
                                <%# Eval("BlockTypeName") %><br />
                                <small><%# Eval("BlockTypePath") %></small>
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                        <Rock:DeleteField OnClick="gLayoutBlocks_Delete" />
                    </Columns>
                </Rock:Grid>
            </div>

        </div>

         <div id="divSite" runat="server" class="tab-pane" >
            
            <div class="grid">
                <Rock:Grid ID="gSiteBlocks" runat="server" AllowPaging="false" EmptyDataText="No Site Blocks Found" OnRowSelected="gSiteBlocks_Edit">
                    <Columns>
                        <Rock:ReorderField />
                        <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                        <Rock:RockTemplateField HeaderText="Type" >
                            <ItemTemplate>
                                <%# Eval("BlockTypeName") %><br />
                                <small><%# Eval("BlockTypePath") %></small>
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                        <Rock:DeleteField OnClick="gSiteBlocks_Delete" />
                    </Columns>
                </Rock:Grid>
            </div>

        </div>

    </asp:Panel>

    <asp:Panel ID="pnlDetails" runat="server" Visible="false" CssClass="admin-details">

        <asp:HiddenField ID="hfBlockLocation" runat="server" />
        <asp:HiddenField ID="hfBlockId" runat="server" />

        <asp:ValidationSummary ID="vsZoneBlocks" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="ZoneBlockValidationGroup"/>
        <fieldset>
            <legend><asp:Literal ID="lAction" runat="server"></asp:Literal> Block</legend>
            <Rock:DataTextBox ID="tbBlockName" runat="server" SourceTypeName="Rock.Model.Block, Rock" PropertyName="Name" Required="true" 
                ValidationGroup="ZoneBlockValidationGroup" CssClass="input-large"/>
            <div class="row">
                <div class="col-md-6">
                    <Rock:RockDropDownList ID="ddlBlockType" runat="server" Label="Type" AutoPostBack="true" OnSelectedIndexChanged="ddlBlockType_SelectedIndexChanged" EnhanceForLongLists="true" />
                </div>
                <div class="col-md-6 padding-t-md">
                    <label>Common Block Types</label><br />
                    <asp:Repeater ID="rptCommonBlockTypes" runat="server" OnItemDataBound="rptCommonBlockTypes_ItemDataBound">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnNewBlockQuickSetting" runat="server" Text="Todo" CssClass="btn btn-default btn-xs" OnClick="btnNewBlockQuickSetting_Click" />
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </fieldset>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" onclick="btnSave_Click" ValidationGroup="ZoneBlockValidationGroup" />
            <asp:LinkButton id="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </asp:Panel>

    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" Dismissable="true" />

</ContentTemplate>
</asp:UpdatePanel>

