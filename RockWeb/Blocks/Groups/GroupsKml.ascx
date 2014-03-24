<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupsKml.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupsKml" %>



<asp:Panel ID="pnlExport" runat="server">

    <Rock:RockRadioButtonList ID="rblExportFileType" RepeatDirection="Horizontal" runat="server" Label="Export File Type">
        <asp:ListItem Value="kml" Text="KML" Selected="True" />
        <asp:ListItem Value="kmz" Text="KMZ" />
    </Rock:RockRadioButtonList>
    <Rock:RockCheckBoxList ID="cblExportGeoTypes" RepeatDirection="Horizontal" runat="server" Label="Include Geography Types" Help="Select the items you would like to include on your map. Points will add the exact location of the group where as geofences will provide any boundaries defined for the groups location.">
        <asp:ListItem Value="points" Text="Points" Selected="True" />
        <asp:ListItem Value="geofences" Text="Geofences" />
    </Rock:RockCheckBoxList>
    <asp:Button ID="btnExport" runat="server" CssClass="btn btn-primary" Text="Export" OnClick="btnExport_Click" /> 
</asp:Panel>

