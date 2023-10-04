<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AutomationJobConfiguration.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.DatabaseThinner.AutomationJobConfiguration" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">
            <div class="panel-heading">
                <h3 class="panel-title">Automation Settings</h3>
            </div>

            <div class="panel-body">
                <asp:Panel ID="pnlCompressCommunications" runat="server" CssClass="panel panel-default">
                    <div class="panel-heading">
                        <h3 class="panel-title">Compress Communications</h3>
                    </div>

                    <div class="panel-body">
                        <Rock:RockCheckBox ID="cbCompressCommunicationsEnabled" runat="server" Label="Enabled" AutoPostBack="true" OnCheckedChanged="bgCompressCommunicationsEnabled_CheckedChanged" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:NumberBox ID="nbCompressCommunicationsOlderThan" runat="server" Label="Compress Content Older Than" Help="The number of days that a communication must be older than in order to be compressed." MinimumValue="0" />
                            </div>

                            <div class="col-md-6">
                                <Rock:NumberBox ID="nbCompressCommunicationsBatchSize" runat="server" Label="Batch Size" Help="The number of communications to compress or decompress in a single job run." MinimumValue="0" />
                            </div>
                        </div>

                        <Rock:RockCheckBox ID="cbCompressCommunicationsShouldDecompress" runat="server" Label="Decompress Newer Communications" Help="Should communications that are newer than the specified time period be decompressed." />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlDeleteSystemCommunications" runat="server" CssClass="panel panel-default">
                    <div class="panel-heading">
                        <h3 class="panel-title">Delete System Communications</h3>
                    </div>

                    <div class="panel-body">
                        <Rock:RockCheckBox ID="cbDeleteSystemCommunicationsEnabled" runat="server" Label="Enabled" AutoPostBack="true" OnCheckedChanged="cbDeleteSystemCommunicationsEnabled_CheckedChanged" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:NumberBox ID="nbDeleteSystemCommunicationsOlderThan" runat="server" Label="Delete Communications Older Than" Help="A message must be at least this many days old to be considered for deletion." MinimumValue="0" />
                                <Rock:NumberBox ID="nbDeleteSystemCommunicationsMaximumDaysBack" runat="server" Label="Maximum Days Back" Help="A message must be no more than this many days old to be considered for deletion." MinimumValue="0" />
                                <asp:LinkButton ID="lbDeleteSystemCommunicationsSuggestSubjects" runat="server" Text="Suggest Subjects" CssClass="btn btn-default" OnClick="lbDeleteSystemCommunicationsSuggestSubjects_Click" />
                            </div>

                            <div class="col-md-6">
                                <Rock:ValueList ID="vlDeleteSystemCommunicationsSubjects" runat="server" Label="Subjects" Help="E-mail communications must match one of these subjects to be considered for deletion." />
                            </div>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlTransactionImages" runat="server" CssClass="panel panel-default">
                    <div class="panel-heading">
                        <h3 class="panel-title">Delete Transaction Images</h3>
                    </div>

                    <div class="panel-body">
                        <Rock:RockCheckBox ID="cbTransactionImagesEnabled" runat="server" Label="Enabled" AutoPostBack="true" OnCheckedChanged="cbTransactionImagesEnabled_CheckedChanged" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:NumberBox ID="nbTransactionImagesOlderThan" runat="server" Label="Delete Images Older Than" Help="The number of days that a transaction must be older than in order for it's images to be deleted." MinimumValue="0" />
                            </div>

                            <div class="col-md-6">
                            </div>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlUnusedFiles" runat="server" CssClass="panel panel-default">
                    <div class="panel-heading">
                        <h3 class="panel-title">Delete Unused Files</h3>
                    </div>

                    <div class="panel-body">
                        <Rock:RockCheckBox ID="cbUnusedFilesEnabled" runat="server" Label="Enabled" AutoPostBack="true" OnCheckedChanged="cbUnusedFilesEnabled_CheckedChanged" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:NumberBox ID="nbUnusedFilesQuarantineDays" runat="server" Label="Days to Quarantine Files" Help="The number of days to keep files in quarantine before they are automatically deleted." MinimumValue="0" />
                            </div>

                            <div class="col-md-6">
                            </div>
                        </div>
                    </div>
                </asp:Panel>

                <Rock:NotificationBox ID="nbSaved" runat="server" NotificationBoxType="Info" Dismissable="true" Visible="false">
                    Settings have been saved.
                </Rock:NotificationBox>

                <asp:LinkButton ID="lbSave" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="lbSave_Click" />
            </div>
        </div>

        <Rock:ModalDialog ID="mdSuggestSubjects" runat="server" Title="Suggested Subjects" OnSaveClick="mdSuggestSubjects_SaveClick">
            <Content>
                <asp:HiddenField ID="hfSuggestedSubjects" runat="server" />

                <div class="alert alert-info">
                    <p>
                        Below is a list of e-mail communication subjects that we recommend considering for deleting.
                        Before selecting these subjects to delete, consider what their content is and if it is
                        something that you want to keep around forever or not.
                    </p>
                    <p>
                        The numbers are based off the <code>Older Than</code> and <code>Maximum Days Back</code> settings you have already configured.
                    </p>
                </div>

                <Rock:Grid ID="gSuggestedSubjects" runat="server" AllowSorting="true" OnGridRebind="gSuggestedSubjects_GridRebind">
                    <Columns>
                        <Rock:SelectField />
                        <Rock:RockBoundField DataField="Subject" HeaderText="Subject" SortExpression="Subject" />
                        <Rock:RockBoundField DataField="Count" HeaderText="Count" SortExpression="Count" DataFormatString="{0:N0}" />
                        <Rock:RockBoundField DataField="SizeMB" HeaderText="Size" SortExpression="Size" DataFormatString="{0:N2} MB" />
                    </Columns>
                </Rock:Grid>
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
