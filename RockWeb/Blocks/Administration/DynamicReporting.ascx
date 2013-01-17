<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DynamicReporting.ascx.cs" Inherits="RockWeb.Blocks.Administration.DynamicReporting" %>
<asp:UpdatePanel ID="upReporting" runat="server">
    <ContentTemplate>
        <div class="row-fluid">
            <div class="span6">
                <Rock:LabeledCheckBoxList ID="lstColumns" runat="server" LabelText="Result Columns" RepeatColumns="3" />
            </div>
            <div class="span6">
                <div>
                    <asp:Label ID="lblWhere" runat="server" Text="Where clause" />
                </div>
                <div>
                    <asp:TextBox ID="tbWhereClause" runat="server" Columns="80" Rows="10" TextMode="MultiLine" />
                </div>
                <div>
                    <asp:LinkButton ID="btnGo" runat="server" Text="Click Me" OnClick="btnGo_Click" />
                </div>
            </div>
        </div>
        <div>
            <asp:Label runat="server" ID="lblGridTitle" Text="Results" />
            <Rock:Grid ID="gResults" runat="server">
                <Columns>
                </Columns>
            </Rock:Grid>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
