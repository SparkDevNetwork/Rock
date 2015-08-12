<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BookManagement.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Prayerbook.BookManagement" %>

<h1>Open, Close, and Publish Books</h1>

<div class="row">
    <div class="col-md-2">
        <asp:Button ID="btnOpenNewBook" Text="Open New Book" runat="server" OnClick="btnOpenNewBook_Click"
            CssClass="btn btn-primary btn-block" />
        <p></p>
        <asp:Button ID="btnCloseBook" Text="Close Book" runat="server" OnClick="btnCloseBook_Click"
            CssClass="btn btn-primary btn-block" />
        <p></p>
        <asp:Button ID="btnReopenBook" Text="Re-open Book" runat="server" OnClick="btnReopenBook_Click"
            CssClass="btn btn-primary btn-block" />
        <p></p>
        <asp:Button ID="btnExportEntries" Text="Export Entries For Publication" runat="server" OnClick="btnExportEntries_Click"
            CssClass="btn btn-primary btn-block" />
        <p></p>
        <asp:Button ID="btnPublishBook" Text="Publish Book" runat="server" OnClick="btnPublishBook_Click"
            CssClass="btn btn-primary btn-block" />
        <p></p>
        <asp:Button ID="btnCancel" Text="Return To UP Team Book Home" runat="server" OnClick="btnCancel_Click"
            CssClass="btn btn-primary btn-block" />
    </div>
    <div class="col-md-5">
        <asp:Label Text="" ID="lblBookStatus" runat="server" /><br />
        <p></p>
        <p>
            <Rock:RockLiteral ID="litInstructions" runat="server" />
        </p>
        <asp:UpdatePanel ID="upnlOpenNewBook" runat="server" Visible="false">
            <ContentTemplate>
                <p>
                    <Rock:RockTextBox ID="txtTitle" runat="server" BorderStyle="None" ReadOnly="true" Label="Title: " />
                </p>
                <p>
                    <Rock:DatePicker ID="dpOpenDate" runat="server" Label="Open Date: " />
                </p>
                <p>
                    <Rock:DatePicker ID="dpCloseDate" runat="server" OnTextChanged="dpCloseDate_TextChanged" AutoPostBack="true" Label="Close Date: " />
                </p>
                <p>
                    <Rock:BootstrapButton Text="Save Book" ID="btnSaveBook" runat="server" OnClick="btnSaveBook_Click" CssClass="btn btn-primary" />
                </p>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</div>
