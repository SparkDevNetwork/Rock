<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.WorkflowDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">

            <div class="banner">
                <h1>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
                <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" />
            </div>
            
            <div id="pnlEditDetails" runat="server">

                <asp:HiddenField ID="hfActiveTab" runat="server" />
                <ul class="nav nav-pills margin-b-md">
                    <li id="liDetails" runat="server" class="active"><a href='#<%=divDetails.ClientID%>' data-toggle="pill" data-active-div="Details">Details</a></li>
                    <li id="liActivities" runat="server"><a href='#<%=divActivities.ClientID%>' data-toggle="pill" data-active-div="Activities">Activities</a></li>
                    <li id="liLog" runat="server"><a href='#<%=divLog.ClientID%>' data-toggle="pill" data-active-div="Log">Log</a></li>
                </ul>

                <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div class="tab-content">

                    <div id="divDetails" runat="server" class="tab-pane active">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Workflow, Rock" PropertyName="Name" />
                                <Rock:DataTextBox ID="tbStatus" runat="server" SourceTypeName="Rock.Model.Workflow, Rock" PropertyName="Status" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbIsCompleted" runat="server" Label="Completed" Text="Yes" />
                                <Rock:RockControlWrapper ID="cwState" runat="server" Label="&nbsp;">
                                    <asp:Literal ID="lState" runat="server" />
                                </Rock:RockControlWrapper>
                            </div>
                        </div>
                        <asp:PlaceHolder ID="phAttributes" runat="server" />
                    </div>

                    <div id="divActivities" runat="server" class="tab-pane">
                        <div class="workflow-activity-list">
                            <asp:PlaceHolder ID="phActivities" runat="server" />
                        </div>
                        <span class="pull-right">
                            <asp:DropDownList ID="ddlActivateNewActivity" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlActivateNewActivity_SelectedIndexChanged" />
                        </span>
                    </div>

                    <div id="divLog" runat="server" class="tab-pane">
                        <Rock:Grid ID="gLog" runat="server" AllowSorting="false" RowItemText="Entry">
                            <Columns>
                                <Rock:DateTimeField DataField="LogDateTime" HeaderText="When" FormatAsElapsedTime="true" />
                                <asp:BoundField DataField="LogText" HeaderText="Message" />
                            </Columns>
                        </Rock:Grid>
                    </div>

                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
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
