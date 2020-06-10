<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RestControllerList.ascx.cs" Inherits="RockWeb.Blocks.Administration.RestControllerList" %>

<asp:UpdatePanel ID="upnlList" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-exchange"></i>REST Controller List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gControllers" runat="server" AllowSorting="true" OnRowSelected="gControllers_RowSelected" OnRowDataBound="gControllers_RowDataBound">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Controller Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="ClassName" HeaderText="Controller Type" SortExpression="ClassName" />
                            <Rock:RockBoundField DataField="Actions" HeaderText="Actions" SortExpression="Actions" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:RockLiteralField ID="litCacheHeader" />
                            <Rock:SecurityField TitleField="Name" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

        <div class="actions">
            <asp:LinkButton ID="btnRefreshAll" runat="server" Text="Requery REST Data" CssClass="btn btn-link" CausesValidation="false" OnClick="btnRefreshAll_Click" />
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
