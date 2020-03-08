<%@ Control Language="C#" AutoEventWireup="true" CodeFile="OnboardingDashboard.ascx.cs" Inherits="RockWeb.Plugins.com_lcbcchurch.NewVisitor.OnboardingDashboard" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fas fa-hands-heart"></i> 16 Week Onboarding Dashboard</h1>
                <div class="pull-right">
                    <Rock:RockDropDownList ID="ddlCampus" runat="server" Visible="false" AutoPostBack="true" OnSelectedIndexChanged="ddlCampus_SelectionChanged" DataTextField="Name" DataValueField="Id" />
                </div>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" Visible="false" />
                <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfFilter" runat="server">
                        <Rock:NumberBox ID="nbWeek" runat="server" CssClass="input-width-md" Label="Week" MinimumValue="0" MaximumValue="16" NumberType="Integer" />
                        <Rock:NumberBox ID="nbEngagementScore" runat="server" CssClass="input-width-md" Label="Engagement Score" MinimumValue="0" NumberType="Integer" />
                        <Rock:RockCheckBoxList ID="cblFirstSteps" runat="server" Label="First Steps Class" RepeatDirection="Horizontal">
                            <asp:ListItem Text="Yes" Value="1" />
                            <asp:ListItem Text="No" Value="0" />
                        </Rock:RockCheckBoxList>
                        <Rock:RockCheckBoxList ID="cblWelcomeEmail" runat="server" Label="Welcome Email" RepeatDirection="Horizontal">
                            <asp:ListItem Text="Yes" Value="1" />
                            <asp:ListItem Text="No" Value="0" />
                        </Rock:RockCheckBoxList>
                        <Rock:RockCheckBoxList ID="cblWelcomeLetter" runat="server" Label="Welcome Letter" RepeatDirection="Horizontal">
                            <asp:ListItem Text="Yes" Value="1" />
                            <asp:ListItem Text="No" Value="0" />
                        </Rock:RockCheckBoxList>
                        <Rock:RockCheckBoxList ID="cblCokieDrop" runat="server" Label="Cookie Drop" RepeatDirection="Horizontal">
                            <asp:ListItem Text="Yes" Value="1" />
                            <asp:ListItem Text="No" Value="0" />
                        </Rock:RockCheckBoxList>
                        <Rock:RockCheckBoxList ID="cblNoReturnCard" runat="server" Label="No Return Card" RepeatDirection="Horizontal">
                            <asp:ListItem Text="Yes" Value="1" />
                            <asp:ListItem Text="No" Value="0" />
                        </Rock:RockCheckBoxList>
                        <Rock:RockCheckBoxList ID="cblServingCard" runat="server" Label="Serving Card" RepeatDirection="Horizontal">
                            <asp:ListItem Text="Yes" Value="1" />
                            <asp:ListItem Text="No" Value="0" />
                        </Rock:RockCheckBoxList>
                        <Rock:RockCheckBoxList ID="cblSMSPersonal" runat="server" Label="SMS Personal Connection" RepeatDirection="Horizontal">
                            <asp:ListItem Text="Yes" Value="1" />
                            <asp:ListItem Text="No" Value="0" />
                        </Rock:RockCheckBoxList>
                        <Rock:RockCheckBoxList ID="cblEmailPersonal" runat="server" Label="Email Personal Connection" RepeatDirection="Horizontal">
                            <asp:ListItem Text="Yes" Value="1" />
                            <asp:ListItem Text="No" Value="0" />
                        </Rock:RockCheckBoxList>
                        <Rock:RockCheckBoxList ID="cblFaceToFace" runat="server" Label="Face-To-Face Personal Connection" RepeatDirection="Horizontal">
                            <asp:ListItem Text="Yes" Value="1" />
                            <asp:ListItem Text="No" Value="0" />
                        </Rock:RockCheckBoxList>
                        <Rock:RockCheckBoxList ID="cblPhoneCall" runat="server" Label="Phone Call Personal Connection" RepeatDirection="Horizontal">
                            <asp:ListItem Text="Yes" Value="1" />
                            <asp:ListItem Text="No" Value="0" />
                        </Rock:RockCheckBoxList>
                        <Rock:RockCheckBoxList ID="cblMailedPersonalNote" runat="server" Label="Mailed Personal Note Personal Connection" RepeatDirection="Horizontal">
                            <asp:ListItem Text="Yes" Value="1" />
                            <asp:ListItem Text="No" Value="0" />
                        </Rock:RockCheckBoxList>
                        <Rock:RockCheckBoxList ID="cblTouchpointConversation" runat="server" Label="Touchpoint Conversation Personal Connection" RepeatDirection="Horizontal">
                            <asp:ListItem Text="Yes" Value="1" />
                            <asp:ListItem Text="No" Value="0" />
                        </Rock:RockCheckBoxList>
                    </Rock:GridFilter>
                    <Rock:Grid ID="gPersons" runat="server" CssClass="js-grid-requests" AllowSorting="true" OnRowDataBound="gPersons_RowDataBound" ExportSource="ColumnOutput">
                        <Columns>
                            <Rock:SelectField />
                            <Rock:RockTemplateField HeaderText="Week" ExcelExportBehavior="NeverInclude" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" SortExpression="Week">
                                <ItemTemplate>
                                    <%# GetWeekColumnHtml((int)Eval("Week")) %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="Week" HeaderText="Week" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                            <asp:HyperLinkField DataTextField="FullName" DataNavigateUrlFields="Id" SortExpression="Person.LastName,Person.NickName" DataNavigateUrlFormatString="~/Person/{0}" HeaderText="Name" />
                            <Rock:RockBoundField DataField="Attendance" HeaderText="Attended" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                            <Rock:RockBoundField DataField="AttendancePercent" HeaderText="AttendancePercent" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                            <Rock:RockBoundField DataField="EngagementScore" HeaderText="Engagement Score" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                            <Rock:RockTemplateField HeaderText="Attendance" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" SortExpression="Attendance" ExcelExportBehavior="NeverInclude">
                                <ItemTemplate>
                                    <%# GetAttendanceColumnHtml((int)Eval("Attendance"),(decimal)Eval("AttendancePercent") ) %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="Engagement Score" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" SortExpression="EngagementScore" ExcelExportBehavior="NeverInclude">
                                <ItemTemplate>
                                    <%# GetEngagementScoreColumnHtml((int)Eval("EngagementScore"),(int)Eval("Week")) %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:BoolField DataField="IsFirstSteps" HeaderText="First Steps Class" />
                            <Rock:RockLiteralField ID="lActionsTaken" HeaderText="Action Taken" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                            <Rock:RockLiteralField ID="lConnection" HeaderText="Connections" ItemStyle-HorizontalAlign="Left" HeaderStyle-HorizontalAlign="Left" />
                        </Columns>
                    </Rock:Grid>
                    <div class="row padding-h-md">
                        <div class="col-md-12">
                            <asp:Repeater ID="rptWorkFlow" runat="server" OnItemCommand="rptWorkFlow_ItemCommand">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lbLaunchWorkflow" runat="server" CssClass="btn btn-default btn-sm" CommandArgument='<%# Eval("Key") %>' CommandName="Display">
                                <i class="fa fa-cog"></i> <%# Eval("Value") %>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:LinkButton ID="lbFollow" runat="server" CssClass="btn btn-sm btn-default pull-right" OnClick="lbFollow_Click">
                                <i class="fas fa-flag"></i> Follow
                            </asp:LinkButton>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <Rock:ModalDialog ID="mdConfirm" runat="server" Title="Confirm" SaveButtonText="Yes" OnSaveClick="mdConfirm_Click">
            <Content>
                <asp:HiddenField ID="hfWorkflowTypeId" runat="server" />
                <asp:Literal ID="lConfirmMsg" runat="server" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
