﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FontAwesomeSettings.ascx.cs" Inherits="RockWeb.Blocks.Cms.FontAwesomeSettings" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-font-awesome"></i>
                    Font Awesome Settings
                </h1>

            </div>
            <div class="panel-body">
                <div class="alert alert-warning">
                    <strong>Please Note:</strong> Rock does not support Font Awesome 6, please use Font Awesome 5 ZIP files to prevent errors.
                </div>
                <asp:Panel ID="pnlFontAwesomeFree" runat="server">
                    <Rock:NotificationBox ID="nbFontAwesomeFree" runat="server" NotificationBoxType="Info">
                        You are currently using the free version of Font Awesome. Consider upgrading to the Pro version to unlock hundreds of new icons. <a target="_blank" href="https://fontawesome.com/" rel="noopener noreferrer">Learn More</a>
                    </Rock:NotificationBox>
                </asp:Panel>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbFontAwesomeProKey" runat="server" Label="Font Awesome Pro Key" />
                        <Rock:FileUploader ID="fupFontAwesomeProPackage" runat="server" Label="Font Awesome 5 Pro Package Upload" Help="Font Awesome 6 is not supported." />
                    </div>
                </div>
                <Rock:NotificationBox ID="nbInstallSuccess" runat="server" NotificationBoxType="Success" Text="" />

                <div class="actions">
                    <asp:LinkButton ID="btnInstallUpdate" runat="server" CssClass="btn btn-primary" Text="Update" OnClick="btnInstallUpdate_Click" />
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
