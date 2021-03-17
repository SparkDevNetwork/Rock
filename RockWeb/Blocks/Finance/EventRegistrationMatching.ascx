<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventRegistrationMatching.ascx.cs" Inherits="RockWeb.Blocks.Finance.EventRegistrationMatching" %>


<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <script>
            Sys.Application.add_load(function () {
                $(".transaction-image a").fluidbox();
            });
        </script>
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square-o"></i>&nbsp;Event Registration Matching</h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" Visible="false" />
                <div class="row">
                    <div class="col-md-4">
                        <Rock:RockDropDownList ID="ddlBatch" runat="server" Label="Open Batches" Required="true" AutoPostBack="true" OnSelectedIndexChanged="ddlBatch_SelectedIndexChanged" EnhanceForLongLists="true" />
                    </div>
                    <div class="col-md-4">
                        <Rock:RegistrationTemplatePicker ID="rtpRegistrationTemplate" runat="server" Required="true" Label="Registration Template" OnSelectItem="rtpRegistrationTemplate_SelectItem" />
                    </div>
                    <div class="col-md-4">
                        <Rock:RockDropDownList ID="ddlRegistrationInstance" runat="server" Label="Registration Instance" Required="true" AutoPostBack="true" OnSelectedIndexChanged="ddlRegistrationInstance_SelectedIndexChanged" EnhanceForLongLists="true" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-4">
                        <Rock:RockCheckBox ID="cbHideFullyPaidRegistrations" runat="server" Text="Hide Fully Paid Registrations" AutoPostBack="true" OnCheckedChanged="cbHideFullyPaidRegistrations_CheckedChanged" />
                    </div>
                </div>
                <div class="grid grid-panel margin-t-md">
                    <asp:Panel ID="pnlTransactions" runat="server">
                        <table class="grid-table table table-striped">
                            <thead>
                                <asp:Literal ID="lHeaderHtml" runat="server" />
                            </thead>
                            <tbody>
                                <asp:PlaceHolder ID="phTableRows" runat="server" />
                            </tbody>
                        </table>
                    </asp:Panel>
                </div>

            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
