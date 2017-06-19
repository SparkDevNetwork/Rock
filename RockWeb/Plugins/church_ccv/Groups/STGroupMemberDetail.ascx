<%@ Control Language="C#" AutoEventWireup="true" CodeFile="STGroupMemberDetail.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Groups.STGroupMemberDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfGroupId" runat="server" />
        <asp:HiddenField ID="hfGroupMemberId" runat="server" />
        <asp:HiddenField ID="hfAdminPersonId" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"></h1>

                <div class="panel-labels">
                </div>
            </div>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" />

                <div id="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <h3>
                                <asp:Literal ID="lPersonName" runat="server" />
                                <small>
                                    <asp:Literal ID="lGroupRole" runat="server" />
                                </small>
                                <Rock:ImageEditor ID="imgPhoto" runat="server" BinaryFileTypeGuid="03BD8476-8A9F-4078-B628-5B538F967AFC" />
                            </h3>

                            <Rock:RockRadioButtonList ID="rblActivePendingStatus" runat="server" Label="Status" RepeatDirection="Horizontal" />
                            <Rock:EmailBox ID="ebEmailAddress" runat="server" Label="Email Address" RepeatDirection="Horizontal" />
                            <Rock:PhoneNumberBox ID="pnHome" runat="server" Label="Home Phone" />
                       </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:PhoneNumberBox ID="pnMobile" runat="server" Label="Mobile Phone" />
                            <asp:CheckBox ID="cbSms" runat="server" Text="Allows SMS"/>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DatePicker ID="dpAnniversaryDate" runat="server" Label="Anniversary Date" RepeatDirection="Horizontal" />

                            <Rock:RockDropDownList ID="ddlOptOutReason" runat="server" Label="Opt Out" AutoPostBack="true" OnSelectedIndexChanged="ddlOptOutReason_SelectedIndexChanged" />
                            <Rock:DatePicker ID="dpFollowUpDate" runat="server" Label="Follow Up Date" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false" />
                    </div>

                </div>

            </div>
        </div>

        <asp:Literal ID="lReceipt" runat="server" Text="" />

    </ContentTemplate>
</asp:UpdatePanel>
