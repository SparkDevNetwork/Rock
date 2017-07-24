<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BulkImportTool.ascx.cs" Inherits="RockWeb.Blocks.BulkImport.BulkImportTool" %>

<script src="/SignalR/hubs"></script>
<script type="text/javascript">
    $(function ()
    {
        var proxy = $.connection.rockMessageHub;

        proxy.client.receiveNotification = function (name, message, results)
        {
            if (name == '<%=this.SignalRNotificationKey %>') {
                $('#<%=pnlProgress.ClientID%>').show();

                if (message) {
                    $('#<%=lProgressMessage.ClientID %>').html(message);
                }

                if (results) {
                    $('#<%=lProgressResults.ClientID %>').html(results);
                }
            }
        }

        proxy.client.showButtons = function (name, visible)
        {
            if (name == '<%=this.SignalRNotificationKey %>') {
                
                if (visible)
                {
                    $('#<%=pnlActions.ClientID%>').show();
                }
                else {
                    $('#<%=pnlActions.ClientID%>').hide();
                }
            }
        }

        $.connection.hub.start().done(function ()
        {
            // hub started... do stuff here if you want to let the user know something
        });
    })
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-star"></i>&nbsp;Bulk Import Tool</h1>
            </div>
            <div class="panel-body">
                <Rock:RockTextBox ID="tbForeignSystemKey" runat="server" Required="true" Label="Foreign System Key" Help="The Key used to uniquely identify the source system" />
                <asp:LinkButton ID="btnCheckForeignSystemKey" runat="server" CssClass="btn btn-xs btn-action margin-b-md" Text="Check Foreign System Key" OnClick="btnCheckForeignSystemKey_Click" />
                <Rock:NotificationBox ID="nbCheckForeignSystemKey" runat="server" CssClass="margin-b-md" NotificationBoxType="Warning" Visible = "false" Dismissable="true" />

                <Rock:FileUploader ID="fupSlingshotFile" runat="server" Label="Select Slingshot File" IsBinaryFile="false" RootFolder="~/App_Data/SlingshotFiles" DisplayMode="DropZone" OnFileUploaded="fupSlingshotFile_FileUploaded" OnFileRemoved="fupSlingshotFile_FileRemoved" />
                <asp:Literal ID="lSlingshotFileInfo" runat="server" Text="" />


                <asp:Panel ID="pnlProgress" runat="server" CssClass="js-messageContainer" Style="display:none">
                    <strong>Progress</strong><br />
                    <div class="alert alert-info"><asp:Label ID="lProgressMessage" CssClass="js-progressMessage" runat="server" /></div>

                    <strong>Details</strong><br />
                    <div class="alert alert-info">
                        <pre><asp:Label ID="lProgressResults" CssClass="js-progressResults" runat="server" /></pre>
                    </div>
                </asp:Panel>


                <asp:Panel ID="pnlActions" runat="server" CssClass="actions" Visible="false">
                    <asp:LinkButton ID="btnImport" runat="server" CssClass="btn btn-primary" Text="Import" OnClick="btnImport_Click" />
                    <asp:LinkButton ID="btnImportPhotos" runat="server" CssClass="btn btn-primary" Text="Import Photos" OnClick="btnImportPhotos_Click" />
                </asp:Panel>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
