<%@ Control Language="C#" AutoEventWireup="true" CodeFile="UserList.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.MinistrySafe.UserList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-file-text-o"></i> Users</h1>
                </div>
                <div class="panel-body">

                    <Rock:NotificationBox ID="nbNotice" runat="server" Visible="false" />

                    <div class="grid grid-panel">

                        <Rock:GridFilter ID="fUser" runat="server">
                            <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                            <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                            <Rock:DateRangePicker ID="drpRequestDates" runat="server" Label="Requested Date Range" />
                            <Rock:DateRangePicker ID="drpResponseDates" runat="server" Label="Completed Date Range" />
                        </Rock:GridFilter>
        
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                
                        <Rock:Grid ID="gUser" runat="server" AllowSorting="true" PersonIdField="PersonId">
                            <Columns>

                                <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" HtmlEncode="false" />
                                <Rock:DateField DataField="RequestDate" HeaderText="Requested" SortExpression="RequestDate" />
                                <Rock:DateField DataField="CompletedDateTime" HeaderText="Completed" SortExpression="ResponseDate" />
                                <Rock:DateField DataField="Score" HeaderText="Score" SortExpression="Score" />
                                <Rock:EditField OnClick="gUser_ViewWorkflow" IconCssClass="fa fa-cogs" ToolTip="View Workflow" />

                            </Columns>
                        </Rock:Grid>
                    </div>

                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
