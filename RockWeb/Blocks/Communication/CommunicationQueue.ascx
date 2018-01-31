<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationQueue.ascx.cs" Inherits="RockWeb.Blocks.Communication.CommunicationQueue" %>

<asp:UpdatePanel ID="upCommunication" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comments-o"></i> Communication Queue</h1>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-sm-4">
                        <Rock:RockCheckBox ID="cbFutureComm" runat="server" Label="Show Future Communications" AutoPostBack="true" OnCheckedChanged="cbFilter_Changed" />
                    </div>
                    <div class="col-sm-4">
                        <Rock:RockCheckBox ID="cbPendingApproval" runat="server" Label="Show Communications Pending Approval" AutoPostBack="true" OnCheckedChanged="cbFilter_Changed" />
                    </div>
                    <div class="col-sm-4">
                        <Rock:RockCheckBoxList ID="cblType"  runat="server" Label="Communication Type" RepeatDirection="Horizontal" AutoPostBack="true" />
                    </div>
                </div>

                <div class="grid grid-panel">
                    <Rock:Grid ID="gCommunicationQueue" runat="server" AllowSorting="true" OnRowSelected="gCommunicationQueue_RowSelected">
                        <Columns>
                            <Rock:DateTimeField DataField="SendDateTime" SortExpression="SendDateTime" HeaderText="Send Date / Time" />
                            <Rock:RockBoundField DataField="Sender.FullName" HeaderText="Sender" SortExpression="Sender.LastName,Sender.NickName" />
                            <Rock:RockBoundField DataField="Subject" SortExpression="Subject" HeaderText="Subject" />
                            <Rock:EnumField DataField="Status" SortExpression="Status" HeaderText="Status" />
                            <Rock:RockBoundField DataField="PendingRecipients" HeaderText="Pending Recipients" />
                            <asp:TemplateField HeaderText="Type" SortExpression="CommunicationType">
                                <ItemTemplate>
                                    <span class='label label-<%# Eval("MediumLabel") %>'><%# ((Rock.Model.CommunicationType)Eval("CommunicationType")).ToString() %></span>
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
