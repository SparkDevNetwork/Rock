<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckrSettings.ascx.cs" Inherits="RockWeb.Blocks.Security.BackgroundCheck.CheckrSettings" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="maUpdated" runat="server" />
        <asp:Panel ID="pnlWrapper" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-shield"></i> Checkr Background Checks</h1>
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
                                    <Rock:Lava ID="lavaCheckrDescription" runat="server">
                                    <img src="{{ '~/Assets/Images/Checkr.svg' | ResolveRockUrl }}" style="max-width: 35%;margin:0 auto 16px;display:block;">
                                    <p><a href="https://checkr.com" target="_blank" rel="noopener noreferrer">Checkr</a> provides modern and compliant background checks for use with Rock. With your <a href="https://rockrms.com/checkrauth?organization={{ 'Global' | Attribute:'OrganizationName' | EscapeDataString }}" target="_blank">Checkr token from RockRMS.com</a> you’ll be able to initiate background checks using Checkr’s scalable, cost-effective, and rapid screening solutions.</p>
                                    <a href="https://rockrms.com/checkrauth?organization={{ 'Global' | Attribute:'OrganizationName' | EscapeDataString }}" class="btn btn-primary btn-block" target="_blank">Connect to Checkr</a>
                                    </Rock:Lava>
                                </div>
                                <div class="col-md-5 col-md-offset-1 col-sm-6">
                                    <Rock:RockTextBox ID="tbAccessToken" runat="server" Label="Checkr Access Token" Required="true" RequiredErrorMessage="A Checkr Access Token is Required" Help="The Checkr Access Token is generated when a Checkr Account is created on the Rock website." />
                                        <div class="panel-actions">
                                            <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary" OnClick="btnSave_Click">Save</asp:LinkButton>
                                        </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlPackages" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lViewColumnLeft" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockLiteral ID="lPackages" runat="server" Label="Enabled Background Check Types" />
                            <div class="panel-actions">
                                <asp:LinkButton ID="btnUpdate" runat="server" CssClass="btn btn-default btn-xs" OnClick="btnUpdate_Click"><i class="fa fa-sync"></i> Update Packages</asp:LinkButton>
                            </div>
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-primary" OnClick="btnEdit_Click">Edit</asp:LinkButton>
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>