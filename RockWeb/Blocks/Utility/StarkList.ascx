<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StarkList.ascx.cs" Inherits="RockWeb.Blocks.Utility.StarkList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-star"></i> Blank List Block</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="FullName" HeaderText="Name" SortExpression="FullName" />
                            <Rock:RockBoundField DataField="Gender" HeaderText="Gender" SortExpression="Gender" />
                            <Rock:RockBoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
