<%@ Control Language="C#" AutoEventWireup="true" CodeFile="InteractionChannelDetail.ascx.cs" Inherits="RockWeb.Blocks.Reporting.InteractionChannelDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfChannelId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-random"></i>
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
            </div>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" Visible="false" />

                <asp:Panel ID="pnlViewDetails" runat="server">

                    <asp:Literal ID="lContent" runat="server"></asp:Literal>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" data-shortcut-key="e" AccessKey="m" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                        <span class="pull-right">
                            <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-square btn-security" Title="Secure Channel" />
                        </span>
                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlEditDetails" runat="server" Visible="false">

                    <asp:ValidationSummary ID="valChannel" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server"
                                SourceTypeName="Rock.Model.InteractionChannel, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                        </div>
                        <div class="col-md-6">
                            <Rock:NumberBox ID="nbEngagementStrength" runat="server" Label="Engagement Strength" NumberType="Integer" />
                        </div>
                        <div class="col-md-6">
                            <Rock:NumberBox ID="nbRetentionDuration" runat="server" Label="Retention Duration" NumberType="Integer" AppendText="days" />
                        </div>
                        <div class="col-md-6">
                            <Rock:NumberBox ID="nbComponentCacheDuration" runat="server" Label="Component Cache Duration" NumberType="Integer" AppendText="minutes" />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbChannelCustom1Label" runat="server"
                                SourceTypeName="Rock.Model.InteractionChannel, Rock" PropertyName="InteractionCustom1Label" />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbChannelCustom2Label" runat="server"
                                SourceTypeName="Rock.Model.InteractionChannel, Rock" PropertyName="InteractionCustom2Label" />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbChannelCustomIndexed1Label" runat="server"
                                SourceTypeName="Rock.Model.InteractionChannel, Rock" PropertyName="InteractionCustomIndexed1Label" />
                        </div>
                    </div>

                    <Rock:CodeEditor ID="ceChannelList" Visible="True" runat="server" Label="Channel List Lava Template" EditorMode="Lava" EditorHeight="200"
                        Help="This Lava template will be used by the Interactions block when viewing channel list." />

                    <Rock:CodeEditor ID="ceChannelDetail" Visible="True" runat="server" Label="Channel Detail Lava Template" EditorMode="Lava" EditorHeight="200"
                        Help="This Lava template will be used by the Interaction Channel Details block when viewing a interaction channel. This allows you to customize the layout of a channel." />

                    <Rock:CodeEditor ID="ceComponentList" Visible="True" runat="server" Label="Component List Lava Template" EditorMode="Lava" EditorHeight="200"
                        Help="This Lava template will be used by the block when viewing component list." />

                    <Rock:CodeEditor ID="ceComponentDetail" Visible="True" runat="server" Label="Component Detail Lava Template" EditorMode="Lava" EditorHeight="200"
                        Help="This Lava template will be used by the Interaction Component Details block when viewing a interaction component. This allows you to customize the layout of a component." />

                    <Rock:CodeEditor ID="ceSessionList" Visible="True" runat="server" Label="Session List Lava Template" EditorMode="Lava" EditorHeight="200"
                        Help="This Lava template will be used by the block when viewing session list." />

                    <Rock:CodeEditor ID="ceInteractionList" Visible="True" runat="server" Label="Interaction List Lava Template" EditorMode="Lava" EditorHeight="200"
                        Help="This Lava template will be used by the block when viewing interaction list." />

                    <Rock:CodeEditor ID="ceInteractionDetail" Visible="True" runat="server" Label="Interaction Detail Lava Template" EditorMode="Lava" EditorHeight="200"
                        Help="This Lava template will be used by the Interaction Details block when viewing a interaction. This allows you to customize the layout of a interaction." />

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </asp:Panel>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
