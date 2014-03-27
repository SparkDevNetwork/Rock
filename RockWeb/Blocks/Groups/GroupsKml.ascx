<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupsKml.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupsKml" %>

<div class="banner">
    <h1><asp:Literal ID="lTitle" runat="server" /></h1>
</div>

<ul class="nav nav-pills nav-pagelist">
        <li id="liPillExport" runat="server" class="active">
            <asp:LinkButton ID="btnPillExport" runat="server" Text="Export" OnClick="btnPillExport_Click" />
        </li>
        <li id="liPillImport" runat="server">
            <asp:LinkButton ID="btnPillImport" runat="server" Text="Import" OnClick="btnPillImport_Click" />
        </li>
</ul>

<asp:Panel ID="pnlExport" runat="server">

    <Rock:RockDropDownList ID="ddlExportGroupTypes" CausesValidation="false" Label="Group Types" runat="server" OnSelectedIndexChanged="ddlExportGroupTypes_SelectedIndexChanged" AutoPostBack="true" />
    <Rock:RockDropDownList ID="ddlExportGroupLocationType" CausesValidation="false" Label="Group Location Types" runat="server" Enabled="false" OnSelectedIndexChanged="ddlExportGroupLocationType_SelectedIndexChanged" AutoPostBack="true" />
    <Rock:RockRadioButtonList ID="rblExportGeoTypes" RepeatDirection="Horizontal" runat="server" Label="Include Geography Types" Help="Select the items you would like to include on your map. Points will add the exact location of the group where as geofences will provide any boundaries defined for the groups location.">
        <asp:ListItem Value="points" Text="Points" Selected="True" />
        <asp:ListItem Value="geofences" Text="Geofences" />
    </Rock:RockRadioButtonList>
    <Rock:RockRadioButtonList ID="rblExportFileType" RepeatDirection="Horizontal" runat="server" Label="Export File Type">
        <asp:ListItem Value="kml" Text="KML" Selected="True" />
        <asp:ListItem Value="kmz" Text="KMZ" />
    </Rock:RockRadioButtonList>

    <asp:Button ID="btnExport" runat="server" Enabled="false" CssClass="btn btn-primary" Text="Export" OnClick="btnExport_Click" /> 
</asp:Panel>

<asp:Panel ID="pnlImport" Visible="false" runat="server">
    <Rock:FileUploader ID="fuprImportFile" runat="server"  IsBinaryFile="false" RootFolder="~/App_Data/TemporaryFiles"  OnFileUploaded="fuprImportFile_FileUploaded" />

    <asp:Literal ID="lMessages" runat="server" />
</asp:Panel>

