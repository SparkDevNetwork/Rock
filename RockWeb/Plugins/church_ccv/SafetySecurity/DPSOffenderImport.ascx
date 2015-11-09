<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DPSOffenderImport.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.SafetySecurity.DPSOffenderImport" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-flag "></i>&nbsp;DPS Offender Import</h1>
            </div>
            <div class="panel-body">


                <Rock:FileUploader ID="fuImport" runat="server" Label="Import File" OnFileUploaded="fuImport_FileUploaded" />
                <Rock:NotificationBox ID="nbResult" runat="server" Visible="false" />

                <asp:LinkButton ID="btnMatchAddresses" runat="server" CssClass="btn btn-action" Text="Match Addresses" OnClick="btnMatchAddresses_Click" />
                <asp:LinkButton ID="btnMatchPeople" runat="server" CssClass="btn btn-action" Text="Match People" OnClick="btnMatchPeople_Click" />

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
