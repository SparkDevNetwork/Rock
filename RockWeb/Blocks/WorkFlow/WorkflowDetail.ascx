<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowDetail.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.WorkflowDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbNotAuthorized" runat="server" NotificationBoxType="Warning" Title="Sorry" Text="You are not authorized to view the workflow you requested." Visible="false"></Rock:NotificationBox>

        <asp:Panel ID="pnlContent" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-cog"></i>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" />
                    <Rock:HighlightLabel ID="hlState" runat="server" />
                </div>
            </div>

            <div class="panel-body">

                <asp:HiddenField ID="hfMode" runat="server" />
                <asp:HiddenField ID="hfActiveTab" runat="server" />

                <ul class="nav nav-pills margin-b-md">
                    <li id="liDetails" runat="server" class="active">
                        <a href='#<%=divDetails.ClientID%>' data-toggle="pill" data-active-div="Details">Details</a>
                    </li>
                    <li id="liActivities" runat="server">
                        <a href='#<%=divActivities.ClientID%>' data-toggle="pill" data-active-div="Activities">Activities</a>
                    </li>
                    <li id="liLog" runat="server">
                        <a href='#<%=divLog.ClientID%>' data-toggle="pill" data-active-div="Log">Log</a>
                    </li>
                </ul>

                <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div class="tab-content">

                    <div id="divDetails" runat="server" class="tab-pane active">

                        <asp:Panel ID="pnlDetailsView" runat="server" >

                            <div class="row">
                                <div class="col-md-6">
                                    <dl class="dl-horizontal margin-b-md">
                                        <Rock:TermDescription ID="tdName" runat="server" Term="Name" />
                                        <Rock:TermDescription ID="tdInitiator" runat="server" Term="Initiator" />
                                        <Rock:TermDescription ID="tdStatus" runat="server" Term="Status" />
                                    </dl>
                                </div>
                                <div class="col-md-6">
                                    <dl class="dl-horizontal margin-b-md">
                                        <Rock:TermDescription ID="tdActivatedWhen" runat="server" Term="Activated" />
                                        <Rock:TermDescription ID="tdLastProcessed" runat="server" Term="Last Processed" />
                                        <Rock:TermDescription ID="tdCompletedWhen" runat="server" Term="Completed" />
                                    </dl>
                                    <dl class="dl-horizontal margin-b-md">
                                        <asp:PlaceHolder ID="phViewAttributes" runat="server" />
                                    </dl>
                                </div>
                            </div>

                        </asp:Panel>

                        <asp:Panel ID="pnlDetailsEdit" runat="server">

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Workflow, Rock" PropertyName="Name" />
                                    <Rock:PersonPicker ID="ppInitiator" runat="server" Label="Initiator" Help="The person who initiated this workflow." />
                                    <Rock:DataTextBox ID="tbStatus" runat="server" SourceTypeName="Rock.Model.Workflow, Rock" PropertyName="Status" Label="Status Text" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockCheckBox ID="cbIsCompleted" runat="server" Label="Completed" Text="Yes" />
                                    <Rock:RockLiteral ID="lIsCompleted" runat="server" Label="Completed" Visible="false" />
                                    <Rock:RockControlWrapper ID="cwState" runat="server">
                                        <asp:Literal ID="lState" runat="server" />
                                    </Rock:RockControlWrapper>
                                </div>
                            </div>
                            <asp:PlaceHolder ID="phAttributes" runat="server" />

                        </asp:Panel>

                    </div>

                    <div id="divActivities" runat="server" class="tab-pane">

                        <asp:Panel ID="pnlActivitesView" runat="server">

                            <asp:Repeater ID="rptrActivities" runat="server">
                                <ItemTemplate>
                                    <div class="panel panel-block workflow-activity">
                                        <div class="panel-heading">
                                            <h3 class="panel-title"><%# Eval("ActivityType.Name") %></h3>
                                            <div class="panel-labels">
                                                <asp:Label ID="lblComplete" runat="server" CssClass="label label-default">Complete</asp:Label>
                                                <asp:Label ID="lblActive" runat="server" CssClass="label label-success">Active</asp:Label>
                                            </div>
                                        </div>
                                        <div class="panel-body">
                                            <div class="row">
                                                <div class="col-md-6">
                                                    <dl class="dl-horizontal margin-b-md">
                                                        <Rock:TermDescription ID="tdAssignedToPerson" runat="server" Term="Assigned To Person"></Rock:TermDescription>
                                                        <Rock:TermDescription ID="tdAssignedToGroup" runat="server" Term="Assigned To Group"></Rock:TermDescription>
                                                    </dl>
                                                </div>
                                                <div class="col-md-6">
                                                    <dl class="dl-horizontal margin-b-md">
                                                        <Rock:TermDescription ID="tdActivated" runat="server" Term="Activated"></Rock:TermDescription>
                                                        <Rock:TermDescription ID="tdCompleted" runat="server" Term="Completed"></Rock:TermDescription>
                                                    </dl>
                                                    <dl class="dl-horizontal margin-b-md">
                                                        <asp:PlaceHolder ID="phActivityAttributes" runat="server" />
                                                    </dl>
                                                </div>
                                            </div>
                                            <fieldset>
                                                <legend>Actions</legend>
                                                <Rock:Grid ID="gridActions" runat="server" DisplayType="Light" AllowSorting="false">
                                                    <Columns>
                                                        <Rock:RockBoundField DataField="Name" HeaderText="Action" />
                                                        <Rock:RockBoundField DataField="LastProcessed" HeaderText="Last Processed" />
                                                        <Rock:BoolField DataField="Completed" HeaderText="Completed" />
                                                        <Rock:RockBoundField DataField="CompletedWhen" />
                                                    </Columns>
                                                </Rock:Grid>
                                            </fieldset>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>

                        </asp:Panel>

                        <asp:Panel ID="pnlActivitesEdit" runat="server">

                            <div class="workflow-activity-list">
                                <asp:PlaceHolder ID="phActivities" runat="server" />
                            </div>
                            <span class="pull-right">
                                <asp:DropDownList ID="ddlActivateNewActivity" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlActivateNewActivity_SelectedIndexChanged" />
                            </span>

                        </asp:Panel>

                    </div>

                    <div ID="divLog" runat="server" class="tab-pane">

                        <div class="grid">
                            <Rock:Grid ID="gLog" runat="server" AllowSorting="false" RowItemText="Entry">
                                <Columns>
                                    <Rock:DateTimeField DataField="LogDateTime" HeaderText="When" FormatAsElapsedTime="true" />
                                    <Rock:RockBoundField DataField="LogText" HeaderText="Message" />
                                    <Rock:RockBoundField DataField="CreatedByPersonAlias.Person.FullName" HeaderText="By" />
                                </Columns>
                            </Rock:Grid>
                        </div>

                    </div>
                        
                </div>

                <div class="actions margin-b-md">
                    <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" Visible="false" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

        </asp:Panel>

        <script>
            Sys.Application.add_load(function () {
                $('a[data-toggle="pill"]').on('shown.bs.tab', function (e) {
                    $('#<%=hfActiveTab.ClientID%>').val($(e.target).attr("data-active-div"));
                });
            })
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
