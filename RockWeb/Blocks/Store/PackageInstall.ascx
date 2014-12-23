<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PackageInstall.ascx.cs" Inherits="RockWeb.Blocks.Store.PackageInstall" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-gift"></i> Package Install</h1>
            </div>
            <div class="panel-body">

                <div class="row">
                    <div class="col-md-4">
                        <asp:Literal ID="lPackageImage" runat="server" />

                        <h1><asp:Literal id="lPackageName" runat="server" /></h1>

                        <h4>Package Description</h4>
                        <p class="margin-b-lg">
                            <asp:Literal ID="lPackageDescription" runat="server" />
                        </p>
                    </div>
                    <div class="col-md-8">

                        <asp:Literal ID="lInstallMessage" runat="server" />

                        <Rock:RockTextBox ID="txtUsername" runat="server" Label="Store Username" />
                        <Rock:RockTextBox ID="txtPassword" runat="server" Label="Store Password" />

                        <Rock:RockCheckBox ID="cbAgreeToTerms" runat="server" Label="I agree to the terms of the Rock Store" AutoPostBack="true" OnCheckedChanged="cbAgreeToTerms_CheckedChanged" />

                        <asp:Button ID="btnInstall" CssClass="btn btn-primary" runat="server" Text="Install" Enabled="false" />
                    </div>
                </div>

                
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
