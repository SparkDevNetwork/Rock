﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelListSettings.ascx.cs" Inherits="RockWeb.Blocks.Mobile.ContentChannelListSettings" %>

<div class="row">
    <div class="col-md-6">
        <Rock:RockDropDownList ID="ddlContentChannel" runat="server" Label="Content Channel" EnhanceForLongLists="true" OnSelectedIndexChanged="ddlContentChannel_SelectedIndexChanged" AutoPostBack="true" />
    </div>
    <div class="col-md-6">
        <Rock:PagePicker ID="ppDetailPage" runat="server" Label="Detail Page" Help="The page to redirect to when selecting an item." />
    </div>
</div>

<div class="row">
    <div class="col-md-6">
        <Rock:NumberBox ID="nbPageSize" runat="server" Label="Page Size" Help="The number of results to return per page." />
    </div>

    <div class="col-md-6">
        <Rock:RockCheckBox ID="cbIncludeFollowing" runat="server" Label="Include Following Data" Help="Determines if following data should be appended to the dataset. This will determine if the current person is following each content channel item. This does decrease performance a bit to query and append this data." />
    </div>
</div>

<div class="row">
    <div class="col-md-6">
        <Rock:RockCheckBox ID="cbCheckItemSecurity" runat="server" Label="Check Item Security" Help="Determines if security should be check on each returned item to see if the current person has View rights. This check can slow down the results, especially with channels with lots of items." />
    </div>

    <div class="col-md-6">
        <Rock:RockCheckBox ID="cbShowChildrenOfParent" runat="server" Label="Show Children of Parent" Help="If enabled the block will look for a passed ParentItemId parameter and if found filter for children of this parent item." />
    </div>
</div>

<Rock:JsonFieldsBuilder ID="jfBuilder" runat="server" Label="Additional Fields" Help="Configure the fields you would like to display in the list control. Each field you define here will be available via the key you provide." />

<h5>Filter</h5>
    <asp:HiddenField ID="hfDataFilterId" runat="server" />
    <asp:PlaceHolder ID="phFilters" runat="server"></asp:PlaceHolder>

    <div class="row">
        <div class="col-md-6">
            <Rock:RockCheckBox ID="cbQueryParamFiltering" runat="server" Label="Enable Query/Route Parameter Filtering" Text="Yes" Help="Enabling this option will allow results to be filtered further by any query string our route parameters that are included. This includes item properties or attributes." />
        </div>
        <div class="col-md-6">
            <Rock:KeyValueList ID="kvlOrder" runat="server" Label="Order Items By" KeyPrompt="Field" ValuePrompt="Direction" Help="The field value and direction that items should be ordered by." />
        </div>
    </div>


