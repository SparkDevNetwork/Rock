<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TellerImport.ascx.cs" Inherits="RockWeb.Plugins.org_secc.Finance.TellerImport" %>

<script src="/SignalR/hubs"></script>
<script type="text/javascript">

    var prm = Sys.WebForms.PageRequestManager.getInstance();
    prm.add_initializeRequest(InitializeRequest);

    function InitializeRequest(sender, args) {
        var updateProgress = $get('updateProgress');
        var postBackElement = args.get_postBackElement();
        if (postBackElement.id == '<%= btnConfirm.ClientID %>') {
            updateProgress.control._associatedUpdatePanelId = 'dummyId';
            $('#<%= btnCancelConfirm.ClientID %>').hide();
        }
        else{
            updateProgress.control._associatedUpdatePanelId = null;
        }
    }

    $(function () {

        var proxy = $.connection.rockMessageHub;

        proxy.client.receiveNotification = function (name, message) {
            if (name == '<%=signalREventName %>') {
                $("div.progress").show();
                var percentage = message + '%'
                var $progress = $("div.progress-bar");
                $progress.attr("aria-valuenow", message);
                $progress.css("width", percentage);
                $progress.html(percentage);
            }
        }

        $.connection.hub.start().done(function () {
            // hub started... 
        });
    })

</script>

<asp:UpdatePanel ID="upnlContent" runat="server">

    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-download"></i>Teller Import</h1>
            </div>

            <div class="panel-body">

                <asp:Panel ID="pnlEntry" runat="server" Visible="true" >

                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                    <Rock:NotificationBox ID="nbMessage" runat="server" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:FileUploader ID="fuTellerFile" runat="server" Label="Teller File" Required="true" OnFileUploaded="fuTellerFile_FileUploaded" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlBatch" runat="server" Label="Batch" Required="false" DataValueField="Id" DataTextField="Name"
                                Help="The batch to add transactions to. Leave blank to create a new batch." />
                        </div>
                         <div class="col-md-6">
                            <Rock:DataTextBox ID="tbBatchName" runat="server" SourceTypeName="Rock.Model.FinancialBatch" PropertyName="Name" Label="Batch Name" MaxLength="50"/>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnImport" runat="server" CssClass="btn btn-primary" Text="Import" OnClick="btnImport_Click" />
                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlConfirm" runat="server" Visible="false" >

                    <h4><asp:Literal ID="lConfirm" runat="server" /></h4>
                    <br />

                    <div class="progress" style="display:none">
                        <div class="progress-bar" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="min-width: 2em;">
                            0%
                        </div>
                    </div>

                    <div class="actions">
                        <Rock:BootstrapButton ID="btnConfirm" runat="server" CssClass="btn btn-primary" Text="Confirm" DataLoadingText="Importing..." CausesValidation="true" OnClick="btnConfirm_Click" />
                        <asp:LinkButton ID="btnCancelConfirm" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="btnCancelConfirm_Click"/>
                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlResults" runat="server" Visible="false" >

                    <Rock:NotificationBox ID="nbSuccess" runat="server" NotificationBoxType="Success" Heading="Import Summary:"  />
                    <Rock:NotificationBox ID="nbErrors" runat="server" NotificationBoxType="Danger" Heading="Import Errors:"  />

                    <div class="actions">
                        <asp:LinkButton ID="btnImportAnother" runat="server" CssClass="btn btn-link" Text="Import Another" OnClick="btnImportAnother_Click" />
                    </div>

                </asp:Panel>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
