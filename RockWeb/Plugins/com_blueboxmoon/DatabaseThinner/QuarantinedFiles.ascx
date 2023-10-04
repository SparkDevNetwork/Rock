<%@ Control Language="C#" AutoEventWireup="true" CodeFile="QuarantinedFiles.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.DatabaseThinner.QuarantinedFiles" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfDialogMessage" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h3 class="panel-title">Quarantined Files</h3>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfFiles" runat="server" OnApplyFilterClick="gfFiles_ApplyFilterClick" OnDisplayFilterValue="gfFiles_DisplayFilterValue" OnClearFilterClick="gfFiles_ClearFilterClick">
                        <Rock:RockTextBox ID="tbFileNameFilter" runat="server" Label="File Name" />
                    </Rock:GridFilter>

                    <Rock:Grid ID="gFiles" runat="server" AllowSorting="true" OnGridRebind="gFiles_GridRebind">
                        <Columns>
                            <asp:BoundField DataField="FileName" SortExpression="FileName" HeaderText="FileName" />
                            <asp:BoundField DataField="Id" SortExpression="Id" HeaderText="Id" />
                            <asp:BoundField DataField="Guid" SortExpression="Guid" HeaderText="Guid" />
                            <Rock:DateTimeField DataField="QuarantinedDateTime" SortExpression="QuarantinedDatetime" HeaderText="Quarantined Date Time" />
                            <asp:BoundField DataField="FileSize" SortExpression="FileSize" HeaderText="File Size" DataFormatString="{0:N0}" />
                            <Rock:EditField IconCssClass="fa fa-recycle" ToolTip="Restore" OnClick="gFilesRestore_Click" />
                            <asp:TemplateField ItemStyle-CssClass="grid-columncommand" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <a href="?ViewFileId=<%# Eval( "Id" ) %>" title="View" class="btn btn-default btn-sm" target="_blank">
                                        <i class="fa fa-eye"></i>
                                    </a>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <Rock:DeleteField OnClick="gFilesDelete_Click" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>

<script>
Sys.Application.add_load(function () {
    var $dialogMessage = $('#<%= hfDialogMessage.ClientID %>');

    if ($dialogMessage.val() !== '') {
        Rock.dialogs.alert($dialogMessage.val());
        $dialogMessage.val('');
    }
});
</script>
