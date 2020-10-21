<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CampusServiceTimeRoomFilter.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.BarcodeAttendance.CampusServiceTimeRoomFilter" %>

<script type="text/javascript">
    (function ($) {
        function setup() {
            $("table[id$='grdGroups']").find("input[type='checkbox']").each(function (index) {
                $(this).click(function () {
                    console.log($(this).parents("tr").first());
                    javascript: __doPostBack('<%=upnlContent.ClientID %>', 'OnClick');
                    //javascript: __doPostBack($(this).parents("tr").first().prop("id").replace(/_/g,"$"), '');

                });
            });

        }
        $(document).ready(function () {
            setup();
        });
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_endRequest(function () {
            setup();
        });
    })(jQuery);
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfGroupTypesInclude" runat="server" />
        <asp:HiddenField ID="hfGroupTypesExclude" runat="server" />
        <asp:HiddenField ID="hfSelectedServiceTimes" runat="server" />
        <asp:HiddenField ID="hfSelectedLocations" runat="server" />
        <div class="grid-container">

            <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block" ViewStateMode="Enabled">

                <div id="divZeroLavaToPdfPageId" runat="server">
                    <Rock:WarningBlock ID="wbZeroLavaToPdfPageId" runat="server" Visible="false" Text="Please configure a report page to redirect to in the block settings." />
                </div>

                <%--<div id="divNoPdfToolKitInstalled" runat="server">
                    <Rock:WarningBlock ID="wbNoPdfToolkitInstalled" runat="server" Visible="false" Text="Plugin requires the Lava To PDF toolkit that is availabe from the Rock shop.  Please install the Lava to PDF toolkit in order to continue." />
                </div>--%>

                <div class="panel-body" style="width: 100%">

                    <div class="row">

                        <div class="col-lg-6">
                            <div class="row">
                                <div class="col-lg-12">
                                    <Rock:RockDropDownList runat="server" ID="ddl_GroupType" AutoPostBack="true" OnSelectedIndexChanged="ddl_GroupType_SelectedIndexChanged"
                                        DataTextField="Text" DataValueField="Value" EnableViewState="true" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-6">
                                    <Rock:RockDropDownList ID="bddlCampus" runat="server" Label="Campus" Required="false" AutoPostBack="true" OnSelectedIndexChanged="r_CampusPicker_SelectedIndexChanged" EnableViewState="true" DataTextField="Name" DataValueField="Id"/>
                                </div>
                                <div class="col-lg-6">
                                    <Rock:RockListBox runat="server" ID="rddl_ParentGroup" AutoPostBack="true" Label="Parent Groups"
                                        DataValueField="Value" DataTextField="Text" OnSelectedIndexChanged="rddl_ParentGroup_SelectedIndexChanged" SelectionMode="Multiple" EnableViewState="true" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-6">
                                    <Rock:RockListBox AutoPostBack="true" runat="server" ID="rddl_ServiceTimeSelector"
                                        Label="Schedule" DataValueField="Value" DataTextField="Text"
                                        OnSelectedIndexChanged="rddl_ServiceTimeSelector_SelectedIndexChanged"
                                        SelectionMode="Multiple" EnableViewState="true" />

                                </div>
                                <div class="col-lg-6">
                                    <Rock:RockListBox AutoPostBack="true" runat="server" ID="rddl_ClassSelector"
                                        Label="Locations" DataValueField="Value" DataTextField="Text"
                                        OnSelectedIndexChanged="rddl_ClassSelector_SelectedIndexChanged"
                                        SelectionMode="Multiple" EnableViewState="true" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-12">
                                    <Rock:BootstrapButton runat="server" ID="rbb_Filter" CssClass="btn btn-primary" Text="Filter" OnClick="rbb_Filter_Click" />
                                </div>
                            </div>
                        </div>
                        <div class="col-lg-6">
                            <div class="row">
                                <div class="col-lg-6">
                                    <Rock:RockRadioButtonList runat="server" ID="rrb_DisplayName" Help="Display name as NickName,LastName or LastName,NickName" Label="Display Name Format" AutoPostBack="true" Visible="false">
                                        <asp:ListItem Text="NickName, LastName" Value="NickName,LastName" Selected="True" />
                                        <asp:ListItem Text="LastName, NickName" Value="LastName,NickName" />
                                    </Rock:RockRadioButtonList>
                                </div>
                                <div class="col-lg-6">
                                    <Rock:RockRadioButtonList runat="server" ID="rrb_CombineClasses" Help="Prints all group on same page, or seperate pages by group" Label="Combine Classes" AutoPostBack="true">
                                        <asp:ListItem Text="Yes" Value="Yes" />
                                        <asp:ListItem Text="No" Value="No" Selected="True" />
                                    </Rock:RockRadioButtonList>
                                </div>
                            </div>

                        </div>
                    </div>
                    <div class="row">
                        <div class="col-lg-12">
                            <div class="pull-right">

                                <asp:LinkButton ID="btnGenerate" runat="server" Text="Generate" CssClass="btn btn-primary" />

                            </div>
                        </div>
                    </div>
                    <hr />
                    <div class="row">
                        <div class="col-md-12">
                            <div class="grid grid-panel">
                                <Rock:Grid ID="grdGroups" runat="server" AllowSorting="true" DataKeyNames="GroupId" ViewStateMode="Enabled">
                                    <Columns>
                                        <Rock:RockTemplateField HeaderText="Select">
                                            <HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" />
                                            <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" />
                                            <ItemTemplate>
                                                <Rock:RockCheckBox runat="server" />
                                                <asp:HiddenField runat="server" ID="GroupId" Value='<%# Eval("GroupId") %>' />
                                            </ItemTemplate>
                                        </Rock:RockTemplateField>
                                        <Rock:RockBoundField DataField="Campus" HeaderText="Campus" />
                                        <Rock:RockBoundField DataField="ParentGroupName" HeaderText="Parent Group Name" />
                                        <Rock:RockBoundField DataField="GroupName" HeaderText="Group Name" SortExpression="Name" />
                                        <Rock:RockBoundField DataField="GroupCapacity" HeaderText="Group Capacity" />
                                        <Rock:RockBoundField DataField="MeetingLocation" HeaderText="Class Rooms" />
                                        <Rock:RockBoundField DataField="ScheduledList" HeaderText="Service Times" />
                                        <Rock:BadgeField DataField="MembersCount" HeaderText="Member Count" SortExpression="Members.Count"
                                            InfoMin="1" InfoMax="1" SuccessMin="2" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="panel-footer" style="padding-top: 30px">
                </div>
            </asp:Panel>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
