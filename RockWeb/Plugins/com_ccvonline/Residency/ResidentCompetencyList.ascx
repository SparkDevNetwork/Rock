<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResidentCompetencyList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.ResidentCompetencyList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Repeater ID="rpTracks" runat="server" OnItemDataBound="rpTracks_ItemDataBound">
            <ItemTemplate>
                
                <div class="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title"><i class="fa fa-code-fork"></i> <%# DataBinder.Eval( Container.DataItem, "Name") %></h1>
                    </div>
                    <div class="panel-body">

                        <div class="grid grid-panel">

                            <Rock:Grid ID="gCompetencyList" runat="server" AllowSorting="false" OnRowSelected="gCompetencyList_RowSelected">
                                <Columns>
                                    <asp:BoundField DataField="CompetencyName" HeaderText="Competency" SortExpression="CompetencyName" ItemStyle-Width="50%" />
                                    <asp:BoundField DataField="CompletedProjectAssessmentsTotal" HeaderText="Project Assessments Completed" SortExpression="CompletedProjectAssessmentsTotal" />
                                    <asp:BoundField DataField="MinProjectAssessmentsTotal" HeaderText="Project Assessments Required" SortExpression="MinProjectAssessmentsTotal" />
                                </Columns>
                            </Rock:Grid>

                        </div>

                    </div>
                </div>

            </ItemTemplate>
        </asp:Repeater>
    </ContentTemplate>
</asp:UpdatePanel>
