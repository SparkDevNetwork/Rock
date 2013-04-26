<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExceptionOccurrences.ascx.cs" Inherits="RockWeb.Blocks.Administration.ExceptionOccurrences" %>
<asp:UpdatePanel ID="upExceptionOccurrences" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlExceptionOccurrences" runat="server" CssClass="well"> 
            <asp:HiddenField ID="hfExceptionLogID" runat="server" />
            <div class="exception-occurrence-summary">
                <fieldset>
                    <legend>
                        <h4>Exception Occurrences</h4>
                    </legend>
                    <div>Site:
                        <asp:Label ID="lblSite" runat="server" /></div>
                    <div>Page:
                        <asp:Label ID="lblPage" runat="server" /></div>
                    <div>Type:
                        <asp:Label ID="lblType" runat="server" /></div>
                </fieldset>
            </div>
            <Rock:Grid ID="gExceptionOccurrences" runat="server" AllowSorting="true">
                <Columns>
                    <Rock:DateTimeField DataField="ExceptionDateTime" HeaderText="Date" SortExpression="ExceptionDateTime" />
                    <asp:BoundField DataField="FullName" HeaderText="Logged In User" SortExpression="FullName" />
                    <asp:BoundField DataField="Description" HeaderText="Exception" SortExpression="Description" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
