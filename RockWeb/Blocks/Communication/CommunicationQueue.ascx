<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationQueue.ascx.cs" Inherits="RockWeb.Blocks.Communication.CommunicationQueue" %>
<script>
    Sys.Application.add_load(function ()
    {
        $('.grid-table span.badge').tooltip({ html: true, container: 'body', delay: { show: 500, hide: 100 } });
    });
</script>
<asp:UpdatePanel ID="upFinancial" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comments-o"></i>Communication Queue</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:RockCheckBox ID="cbFutureComm" runat="server" Label="Show Future Communications" />
                        <Rock:RockCheckBox ID="cbPendingApproval" runat="server" Label="Show Communications Pending Approval" />
                        <Rock:RockCheckBoxList ID="cblMedium"  runat="server" Label="Communication Medium" RepeatDirection="Horizontal">
                        </Rock:RockCheckBoxList>
                    </Rock:GridFilter>

                    <Rock:Grid ID="gCommunicationQueue" runat="server" AllowSorting="true" OnRowSelected="gCommunicationQueue_RowSelected">
                        <Columns>
                            <Rock:DateTimeField DataField="SendDateTime" SortExpression="SendDateTime" HeaderText="Send Date / Time" />
                            <Rock:RockBoundField DataField="Sender.FullName" HeaderText="Sender" SortExpression="Sender.LastName,Sender.NickName" />
                            <Rock:RockBoundField DataField="Subject" SortExpression="Subject" HeaderText="Subject" />
                            <Rock:EnumField DataField="Status" SortExpression="Status" HeaderText="Status" />
                            <Rock:RockBoundField DataField="PendingRecipients" HeaderText="Pending Recipients" />
                            <asp:TemplateField HeaderText="Medium" SortExpression="MediumName">
                                <ItemTemplate>
                                    <span class='label label-<%# Eval("MediumLabel") %>'><%# Eval("MediumName") %></span>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />



    </ContentTemplate>
</asp:UpdatePanel>
