<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckinTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Config.CheckinTypeDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfGroupTypeId" runat="server" />

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
                    
                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" />
                    </div>
                </div>
                <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
                <div class="panel-body">

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div id="pnlEditDetails" runat="server">

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbName" runat="server" Required="true" Label="Name" />
                            </div>
                            <div class="col-md-6">
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-12">
                                <Rock:RockTextBox ID="tbDescription" runat="server" Label="Description" TextMode="MultiLine" Rows="3" />
                            </div>
                        </div>

                        <Rock:PanelWidget ID="wpGeneral" runat="server" Title="General Settings">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlType" runat="server" Label="Check-in Type" AutoPostBack="true" OnSelectedIndexChanged="ddlType_SelectedIndexChanged"
                                        Help="The type of check-in experience to use for this type. Family check-in allows more than one person in the family to be checked in at a time.">
                                        <asp:ListItem Text="Individual" Value="0" />
                                        <asp:ListItem Text="Family" Value="1" />
                                    </Rock:RockDropDownList>
                                    <div class="well">
                                        <Rock:RockControlWrapper ID="rcwSecurityCode" runat="server" Label="Label Security Code Length" 
                                            Help="The number of alpha-numeric, alpha, and/or numeric characters to use when generating a unique security code for labels. Note: Alpha-Numeric characters will be printed first, followed by Alpha characters, then by numeric characters.">
                                            <div class="row">
                                                <div class="col-sm-4 col-xs-12">
                                                    <Rock:NumberBox ID="nbCodeAlphaNumericLength" runat="server" Label="Alpha-Numeric" MinimumValue="0" MaximumValue="10" NumberType="Integer" />
                                                </div>
                                                <div class="col-sm-4 col-xs-6">
                                                    <Rock:NumberBox ID="nbCodeAlphaLength" runat="server" Label="Alpha" MinimumValue="0" MaximumValue="10" NumberType="Integer" />
                                                </div>
                                                <div class="col-sm-4 col-xs-6">
                                                    <Rock:NumberBox ID="nbCodeNumericLength" runat="server" Label="Numeric" MinimumValue="0" MaximumValue="10" NumberType="Integer" />
                                                </div>
                                            </div>
                                            <Rock:RockCheckBox ID="cbCodeRandom" runat="server" Text="Random Numeric Values"
                                                Help="Should the numbers be randomized (vs. generated in order)." />
                                        </Rock:RockControlWrapper>
                                    </div>
                                    <Rock:RockCheckBox ID="cbEnableManager" runat="server" Label="Enable Manager Option" Text="Yes" 
                                        Help="Should an option be displayed on the check-in welcome screen that allows user to view the management screen (after entering a passcode)?" />
                                    <Rock:RockCheckBox ID="cbEnableOverride" runat="server" Label="Enable Override" Text="Yes" 
                                        Help="Should an override button be displayed on the check-in Manager screen that allows a manager to check-in a person and ignore any age and/or grade requirements?" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:NumberBox ID="nbAutoSelectDaysBack" runat="server" Label="Auto Select Days Back" MinimumValue="0" NumberType="Integer" 
                                        Help="The number of days back to look for a previous check-in for each person in the family (or related person). If they have previously checked 
                                        within this number of days, they will automatically be selected during the Family check-in process." />
                                    <Rock:RockCheckBox ID="cbReuseCode" runat="server" Label="Use Same Code for Family" Text="Yes"
                                        Help="Should the same security code be used for each person from the same family that is checking in at the same time?" />
                                    <Rock:RockCheckBox ID="cbUseSameOptions" runat="server" Label="Use Same Service Options" Text="Yes"
                                        Help="If family member(s) is checking into more than one service, should the same options for additional services be automatically selected that were selected for first service?" />
                                    <Rock:RockCheckBox ID="cbHidePhotos" runat="server" Label="Hide Photos" Text="Yes"
                                        Help="Select this option if person photos should not be displayed when selecting the people from the selected family that are checking in." />
                                    <Rock:RockCheckBox ID="cbPreventInactivePeople" runat="server" Label="Prevent Inactive People" Text="Yes" 
                                        Help="Should people who are inactive be excluded from being able to check-in?" />
                                    <Rock:RockCheckBox ID="cbPreventDuplicateCheckin" runat="server" Label="Prevent Duplicate Check-in" Text="Yes" 
                                        Help="Should people be prevented from checking into a specifice service time (schedule) more than once?" />
                                </div>
                            </div>

                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpSearch" runat="server" Title="Search Settings">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockDropDownList ID="ddlSearchType" runat="server" Label="Search Type" Required="true" AutoPostBack="true" OnSelectedIndexChanged="ddlSearchType_SelectedIndexChanged" />
                                    <Rock:NumberBox ID="nbMaxResults" runat="server" Label="Maximum Number of Results"  NumberType="Integer" 
                                        Help="The maximum number of search results to return when searching (default is 100)." />
                                </div>
                                <div class="col-md-6">
                                    <Rock:NumberBox ID="nbMinPhoneLength" runat="server" Label="Minimum Phone Number Length" NumberType="Integer" MinimumValue="1" MaximumValue="10"
                                        Help="The minimum number of digits that needs to be entered for a phone number search (default is 4)." />
                                    <Rock:NumberBox ID="nbMaxPhoneLength" runat="server" Label="Maximum Phone Number Length" NumberType="Integer" MinimumValue="1" MaximumValue="10"
                                        Help="The maximum number of digits that can to be entered for a phone number search (default is 10)." />
                                    <Rock:RockDropDownList ID="ddlPhoneSearchType" runat="server" Label="Phone Search Type" 
                                        Help="Controls how a person's phone number should be compared to the digits that were entered by person when checking in.">
                                        <asp:ListItem Text="Contains" Value="0" />
                                        <asp:ListItem Text="Ends With" Value="1" />
                                    </Rock:RockDropDownList>
                                </div>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpAdvanced" runat="server" Title="Advanced Settings">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbSearchRegex" runat="server" Label="Regular Expression Filter" 
                                        Help="An optional regular expression that will be run against any search input before the search is performed. This is useful for removing any special characters." />
                                    <Rock:NumberBox ID="nbRefreshInterval" runat="server" Label="Refresh Interval" NumberType="Integer" 
                                        Help="How often (seconds) should the welcome page automatically refresh and check for updated configuration information." />
                                </div>
                                <div class="col-md-6">
                                    <Rock:RockCheckBox ID="cbAgeRequired" runat="server" Label="Age is Required" Text="Yes" 
                                        Help="If an area and/or group has an age requirement, should people without an age be allowed to check-in to that area/group?" />
                                    <Rock:RockCheckBox ID="cbGradeRequired" runat="server" Label="Grade is Required" Text="Yes" 
                                        Help="If an area and/or group has a grade requirement, should people without an grade be allowed to check-in to that area/group?" />
                                    <Rock:RockCheckBox ID="cbDisplayLocCount" runat="server" Label="Display Location Count" Text="Yes" 
                                        Help="Should the room locations options include a count of how many people are currently checked into that location?" />
                                </div>
                            </div>
                        </Rock:PanelWidget>

                        <Rock:PanelWidget ID="wpCustom" runat="server" Title="Custom Settings">
                            <asp:PlaceHolder ID="phAttributeEdits" runat="server" EnableViewState="false"></asp:PlaceHolder>
                        </Rock:PanelWidget>

                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>

                    </div>

                    <fieldset id="fieldsetViewDetails" runat="server">

                        <div class="row margin-b-md">
                            <div class="col-md-12">
                                <asp:Literal ID="lDescription" runat="server" />
                            </div>
                        </div>

                        <asp:Literal ID="lblMainDetails" runat="server" />
                        <div class="row ">
                            <div class="col-sm-6">
                                <asp:Literal ID="lblLeftDetails" runat="server" />
                            </div>
                            <div class="col-sm-6">
                                <asp:Literal ID="lblRightDetails" runat="server" />
                            </div>
                        </div>

                        <div class="actions">
                            <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                            <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                            <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                            <div class="pull-right">
                                <asp:LinkButton ID="btnSchedules" runat="server" CssClass="btn btn-default" OnClick="btnSchedules_Click"><i class="fa fa-calendar"></i> Schedule</asp:LinkButton>
                            </div>
                        </div>

                    </fieldset>

                </div>
            </div>


            
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>