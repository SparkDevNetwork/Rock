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

            <asp:Panel ID="pnlCheckinTimeDetails" runat="server">
                <Rock:RockLiteral ID="lCheckinTime" runat="server" Label="Check-in" />
            </asp:Panel>

            <asp:Panel ID="pnlPresentDetails" runat="server">
                <Rock:RockLiteral ID="lPresentTime" runat="server" Label="Present" />
            </asp:Panel>

            <asp:Panel ID="pnlCheckedOutDetails" runat="server">
                <Rock:RockLiteral ID="lCheckedOutTime" runat="server" Label="Checked-out" />
            </asp:Panel>

            <div class="actions">
                <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" />
            </div>

        </div>

        <Rock:ModalDialog ID="mdMovePerson" runat="server" Title="Move Person" SaveButtonText="Move" OnSaveClick="mdMovePerson_SaveClick">
            <Content>
                <div class="row">
                    <div class="col-md-4">
                        <Rock:RockDropDownList ID="ddlMovePersonSchedule" runat="server" Label="Service" AutoPostBack="false" />
                    </div>
                    <div class="col-md-4">
                        <Rock:LocationItemPicker ID="lpMovePersonLocation" runat="server" Label="Location" OnSelectItem="lpMovePersonLocation_SelectItem" EnableFullWidth="true" />
                    </div>
                    <div class="col-md-4">
                        <Rock:RockDropDownList ID="ddlMovePersonGroup" runat="server" Label="Group" AutoPostBack="true" OnSelectedIndexChanged="ddlMovePersonGroup_SelectedIndexChanged" />
                    </div>
                </div>

                <Rock:NotificationBox ID="nbMovePersonLocationFull" runat="server" NotificationBoxType="Warning" />

            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
