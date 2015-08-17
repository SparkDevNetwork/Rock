<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonSuggestionList.ascx.cs" Inherits="RockWeb.Blocks.Follow.PersonSuggestionList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="maGridWarning" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-flag-o"></i> Your Following Suggestions</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gSuggestions" runat="server" AllowSorting="true" RowItemText="Following">
                        <Columns>
                            <Rock:SelectField />
                            <Rock:PersonField DataField="Person" HeaderText="Name" SortExpression="LastName,NickName" />
                            <Rock:DateField DataField="LastPromotedDateTime" HeaderText="Last Suggested" SortExpression="LastPromotedDateTime" />
                            <Rock:RockBoundField DataField="ReasonNote" HeaderText="Reason" />
                            <Rock:RockTemplateField HeaderText="Status">
                                <ItemTemplate>
                                    <%# Eval("StatusLabel") %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:DateTimeField DataField="StatusChangedDateTime" HeaderText="Status Changed" SortExpression="StatusChangedDateTime" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
