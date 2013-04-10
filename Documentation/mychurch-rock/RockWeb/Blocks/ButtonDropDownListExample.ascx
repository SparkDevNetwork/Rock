<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ButtonDropDownListExample.ascx.cs" Inherits="ButtonDropDownListExample" %>
<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <hr />
            <h4>ButtonDropDown with No Postback</h4>
            <Rock:ButtonDropDownList ID="ddlNoPostBack" runat="server" />
            <asp:LinkButton ID="bbGetNoPostBackText" runat="server" OnClick="bbGetNoPostBackText_Click" text="Get Value" CssClass="btn btn-mini"/>
            <Rock:LabeledText ID="lblSelectedItem1" runat="server" LabelText="Selected Item" />
            <hr />
            <h4>ButtonDropDown with Postback</h4>
            <Rock:ButtonDropDownList ID="ddlWithPostBack" runat="server" OnSelectionChanged="ddlWithPostBack_SelectionChanged"/>
            <Rock:LabeledText ID="lblSelectedItem2" runat="server" LabelText="Selected Item" />
            <hr />
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
