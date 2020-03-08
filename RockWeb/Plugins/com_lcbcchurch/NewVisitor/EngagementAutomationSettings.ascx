<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EngagementAutomationSettings.ascx.cs" Inherits="RockWeb.Plugins.com_lcbcchurch.NewVisitor.EngagementAutomationSettings" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfScoringItemGuid.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-cogs"></i>Engagement Automation Settings
                </h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <Rock:PanelWidget ID="pwGeneralSettings" runat="server" Title="General" Expanded="true">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlEngagementBeginAttribute" runat="server" Label="Engagement Begin Attribute" Required="true" CssClass="input-width-xl" />
                        </div>
                        <div class="col-md-6">
                            <Rock:NumberBox ID="nbEEngagementWeeks" MinimumValue="1" Label="Weeks in Engagement Window" runat="server" Required="true"/>
                        </div>
                        <div class="col-md-12">
                            <Rock:RockDropDownList ID="ddlEngagementScoreAttribute" runat="server" Label="Engagement Score Attribute" Required="true" CssClass="input-width-xl" />
                        </div>
                    </div>
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gScoringItems" runat="server" ShowActionsInHeader="false">
                            <Columns>
                                <Rock:RockBoundField DataField="Name" HeaderText="Engagement Scoring Items" SortExpression="Name" />
                                <Rock:EditField OnClick="gScoringItems_Edit" />
                                <Rock:DeleteField OnClick="gScoringItems_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </Rock:PanelWidget>
                <div class="actions margin-t-lg">
                    <Rock:BootstrapButton ID="btnSaveConfig" runat="server" CssClass="btn btn-primary" AccessKey="s" ToolTip="Alt+s" OnClick="btnSaveConfig_Click" Text="Save"
                        DataLoadingText="&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Saving"
                        CompletedText="Success" CompletedMessage="<div class='margin-t-md alert alert-success'>Changes have been saved.</div>" CompletedDuration="3"></Rock:BootstrapButton>
                </div>
            </div>
        </div>

        <Rock:ModalDialog ID="mdScoringItem" runat="server" Title="Scoring Item" ValidationGroup="ScoringItem" OnCancelScript="clearActiveDialog();">
            <Content>
                <asp:HiddenField ID="hfScoringItemGuid" runat="server" />
                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RockDropDownList ID="ddlScoringType" AutoPostBack="true" CssClass="input-width-xxl" Label="Scoring Type" runat="server" Required="true" ValidationGroup="ScoringItem" OnSelectedIndexChanged="ddlScoringType_SelectedIndexChanged"></Rock:RockDropDownList>
                        <Rock:NotificationBox ID="nbScoringMessage" runat="server" NotificationBoxType="Info" Visible="true"></Rock:NotificationBox>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbName" runat="server" Label="Name"  Required="true" ValidationGroup="ScoringItem" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbIconCssClass" runat="server" Label="Icon CssClass"  Required="true" ValidationGroup="ScoringItem" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:GroupTypePicker ID="gtpGroupType" UseGuidAsValue="true" runat="server" Visible="false" Label="Group Type" EnhanceForLongLists="true" ValidationGroup="ScoringItem" Required="true" />
                        <Rock:RockDropDownList ID="ddlAttribute" DataTextField="text" DataValueField="value" runat="server" Label="Attribute" Visible="false" Required="true" ValidationGroup="ScoringItem" AutoPostBack="true" OnSelectedIndexChanged="ddlAttribute_SelectedIndexChanged" />
                        <Rock:AccountPicker ID="apAccount" AllowMultiSelect="true" runat="server" Label="Account" Visible="false" Required="true" ValidationGroup="ScoringItem" />
                        <Rock:DataViewItemPicker ID="dvpDataView" runat="server" Label="Data View" Required="true" ValidationGroup="ScoringItem"  />
                    </div>
                    <div class="col-md-6">
                        <Rock:DynamicPlaceholder ID="phAttributes1" runat="server" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12">
                        <Rock:NumberUpDown ID="numPoints" Label="Points" runat="server" CssClass="input-md" ValidationGroup="ScoringItem" Required="true" Minimum="1" />
                    </div>
                </div>
                <Rock:NotificationBox ID="nbError" NotificationBoxType="Warning" runat="server" Visible="false" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
