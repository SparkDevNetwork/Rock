<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationJobPreview.ascx.cs" Inherits="RockWeb.Blocks.Communication.CommunicationJobPreview" %>

<style>
.email-wrapper {
    padding-top:0px !important;
}

.email-content-desktop {
    height:670px !important;
}

.email-content-mobile {
    height:610px !important;
}
</style>

<asp:UpdatePanel ID="upMain" runat="server">
    <ContentTemplate>
        <!-- Content -->
        <div class="panel panel-block">
            <div class="panel-heading">
                <i class="fa fa-mail-bulk pr-1"></i>
                <asp:Literal ID="lTitle" runat="server" />
            </div>

            <div class="panel-body">
                <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />
                <div class="row">
                    <!-- Left Menu -->
                    <nav id="sidebarMenu" class="col-xs-2 d-md-block bg-light sidebar ">
                        <div class="position-sticky">
                            <ul class="nav flex-column">
                                <li class="nav-item pb-3">
                                    <div class="row">
                                        <div class="col-xs-12">
                                            <span class="text-semibold">System Communication</span>
                                            <br />
                                            <asp:Literal ID="lNavTitle" runat="server" />
                                        </div>
                                    </div>
                                </li>
                                <li class="nav-item" id="navMessageDate" runat="server">
                                    <div class="row">
                                        <div class="col-xs-12">
                                            <Rock:RockDropDownList ID="ddlMessageDate" ClientIDMode="Static" runat="server" Label="Message Date" Width="222px"
                                                Required="true" DisplayRequiredIndicator="true" Help="Date to use when previewing the message." OnSelectedIndexChanged="ddlMessageDate_SelectedIndexChanged" />
                                        </div>
                                    </div>
                                </li>
                                <li class="nav-item">
                                    <div class="row">
                                        <div class="col-xs-12">
                                            <Rock:PersonPicker ID="ppTargetPerson" runat="server" Label="Target Person" Help="Person used to customize the email preview." OnSelectPerson="ppTargetPerson_SelectPerson" />
                                        </div>
                                    </div>
                                </li>
                                <li class="nav-item">
                                    <div class="row">
                                        <div class="col-xs-12">
                                            <asp:LinkButton CssClass="btn btn-primary" ID="lbUpdate" runat="server" OnClick="lbUpdate_Click" Text="Update" Visible="true" />
                                        </div>
                                    </div>
                                </li>
                            </ul>
                        </div>
                    </nav>
                    <!-- End Left Menu -->

                    <!-- Mail Preview -->
                    <main class="col-xs-8">
                        <div class="card">
                            <div class="card-header bg-white">
                                <div class="row">
                                    <div class="col-xs-2 pr-0">
                                        <h6 class="card-title text-gray-600">From</h6>
                                    </div>
                                    <div class="col-xs-10 pl-0">
                                        <asp:Literal ID="lFrom" runat="server" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-xs-2 pr-0">
                                        <h6 class="card-title text-gray-600">Subject</h6>
                                    </div>
                                    <div class="col-xs-10 pl-0">
                                        <asp:Literal ID="lSubject" runat="server" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-xs-2 pr-0">
                                        <h6 class="card-title text-gray-600">Date</h6>
                                    </div>
                                    <div class="col-xs-10 pl-0">
                                        <asp:Literal ID="lDate" runat="server" />
                                    </div>
                                </div>
                            </div>
                            <div class="card-header bg-white">
                                <div class="col-xs-11 text-center">
                                    <button id="btnDesktop" runat="server" type="button" onclick="DesktopMode();" class="btn  btn-xs bg-gray-600 border-gray-600 text-white">Desktop</button>
                                    <button id="btnMobile" runat="server" type="button" onclick="MobileMode();" class="btn btn-xs bg-gray-100 border-gray-600 text-black">Mobile</button>
                                </div>
                                <div class="col-xs-1">
                                    <asp:LinkButton ID="btnSendEmail" runat="server" Text="Send Test" CssClass="btn btn-primary btn-xs" CausesValidation="false" OnClick="btnSendEmail_Click" />
                                </div>
                            </div>
                            <div class="card-body">
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

    function DesktopMode() {
        $("#divEmailPreview").removeClass("email-preview js-email-preview center-block device-mobile");
        $("#divEmailPreview").addClass("email-preview js-email-preview center-block device-browser");

        $("#ifEmailPreview").removeClass("email-content-mobile");
        $("#ifEmailPreview").addClass("email-content-desktop");
    };

    function MobileMode() {
        $("#divEmailPreview").removeClass("email-preview js-email-preview center-block device-browser");
        $("#divEmailPreview").addClass("email-preview js-email-preview center-block device-mobile");

        $("#ifEmailPreview").removeClass("email-content-desktop");
        $("#ifEmailPreview").addClass("email-content-mobile");
    };
</script>
