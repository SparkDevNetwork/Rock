<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LogViewer.ascx.cs" Inherits="RockWeb.Blocks.Administration.LogViewer" %>
<asp:UpdatePanel runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdAlert" runat="server" />
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-stream"></i> Logs</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="rGrid" runat="server" EmptyDataText="No Logs Found" AllowCustomPaging="true" ShowPaginationText="false" >
                        <Columns>
                            <Rock:RockBoundField DataField="DateTime" HeaderText="Date" />
                            <Rock:RockBoundField DataField="Level" HeaderText="Level" />
                            <Rock:RockBoundField DataField="Domain" HeaderText="Domain" />
                            <Rock:RockBoundField DataField="Message" HeaderText="Message" />
                            <Rock:RockBoundField DataField="SerializedException" HeaderText="Exception" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
