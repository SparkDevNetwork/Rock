<%@ Control Language="C#" AutoEventWireup="true" CodeFile="VolunteerScreening_CharacterReferenceList.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.SafetySecurity.VolunteerScreening_CharacterReferenceList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pCharReferenceList" runat="server" class="panel panel-block">
            <div class="panel-heading">
                <asp:Literal runat="server" ID="lHeader"></asp:Literal>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">

                    <Rock:Grid ID="gGrid" runat="server" OnRowSelected="gGrid_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="CompletionNumber" HeaderText="Number" SortExpression="Number" />
                            <Rock:RockBoundField DataField="Date" HeaderText="Date" SortExpression="Date" />
                            <Rock:RockBoundField DataField="VolunteerApplicantsName" HeaderText="Volunteer Applicants Name"/>
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
