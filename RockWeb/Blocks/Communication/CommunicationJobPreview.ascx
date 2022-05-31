<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationJobPreview.ascx.cs" Inherits="RockWeb.Blocks.Communication.CommunicationJobPreview" %>

<style>
.card-message-preview {
    max-width: 874px;
    margin: 16px auto;
}
.mobile-preview {
    max-width: 414px;
}
</style>

<asp:UpdatePanel ID="upMain" runat="server">
    <ContentTemplate>
        <!-- Content -->
        <div class="panel panel-block panel-analytics">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-mail-bulk"></i>
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
            </div>

            <div class="panel-body">

                <div class="row row-eq-height-md">
                    <!-- Left Menu -->
                    <div class="col-md-3 filter-options">
                        <Rock:RockLiteral ID="lNavTitle" Label="System Communication" runat="server" />

                        <Rock:RockDropDownList ID="ddlMessageDate" runat="server" Label="Message Date" AutoPostBack="true"
                            Required="true" DisplayRequiredIndicator="true" Help="Date to use when previewing the message." OnSelectedIndexChanged="ddlMessageDate_SelectedIndexChanged" />

                        <Rock:PersonPicker ID="ppTargetPerson" runat="server" Label="Target Person" Help="Person used to customize the email preview." EnableSelfSelection="true" OnSelectPerson="ppTargetPerson_SelectPerson" />

                        <asp:LinkButton CssClass="btn btn-primary" ID="lbUpdate" runat="server" OnClick="lbUpdate_Click" Text="Update" Visible="true" />
                    </div>
                    <!-- End Left Menu -->

                    <!-- Mail Preview -->
                    <main class="col-md-9">
                        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />
                        <div id="messagePreview" class="card shadow card-message-preview">
                            <div class="card-header bg-white py-2 px-0">
                                <div class="row no-gutters p-2">
                                    <div class="col-xs-3 text-right">
                                        <span class="text-muted pr-md-4 text-nowrap">From</span>
                                    </div>
                                    <div class="col-xs-9">
                                        <asp:Literal ID="lFrom" runat="server" />
                                    </div>
                                </div>
                                <div class="row no-gutters p-2">
                                    <div class="col-xs-3  text-right">
                                        <span class="text-muted pr-md-4 text-nowrap">Subject</span>
                                    </div>
                                    <div class="col-xs-9 ">
                                        <asp:Literal ID="lSubject" runat="server" />
                                    </div>
                                </div>
                                <div class="row no-gutters p-2">
                                    <div class="col-xs-3 text-right">
                                        <span class="text-muted pr-md-4 text-nowrap">Date</span>
                                    </div>
                                    <div class="col-xs-9 ">
                                        <asp:Literal ID="lDate" runat="server" />
                                    </div>
                                </div>
                            </div>
                            <div class="card-header py-1 bg-white position-relative d-flex justify-content-end">
                                <div class="inset-0 d-flex align-items-center justify-content-center">
                                    <div class="btn-group btn-group-view-control" role="group" aria-label="View Mode">
                                    <button id="btnDesktop" runat="server" type="button" onclick="DesktopMode();" class="btn btn-xs btn-info">Desktop</button>
                                    <button id="btnMobile" runat="server" type="button" onclick="MobileMode();" class="btn btn-xs btn-default">Mobile</button>
                                    </div>
                                </div>
                                <div class="justify-self-end z-10">
                                    <asp:LinkButton ID="btnSendEmail" runat="server" Text="Send Test" CssClass="btn btn-primary btn-xs" CausesValidation="false" OnClick="btnSendEmail_Click" />
                                </div>
                            </div>
                            <div class="card-body p-0 styled-scroll">
                                <asp:Literal ID="lContent" runat="server"></asp:Literal>
                            </div>
                        </div>
                    </main>
                    <!-- End Mail Preview -->
                </div>
            </div>
        </div>

        <!-- Modal -->
        <Rock:ModalDialog ID="mdSendTest" runat="server" Title="Send Test Email" SaveButtonText="Send" SaveButtonCausesValidation="true" OnSaveClick="mdSendTest_SaveClick" ValidationGroup="valSendTestGroup">
            <Content>
                <asp:ValidationSummary ID="vsSendTestGroup" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation"
                    ValidationGroup="valSendTestGroup" ShowSummary="true" ShowValidationErrors="true" DisplayMode="BulletList" Enabled="true" Visible="true" />
                <div class="row">
                    <div class="col-xs-12">
                        <Rock:EmailBox ID="ebSendTest" runat="server" Label="Email" TextMode="Email" Required="true" RequiredErrorMessage="A valid email address is required."
                            Help="This will temporarily change your email address during the test, but it will be changed back after the test is complete."
                            ValidationGroup="valSendTestGroup" />
                    </div>
                </div>
                <Rock:NotificationBox ID="nbSendTest" runat="server"></Rock:NotificationBox>
            </Content>
        </Rock:ModalDialog>
        <!-- End Modal -->

        <asp:Literal ID="lavaDebug" runat="server" Visible="false"></asp:Literal>
    </ContentTemplate>
</asp:UpdatePanel>

<script type="text/javascript">
    function rescaleIframe() {
        var $emailPreviewIframe = $('.js-emailpreview-iframe');
        $emailPreviewIframe.height('auto');
        var emailPreviewIframe = $emailPreviewIframe[0];
        var newHeight = $(emailPreviewIframe.contentWindow.document).height();
        if ($(emailPreviewIframe).height() != newHeight) {
            $(emailPreviewIframe).height(newHeight);
        }

        $emailPreviewIframe.load(function () {
            var emailPreviewIframe = $emailPreviewIframe[0];
            var newHeight = $(emailPreviewIframe.contentWindow.document).height();
            if ($(emailPreviewIframe).height() != newHeight) {
                $(emailPreviewIframe).height(newHeight);
            }
        });
    }

    $(document).ready(function () {
        rescaleIframe();
        Sys.Application.add_load(function () {
            rescaleIframe();
        });
    });

    function DesktopMode() {
        $("#messagePreview").removeClass("mobile-preview");
        $(".btn-group-view-control .btn").toggleClass("btn-default btn-info");
        rescaleIframe();
    };
    function MobileMode() {
        $("#messagePreview").addClass("mobile-preview");
        $(".btn-group-view-control .btn").toggleClass("btn-default btn-info");
        rescaleIframe();
    };
</script>