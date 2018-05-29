<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckrSettings.ascx.cs" Inherits="RockWeb.Blocks.Security.BackgroundCheck.CheckrSettings" %>
<style>
    .minerlist .form-control {
        width: 400px;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlWrapper" runat="server" CssClass="panel panel-block">        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-file-check"></i> Checkr Background Checks</h1>
            </div>
            <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
            <Rock:NotificationBox ID="nbNotification" runat="server" Title="Please Correct the Following" NotificationBoxType="Danger" Visible="false" />
            <div class="panel-body">
                <asp:Panel ID="pnlToken" runat="server" Visible="false">
                    <div class="row">
                        <div class="col-md-12">
                            <div class="row">
                                <div class="col-md-6">
                                    <asp:Image ID="imgCheckrImage" runat="server" CssClass="img-responsive" style=" max-width: 25%;"/>
                                    <Rock:NotificationBox ID="nbExampleInfo" runat="server" Text="To register for a Checkr account go to the Rock Website." NotificationBoxType="Info" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbAccessToken" runat="server" Label="Checkr Access Token" Required="false" RequiredErrorMessage="A Checkr Access Token is Required" Help="The Checkr Access Token is generated when a Checkr Account is created on the Rock website." />
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
                                        <asp:LinkButton ID="btnDefault" runat="server" CssClass="btn btn-primary" OnClick="btnDefault_Click">Enable As Default Background Check Provider</asp:LinkButton>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockLiteral ID="lPackages" runat="server" Label="Enabled Background Check Types" />
                                    <div class="actions">
                                        <asp:LinkButton ID="btnUpdate" runat="server" CssClass="btn btn-primary btn-xs" OnClick="btnUpdate_Click"><i class="fa fa-sync"></i> Update Packages</asp:LinkButton>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <Rock:ModalDialog ID="modalUpdated" runat="server" Title="Update Finished">
                        <Content>
                        </Content>
                    </Rock:ModalDialog>
                </asp:Panel>
            </div>        
        </asp:Panel>        
    </ContentTemplate>
</asp:UpdatePanel>