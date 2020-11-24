<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CalendarLavaCust.ascx.cs" Inherits="com_bemaservices.Event.CalendarLava" %>



<style>
.filter-menu .panel-heading {
 padding:0px 15px;
 font-size:14px;
}

.filter-menu .panel-heading h4 {
font-size:14px;
}


.checkbox input[type="checkbox"] {
    position: inherit;
    margin-left: 0px;
    margin-right: 14px;
    margin-bottom: 8px;
    display:none;
}

.radio label, .checkbox label {
    padding:0px;
}

.checkbox {
    display: inline-block;
    margin-left: 0px !important;
}

.checkbox label, .btn-event  {
    padding: 6px 14px;
    border:2px solid #f1f1f1;
    border-radius: 4px;
    margin-right: 10px;
    margin-top: 4px; 
    font-weight: 700;
    font-size: 14px;
    background:none;
    color:#444;
    
}

    .btn-event.active {
        background:#428bca;
        box-shadow:none;
        color:#fff;
        border:2px solid #428bca;
    }

    .checkedbg {
        background-color: #428bca !important;
        color:#fff !important;
        border:2px solid #428bca !important;
    }

label.checkbox {
    border: #3f4c63 1px solid;
}

.inline-block-float {
    display:inline-block;
    float:left;
}

.form-control.input-width-md, .input-group.input-width-md, .form-control-group.input-width-md input {
    width: 240px;
}

.Filters.section {
padding-top: 40px;
    padding-bottom: 40px;
}

</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="cblCampus" />
        <asp:AsyncPostBackTrigger ControlID="cblCategory" />
          <asp:AsyncPostBackTrigger ControlID="ddlCatPicker" />
        <asp:AsyncPostBackTrigger ControlID="ddlCommunityPicker" />
    </Triggers>
    <ContentTemplate>
	
        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />
		 
    
        <asp:Panel id="pnlDetails" runat="server" CssClass="Small Groups Filters section">
			
			<div id="group-filters" class="container">
			
            <asp:Panel ID="pnlFilters" CssClass="hidden-print " runat="server">

                <asp:Panel ID="pnlCalendar" CssClass="calendar" runat="server">
                    <asp:Calendar ID="calEventCalendar" runat="server" DayNameFormat="FirstLetter" SelectionMode="Day" BorderStyle="None"
                        TitleStyle-BackColor="#ffffff" NextPrevStyle-ForeColor="#333333" FirstDayOfWeek="Sunday" Width="100%" CssClass="calendar-month" OnSelectionChanged="calEventCalendar_SelectionChanged" OnDayRender="calEventCalendar_DayRender" OnVisibleMonthChanged="calEventCalendar_VisibleMonthChanged">
                        <DayStyle CssClass="calendar-day" />
                        <TodayDayStyle CssClass="calendar-today" />
                        <SelectedDayStyle CssClass="calendar-selected" BackColor="Transparent" />
                        <OtherMonthDayStyle CssClass="calendar-last-month" />
                        <DayHeaderStyle CssClass="calendar-day-header" />
                        <NextPrevStyle CssClass="calendar-next-prev" />
                        <TitleStyle CssClass="calendar-title" />
                    </asp:Calendar>
                </asp:Panel>

               
                 <div class="col-md-4 margin-b-md hidden">
						   <label class="control-label clearfix" >Filter by Date</label>
						  <div class="hidden-print clearfix margin-b-md" role="group">
							
						<Rock:BootstrapButton ID="btnDay" runat="server" CssClass="btn btn-event" Text="Day" OnClick="btnViewMode_Click" />
						<Rock:BootstrapButton ID="btnWeek" runat="server" CssClass="btn btn-event" Text="Week" OnClick="btnViewMode_Click" />
						<Rock:BootstrapButton ID="btnMonth" runat="server" CssClass="btn btn-event" Text="Month" OnClick="btnViewMode_Click" />
						<Rock:BootstrapButton ID="btnYear" runat="server" CssClass="btn btn-event" Text="Year" OnClick="btnViewMode_Click" />
					</div>
					 <label class="control-label clearfix hidden" >Calendar Type</label>
					  <div class=" hidden-print clearfix " role="group">
						   
						<Rock:BootstrapButton ID="btnList" runat="server" CssClass="btn btn-event" Text="List" OnClick="btnViewFeaturedMode_Click" />
						<Rock:BootstrapButton ID="btnFeatured" runat="server" CssClass="btn btn-event" Text="Featured" OnClick="btnViewFeaturedMode_Click" />
			   
					</div>
                </div>
				 <div class="row margin-b-md">
					<div class="col-md-4">
                        <Rock:DefinedValuePicker runat="server" ID="ddlCatPicker" DefinedTypeId="16" OnSelectedIndexChanged="ddlCatPicker_SelectedIndexChanged" AutoPostBack="true" />
                    </div>
                    <div class="col-md-4">
                        <Rock:DefinedValuePicker runat="server" ID="ddlCommunityPicker" DefinedTypeId="66" OnSelectedIndexChanged="ddlCommunityPicker_SelectedIndexChanged" AutoPostBack="true" />
                    </div>
					 <div class="col-md-4">
                        <Rock:DefinedValuePicker runat="server" ID="ddlTopicPicker" DefinedTypeId="88" OnSelectedIndexChanged="ddlTopicPicker_SelectedIndexChanged" AutoPostBack="true" />
                    </div>
                </div>
                <div id="campus-filter" class="col-md-4 filter-menu"> <%--Added campus-filter id - JM - 8/3/2018--%>
                <% if ( CampusPanelOpen || CampusPanelClosed )
                  { %>
                    <div class="panel panel-default">
                        <div class="panel-heading">
                            <h4 class="panel-title">
                                <a role="button" class="btn-block" data-toggle="collapse" href="#collapseOne">Campus Filter
                                </a>
                            </h4>
                        </div>
                        <div id="collapseOne" class='<%= CampusPanelOpen ? "panel-collapse collapse in" : "panel-collapse collapse out" %>'>
                            <div class="panel-body">
                <% } %>

                                <%-- Note: RockControlWrapper/Div/CheckboxList is being used instead of just a RockCheckBoxList, because autopostback does not currently work for RockControlCheckbox--%>
                                <Rock:RockControlWrapper ID="rcwCampus" runat="server" Label="Filter by Campus">
                                    <div class="controls">
                                        <asp:CheckBoxList ID="cblCampus" RepeatDirection="Vertical" runat="server" DataTextField="Name" DataValueField="Id"
                                            OnSelectedIndexChanged="cblCampus_SelectedIndexChanged" AutoPostBack="true" />
                                    </div>
                                </Rock:RockControlWrapper>

                <% if ( CampusPanelOpen || CampusPanelClosed )
                    { %>
                            </div>
                        </div>
                    </div>
                <% } %>

                </div>
                <div class="col-md-4 filter-menu">

                <% if ( CategoryPanelOpen || CategoryPanelClosed )
                   { %>
                    <div class="panel panel-default">
                        <div class="panel-heading">
                            <h4 class="panel-title">
                                <a role="button" class="btn-block" data-toggle="collapse" href="#collapseTwo">Category Filter
                                </a>
                            </h4>
                        </div>
                        <div id="collapseTwo" class='<%= CategoryPanelOpen ? "panel-collapse collapse in" : "panel-collapse collapse out" %>'>
                            <div class="panel-body">
                <% } %>

                                <Rock:RockControlWrapper ID="rcwCategory" runat="server" Label="Filter by Category">
                                    <div class="controls">
                                        <asp:CheckBoxList ID="cblCategory" RepeatDirection="Vertical" runat="server" DataTextField="Value" DataValueField="Id" OnSelectedIndexChanged="cblCategory_SelectedIndexChanged" AutoPostBack="true" />
                                    </div>
                                </Rock:RockControlWrapper>

                <% if ( CategoryPanelOpen || CategoryPanelClosed )
                    { %>
                            </div>
                        </div>
                    </div>
                <% } %>
                </div>
 <div class="row">
                <div class="col-md-8 col-md-offset-2">
                 <Rock:DateRangePicker ID="drpDateRange" runat="server" Label="" CssClass="inline-block-float" />
                 <asp:Button ID="lbDateRangeRefresh" runat="server" CssClass="btn btn-primary" Text="Filter by Date" OnClick="lbDateRangeRefresh_Click" />
                </div>
</div>
               

			</div>
			</div>
            </asp:Panel>

            <asp:Panel ID="pnlList" CssClass="" runat="server">
				<div class=" section gray-lighter " style="background-color: #F0EFEE;">
					<div class="block-content">
						<asp:Literal ID="lOutput" runat="server"></asp:Literal>
						<asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>
					</div>
				</div>
            </asp:Panel>

        </asp:Panel>
		

	
    </ContentTemplate>
</asp:UpdatePanel>
