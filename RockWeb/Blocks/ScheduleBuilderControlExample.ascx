<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduleBuilderControlExample.ascx.cs" Inherits="RockWeb.Blocks.ScheduleBuilderControlExample" %>

<asp:UpdatePanel runat="server" ID="upPanel">
    <ContentTemplate>
        <Rock:ScheduleBuilder runat="server" ID="sbExample" LabelText="Some Schedule Label" OnSaveSchedule="sbExample_SaveSchedule" OnCancelSchedule="sbExample_CancelSchedule" />
    </ContentTemplate>
</asp:UpdatePanel>
