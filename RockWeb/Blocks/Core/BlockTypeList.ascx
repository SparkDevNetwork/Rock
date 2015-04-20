<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BlockTypeList.ascx.cs" Inherits="RockWeb.Blocks.Core.BlockTypeList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-th-large"></i> Block Type List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfSettings" runat="server">
                        <Rock:RockTextBox ID="tbNameFilter" runat="server" Label="Name" />
                        <Rock:RockTextBox ID="tbPathFilter" runat="server" Label="Path" />
                        <Rock:RockDropDownList ID="ddlCategoryFilter" runat="server" Label="Category" />
                        <Rock:RockCheckBox ID="cbExcludeSystem" runat="server" Label="Exclude 'System' types?" Text="Yes" />
                    </Rock:GridFilter>
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gBlockTypes" runat="server" AllowSorting="true" OnRowDataBound="gBlockTypes_RowDataBound" OnRowSelected="gBlockTypes_Edit" TooltipField="Description">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField HeaderText="Category" DataField="Category" SortExpression="Category" />
                            <Rock:RockBoundField HeaderText="Path" DataField="Path" SortExpression="Path" />
                            <Rock:BadgeField HeaderText="Usage" DataField="BlocksCount" SortExpression="BlocksCount"
                                ImportantMin="0" ImportantMax="0" InfoMin="1" InfoMax="1" SuccessMin="2" />
                            <Rock:RockBoundField HeaderText="Status" SortExpression="Status" />
                            <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                            <Rock:DeleteField OnClick="gBlockTypes_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

        <div class="actions">
            <asp:LinkButton id="btnRefreshAll" runat="server" Text="Reload All Block Type Attributes" CssClass="btn btn-link" CausesValidation="false" OnClick="btnRefreshAll_Click" />
        </div>
    </ContentTemplate>
</asp:UpdatePanel>

