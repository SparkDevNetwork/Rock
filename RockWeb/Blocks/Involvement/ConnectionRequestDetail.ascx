<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConnectionRequestDetail.ascx.cs" Inherits="RockWeb.Blocks.Involvement.ConnectionRequestDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfConnectionOpportunityId" runat="server" />
        <asp:HiddenField ID="hfConnectionRequestId" runat="server" />
        <asp:Panel ID="pnlReadDetails" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lConnectionOpportunityIconHtml" runat="server" />
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlCampus" runat="server" LabelType="Campus" Text="Main campus" />
                    <Rock:HighlightLabel ID="hlOpportunity" runat="server" LabelType="Info" Text="Ushers" />
                    <Rock:HighlightLabel ID="hlStatus" runat="server" LabelType="Primary" Text="No Contact" />
                    <Rock:HighlightLabel ID="hlState" runat="server" LabelType="Success" Text="Active" />
                </div>
            </div>
            <div class="panel-body">

                <div class="pull-right">
                    <Rock:HighlightLabel ID="hlFollowUp" runat="server" LabelType="Success" Text="Follow-up: 5/6/2015" />
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <div class="col-md-4">
                            <Rock:ImageEditor ID="imgPhoto" runat="server" BinaryFileTypeGuid="03BD8476-8A9F-4078-B628-5B538F967AFC" />
                        </div>
                        <div class="col-md-8">
                            <Rock:RockLiteral ID="lHomePhone" runat="server" Text="(623) 555-3322   Home" />
                            <Rock:RockLiteral ID="lWorkPhone" runat="server" Text="(623) 555-3322   Work" />
                            <Rock:RockLiteral ID="lCellPhone" runat="server" Text="(623) 555-3322   Cell" />
                            <Rock:RockLiteral ID="lEmail" runat="server" Text="ted@rocksoliddemochurch.com" />
                        </div>
                    </div>
                    <div class="col-md-6">
                        <Rock:RockLiteral ID="lRequestDate" runat="server" Label="Request Date" Text="2/1/2015" />
                        <Rock:RockLiteral ID="lAssignedGroup" runat="server" Label="Assigned Group" Text="Ushers 9:30am" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        +1 Pinterest cardigan gastropub, freegan yr YOLO pop-up pickled authentic. Austin flannel kale chips listicle Shoreditch readymade sriracha, deep v salvia mixtape lo-fi umami kitsch. Blue Bottle hella scenester, leggings Banksy PBR&B kogi wolf fingerstache twee asymmetrical Schlitz farm-to-table. Fashion axe Shoreditch Echo Park, American Apparel cold-pressed yr tousled bicycle rights fap Tumblr Banksy hashtag butcher flannel. Flexitarian YOLO cardigan, Etsy chia fashion axe chambray tilde sriracha gastropub Wes Anderson quinoa. Actually mixtape Kickstarter tousled pickled gluten-free, YOLO kitsch artisan pour-over tilde. Plaid Truffaut taxidermy twee, gluten-free Vice bitters.
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <div class="col-md-6">
                            <asp:Label ID="lblSomething" Text="Available Workflows" Font-Bold="true" runat="server" />
                            <asp:LinkButton ID="btnAvailableWorkflows" runat="server" Text="Send Application" CssClass="btn btn-default btn-xs" />
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>
                    <div class="col-md-6">
                        <asp:Panel ID="pnlRequirements" runat="server">
                            <div class="row">
                                <div class="col-md-6">
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockControlWrapper ID="rcwRequirements" runat="server" Label="Group Requirements">
                                        <Rock:NotificationBox ID="nbRequirementsErrors" runat="server" Dismissable="true" NotificationBoxType="Warning" />
                                        <Rock:RockCheckBoxList ID="cblManualRequirements" RepeatDirection="Vertical" runat="server" Label="" />
                                        <div class="labels">
                                            <asp:Literal ID="lRequirementsLabels" runat="server" />
                                        </div>
                                    </Rock:RockControlWrapper>
                                </div>
                            </div>
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
        <Rock:PanelWidget ID="wpConnectionRequestWorkflow" runat="server" Title="Workflows">
            <div class="grid">
                <Rock:Grid ID="gConnectionRequestWorkflows" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Location">
                    <Columns>
                        <Rock:RockBoundField DataField="WorkflowType" HeaderText="Workflow Type" />
                        <Rock:RockBoundField DataField="Trigger" HeaderText="Trigger" />
                    </Columns>
                </Rock:Grid>
            </div>
        </Rock:PanelWidget>
        <Rock:PanelWidget ID="wpConnectionOpportunityActivities" runat="server" Title="Activity">
            <div class="grid">
                <Rock:Grid ID="gConnectionOpportunityActivities" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Group">
                    <Columns>
                        <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                        <Rock:RockBoundField DataField="Campus" HeaderText="Campus" />
                    </Columns>
                </Rock:Grid>
            </div>
        </Rock:PanelWidget>
        <asp:Panel ID="pnlEditDetails" runat="server" Visible="false" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lConnectionOpportunityIconHtml2" runat="server" />
                    <asp:Literal ID="lReadOnlyTitle2" runat="server" />
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlOpportunityEdit" runat="server" LabelType="info" Text="Ushers" />
                </div>
            </div>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:CustomValidator ID="cvConnectionRequest" runat="server" Display="None" />
                <Rock:NotificationBox ID="nbErrorMessage" runat="server" NotificationBoxType="Danger" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:PersonPicker runat="server" ID="ppConnectionRequestPerson" Label="Person" CssClass="js-authorizedperson" Required="true" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockRadioButtonList ID="rblStatus" runat="server" Label="Status" RepeatDirection="Horizontal" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                    </div>
                    <div class="col-md-6">
                        <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                        <asp:PlaceHolder ID="phAttributesReadOnly" runat="server" EnableViewState="false"></asp:PlaceHolder>
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click"></asp:LinkButton>
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false"></asp:LinkButton>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
