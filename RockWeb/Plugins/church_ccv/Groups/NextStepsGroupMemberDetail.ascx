<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NextStepsGroupMemberDetail.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Groups.NextStepsGroupMemberDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfGroupId" runat="server" />
        <asp:HiddenField ID="hfGroupMemberId" runat="server" />


        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"></h1>

                <div class="panel-labels">
                </div>
            </div>

            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <asp:CustomValidator ID="cvGroupMember" runat="server" Display="None" />
                <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" />

                <div id="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <h3>
                                <asp:Literal ID="lPersonName" runat="server" />
                                <small>
                                    <asp:Literal ID="lGroupRole" runat="server" /></small>
                            </h3>
                            <Rock:RockRadioButtonList ID="rblActivePendingStatus" runat="server" Label="Status" RepeatDirection="Horizontal" />
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlOptOutReason" runat="server" Label="Opt Out" AutoPostBack="true" OnSelectedIndexChanged="ddlOptOutReason_SelectedIndexChanged" />
                            <Rock:DatePicker ID="dpFollowUpDate" runat="server" Label="Follow Up Date" />
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false" />
                    </div>

                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
