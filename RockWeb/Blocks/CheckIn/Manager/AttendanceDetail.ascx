<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceDetail.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Manager.AttendanceDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <!-- Attendance Details -->
        <div class="panel panel-body">
            <div class="row">
                <div class="col-sm-6">
                    <Rock:RockLiteral ID="lGroupName" runat="server" Label="Group" />
                    <Rock:RockLiteral ID="lLocationName" runat="server" Label="Location" />
                </div>
                <div class="col-sm-6">
                    <Rock:RockLiteral ID="lTag" runat="server" Label="Tag" />
                    <Rock:RockLiteral ID="lScheduleName" runat="server" Label="Schedule" />
                </div>
            </div>

            <hr />

            <asp:Panel ID="pnlCheckinTimeDetails" runat="server" CssClass="row">
                <div class="col-sm-6">
                    <Rock:RockLiteral ID="lCheckinTime" runat="server" Label="Check-in" />
                </div>
                <div class="col-sm-6">
                    <Rock:RockLiteral ID="lCheckinByPerson" runat="server" Label=" " />
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlPresentDetails" runat="server" CssClass="row">
                <div class="col-sm-6">
                    <Rock:RockLiteral ID="lPresentTime" runat="server" Label="Present" />
                </div>
                <div class="col-sm-6">
                    <Rock:RockLiteral ID="lPresentByPerson" runat="server" Label=" " />
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlCheckedOutDetails" runat="server" CssClass="row">
                <div class="col-sm-6">
                    <Rock:RockLiteral ID="lCheckedOutTime" runat="server" Label="Checked-out" />
                </div>
                <div class="col-sm-6">
                    <Rock:RockLiteral ID="lCheckedOutByPerson" runat="server" Label=" " />
                </div>
            </asp:Panel>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
