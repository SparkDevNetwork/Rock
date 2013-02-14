<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerRequestList.ascx.cs" Inherits="RockWeb.Blocks.Prayer.PrayerRequestList" %>
<asp:UpdatePanel ID="upPrayerRequests" runat="server">
    <ContentTemplate>
        <script>
            function updateSubCategories() {
                
                var e = document.getElementById("<%= ddlPrayerCategoryFilter.ClientID %>");
                var v = e.options[e.selectedIndex].value;
                alert("The selected index for "+ "<%= ddlPrayerCategoryFilter.ClientID %>" +" is: " + v );

                var restUrl = "<%=ResolveUrl( "~/api/categories/getchildren/" ) %>";
                var selectedPrayerCategoryId = $('#<%= ddlPrayerCategoryFilter.ClientID %> option:selected').val();

                var dataList = new kendo.data.DataSource({
                    transport: {
                        read: {
                            url: function (options) {
                                alert('option was: ' + selectedPrayerCategoryId);
                                var requestUrl = restUrl + (selectedPrayerCategoryId || 0) + '/' + 'PrayerRequest'
                                return requestUrl;
                            }
                        }
                    }
                });

                var $ddlPrayerCategoryFilter = $('#<%= ddlPrayerCategoryFilter.ClientID %>');
                $ddlPrayerCategoryFilter.kendoDropDownList({
                    dataTextField: "Name",
                    dataValueField: "Id",
                    cascadeFrom: '#<%= ddlGroupCategoryFilter.ClientID %>',
                    dataSource: dataList
                });
            }
        </script>
        <asp:Panel ID="pnlLists" runat="server" Visible="true">
            <h3>Prayer Requests</h3>
            <Rock:GridFilter ID="rFilter" runat="server" OnApplyFilterClick="rFilter_ApplyFilterClick" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                <Rock:LabeledDropDownList ID="ddlGroupCategoryFilter" runat="server" LabelText="Prayer Group Category" OnTextChanged="ddlGroupCategoryFilter_TextChanged" />
                <Rock:LabeledDropDownList ID="ddlPrayerCategoryFilter" runat="server" LabelText="Prayer Category" />
                <Rock:DateTimePicker ID="dtRequestEnteredDateRangeStartDate" runat="server" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="EnteredDate" LabelText="From date" />
                <Rock:DateTimePicker ID="dtRequestEnteredDateRangeEndDate" runat="server" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="EnteredDate" LabelText="To date" />
                <Rock:LabeledCheckBox ID="cbShowApproved" runat="server" LabelText="Show approved" />
            </Rock:GridFilter>
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:Grid ID="gPrayerRequests" runat="server" AllowSorting="true" OnRowSelected="gPrayerRequests_Edit" OnRowDataBound="gPrayerRequests_RowDataBound" ShowActionExcelExport="false">
                <Columns>
                    <asp:BoundField DataField="FullName" HeaderText="Name" SortExpression="FirstName" />
                    <asp:BoundField DataField="Category.Name" HeaderText="Category" SortExpression="Category.Name" />
                    <Rock:DateField DataField="EnteredDate" HeaderText="Entered" SortExpression="EnteredDate"/>
                    <asp:BoundField DataField="Text" HeaderText="Request" SortExpression="Text" />
                    <Rock:BadgeField DataField="FlagCount" HeaderText="Flag Count" SortExpression="FlagCount" ImportantMin="4" WarningMin="2" InfoMin="1" InfoMax="2" />
                    <Rock:ToggleField DataField="IsApproved" HeaderText="Approval Status" CssClass="switch-mini" Enabled="True" OnText="yes" OffText="no" SortExpression="IsApproved" OnCheckedChanged="gPrayerRequests_CheckChanged" />
                    <Rock:DeleteField OnClick="gPrayerRequests_Delete"  />
                </Columns>
            </Rock:Grid>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
