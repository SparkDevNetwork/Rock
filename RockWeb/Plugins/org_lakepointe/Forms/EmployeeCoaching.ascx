<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmployeeCoaching.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Forms.EmployeeCoaching" %>
<style>
    .irs-bar-edge {
        background: unset;
    }

    .irs-bar {
        background: unset;
        border-top: unset;
        border-bottom: unset;
    }

    .control-label {
        text-transform: uppercase;
    }

    .guide {
        margin-top: -15px;
        margin-bottom: +15px;
        font-weight: bold;
    }

    .form-group {
        padding-top: 40px;
    }

    .prelude {
        margin-bottom: -47px;
    }
    .row {
        margin-right: unset;
        margin-left: unset;
    }
    .irs-min {
        visibility: hidden !important;
        display: none !important;
    }

    .irs-max {
        visibility: hidden !important;
        display: none !important;
    }
    .irs-single {
        visibility: hidden !important;
        display: none !important;
    }
}
</style>
<asp:UpdatePanel ID="upEmployeeCoaching" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="maAlert" runat="server" />
        <asp:ValidationSummary ID="ValidationSummary1" runat="server" ValidationGroup="vg" HeaderText="Please correct the following:" CssClass="alert alert-danger" />
        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Danger" />
        <Rock:NotificationBox ID="nbInfo" runat="server" NotificationBoxType="Info" />
        <asp:Panel ID="pnlInfo" runat="server" Visible="true">
            <asp:HiddenField ID="hfWorkflowId" runat="server" Visible="false" />
            <asp:HiddenField ID="hfCurrentOwner" runat="server" Visible="false" />
            <div class="row" style="background-color: #f04b28; color: #fff; padding-left: 15px; padding-right: 15px; margin-bottom: 30px;">
                <h1 style="font-size: 75px; font-weight: 700;">1:1 COACHING</h1>
                <h2 style="font-size: 50px;">EMPLOYEE & SUPERVISOR</h2>
            </div>
            <div class="row" style="border-color: #f04b28; padding-left: 15px; padding-right: 15px;">
                <p>
                    <b>INSTRUCTIONS</b><br />
                    The SUPERVISOR should use this form to initiate a meeting with their EMPLOYEE and discuss performance and goals.
                </p>
                <p>
                    <b>WORKFLOW</b><br />
                    Supervisor: As you go over the 6 Leadership Behaviors, highlight a couple where the employee has excelled over the past month. Also pick 1 or 2 where you would like to see growth. USE EXAMPLES to illustrate both.
                </p>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <Rock:DateTimePicker ID="dtpSubmitDate" runat="server" Label="Date Submitted by Employee" Enabled="false" />
                </div>
                <div class="col-md-6">
                    <Rock:DateTimePicker ID="dtpSupervisorDate" runat="server" Label="Date Submitted by Supervisor" Enabled="false" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-3">
                    <Rock:DataDropDownList ID="dddlSupervisor" runat="server" Label="<b>1. </b>Supervisor" ValidationGroup="vg" Required="true" SourceTypeName="Rock.Model.Person, Rock" PropertyName="FullName" />
                </div>
                <div class="col-md-6">
                    <Rock:ButtonDropDownList ID="bddlMonth" runat="server" ValidationGroup="vg" Required="true" Label="<b>2. </b>This is the 1:1 coaching form for the month of:" Enabled="true">
                        <asp:ListItem Text="January" Value="1" />
                        <asp:ListItem Text="February" Value="2" />
                        <asp:ListItem Text="March" Value="3" />
                        <asp:ListItem Text="April" Value="4" />
                        <asp:ListItem Text="May" Value="5" />
                        <asp:ListItem Text="June" Value="6" />
                        <asp:ListItem Text="July" Value="7" />
                        <asp:ListItem Text="August" Value="8" />
                        <asp:ListItem Text="September" Value="9" />
                        <asp:ListItem Text="October" Value="10" />
                        <asp:ListItem Text="November" Value="11" />
                        <asp:ListItem Text="December" Value="12" />
                    </Rock:ButtonDropDownList>
                </div>
                <div class="col-md-3">
                    <Rock:RockDropDownList ID="ddlStatusFilter" runat="server" Visible="true" Label="Report Status">
                        <asp:ListItem Text="Employee" Value="Employee"></asp:ListItem>
                        <asp:ListItem Text="Supervisor" Value="Supervisor"></asp:ListItem>
                        <asp:ListItem Text="HR" Value="HR"></asp:ListItem>
                    </Rock:RockDropDownList>
                </div>
            </div>
            <div class="row">
                <div class="col-md-3">
                    <Rock:PersonPicker ID="ppEmployee" runat="server" ValidationGroup="vg" Label="<b>3. </b>Employee" Required="true" />
                </div>
                <div class="col-md-6">
                    <Rock:RockTextBox ID="tbPosition" runat="server" ValidationGroup="vg" Label="<b>4. </b>Position Title" Required="true" />
                </div>
            </div>
            <div class="row dual-slider">
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RangeSlider ID="rsLove" runat="server" Required="true" Label="<b>5. </b>Loves & Follows Jesus" MaxValue="10" MinValue="0" SelectedValue="5"
                            Help="They exhibit the fruit of the Spirit in their work and interaction with others. They are actively deepening their personal relationship with the Lord and seek to encourage others in their relationship with the Lord." />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RangeSlider ID="rsLoveSupervisor" runat="server" Required="false" MaxValue="10" MinValue="0" SelectedValue="5" />
                    </div>
                </div>
                <br />
                <div class="row guide">
                    <div class="col-xs-4">
                        <div style="text-align: left;">Below Expectations</div>
                    </div>
                    <div class="col-xs-4">
                        <div style="text-align: center;">Meets Expectations</div>
                    </div>
                    <div class="col-xs-4">
                        <div style="text-align: right;">Exceeds Expectations</div>
                    </div>
                </div>
            </div>

            <div class="row dual-slider">
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RangeSlider ID="rsHonor" runat="server" Required="true" Label="<b>6. </b>Honors Up Down & All Around" MaxValue="10" MinValue="0" SelectedValue="5"
                            Help="They speak in a positive manner regarding co-workers, supervisors, and the church leadership, not involved in gossip nor speaking negatively about others. When faced with a concern or disagreement with decisions made by leadership, they will only discuss it with their supervisor or leadership staff." />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RangeSlider ID="rsHonorSupervisor" runat="server" Required="false" MaxValue="10" MinValue="0" SelectedValue="5" />
                    </div>
                </div>
                <br />
                <div class="row guide">
                    <div class="col-xs-4">
                        <div style="text-align: left;">Below Expectations</div>
                    </div>
                    <div class="col-xs-4">
                        <div style="text-align: center;">Meets Expectations</div>
                    </div>
                    <div class="col-xs-4">
                        <div style="text-align: right;">Exceeds Expectations</div>
                    </div>
                </div>
            </div>
            <div class="row dual-slider">
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RangeSlider ID="rsFun" runat="server" Required="true" Label="<b>7. </b>Makes it Fun" MaxValue="10" MinValue="0" SelectedValue="5"
                            Help="This is about positive energy. They exhibit an open and cheerful attitude towards LP and the staff around them. Their presence brings a spirit of fun and joy." />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RangeSlider ID="rsFunSupervisor" runat="server" Required="false" MaxValue="10" MinValue="0" SelectedValue="5" />
                    </div>
                </div>
                <br />
                <div class="row guide">
                    <div class="col-xs-4">
                        <div style="text-align: left;">Below Expectations</div>
                    </div>
                    <div class="col-xs-4">
                        <div style="text-align: center;">Meets Expectations</div>
                    </div>
                    <div class="col-xs-4">
                        <div style="text-align: right;">Exceeds Expectations</div>
                    </div>
                </div>
            </div>
            <div class="row dual-slider">
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RangeSlider ID="rsGreat" runat="server" Required="true" Label="<b>8. </b>Rejects Good for Great" MaxValue="10" MinValue="0" SelectedValue="5"
                            Help="They never say, &quot;That’s good enough.&quot; Excellence in the execution of their work is very important to them. They are willing to put in additional time and effort, paying attention to detail in order to provide the best result that will honor the church and God." />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RangeSlider ID="rsGreatSupervisor" runat="server" Required="false" MaxValue="10" MinValue="0" SelectedValue="5" />
                    </div>
                </div>
                <br />
                <div class="row guide">
                    <div class="col-xs-4">
                        <div style="text-align: left;">Below Expectations</div>
                    </div>
                    <div class="col-xs-4">
                        <div style="text-align: center;">Meets Expectations</div>
                    </div>
                    <div class="col-xs-4">
                        <div style="text-align: right;">Exceeds Expectations</div>
                    </div>
                </div>
            </div>
            <div class="row dual-slider">
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RangeSlider ID="rsWhatever" runat="server" Required="true" Label="<b>9. </b>Whatever it Takes" MaxValue="10" MinValue="0" SelectedValue="5"
                            Help="This is about flexibility for change. If asked to take on new or different roles, titles, responsibilities, or if asked to relinquish roles, titles, responsibilities, etc., they embrace change with a positive attitude because they’re willing to do &quot;whatever it takes&quot; to reach people for Christ. They never display an attitude that says, &quot;that’s not my job.&quot;" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RangeSlider ID="rsWhateverSupervisor" runat="server" Required="false" MaxValue="10" MinValue="0" SelectedValue="5" />
                    </div>
                </div>
                <br />
                <div class="row guide">
                    <div class="col-xs-4">
                        <div style="text-align: left;">Below Expectations</div>
                    </div>
                    <div class="col-xs-4">
                        <div style="text-align: center;">Meets Expectations</div>
                    </div>
                    <div class="col-xs-4">
                        <div style="text-align: right;">Exceeds Expectations</div>
                    </div>
                </div>
            </div>
            <div class="row dual-slider">
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RangeSlider ID="rsLakepointe" runat="server" Required="true" Label="<b>10. </b>Loves Lakepointe" MaxValue="10" MinValue="0" SelectedValue="5"
                            Help="They model a calling-level love for Lakepointe in their attendance, giving, serving, support, and speech. Their lives and speech are a walking advertisement for the work of God at LP." />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RangeSlider ID="rsLakepointeSupervisor" runat="server" Required="false" MaxValue="10" MinValue="0" SelectedValue="5" />
                    </div>
                </div>
                <br />
                <div class="row guide">
                    <div class="col-xs-4">
                        <div style="text-align: left;">Below Expectations</div>
                    </div>
                    <div class="col-xs-4">
                        <div style="text-align: center;">Meets Expectations</div>
                    </div>
                    <div class="col-xs-4">
                        <div style="text-align: right;">Exceeds Expectations</div>
                    </div>
                </div>
            </div>
            <div class="row dual-slider">
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RangeSlider ID="rsExecution" runat="server" Required="true" Label="<b>11. </b>Executing at expected level & accomplishing objectives" MaxValue="10" MinValue="0" SelectedValue="5"
                            Help="Rate the level at which the employee is performing the function of their position and meeting goals or expectations given to them." />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RangeSlider ID="rsExecutionSupervisor" runat="server" Required="false" MaxValue="10" MinValue="0" SelectedValue="5" />
                    </div>
                </div>
                <br />
                <div class="row guide">
                    <div class="col-xs-4">
                        <div style="text-align: left;">Below Expectations</div>
                    </div>
                    <div class="col-xs-4">
                        <div style="text-align: center;">Meets Expectations</div>
                    </div>
                    <div class="col-xs-4">
                        <div style="text-align: right;">Exceeds Expectations</div>
                    </div>
                </div>
            </div>
            <div class="row dual-slider">
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RangeSlider ID="rsJoy" runat="server" Required="true" Label="<b>12. </b>How are you enjoying your life and ministry?" MaxValue="10" MinValue="0" SelectedValue="5" />
                    </div>
                </div>
                <br />
                <div class="row guide">
                    <div class="col-xs-4">
                        <div style="text-align: left;">Below Expectations</div>
                    </div>
                    <div class="col-xs-4">
                        <div style="text-align: center;">Meets Expectations</div>
                    </div>
                    <div class="col-xs-4">
                        <div style="text-align: right;">Exceeds Expectations</div>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <Rock:RockTextBox ID="tbWins" runat="server" Required="false" Label="<b>13. </b>Wins since our last meeting"
                        TextMode="MultiLine" Rows="3" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <Rock:RockTextBox ID="tbHelp" runat="server" Required="false" Label="<b>14. </b>TOPICS TO DISCUSS, ISSUES YOU MIGHT HAVE, INITIATIVES YOU ARE WORKING ON FOR THE NEXT MONTH"
                        TextMode="MultiLine" Rows="3" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <Rock:RockTextBox ID="tbStrengths" runat="server" Required="false" Label="<b>15. </b>List 1-3 strengths you see in yourself"
                        TextMode="MultiLine" Rows="3" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <Rock:RockTextBox ID="tbOpportunities" runat="server" Required="false" Label="<b>16. </b>List 1-3 opportunities for growth you see for yourself"
                        TextMode="MultiLine" Rows="3" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <Rock:RockTextBox ID="tbNextSteps" runat="server" Required="false" Label="<b>17. </b>List 2-3 next steps for your development"
                        TextMode="MultiLine" Rows="3" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <Rock:RockTextBox ID="tbBugs" runat="server" Required="false" Label="<b>18. </b>What's bugging you?"
                        TextMode="MultiLine" Rows="3" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <asp:CustomValidator runat="server" ID="CheckBoxRequired" EnableClientScript="true" OnServerValidate="CheckBoxRequired_ServerValidate" ValidationGroup="vg">
                        <h1 style="color:red; margin-bottom:-40px;">You must select this box to proceed.</h1>
                    </asp:CustomValidator>
                    <Rock:RockCheckBox ID="cbAcknowledge" runat="server" Required="true" ValidationGroup="vg" 
                        SelectedIconCssClass="fa fa-check-square-o fa-lg" UnSelectedIconCssClass="fa fa-square-o fa-lg"
                        Label="I acknowledge my supervisor will complete their portion of this form following our 1:1 Coaching time together.  It is my responsibility to review this completed form through ROCK once my supervisor has submitted the form."
                        />
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <Rock:RockTextBox ID="tbFeedback" runat="server" Required="false" Label="<b>19. </b>Feedback or redirection from supervisor"
                        TextMode="MultiLine" Rows="3" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <asp:CustomValidator runat="server" ID="CustomValidator1" EnableClientScript="true" OnServerValidate="SupervisorConfidential_ServerValidate" ValidationGroup="vg1">
                        <h1 style="color:red; margin-bottom:-40px;">You must enter a response in this box to proceed.</h1>
                    </asp:CustomValidator>
                    <Rock:RockTextBox ID="tbSupervisorConfidential" runat="server" Required="true" ValidationGroup="vg1"
                        Label="<b>20. </b>Confidential notes from supervisor to HR. Employee will not see these notes. Please indicate an overall performance rating (combination of Leadership Behaviors & Execution of Responsibilities) of employee: &quot;Below Expectations&quot;, &quot;Meets Expectations&quot;, or &quot;Exceeds Expectations.&quot;"
                        TextMode="MultiLine" Rows="3" />
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlConfirmation" runat="server" Visible="false">
            <Rock:NotificationBox ID="nbConfirmation" runat="server" NotificationBoxType="Success" />
        </asp:Panel>

        <asp:Panel ID="pnlNavigation" runat="server" Visible="true">
            <div class="row" style="margin-top: 20px">
                <div class="col-md-12 text-center">
                    <asp:LinkButton ID="lbCancel" runat="server" Visible="true" CausesValidation="false" CssClass="btn btn-primary btn-lg">Cancel</asp:LinkButton>
                    <asp:LinkButton ID="lbSave" runat="server" Visible="true" CausesValidation="false" CssClass="btn btn-primary btn-lg">Save</asp:LinkButton>
                    <asp:LinkButton ID="lbSubmit" runat="server" Visible="true" CausesValidation="true" ValidationGroup="vg" CssClass="btn btn-primary btn-lg">Submit</asp:LinkButton>
                    <asp:LinkButton ID="lbSupervisorSubmit" runat="server" Visible="false" CausesValidation="true" ValidationGroup="vg1" CssClass="btn btn-primary btn-lg">Submit</asp:LinkButton>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
