<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NoteTypes.ascx.cs" Inherits="RockWeb.Blocks.Core.NoteTypes" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" runat="server">

            <Rock:NotificationBox ID="nbOrdering" runat="server" NotificationBoxType="Info" Text="Note: Select a specific entity type filter in order to reorder note types." Dismissable="true" Visible="false" />
                    
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-edit"></i> Note Types</h1>
                </div>
                <div class="panel-body">

                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                            <Rock:EntityTypePicker ID="entityTypeFilter" runat="server" Required="false" Label="Entity Type" IncludeGlobalOption="false" />
                        </Rock:GridFilter>
                        <Rock:Grid ID="rGrid" runat="server" RowItemText="Note Type" OnRowSelected="rGrid_Edit" >
                            <Columns>
                                <Rock:ReorderField />
                                <asp:BoundField DataField="EntityType.Name" HeaderText="Entity Type" />
                                <Rock:RockBoundField DataField="Name" HeaderText="Note Type" />
                                <Rock:RockBoundField DataField="CssClass" HeaderText="CSS Class" />
                                <Rock:RockBoundField DataField="IconCssClass" HeaderText="Icon CSS Class" />
                                <Rock:BoolField DataField="UserSelectable" HeaderText="User Selectable" />
                                <Rock:BoolField DataField="IsSystem" HeaderText="System" />
                                <Rock:SecurityField />
                                <Rock:DeleteField OnClick="rGrid_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>

                </div>
            </div>

            

        </asp:Panel>

        <Rock:ModalDialog ID="modalDetails" runat="server" Title="Category" ValidationGroup="EntityTypeName">
            <Content>

                <asp:HiddenField ID="hfIdValue" runat="server" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbName" runat="server" Label="Name" Required="true" />
                        <Rock:EntityTypePicker ID="entityTypePicker" runat="server" Required="true" Label="Entity Type" IncludeGlobalOption="true" />
                        <Rock:RockCheckBox ID="cbUserSelectable" runat="server" Label="User Selectable" Text="Yes" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbCssClass" runat="server" Label="CSS Class" />
                        <Rock:RockTextBox ID="tbIconCssClass" runat="server" Label="Icon CSS Class" />
                    </div>
                </div>

            </Content>
        </Rock:ModalDialog>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
