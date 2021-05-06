<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MediaElementDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.MediaElementDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading ">
                <h1 class="panel-title"><i class="fa fa-play-circle"></i>
                    <asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>

            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:HiddenField ID="hfId" runat="server" />
                <asp:HiddenField ID="hfMediaFolderId" runat="server" />
                <asp:HiddenField ID="hfDisallowManualEntry" runat="server" />
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlViewDetails" runat="server">
                    <div class="row">
                        <div class="col-sm-6 col-md-7 col-lg-8">
                            <div class="margin-b-lg">
                                <asp:Literal ID="lDescription" runat="server" />
                            </div>
                        </div>
                        <div class="col-sm-6 col-md-5 col-lg-4">
                            <asp:Literal ID="lMetricData" runat="server" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <label>Media Files</label>
                            <Rock:Grid ID="gViewMediaFiles" runat="server" EmptyDataText="No Media Files" DisplayType="Light" ShowHeader="true" >
                                <Columns>
                                    <Rock:RockBoundField DataField="Quality" HeaderText="Quality" />
                                    <Rock:RockBoundField DataField="Format" HeaderText="Format" />
                                    <Rock:RockBoundField DataField="Dimension" HeaderText="Dimension" />
                                    <Rock:RockBoundField DataField="FormattedFileSize" HeaderText="Size" />
                                    <Rock:BoolField DataField="AllowDownload" HeaderText="Allow Download" SortExpression="AllowDownload" />
                                    <Rock:RockBoundField DataField="Link" HeaderText="Link" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                        <div class="col-md-12 margin-t-lg">
                            <label>Thumbnail Files</label>
                            <Rock:Grid ID="gViewThumbnailFiles" runat="server" EmptyDataText="No Media Files" RowItemText="Thumbnail" DisplayType="Light" ShowHeader="true">
                                <Columns>
                                    <Rock:RockBoundField DataField="Dimension" HeaderText="Dimension" />
                                    <Rock:RockBoundField DataField="FormattedFileSize" HeaderText="Size" />
                                    <Rock:RockBoundField DataField="Link" HeaderText="Link" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>

                    <div class="actions margin-t-lg">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                    </div>
                </div>
                <div id="pnlEditDetails" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.MediaElement, Rock" PropertyName="Name" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.MediaElement, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                        </div>
                        <div class="col-md-12">
                            <Rock:NumberBox ID="nbDuration" CssClass="input-width-xl" runat="server" NumberType="Integer" Label="Duration (seconds)" />
                        </div>

                        <div class="col-md-12">
                            <label>Media Files</label>
                            <Rock:Grid ID="gMediaFiles" runat="server" EmptyDataText="No Media Files" RowItemText="Media" DisplayType="Light" ShowHeader="true">
                                <Columns>
                                    <Rock:RockBoundField DataField="Quality" HeaderText="Quality" />
                                    <Rock:RockBoundField DataField="Format" HeaderText="Format" />
                                    <Rock:RockBoundField DataField="Dimension" HeaderText="Dimension" />
                                    <Rock:RockBoundField DataField="FormattedFileSize" HeaderText="Size" />
                                    <Rock:BoolField DataField="AllowDownload" HeaderText="Allow Download" SortExpression="AllowDownload" />
                                    <Rock:RockBoundField DataField="Link" HeaderText="Link" />
                                    <Rock:EditField OnClick="gMediaFiles_Edit" />
                                </Columns>
                            </Rock:Grid>
                        </div>

                        <div class="col-md-12">
                            <label>Thumbnail Files</label>
                            <Rock:Grid ID="gThumbnailFiles" runat="server" EmptyDataText="No Thumbnail Files" RowItemText="Thumbnail" DisplayType="Light" ShowHeader="true">
                                <Columns>
                                    <Rock:RockBoundField DataField="Dimension" HeaderText="Dimension" />
                                    <Rock:RockBoundField DataField="FormattedFileSize" HeaderText="Size" />
                                    <Rock:RockBoundField DataField="Link" HeaderText="Link" />
                                    <Rock:EditField OnClick="gThumbnailFiles_Edit" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>

                    <div class="actions margin-t-lg">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>
        <asp:HiddenField ID="hfActiveDialog" runat="server" />
        <Rock:ModalDialog ID="mdMediaFile" runat="server" Title="File Info" OnSaveClick="mdMediaFile_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="MediaFile">
            <Content>
                <asp:HiddenField ID="hfMediaElementData" runat="server" />
                <asp:ValidationSummary ID="ValidationSummaryMediaFile" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="MediaFile" />
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RockTextBox ID="tbPublicName" runat="server" Label="Public Name" Help="Description of the media file to be used when allowing someone to select a format. (e.g. 1080p, 720p) " ValidationGroup="MediaFile" Required="true" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:UrlLinkBox ID="urlLink" runat="server" Label="Link" ValidationGroup="MediaFile" CssClass="input-width-xxl" Required="true"  />
                    </div>
                     <div class="col-md-6">
                         <Rock:RockCheckBox ID="cbAllowDownload" runat="server" Label="Allow Download" ValidationGroup="MediaFile" />
                     </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlQuality" runat="server" Label="Quality" Help="This is typically used to filter media files when several qualities exist." Required="true" ValidationGroup="MediaFile" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbFormat" runat="server" Label="Format" Help="The MIME type of the media format." ValidationGroup="MediaFile" CssClass="input-width-lg" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbWidth" runat="server" Label="Width" Help="The width in pixels of the media element." NumberType="Integer" ValidationGroup="MediaFile" CssClass="input-width-md"/>
                    </div>
                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbHeight" runat="server" Label="Height" Help="The height in pixels of the media element." NumberType="Integer" ValidationGroup="MediaFile" CssClass="input-width-md"/>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbFPS" runat="server" Label="FPS" Help="The Frames Per Second if the media file is a video." NumberType="Integer" ValidationGroup="MediaFile" CssClass="input-width-md" />
                    </div>
                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbSize" runat="server" Label="Filesize" Help="The size of the media file in bytes." NumberType="Integer" ValidationGroup="MediaFile" CssClass="input-width-lg"/>
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>
        <Rock:ModalDialog ID="mdThumbnailFile" runat="server" Title="Thumbnail File Info" OnSaveClick="mdThumbnailFile_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="ThumbnailFile">
            <Content>
                <asp:HiddenField ID="hfThumbnailFile" runat="server" />
                <asp:ValidationSummary ID="ValidationSummary2" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="ThumbnailFile" />
                <div class="row">
                    <div class="col-md-12">
                        <Rock:UrlLinkBox ID="urlThumbnailLink" runat="server" Label="Link" ValidationGroup="ThumbnailFile" CssClass="input-width-xxl" Required="true" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbThumbnailWidth" runat="server" Label="Width" Help="The width in pixels of the thumbnail element." NumberType="Integer" ValidationGroup="ThumbnailFile" CssClass="input-width-md"/>
                    </div>
                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbThumbnailHeight" runat="server" Label="Height" Help="The height in pixels of the thumbnail element." NumberType="Integer" ValidationGroup="ThumbnailFile" CssClass="input-width-md"/>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbThumbnailSize" runat="server" Label="Filesize" NumberType="Integer" Help="The size of the thumbnail file in bytes." ValidationGroup="ThumbnailFile" CssClass="input-width-lg"/>
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
