<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonDuplicateList.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDuplicateList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-users"></i> Person Duplicate List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" DataKeyNames="PersonId" AllowSorting="true" OnRowSelected="gList_RowSelected">
                        <Columns>
                            <Rock:RockTemplateField HeaderText="Confidence" ItemStyle-HorizontalAlign="Center" SortExpression="MaxConfidenceScore">
                                <ItemTemplate>
                                    <%# GetConfidenceScoreColumnHtml((double?)Eval("MaxConfidenceScore")) %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            
                            <Rock:RockBoundField DataField="FirstName" HeaderText="First Name" SortExpression="FirstName, LastName" />
                            <Rock:RockBoundField DataField="LastName" HeaderText="Last Name" SortExpression="LastName, FirstName" />
                            <Rock:RockBoundField DataField="MatchCount" HeaderText="Match Count" SortExpression="MatchCount" ItemStyle-HorizontalAlign="Right" />

                            <Rock:DateTimeField DataField="PersonModifiedDateTime" HeaderText="Modified" SortExpression="PersonModifiedDateTime" />
                            <Rock:RockBoundField DataField="CreatedByPerson" HeaderText="Created By" SortExpression="CreatedByPerson" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
