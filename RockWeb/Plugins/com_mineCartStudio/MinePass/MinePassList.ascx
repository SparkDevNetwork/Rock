<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MinePassList.ascx.cs" Inherits="RockWeb.Plugins.com_mineCartStudio.MinePass.MinePassList" %>

<asp:UpdatePanel ID="upnlContent" runat="server" >
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-users"></i> Mine Passes</h1>
            </div>
            <div class="panel-body">
                <Rock:ModalAlert id="mdGridWarning" runat="server" />
                <div class="grid grid-panel">
                    <Rock:Grid ID="gMinePassList" runat="server" AllowSorting="true" OnRowDataBound="gMinePassList_RowDataBound">
                        <Columns>
                            <Rock:RockBoundField DataField="PersonAlias.Person.FullName" HeaderText="Name" SortExpression="PersonAlias.Person.FullName" />
                            <Rock:RockBoundField DataField="MinePassStatus" HeaderText="Status" SortExpression="MinePassStatus" />
                            <Rock:RockBoundField DataField="LastUpdateDateTime" HeaderText="Last Updated" SortExpression="LastUpdateDateTime" />
                            
                            <asp:TemplateField HeaderText="" HeaderStyle-CssClass="grid-columncommand" ItemStyle-CssClass="grid-columncommand" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:HyperLink ID="hlDownload" runat="server" Text="<div class='btn btn-default btn-sm'><i class='fa fa-download'></i></div>" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        
        </asp:Panel>

        <Rock:ModalAlert ID="maMessages" runat="server" />

        <Rock:ModalDialog ID="mdPassDetail" runat="server" Title="Mine Pass Detail" OnSaveClick="mdPassDetail_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="PassDetail">
            <Content>
                <asp:ValidationSummary ID="valSummaryPass" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="PassDetail" />

                <Rock:PersonPicker ID="ppPassPerson" runat="server" Label="Individual" />

            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
