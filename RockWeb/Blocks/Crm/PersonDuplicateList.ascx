<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonDuplicateList.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDuplicateList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-files-o"></i> Person Duplicate List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" DataKeyNames="PersonId" AllowSorting="true" OnRowSelected="gList_RowSelected">
                        <Columns>
                            <asp:BoundField DataField="FirstName" HeaderText="First Name" SortExpression="FirstName, LastName" />
                            <asp:BoundField DataField="LastName" HeaderText="Last Name" SortExpression="LastName, FirstName" />
                            <asp:BoundField DataField="MatchCount" HeaderText="Match Count" SortExpression="MatchCount" ItemStyle-HorizontalAlign="Right" />

                            <asp:TemplateField HeaderText="Max Score" ItemStyle-HorizontalAlign="Right" SortExpression="MaxScorePercent">
                                <ItemTemplate>
                                    <span class='<%# GetMatchLabelClass((double?)Eval("MaxScorePercent"))  %>'><%# ((double?)Eval("MaxScorePercent")/100).Value.ToString("P") %></span>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <Rock:DateTimeField DataField="PersonModifiedDateTime" HeaderText="Modified" SortExpression="PersonModifiedDateTime" />
                            <asp:BoundField DataField="CreatedByPerson" HeaderText="Created By" SortExpression="CreatedByPerson" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
