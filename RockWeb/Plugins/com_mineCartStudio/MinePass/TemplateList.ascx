<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TemplateList.ascx.cs" Inherits="RockWeb.Plugins.com_mineCartStudio.MinePass.TemplateList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-id-badge"></i> Template List</h1>
            </div>
            <div class="panel-body">
                <Rock:ModalAlert id="mdGridWarning" runat="server" />
                <div class="grid grid-panel">
                    <Rock:Grid ID="gTemplateList" runat="server" AllowSorting="true" OnRowSelected="gTemplateList_Edit" TooltipField="Id">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                      
                            <Rock:BoolField DataField="IsActive" HeaderText="Is Active" SortExpression="IsActive" />
                            <Rock:DeleteField OnClick="gTemplateDelete_Click"/>
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
