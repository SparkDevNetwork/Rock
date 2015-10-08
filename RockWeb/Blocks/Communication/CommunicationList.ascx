<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationList.ascx.cs" Inherits="RockWeb.Blocks.Communication.CommunicationList" %>
<script>
    Sys.Application.add_load(function () {
        $('.grid-table span.badge').tooltip({ html: true, container: 'body', delay: { show: 500, hide: 100 } });
    });
</script>
<asp:UpdatePanel ID="upFinancial" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comments-o"></i> Communication List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:RockTextBox ID="tbSubject" runat="server" Label="Subject" />
                        <Rock:ComponentPicker ID="cpMedium" runat="server" ContainerType="Rock.Communication.MediumContainer, Rock" Label="Medium" />
                        <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                        <Rock:PersonPicker ID="ppSender" runat="server" Label="Created By" />
                        <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" Help="Note: Leaving dates blank will default to last 7 days." />
                        <Rock:RockTextBox ID="tbContent" runat="server" Label="Content" />
                    </Rock:GridFilter>

                    <Rock:Grid ID="gCommunication" runat="server" AllowSorting="true" OnRowSelected="gCommunication_RowSelected" OnRowDataBound="gCommunication_RowDataBound">
                        <Columns>
                            <Rock:RockBoundField DataField="Subject" SortExpression="Subject" HeaderText="Subject" />
                            <Rock:RockBoundField DataField="MediumName" SortExpression="MediumName" HeaderText="Medium" />
                            <Rock:DateTimeField DataField="CreatedDateTime" SortExpression="CreatedDateTime" ColumnPriority="DesktopLarge" HeaderText="Created" />
                            <Rock:RockBoundField DataField="Sender.FullName" HeaderText="Created By" SortExpression="Sender.LastName,Sender.NickName" />
                            <Rock:EnumField DataField="Status" SortExpression="Status" HeaderText="Status" />
                            <Rock:DateTimeField DataField="ReviewedDateTime" SortExpression="ReviewedDateTime" ColumnPriority="DesktopLarge" HeaderText="Reviewed" />
                            <Rock:RockBoundField DataField="Reviewer.FullName" HeaderText="Reviewed By" ColumnPriority="DesktopLarge" SortExpression="Reviewer.LastName,Reviewer.NickName"  />
                            <Rock:RockTemplateField HeaderText="Recipients" ItemStyle-HorizontalAlign="Center" SortExpression="Recipients">
                                <ItemTemplate>
                                    <span class="badge badge-success" title="Opened" data-toggle="tooltip" style='<%# (int)Eval("OpenedRecipients") > 0 ? "display:inline-block" : "display:none" %>'><%# Eval("OpenedRecipients") %></span>
                                    <span class="badge badge-info" title="Delivered" data-toggle="tooltip" style='<%# (int)Eval("DeliveredRecipients") > 0 ? "display:inline-block" : "display:none" %>'><%# Eval("DeliveredRecipients") %></span>
                                    <span class="badge badge-none" title="Pending" data-toggle="tooltip" style='<%# (int)Eval("PendingRecipients") > 0 ? "display:inline-block" : "display:none" %>'><%# Eval("PendingRecipients") %></span>
                                    <span class="badge badge-warning" title="Cancelled" data-toggle="tooltip" style='<%# (int)Eval("CancelledRecipients") > 0 ? "display:inline-block" : "display:none" %>'><%# Eval("CancelledRecipients") %></span>
                                    <span class="badge badge-danger" title="Failed" data-toggle="tooltip" style='<%# (int)Eval("FailedRecipients") > 0 ? "display:inline-block" : "display:none" %>'><%# Eval("FailedRecipients") %></span>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:DeleteField OnClick="gCommunication_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        

    </ContentTemplate>
</asp:UpdatePanel>
