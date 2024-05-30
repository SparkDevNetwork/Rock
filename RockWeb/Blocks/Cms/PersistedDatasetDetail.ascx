<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersistedDatasetDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.PersistedDatasetDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading ">
                <h1 class="panel-title">
                    <i class="fa fa-database"></i>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </h1>
            </div>

            <asp:HiddenField ID="hfPersistedDatasetId" runat="server" />

            <div class="panel-body">
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.PersistedDataset, Rock" PropertyName="Name" />

                        <Rock:NotificationBox ID="nbAccessKeyWarning" runat="server" NotificationBoxType="Warning" />
                        <Rock:DataTextBox ID="tbAccessKey" runat="server" SourceTypeName="Rock.Model.PersistedDataset, Rock" Required="true" PropertyName="AccessKey" Help="The key to use to uniquely identify this dataset. This will be the key to use when using the PersistedDataset lava filter." />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" Help="Set this to false to have the PersistedDataset lava filter return null for this dataset, and to exclude this dataset when rebuilding." />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.PersistedDataset, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                        <Rock:CodeEditor ID="ceBuildScript" runat="server" EditorMode="Lava" Label="Build Script" Help="Lava Template to use for building JSON that will be used as the cached dataset object." EditorHeight="300" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:LavaCommandsPicker ID="lcpEnabledLavacommands" runat="server" Label="Enabled Lava Commands" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbRefreshIntervalHours" runat="server" Label="Refresh Interval" AppendText="hour(s)" CssClass="input-width-md" NumberType="Double" Required="false" Help="How often the dataset should be updated by the Update Persisted Dataset job." />
                        <Rock:NumberBox ID="nbMemoryCacheDurationHours" runat="server" Label="Memory Cache Duration" AppendText="hour(s)" CssClass="input-width-md" NumberType="Double" Help="How long the persisted object should be cached in memory. This is a sliding timeline, so each time the object is read the counter will reset. Leave blank to not cache the object in memory which will mean it will be deserialized into the object on each request (still fast). " />
                        <Rock:DatePicker ID="dtpExpireDateTime" runat="server" Label="Expires on" Help="Set this to consider the dataset inactive after the specified date. This will mean that its value is no longer updated by the refresh job and that it will return empty when requested through Lava." />
                    </div>
                    <div class="col-md-6">
                        <Rock:EntityTypePicker ID="etpEntityType" runat="server" Required="false" Label="Entity Type" Help="Set this to indicate which EntityType the JSON object should be associated with. This will be used by the PersistedDataset Lava Filter when entity related options such as 'AppendFollowing' are specified.'" />

                        <Rock:RockCheckBox ID="cbAllowManualRefresh" runat="server" Label="Allow Manual Refresh" Help="Determines if the persisted dataset can be manually refreshed in the Persisted Dataset list." />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
