<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageParameterFilter.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.Cms.PageParameterFilter" %>

<asp:UpdatePanel ID="upnlContent" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
    <ContentTemplate>

        <asp:Panel ID="pnlEditModel" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="lbSave_Click" Title="Edit Parameters">
                <Content>
                    <asp:UpdatePanel runat="server" ID="upnlEdit">
                        <ContentTemplate>
                            <asp:Panel ID="pnlEdit" runat="server">
                                <h4>Boolean Filters</h4>
                                <hr />
                                <div class="row">
                                    <div class="col-md-4">
                                        <Rock:RockTextBox ID="tbBooleanFilter" runat="server" Label="Boolean" />
                                    </div>
                                    <div class="col-md-4">
                                        <Rock:RockTextBox ID="tbBooleanFilter2" runat="server" Label="Boolean" />
                                    </div>
                                    <div class="col-md-4">
                                        <Rock:RockTextBox ID="tbBooleanFilter3" runat="server" Label="Boolean" />
                                    </div>
                                </div>
                                <h4>Date Filters</h4>
                                <hr />
                                <div class="row">
                                    <div class="col-md-4">
                                        <Rock:RockTextBox ID="tbDateRange" runat="server" Label="Date Range" />
                                    </div>
                                    <div class="col-md-4">
                                        <Rock:RockTextBox ID="tbDateRange2" runat="server" Label="Date Range" />
                                    </div>
                                    <div class="col-md-4">
                                        <Rock:RockTextBox ID="tbDateRange3" runat="server" Label="Date Range" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-4">
                                        <Rock:RockTextBox ID="tbDate" runat="server" Label="Date" />
                                    </div>
                                </div>
                                <h4>Multi Selct Filters</h4>
                                <hr />
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="tbMultiSelectLabel" runat="server" Label="Multi-Select" />
                                        <Rock:KeyValueList ID="kvMultiSelect" runat="server" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="tbMultiSelectLabel2" runat="server" Label="Multi-Select" />
                                        <Rock:KeyValueList ID="kvMultiSelect2" runat="server" />
                                    </div>
                                </div>
                                <h4>Number Range Filters</h4>
                                <hr />
                                <div class="row">
                                    <div class="col-md-4">
                                        <Rock:RockTextBox ID="tbNumberRangeLabel" runat="server" Label="Number Range" />
                                    </div>
                                    <div class="col-md-4">
                                        <Rock:RockTextBox ID="tbNumberRangeLabel2" runat="server" Label="Number Range" />
                                    </div>
                                    <div class="col-md-4">
                                        <Rock:RockTextBox ID="tbNumberRangeLabel3" runat="server" Label="Number Range" />
                                    </div>
                                </div>
                                <h4>Special Filters</h4>
                                <hr />
                                <div class="row">
                                    <div class="col-md-4">
                                        <Rock:RockDropDownList ID="ddlEnableCampuses" runat="server" Label="Enable Campuses Filter">
                                            <asp:ListItem Text="False" Value="False"></asp:ListItem>
                                            <asp:ListItem Text="True" Value="True"></asp:ListItem>
                                        </Rock:RockDropDownList>
                                    </div>
                                    <div class="col-md-4">
                                        <Rock:RockDropDownList ID="ddlEnableAccounts" runat="server" Label="Enable Accounts Filter">
                                            <asp:ListItem Text="False" Value="False"></asp:ListItem>
                                            <asp:ListItem Text="True" Value="True"></asp:ListItem>
                                        </Rock:RockDropDownList>
										<Rock:RockTextBox ID="tbAccountsTitle" runat="server" Label="Accounts Title" />
                                    </div>
									<div class="col-md-4">
                                        <Rock:RockDropDownList ID="ddlEnableAccounts2" runat="server" Label="Enable Accounts 2 Filter">
                                            <asp:ListItem Text="False" Value="False"></asp:ListItem>
                                            <asp:ListItem Text="True" Value="True"></asp:ListItem>
                                        </Rock:RockDropDownList>
										<Rock:RockTextBox ID="tbAccounts2Title" runat="server" Label="Accounts 2 Title" />
                                    </div>
									<div class="col-md-4">
                                        <Rock:RockDropDownList ID="ddlEnableAccount" runat="server" Label="Enable Account Filter">
                                            <asp:ListItem Text="False" Value="False"></asp:ListItem>
                                            <asp:ListItem Text="True" Value="True"></asp:ListItem>
                                        </Rock:RockDropDownList>
										<Rock:RockTextBox ID="tbAccountTitle" runat="server" Label="Account Title" />
                                    </div>
                                    <div class="col-md-4">
                                        <Rock:RockDropDownList ID="ddlEnablePersonPicker" runat="server" Label="Enable Person Filter">
                                            <asp:ListItem Text="False" Value="False"></asp:ListItem>
                                            <asp:ListItem Text="True" Value="True"></asp:ListItem>
                                        </Rock:RockDropDownList>
                                    </div>
                                </div>
                                <h4>Misc</h4>
                                <hr />
                                <div class="row">
                                    <div class="col-md-12">
                                        <Rock:PagePicker ID="ppRedirectPage" runat="server" Label="Redirect Page" />
                                    </div>
                                </div>
                            </asp:Panel>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h4 class="panel-title">Filters</h4>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div id="ddlIsActiveDiv" runat="server" class="col-md-6" visible="false">
                        <Rock:RockDropDownList ID="ddlIsActive" runat="server" Label="Is Active" />
                    </div>
                    <div id="ddlBoolean1Div" runat="server" class="col-md-6" visible="false">
                        <Rock:RockDropDownList ID="ddlBoolean1" runat="server" />
                    </div>
                    <div id="ddlBoolean2Div" runat="server" class="col-md-6" visible="false">
                        <Rock:RockDropDownList ID="ddlBoolean2" runat="server" />
                    </div>
                    <div id="ddlBoolean3Div" runat="server" class="col-md-6" visible="false">
                        <Rock:RockDropDownList ID="ddlBoolean3" runat="server" />
                    </div>
                    <div id="drpDateRangeDiv" runat="server" class="col-md-6" visible="false">
                        <Rock:DateRangePicker ID="drpDateRange" runat="server" />
                    </div>
                    <div id="drpDateRange2Div" runat="server" class="col-md-6" visible="false">
                        <Rock:DateRangePicker ID="drpDateRange2" runat="server" />
                    </div>
                    <div id="drpDateRange3Div" runat="server" class="col-md-6" visible="false">
                        <Rock:DateRangePicker ID="drpDateRange3" runat="server" />
                    </div>
                    <div id="dpDateDiv" runat="server" class="col-md-6" visible="false">
                        <Rock:DatePicker ID="dpDate" runat="server" />
                    </div>
                    <div id="cbMultiSelectListDiv" runat="server" class="col-md-6" visible="false">
                        <Rock:RockCheckBoxList ID="cbMultiSelectList" runat="server" RepeatDirection="Horizontal" />
                    </div>
                    <div id="cbMultiSelectList2Div" runat="server" class="col-md-6" visible="false">
                        <Rock:RockCheckBoxList ID="cbMultiSelectList2" runat="server" RepeatDirection="Horizontal" />
                    </div>
                    <div id="nrNumberRangeDiv" runat="server" class="col-md-6" visible="false">
                        <Rock:NumberRangeEditor ID="nrNumberRange" runat="server" NumberType="Currency" />
                    </div>
                    <div id="nrNumberRangeDiv2" runat="server" class="col-md-6" visible="false">
                        <Rock:NumberRangeEditor ID="nrNumberRange2" runat="server" NumberType="Currency" />
                    </div>
                    <div id="nrNumberRangeDiv3" runat="server" class="col-md-6" visible="false">
                        <Rock:NumberRangeEditor ID="nrNumberRange3" runat="server" NumberType="Currency" />
                    </div>
                    <div id="cpCampusesDiv" runat="server" class="col-md-6" visible="false">
                        <Rock:CampusesPicker ID="cpCampuses" runat="server" />
                    </div>
                    <div id="cpAcountsDiv" runat="server" class="col-md-6" visible="false">
                        <Rock:AccountPicker ID="apAccounts" runat="server" Label="Accounts" AllowMultiSelect="true" DisplayActiveOnly="true" />
                    </div>
					<div id="cpAcountsDiv2" runat="server" class="col-md-6" visible="false">
                        <Rock:AccountPicker ID="apAccounts2" runat="server" Label="Accounts" AllowMultiSelect="true" DisplayActiveOnly="true" />
                    </div>
					<div id="cpAcountDiv" runat="server" class="col-md-6" visible="false">
                        <Rock:AccountPicker ID="apAccount" runat="server" Label="Account"  DisplayActiveOnly="true" />
                    </div>
                    <div id="cpPersonDiv" runat="server" class="col-md-6" visible="false">
                        <Rock:PersonPicker ID="ppPersonPicker" runat="server" Label="Person" />
                    </div>
                </div>

                <div class="pull-right">
                    <Rock:BootstrapButton ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary" OnClick="btnFilter_Click" />
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>