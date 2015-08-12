<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EditEntry.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Prayerbook.EditEntry" %>

<asp:HiddenField runat="server" ID="hidEntryId" Visible="false" />
<asp:HiddenField runat="server" ID="hidBookId" Visible="false" />

<asp:UpdatePanel runat="server" ID="upnlContent">
    <ContentTemplate>
        <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
        <div class="row">
            <div class="col-md-3">
                <Rock:RockLiteral ID="lName" runat="server" Label="Name"></Rock:RockLiteral>
                <Rock:RockDropDownList ID="ddlContributors" runat="server" Label="Name" DataValueField="Id" DataTextField="FullName" AutoPostBack="true" CausesValidation="false" SourceTypeName="Rock.Model.Person, Rock" OnSelectedIndexChanged="ddlContributors_SelectedIndexChanged" Required="true" />
            </div>
            <div class="col-md-3">
                <Rock:RockLiteral ID="lSpouseName" runat="server" Label="Spouse"></Rock:RockLiteral>
            </div>
        </div>
        <div class="row">
            <div class="col-md-3">
                <Rock:RockDropDownList ID="ddlMinistry" runat="server" Label="Ministry" DataValueField="Id" DataTextField="Value" CausesValidation="false" SourceTypeName="Rock.Model.DefinedValue, Rock" />
            </div>
            <div class="col-md-3">
                <Rock:RockDropDownList ID="ddlSubministry" runat="server" Label="Subministry" DataValueField="Id" DataTextField="Value" CausesValidation="false" SourceTypeName="Rock.Model.DefinedValue, Rock" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-6">
                <Rock:RockTextBox ID="dtbPraise1" runat="server" Label="Praise 1" CausesValidation="false" Rows="4" TextMode="MultiLine" Required="true" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-6">
                <Rock:RockTextBox ID="dtbPersonalRequest1" runat="server" Label="Personal Request 1" CausesValidation="false" Rows="4" TextMode="MultiLine" Required="true" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-6">
                <Rock:RockTextBox ID="dtbPersonalRequest2" runat="server" Label="PersonalRequest 2" CausesValidation="false" Rows="4" TextMode="MultiLine" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-6">
                <Rock:RockTextBox ID="dtbMinistryNeed1" runat="server" Label="Ministry Need 1" CausesValidation="false" Rows="4" TextMode="MultiLine" Required="true" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-6">
                <Rock:RockTextBox ID="dtbMinistryNeed2" runat="server" Label="Ministry Need 2" CausesValidation="false" Rows="4" TextMode="MultiLine" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-6">
                <Rock:RockTextBox ID="dtbMinistryNeed3" runat="server" Label="Ministry Need 3" CausesValidation="false" Rows="4" TextMode="MultiLine" />
            </div>
        </div>
        <div class="container-fluid">
            <div class="row">
                <div class="col-md-1">
                    <Rock:BootstrapButton ID="btnSave" runat="server" Text="Save Entry" OnClick="btnSave_Click" CssClass="btn btn-primary" />
                </div>
                <div class="col-md-1">
                    <Rock:BootstrapButton ID="btnDelete" runat="server" Text="Delete Entry" OnClick="btnDelete_Click" CssClass="btn btn-default" CausesValidation="false" />
                </div>
                <div class="col-md-1">
                    <Rock:BootstrapButton ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" CssClass="btn btn-link" CausesValidation="false" />
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
