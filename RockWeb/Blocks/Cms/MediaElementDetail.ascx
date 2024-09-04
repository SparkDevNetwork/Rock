<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MediaElementDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.MediaElementDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">

            <div class="panel-heading">
                <div class="pull-left">
                    <h1 class="panel-title"><i class="fa fa-play-circle"></i>
                        <asp:Literal ID="lActionTitle" runat="server" /></h1>
                </div>

                <div class="panel-labels">
                    <asp:LinkButton ID="lbMediaFiles" runat="server" CssClass="btn btn-default btn-xs" OnClick="lbMediaFiles_Click" Visible="false">
                        <i class="fas fa-file-text"></i> View Media Assets
                    </asp:LinkButton>

                    <asp:LinkButton ID="lbMediaAnalytics" runat="server" CssClass="btn btn-default btn-xs" OnClick="lbMediaAnalytics_Click" Visible="false">
                        <i class="fa fa-line-chart"></i> View Media Analytics
                    </asp:LinkButton>
                    <Rock:HighlightLabel ID="hlDuration" runat="server" LabelType="Default" ToolTip="Media duration" />
                </div>
            </div>

            <asp:Panel ID="pnlViewAnalytics" runat="server" CssClass="panel-body">
                <div class="row d-flex flex-wrap">
                    <div class="col-xs-12 col-md-6 align-self-center">
                        <asp:Panel ID="pnlChart" runat="server">
                            <div class="video-container">
                                <div class="chart-container">
                                    <canvas id="cChart" runat="server" class="chart-canvas"></canvas>
                                </div>

                                <asp:Panel ID="pnlMediaPlayer" runat="server" />
                                <Rock:MediaPlayer Visible="false" ID="mpMedia" runat="server" ClickToPlay="false" PlayerControls="" AutoResumeInDays="0" CombinePlayStatisticsInDays="0" TrackSession="false" />
                            </div>
                        </asp:Panel>
                    </div>

                    <div class="col-xs-12 col-md-6">
                        <asp:Panel ID="pnlAllTimeDetails" runat="server">
                            <hr class="d-block d-md-none mb-3" />
                            <strong>Rock Media Analytics</strong>
                            <asp:Literal ID="lAllTimeContent" runat="server" />
                        </asp:Panel>
                    </div>
                </div>

                <Rock:NotificationBox ID="nbNoData" runat="server" NotificationBoxType="Info" Visible="false" Text="No statistical data is available yet." CssClass="margin-t-md" />

                <asp:Panel ID="pnlAnalytics" runat="server">
                    <hr class="my-3" />
                    <asp:HiddenField ID="hfAllTimeVideoData" runat="server" Value="" />
                    <asp:HiddenField ID="hfLast12MonthsVideoData" runat="server" Value="" />
                    <asp:HiddenField ID="hfLast90DaysVideoData" runat="server" Value="" />

                    <div class="d-flex flex-wrap align-items-center justify-content-between my-3">
                        <span class="mb-2 mb-sm-0">
                            <strong>
                                <asp:Label ID="lblChartTitle" runat="server">Plays Per Day</asp:Label>
                            </strong>
                        </span>
                        <Rock:RockDropDownList ID="rddlTileFrame" CssClass="input-width-lg" runat="server" AutoPostBack="true" OnSelectedIndexChanged="rddlTileFrame_SelectedIndexChanged">
                            <asp:ListItem Text="Last 90 Days" Value="90Days" Selected="True" />
                            <asp:ListItem Text="Last 12 Months" Value="12Months" />
                        </Rock:RockDropDownList>
                    </div>

                    <asp:Panel ID="pnlLast90DaysDetails" runat="server" Visible="true">
                        <asp:Literal ID="lLast90DaysContent" runat="server" />
                    </asp:Panel>

                    <asp:Panel ID="pnlLast12MonthsDetails" runat="server" Visible="false">
                        <asp:Literal ID="lLast12MonthsContent" runat="server" />
                    </asp:Panel>

                    <asp:Panel ID="pnlIndividualPlays" runat="server">
                        <hr class="my-3" />
                        <p class="my-3 py-1">
                            <strong>Individual Plays</strong>
                        </p>

                        <button class="js-load-more btn btn-primary mt-2">Load More</button>
                    </asp:Panel>
                </asp:Panel>

            </asp:Panel>

            <asp:Panel ID="pnlViewFile" runat="server" CssClass="panel-body" Visible="false">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:HiddenField ID="hfId" runat="server" />
                <asp:HiddenField ID="hfMediaFolderId" runat="server" />
                <asp:HiddenField ID="hfDisallowManualEntry" runat="server" />
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlViewFileDetails" runat="server">
                    <div>
                        <asp:Literal ID="lDescription" runat="server" />
                    </div>

                    <div class="form-group">
                        <label class="control-label">Media Files</label>
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

                    <div class="form-group">
                        <label class="control-label">Thumbnail Files</label>
                        <Rock:Grid ID="gViewThumbnailFiles" runat="server" EmptyDataText="No Media Files" RowItemText="Thumbnail" DisplayType="Light" ShowHeader="true">
                            <Columns>
                                <Rock:RockBoundField DataField="Dimension" HeaderText="Dimension" />
                                <Rock:RockBoundField DataField="FormattedFileSize" HeaderText="Size" />
                                <Rock:RockBoundField DataField="Link" HeaderText="Link" />
                            </Columns>
                        </Rock:Grid>
                    </div>


                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" data-shortcut-key="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                    </div>
                </div>
                <div id="pnlEditDetails" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.MediaElement, Rock" PropertyName="Name" />
                        </div>
                    </div>

                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.MediaElement, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />

                    <Rock:NumberBox ID="nbDuration" CssClass="input-width-xl" runat="server" NumberType="Integer" Label="Duration" AppendText="seconds" />

                    <div class="form-group">
                        <label class="control-label">Media Files</label>
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

                    <div class="form-group">
                        <label class="control-label">Thumbnail Files</label>
                        <Rock:Grid ID="gThumbnailFiles" runat="server" EmptyDataText="No Thumbnail Files" RowItemText="Thumbnail" DisplayType="Light" ShowHeader="true">
                            <Columns>
                                <Rock:RockBoundField DataField="Dimension" HeaderText="Dimension" />
                                <Rock:RockBoundField DataField="FormattedFileSize" HeaderText="Size" />
                                <Rock:RockBoundField DataField="Link" HeaderText="Link" />
                                <Rock:EditField OnClick="gThumbnailFiles_Edit" />
                            </Columns>
                        </Rock:Grid>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlNoFolder" runat="server" CssClass="panel-body" Visible="false">
                <Rock:NotificationBox ID="nbNoMediaFolder" runat="server" NotificationBoxType="Danger">
                    Cannot create a new media element because no media folder was specified.
                </Rock:NotificationBox>
            </asp:Panel>
        </div>

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
                    <div class="col-md-12">
                        <Rock:UrlLinkBox ID="urlLink" runat="server" Label="Link" ValidationGroup="MediaFile" Required="true"  />
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
                        <Rock:UrlLinkBox ID="urlThumbnailLink" runat="server" Label="Link" ValidationGroup="ThumbnailFile" Required="true" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-4">
                        <Rock:NumberBox ID="nbThumbnailWidth" runat="server" Label="Width" Help="The width in pixels of the thumbnail element." NumberType="Integer" ValidationGroup="ThumbnailFile" CssClass="input-width-md"/>
                    </div>
                    <div class="col-md-4">
                        <Rock:NumberBox ID="nbThumbnailHeight" runat="server" Label="Height" Help="The height in pixels of the thumbnail element." NumberType="Integer" ValidationGroup="ThumbnailFile" CssClass="input-width-md"/>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbThumbnailSize" runat="server" Label="Filesize" NumberType="Integer" Help="The size of the thumbnail file in bytes." ValidationGroup="ThumbnailFile" CssClass="input-width-md"/>
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
