﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PackageInstall.ascx.cs" Inherits="RockWeb.Blocks.Store.PackageInstall" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-gift"></i> Package Install</h1>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlInstall" runat="server">
                    <div class="panel-headerimage">
                        <asp:Image ID="imgPackageImage" runat="server" CssClass="packagedetail-image" />
                    </div>

                    
                    
                    <div class="row">
                        <div class="col-md-4">
                            <div class="alert alert-warning">
                                <strong>Note</strong><br />
                                <p>
                                    The installation of plugins may cause Rock to restart. You may want to consider
                                    installing during off-peak times.
                                </p>
                            </div>
                        </div>
                        <div class="col-md-8">
                            
                            <p>
                                <asp:Literal ID="lInstallMessage" runat="server" />
                            </p>

                            <Rock:RockTextBox ID="txtUsername" runat="server" Label="Store Username" />
                            <Rock:RockTextBox ID="txtPassword" runat="server" TextMode="Password" Label="Store Password" />

                            <Rock:RockCheckBox ID="cbAgreeToTerms" runat="server" Label="I have read and agree to the terms of the Rock Store <small><a href='http://www.rockrms.com/Store/Terms' target='_blank'>(read terms)</a></small>" AutoPostBack="true" OnCheckedChanged="cbAgreeToTerms_CheckedChanged" />

                            <asp:Button ID="btnInstall" CssClass="btn btn-primary" OnClick="btnInstall_Click" runat="server" Text="Install" Enabled="false" />

                            <asp:Literal id="lMessages" runat="server" />
                        </div>
                    </div>
                </asp:Panel>
                
                <asp:Panel ID="pnlError" runat="server" Visible="false">
                    <div class="alert alert-warning">
                        <h4>Store Currently Not Available</h4>
                        <p>We're sorry, the Rock Store is currently not available. Check back soon!</p>
                        <small><em><asp:Literal ID="lErrorMessage" runat="server" /></em></small>
                    </div>
                </asp:Panel>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
