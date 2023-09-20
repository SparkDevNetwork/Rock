<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EmployeeEvaluation.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Forms.EmployeeEvaluation" %>
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
</style>
<asp:UpdatePanel ID="upEmployeeEvaluation" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Danger" />
        <asp:Panel ID="pnlInfo" runat="server" Visible="true">
            <div class="row" style="background-color: #f04b28; color: #fff; padding-left: 15px; padding-right: 15px;">
                <h1>EMPLOYEE EVALUATIONS</h1>
                <p>
                    Provide the employee name and the job title for the position you are rating them.<br>
                    Using a scale from 0 to 10, rate the employee's performance in each of our six Leadership Behaviors & Job Performance.
                </p>
            </div>
            <div class="row">
                <Rock:DatePicker ID="dpDate" runat="server" Label="Report Date" Visible="false" />
            </div>
            <div class="row">
                <Rock:RockLiteral ID="lSupervisor" runat="server" Label="Supervisor" Text="-" />
            </div>
            <div class="row">
                <Rock:PersonPicker ID="ppEmployee" runat="server" Label="Employee" Required="true" />
            </div>
            <div class="row">
                <Rock:RockLiteral ID="lposition" runat="server" Label="" CssClass="prelude" Text="<i>I am rating the employee as their supervisor in the following position:</i>" />
                <Rock:RockTextBox ID="tbPosition" runat="server" Label="Position Title" Required="true" />
            </div>
            <div class="row">
                <Rock:RockRadioButtonList ID="rblCurrent" runat="server" Label="I currently supervise this employee in this position" RepeatDirection="Horizontal" Required="true">
                    <asp:ListItem Text="Yes" Value="True" />
                    <asp:ListItem Text="No" Value="False" />
                </Rock:RockRadioButtonList>
            </div>
            <div class="row">
                <Rock:RangeSlider ID="rsLove" runat="server" Required="true" Label="Loves & Follows Jesus" MaxValue="10" MinValue="0" SelectedValue="5"
                    Help="They exhibit the fruit of the Spirit in their work and interaction with others. They are actively deepening their personal relationship with the Lord and seek to encourage others in their relationship with the Lord." />
            </div>
            <div class="row guide">
                <div style="float: left">Toxic</div>
                <div style="float: right">Contagious</div>
                <div style="margin: 0 auto; width: 100px;">Acceptable</div>
            </div>
            <div class="row">
                <Rock:RangeSlider ID="rsHonor" runat="server" Required="true" Label="Honors Up Down & All Around" MaxValue="10" MinValue="0" SelectedValue="5"
                    Help="They speak in a positive manner regarding co-workers, supervisors, and the church leadership, not involved in gossip nor speaking negatively about others. When faced with a concern or disagreement with decisions made by leadership, they will only discuss it with their supervisor or leadership staff." />
            </div>
            <div class="row guide">
                <div style="float: left">Toxic</div>
                <div style="float: right">Contagious</div>
                <div style="margin: 0 auto; width: 100px;">Acceptable</div>
            </div>
            <div class="row">
                <Rock:RangeSlider ID="rsFun" runat="server" Required="true" Label="Makes it Fun" MaxValue="10" MinValue="0" SelectedValue="5"
                    Help="This is about positive energy. They exhibit an open and cheerful attitude towards LP and the staff around them. Their presence brings a spirit of fun and joy." />
            </div>
            <div class="row guide">
                <div style="float: left">Toxic</div>
                <div style="float: right">Contagious</div>
                <div style="margin: 0 auto; width: 100px;">Acceptable</div>
            </div>
            <div class="row">
                <Rock:RangeSlider ID="rsGreat" runat="server" Required="true" Label="Rejects Good for Great" MaxValue="10" MinValue="0" SelectedValue="5"
                    Help="They never say, &quot;That’s good enough.&quot; Excellence in the execution of their work is very important to them. They are willing to put in additional time and effort, paying attention to detail in order to provide the best result that will honor the church and God." />
            </div>
            <div class="row guide">
                <div style="float: left">Toxic</div>
                <div style="float: right">Contagious</div>
                <div style="margin: 0 auto; width: 100px;">Acceptable</div>
            </div>
            <div class="row">
                <Rock:RangeSlider ID="rsWhatever" runat="server" Required="true" Label="Whatever it Takes" MaxValue="10" MinValue="0" SelectedValue="5"
                    Help="This is about flexibility for change. If asked to take on new or different roles, titles, responsibilities, or if asked to relinquish roles, titles, responsibilities, etc., they embrace change with a positive attitude because they’re willing to do &quot;whatever it takes&quot; to reach people for Christ. They never display an attitude that says, &quot;that’s not my job.&quot;" />
            </div>
            <div class="row guide">
                <div style="float: left">Toxic</div>
                <div style="float: right">Contagious</div>
                <div style="margin: 0 auto; width: 100px;">Acceptable</div>
            </div>
            <div class="row">
                <Rock:RangeSlider ID="rsLakepointe" runat="server" Required="true" Label="Loves Lakepointe" MaxValue="10" MinValue="0" SelectedValue="5"
                    Help="They model a calling-level love for Lakepointe in their attendance, giving, serving, support, and speech. Their lives and speech are a walking advertisement for the work of God at LP." />
            </div>
            <div class="row guide">
                <div style="float: left">Toxic</div>
                <div style="float: right">Contagious</div>
                <div style="margin: 0 auto; width: 100px;">Acceptable</div>
            </div>
            <div class="row">
                <Rock:RangeSlider ID="rsExpertise" runat="server" Required="true" Label="Level of Expertise in their Current Role" MaxValue="10" MinValue="0" SelectedValue="5"
                    Help="This measures their subject knowledge, skill, and talent in their role on the team. How well does the employee understand their current role? Do they require input on a continuous basis, or do they understand the requirements of their position in a way that allows them to function with reasonable autonomy?" />
            </div>
            <div class="row guide">
                <div style="float: left">Toxic</div>
                <div style="float: right">Contagious</div>
                <div style="margin: 0 auto; width: 100px;">Acceptable</div>
            </div>
            <div class="row">
                <Rock:RangeSlider ID="rsExecution" runat="server" Required="true" Label="Level of Execution of their Duties" MaxValue="10" MinValue="0" SelectedValue="5"
                    Help="This question is about how well they do their job vs. how well they know the subject matter. Do they perform with excellence? Do they work well within their team? Are they able to complete assignments on time with little-to-no errors?" />
            </div>
            <div class="row guide">
                <div style="float: left">Toxic</div>
                <div style="float: right">Contagious</div>
                <div style="margin: 0 auto; width: 100px;">Acceptable</div>
            </div>
            <div class="row">
                <Rock:RockTextBox ID="tbComments" runat="server" Required="false" Label="PROVIDE ANY ADDITIONAL INFORMATION YOU WOULD LIKE TO SHARE REGARDING THIS EMPLOYEE'S PERFORMANCE."
                    TextMode="MultiLine" Rows="5" />
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlConfirmation" runat="server" Visible="false">
            <Rock:NotificationBox ID="nbConfirmation" runat="server" NotificationBoxType="Success" />
        </asp:Panel>

        <asp:Panel ID="pnlNavigation" runat="server" Visible="true">
            <div class="row" style="margin-top: 20px">
                <div class="col-md-12 text-center">
                    <asp:LinkButton ID="lbSave" runat="server" Visible="true" CausesValidation="true" ValidationGroup="vgSignup" CssClass="btn btn-primary btn-lg">Submit</asp:LinkButton>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
