<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowLaunch.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.WorkflowLaunch" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lIcon" runat="server" />
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
            </div>

            <div class="panel-body">
                <asp:ValidationSummary ID="valValidation" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div class="margin-t-md">
                    <asp:Literal ID="lEntityTypeName" runat="server" />
                </div>

                <div class="row d-flex flex-wrap margin-t-sm">
                    <asp:Repeater ID="rEntitySetItems" runat="server">
                        <ItemTemplate>
                            <div class="col-xs-12 col-sm-6 col-md-3 mb-sm-3">
                                <%# Eval("Html") %>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <div class="margin-b-lg">
                    <small class="text-muted">
                        <asp:Literal ID="lSummary" runat="server" />
                        <asp:LinkButton ID="lbShowAll" runat="server" OnClick="lbShowAll_Click" CssClass="margin-l-sm" CausesValidation="false">
                            Show All
                        </asp:LinkButton>
                    </small>
                </div>

                <div class="row">
                    <div class="col-md-6 col-lg-3">
                        <Rock:WorkflowTypePicker ID="wtpWorkflowType" runat="server" Label="Workflow Type" Required="true" />
                        <Rock:RockDropDownList ID="ddlWorkflowType" runat="server" Label="Workflow Type" Required="true" DataValueField="Id" DataTextField="Name" />
                        <asp:Literal ID="lWorkflowType" runat="server" />
                    </div>
                </div>

                <Rock:NotificationBox ID="nbNotificationBox" runat="server" />

                <div class="margin-t-lg">
                    <Rock:BootstrapButton ID="btnLaunch" runat="server" Text="Launch" CssClass="btn btn-primary" OnClick="btnLaunch_Click" />
                    <Rock:BootstrapButton ID="btnReset" runat="server" Text="Launch Another Workflow" CssClass="btn btn-default" OnClick="btnReset_Click" />
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>