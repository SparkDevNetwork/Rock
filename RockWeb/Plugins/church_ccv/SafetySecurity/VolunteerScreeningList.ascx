<%@ Control Language="C#" AutoEventWireup="true" CodeFile="VolunteerScreeningList.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.SafetySecurity.VolunteerScreeningList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pVolunteerScreeningList" runat="server" class="panel panel-block">
            <div class="panel-heading">
                <div class="row col-sm-4">
                    <h4 class="panel-title">Volunteer Screening</h4>
                    <br />
                </div>
            </div>
            <div class="panel-body">
            

                <div class="grid grid-panel">

                    <Rock:Grid ID="gGrid" runat="server" OnRowSelected="gGrid_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="Type" HeaderText="Type" SortExpression="Type" />
                            <Rock:RockBoundField DataField="Date" HeaderText="Application Date" SortExpression="Date" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
