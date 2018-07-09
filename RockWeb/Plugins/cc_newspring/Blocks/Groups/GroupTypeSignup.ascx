<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupTypeSignup.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.Groups.GroupTypeSignup" %>

<style>
    .schedule-item {
        float: left;
        margin: 0 24px 12px 0;
        width: 140px;
    }

    .schedule-item.header {
        font-weight: 700;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar-check-o"></i> <asp:Literal ID="lBlockTitle" runat="server" /></h1>

                <div class="panel-labels">
                    
                </div>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbMessages" runat="server" />

                <asp:Repeater ID="rptScheduleDates" runat="server">
                    
                    <ItemTemplate>
                        <div class="row margin-b-md">
                            <div class="col-md-2">
                                <strong>
                                    <asp:Literal ID="lScheduleDate" runat="server" Text='<%# ((DateTime)Eval("Date")).ToShortDateString() %>' />
                                </strong>
                            </div>
                            <div class="col-md-10">
                                <asp:Placeholder ID="phGroups" runat="server" />
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <asp:Button ID="btnSave" runat="server" CssClass="btn btn-primary" OnClick="btnSave_Click" Text="Save" />
            </div>
        </asp:Panel>

        

    </ContentTemplate>
</asp:UpdatePanel>

