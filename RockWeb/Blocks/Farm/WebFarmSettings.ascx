<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WebFarmSettings.ascx.cs" Inherits="RockWeb.Blocks.Farm.WebFarmSettings" %>

<asp:UpdatePanel ID="upUpdatePanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-network-wired"></i>
                    Web Farm Settings
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlActive" runat="server" />
                </div>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valValidation" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlViewDetails" runat="server">
                    <div class="row">
                        <div class="col-md-4">
                            <asp:Literal ID="lDescription" runat="server" />
                        </div>
                        <div class="col-md-8">
                            <h5>Nodes</h5>
                            <div class="row">
                                <asp:Repeater ID="rNodes" runat="server">
                                    <ItemTemplate>
                                        <div class="col-sm-6 col-md-4 col-lg-3">
                                            <h6><%# Eval("NodeName") %></h6>
                                            <dl>
                                                <dt>Active</dt>
                                                <dd><%# Eval("IsActive") %></dd>
                                                <dt>Leader</dt>
                                                <dd><%# Eval("IsLeader") %></dd>
                                                <dt>Job Runner</dt>
                                                <dd><%# Eval("IsJobRunner") %></dd>
                                                <dt>Last Seen</dt>
                                                <dd><%# Eval("LastSeen") %></dd>
                                                <dt>Polling Interval Seconds</dt>
                                                <dd><%# Eval("PollingIntervalSeconds") %></dd>
                                            </dl>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">
                    <div class="alert alert-info">
                        In order to respect any new setting changes made here, please restart Rock on each node after saving. 
                    </div>

                    <div class="row">
                        <div class="col-md-6 col-sm-9">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                            <Rock:RockTextBox ID="tbWebFarmKey" runat="server" Label="Key" Help="This feature is intended for enterprise size churches that would benefit from a distributed environment. Most Rock churches should not use the Web Farm because of the low level of benefit and a high complexity cost. A special key is required to activate this feature." />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6 col-sm-9">
                            <Rock:NumberBox ID="nbPollingMin" runat="server" Label="Polling Minimum" Help="The number of seconds that is the minimum wait time before a node attempts to execute leadership. If this value is left blank, then a default will be used." />
                            <Rock:NumberBox ID="nbPollingMax" runat="server" Label="Polling Maximum" Help="The number of seconds that is the maximum wait time before a node attempts to execute leadership. If this value is left blank, then a default will be used." />
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
