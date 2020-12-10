<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConsumerList.ascx.cs" Inherits="RockWeb.Blocks.Bus.ConsumerList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-bus"></i> 
                    Consumers
                </h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="QueueName" HeaderText="Queue" SortExpression="QueueName" />
                            <Rock:RockBoundField DataField="ConsumerName" HeaderText="Consumer" SortExpression="ConsumerName" />
                            <Rock:RockBoundField DataField="MessageName" HeaderText="Message" SortExpression="MessageName" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>