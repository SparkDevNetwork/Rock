<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionRequestDetail.ascx.cs" Inherits="RockWeb.Blocks.Connection.ConnectionRequestDetail" %>

<style>
    #<%= upDetail.ClientID %> .request-photo {
        width: 100%;
        max-width: 200px;
        margin: 0 auto 16px;
        background: center/cover #cbd4db;
        border-radius: 50%;
        box-shadow: inset 0 0 0 1px rgba(0, 0, 0, 0.07);
    }

    #<%= upDetail.ClientID %> .request-photo:after {
        content: "";
        display: block;
        padding-bottom: 100%;
    }

    #<%= upDetail.ClientID %> .board-card-photo {
        width: 24px;
        height: 24px;
        align-items: center;
        background: center/cover #cbd4db;
        border-radius: 50%;
        box-shadow: inset 0 0 0 1px rgba(0, 0, 0, 0.07);
        -moz-box-sizing: border-box;
        box-sizing: border-box;
        display: inline-flex;
        justify-content: center;
        position: relative;
        vertical-align: top; 
    }

    .lava-activity-add-no-table-border {
        border-style:hidden !important;
    }
</style>

<script type="text/javascript">
    function cancelDeleteActivity() {
        var pathName = window.location.pathname + "?ConnectionRequestId=" + escape($("#hfConnectionRequestId").val()) + "&ConnectionOpportunityId=" + escape($("#hfConnectionOpportunityId").val());
        console.debug('cancelDeleteActivity', pathName);
        window.location = pathName;
    }
</script>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbSecurityWarning" runat="server" NotificationBoxType="Warning" Text="The information provided is not valid or you are not authorized to view this content." Visible="false" />
        <asp:Panel ID="pnlDetail" runat="server">
            <asp:HiddenField ID="hfConnectionOpportunityId" ClientIDMode="Static" runat="server" />
            <asp:HiddenField ID="hfConnectionRequestId" ClientIDMode="Static" runat="server" />
            <asp:HiddenField ID="hfActiveDialog" runat="server" />

            <Rock:NotificationBox ID="nbNoParameterMessage" runat="server" NotificationBoxType="Warning" Heading="Missing Parameter(s)"
                Text="This block requires a valid connection request id and/or a connection opportunity id as query string parameters." />

            <asp:Panel ID="pnlContents" runat="server" CssClass="panel panel-block">
                <div id="divHeaderDefault" runat="server">
                    <div class="panel-heading">
                        <h1 class="panel-title">
                            <asp:Literal ID="lConnectionOpportunityIconHtml" runat="server" />Connection Request Detail
                        </h1>
                        <div class="panel-labels pt-2 mb-3">
                            <Rock:HighlightLabel ID="hlCampus" runat="server" LabelType="Campus" />
                            <Rock:HighlightLabel ID="hlOpportunity" runat="server" LabelType="Info" />
                            <Rock:HighlightLabel ID="hlStatus" runat="server" LabelType="Default" Visible="false" />
                            <Rock:HighlightLabel ID="hlState" runat="server" Visible="false" />
                        </div>
                    </div>
                </div>
                <asp:Panel ID="pnlReadDetails" runat="server">

                    <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>

                    <div class="panel-body">
                        <asp:Literal ID="lHeading" runat="server"></asp:Literal>
                        <div class="row">
                            <div class="col-sm-2">
                                <div id="divPhoto" runat="server" class="request-photo mb-1"></div>
                            </div>
                            <div class="col-sm-10">
                                <asp:HyperLink ID="lbProfilePage" runat="server" CssClass="small pull-right">
                                <i class="fa fa-user"></i>
                                Person Profile
                                </asp:HyperLink>
                                <h3 class="mt-0 mb-3">
                                    <asp:Literal ID="lTitle" runat="server" />
                                </h3>
                                <div class="row">
                                    <div class="col-sm-6 col-md-5 mb-3">
                                        <h6 class="mt-0 mb-1">Contact Information</h6>
                                        <div class="personcontact">
                                            <Rock:RockLiteral ID="lContactInfo" runat="server" />
                                        </div>
                                    </div>
                                    <div class="col-sm-6 col-md-4 mb-3">
                                        <h6 class="mt-0 mb-1">Connector</h6>
                                        <div class="btn-group">
                                            <div class="dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" style="cursor: pointer;">
                                                <asp:Literal runat="server" ID="lConnectorProfilePhoto" />
                                                <asp:Literal ID="lConnectorFullName" runat="server" />
                                            </div>
                                            <ul class="dropdown-menu">
                                                <asp:Repeater ID="rConnectorSelect" runat="server" OnItemCommand="rConnectorSelect_ItemCommand">
                                                    <ItemTemplate>
                                                        <li>
                                                            <asp:LinkButton runat="server" CommandArgument='<%# Eval("PersonAliasId") %>'>
                                                            <%# Eval("FullName") %>
                                                            </asp:LinkButton>
                                                        </li>
                                                    </ItemTemplate>
                                                </asp:Repeater>
                                            </ul>
                                        </div>
                                    </div>
                                    <div class="col-sm-12 col-md-3 text-left text-md-right mb-3">
                                        <Rock:RockLiteral ID="lRequestDate" runat="server" Label="Request Date" />
                                        <Rock:RockLiteral ID="lPlacementGroup" runat="server" Label="Placement Group" />
                                        <Rock:DynamicPlaceholder ID="phGroupMemberAttributesView" runat="server" />
                                    </div>
                                </div>

                                <asp:Panel runat="server" CssClass="badge-bar margin-b-sm" ID="pnlBadges">
                                    <Rock:BadgeListControl ID="blStatus" runat="server" />
                                </asp:Panel>

                            </div>

                        </div>

                        <div class="row">
                            <div class="col-md-12">
                                <Rock:RockLiteral ID="lComments" runat="server" />
                            </div>
                        </div>
                        <asp:Literal ID="lBadgeBar" runat="server" />
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:AttributeValuesContainer ID="avcAttributesReadOnly" runat="server" DisplayAsTabs="true" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:ModalAlert ID="mdWorkflowLaunched" runat="server" />
                                <asp:Label ID="lblWorkflows" Text="Available Workflows" Font-Bold="true" runat="server" />
                                <div class="margin-b-lg">
                                    <asp:Repeater ID="rptRequestWorkflows" runat="server">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="lbRequestWorkflow" runat="server" CssClass="btn btn-default btn-xs" CommandArgument='<%# Eval("Id") %>' CommandName="LaunchWorkflow">
                                         <i class="<%# Eval("WorkflowType.IconCssClass") %>"></i>&nbsp;<%# Eval("WorkflowType.Name") %>
                                            </asp:LinkButton>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <asp:Panel ID="pnlRequirements" runat="server">
                                    <Rock:RockControlWrapper ID="rcwRequirements" runat="server" Label="Group Requirements">
                                        <Rock:RockCheckBoxList ID="cblManualRequirements" RepeatDirection="Vertical" runat="server" Label="" />
                                        <Rock:NotificationBox ID="nbRequirementsErrors" runat="server" Dismissable="true" NotificationBoxType="Danger" />
                                        <div class="labels">
                                            <asp:Literal ID="lRequirementsLabels" runat="server" />
                                        </div>
                                    </Rock:RockControlWrapper>
                                </asp:Panel>
                            </div>
                        </div>

                        <div class="actions">
                            <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="lbEdit_Click"></asp:LinkButton>
                            <asp:LinkButton ID="lbTransfer" runat="server" Text="Transfer" CssClass="btn btn-link" CausesValidation="false" OnClick="lbTransfer_Click"></asp:LinkButton>
                            <div class="pull-right">
                                <asp:LinkButton ID="lbConnect" runat="server" Text="Connect" CssClass="btn btn-success" CausesValidation="false" OnClick="lbConnect_Click"></asp:LinkButton>
                            </div>
                        </div>

                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlEditDetails" runat="server" Visible="false">

                    <div class="panel-body">

                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                        <asp:CustomValidator ID="cvConnectionRequest" runat="server" Display="None" />
                        <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" />
                        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                        <div class="row">
                            <div class="col-md-3">
                                <Rock:PersonPicker runat="server" ID="ppRequestor" Label="Requestor" Required="true" OnSelectPerson="ppRequestor_SelectPerson" />
                            </div>
                            <div class="col-md-3">
                                <Rock:RockDropDownList ID="ddlConnectorEdit" runat="server" Label="Connector" EnhanceForLongLists="true" />
                            </div>
                            <div class="col-md-4 col-md-offset-2">
                                <Rock:RockRadioButtonList ID="rblState" runat="server" Label="State" RepeatDirection="Horizontal" OnSelectedIndexChanged="rblState_SelectedIndexChanged" AutoPostBack="true" />
                                <Rock:DatePicker ID="dpFollowUp" runat="server" Label="Follow-up Date" AllowPastDateSelection="false" Visible="false" />
                            </div>
                        </div>

                        <Rock:RockTextBox ID="tbComments" Label="Comments" runat="server" TextMode="MultiLine" Rows="4" ValidateRequestMode="Disabled" />

                        <Rock:RockRadioButtonList ID="rblStatus" runat="server" Label="Status" RepeatDirection="Horizontal" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlPlacementGroup" runat="server" Label="Placement Group" AutoPostBack="true" OnSelectedIndexChanged="ddlPlacementGroup_SelectedIndexChanged" EnhanceForLongLists="true" />
                                <Rock:RockDropDownList ID="ddlPlacementGroupRole" runat="server" Label="Group Member Role" Visible="false" AutoPostBack="true" OnSelectedIndexChanged="ddlPlacementGroupRole_SelectedIndexChanged" />
                                <Rock:RockDropDownList ID="ddlPlacementGroupStatus" runat="server" Label="Group Member Status" Visible="false" />
                            </div>
                            <div class="col-md-6">
                                <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" AutoPostBack="true" OnSelectedIndexChanged="cpCampus_SelectedIndexChanged" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:AttributeValuesContainer ID="avcAttributes" runat="server" />
                            </div>
                        </div>

                        <asp:HiddenField ID="hfGroupMemberAttributeValues" runat="server" />
                        <Rock:DynamicPlaceholder ID="phGroupMemberAttributes" runat="server" />

                        <Rock:NotificationBox ID="nbRequirementsWarning" runat="server" NotificationBoxType="Warning" Visible="false" />

                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click"></asp:LinkButton>
                            <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false"></asp:LinkButton>
                        </div>

                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlTransferDetails" runat="server" Visible="false">

                    <div class="panel-body">
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:NotificationBox
                                    runat="server"
                                    ID="nbTranferFailed"
                                    Text="You must select a new opportunity to transfer this request."
                                    Visible="false"
                                    NotificationBoxType="Warning"></Rock:NotificationBox>
                            </div>
                            <div class="col-md-6">

                                <Rock:RockControlWrapper ID="rcwTransferOpportunity" runat="server" Label="Opportunity">
                                    <div class="row">
                                        <div class="col-md-8">
                                            <Rock:RockDropDownList ID="ddlTransferOpportunity" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlTransferOpportunity_SelectedIndexChanged" EnhanceForLongLists="true" />
                                        </div>
                                        <div class="col-md-4">
                                            <Rock:BootstrapButton ID="btnSearch" runat="server" CssClass="btn btn-primary" Text="Search" OnClick="btnSearch_Click" />
                                        </div>
                                    </div>
                                </Rock:RockControlWrapper>

                                <Rock:RockControlWrapper ID="rcwConnector" runat="server" Label="Connector">
                                    <div>
                                        <Rock:RockRadioButton ID="rbTransferDefaultConnector" runat="server" CssClass="js-transfer-connector" Text="Default Connector" GroupName="TransferOpportunityConnector" />
                                    </div>
                                    <div>
                                        <Rock:RockRadioButton ID="rbTransferCurrentConnector" runat="server" CssClass="js-transfer-connector" Text="Current Connector" GroupName="TransferOpportunityConnector" />
                                    </div>
                                    <div>
                                        <Rock:RockRadioButton ID="rbTransferSelectConnector" runat="server" CssClass="js-transfer-connector" Text="Select Connector" GroupName="TransferOpportunityConnector" />
                                        <Rock:RockDropDownList ID="ddlTransferOpportunityConnector" CssClass="margin-l-lg" runat="server" Style="display: none" />
                                    </div>
                                    <div>
                                        <Rock:RockRadioButton ID="rbTransferNoConnector" runat="server" CssClass="js-transfer-connector" Text="No Connector" GroupName="TransferOpportunityConnector" />
                                    </div>
                                </Rock:RockControlWrapper>

                            </div>
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlTransferStatus" runat="server" Label="Status" />
                                <Rock:CampusPicker ID="cpTransferCampus" runat="server" Label="Campus" AutoPostBack="true" OnSelectedIndexChanged="cpTransferCampus_SelectedIndexChanged" />
                            </div>
                        </div>

                        <Rock:RockTextBox ID="tbTransferNote" runat="server" Label="Note" TextMode="MultiLine" Rows="4" />

                        <div class="actions">
                            <asp:LinkButton ID="btnTransferSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Transfer" CssClass="btn btn-primary" OnClick="btnTransferSave_Click"></asp:LinkButton>
                            <asp:LinkButton ID="btnTransferCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false"></asp:LinkButton>
                        </div>

                    </div>

                </asp:Panel>

            </asp:Panel>

            <Rock:PanelWidget ID="wpConnectionRequestWorkflow" runat="server" Title="Workflows" CssClass="clickable">
                <div class="grid">
                    <Rock:Grid ID="gConnectionRequestWorkflows" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Location" OnRowSelected="gConnectionRequestWorkflows_RowSelected">
                        <Columns>
                            <Rock:RockBoundField DataField="WorkflowType" HeaderText="Workflow Type" />
                            <Rock:RockBoundField DataField="Trigger" HeaderText="Trigger" />
                            <Rock:RockBoundField DataField="CurrentActivity" HeaderText="Current Activity" />
                            <Rock:RockBoundField DataField="Date" HeaderText="Start Date" />
                            <Rock:RockBoundField DataField="Status" HeaderText="Status" HtmlEncode="false" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </Rock:PanelWidget>

            <asp:Panel ID="pnlConnectionRequestActivities" runat="server" CssClass="panel panel-block" Visible="true">
                <div id="divLavaActivities" runat="server">
                    <div class="panel-body">
                        <div id="divLavaActivitiesContent" >
                            <asp:Literal ID="lActivityLavaTemplate" runat="server"></asp:Literal>
                        </div>
                        <table class="table table-condensed table-light lava-activity-add-no-table-border">
                            <tfoot>
                                <tr title="">
                                    <td class="grid-actions" colspan="6">
                                        <asp:LinkButton ID="lbActivityAdd" runat="server" AccessKey="n" ToolTip="Alt+N" CssClass="btn btn-grid-action btn-add btn-default btn-sm" OnClick="lbActivityAdd_Click">
                                            <i class="fa fa-plus-circle fa-fw"></i>
                                        </asp:LinkButton>
                                    </td>
                                </tr>
                            </tfoot>
                        </table>
                    </div>
                </div>

                <div id="divGridActivities" runat="server">
                    <div class="panel-heading">
                        <h1 class="panel-title">Activities</h1>
                    </div>
                    <div class="panel-body">
                        <div class="grid grid-panel">
                            <Rock:Grid ID="gConnectionRequestActivities" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Activity"
                                OnRowDataBound="gConnectionRequestActivities_RowDataBound" OnRowSelected="gConnectionRequestActivities_Edit">
                                <Columns>
                                    <Rock:RockBoundField DataField="Date" HeaderText="Date" />
                                    <Rock:RockBoundField DataField="Activity" HeaderText="Activity" />
                                    <Rock:RockBoundField DataField="Opportunity" HeaderText="Opportunity" />
                                    <Rock:RockBoundField DataField="Connector" HeaderText="Connector" />
                                    <Rock:RockBoundField DataField="Note" HeaderText="Note" />
                                    <Rock:DeleteField OnClick="gConnectionRequestActivities_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </div>
            </asp:Panel>

            <Rock:ModalDialog ID="dlgDeleteActivity" runat="server" SaveButtonText="Ok" OnSaveClick="dlgDeleteActivity_SaveClick" OnCancelScript="cancelDeleteActivity()" Title="Delete Activity">
                <Content>
                    <span>Are you sure you want to delete this Activity?</span>
                </Content>
            </Rock:ModalDialog>

            <Rock:ModalDialog ID="dlgConnectionRequestActivities" runat="server" SaveButtonText="Add" OnSaveClick="btnAddConnectionRequestActivity_Click" Title="Add Activity" ValidationGroup="Activity">
                <Content>
                    <asp:ValidationSummary ID="valConnectorGroup" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Activity" />
                    <asp:HiddenField ID="hfAddConnectionRequestActivityGuid" runat="server" />
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlActivity" runat="server" Label="Activity" Required="true" ValidationGroup="Activity" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlActivityConnector" runat="server" Label="Connector" Required="true" ValidationGroup="Activity" />
                        </div>
                    </div>
                    <Rock:AttributeValuesContainer ID="avcActivityAttributes" runat="server" />
                    <Rock:RockTextBox ID="tbNote" runat="server" Label="Note" TextMode="MultiLine" Rows="4" ValidationGroup="Activity" />
                </Content>
            </Rock:ModalDialog>

            <Rock:ModalDialog ID="dlgSearch" runat="server" ValidationGroup="Search" SaveButtonText="Search" OnSaveClick="dlgSearch_SaveClick" Title="Search Opportunities">
                <Content>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbSearchName" runat="server" Label="Name" />
                            <Rock:RockCheckBoxList ID="cblCampus" runat="server" Label="Campuses" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />
                            <Rock:DynamicPlaceholder ID="phAttributeFilters" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <asp:Repeater ID="rptSearchResult" runat="server">
                                <ItemTemplate>
                                    <Rock:PanelWidget ID="SearchTermPanel" runat="server" CssClass="panel panel-block" TitleIconCssClass='<%# Eval("IconCssClass") %>' Title='<%# Eval("Name") %>'>
                                        <div class="row">
                                            <div class="col-md-4">
                                                <div class="photo">
                                                    <img src='<%# Eval("PhotoUrl") %>'></img>
                                                </div>
                                            </div>
                                            <div class="col-md-8">
                                                <%# Eval("Description") %>
                                                <br />
                                                <Rock:BootstrapButton ID="btnSearchSelect" runat="server" CommandArgument='<%# Eval("Id") %>' CommandName="Display" Text="Select" CssClass="btn btn-default btn-sm" />
                                            </div>
                                        </div>
                                    </Rock:PanelWidget>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                </Content>
            </Rock:ModalDialog>
            <script>
                Sys.Application.add_load(function () {
                    $(".js-transfer-connector").on("click", function (a) {
                        $("#<%=ddlTransferOpportunityConnector.ClientID%>").toggle($(this).is('#<%=rbTransferSelectConnector.ClientID%>'));
                    });

                    $("#<%=ddlTransferOpportunityConnector.ClientID%>").toggle($('#<%=rbTransferSelectConnector.ClientID%>').is(":checked"));
                })
            </script>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
