<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RecurringCommunicationsList.ascx.cs" Inherits="RockWeb.Plugins.org_secc.RecurringCommunications.RecurringCommunicationsList" %>
<asp:UpdatePanel runat="server" ID="upContent">
    <ContentTemplate>
        <Rock:Grid runat="server" ID="gRC" DataKeyNames="Id" OnRowSelected="gRC_RowSelected">
            <Columns>
                <Rock:RockBoundField HeaderText="Communication Name" DataField="Name" />
                <Rock:RockBoundField HeaderText="Created By" DataField="CreatedByPersonAlias.Person.FullName" />
                <Rock:RockBoundField HeaderText="Communication Type" DataField="CommunicationType" />
                <Rock:RockBoundField HeaderText="Schedule" DataField="ScheduleDescription" />
                <Rock:RockBoundField HeaderText="DataView" DataField="DataView.Name" />
                <Rock:RockBoundField HeaderText="Transformation" DataField="TransformationEntityType.FriendlyName" />
                <Rock:DeleteField ID="btnDelete" OnClick="btnDelete_Click" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
