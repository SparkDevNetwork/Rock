<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ServiceUEvents.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.ServiceUEvents.ServiceUEvents" %>

<h1>Event Calendar</h1>

<%--Search box--%>

<asp:HiddenField ID="hdnEventId" runat="server" />
<asp:HiddenField ID="hdnCampus" runat="server" />

<div class="container nopadding">
    <div class="col-xs-12 col-md-8">
        <input type="text" id="calendar-search" name="calendar-search" placeholder="Search for Events Here" class="element text large" />
    </div>
    <div class="col-xs-12 col-md-4"></div>
    <div id="CalendarFilters">

        <div id="CampusFilterButtons" class="hidden-xs col-md-6">
            <h4>Choose a campus</h4>
            <asp:Repeater runat="server" ID="rptCampuses">
                <ItemTemplate>
                    <asp:HyperLink runat="server" ID="lnk" OnDataBinding="lnk_DataBinding"></asp:HyperLink>
                </ItemTemplate>
            </asp:Repeater>
            <a href="#" class="btn btn-default btn-block-xs campus-all-hover" data-campuscode="ALL">ALL</a>
        </div>

        <div id="CampusFilterDropdown" class="col-xs-12 hidden-sm hidden-md hidden-lg">
            <h4>Choose a campus</h4>
            <asp:DropDownList runat="server" ID="ddlCampusDropdown" CssClass="form-control"></asp:DropDownList>
        </div>

        <div id="CategoryFilterButtons" class="hidden-xs col-md-6">
            <h4>Choose a category</h4>
            <a href="#" class="btn btn-default category-cm-hover" data-categorycode="cm">Kids</a>
            <a href="#" class="btn btn-default category-sm-hover" data-categorycode="sm">Students</a>
            <a href="#" class="btn btn-default category-g-hover" data-categorycode="g">Adults</a><br />
            <a href="#" class="btn btn-default category-ae-hover" data-categorycode="ae">Everything Else</a>
            <a href="#" class="btn btn-default category-all-hover" data-categorycode="all">ALL</a>
        </div>

        <div id="CategoryFilterDropdown" class="col-xs-12 hidden-sm hidden-md hidden-lg">
            <h4>Choose a category</h4>
            <asp:DropDownList ID="ddlCategoryDropdown" CssClass="form-control" runat="server">
                <asp:ListItem Text="ALL" Value="all" />
                <asp:ListItem Text="Kids" Value="cm" />
                <asp:ListItem Text="Students" Value="sm" />
                <asp:ListItem Text="Adults" Value="g" />
                <asp:ListItem Text="Everything Else" Value="ae" />
            </asp:DropDownList>
        </div>

    </div>
</div>


<div class="row" style="margin-bottom: 10px;">
    <div class="col-md-4 col-md-push-8 top-cal">
        <%--<div class="col-md-12 hidden-xs hidden-sm col-spacer"></div>--%>
        <h2 class="text-center" id="event-date">Event Details</h2>
        <div id="events-list" data-key="na"><i class="fa fa-spinner fa-spin fa-3x text-center"></i></div>
        <div id="event-description"></div>
    </div>
    <div class="col-md-8 col-md-pull-4">

        <div class="page-header">


            <div class="pull-right form-inline">
                <div class="btn-group">
                    <button class="btn btn-primary" data-calendar-nav="prev" style="margin-right: 10px;"><< Prev</button>

                    <button class="btn btn-primary" data-calendar-nav="next">Next >></button>
                </div>
                <%--                <div class="btn-group">
                    <button class="btn btn-warning" data-calendar-view="year">Year</button>
                    <button class="btn btn-warning active" data-calendar-view="month">Month</button>
                    <button class="btn btn-warning" data-calendar-view="week">Week</button>
                    <button class="btn btn-warning" data-calendar-view="day">Day</button>
                </div>--%>
            </div>
            <h3></h3>
        </div>

        <div id="calendar"></div>
    </div>

</div>

