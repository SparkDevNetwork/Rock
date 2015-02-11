<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupAttendanceDetail.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupAttendanceDetail" %>

<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbNotice" runat="server" />

        <asp:Panel id="pnlDetails" runat="server">

            <h4>Enter Attendance For: <asp:Literal ID="lOccurrenceDate" runat="server" /></h4>

            <div class="actions">
                <asp:HyperLink ID="aPrev" runat="server" Text="Previous Week" CssClass="btn btn-default" />
                <asp:HyperLink ID="aNext" runat="server" Text="Previous Week" CssClass="btn btn-default" />
            </div>

            <Rock:RockCheckBox ID="cbDidNotMeet" runat="server" Text="We Did Not Meet" />

            <div class="group-attendance-roster">
                <asp:ListView ID="lvMembers" runat="server" OnItemCommand="lvMembers_ItemCommand">
                    <LayoutTemplate>
                        <ul class="roster-list">
                            <li id="itemPlaceHolder" runat="server"></li>
                        </ul>
                    </LayoutTemplate>
                    <ItemTemplate>
                        <li id="li1" runat="server">
                            <asp:HiddenField ID="hfMember" runat="server" Value='<%# Eval("Id") %>' />
                            <Rock:RockCheckBox ID="cbMember" runat="server" Checked='<%# Eval("Attended") %>' Text='<%# Eval("FullName") %>' />
                            <asp:LinkButton ID="lbDelete" runat="server" CausesValidation="false" CommandArgument='<%# Eval("Id") %>' CssClass="js-remove-member" ><i class="fa fa-delete"></i></asp:LinkButton>
                        </li>
                    </ItemTemplate>
                </asp:ListView>
            </div>

            <div class="actions">
                <asp:LinkButton ID="lbSave" runat="server" Text="Save Attendance" CssClass="btn btn-Primary" OnClick="lbSave_Click" CausesValidation="false" />
                <asp:LinkButton ID="lbAdd" runat="server" Text="Add New Person to the Group" CssClass="btn btn-Link" OnClick="lbAdd_Click" CausesValidation="false" />
            </div>

            <div class="group-attendance-pending">
                <h4>Pending Members</h4>
                <asp:ListView ID="lvPending" runat="server" OnItemCommand="lvPending_ItemCommand">
                    <LayoutTemplate>
                        <ul class="pending-list">
                            <li id="itemPlaceHolder" runat="server"></li>
                        </ul>
                    </LayoutTemplate>
                    <ItemTemplate>
                        <li id="Li2" runat="server">
                            <asp:HiddenField ID="hfMember" runat="server" Value='<%# Eval("Id") %>' />
                            <asp:Label ID="lblName" runat="server" Text='<%# Eval("FullName") %>' />
                            <asp:LinkButton ID="lbAdd" runat="server" CausesValidation="false" CommandArgument='<%# Eval("Id") %>' CssClass="js-add-member" ><i class="fa fa-plus"></i></asp:LinkButton>
                        </li>
                    </ItemTemplate>
                </asp:ListView>
            
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
