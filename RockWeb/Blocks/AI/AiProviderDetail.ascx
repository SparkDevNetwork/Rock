<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AiProviderDetail.ascx.cs" Inherits="RockWeb.Blocks.AI.AiProviderDetail" %>

<asp:UpdatePanel ID="pnlEntityUpdatePanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
           
            <asp:HiddenField ID="hfEntityId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-brain"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>
                
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div id="pnlViewDetails" runat="server">
                    <p class="description">
                        <asp:Literal ID="lEntityDescription" runat="server"></asp:Literal>
                    </p>

                    <div class="row">
                        <div class="col-md-12">
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                    </div>

                </div>

                <div id="pnlEditDetails" runat="server">

                    <asp:ValidationSummary ID="valEntityDetail" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.AIProvider, Rock" PropertyName="Name" />
                        </div>

                        <div class="col-md-1">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" AutoPostBack="true" />
                        </div>

                        <div class="col-md-5">
                            <Rock:NotificationBox
                                ID="nbIsActiveWarning"
                                runat="server"
                                NotificationBoxType="Warning"
                                Heading="Important!"
                                Visible="false"></Rock:NotificationBox>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.AIProvider, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                        </div>
                        <div class="col-md-6">
                            <Rock:ComponentPicker ID="cpProviderType" runat="server" Label="Provider Type" Required="true" ContainerType="Rock.AI.Provider.AIProviderContainer" AutoPostBack="true" OnSelectedIndexChanged="cpProviderType_SelectedIndexChanged" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
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
