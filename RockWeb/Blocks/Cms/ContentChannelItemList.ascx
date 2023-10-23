<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelItemList.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentChannelItemList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:panel ID="pnlContent" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lIcon" runat="server" />
                    <asp:Literal ID="lContentChannel" runat="server" /> Items
                </h1>

                <div class="panel-labels">
                    <Rock:BootstrapButton ID="bDownloadFromLibrary" runat="server" CssClass="btn btn-xs btn-default" OnClick="bDownloadFromLibrary_Click"><i class="fa fa-download"></i> Download from Library</Rock:BootstrapButton>
                </div>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">

                    <Rock:GridFilter ID="gfFilter" runat="server">
                        <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                        <Rock:DateRangePicker ID="drpDateRange" runat="server" Label="Date Range" />
                        <Rock:RockTextBox ID="tbTitle" runat="server" Label="Title" />
                    </Rock:GridFilter>

                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                    <asp:HiddenField ID="hfItemId" runat="server" />

                    <Rock:Grid ID="gItems" runat="server" EmptyDataText="No Items Found" RowItemText="Item" OnRowSelected="gItems_Edit" OnRowDataBound="gItems_RowDataBound">
                        <Columns>
                            <Rock:ReorderField />
                            <Rock:RockBoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                            <Rock:DateTimeField DataField="StartDateTime" HeaderText="Start" SortExpression="StartDateTime" />
                            <Rock:DateField DataField="StartDateTime" HeaderText="Start" SortExpression="StartDateTime" />
                            <Rock:RockBoundField DataField="DateStatus" HeaderText="" HtmlEncode="false" />
                            <Rock:DateTimeField DataField="ExpireDateTime" HeaderText="Expire" SortExpression="ExpireDateTime" />
                            <Rock:DateField DataField="ExpireDateTime" HeaderText="Expire" SortExpression="ExpireDateTime" />
                            <Rock:RockBoundField DataField="Priority" HeaderText="Priority" SortExpression="Priority" DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:BoolField DataField="Occurrences" HeaderText="Event Occurrences" SortExpression="Occurrences" />
                            <Rock:RockTemplateField  ID="rtfContentLibrary" HeaderText="Library" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" ItemStyle-CssClass="grid-columncommand">
                                <ItemTemplate >
                                    <Rock:BootstrapButton ID="lbDownloadFromContentLibrary" runat="server" OnCommand="lbDownloadFromContentLibrary_Command" CommandArgument='<%# "{ \"Id\": " + Eval( "Id" ) + ", \"Name\": \"" + Eval( "Title" ) + "\", \"LicenseGuid\": \"" + Eval( "ContentLibraryLicenseTypeGuid" ) + "\" }" %>' ToolTip="Re-download" CssClass="btn btn-default btn-sm text-success" Visible="false"><i class="fa fa-download"></i></Rock:BootstrapButton>
                                    <Rock:BootstrapButton ID="lbUploadToContentLibrary" runat="server" OnCommand="lbUploadToContentLibrary_Command" CommandArgument='<%# "{ \"Id\": " + Eval( "Id" ) + ", \"Name\": \"" + Eval( "Title" ) + "\", \"LicenseGuid\": \"" + Eval( "ContentLibraryLicenseTypeGuid" ) + "\" }" %>' ToolTip="Upload" CssClass="btn btn-default btn-sm text-muted" Visible="false"><i class="fa fa-upload"></i></Rock:BootstrapButton>
                                    <Rock:BootstrapButton ID="lbUpdateToContentLibrary" runat="server" OnCommand="lbUpdateToContentLibrary_Command" CommandArgument='<%# "{ \"Id\": " + Eval( "Id" ) + ", \"Name\": \"" + Eval( "Title" ) + "\", \"LicenseGuid\": \"" + Eval( "ContentLibraryLicenseTypeGuid" ) + "\" }" %>' ToolTip="Update" CssClass="btn btn-default btn-sm text-info" Visible="false"><i class="fa fa-upload"></i></Rock:BootstrapButton>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

        </asp:panel>

        <Rock:ModalDialog ID="mdRedownloadContentLibrary" runat="server" Title="Re-download from Library" SaveButtonText="Re-download" CancelLinkVisible="true" OnSaveClick="mdRedownloadContentLibrary_SaveClick">
            <Content>
                <Rock:NotificationBox ID="nbRedownloadWarning" runat="server" NotificationBoxType="Warning" />
            </Content>
        </Rock:ModalDialog>
        <Rock:ModalDialog ID="mdUpdateContentLibrary" runat="server" Title="Send to Library" SaveButtonText="Update" CancelLinkVisible="true" OnSaveClick="mdUpdateContentLibrary_SaveClick">
            <Content>
                Are you sure you would like to update the item "<asp:Literal ID="lUpdateName" runat="server" />" in the library? Updating an item will temporarily remove it from the library until it can be re-approved.
            </Content>
        </Rock:ModalDialog>
        <Rock:ModalDialog ID="mdUploadContentLibrary" runat="server" Title="Send to Library" SaveButtonText="Upload" CancelLinkVisible="true" OnSaveClick="mdUploadContentLibrary_SaveClick">
            <Content>
                Are you sure you would like to upload the item "<asp:Literal ID="lUploadName" runat="server" />" to the library? This will be shared with the <a id="aLibraryLicense" runat="server" />.
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
