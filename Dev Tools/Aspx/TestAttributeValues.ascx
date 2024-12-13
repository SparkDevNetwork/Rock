﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TestAttributeValues.ascx.cs" Inherits="RockWeb.Blocks.Examples.TestAttributeValues" %>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <Rock:RockDropDownList ID="ddlMethod" runat="server" Label="Test Method" AutoPostBack="true" OnSelectedIndexChanged="ddlMethod_SelectedIndexChanged">
            <asp:ListItem Selected="True" Value="1" Text="Single Entity Type" />
            <asp:ListItem Value="2" Text="All Entity Types" />
        </Rock:RockDropDownList>

        <Rock:EntityTypePicker ID="etPicker" runat="server" Label="Starting Entity Type" Help="Entity types are processed alphabetically, this specifies where to start" />

        <asp:LinkButton ID="btnRun" runat="server" OnClick="btnRun_Click" Text="Run" CssClass="btn btn-primary" />

        <div class="mt-4">Status:</div>
        <div>
            <Rock:TaskActivityProgressReporter ID="tapReporter" runat="server" />
            <asp:Literal ID="ltMessage" runat="server" />
        </div>
    </ContentTemplate>
</asp:UpdatePanel>