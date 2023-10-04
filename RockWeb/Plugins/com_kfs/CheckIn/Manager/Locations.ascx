<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Locations.ascx.cs" Inherits="RockWeb.Plugins.com_kfs.CheckIn.Manager.Locations" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('.js-cancel-checkin').click(function (event) {
            event.stopImmediatePropagation();
            var personName = $(this).closest(".list-group-item").find(".js-checkin-person-name").first().text();
            return Rock.dialogs.confirmDelete(event, 'Checkin for ' + personName);
        });
        $('.js-checkout').click(function (event) {
            event.stopImmediatePropagation();
            var personName = $(this).closest(".list-group-item").find(".js-checkin-person-name").first().text();
            return Rock.dialogs.confirmDelete(event, 'Checkin and Check Out: ' + personName);
        });
    });

    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
        $('#<%=hfPersonId.ClientID %>').val('');
        $('#<%=hfLocationId.ClientID %>').val('');
        $('#<%=hfGroupIds.ClientID %>').val('');
    }

    function isEdge() {
        var hiddenControl = '<%=hfIsEdge.ClientID %>';
        if (window.navigator.userAgent.indexOf("Edge") > -1) {
            document.getElementById(hiddenControl).value = 'true';
        }
    }
</script>
<Rock:RockUpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" Dismissable="true" />

        <asp:Panel ID="pnlContent" runat="server" CssClass="checkin-manager">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-sitemap"></i>&nbsp;<asp:Literal ID="lGroupTypeName" runat="server" /></h1>
            </div>
            <br />

            <asp:HiddenField ID="hfChartData" runat="server" />
            <asp:HiddenField ID="hfChartOptions" runat="server" />
            <asp:Panel ID="pnlChart" runat="server" Style="width: 100%; height: 170px" CssClass="clickable" />

            <br />

            <div class="input-group">
                <Rock:RockTextBox ID="tbSearch" runat="server" Placeholder="Person Search..." />
                <span class="input-group-btn">
                    <asp:LinkButton ID="lbSearch" runat="server" CssClass="btn btn-default" OnClick="lbSearch_Click"><i class="fa fa-search"></i> Search</asp:LinkButton>
                </span>
            </div>

            <br />

            <div class="panel panel-default">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <asp:Panel ID="pnlNavHeading" runat="server" CssClass="panel-heading clickable clearfix">
                    <asp:PlaceHolder runat="server">
                        <div class="margin-t-sm pull-left">
                            <i class="fa fa-chevron-left"></i>
                            <asp:Literal ID="lNavHeading" runat="server" />
                        </div>
                        <div class="pull-right margin-v-sm">
                            <Rock:Toggle ID="tglHeadingRoom" runat="server" OnText="Open" OffText="Close" ButtonSizeCssClass="btn-xs" OnCssClass="btn-success" OffCssClass="btn-danger" OnCheckedChanged="tglRoom_CheckedChanged" />
                        </div>
                        <div class="pull-right margin-v-sm">
                            <asp:LinkButton CssClass="btn-sm btn-info" ID="lbMoveAll" runat="server" OnClick="lbMoveAll_Click"><i class="fa fa-sign-out"></i> Move All</asp:LinkButton>&nbsp;&nbsp;
                        </div>
                        <asp:Panel ID="pnlThreshold" runat="server" CssClass="pull-right margin-r-md margin-t-sm js-threshold paneleditor">
                            <span class="paneleditor-label">Threshold:</span>
                            <Rock:HiddenFieldWithClass ID="hfThreshold" runat="server" CssClass="js-threshold-hf" />
                            <asp:Label ID="lThreshold" runat="server" CssClass="js-threshold-view js-threshold-l" />
                            <a class="btn btn-default btn-xs js-threshold-view js-threshold-btn-edit"><i class="fa fa-edit"></i></a>
                            <Rock:NumberBox ID="nbThreshold" runat="server" CssClass="input-width-xs js-threshold-edit js-threshold-nb paneleditor-input" NumberType="Integer" Style="display: none"></Rock:NumberBox>
                            <asp:LinkButton ID="lbUpdateThreshold" runat="server" CssClass="btn btn-success btn-xs js-threshold-edit js-threshold-btn-save paneleditor-button" OnClick="lbUpdateThreshold_Click" Style="display: none"><i class="fa fa-check"></i></asp:LinkButton>
                            <a class="btn btn-warning btn-xs js-threshold-edit js-threshold-btn-cancel paneleditor-button" style="display: none"><i class="fa fa-ban"></i></a>
                        </asp:Panel>
                    </asp:PlaceHolder>
                </asp:Panel>

                <ul class="list-group">

                    <asp:Repeater ID="rptNavItems" runat="server">
                        <ItemTemplate>
                            <li id="liNavItem" runat="server" class="list-group-item clickable">
                                <div class="content margin-v-sm"><%# Eval("Name") %></div>
                                <div class="pull-right margin-v-sm">
                                    <asp:Label ID="lblCurrentCount" runat="server" CssClass="badge" />
                                    &nbsp;&nbsp;

                                    <Rock:Toggle ID="tglRoom" runat="server" OnText="Open" OffText="Close" ButtonSizeCssClass="btn-xs" OnCssClass="btn-success" OffCssClass="btn-danger" OnCheckedChanged="tglRoom_CheckedChanged" />
                                    <i class='fa fa-fw fa-chevron-right'></i>
                                </div>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:Repeater ID="rptPeople" runat="server" OnItemCommand="rptPeople_ItemCommand">
                        <ItemTemplate>
                            <li id="liNavItem" runat="server" class="list-group-item clickable clearfix">
                                <div class="photoframe pull-left margin-r-md">
                                    <asp:Literal ID="imgPerson" runat="server" /></div>
                                <div class="pull-left margin-t-sm">
                                    <span class="js-checkin-person-name"><%# Eval("Name") %></span><asp:Literal ID="lAge" runat="server" />
                                    <%# Eval("ScheduleGroupNames") %>
                                </div>
                                <span class="pull-right margin-t-sm">
                                    <asp:Literal ID="lStatus" runat="server" />
                                    <asp:LinkButton ID="lbPrintLabel" runat="server" CssClass="js-pring-label btn btn-success"
                                        CommandName="PrintLabel" Visible='<%# (bool)Eval("ShowPrintLabel") %>' OnClientClick="isEdge();"><i class="fa fa-print"></i> View Labels</asp:LinkButton>
                                    <asp:LinkButton ID="lbMovePerson" runat="server" CssClass="js-move-person btn btn-info"
                                        CommandArgument='<%# Eval("Id") %>' CommandName="Move" Visible='<%# (bool)Eval("ShowMove") %>'><i class="fa fa-sign-out"></i> Move</asp:LinkButton>
                                    <asp:LinkButton ID="lbCheckOut" runat="server" CssClass="js-checkout btn btn-warning"
                                        CommandArgument='<%# Eval("Id") %>' CommandName="Checkout" Visible='<%# (bool)Eval("ShowCheckout") %>'><i class="fa fa-minus-circle"></i> Checkout</asp:LinkButton>
                                    <asp:LinkButton ID="lbRemoveAttendance" runat="server" CssClass="js-cancel-checkin btn btn-xs btn-danger"
                                        CommandArgument='<%# Eval("Id") %>' CommandName="Delete" Visible='<%# (bool)Eval("ShowCancel") %>'><i class="fa fa-times"></i></asp:LinkButton>
                                </span>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>
            </div>
        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />
        <asp:HiddenField ID="hfPersonId" runat="server" />
        <asp:HiddenField ID="hfLocationId" runat="server" />
        <asp:HiddenField ID="hfGroupIds" runat="server" />
        <asp:HiddenField ID="hfIsEdge" runat="server" />

        <Rock:ModalDialog ID="dlgMoveLocation" runat="server" Title="Select Move Location" OnSaveClick="dlgMoveLocation_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="MoveLocation">
            <Content>
                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="MoveLocation" />

                <Rock:LocationPicker ID="lpNewLocation" runat="server" ValidationGroup="MoveLocation" Required="true" Label="Selected Location" AllowedPickerModes="Named" />

                <Rock:RockDropDownList ID="rdlNewGroup" runat="server" ValidationGroup="MoveLocation" Label="Selected Group" Visible="false" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgPrintLabel" runat="server" Title="Print Label" OnCancelScript="clearActiveDialog();" ValidationGroup="PrintLabel">
            <Content>
                <div class="margin-t-md">
                    <div class="form-inline">
                        <Rock:RockDropDownList ID="ddlLabelToPrint" runat="server" Label="Select Label" AppendDataBoundItems="false" ValidationGroup="PrintLabel" OnSelectedIndexChanged="btnViewLabel_Click" OnClientClick="isEdge();" AutoPostBack="true"></Rock:RockDropDownList>
                        <Rock:RockControlWrapper ID="rcwPrint" runat="server" Label=" " FormGroupCssClass="margin-l-md">
                            <asp:LinkButton ID="lbPrintLabel" runat="server" Text="Print Label" CssClass="btn btn-default margin-l-lg" OnClick="btnPrintLabel_Click" ValidationGroup="PrintLabel" />
                        </Rock:RockControlWrapper>
                        <asp:Panel runat="server" id="pnlEdgeAlert" Visible="false" class="alert alert-warning">For Microsoft Edge, right-click the label and select Print.</asp:Panel>
                    </div>
                </div>
                <br />
                <Rock:PanelWidget ID="wpAdvancedPrintOptions" runat="server" Title="Advanced Print Options" Visible="false">
                    <div class="panel-body form-inline">
                        <Rock:RockDropDownList ID="ddlPrintDensity" runat="server" Label="Print Density">
                            <asp:ListItem Text="6 dpmm (152 dpi)" Value="6"></asp:ListItem>
                            <asp:ListItem Text="8 dpmm (203 dpi)" Value="8"></asp:ListItem>
                            <asp:ListItem Text="12 dpmm (300 dpi)" Value="12"></asp:ListItem>
                            <asp:ListItem Text="24 dpmm (600 dpi)" Value="24"></asp:ListItem>
                        </Rock:RockDropDownList>
                        <Rock:RockControlWrapper ID="rcwLabelSize" runat="server" Label="Label Size" FormGroupCssClass="margin-l-md">
                            <Rock:NumberBox ID="nbLabelWidth" runat="server" CssClass="input-width-xs" NumberType="Double"></Rock:NumberBox>
                            X

                            <Rock:NumberBox ID="nbLabelHeight" runat="server" CssClass="input-width-xs" NumberType="Double"></Rock:NumberBox>
                        </Rock:RockControlWrapper>
                        <Rock:RockControlWrapper ID="rcwShowLabel" runat="server" Label="Show Label" Help="(0 = first label, 1 = second label, etc.)" FormGroupCssClass="margin-l-md">
                            <Rock:NumberBox ID="nbShowLabel" runat="server" CssClass="input-width-xs" Text="0" NumberType="Integer"></Rock:NumberBox>
                        </Rock:RockControlWrapper>
                    </div>
                </Rock:PanelWidget>
                <asp:Literal ID="litLabel" runat="server"></asp:Literal>
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</Rock:RockUpdatePanel>
