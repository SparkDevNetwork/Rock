<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MobileCustomAdvancedSettings.ascx.cs" Inherits="RockWeb.Blocks.Mobile.MobileCustomAdvancedSettings" %>

<div class="row">
    <div class="col-md-4">
        <Rock:RockCheckBox ID="cbShowOnPhone" runat="server" Label="Show On Phone" />
    </div>

    <div class="col-md-4">
        <Rock:RockCheckBox ID="cbShowOnTablet" runat="server" Label="Show On Tablet" />
    </div>

    <div class="col-md-4">
        <Rock:RockCheckBox ID="cbRequiresNetwork" runat="server" Label="Requires Network" />
    </div>
</div>

<Rock:CodeEditor ID="ceNoNetworkContent" runat="server" Label="No Network Content" EditorMode="Xml" />
