<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MediaFolderDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.MediaFolderDetail" %>

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
                <asp:HiddenField ID="hfMediaAccountId" runat="server" />
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlViewDetails" runat="server">
                    <div class="row margin-b-lg">
                        <div class="col-md-12">
                            <asp:Literal ID="lDescription" runat="server" />
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                    </div>
                </div>
                <div id="pnlEditDetails" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.MediaFolder, Rock" PropertyName="Name" />
                        </div>
                    </div>

                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.MediaFolder, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />

                    <Rock:Switch ID="swEnableContentChannelSync" runat="server" Text="Enable Content Channel Sync" OnCheckedChanged="swEnableContentChannelSync_CheckedChanged" AutoPostBack="true" />

                    <asp:Panel ID="pnlContentChannel" runat="server">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlContentChannel" runat="server" Label="Content Channel" Required="true" AutoPostBack="true" OnSelectedIndexChanged="ddlContentChannel_SelectedIndexChanged"  Help="The content channel that content channel items will be added to when new media files are created."/>
                            </div>
                            <div class="col-md-6">
                                <Rock:RockRadioButtonList ID="rrbContentChannelItemStatus" runat="server" Label="Content Channel Item Status" RepeatDirection="Horizontal" Required="true" Help="The status to use for the new content channel items that are created." />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlChannelAttribute" runat="server" Label="Media File Attribute" Required="true" Help="The attribute that the media will be assigned to. The attribute must be of type 'Media File'."/>
                            </div>
                        </div>
                    </asp:Panel>
                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
