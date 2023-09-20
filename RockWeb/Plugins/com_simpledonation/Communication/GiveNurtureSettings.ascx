<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GiveNurtureSettings.ascx.cs" Inherits="RockWeb.Plugins.com_simpledonation.Communication.GiveNurtureSettings" %>
<style>
    .margin-t-xs {
        margin-top: 3px !important;
    }
</style>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-hand-holding-usd"></i>
                    Give Nurture Settings
                </h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <Rock:PanelWidget ID="pwFirstGift" runat="server" Title="First Gift">

                    <Rock:RockCheckBox ID="cbFirstGift" runat="server"
                        Label="Enable" Text="Email a thank you after first financial gift made."
                        AutoPostBack="true" OnCheckedChanged="cbGiveNurtureEnabled_CheckedChanged" />

                    <hr />

                    <asp:Panel ID="pnlFirstGift" runat="server" Enabled="false" CssClas="data-integrity-options">

                        <div class="clearfix margin-b-lg">
                            <div class="alert alert-info">
                                <strong>Customize this email</strong>
                                <ol>
                                    <li>Add specific ministries and impacts your church makes in the community. The purpose of this email is to connect the donor's gift to community impact.</li>
                                    <li>Choose a real person on staff to the From Name and From Email.</li>
                                </ol>
                            </div>
                            <Rock:RockTextBox ID="tbFirstGiftSubject" runat="server" Label="Message Subject" />
                            <Rock:CodeEditor ID="ceFirstGiftBody" runat="server" EditorMode="Html" EditorTheme="Rock" Rows="15" />

                            <Rock:NumberBox ID="nbFirstGiftMaxRecipients" runat="server" Label="Max Recipients" Help="Limits the number of emails that will be sent per day." />
                        </div>
 
                    </asp:Panel>

                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwSetupRecurring" runat="server" Title="Setup Recurring Giving">

                    <Rock:RockCheckBox ID="cbSetupRecurring" runat="server"
                        Label="Enable" Text="Recurring giving automates people's monthly (or weekly) gift and can be a real service to them. Some regular givers don't know about recurring giving. Some do know about, but haven't taken action yet to set it up. This email goes out once to each regular giver (gave at least once past 3 months) who haven't yet setup recurring giving."
                        AutoPostBack="true" OnCheckedChanged="cbGiveNurtureEnabled_CheckedChanged" />

                    <hr />

                    <asp:Panel ID="pnlSetupRecurring" runat="server" Enabled="false" CssClas="data-integrity-options">

                        <div class="clearfix margin-b-lg">
                            <div class="alert alert-info">
                                <strong>Customize this email</strong>
                                <ol>
                                    <li>Choose a real person on staff to the From Name and From Email.</li>
                                    <li>Add step-by-step instructions on how to setup recurring giving at your church.</li>
                                    <li>Check the link to your giving page that is going to right page.</li>
                                </ol>
                            </div>
                            <Rock:RockTextBox ID="tbSetupRecurringSubject" runat="server" Label="Message Subject" />
                            <Rock:CodeEditor ID="ceSetupRecurringBody" runat="server" Label="Message Body" EditorMode="Html" EditorTheme="Rock" Rows="15" />

                            <Rock:NumberBox ID="nbSetupRecurringMaxRecipients" runat="server" Label="Max Recipients" Help="Limits the number of emails that will be sent per day." />
                        </div>

                    </asp:Panel>

                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwRescueLapsedGivers" runat="server" Title="Rescue Lapsed Givers">

                    <Rock:RockCheckBox ID="cbRescueLapsedGivers" runat="server"
                        Label="Enable" Text="Lapsed givers are people who have stopped giving in the past 8 weeks who normally are giving at this time of year. The email just asks them if they want to meet with a pastor. The perhaps of the meeting is not to talk about their giving, but to connect with them about what's going on in their life. Giving lapses can come from hardship (health, unemployment, divorce), church conflict, and faith crises, all of which are an alarm where a pastor could really help."
                        AutoPostBack="true" OnCheckedChanged="cbGiveNurtureEnabled_CheckedChanged" />

                    <hr />

                    <asp:Panel ID="pnlRescueLapsedGivers" runat="server" Enabled="false" CssClas="data-integrity-options">

                        <div class="clearfix margin-b-lg">
                            <div class="alert alert-info">
                                <strong>Customize this email</strong>
                                <ol>
                                    <li>Choose a real person on staff to the From Name and From Email.</li>
                                    <li>Rewrite the email to the voice of the From Name pastor.</li>
                                </ol>
                            </div>
                            <Rock:RockTextBox ID="tbRescueLapsedGiversSubject" runat="server" Label="Message Subject" />
                            <Rock:CodeEditor ID="ceRescueLapsedGiversBody" runat="server" Label="Message Body" EditorMode="Html" EditorTheme="Rock" Rows="15" />

                            <Rock:NumberBox ID="nbRescueLapsedGiversMaxRecipients" runat="server" Label="Max Recipients" Help="Limits the number of emails that will be sent per day." />
                        </div>

                    </asp:Panel>

                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwNonGiversInGroup" runat="server" Title="Ask non-givers who are in a group to give">

                    <Rock:RockCheckBox ID="cbNonGiversInGroup" runat="server"
                        Label="Enable" Text="New attendees who both 1) find a group, 2) start giving are the most likely to stay Christian and stay at church. This email asks people to start giving."
                        AutoPostBack="true" OnCheckedChanged="cbGiveNurtureEnabled_CheckedChanged" />

                    <hr />

                    <asp:Panel ID="pnlNonGiversInGroup" runat="server" Enabled="false" CssClas="data-integrity-options">

                        <div class="clearfix margin-b-lg">
                            <div class="alert alert-info">
                                <strong>Customize this email</strong>
                                <ol>
                                    <li>Choose a real person on staff to the From Name and From Email.</li>
                                    <li>Rewrite the email into your church's message and voice.</li>
                                </ol>
                            </div>
                            <Rock:RockTextBox ID="tbNonGiversInGroupSubject" runat="server" Label="Message Subject" />
                            <Rock:CodeEditor ID="ceNonGiversInGroupBody" runat="server" Label="Message Body" EditorMode="Html" EditorTheme="Rock" Rows="15" />

                            <Rock:NumberBox ID="nbNonGiversInGroupMaxRecipients" runat="server" Label="Max Recipients" Help="Limits the number of emails that will be sent per day." />
                        </div>

                    </asp:Panel>

                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwNewishGiversNotInGroup" runat="server" Title="Ask new-ish givers, not in a group, to find a group">

                    <Rock:RockCheckBox ID="cbNewishGiversNotInGroup" runat="server"
                        Label="Enable" Text="New attendees who both 1) find a group, 2) start giving are the most likely to stay Christian and stay at church. This email asks new givers to let us help you find a group."
                        AutoPostBack="true" OnCheckedChanged="cbGiveNurtureEnabled_CheckedChanged" />

                    <hr />

                    <asp:Panel ID="pnlNewishGiversNotInGroup" runat="server" Enabled="false" CssClas="data-integrity-options">

                        <div class="clearfix margin-b-lg">
                            <div class="alert alert-info">
                                <strong>Customize this email</strong>
                                <ol>
                                    <li>Choose a real person on staff to the From Name and From Email.</li>
                                    <li>Rewrite the email into your church's message and voice.</li>
                                    <li>Do you have a "Connections, I'm interested" page for people to discover groups, add that link.</li>
                                </ol>
                            </div>
                            <Rock:RockTextBox ID="tbNewishGiversNotInGroupSubject" runat="server" Label="Message Subject" />
                            <Rock:CodeEditor ID="ceNewishGiversNotInGroupBody" runat="server" Label="Message Body" EditorMode="Html" EditorTheme="Rock" Rows="15" />

                            <Rock:NumberBox ID="nbNewishGiversNotInGroupMaxRecipients" runat="server" Label="Max Recipients" Help="Limits the number of emails that will be sent per day." />
                        </div>

                    </asp:Panel>

                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwYearEndAssetGiving" runat="server" Title="Year-end, Asset-giving info. Goes out Nov 10 to ages 55+ who give $3,000+ per year">

                    <Rock:RockCheckBox ID="cbYearEndAssetGiving" runat="server"
                        Label="Enable" Text="Giving stock/investments and retirement account distributions directly to a non-profit can avoid high tax bills. This email will email donors who give at least $3,000/yr and are age 55+ information about it so that they know this exists, and they you will help them."
                        AutoPostBack="true" OnCheckedChanged="cbGiveNurtureEnabled_CheckedChanged" />

                    <hr />

                    <asp:Panel ID="pnlYearEndAssetGiving" runat="server" Enabled="false" CssClas="data-integrity-options">

                        <div class="clearfix margin-b-lg">
                            <div class="alert alert-info">
                                <strong>Customize this email</strong>
                                <ol>
                                    <li>Choose a real person on staff to the From Name and From Email.</li>
                                    <li>Fix that the generic call action is to email the church. If you have instructions for each scenario, include them in the email.</li>
                                    <li>Do you do other asset giving programs like Charitable Remainer Trusts? Then add that to the email.</li>
                                </ol>
                            </div>
                            <Rock:RockTextBox ID="tbYearEndAssetGivingSubject" runat="server" Label="Message Subject" />
                            <Rock:CodeEditor ID="ceYearEndAssetGivingBody" runat="server" Label="Message Body" EditorMode="Html" EditorTheme="Rock" Rows="15" />

                            <Rock:NumberBox ID="nbYearEdndAssetGivingMaxRecipients" runat="server" Label="Max Recipients" Help="Limits the number of emails that will be sent per day." />
                        </div>

                    </asp:Panel>

                </Rock:PanelWidget>

                <div class="actions margin-t-lg">
                    <Rock:BootstrapButton ID="bbtnSaveConfig" runat="server" CssClass="btn btn-primary" AccessKey="s" ToolTip="Alt+s" OnClick="bbtnSaveConfig_Click" Text="Save"
                        DataLoadingText="&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Saving"
                        CompletedText="Success" CompletedMessage="<div class='margin-t-md alert alert-success'>Changes have been saved.</div>" CompletedDuration="3"></Rock:BootstrapButton>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
