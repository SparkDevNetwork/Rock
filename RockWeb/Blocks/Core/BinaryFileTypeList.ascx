<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BinaryFileTypeList.ascx.cs" Inherits="RockWeb.Blocks.Administration.BinaryFileTypeList" %>

<asp:UpdatePanel ID="upBinaryFileType" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-file-o"></i> File Types List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gBinaryFileType" runat="server" AllowSorting="true" OnRowSelected="gBinaryFileType_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                            <Rock:RockBoundField DataField="StorageEntityType" HeaderText="Storage Type" SortExpression="StorageEntityType" />
                            <Rock:RockBoundField DataField="BinaryFileCount" HeaderText="File Count" SortExpression="BinaryFileCount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                            <Rock:BoolField DataField="AllowCaching" HeaderText="Allows Caching" SortExpression="AllowCaching" />
                            <Rock:BoolField DataField="RequiresViewSecurity" HeaderText="Requires View Security" SortExpression="RequiresViewSecurity" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField OnClick="gBinaryFileType_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>
        
    </ContentTemplate>
</asp:UpdatePanel>
