<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MinistrySafeSettings.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.MinistrySafe.MinistrySafeSettings" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="maUpdated" runat="server" />
        <asp:Panel ID="pnlWrapper" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-shield"></i>MinistrySafe</h1>
                <div class="pull-right">
                    <asp:LinkButton ID="btnDefault" runat="server" CssClass="btn btn-default btn-xs" OnClick="btnDefault_Click">Enable As Default Background Check Provider</asp:LinkButton>
                </div>
            </div>
            <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-danger" />
            <Rock:NotificationBox ID="nbNotification" runat="server" Title="Please correct the following:" NotificationBoxType="Danger" Visible="false" />
            <div class="panel-body">
                <asp:Panel ID="pnlToken" runat="server" Visible="false">
                    <div class="row">
                        <div class="col-md-12">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:Lava ID="lavaMinistrySafeDescription" runat="server">
                                    <img src="{{ '~/Plugins/com_bemaservices/MinistrySafe/Assets/Images/MinistrySafe.png' | ResolveRockUrl }}" style="max-width: 35%;margin:0 auto 16px;display:block;">
                                    </Rock:Lava>
                                </div>
                                <div class="col-md-5 col-md-offset-1 col-sm-6">
                                    <Rock:RockTextBox ID="tbAccessToken" runat="server" Label="MinistrySafe API Token" Required="true" RequiredErrorMessage="A MinistrySafe API Token is Required" Help="The MinistrySafe Access Token is generated when a MinistrySafe Account is created on the Rock website." />
                                    <Rock:RockCheckBox ID="cbIsStaging" runat="server" Label="Is Staging Environment" Help="Are you using a staging environment?" />
                                    <div class="actions">
                                        <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary" OnClick="btnSave_Click">Save</asp:LinkButton>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlPackages" runat="server">
                    <div class="row">
                        <div class="col-md-12">
                            <div class="row">
                                <div class="col-md-6">
                                    <asp:Literal ID="lViewColumnLeft" runat="server" />
                                    <div class="actions">
                                        <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-primary" OnClick="btnEdit_Click">Edit</asp:LinkButton>
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <Rock:RockLiteral ID="lPackages" runat="server" Label="Enabled Background Check Types" />
                                    <div class="actions">
                                        <asp:LinkButton ID="btnUpdate" runat="server" CssClass="btn btn-default btn-xs" OnClick="btnUpdate_Click"><i class="fa fa-sync"></i> Update Packages</asp:LinkButton>
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <Rock:RockLiteral ID="lTags" runat="server" Label="Enabled Tags" />
                                    <div class="actions">
                                        <asp:LinkButton ID="btnUpdateTags" runat="server" CssClass="btn btn-default btn-xs" OnClick="btnUpdateTags_Click"><i class="fa fa-sync"></i> Update Tags</asp:LinkButton>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
