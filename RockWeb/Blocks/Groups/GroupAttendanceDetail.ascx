<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupAttendanceDetail.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupAttendanceDetail" %>

<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbNotice" runat="server" />

        <asp:Panel id="pnlDetails" runat="server">

            <h3><asp:Literal ID="lGroupName" runat="server"></asp:Literal></h3>



            <div class="row">
                <div class="col-md-6">

                    <h4>Attendance For: <asp:Literal ID="lOccurrenceDate" runat="server" /></h4>

                    <div class="actions">
                        <asp:HyperLink ID="aPrev" runat="server" Text="Previous" CssClass="btn btn-default" />
                        <asp:HyperLink ID="aNext" runat="server" Text="Next" CssClass="btn btn-default" />
                    </div>                        

                    <div class="row">
                        <div class="col-sm-12">
                            <Rock:RockCheckBox ID="cbDidNotMeet" runat="server" Text="We Did Not Meet" />
                        </div>
                    </div>

                    <div class="row group-attendance-roster">
                        <div class="col-sm-12">
                            <asp:ListView ID="lvMembers" runat="server">
                                <ItemTemplate>
                                    <asp:HiddenField ID="hfMember" runat="server" Value='<%# Eval("Id") %>' />
                                    <Rock:RockCheckBox ID="cbMember" runat="server" Checked='<%# Eval("Attended") %>' Text='<%# Eval("FullName") %>' />
                                </ItemTemplate>
                            </asp:ListView>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" Text="Save Attendance" CssClass="btn btn-primary" OnClick="lbSave_Click" CausesValidation="false" />
                    </div>

                </div>

                <div class="col-md-6">

                    <asp:panel id="pnlPendingMembers" runat="server" visible="false">
                        <h4>Pending Members</h4>
                        <asp:ListView ID="lvPendingMembers" runat="server" OnItemCommand="lvPendingMembers_ItemCommand">
                            <ItemTemplate>
                                <div class="form-group">
                                    <asp:HiddenField ID="hfMember" runat="server" Value='<%# Eval("Id") %>' />
                                    <asp:Label ID="lName" runat="server" Text='<%# Eval("FullName") %>' />
                                    <asp:LinkButton ID="lbAdd" runat="server" ToolTip="Add Person to Group" CausesValidation="false" CommandName="Add" CommandArgument='<%# Eval("Id") %>' CssClass="js-add-member" ><i class="fa fa-plus"></i></asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:ListView>
                    </asp:panel>

                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
