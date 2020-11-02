﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationList.ascx.cs" Inherits="RockWeb.Blocks.Communication.CommunicationList" %>
<script>
    Sys.Application.add_load(function () {
        $('.grid-table span.badge').tooltip({ html: true, container: 'body', delay: { show: 500, hide: 100 } });
    });
</script>
<asp:UpdatePanel ID="upFinancial" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbBindError" runat="server" NotificationBoxType="Warning" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comments-o"></i> Communication List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:RockTextBox ID="tbSubject" runat="server" Label="Subject" />
                        <Rock:RockDropDownList ID="ddlType" runat="server" Label="Communication Type" />
                        <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                        <Rock:PersonPicker ID="ppSender" runat="server" Label="Created By" />
                        <Rock:DateRangePicker ID="drpCreatedDates" runat="server" Label="Created Date Range" Help="Note: Leaving dates blank will default to last 7 days." />
                        <Rock:DateRangePicker ID="drpSentDates" runat="server" Label="Sent Date Range" />
                        <Rock:RockTextBox ID="tbContent" runat="server" Label="Content" />
                    </Rock:GridFilter>

                    <Rock:Grid ID="gCommunication" runat="server" AllowSorting="true" OnRowSelected="gCommunication_RowSelected" OnRowDataBound="gCommunication_RowDataBound" PageSizes="50,500">
                        <Columns>
                             <Rock:RockTemplateField SortExpression="CommunicationType" >
                                <ItemTemplate>
                                   <div class="text-center"><i class="<%# Eval( "TypeIconCssClass" ) %>"></i></div>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="Subject" SortExpression="Subject" HeaderText="Subject" />
                            <Rock:EnumField DataField="Status" SortExpression="Status" HeaderText="Status" />
                            <Rock:RockLiteralField HeaderText="Details" ID="lDetails" HeaderStyle-CssClass="grid-columncommand" ItemStyle-CssClass="grid-columncommand" />
                            <Rock:RockBoundField  DataField="SendDateTimeFormat" HtmlEncode="false" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" SortExpression="SendDateTime" ColumnPriority="Desktop" HeaderText="Sent" />
                            <Rock:RockTemplateField HeaderText="Recipients" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" SortExpression="Recipients">
                                <ItemTemplate>
                                    <span class="badge badge-success" title="Opened" style='<%# (int)Eval("OpenedRecipients") > 0 ? "display:inline-block" : "display:none" %>'><%# Eval("OpenedRecipients") %></span>
                                    <span class="badge badge-info" title="Delivered" style='<%# (int)Eval("DeliveredRecipients") > 0 ? "display:inline-block" : "display:none" %>'><%# Eval("DeliveredRecipients") %></span>
                                    <span class="badge badge-none" title="Pending" style='<%# (int)Eval("PendingRecipients") > 0 ? "display:inline-block" : "display:none" %>'><%# Eval("PendingRecipients") %></span>
                                    <span class="badge badge-warning" title="Cancelled" style='<%# (int)Eval("CancelledRecipients") > 0 ? "display:inline-block" : "display:none" %>'><%# Eval("CancelledRecipients") %></span>
                                    <span class="badge badge-danger" title="Failed" style='<%# (int)Eval("FailedRecipients") > 0 ? "display:inline-block" : "display:none" %>'><%# Eval("FailedRecipients") %></span>
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
