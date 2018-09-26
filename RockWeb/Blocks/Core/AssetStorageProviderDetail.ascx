<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AssetStorageProviderDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.AssetStorageProviderDetail" %>
<asp:UpdatePanel ID="pnlAssetStorageProviderUpdatePanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-cloud-upload"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>
                
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                </div>
            </div>

            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>

            <div class="panel-body">

                <asp:ValidationSummary ID="valAssetStorageProviderDetail" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div id="pnlEditDetails" runat="server">
                
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.AssetStorageProvider, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.AssetStorageProvider, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6"> 
                            <Rock:ComponentPicker ID="cpAssetStorageType" runat="server" Label="Asset Storage Type" Required="true" ContainerType="Rock.Storage.AssetStorage.AssetStorageContainer" AutoPostBack="true" OnSelectedIndexChanged="cpAssetStorageType_SelectedIndexChanged" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DynamicPlaceHolder ID="phAttributes" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

            </div>


        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>