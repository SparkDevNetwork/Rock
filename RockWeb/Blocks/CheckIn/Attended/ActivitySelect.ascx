<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ActivitySelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.ActivitySelect" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>

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

    };
    $(document).ready(function () { setControlEvents(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(setControlEvents);

</script>

<asp:UpdatePanel ID="upContent" runat="server" UpdateMode="Conditional">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <asp:Panel ID="pnlActivitySelect" runat="server" >
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

            <asp:UpdatePanel ID="pnlSelectGroupType" runat="server" UpdateMode="Conditional" class="span3">
            <ContentTemplate>        
                <div class="attended-checkin-body-container">
                    <h3>GroupType</h3>
                    <asp:Repeater ID="rGroupType" runat="server" OnItemCommand="rGroupType_ItemCommand" OnItemDataBound="rGroupType_ItemDataBound">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbSelectGroupType" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select ministry" CausesValidation="false" />
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </ContentTemplate>
            </asp:UpdatePanel>

            <asp:UpdatePanel ID="pnlSelectLocation" runat="server" UpdateMode="Conditional" class="span3">
            <ContentTemplate>        
                <div class="attended-checkin-body-container">
                    <h3>Location</h3>
                    <asp:ListView ID="lvLocation" runat="server" OnPagePropertiesChanging="lvLocation_PagePropertiesChanging" OnItemCommand="lvLocation_ItemCommand" OnItemDataBound="lvLocation_ItemDataBound">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbSelectLocation" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" ></asp:LinkButton>
                        </ItemTemplate>
                    </asp:ListView>
                    <asp:DataPager ID="Pager" runat="server" PageSize="5" PagedControlID="lvLocation">
                        <Fields>
                            <asp:NextPreviousPagerField ButtonType="Button" ButtonCssClass="btn btn-primary" />
                        </Fields>
                    </asp:DataPager>
                </div>
            </ContentTemplate>
            </asp:UpdatePanel>

            <asp:UpdatePanel ID="pnlSelectSchedule" runat="server" UpdateMode="Conditional" class="span3">
            <ContentTemplate>        
                <div class="attended-checkin-body-container">
                    <h3>Schedule</h3>
                    <asp:Repeater ID="rSchedule" runat="server" OnItemCommand="rSchedule_ItemCommand" OnItemDataBound="rSchedule_ItemDataBound">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbSelectSchedule" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select time" CausesValidation="false" />
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </ContentTemplate>
            </asp:UpdatePanel>

            <div class="span3">
                <div class="attended-checkin-body-container">
                    <h3>Selected</h3>
                    <asp:UpdatePanel ID="pnlSelectedGrid" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>        
                        <Rock:Grid ID="gSelectedList" runat="server" AllowSorting="true" AllowPaging="false" ShowActionRow="false" ShowHeader="false" CssClass="select" DataKeyNames="LocationId, ScheduleId">
                            <Columns>
                                <asp:BoundField DataField="Schedule" />
                                <asp:BoundField DataField="Location" />
                                <asp:BoundField DataField="LocationId" Visible="false" />
                                <asp:BoundField DataField="ScheduleId" Visible="false" />
                                <Rock:DeleteField OnClick="gSelectedList_Delete" ControlStyle-CssClass="btn btn-large btn-primary" />
                            </Columns>
                        </Rock:Grid>
                    </ContentTemplate>
                    </asp:UpdatePanel>
                    <asp:LinkButton ID="lbAddCondition" runat="server" Text="Add an Allergy" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddCondition_Click" CausesValidation="false" />
                    <asp:LinkButton ID="lbAddNote" runat="server" Text="Add a Note" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddNote_Click" CausesValidation="false" />                    
                </div>
            </div>
        </div>   
    </asp:Panel>

    <!-- Add Note Panel -->
    <asp:Panel ID="pnlAddNote" runat="server" CssClass="add-note" DefaultButton="lbAddNoteSave">
        <Rock:ModalAlert ID="maAddNote" runat="server" />
        <div class="row-fluid attended-checkin-header">
            <div class="span3 attended-checkin-actions">
                <asp:LinkButton ID="lbAddNoteCancel" CssClass="btn btn-large btn-primary" runat="server" OnClick="lbAddNoteCancel_Click" Text="Cancel" CausesValidation="false"/>
            </div>

            <div class="span6">
                <h1>Add Note</h1>
            </div>

            <div class="span3 attended-checkin-actions">
                <asp:LinkButton ID="lbAddNoteSave" CssClass="btn btn-large last btn-primary" runat="server" OnClick="lbAddNoteSave_Click" Text="Save" />
            </div>
        </div>
		
        <div class="row-fluid attended-checkin-body addnote">
            <div class="span12">
                <Rock:LabeledTextBox ID="tbNote" runat="server" CssClass="fullWidth" MaxLength="40" />
            </div>
        </div>

    </asp:Panel>
    <asp:ModalPopupExtender ID="mpeAddNote" runat="server" TargetControlID="hfOpenNotePanel" PopupControlID="pnlAddNote" 
        CancelControlID="lbAddNoteCancel" BackgroundCssClass="modalBackground" />
    <asp:HiddenField ID="hfOpenNotePanel" runat="server" />    

    <!-- Add Condition Panel -->
    <asp:Panel ID="pnlAddCondition" runat="server" CssClass="add-condition" DefaultButton="lbAddConditionSave">
        <Rock:ModalAlert ID="maAddCondition" runat="server" />
        <div class="row-fluid attended-checkin-header">
            <div class="span3 attended-checkin-actions">
                <asp:LinkButton ID="lbAddConditionCancel" CssClass="btn btn-large btn-primary" runat="server" OnClick="lbAddConditionCancel_Click" Text="Cancel" CausesValidation="false"/>
            </div>

            <div class="span6">
                <h1>Add Allergy/Medical</h1>
            </div>

            <div class="span3 attended-checkin-actions">
                <asp:LinkButton ID="lbAddConditionSave" CssClass="btn btn-large last btn-primary" runat="server" OnClick="lbAddConditionSave_Click" Text="Save" />
            </div>
        </div>
		
        <asp:Repeater ID="rptCondition" runat="server" OnItemDataBound="rptAddCondition_ItemDataBound">
            <HeaderTemplate>
                
            </HeaderTemplate>
            <ItemTemplate>
                <div class="span4">
                    <asp:LinkButton ID="lbConditionName" CssClass="btn btn-large last btn-primary" runat="server" />
                </div>
            </ItemTemplate>
        </asp:Repeater>
        
    </asp:Panel>
    <asp:ModalPopupExtender ID="mpeAddCondition" runat="server" TargetControlID="hfOpenConditionPanel" PopupControlID="pnlAddCondition" 
        CancelControlID="lbAddConditionCancel" BackgroundCssClass="modalBackground" />
    <asp:HiddenField ID="hfConditionPanel" runat="server" />    

</ContentTemplate>
</asp:UpdatePanel>
