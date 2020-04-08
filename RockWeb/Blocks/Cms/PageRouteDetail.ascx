<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageRouteDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.PageRouteDetail" %>

<asp:UpdatePanel ID="upPageRoutes" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">

            <asp:HiddenField ID="hfPageRouteId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-exchange"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" Visible="false" />

                <fieldset>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:PagePicker ID="ppPage" runat="server" Label="Page" Required="true" PromptForPageRoute="false" OnSelectItem="ppPage_SelectItem" />
                            <Rock:RockLiteral ID="lSite" runat="server" Label="Site" />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbRoute" runat="server" SourceTypeName="Rock.Model.PageRoute, Rock" PropertyName="Route" />
                            <Rock:RockCheckBox ID="cbIsGlobal" runat="server" Label="Is Global" Help="Check this if the page should be used by every site even if 'Enable Exclusive Routes' is turned on." />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6">
                            <div class="attributes">
                                <Rock:DynamicPlaceholder ID="phAttributes" runat="server" ></Rock:DynamicPlaceholder>
                            </div>
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </fieldset>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>


