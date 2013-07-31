<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilySelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.FamilySelect" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>

<script type="text/javascript">

    function cvBirthDateValidator_ClientValidate(sender, args) {
        args.IsValid = false;
        alert(parseDate(args.Value));
        if (parseDate(args.Value) != null) {
            args.IsValid = true;
            return;
        }
    };

    function parseDate(str) {
        return str.match(/^(\d{1,2})[- /.](\d{1,2})[- /.](\d{2,4})$/);
        //return (m) ? new Date(m[3], m[2] - 1, m[1]) : null;
    };

    function isValidDate(value, userFormat) {
        userFormat = userFormat || 'mm/dd/yyyy' || 'mm/dd/yy';
        delimiter = /[^mdy]/.exec(userFormat)[0];
        theFormat = userFormat.split(delimiter);
        theDate = value.split(delimiter);
        isDate = function (date, format) {
            var m, d, y;
            for (var i = 0, len = format.length; i < len; i++) {
                if (/m/.test(format[i])) m = date[i];
                if (/d/.test(format[i])) d = date[i];
                if (/y/.test(format[i])) y = date[i];
            }
            return (m > 0 && m < 13 && y && (y.length === 2 || y.length === 4) && d > 0 && d <= (new Date(y, m, 0)).getDate());
        }
        return isDate(theDate, theFormat);
    }

    function setControlEvents() {
        //$('.family').change(function () {
        $('.family').unbind('click').on('click', function () {
            $(this).toggleClass('active');
            if ($(this).hasClass('active')) {
                $('.family').not(this).removeClass('active');
            };
            return true;
        });

        $('.person').unbind('click').on('click', function () {
            $(this).toggleClass('active');
            var selectedIds = $('#hfSelectedPerson').val();
            var buttonId = this.getAttribute('data-id') + ',';
            if (typeof selectedIds == "string" && (selectedIds.indexOf(buttonId) >= 0)) {
                $('#hfSelectedPerson').val(selectedIds.replace(buttonId, ''));
            } else {
                $('#hfSelectedPerson').val(buttonId + selectedIds);
            }
            return false;
        });

        $('.visitor').unbind('click').on('click', function () {
            $(this).toggleClass('active');
            var selectedIds = $('#hfSelectedVisitor').val();
            var buttonId = this.getAttribute('data-id') + ',';
            if (typeof selectedIds == "string" && (selectedIds.indexOf(buttonId) >= 0)) {
                $('#hfSelectedVisitor').val(selectedIds.replace(buttonId, ''));
                alert('removing the class');
                alert('selected visitor = ' + $('#hfSelectedVisitor').val());
            } else {
                $('#hfSelectedVisitor').val(buttonId + selectedIds);
                alert('adding the class');
                alert('selected visitor = ' + $('#hfSelectedVisitor').val());
            }
            return false;
        });

    };
    $(document).ready(function () { setControlEvents(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(setControlEvents);

</script>

<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />
    <asp:HiddenField ID="personVisitorType" runat="server" />
    <asp:HiddenField ID="hfSelectedPerson" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hfSelectedVisitor" runat="server" ClientIDMode="Static" />

    <!-- Start Family Select -->

    <div class="row-fluid attended-checkin-header">
        <div class="span3 attended-checkin-actions">
            <asp:LinkButton ID="lbBack" CssClass="btn btn-large btn-primary" runat="server" OnClick="lbBack_Click" Text="Back" CausesValidation="false"/>
        </div>

        <div class="span6">
            <h1 id="familyTitle" runat="server">Search Results</h1>
        </div>

        <div class="span3 attended-checkin-actions">
            <asp:LinkButton ID="lbNext" CssClass="btn btn-large last btn-primary" runat="server" OnClick="lbNext_Click" Text="Next"/>
        </div>
    </div>
                
    <div class="row-fluid attended-checkin-body">
        
        <div id="familyDiv" class="span3 family-div" runat="server">
            <div class="attended-checkin-body-container">
                <h3>Families</h3>
                <asp:ListView ID="lvFamily" runat="server" OnPagePropertiesChanging="lvFamily_PagePropertiesChanging" OnItemCommand="lvFamily_ItemCommand">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbSelectFamily" runat="server" CommandArgument='<%# Eval("Group.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select family" CausesValidation="false" ><%# Eval("Caption") %><br /><span class="checkin-sub-title"><%# Eval("SubCaption") %></span></asp:LinkButton>
                    </ItemTemplate>
                </asp:ListView>
                <asp:DataPager ID="dpPager" runat="server" PageSize="4" PagedControlID="lvFamily">
                    <Fields>
                        <asp:NextPreviousPagerField ButtonType="Button" ButtonCssClass="btn btn-primary" />
                    </Fields>
                </asp:DataPager>
            </div>
        </div>

        <div id="personDiv" class="span3 person-div" runat="server">
            <div class="attended-checkin-body-container">
                <h3>People</h3>
                <asp:Repeater ID="repPerson" runat="server" OnItemDataBound="repPerson_ItemDataBound">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbSelectPerson" runat="server" Text='<%# Container.DataItem.ToString() %>' data-id='<%# Eval("Person.Id") %>' CommandArgument='<%# Eval("Person.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select person" CausesValidation="false" />
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <div id="visitorDiv" class="span3 visitor-div" runat="server">
            <div class="attended-checkin-body-container">
                <h3>Visitors</h3>
                <asp:Repeater ID="repVisitors" runat="server" OnItemDataBound="repVisitors_ItemDataBound">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbSelectVisitor" runat="server" Text='<%# Container.DataItem.ToString() %>' data-id='<%# Eval("Person.Id") %>' CommandArgument='<%# Eval("Person.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select visitor" CausesValidation="false" />
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <div id="nothingFoundMessage" class="span9 nothing-found-message" runat="server">
            <div class="span12">
            <p>
                Please add them using one of the buttons on the right
            </p>
            </div>
        </div>

        <div class="span3 add-someone">
            <div class="attended-checkin-body-container last">
                <asp:LinkButton ID="lbAddPerson" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddPerson_Click" Text="Add Person" CausesValidation="false"></asp:LinkButton>
                <asp:LinkButton ID="lbAddVisitor" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddVisitor_Click" Text="Add Visitor" CausesValidation="false"></asp:LinkButton>                
                <asp:LinkButton ID="lbAddFamily" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddFamily_Click" Text="Add Family" CausesValidation="false" />
            </div>
        </div>
    </div>

    <!-- End Family Select -->

    <!-- Start New People -->

    <asp:Panel ID="AddPersonPanel" runat="server" CssClass="add-person">
        <Rock:ModalAlert ID="maAddPerson" runat="server" />
        <div class="row-fluid attended-checkin-header">
            <div class="span3 attended-checkin-actions">
                <asp:LinkButton ID="lbAddPersonCancel" CssClass="btn btn-large btn-primary" runat="server" OnClick="lbAddPersonCancel_Click" Text="Cancel" CausesValidation="false"/>
            </div>

            <div class="span6">
                <h1><asp:Label ID="lblAddPersonHeader" runat="server"></asp:Label></h1>
            </div>

            <div class="span3 attended-checkin-actions">
                <asp:LinkButton ID="lbAddPersonSearch" CssClass="btn btn-large last btn-primary" runat="server" OnClick="lbAddPersonSearch_Click" Text="Search" CausesValidation="false" />
            </div>
        </div>
        <div class="row-fluid attended-checkin-body">
            <div class="span3 ">
                <h3>First Name</h3>
            </div>
            <div class="span3">
                <h3>Last Name</h3>
            </div>
            <div class="span3">
                <h3>DOB/Age</h3>
            </div>
            <div class="span3">
                <h3>Grade</h3>
            </div>
        </div>
        <div class="row-fluid attended-checkin-body searchperson">
            <div class="span3">
                <Rock:DataTextBox ID="tbFirstNameSearch" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
            </div>
            <div class="span3">
                <Rock:DataTextBox ID="tbLastNameSearch" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
            </div>
            <div class="span3">
                <Rock:DatePicker ID="dtpDOBSearch" runat="server" CssClass="datePickerClass"></Rock:DatePicker>
                <asp:CustomValidator ID="cvDOBSearch" runat="server" ErrorMessage="The first DOB/Age field is incorrect."
                    CssClass="align-middle" EnableClientScript="true" Display="None" 
                    ClientValidationFunction="cvBirthDateValidator_ClientValidate" 
                    OnServerValidate="cvBirthDateValidator_ServerValidate" ControlToValidate="dtpDOBSearch" />
            </div>
            <div class="span3">s
                <Rock:DataTextBox ID="tbGradeSearch" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
            </div>
        </div>
        <br />
        <div class="row-fluid attended-checkin-body searchperson">
            <Rock:Grid ID="grdPersonSearchResults" runat="server" AllowPaging="true" OnRowCommand="grdPersonSearchResults_RowCommand" ShowActionRow="false" PageSize="3" DataKeyNames="personId">
                <Columns>
                    <asp:BoundField DataField="personId" Visible="false" />
                    <asp:BoundField DataField="personFirstName" HeaderText="First Name" />
                    <asp:BoundField DataField="personLastName" HeaderText="Last Name" />
                    <asp:BoundField DataField="personDOB" HeaderText="DOB" />
                    <asp:BoundField DataField="personGrade" HeaderText="Grade" />
                    <asp:TemplateField HeaderText="Add">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbAdd" runat="server" CssClass="btn ConfirmButtons" CommandName="Add" Text="Add" CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"><i class="icon-plus"></i></asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </Rock:Grid>
        </div>
        <div class="row-fluid attended-checkin-body searchperson">
            <asp:LinkButton ID="lbAddSearchedForPerson" runat="server" Text="None of these, add me as a new [person/visitor]." Visible="false" OnClick="lbAddSearchedForPerson_Click" CausesValidation="false"></asp:LinkButton>
        </div>
    </asp:Panel>
    <asp:ModalPopupExtender ID="mpePerson" runat="server" TargetControlID="lbOpenPersonPanel" PopupControlID="AddPersonPanel" CancelControlID="lbAddPersonCancel" BackgroundCssClass="modalBackground"></asp:ModalPopupExtender>
    <asp:LinkButton ID="lbOpenPersonPanel" runat="server" CausesValidation="false"></asp:LinkButton>

    <!-- End New People -->

    <!-- Start New Family -->

    <asp:Panel ID="pnlAddFamily" runat="server" CssClass="add-family">
        <div class="row-fluid attended-checkin-header">
            <div class="span3 attended-checkin-actions">
                <asp:LinkButton ID="lbAddFamilyCancel" CssClass="btn btn-large btn-primary" runat="server" OnClick="lbAddFamilyCancel_Click" Text="Cancel" CausesValidation="false" />
            </div>

            <div class="span6">
                <h1>Add Family</h1>
            </div>

            <div class="span3 attended-checkin-actions">
                <asp:LinkButton ID="lbAddFamilySave" CssClass="btn btn-large last btn-primary" runat="server" OnClick="lbAddFamilySave_Click" Text="Save" />
            </div>
        </div>

        <div class="row-fluid attended-checkin-body">
            <div class="span3">
                <h3>First Name</h3>
            </div>
            <div class="span3">
                <h3>Last Name</h3>
            </div>
            <div class="span2">
                <h3>DOB</h3>
            </div>
            <div class="span2">
                <h3>Ability/Grade</h3>
            </div>
            <div class="span2">
                <h3>Gender</h3>
            </div>
        </div>

        <div class="row-fluid">
            
            <div class="row-fluid attended-checkin-body person">

                <asp:Repeater ID="repAddFamily" runat="server" OnItemDataBound="repAddFamily_ItemDataBound" >
                <ItemTemplate>
                    <div class="row-fluid">
                        <div class="span3">
                            <asp:TextBox ID="tbFirstName" runat="server" CssClass="fullBlock" />
                        </div>
                        <div class="span3">
                            <asp:TextBox ID="tbLastName" runat="server" CssClass="fullBlock" />
                        </div>
                        <div class="span2">
                            <Rock:DatePicker ID="dpBirthDate" runat="server" />
                            <asp:CustomValidator ID="cvBirthDateValidator" runat="server" 
                                ErrorMessage="Please enter a valid birth date."
                                CssClass="align-middle" EnableClientScript="true" Display="Dynamic"
                                ClientValidationFunction="cvBirthDateValidator_ClientValidate"
                                OnServerValidate="cvBirthDateValidator_ServerValidate" 
                                ControlToValidate="dpBirthDate" />
                        </div>
                        <div class="span2">
                            <Rock:RockDropDownList ID="ddlAbilityGrade" runat="server" CssClass="fullBlock"  />
                        </div>
                        <div class="span2">
                            <Rock:RockDropDownList ID="ddlGender" runat="server" CssClass="fullBlock" />
                        </div>      
                    </div>
                </ItemTemplate>
                </asp:Repeater>  
                              
            </div>

        </div>

        <div class="row-fluid attended-checkin-body buttons">
            <asp:LinkButton ID="PreviousButton" CssClass="btn btn-large btn-primary left-button" runat="server" OnClick="PreviousButton_Click" Text="Previous" Visible="false" />
            <asp:LinkButton ID="MoreButton" CssClass="btn btn-large btn-primary right-button" runat="server" OnClick="MoreButton_Click" Text="More" />
        </div>
        <asp:ValidationSummary ID="valSummaryBottom" runat="server" HeaderText="Please Correct the Following:" CssClass="alert alert-error block-message error alert error-modal" />
    </asp:Panel>

    <asp:ModalPopupExtender ID="mpeAddFamily" runat="server" TargetControlID="hfOpenFamilyPanel" PopupControlID="pnlAddFamily"
        CancelControlID="lbAddFamilyCancel" BackgroundCssClass="modalBackground" />
    <asp:HiddenField ID="hfOpenFamilyPanel" runat="server" />

    <!-- End New People -->

</ContentTemplate>
</asp:UpdatePanel>
