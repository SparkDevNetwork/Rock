<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExportToGL.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.Finance.ExportToGL" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
	<Triggers>
        <asp:PostBackTrigger ControlID="lbDownload" />
    </Triggers>	
    <ContentTemplate>
        <asp:Panel ID="pnlMain" runat="server">
            <asp:LinkButton ID="lbShowExport" runat="server" CssClass="btn btn-default hidden" CausesValidation="false" OnClick="lbShowExport_Click">
                <i class="fa fa-share-square-o"></i>
                Export Batch to GL
            </asp:LinkButton>
            <asp:LinkButton ID="lbDownload" runat="server" CssClass="hidden" CausesValidation="false" OnClick="lbDownload_Click">Download</asp:LinkButton>
        </asp:Panel>

        <asp:Panel ID="pnlExportModal" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdExport" runat="server" OnSaveClick="lbExportSave_Click" SaveButtonText="Export" Title="Export to GL">
                <Content>
                    <asp:UpdatePanel ID="upnlExport" runat="server">
                        <ContentTemplate>
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:NotificationBox ID="nbAlreadyExported" runat="server" Text="You have already exported this batch to GL. Make certain that you want to re-export it before proceeding." NotificationBoxType="Warning" Visible="false" />
                                    <Rock:NotificationBox ID="nbNotClosed" runat="server" Text="The batch you are trying to export has not been closed and could be modified after you have exported to GL. You may wish to close the batch first." NotificationBoxType="Warning" Visible="false" />
                                    <Rock:DatePicker ID="dpDate" runat="server" Label="Deposit Date" Help="The date to mark the general ledger entry as deposited." Required="true" />
                                    <Rock:RockTextBox ID="tbAccountingPeriod" runat="server" Label="Accounting Period" Help="Accounting period for this deposit." MaxLength="2" Required="true" />
                                    <Rock:RockTextBox ID="tbJournalType" runat="server" Label="Journal Entry Type" Help="The type of journal entry for this deposit." MaxLength="2" Required="true" />
                                </div>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

        <script type="text/javascript">
            (function ($) {

                if (document.getElementById('downloadGuid')){
                    var hiddenElement = document.createElement('a');
                    hiddenElement.href = '/GetFile.ashx?guid=' + document.getElementById('downloadGuid');
                    hiddenElement.target = '_blank';
                    hiddenElement.download = 'GLTRN2000.txt';
                    hiddenElement.click();
                }

                function setup() {
                    var $button = $('#<%= lbShowExport.ClientID %>');
                    var $label = $('span.label:contains("Exported")');

                    $button.insertBefore('a.btn-default:contains("Match Transactions")').after(' ').removeClass('hidden');

                    if (<%= IsExported %> == 1) {
                        if ($label.length == 0) {
                            $label = $('<span class="label label-primary">Exported</span>');
                        }

                        $label.insertAfter('span.label:contains("Batch #")').before(' ').removeClass('hidden');
                    }
                    else {
                        $label.addClass('hidden');
                    }
                }

                $(document).ready(function () {
                    setup();
                });

                var prm = Sys.WebForms.PageRequestManager.getInstance();
                prm.add_endRequest(function () {
                    setup();
                });
            })(jQuery);
        </script>
    </ContentTemplate>

</asp:UpdatePanel>