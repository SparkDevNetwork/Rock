<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReminderEdit.ascx.cs" Inherits="RockWeb.Blocks.Reminders.ReminderEdit" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-bell"></i> 
                    Reminder for <asp:Literal ID="lEntity" runat="server" />
                </h1>
            </div>

            <div class="panel-body">
                <asp:HiddenField ID="hfReminderId" runat="server" Value="0" />
                <Rock:DatePicker ID="rdpReminderDate" runat="server" Label="Reminder Date" Required="true" AllowPastDateSelection="false" />
                <Rock:RockTextBox ID="rtbNote" runat="server" Label="Note" TextMode="MultiLine" />
                <Rock:RockDropDownList ID="rddlReminderType" runat="server" Label="Reminder Type" Required="true" />
                <Rock:PersonPicker ID="rppPerson" runat="server" Label="Send Reminder To" Required="true" EnableSelfSelection="true" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:NumberBox ID="rnbRepeatDays" runat="server" Label="Repeat Every" Help="Will repeat the reminder the provided number of days after the completion." AppendText="days" />
                    </div>
                    <div class="col-md-6">
                        <Rock:NumberBox ID="rnbRepeatTimes" runat="server" Label="Number of Times to Repeat" Help="The number of times to repeat.  Leave blank to repeat indefinitely." AppendText="times" />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>