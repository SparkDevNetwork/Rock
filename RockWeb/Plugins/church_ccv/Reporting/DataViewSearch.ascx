<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DataViewSearch.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Reporting.DataViewSearch" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title">DataView Search</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowDataBound="gList_RowDataBound" OnRowSelected="gList_RowSelected" DataKeyNames="Id">
                        <Columns>
                            <Rock:RockLiteralField ID="lDataViewPath" HeaderText="Path" SortExpression="Category.Name" />
                            <Rock:RockBoundField DataField="Name" HeaderText="Gender" SortExpression="Name" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
