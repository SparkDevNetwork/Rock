<%@ Control Language="C#" AutoEventWireup="true" CodeFile="QueueDetail.ascx.cs" Inherits="RockWeb.Blocks.Bus.QueueDetail" %>

<asp:UpdatePanel ID="upUpdatePanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-code-branch"></i>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valValidation" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlViewDetails" runat="server">
                    <div>
                        <asp:Literal ID="lDescription" runat="server" />
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">
                    <div class="row">
                        <div class="col-md-6 col-sm-9">
                            <Rock:NumberBox ID="nTimeToLive" runat="server" Label="Time to Live (Seconds)" Help="The amount of seconds that a message sent on the queue can persist without being consumed before being discarded. A value less than 1 means the message will not expire." />
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
