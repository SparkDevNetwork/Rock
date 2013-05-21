<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduleBuilderControlExample.ascx.cs" Inherits="RockWeb.Blocks.ScheduleBuilderControlExample" %>

<asp:UpdatePanel runat="server" ID="upPanel">
    <ContentTemplate>
        <Rock:LabeledTextBox ID="lblScheduleCalendarContent" runat="server" LabelText="iCal Content" TextMode="MultiLine" Rows="20" Style="white-space: pre" Font-Names="Consolas" Font-Size="9" CssClass="input-xxlarge"/>
        <asp:Button ID="btnOK" runat="server" Text="Update"  OnClick="btnOK_Click"/>
        <Rock:ScheduleBuilder runat="server" ID="sbExample" LabelText="Some Schedule Label" OnSaveSchedule="sbExample_SaveSchedule" OnCancelSchedule="sbExample_CancelSchedule" />
        <Rock:LabeledText ID="lblOccurrances" runat="server" LabelText="Next Occurrences" />
    </ContentTemplate>
</asp:UpdatePanel>
