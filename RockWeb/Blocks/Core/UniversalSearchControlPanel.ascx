<%@ Control Language="C#" AutoEventWireup="true" CodeFile="UniversalSearchControlPanel.ascx.cs" Inherits="RockWeb.Blocks.Core.UniversalSearchControlPanel" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-search-plus"></i> Universal Search</h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblEnabled" runat="server" LabelType="Danger" Text="Disabled" />
                </div>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbMessages" runat="server" NotificationBoxType="Warning" />
                
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockLiteral ID="lIndexLocation" runat="server" Label="Index Location" />
                    </div>
                    <div class="col-md-6">
                        
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="lbClearIndex" runat="server" CssClass="btn btn-default"><i class="fa fa-refresh"></i> Clear Index</asp:LinkButton>
                    <asp:LinkButton ID="lbBulkLoad" runat="server" CssClass="btn btn-default"><i class="fa fa-download"></i> Bulk Load Entities</asp:LinkButton>
                </div>
            </div>
        
        </div>

        <div class="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-square"></i> Indexable Entities</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gEntityList" runat="server" AllowSorting="true" OnRowSelected="gEntityList_RowSelected">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Entity Name" SortExpression="Name" />
                            <Rock:BoolField DataField="IsIndexingEnabled" HeaderText="Indexing Enabled" SortExpression="IsIndexingEnabled" />
                            <Rock:LinkButtonField CssClass="btn btn-default" HeaderText="Delete All Documents" HeaderStyle-HorizontalAlign="Center" Text="<i class='fa fa-eraser'></i>" OnClick="gClearIndex_Click" />    
                            <Rock:LinkButtonField CssClass="btn btn-default" HeaderText="Bulk Load Documents" HeaderStyle-HorizontalAlign="Center" Text="<i class='fa fa-download'></i>" OnClick="gBulkLoad_Click" />                  
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>

        <Rock:ModalAlert ID="maMessages" runat="server" />

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="mdEditEntityType" runat="server" Title="Entity Type Details" OnCancelScript="clearActiveDialog();">
            <Content>
                <asp:HiddenField ID="hfIdValue" runat="server" />
                <Rock:RockCheckBox ID="cbEnabledIndexing" runat="server" Label="Enable Indexing" />

            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
