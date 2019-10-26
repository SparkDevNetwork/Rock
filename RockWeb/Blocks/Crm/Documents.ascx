<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Documents.ascx.cs" Inherits="RockWeb.Blocks.Crm.Documents" %>
<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlList" runat="server">
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="<%=this.icon %>"></i> <%=this.title %></h1>
                <div class="form-inline pull-right clearfix">
                    <asp:DropDownList ID="ddlDocumentType" runat="server" Label="Document Types" IncludeGlobalOption="true" AutoPostBack="true" OnSelectedIndexChanged="ddlDocumentType_SelectedIndexChanged" />
                </div>
            </div>
        
            <%-- Grid --%>
            <Rock:Grid ID="gFileList" runat="server" OnRowSelected="gFileList_RowSelected">
                <Columns>
                    <Rock:ReorderField />
                    <Rock:RockBoundField DataField="Name" HeaderText="Name"></Rock:RockBoundField>


                    <Rock:SecurityField />
                    <Rock:DeleteField OnClick="gFileListDelete_Click" />
                </Columns>
            </Rock:Grid>
        </div>
        </asp:Panel>

        <asp:Panel ID="pnlAddEdit" runat="server" Visible="false">
            <%-- Edit Controls --%>

        </asp:Panel>



    </ContentTemplate>
</asp:UpdatePanel>