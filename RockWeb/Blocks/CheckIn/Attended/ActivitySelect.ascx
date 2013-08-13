<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ActivitySelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.ActivitySelect" %>

<script type="text/javascript">

    function setControlEvents() {

        //$('.ministry').unbind('click').on('click', function () {
        //    $(this).toggleClass('active');
        //    var selectedIds = $('#hfSelectedMinistry').val();
        //    var buttonId = this.getAttribute('data-id') + ',';
        //    if (typeof selectedIds == "string" && (selectedIds.indexOf(buttonId) >= 0)) {
        //        $('#hfSelectedMinistry').val(selectedIds.replace(buttonId, ''));
        //    } else {
        //        $('#hfSelectedMinistry').val(buttonId + selectedIds);
        //    }
        //    return false;
        //});

        //$('.time').unbind('click').on('click', function () {
        //    $(this).toggleClass('active');
        //    var selectedIds = $('#hfSelectedTime').val();
        //    var buttonId = this.getAttribute('data-id') + ',';
        //    if (typeof selectedIds == "string" && (selectedIds.indexOf(buttonId) >= 0)) {
        //        $('#hfSelectedTime').val(selectedIds.replace(buttonId, ''));
        //    } else {
        //        $('#hfSelectedTime').val(buttonId + selectedIds);
        //    }
        //    return false;
        //});
        
    };
    $(document).ready(function () { setControlEvents(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(setControlEvents);

</script>

<asp:UpdatePanel ID="upContent" runat="server" UpdateMode="Conditional">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />
    <asp:HiddenField ID="hfSelectedMinistry" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hfSelectedTime" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hfSelectedActivity" runat="server" ClientIDMode="Static" />

    <div class="row-fluid attended-checkin-header">
        <div class="span3 attended-checkin-actions">
            <asp:LinkButton ID="lbBack" CssClass="btn btn-primary btn-large" runat="server" OnClick="lbBack_Click" Text="Back"/>
        </div>

        <div class="span6">
            <h1><asp:Label ID="lblPersonName" runat="server"></asp:Label></h1>
        </div>

        <div class="span3 attended-checkin-actions">
            <asp:LinkButton ID="lbNext" CssClass="btn btn-primary btn-large last" runat="server" OnClick="lbNext_Click" Text="Next"/>
        </div>
    </div>
                
    <div class="row-fluid attended-checkin-body">

        <asp:UpdatePanel ID="pnlSelectMinistry" runat="server" UpdateMode="Conditional" class="span3">
        <ContentTemplate>        
            <div class="attended-checkin-body-container">
                <h3>Ministry</h3>
                <asp:Repeater ID="rMinistry" runat="server" OnItemCommand="rMinistry_ItemCommand" OnItemDataBound="rMinistry_ItemDataBound">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbSelectMinistry" runat="server" Text='<%# Container.DataItem.ToString() %>' data-id='<%# Eval("Id") %>' CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select ministry" CausesValidation="false" />
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </ContentTemplate>
        </asp:UpdatePanel>

        <div class="span3">
            <div class="attended-checkin-body-container">
                <h3>Time</h3>
                <asp:Repeater ID="rTime" runat="server" OnItemCommand="rTime_ItemCommand" OnItemDataBound="rTime_ItemDataBound">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbSelectTime" runat="server" Text='<%# Container.DataItem.ToString() %>' data-id='<%# Eval("Id") %>' CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select time" CausesValidation="false" />
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <asp:HiddenField ID="hfTimes" runat="server"></asp:HiddenField>
        </div>

        <asp:UpdatePanel ID="pnlSelectActivity" runat="server" UpdateMode="Conditional" class="span3">
        <ContentTemplate>        
            <div class="attended-checkin-body-container">
                <h3>Activity</h3>
                <asp:ListView ID="lvActivity" runat="server" OnPagePropertiesChanging="lvActivity_PagePropertiesChanging" OnItemCommand="lvActivity_ItemCommand" OnItemDataBound="lvActivity_ItemDataBound">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbSelectActivity" runat="server" Text='<%# Container.DataItem.ToString() %>' CommandArgument='<%# Eval("Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" ></asp:LinkButton>
                    </ItemTemplate>
                </asp:ListView>
                <asp:DataPager ID="Pager" runat="server" PageSize="6" PagedControlID="lvActivity">
                    <Fields>
                        <asp:NextPreviousPagerField ButtonType="Button" ButtonCssClass="btn btn-primary" />
                    </Fields>
                </asp:DataPager>
            </div>
        </ContentTemplate>
        </asp:UpdatePanel>

        <div class="span3">
            <div class="attended-checkin-body-container">
                <h3>Selected</h3>
                <Rock:Grid ID="gActivityList" runat="server" AllowSorting="true" AllowPaging="false" ShowActionRow="false" ShowHeader="false" CssClass="select">
                    <Columns>
                        <asp:BoundField DataField="ListId" Visible="false" />
                        <asp:BoundField DataField="Time" />
                        <asp:BoundField DataField="AssignedTo" />
                        <Rock:DeleteField OnClick="gActivityList_Delete" />
                    </Columns>
                </Rock:Grid>
            </div>
        </div>

    </div>   

</ContentTemplate>
</asp:UpdatePanel>
