<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DataIntegritySettings.ascx.cs" Inherits="RockWeb.Blocks.Administration.DataIntegritySettings" %>

<style>
    table.select-option {
        width: 100%;
    }

        table.select-option td:first-child {
            width: 25px;
            min-width: 25px;
            vertical-align: top;
        }
</style>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-tachometer"></i>
                    Data Integrity Settings
                </h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <Rock:PanelWidget ID="pwGeneralSettings" runat="server" Title="General Settings">
                    <Rock:NumberBox ID="nbGenderAutoFill" runat="server" AppendText="%" CssClass="input-width-md" Label="Gender AutoFill Confidence" MinimumValue="0" MaximumValue="100" NumberType="Integer" />
                </Rock:PanelWidget>

                   <Rock:PanelWidget ID="pwBootstrapButtonConfiguration" runat="server" Title="Bootstrap Button Configuration">
                    <div class="row">
                        <div class="col-md-4">
                            <Rock:RockTextBox ID="tbDataLoadingText" runat="server"  Label="Data Loading Text" />
                        </div>
                        <div class="col-md-4">
                            <Rock:RockTextBox ID="tbCompleteText" runat="server"  Label="Complete Text" />
                        </div>
                        <div class="col-md-4">
                            <Rock:NumberBox ID="nbCompletedTimeout" runat="server" AppendText="milliseconds" CssClass="input-width-lg" Label="Completed Timeout" NumberType="Integer" />
                        </div>
                    </div>
                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwNcoaConfiguration" runat="server" Title="NCOA Configuration">
                    <div class="row">
                        <div class="col-md-4">
                            <Rock:NumberBox ID="nbMinMoveDistance" runat="server" AppendText="miles" CssClass="input-width-md" Label="Minimum Move Distance to Inactivate" NumberType="Double" Text="250" />
                        </div>
                        <div class="col-md-4">
                            <Rock:RockCheckBox ID="cb48MonAsPrevious" runat="server" Label="Mark 48 Month Move as Previous Addresses" />
                        </div>
                        <div class="col-md-4">
                            <Rock:RockCheckBox ID="cbInvalidAddressAsPrevious" runat="server" Label="Mark Invalid Addresses as Previous Addresses" />
                        </div>
                    </div>
                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwDataAutomation" runat="server" Title="Data Automation">

                    <section class="panel panel-widget rock-panel-widget">
                        <header class="panel-heading clearfix">
                            <table class="select-option">
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbReactivatePeople" runat="server" SelectedIconCssClass="fa fa-lg fa-check-square-o" UnSelectedIconCssClass="fa fa-lg fa-square-o" /></td>
                                    <td>
                                        <strong>Reactivate People</strong><br />
                                        Looks for recent activity on the family and will reactivate all individuals if any are met.
                                        Individuals with inactive reasons marked not to reactivate( e.g.Deceased) will not be changed.
                                    </td>
                                </tr>
                            </table>
                        </header>
                        <div class="panel-body">
                            <table class="select-option">
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbLastContribution" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" /></td>
                                    <td>
                                        <Rock:NumberBox ID="nbLastContribution" runat="server" Label="Has contribution in the last" AppendText="days" CssClass="input-width-md" Text="90" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbAttendanceInServiceGroup" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" /></td>
                                    <td>
                                        <Rock:NumberBox ID="nbAttendanceInServiceGroup" runat="server" Label="Has attendance in a group that is considered a service in the last" AppendText="days" CssClass="input-width-md" Text="90" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbAttendanceInGroupType" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" /></td>
                                    <td>
                                        <Rock:RockControlWrapper ID="rcwAttendanceInGroupType" runat="server" Label="Has attendance in group of type in the last">
                                            <div class="row">
                                                <div class="col-xs-12 col-sm-6 col-md-4">
                                                    <Rock:RockListBox ID="rlbAttendanceInGroupType" runat="server" DataTextField="text" DataValueField="value" />
                                                </div>
                                                <div class="col-xs-12 col-sm-6 col-md-4">
                                                    <Rock:NumberBox ID="nbAttendanceInGroupType" runat="server" AppendText="days" CssClass="input-width-md" Text="90" />
                                                </div>
                                            </div>
                                        </Rock:RockControlWrapper>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbPrayerRequest" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" /></td>
                                    <td>
                                        <Rock:NumberBox ID="nbPrayerRequest" runat="server" Label="Has prayer request in the last" AppendText="days" CssClass="input-width-md" Text="90" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbPersonAttributes" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" /></td>
                                    <td>
                                        <Rock:RockControlWrapper ID="rcwPersonAttributes" runat="server" Label="Has new values in the following person attributes in the last">
                                            <div class="row">
                                                <div class="col-xs-12 col-sm-6 col-md-4">
                                                    <Rock:RockListBox ID="rlbPersonAttributes" runat="server" DataTextField="text" DataValueField="value">
                                                    </Rock:RockListBox>
                                                </div>
                                                <div class="col-xs-12 col-sm-6 col-md-4">
                                                    <Rock:NumberBox ID="nbPersonAttributes" runat="server" AppendText="days" CssClass="input-width-md" Text="90" />
                                                </div>
                                            </div>
                                        </Rock:RockControlWrapper>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbIncludeDataView" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" /></td>
                                    <td>
                                        <Rock:DataViewPicker ID="dvIncludeDataView" runat="server" Label="Include those in the following data view" CssClass="input-width-xl" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbExcludeDataView" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" /></td>
                                    <td>
                                        <Rock:DataViewPicker ID="dvExcludeDataView" runat="server" Label="Exclude those in the following data view" CssClass="input-width-xl" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbInteractions" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" /></td>
                                    <td>
                                        <Rock:RockControlWrapper ID="rcwInteractions" runat="server" Label="Has a interaction of the following type in the last">
                                            <asp:Repeater ID="rInteractions" runat="server">
                                                <ItemTemplate>
                                                    <div class="row margin-b-sm">
                                                        <asp:HiddenField ID="hfInteractionTypeId" runat="server" Value='<%# Eval("Guid") %>' />
                                                        <div class="col-md-2 col-sm-3 col-xs-4">
                                                            <Rock:RockCheckBox ID="cbInterationType" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" Text='<%# Eval("Name") %>' Checked='<%# (bool)Eval("IsInteractionTypeEnabled") %>' />
                                                        </div>
                                                        <div class="col-md-10 col-sm-9 col-xs-6">
                                                            <Rock:NumberBox ID="nbInteractionDays" runat="server" AppendText="days" CssClass="input-width-md" Text='<%#Eval("LastInteractionDays") %>' />
                                                        </div>
                                                    </div>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </Rock:RockControlWrapper>
                                    </td>
                                </tr>

                            </table>
                        </div>
                    </section>

                    <section class="panel panel-widget rock-panel-widget">
                        <header class="panel-heading clearfix">
                            <table class="select-option">
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbInactivatePeople" runat="server" SelectedIconCssClass="fa fa-lg fa-check-square-o" UnSelectedIconCssClass="fa fa-lg fa-square-o" /></td>
                                    <td>
                                        <strong>Inactivate People</strong><br />
                                        Looks for recent activity on the family and will inactivate all individuals if all of the selected criteria below are met.
                                    </td>
                                </tr>
                            </table>
                        </header>
                        <div class="panel-body">
                            <table class="select-option">
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbNoLastContribution" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" /></td>
                                    <td>
                                        <Rock:NumberBox ID="nbNoLastContribution" runat="server" Label="Has no contribution in the last" AppendText="days" CssClass="input-width-md" Text="500" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbNoAttendanceInServiceGroup" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" /></td>
                                    <td>
                                        <Rock:NumberBox ID="nbNoAttendanceInServiceGroup" runat="server" Label="Has no attendance in a group that is considered a service in the last" AppendText="days" CssClass="input-width-md" Text="500" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbNoAttendanceInGroupType" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" /></td>
                                    <td>
                                        <Rock:RockControlWrapper ID="rcwNoAttendanceInGroupType" runat="server" Label="Has no attendance in group of type in the last">
                                            <div class="row">
                                                <div class="col-xs-12 col-sm-6 col-md-4">
                                                    <Rock:RockListBox ID="rlbNoAttendanceInGroupType" runat="server" DataTextField="text" DataValueField="value" />
                                                </div>
                                                <div class="col-xs-12 col-sm-6 col-md-4">
                                                    <Rock:NumberBox ID="nbNoAttendanceInGroupType" runat="server" AppendText="days" CssClass="input-width-md" Text="500" />
                                                </div>
                                            </div>
                                        </Rock:RockControlWrapper>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbNoPrayerRequest" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" /></td>
                                    <td>
                                        <Rock:NumberBox ID="nbNoPrayerRequest" runat="server" Label="Has no prayer request in the last" AppendText="days" CssClass="input-width-md" Text="500" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbNoPersonAttributes" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" /></td>
                                    <td>
                                        <Rock:RockControlWrapper ID="rcwNoPersonAttributes" runat="server" Label="Has no new values in the following person attributes in the last">
                                            <div class="row">
                                                <div class="col-xs-12 col-sm-6 col-md-4">
                                                    <Rock:RockListBox ID="rlbNoPersonAttributes" runat="server" DataTextField="text" DataValueField="value">
                                                    </Rock:RockListBox>
                                                </div>
                                                <div class="col-xs-12 col-sm-6 col-md-4">
                                                    <Rock:NumberBox ID="nbNoPersonAttributes" runat="server" AppendText="days" CssClass="input-width-md" Text="500" />
                                                </div>
                                            </div>
                                        </Rock:RockControlWrapper>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbNotInDataView" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" /></td>
                                    <td>
                                        <Rock:DataViewPicker ID="dvNotInDataView" runat="server" Label="Not in the following data view" CssClass="input-width-xl" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbNoInteractions" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" /></td>
                                    <td>
                                        <Rock:RockControlWrapper ID="rcwNoInteractions" runat="server" Label="Does not have a interaction of the following type in the last">
                                            <asp:Repeater ID="rNoInteractions" runat="server">
                                                <ItemTemplate>
                                                    <div class="row margin-b-sm">
                                                        <asp:HiddenField ID="hfInteractionTypeId" runat="server" Value='<%# Eval("Guid") %>' />
                                                        <div class="col-md-2 col-sm-3 col-xs-4">
                                                            <Rock:RockCheckBox ID="cbInterationType" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" Text='   <%# Eval("Name") %>' Checked='<%# (bool)Eval("IsInteractionTypeEnabled") %>' />
                                                        </div>
                                                        <div class="col-md-10 col-sm-9 col-xs-8">
                                                            <Rock:NumberBox ID="nbNoInteractionDays" runat="server" AppendText="days" CssClass="input-width-md" Text='<%#Eval("LastInteractionDays") %>' />
                                                        </div>
                                                    </div>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </Rock:RockControlWrapper>
                                    </td>
                                </tr>

                            </table>
                        </div>
                    </section>

                    <section class="panel panel-widget rock-panel-widget">
                        <header class="panel-heading clearfix">
                            <table class="select-option">
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbCampusUpdate" runat="server" SelectedIconCssClass="fa fa-lg fa-check-square-o" UnSelectedIconCssClass="fa fa-lg fa-square-o" /></td>
                                    <td>
                                        <strong>Update Campus</strong><br />
                                        Looks for recent attendance and giving for the family and will update the family's campus if their attendance or giving is for a different campus than they are currently associated with.
                                    </td>
                                </tr>
                            </table>
                        </header>
                        <div class="panel-body">
                            <table class="select-option">
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbMostFamilyAttendance" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" /></td>
                                    <td>
                                        <Rock:NumberBox ID="nbMostFamilyAttendance" runat="server" Label="Set campus based on the most family attendance to a campus-specified location in the last" AppendText="days" CssClass="input-width-md" Text="90" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbMostFamilyGiving" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" /></td>
                                    <td>
                                        <Rock:NumberBox ID="nbMostFamilyGiving" runat="server" Label="Set campus based on the most family giving to a campus-specified account in the last" AppendText="days" CssClass="input-width-md" Text="90" />
                                    </td>
                                </tr>
                                <tr>
                                    <td></td>
                                    <td>
                                        <Rock:RockDropDownList ID="ddlAttendanceOrGiving" CssClass="input-width-md" runat="server" Label="If the campus for most attendance and the campus for most giving are different" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbIgnoreIfManualUpdate" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" /></td>
                                    <td>
                                        <Rock:NumberBox ID="nbIgnoreIfManualUpdate" runat="server" Label="Ignore if anyone in family has a manual campus update in the last" AppendText="days" CssClass="input-width-md" Text="90" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <Rock:RockCheckBox ID="cbIgnoreCampusChanges" runat="server" SelectedIconCssClass="fa fa-check-square-o" UnSelectedIconCssClass="fa fa-square-o" /></td>
                                    <td>
                                        <Rock:RockControlWrapper ID="rcwIgnoreCampusChanges" runat="server" Label="Ignore any campus changes">
                                            <asp:Repeater ID="rIgnoreCampusChanges" runat="server" OnItemDataBound="rIgnoreCampusChanges_ItemDataBound" OnItemCommand="rIgnoreCampusChanges_ItemCommand">
                                                <ItemTemplate>
                                                    <asp:HiddenField ID="hfRowId" runat="server" Value='<%# Eval("Id") %>' />
                                                    <div class="row margin-l-sm margin-b-sm form-inline">
                                                        <Rock:CampusPicker ID="cpFromCampus" runat="server" Label="From" CssClass="margin-r-sm" />
                                                        <Rock:CampusPicker ID="cpToCampus" runat="server" Label="To" CssClass="margin-r-sm" />
                                                        <Rock:RockDropDownList ID="ddlAttendanceOrGiving" runat="server" Label="Based On" CssClass="margin-r-sm"/>
                                                        <asp:LinkButton ID="lbDelete" runat="server" CssClass="btn btn-xs btn-danger form-action-remove" CommandName="delete" CommandArgument='<%# Eval("Id") %>'><i class="fa fa-minus-circle"></i></asp:LinkButton>
                                                    </div>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                            <asp:LinkButton ID="lbAdd" CssClass="btn btn-xs btn-action margin-l-sm" runat="server" OnClick="lbAdd_Click">
                                                <i class="fa fa-plus-circle"></i>
                                            </asp:LinkButton>
                                        </Rock:RockControlWrapper>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </section>

                </Rock:PanelWidget>

                <div class="actions margin-t-lg">
                    <Rock:BootstrapButton ID="bbtnSaveConfig" runat="server" CssClass="btn btn-primary" AccessKey="s" ToolTip="Alt+s" OnClick="bbtnSaveConfig_Click" Text="Save" 
                        DataLoadingText="&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Saving" 
                        CompletedText="Success" CompletedMessage="&nbsp;Changes Have Been Saved!" CompletedDuration="2"></Rock:BootstrapButton>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
