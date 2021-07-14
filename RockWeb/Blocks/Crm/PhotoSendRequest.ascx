<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PhotoSendRequest.ascx.cs" Inherits="RockWeb.Blocks.Crm.PhotoSendRequest" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list"></i> Select Criteria</h1>
            </div>

            <div class="panel-body">

                <asp:Panel ID="pnlForm" runat="server" Visible="true" CssClass="js-send-request">

                    <Rock:RockCheckBoxList ID="cblRoles" runat="server" Label="Family Roles" DataValueField="Id" DataTextField="Name"></Rock:RockCheckBoxList>
                    <Rock:NumberBox ID="nbAge" runat="server" CssClass="input-width-sm" Label="Age is more than" NumberType="Integer" Text="16" />
                    <Rock:DefinedValuesPicker ID="dvpConnectionStatus" runat="server" Label="Connection Status" DataValueField="Value" DataTextField="Text"></Rock:DefinedValuesPicker>
                    <Rock:NumberBox ID="nbUpdatedLessThan" runat="server" CssClass="input-width-sm" Label="Exclude people with a photo updated in the last (years)" Text="3" ></Rock:NumberBox>

                    <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" Text="" Visible="false" />
                    <Rock:NotificationBox ID="nbTestResult" runat="server" NotificationBoxType="Success" Text="Test communication has been sent." Visible="false" />
                    <Rock:NotificationBox ID="nbConfirmMessage" runat="server" NotificationBoxType="Info" Title="Please Confirm" Visible="false" />

                    <div class="actions">
                        <asp:LinkButton ID="btnSend" runat="server" Text="Send" CssClass="btn btn-primary" OnClick="btnSend_Click" CausesValidation="true" />
                        <asp:LinkButton ID="btnSendConfirmed" runat="server" Text="Confirm Send" CssClass="btn btn-primary" OnClick="btnSendConfirmed_Click" Visible="false" CausesValidation="true" />
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-danger" OnClick="btnCancel_Click" Visible="false" CausesValidation="false" />
                        <asp:LinkButton ID="btnTest" runat="server" Text="Test" CssClass="btn btn-default" OnClick="btnTest_Click" CausesValidation="true" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
                    <Rock:NotificationBox ID="nbSuccess" runat="server" NotificationBoxType="Success" Title="Success" Text="The request has been sent." Visible="true" />
                </asp:Panel>
            </div>

        </asp:Panel>

        <script>
            function resetSendButton() {
                var $sendButton = $('.js-send-request').find("[id$=btnSend]");
                var $sendConfirmedButton = $('.js-send-request').find("[id$=btnSendConfirmed]");
                var $cancelButton = $('.js-send-request').find("[id$=btnCancel]");
                var $confirmMessage = $('.js-send-request').find("[id$=nbConfirmMessage]");

                $sendButton.show();
                $sendButton.removeClass("hidden");
                $sendConfirmedButton.hide();
                $cancelButton.hide();
                $confirmMessage.fadeOut();
            }

            Sys.Application.add_load(function () {
                $('.js-send-request input[type=checkbox]').on('click', function (e) {
                    resetSendButton();
                });

                $('.js-send-request input[type=text]').on('change', function (e) {
                    resetSendButton();
                });
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
