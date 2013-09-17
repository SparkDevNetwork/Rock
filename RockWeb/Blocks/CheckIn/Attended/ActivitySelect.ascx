<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ActivitySelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.ActivitySelect" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>

<script type="text/javascript">

    function setControlEvents() {
        
        $find("mpeAddCondition").add_shown(function () {
            $find("mpeAddCondition")._backgroundElement.onclick = function () {
                $find("mpeAddCondition").hide();
            }
        });

        $find("mpeAddNote").add_shown(function () {
            $find("mpeAddNote")._backgroundElement.onclick = function () {
                $find("mpeAddNote").hide();
            }
        });
        
    };

    $(document).ready(function () { setControlEvents(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(setControlEvents);

</script>

<asp:UpdatePanel ID="upContent" runat="server" UpdateMode="Conditional">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <asp:Panel ID="pnlActivitySelect" runat="server" CssClass="attended" >
        <div class="row-fluid checkin-header">
            <div class="span3 checkin-actions">
                <asp:LinkButton ID="lbBack" CssClass="btn btn-primary btn-large" runat="server" OnClick="lbBack_Click" Text="Back"/>
            </div>

            <div class="span6">
                <h1><asp:Label ID="lblPersonName" runat="server"></asp:Label></h1>
            </div>

            <div class="span3 checkin-actions">
                <asp:LinkButton ID="lbNext" CssClass="btn btn-primary btn-large" runat="server" OnClick="lbNext_Click" Text="Next"/>
            </div>
        </div>
                
        <div class="row-fluid checkin-body">

            <asp:UpdatePanel ID="pnlSelectGroupType" runat="server" UpdateMode="Conditional" class="span3">
            <ContentTemplate>        
                
                    <h3>GroupType</h3>
                    <asp:Repeater ID="rGroupType" runat="server" OnItemCommand="rGroupType_ItemCommand" OnItemDataBound="rGroupType_ItemDataBound">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbSelectGroupType" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" CausesValidation="false" />
                        </ItemTemplate>
                    </asp:Repeater>
                
            </ContentTemplate>
            </asp:UpdatePanel>

            <asp:UpdatePanel ID="pnlSelectLocation" runat="server" UpdateMode="Conditional" class="span3">
            <ContentTemplate>        
                
                    <h3>Location</h3>
                    <asp:ListView ID="lvLocation" runat="server" OnPagePropertiesChanging="lvLocation_PagePropertiesChanging" OnItemCommand="lvLocation_ItemCommand" OnItemDataBound="lvLocation_ItemDataBound">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbSelectLocation" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" ></asp:LinkButton>
                        </ItemTemplate>
                    </asp:ListView>
                    <asp:DataPager ID="Pager" runat="server" PageSize="5" PagedControlID="lvLocation">
                        <Fields>
                            <asp:NextPreviousPagerField ButtonType="Button" ButtonCssClass="btn btn-primary btn-checkin-select" />
                        </Fields>
                    </asp:DataPager>
                
            </ContentTemplate>
            </asp:UpdatePanel>

            <asp:UpdatePanel ID="pnlSelectSchedule" runat="server" UpdateMode="Conditional" class="span3">
            <ContentTemplate>        
                
                    <h3>Schedule</h3>
                    <asp:Repeater ID="rSchedule" runat="server" OnItemCommand="rSchedule_ItemCommand" OnItemDataBound="rSchedule_ItemDataBound">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbSelectSchedule" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" CausesValidation="false" />
                        </ItemTemplate>
                    </asp:Repeater>
                
            </ContentTemplate>
            </asp:UpdatePanel>

            <div class="span3 selected-grid">
                <h3>Selected</h3>
                <asp:UpdatePanel ID="pnlSelectedGrid" runat="server" UpdateMode="Conditional">
                <ContentTemplate> 
                    <Rock:Grid ID="gSelectedList" runat="server" ShowHeader="false" DataKeyNames="LocationId, ScheduleId" DisplayType="Light">
                    <Columns>
                        <asp:BoundField DataField="Schedule" />
                        <asp:BoundField DataField="ScheduleId" Visible="false" />
                        <asp:BoundField DataField="Location" />
                        <asp:BoundField DataField="LocationId" Visible="false" />                        
                        <Rock:DeleteField OnClick="gSelectedList_Delete" ControlStyle-CssClass="btn btn-large btn-primary" />
                    </Columns>
                    </Rock:Grid>
                </ContentTemplate>
                </asp:UpdatePanel>
                <asp:LinkButton ID="lbAddCondition" runat="server" Text="Add an Allergy" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddCondition_Click" CausesValidation="false" />
                <asp:LinkButton ID="lbAddNote" runat="server" Text="Add a Note" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddNote_Click" CausesValidation="false" />                
            </div>
        </div>   
    </asp:Panel>

    <!-- Add Condition Panel -->
    <asp:Panel ID="pnlAddCondition" runat="server" CssClass="attended modal-foreground small" DefaultButton="lbAddConditionSave">
        <div class="checkin-header row-fluid">
            <div class="span3 checkin-actions">
                <asp:LinkButton ID="lbAddConditionCancel" CssClass="btn btn-large btn-primary" runat="server" OnClick="lbAddConditionCancel_Click" Text="Cancel" CausesValidation="false" />
            </div>
            <div class="span6">
                <h3>Add Allergy/Medical</h3>
            </div>
            <div class="span3 checkin-actions">
                <asp:LinkButton ID="lbAddConditionSave" CssClass="btn btn-large btn-primary" runat="server" OnClick="lbAddConditionSave_Click" Text="Save" />
            </div>
        </div>

        <div class="checkin-body">
            <asp:Repeater ID="rptCondition" runat="server" OnItemDataBound="rptAddCondition_ItemDataBound" OnItemCommand="rptCondition_ItemCommand">
                <HeaderTemplate>
                    <div class="row-fluid">
                </HeaderTemplate>
                <ItemTemplate>
                    <%# (Container.ItemIndex > 0 && Container.ItemIndex % 3 == 0) ? "</div><div class=\"row-fluid\">" : string.Empty %>
                    <div class="span4">
                        <asp:LinkButton ID="lbConditionName" CssClass="btn btn-block btn-primary btn-checkin-select" runat="server" />
                    </div>
                </ItemTemplate>
                <FooterTemplate>
                    </div>
                </FooterTemplate>
            </asp:Repeater>
        </div>

    </asp:Panel>
    <asp:ModalPopupExtender ID="mpeAddCondition" runat="server" BehaviorID="mpeAddCondition" TargetControlID="hfConditionPanel" PopupControlID="pnlAddCondition"
        CancelControlID="lbAddConditionCancel" BackgroundCssClass="attended modal-background" />
    <asp:HiddenField ID="hfConditionPanel" runat="server" />

    <!-- Add Note Panel -->
    <asp:Panel ID="pnlAddNote" runat="server" CssClass="attended modal-foreground small" DefaultButton="lbAddNoteSave">
        <div class="checkin-header row-fluid">
            <div class="span3 checkin-actions">
                <asp:LinkButton ID="lbAddNoteCancel" CssClass="btn btn-large btn-primary" runat="server" OnClick="lbAddNoteCancel_Click" Text="Cancel" CausesValidation="false"/>
            </div>
            <div class="span6">
                <h3>Add Note</h3>
            </div>
            <div class="span3 checkin-actions">
                <asp:LinkButton ID="lbAddNoteSave" CssClass="btn btn-large btn-primary" runat="server" OnClick="lbAddNoteSave_Click" Text="Save" />
            </div>
        </div>
		
        <div class="checkin-body">
            <div class="row-fluid">
                <Rock:LabeledTextBox ID="tbNote" runat="server" CssClass="note" MaxLength="40" />
            </div>
        </div>

    </asp:Panel>
    <asp:ModalPopupExtender ID="mpeAddNote" runat="server" BehaviorID="mpeAddNote" TargetControlID="hfOpenNotePanel" PopupControlID="pnlAddNote" 
        CancelControlID="lbAddNoteCancel" BackgroundCssClass="attended modal-background" />
    <asp:HiddenField ID="hfOpenNotePanel" runat="server" />    

</ContentTemplate>
</asp:UpdatePanel>
