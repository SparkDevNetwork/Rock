<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AssetStorageProviderList.ascx.cs" Inherits="RockWeb.Blocks.Core.AssetStorageProviderList" %>
<asp:UpdatePanel ID="pnlAssetStorageProviderListUpdatePanel" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-cloud-upload"></i>
                    Asset Storage List
                </h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="rGridAssetStorageProvider" runat="server" RowItemText="Asset Storage Provider" OnRowSelected="rGridAssetStorageProvider_RowSelected" TooltipField="Description" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <asp:TemplateField HeaderText="Asset Storage Type" SortExpression="EntityType.Name">
                                <ItemTemplate><%# GetComponentName( Eval( "EntityType") )%></ItemTemplate>
                            </asp:TemplateField>
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:DeleteField OnClick="rGridAssetStorageProvider_DeleteClick" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>