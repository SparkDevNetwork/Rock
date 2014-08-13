<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonDuplicateList.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDuplicateList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-star"></i> Person Duplicate List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" PersonIdField="PersonId">
                        <Columns>
                            <asp:BoundField DataField="FullName" HeaderText="Name" SortExpression="FullName" />
                            <asp:BoundField DataField="MatchCount" HeaderText="Match Count" SortExpression="MatchCount" />
                            <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
