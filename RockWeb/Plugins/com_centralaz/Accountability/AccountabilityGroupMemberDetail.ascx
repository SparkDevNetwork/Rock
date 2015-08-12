<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountabilityGroupMemberDetail.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Accountability.AccountabilityGroupMemberDetail" %>
<asp:UpdatePanel ID="upnlContent" runat="server">

    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <asp:HiddenField ID="hfGroupId" runat="server" />
            <asp:HiddenField ID="hfGroupMemberId" runat="server" />
            <asp:HiddenField ID="hfGroupTypeId" runat="server" />

            <div class="panel panel-block">
                <div class="panel-body">

                    <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" />

                    <div id="pnlViewDetails" runat="server">
                        <div class="row">
                            <div class="col-md-6">
                                <h1 class="title name">
                                    <asp:Literal ID="lName" runat="server" /></h1>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-2">
                                <asp:Literal ID="lImage" runat="server" />
                            </div>
                            <div class="col-md-10">
                                <Rock:RockRadioButtonList ID="rblStatus" runat="server" Label="Status" RepeatDirection="Horizontal" Enabled="false" />
                                <h5><b>Member Start Date</b></h5>
                                <asp:Literal ID="lblStartDate" runat="server" />
                            </div>

                        </div>

                        <div class="actions margin-v-md">
                            <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="lbEdit_Click" CausesValidation="false" />
                        </div>
                    </div>

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                    <Rock:NotificationBox ID="NotificationBox1" runat="server" NotificationBoxType="Danger" />
                    <div id="pnlEditDetails" runat="server">

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:PersonPicker runat="server" ID="ppGroupMemberPerson" Label="Person" Required="true" />
                                <Rock:RockDropDownList runat="server" ID="ddlGroupRole" DataTextField="Name" DataValueField="Id" Label="Role" Required="true" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockRadioButtonList ID="rblEditStatus" runat="server" Label="Status" RepeatDirection="Horizontal" />
                                <Rock:DatePicker ID="dpMemberStartDate" runat="server" Label="Member Start Date" />
                            </div>
                        </div>

                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click"></asp:LinkButton>
                            <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false"></asp:LinkButton>
                        </div>

                    </div>
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
