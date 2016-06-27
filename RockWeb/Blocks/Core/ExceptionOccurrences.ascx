<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExceptionOccurrences.ascx.cs" Inherits="RockWeb.Blocks.Administration.ExceptionOccurrences" %>

<asp:UpdatePanel ID="upExceptionList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlExceptionOccurrences" CssClass="panel panel-block" runat="server">
            <asp:HiddenField ID="hfBaseExceptionID" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-bug"></i>
                    <asp:Literal ID="lDetailTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <div class="row">
                    <div class="col-md-12">
                        <p>
                            Below is a list of each occurrence of this error.
                        </p>
                        <asp:Literal ID="lblMainDetails" runat="server" />
                    </div>
                </div>

                <h4>Exception List</h4>

                <div class="grid margin-t-md">
                    <Rock:Grid ID="gExceptionOccurrences" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:DateTimeField DataField="CreatedDateTime" HeaderText="Date/Time" SortExpression="CreatedDateTime" />
                            <Rock:RockBoundField DataField="PageName" HeaderText="Page/URL" SortExpression="PageName" />
                            <Rock:RockBoundField DataField="FullName" HeaderText="Logged In User" SortExpression="FullName" />
                            <Rock:RockBoundField DataField="Description" HeaderText="Exception" SortExpression="Description" HtmlEncode="false" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
