<%@ Control Language="C#" AutoEventWireup="true" CodeFile="InteractionChannelDetail.ascx.cs" Inherits="RockWeb.Blocks.Crm.InteractionChannelDetail" %>

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
                <fieldset id="fieldsetViewDetails" runat="server">

                    <asp:Literal ID="lContent" runat="server"></asp:Literal>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" ToolTip="Alt+m" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                    </div>
                </fieldset>
                <div id="pnlEditDetails" runat="server">

                    <asp:ValidationSummary ID="valChannel" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server"
                                SourceTypeName="Rock.Model.InteractionChannel, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:NumberBox ID="nbRetentionDuration" runat="server" Label="Retention Duration" NumberType="Integer" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:CodeEditor ID="ceChannelList" Visible="True" runat="server" Label="Channel List Lava Template" EditorMode="Lava" EditorHeight="275"
                                Help="This Lava template will be used by the Interactions block when viewing channel list." />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:CodeEditor ID="ceChannelDetail" Visible="True" runat="server" Label="Channel Detail Lava Template" EditorMode="Lava" EditorHeight="275"
                                Help="This Lava template will be used by the Interaction Channel Details block when viewing a interaction channel. This allows you to customize the layout of a channel." />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:CodeEditor ID="ceComponentList" Visible="True" runat="server" Label="Component List Lava Template" EditorMode="Lava" EditorHeight="275"
                                Help="This Lava template will be used by the block when viewing component list." />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:CodeEditor ID="ceComponentDetail" Visible="True" runat="server" Label="Component Detail Lava Template" EditorMode="Lava" EditorHeight="275"
                                Help="This Lava template will be used by the Interaction Component Details block when viewing a interaction component. This allows you to customize the layout of a component." />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:CodeEditor ID="ceSessionList" Visible="True" runat="server" Label="Session List Lava Template" EditorMode="Lava" EditorHeight="275"
                                Help="This Lava template will be used by the block when viewing session list." />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:CodeEditor ID="ceInteractionList" Visible="True" runat="server" Label="Interaction List Lava Template" EditorMode="Lava" EditorHeight="275"
                                Help="This Lava template will be used by the block when viewing interaction list." />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:CodeEditor ID="ceInteractionDetail" Visible="True" runat="server" Label="Interaction Detail Lava Template" EditorMode="Lava" EditorHeight="275"
                                Help="This Lava template will be used by the Interaction Details block when viewing a interaction. This allows you to customize the layout of a interaction." />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
