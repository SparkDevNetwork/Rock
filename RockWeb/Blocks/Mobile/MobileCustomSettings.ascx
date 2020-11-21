<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MobileCustomSettings.ascx.cs" Inherits="RockWeb.Blocks.Mobile.MobileCustomSettings" %>

<div class="row">
    <div class="col-md-4">
        <Rock:NumberBox ID="nbCacheDuration" runat="server" Label="Cache Duration" Help="The number of seconds that any cachable data should be cached." />
    </div>

    <div class="col-md-4">
        <Rock:RockCheckBox ID="cbProcessLavaOnServer" runat="server" Label="Process Lava On Server" />
    </div>

    <div class="col-md-4">
        <Rock:RockCheckBox ID="cbProcessLavaOnClient" runat="server" Label="Process Lava On Client" />
    </div>
</div>

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

<Rock:CodeEditor ID="ceCssStyles" runat="server" Label="Block Scoped CSS" EditorMode="Css" Help="CSS styles that will only be applied to elements inside this block." />
