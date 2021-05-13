<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageParameterFilter.ascx.cs" Inherits="RockWeb.Blocks.Reporting.PageParameterFilter" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <%-- View Panel --%>
        <asp:Panel ID="pnlView" runat="server">
            <div class="panel panel-block">
                <div id="pnlHeading" runat="server" class="panel-heading">
                    <h4 class="panel-title">
                        <asp:Literal ID="lBlockTitleIcon" runat="server" />&nbsp;<asp:Literal ID="lBlockTitle" runat="server" /></h4>
                </div>
                <div class="panel-body">
                    <Rock:NotificationBox
                        runat="server"
                        ID="nbBuildErrors"
                        NotificationBoxType="Warning"
                        Visible="false"
                    />
                        
                    <asp:Panel runat="server" ID="pnlFilters">
                        <div class="row">
                            <div class="col-md-12">
                                <asp:HiddenField ID="hfPostBack" runat="server" />
                                <asp:PlaceHolder ID="phAttributes" runat="server" />
                            </div>
                        </div>

                        <Rock:BootstrapButton ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary" OnClick="btnFilter_Click" />
                        <asp:LinkButton ID="btnResetFilters" runat="server" Text="Reset Filters" CssClass="btn btn-default btn-link" OnClick="btnResetFilters_Click" />

                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>

        <%-- Custom Block Settings --%>
        <asp:Panel ID="pnlSettings" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdSettings" runat="server" OnSaveClick="mdSettings_SaveClick" Title="Page Parameter Filter Configuration" OnCancelScript="clearDialog();">
                <Content>


                    <fieldset>
                        <legend>Block Title Settings</legend>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="rtbBlockTitleText" runat="server" Label="Block Title Text" Help="The text to display as the block title." />
                                <Rock:RockTextBox ID="rtbBlockTitleIconCssClass" runat="server" Label="Block Title Icon CSS Class" Help="The css class name to use for the block title icon." />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBox ID="cbShowBlockTitle" runat="server" Label="Show Block Title" Help="Determines if the Block Title should be displayed" />
                            </div>
                        </div>
                    </fieldset>

                    <fieldset>
                        <legend>Filter Settings</legend>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="rtbFilterButtonText" runat="server" Label="Filter Button Text" Help="Sets the button text for the filter button." />
                                <Rock:RockDropDownList ID="ddlFilterButtonSize" Label="Filter Button Size" runat="server">
                                    <asp:ListItem Text="Normal" Value="1" />
                                    <asp:ListItem Text="Small" Value="2" />
                                    <asp:ListItem Text="Extra Small" Value="3" />
                                </Rock:RockDropDownList>
                                <Rock:NumberBox ID="nbFiltersPerRow" runat="server" Label="Filters Per Row" Help="The number of filters to have per row.  Maximum is 12." MaximumValue="12" MinimumValue="0" />
                            </div>
                            <div class="col-md-6">
                                <Rock:PagePicker ID="ppRedirectPage" runat="server" Label="Redirect Page" Help="If set, the filter button will redirect to the selected page." />
                                <Rock:RockCheckBox ID="cbShowResetFiltersButton" runat="server" Label="Show Reset Filters Button" Help="Determines if the Reset Filters button should be displayed." />
                                <Rock:RockDropDownList ID="ddlSelectionAction" Label="Selection Action" runat="server" Help="Specifies what should happen when a value is changed. Nothing, update page the change will be visible to all blocks on the page, or update block the change will only be visible to the page parameter filter block.">
                                    <asp:ListItem Text="" Value="0" />
                                    <asp:ListItem Text="Update Block" Value="1" />
                                    <asp:ListItem Text="Update Page" Value="2" />
                                </Rock:RockDropDownList>
                                <Rock:RockCheckBox ID="cbHideFilterActions" runat="server" Label="Hide Filter Actions" Help="Hides the filter buttons. This is useful when the Selection action is set to reload the page. Be sure to use this only when the page re-load will be quick." />
                            </div>
                        </div>
                    </fieldset>

                    <div class="grid grid-panel">
                        <legend>Filters</legend>
                        <Rock:Grid ID="gFilters" runat="server" AllowSorting="false" RowItemText="filter" TooltipField="Description" OnGridReorder="gFilters_GridReorder" OnRowDataBound="gFilters_RowDataBound" DisplayType="Light" ShowActionsInHeader="false">
                            <Columns>
                                <Rock:ReorderField Visible="true" />
                                <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                                <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                <Rock:RockBoundField DataField="FieldType" HeaderText="Filter Type" />
                                <Rock:RockTemplateField ID="rtDefaultValue">
                                    <HeaderTemplate>Default Value</HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:Literal ID="lDefaultValue" runat="server"></asp:Literal>
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:EditField OnClick="gFilters_Edit" />
                                <Rock:SecurityField TitleField="Name" />
                                <Rock:DeleteField OnClick="gFilters_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </Content>

            </Rock:ModalDialog>
        </asp:Panel>

        <Rock:ModalDialog ID="mdFilter" runat="server" Title="Filter" SaveButtonText="Save">
            <Content>
                <Rock:AttributeEditor ID="edtFilter" runat="server" ShowActions="false" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
