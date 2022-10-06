<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReminderList.ascx.cs" Inherits="RockWeb.Blocks.Reminders.ReminderList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlNotAuthenticated" runat="server" Visible="false">
            Please log in to use Reminders.
        </asp:Panel>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block styled-scroll panel-groupscheduler">
            <%-- Panel Header --%>
            <div class="panel-heading panel-follow">
                <h1 class="panel-title">
                    <i class="fa fa-bell"></i>
                    Reminders
                </h1>
            </div>

            <%-- Filter Options (Header) --%>
            <div class="panel-collapsable p-0">
                <div class="row row-eq-height no-gutters">
                    <div class="col-lg-3 col-md-4">
                        <%-- Entity Type - Filter Options (Header) --%>
                        <div class="panel-toolbar styled-scroll-white h-100 pr-1 resource-filter-options align-items-center">
                            <asp:Panel ID="pnlEntityType" runat="server">
                                <div class="btn-group">
                                    <div class="dropdown-toggle btn btn-xs btn-tool" data-toggle="dropdown">
                                        <asp:Literal ID="lSelectedEntityType" runat="server" Text="Entity Type" />
                                    </div>

                                    <ul class="dropdown-menu" role="menu">
                                        <asp:Repeater ID="rptEntityTypeList" runat="server" OnItemDataBound="rptEntityTypeList_ItemDataBound">
                                            <ItemTemplate>
                                                <li>
                                                    <asp:LinkButton ID="btnEntityType" runat="server" Text="-" CommandArgument="-" OnClick="btnEntityType_Click" />
                                                </li>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </ul>
                                </div>
                            </asp:Panel>
                        </div>
                    </div>

                    <div class="col-lg-9 col-md-8">
                        <%-- Group - Filter Options (Header) --%>
                        <asp:HiddenField ID="hfSelectedGroupId" runat="server" />
                        <div class="panel-toolbar">
                            <!-- Filter for Groups/ChildGroups -->
                            <asp:Panel ID="pnlGroupPicker" runat="server" Visible="false" CssClass="d-flex">
                                <Rock:GroupPicker ID="gpSelectedGroup" runat="server" Label="" CssClass="occurrences-groups-picker" OnValueChanged="gpSelectedGroup_ValueChanged" />
                            </asp:Panel>

                            <!-- Filter for Groups/ChildGroups -->
                            <asp:Panel ID="pnlPersonPicker" runat="server" Visible="false" CssClass="d-flex">
                                <Rock:PersonPicker ID="ppSelectedPerson" runat="server" Label="" CssClass="occurrences-groups-picker" OnValueChanged="ppSelectedPerson_ValueChanged" />
                            </asp:Panel>
                        </div>
                    </div>
                </div>
            </div>

            <%-- Panel Body --%>
            <div class="panel-body-parent">

                <asp:Repeater ID="rptReminders" runat="server">
                    <ItemTemplate>
                        <div class="row margin-b-sm">
                            <asp:HiddenField ID="hfReminderId" runat="server" Value='<%# Eval("Id") %>' />
                            <div class="col-md-2">
                                <asp:LinkButton ID="lbComplete" runat="server" CommandArgument='<%# Eval("Id") %>' OnClick="lbComplete_Click">
                                    <i class="fa fa-check-circle"></i>
                                </asp:LinkButton>
                                <asp:Literal ID="lReminderDate" runat="server" Text='<%# Eval("ReminderDate", "{0: M/d/yyyy}") %>' />
                            </div>
                            <div class="col-md-8">
                                <asp:Literal ID="lEntity" runat="server" Text='<%# Eval("EntityDescription") %>' />
                                <asp:Literal ID="lClock" runat="server" Visible='<%# Eval("IsRenewing") %>'><i class="fa fa-clock"></i></asp:Literal>
                                <asp:Literal ID="lIcon" runat="server" Text='<%# "<i class=\"fa fa-circle\" style=\"color: " + Eval("HighlightColor") + "\"></i>" %>' />
                                
                                <asp:Literal ID="lReminderType" runat="server"  Text='<%# Eval("ReminderType") %>' />
                                <asp:Literal ID="lNote" runat="server"  Text='<%# Eval("Note") %>' />
                            </div>
                            <div class="col-md-2">
                                <asp:LinkButton ID="lbEdit" runat="server" CommandArgument='<%# Eval("Id") %>' OnClick="lbEdit_Click">
                                    <i class="fa fa-pencil"></i>
                                </asp:LinkButton>
                                <asp:LinkButton ID="lbDelete" runat="server" CommandArgument='<%# Eval("Id") %>' OnClick="lbDelete_Click">
                                    <i class="fa fa-close"></i>
                                </asp:LinkButton>
                            </div>

                        </div>
                        <hr />
                    </ItemTemplate>
                </asp:Repeater>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
