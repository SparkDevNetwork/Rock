<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupInfoRequest.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Groups.GroupInfoRequest" %>
<style type="text/css">
    .padding-noLeft {
        padding-left: 0;
    }

</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbNotice" runat="server" Visible="false" NotificationBoxType="Danger" />
        <Rock:NotificationBox ID="nbWarning" runat="server" Visible="false" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlView" runat="server">
            <asp:Literal ID="lLavaOverview" runat="server" />
            <asp:Literal ID="lLavaOutputDebug" runat="server" Visible="false" />

            <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

            <div class="row">
                <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" Required="true" />
                <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" Required="true" />
            </div>
            <asp:Panel ID="pnlHomePhone" runat="server" CssClass="row">
                <div class="col-sm-7 padding-noLeft">
                    <Rock:PhoneNumberBox ID="pnHome" runat="server" Label="Home Phone" />
                </div>
                <div class="col-sm-5">
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlCellPhone" runat="server" CssClass="row">
                <div class="col-sm-7 padding-noLeft">
                    <Rock:PhoneNumberBox ID="pnCell" runat="server" Label="Cell Phone" />
                </div>
                <div class="col-sm-5 ">
                    <Rock:RockCheckBox ID="cbSMS" runat="server" Label="&nbsp;" Text="Enable SMS" />
                </div>
            </asp:Panel>
            <div class="row">
                <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" Required="true" />
                <Rock:AddressControl ID="acAddress" runat="server" Label="Home Address" />
                <Rock:RockTextBox ID="tbMessage" runat="server" TextMode="MultiLine" Rows="4" Label="Message" Required="false" />
            </div>

            <div class="row actions">
                <asp:LinkButton ID="btnSubmit" runat="server" CssClass="btn btn-primary" OnClick="btnSubmit_Click" />
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlResult" runat="server" Visible="false">
            <asp:Literal ID="lResult" runat="server" />
            <asp:Literal ID="lResultDebug" runat="server" />
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
