<%@ Control AutoEventWireup="true" CodeFile="QuickCreate.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.eSpace.QuickCreate" Language="C#" %>

 <script type="text/javascript">
    (function ($) {
        function setup() {
            var quantityDivs = $("#rptResourceQty").find("span.quantity-span");
            
            quantityDivs.each(function (i) {
                var value = $(this).attr("data-value");
                var node = $('#<%=treeResources.ClientID%>').find("a[title='" + value + "']").first();
                $(this).insertAfter(node);
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

<asp:UpdatePanel ID="upnlEntry" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfEventId" runat="server" />
        <asp:HiddenField ID="hfScheduleId" runat="server" />
        <asp:HiddenField ID="hfErrorMessage" runat="server" />
        <div class="panel panel-block">
            <div class="panel-heading">
                <h4 class="panel-title"><i class="fa fa-calendar-o"></i>&nbsp;eSPACE Quick Event</h4>
                <a href="https://app.espace.cool" target="_blank" ><img class="pull-right" src="/Plugins/com_bemaservices/eSpace/Assets/eSpace_Logo.png" id="imgLogo" /></a>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbAlert" runat="server" Visible="false"/>
                <!-- Alert Box -->
                <asp:Panel ID="pnlAPIEntry" runat="server" Visible="false">
                    
                    <Rock:RockTextBox ID="tbAPIUsername" runat="server" Label="eSPACE Username" Visible="false" TextMode="Email" />
                    <Rock:RockTextBox ID="tbAPIPassword" runat="server" Label="eSPACE Password" Visible="false" TextMode="Password" />
                    <Rock:BootstrapButton ID="btnAPIKeySave" runat="server" Text="Save Credentials" Visible="false" CssClass="btn btn-primary pull-right" OnClick="btnAPIKeySave_Click" />
                </asp:Panel>

                <asp:Panel ID="pnlEntry" runat="server" CssClass="well">
                    <h3>Create Event</h3>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbEventName" runat="server" Label="Event Name" Required="true" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbDescription" runat="server" Label="Description" TextMode="MultiLine" />
                        </div>
                    </div>
                    <hr />
                        <h4>Date/Time</h4>
                    <div class="row">
                        <div class="col-md-2">
                            <Rock:RockCheckBox ID="cbAllDayEvent" runat="server" Label="All Day Event" OnCheckedChanged="cbAllDayEvent_CheckedChanged" Checked="false" AutoPostBack="true" />
                        </div>
                        <div class="col-md-5">
                            <Rock:DatePicker ID="dateEventStart" runat="server" Label="Event Start" Required="true" AutoPostBack="true" HighlightToday="true" OnTextChanged="dateEventStart_TextChanged" />
                               
                            <Rock:TimePicker ID="timeEventStart" runat="server" Required="true"  />
                        </div>
                        <div class="col-md-5">
                            <Rock:DatePicker ID="dateEventEnd" runat="server" Label="Event End" Required="true" HighlightToday="true" />
                            <Rock:TimePicker ID="timeEventEnd" runat="server" Required="true" />
                        </div>
                    </div>
                        <h4>Setup/Teardown</h4>
                    <div class="row">
                        <div class="col-md-2"></div>
                        <div class="col-md-5">
                            <Rock:DateTimePicker ID="dtpSetupStart" runat="server" Label="Begin Setup Date/Time" />
                        </div>
                        <div class="col-md-5">
                            <Rock:DateTimePicker ID="dtpSetupEnd" runat="server" Label="End Teardown Date/Time" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-4">
                            <Rock:ScheduleBuilder ID="scheduleBuilder" runat="server" Label="Schedule Recurrence" ShowDuration="false" OnLoad="scheduleBuilder_Load" OnSaveSchedule="scheduleBuilder_SaveSchedule" />
                        </div>
                        <div class="col-md-4">
                            <Rock:RockCheckBox ID="cbIsPublic" runat="server" Label="Is Public" />
                        </div>
                        <div class="col-md-4">
                            <Rock:NumberBox ID="numNumberOfPeople" runat="server" Label="Number of People" />
                        </div>
                    </div>
                    <hr />
                    <h4>Locations and Categories</h4>
                    <div class="row" >
                        <div class="col-md-6">
                            <Rock:RockListBox ID="cblLocations" runat="server" Label="Locations" Required="true" DataTextField="Name" DataValueField="Id" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockListBox ID="cblCategories" runat="server" Label="Categories" DataTextField="Name" DataValueField="Id" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:BootstrapButton ID="btnEntryNext" runat="server" Text="Next" CssClass="btn btn-primary pull-right" CausesValidation="false" OnClick="btnEntryNext_Click" />
                            <Rock:BootstrapButton ID="btnEntryCancel" runat="server" Text="Cancel" CssClass="btn btn-default pull-right" CausesValidation="false" />
                        </div>
                    </div>
                    
                </asp:Panel>

                <asp:Panel ID="pnlSpacesResourcesServices" runat="server" CssClass="well" Visible="false">
                    <h4>Add Items</h4>
                    <ul class="nav nav-tabs" role="tablist" style="word-wrap: break-word;">
                        <li role="presentation" class="active"><a href="#spaces" aria-controls="spaces" role="tab" data-toggle="tab">Spaces</a></li>
                        <li role="presentation"><a href="#resources" aria-controls="resources" role="tab" data-toggle="tab">Resources</a></li>
                        <li role="presentation"><a href="#services" aria-controls="services" role="tab" data-toggle="tab">Services</a></li>
                    </ul>
                    <div class="tab-content">
                        <div role="tabpanel" class="tab-pane active" id="spaces">
                            <h3 class="text-center">Spaces</h3>
                            <asp:TreeView ID="treeSpaces" runat="server" ShowCheckBoxes="All" ShowExpandCollapse="true" ></asp:TreeView>
                            <%--<Rock:BootstrapButton ID="btnAddSpaces" runat="server" CssClass="btn btn-block btn-default btn-sm fa fa-plus-square" CausesValidation="false" OnClick="btnAddSpaces_Click" > Add Space</Rock:BootstrapButton>
                            <asp:Repeater ID="rptSpaces" runat="server" OnItemDataBound="rptSpaces_ItemDataBound" >
                                <HeaderTemplate><table></HeaderTemplate>
                                <ItemTemplate>
                                    <tr>
                                        <td>
                                            <Rock:RockDropDownList ID="ddlSpacesItem" runat="server" DataTextField="Name" DataValueField="Id" OnSelectedIndexChanged="ddlSpacesItem_SelectedIndexChanged" AutoPostBack="true"/>
                                        </td>
                                        <td class="text-center" width="8%">
                                            <Rock:BootstrapButton ID="delete" runat="server" OnClick="spaces_delete_click"><i class="fa fa-times"></i></Rock:BootstrapButton>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                                <FooterTemplate></table></FooterTemplate>
                            </asp:Repeater>--%>
                        </div>
                        <div role="tabpanel" class="tab-pane" id="resources">
                            <h3 class="text-center">Resources</h3>
                            <asp:TreeView ID="treeResources" runat="server" ShowCheckBoxes="All" ShowExpandCollapse="true" >
                            </asp:TreeView>
                            <asp:Repeater ID="rptResourceQty" runat="server" >
                                <HeaderTemplate>
                                    <div id="rptResourceQty">
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <span class="quantity-span" data-value='<%# Eval( "Id" ) %>'>
                                        <%--<Rock:NumberBox ID="nbQuantity" runat="server" AppendText='<%# "On Hand: " + Eval("QuantityOnHand") %>' CssClass="ResourceNumberBox" Width="60px" />--%>
                                        
                                        <asp:TextBox runat="server" ID="nbQuantity" Width="60px" TextMode="Number" />
                                        <%# "On Hand: " + Eval("QuantityOnHand") %>
                                        <asp:HiddenField runat="server" ID="hfResourceId" Value='<%# Eval( "Id" ) %>' />
                                    </span>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </div>
                                </FooterTemplate>
                            </asp:Repeater>
                            <%--<Rock:BootstrapButton ID="btnAddResources" runat="server" CssClass="btn btn-block btn-default btn-sm fa fa-plus-square" CausesValidation="false" OnClick="btnAddResources_Click"> Add Resource</Rock:BootstrapButton>
                            <asp:Repeater ID="rptResources" runat="server" OnItemDataBound="rptResources_ItemDataBound">
                                <HeaderTemplate><table></HeaderTemplate>
                                <ItemTemplate>
                                    <tr>
                                        <td>
                                            <Rock:RockDropDownList ID="ddlResourcesItem" runat="server" DataTextField="Name" DataValueField="Id" OnSelectedIndexChanged="ddlResourcesItem_SelectedIndexChanged" AutoPostBack="true"/>
                                        </td>
                                        <td width="18%">
                                            <Rock:NumberBox ID="nbQuantity" runat="server" Placeholder='<%# "Max: " + Eval("QuantityOnHand") %>' OnTextChanged="nbQuantity_TextChanged" AutoPostBack="true" />
                                        </td>
                                        <td class="text-center" width="8%">
                                            <Rock:BootstrapButton ID="delete" runat="server" OnClick="resources_delete_click"><i class="fa fa-times"></i></Rock:BootstrapButton>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                                <FooterTemplate></table></FooterTemplate>
                            </asp:Repeater>--%>
                        </div>
                        <div role="tabpanel" class="tab-pane" id="services">
                            <h3 class="text-center">Services</h3>
                            <asp:TreeView ID="treeServices" runat="server" ShowCheckBoxes="All" ShowExpandCollapse="true" ></asp:TreeView>
                            <%--<Rock:BootstrapButton ID="btnAddServices" runat="server" CssClass="btn btn-block btn-default btn-sm fa fa-plus-square" CausesValidation="false" OnClick="btnAddServices_Click"> Add Service</Rock:BootstrapButton>
                            --%>
                            <%--<asp:Repeater ID="rptServices" runat="server" OnItemDataBound="rptServices_ItemDataBound">
                                <HeaderTemplate><table></HeaderTemplate>
                                <ItemTemplate>
                                    <tr>
                                        <td>
                                            <Rock:RockDropDownList ID="ddlServicesItem" runat="server" DataTextField="Name" DataValueField="Id" OnSelectedIndexChanged="ddlServicesItem_SelectedIndexChanged" AutoPostBack="true"/>
                                        </td>
                                        <td class="text-center" width="8%">
                                            <Rock:BootstrapButton ID="delete" runat="server" OnClick="services_delete_click"><i class="fa fa-times"></i></Rock:BootstrapButton>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                                <FooterTemplate></table></FooterTemplate>
                            </asp:Repeater>--%>
                         </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:BootstrapButton ID="btnItemsSubmit" runat="server" Text="Submit" CssClass="btn btn-primary pull-right" CausesValidation="false" OnClick="btnItemsSubmit_Click" />
                            <Rock:BootstrapButton ID="btnItemsCancel" runat="server" Text="Back" CssClass="btn btn-default pull-right" CausesValidation="false" OnClick="btnItemsCancel_Click" />
                        </div>
                    </div>

                    <hr />
                </asp:Panel>
                <asp:Panel ID="pnlSubmitted" runat="server" CssClass="well" Visible="false">
                    <h4>Event Submitted</h4>
                    <Rock:RockLiteral ID="submitLiteral" runat="server" />
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:BootstrapButton ID="btnNew" runat="server" Text="Another Event" CssClass="btn btn-primary pull-right" CausesValidation="false" OnClick="btnNew_Click" />
                            <%--<Rock:BootstrapButton ID="btnDetails" runat="server" Text="See in eSPACE" CssClass="btn btn-default pull-right" CausesValidation="false" />--%>
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>