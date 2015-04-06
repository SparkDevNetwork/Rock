<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttributeCategories.ascx.cs" Inherits="RockWeb.Blocks.Core.AttributeCategories" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" runat="server">

            <Rock:NotificationBox ID="nbOrdering" runat="server" NotificationBoxType="Info" Text="Note: Select a specific entity type filter in order to reorder categories." Dismissable="true" Visible="false" />
                    
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-folder"></i> Attribute Categories</h1>
                </div>
                <div class="panel-body">

                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                            <Rock:EntityTypePicker ID="entityTypeFilter" runat="server" Required="false" Label="Entity Type" IncludeGlobalOption="true" />
                        </Rock:GridFilter>
                        <Rock:Grid ID="rGrid" runat="server" RowItemText="Category" OnRowSelected="rGrid_Edit" TooltipField="Description">
                            <Columns>
                                <Rock:ReorderField />
                                <Rock:RockBoundField DataField="Name" HeaderText="Category" />
                                <Rock:RockTemplateField>
                                    <HeaderTemplate>Entity Type</HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:Literal ID="lEntityType" runat="server"></asp:Literal>
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:RockBoundField DataField="IconCssClass" HeaderText="Icon Class" />
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
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Category, Rock" PropertyName="Name" Required="true" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Category, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:EntityTypePicker ID="entityTypePicker" runat="server" Required="true" Label="Entity Type" IncludeGlobalOption="true" />
                    </div>
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.Category, Rock" PropertyName="IconCssClass" />
                        <Rock:DataTextBox ID="tbHighlightColor" runat="server" SourceTypeName="Rock.Model.Category, Rock" PropertyName="HighlightColor" />
                    </div>

                </div>

            </Content>
        </Rock:ModalDialog>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
