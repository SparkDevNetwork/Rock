<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CampaignConfiguration.ascx.cs" Inherits="RockWeb.Blocks.Connection.CampaignConfiguration" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info"  Visible="false" />
        <asp:Panel ID="pnlDetail" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-plug"></i>
                    <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">
                <asp:HiddenField ID="hfCampaignConnectionGuid" runat="server" />
                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <h4 class="margin-t-md">Campaign Configuration</h4>
                <span class="text-muted">Default Configuration for the campaign.</span>
                <hr class="margin-t-sm" >
                <div class="row">
                    <div class="col-md-8">
                        <Rock:RockTextBox ID="tbName" runat="server" Label="Campaign Name" Required="true" CssClass="input-width-xxl" />
                    </div>
                    <div class="col-md-4">
                        <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" Checked="true" Text="Yes"/>
                    </div>
                </div>

                <Rock:RockDropDownList ID="ddlConnectionType" runat="server" Label="Connection Type" Required="true" Help="The Connection Type and Opportunity for the campaign." AutoPostBack="true" OnSelectedIndexChanged="ddlConnectionType_SelectedIndexChanged" CssClass="input-width-xxl" />
                <Rock:RockDropDownList ID="ddlConnectionOpportunity" runat="server" Label="Connection Opportunity" Required="true" CssClass="input-width-xxl" />

                <asp:HiddenField ID="hfCampaignShowAdvancedSettings" runat="server" Value="false" />
                <a href="#" class="btn btn-xs btn-link js-campaign-show-advanced-settings" >Show Advanced Settings</a>

                <div class="js-campaign-advanced-settings">
                    <Rock:CodeEditor ID="ceCommentLavaTemplate" runat="server" EditorHeight="200" EditorMode="Lava" EditorTheme="Rock" Label="Request Comments Lava Template" Help="The template will be used to generate the comments for the connection request. It will be passed [Person] and [Family] merge fields for use in the template <span class='tip tip-lava'></span>." />
                </div>

                <h4 class="margin-t-xl">Who Should Be Connected</h4>
                <span class="text-muted">Determines who will be considered for creating connection requests.</span>
                <hr class="margin-t-sm" >
                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataViewItemPicker ID="dvRequestor" runat="server" Label="Requestor Data View" Help="The data view to use to select individuals for connection." Required="true" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockRadioButtonList ID="rblFamilyLimits" runat="server" Label="Family Limits" Required="true" RepeatDirection="Horizontal"
                            Help="Given the list of people in the data view, should we create the connection request for each person or limit to just the head of the house for each family." />
                    </div>
                </div>
                <Rock:GroupPicker ID="gpOptOutGroup" runat="server" Label="Opt Out Group"
                    Help="Your Data View can handle this filtering, but you can also use this group to limit who gets a connection request.
                    Consider adding a manual workflow to your connection request to add people to this group." />


                <h4 class="margin-t-xl">Auto Assignment Settings</h4>
                <span class="text-muted">These optional settings determine if request should be auto assigned.</span>
                <hr class="margin-t-sm" >

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockRadioButtonList ID="rblCreateConnectionRequests" runat="server" Label="Create Connection Requests" Required="true" RepeatDirection="Horizontal" OnSelectedIndexChanged="rblCreateConnectionRequests_SelectedIndexChanged" AutoPostBack="true"
                                Help="Choose whether you want to automatically create all the connection requests at once or only as needed based on the daily limits of assigned connection" />
                    </div>
                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbDailyLimit" runat="server" Label="Daily Limit of Assigned Connection Requests"
                        Help="The number of requests each connector will be assigned each day. This can be overrided by Group Member attributes. Leave blank to not auto assign." CssClass="input-width-md"/>
                    </div>
                </div>

                <h4 class="margin-t-xl">Recurrence Settings</h4>
                <span class="text-muted">The settings determine if connection should be created on periodic basis.</span>
                <hr class="margin-t-sm" >
                <Rock:NumberBox ID="nbNumberOfDays" runat="server" Label="Number of Days Between Connection"
                    Help="The number of days since the last connection request was completed before a new connection request should be created." Required="true" CssClass="input-width-md" />
                <Rock:RockCheckBox ID="cbPreferPreviousConnector" runat="server" Label="Prefer Previous Connector"
                    Help="When creating a new request, should the previous connector be used if they are still a connector." />

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>
        </asp:Panel>
        <script>

            Sys.Application.add_load(function () {

                $('.js-campaign-show-advanced-settings').off('click').on('click', function () {
                    var isVisible = !$('.js-campaign-advanced-settings').is(':visible');
                    $('#<%=hfCampaignShowAdvancedSettings.ClientID %>').val(isVisible);
                    $('.js-campaign-show-advanced-settings').text(isVisible ? 'Hide Advanced Settings' : 'Show Advanced Settings');
                    $('.js-campaign-advanced-settings').slideToggle();
                    return false;
                });

                if ($('#<%=hfCampaignShowAdvancedSettings.ClientID %>').val() == "false") {
                    $('.js-campaign-advanced-settings').hide();
                    $('.js-campaign-show-advanced-settings').text('Show Advanced Settings');
                }
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
