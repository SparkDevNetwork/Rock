<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Administration.RestActionList, RockWeb" %>

<asp:UpdatePanel ID="upnlList" runat="server">
    <ContentTemplate>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-exchange"></i> <asp:Literal ID="lControllerName" runat="server"/></h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gActions" runat="server" AllowSorting="true">
                        <Columns>
                            <asp:BoundField DataField="Method" HeaderText="Method" SortExpression="Method" />
                            <asp:BoundField DataField="Path" HeaderText="Relative Path" SortExpression="Path" />
                            <Rock:SecurityField TitleField="Method" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
