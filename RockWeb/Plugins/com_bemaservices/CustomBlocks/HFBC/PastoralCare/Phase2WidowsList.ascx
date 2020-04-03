<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Phase2WidowsList.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.PastoralCare.Phase2WidowsList" %>

<asp:UpdatePanel ID="upReport" runat="server">
    <ContentTemplate>
                
        <asp:Panel runat="server" ID="pnlInfo" Visible="false">
            <div class="panel-heading">
                <asp:Literal Text="Information" runat="server" ID="ltHeading" />
            </div>
            <div class="panel-body">
                <asp:Literal Text="" runat="server" ID="ltBody" />
            </div>
        </asp:Panel>
        <asp:Panel runat="server" ID="pnlMain" Visible="true">

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-hospital-o"></i> Phase 2 Widows List</h1>
                </div>
                
                <Rock:Grid ID="gReport" runat="server" AllowSorting="true" EmptyDataText="No Results" DataKeyNames="Id" OnRowSelected="gReport_RowSelected">
                    <Columns>
                        <Rock:PersonField DataField="Widow" HeaderText="Widow" SortExpression="Person.LastName" />
                        <Rock:PersonField DataField="Widow.PrimaryFamily.Campus" HeaderText="Campus" SortExpression="Widow.PrimaryFamily.Campus" />
                        <Rock:PersonField DataField="Widow.ConnectionStatusValue" HeaderText="Connection Status" SortExpression="Widow.ConnectionStatusValue" />
                        <Rock:RockBoundField DataField="Age" HeaderText="Age" SortExpression="Age"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="Visits" HeaderText="Visits" SortExpression="Visits"></Rock:RockBoundField>
                    </Columns>
                </Rock:Grid>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
