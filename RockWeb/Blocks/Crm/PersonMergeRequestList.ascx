<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonMergeRequestList.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonMergeRequestList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-files-o"></i>&nbsp;Merge Requests</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowDataBound="gList_RowDataBound" OnRowSelected="gList_RowSelected">
                        <Columns>
                            <Rock:DateTimeField DataField="ModifiedDateTime" HeaderText="Date" SortExpression="ModifiedDateTime" />
                            <Rock:RockBoundField DataField="CreatedByPersonAlias.Person" HeaderText="Requestor" SortExpression="CreatedByPersonAlias.Person.LastName, CreatedByPersonAlias.Person.NickName" />
                            <Rock:RockLiteralField ID="lMergeRecords" HeaderText="Merge Records" SortExpression="" />
                            <Rock:RockBoundField DataField="Note" HeaderText="Note" SortExpression="Note" />
                            <Rock:DeleteField OnClick="gList_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
