<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageConfigurationReport.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Cms.PageConfigurationReport" %>
    <style>
        #diagramContainer {
            padding: 20px;
            width:80%; height: 200px;
            border: 1px solid gray;
        }

        .diagram-block {
            min-height:20px; width: 70px;
            border: 1px solid #555;
            margin: auto;
            margin-bottom: 2px;
            font-size: x-small;
            padding: 2px;
            background-color:#eee;
        }
    </style>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Danger"></Rock:NotificationBox>
        <asp:Literal ID="lPages" runat="server" ViewStateMode="Disabled" ></asp:Literal>
    </ContentTemplate>


</asp:UpdatePanel>

<table class="table table-condensed">
    <tr><th>Key</th></tr>
    <tr class="danger"><td>Indicates unused by other pages in this report.</td></tr>
</table>