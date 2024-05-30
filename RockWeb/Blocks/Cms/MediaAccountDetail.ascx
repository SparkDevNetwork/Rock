<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MediaAccountDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.MediaAccountDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <Rock:ModalAlert ID="mdSyncMessage" runat="server" />

            <div class="panel-heading ">
                <h1 class="panel-title"><i class="fa fa-play-circle"></i>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                    <Rock:HighlightLabel ID="hlLastRefresh" runat="server" LabelType="Info" />
                </div>
            </div>

            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:HiddenField ID="hfMediaAccountId" runat="server" />
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

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" data-shortcut-key="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" CausesValidation="false" OnClick="btnDelete_Click" />

                        <span class="pull-right">
                            <asp:LinkButton ID="btnSyncWithProvider" runat="server" CssClass="btn btn-default btn-sm btn-square" ToolTip="Download all data from provider." data-toggle="tooltip" OnClick="btnSyncWithProvider_Click" CausesValidation="false">
                                <i class="fa fa-download"></i>
                            </asp:LinkButton>
                        </span>
                    </div>
                </div>
                <div id="pnlEditDetails" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.MediaAccount, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbActive" runat="server" SourceTypeName="Rock.Model.MediaAccount, Rock" PropertyName="IsActive" Label="Active" Checked="true" Text="Yes" />
                        </div>
                    </div>
                    <div class="well">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:ComponentPicker ID="cpMediaAccountComponent" runat="server" Label="Account Type" Required="true" ContainerType="Rock.Media.MediaAccountContainer" AutoPostBack="true" OnSelectedIndexChanged="cpMediaAccountComponent_SelectedIndexChanged" />
                            </div>
                        </div>
                        <Rock:AttributeValuesContainer ID="avcComponentAttributes" runat="server" NumberOfColumns="2" />
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
