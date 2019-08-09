<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelListSettings.ascx.cs" Inherits="RockWeb.Blocks.Mobile.ContentChannelListSettings" %>

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

<h5>Fields</h5>
<p>
    Configure the fields you would like to display in the list control. Each field you define here will be available via the key you provide.
</p>
<hr />
<Rock:Grid ID="gIncludedAttributes" runat="server" DisplayType="Light" >
    <Columns>
        <Rock:RockBoundField DataField="FieldSource" HeaderText="Source" />
        <Rock:RockBoundField DataField="Key" HeaderText="Key" />
        <Rock:RockBoundField DataField="Value" HeaderText="Expression" />
        <Rock:RockBoundField DataField="FieldFormat" HeaderText="Format" />

        <Rock:EditField OnClick="gIncludedAttributesEdit_Click" />
        <Rock:DeleteField OnClick="gIncludedAttributesDelete_Click" />
    </Columns>
</Rock:Grid>
<asp:Panel ID="pnlDataEdit" runat="server" Visible="false">
    <asp:HiddenField ID="hfOriginalKey" runat="server" />
    <Rock:RockRadioButtonList ID="rblFieldSource" runat="server" Label="Field Source" OnSelectedIndexChanged="rblFieldSource_SelectedIndexChanged" AutoPostBack="true" RepeatDirection="Horizontal" />
    
    <asp:Panel ID="pnlProperties" runat="server">
        <Rock:RockDropDownList ID="ddlContentChannelProperties" runat="server" Label="Property" />
    </asp:Panel>

    <asp:Panel ID="pnlAttributes" runat="server">
        <div class="row">
            <div class="col-md-6"><Rock:RockDropDownList ID="ddlContentChannelAttributes" runat="server" Label="Attribute" /></div>
            <div class="col-md-6"><Rock:RockRadioButtonList ID="rblAttributeFormatType" runat="server" Label="Format Type" RepeatDirection="Horizontal" /></div>
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlLavaExpression" runat="server">
        <div class="row">
            <div class="col-md-6"><Rock:RockTextBox ID="tbKey" runat="server" Label="Key" Help="This will become the prorperty name in the returned JSON." Required="true" /></div>
            <div class="col-md-6"><Rock:RockRadioButtonList ID="rblFieldFormat" runat="server" Label="Field Format" RepeatDirection="Horizontal" /></div>
        </div>
        
        <Rock:CodeEditor id="ceLavaExpression" runat="server" Label="Lava Expression" EditorMode="Lava" Placeholder="{{ item | Attribute:'AttributeKey' }}" Help="Lava expression to use to get the value for the field. Note: The use of entity commands, SQL commands, etc are not recommended for performance reasons." />
        
    </asp:Panel>

    <asp:LinkButton ID="lbApply" runat="server" OnClick="lbApply_Click" CssClass="btn btn-primary btn-xs" Text="Apply" />
    <asp:LinkButton ID="lbCancel" runat="server" OnClick="lbCancel_Click" CssClass="btn btn-link btn-xs" Text="Cancel" />
</asp:Panel>

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


