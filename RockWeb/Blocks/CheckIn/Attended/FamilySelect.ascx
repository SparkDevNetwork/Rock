<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilySelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.FamilySelect" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>

<script type="text/javascript">

    function cvDOBValidator_ClientValidate(sender, args) {
        args.IsValid = false;
        if (isValidDate(args.Value)) {
            args.IsValid = true;
            return;
        }
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
                <asp:Repeater ID="repPerson" runat="server" OnItemCommand="repPerson_ItemCommand" OnItemDataBound="repPerson_ItemDataBound">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbSelectPerson" runat="server" Text='<%# Container.DataItem.ToString() %>' data-id='<%# Eval("Person.Id") %>' CommandArgument='<%# Eval("Person.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select person" CausesValidation="false" />
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <div id="visitorDiv" class="span3 visitor-div" runat="server">
            <div class="attended-checkin-body-container">
                <h3>Visitors</h3>
                <asp:Repeater ID="repVisitors" runat="server" OnItemCommand="repVisitors_ItemCommand" OnItemDataBound="repVisitors_ItemDataBound">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbSelectVisitor" runat="server" Text='<%# Container.DataItem.ToString() %>' data-id='<%# Eval("Person.Id") %>' CommandArgument='<%# Eval("Person.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select visitor" CausesValidation="false" />
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <div id="nothingFoundMessage" class="span9 nothing-found-message" runat="server">
            <div class="span12">
            <p>
                <h1>Aww man!</h1> Too bad you didn't find what you were looking for. Go ahead and add someone using one of the buttons on the right or click the "Back" button and try again!
            </p>
            </div>
        </div>

        <div class="span3 add-someone">
            <div class="attended-checkin-body-container last">
                <h3>Actions</h3>
                <asp:LinkButton ID="lbAddPerson" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddPerson_Click" Text="Add Person" CausesValidation="false"></asp:LinkButton>
                <asp:LinkButton ID="lbAddVisitor" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddVisitor_Click" Text="Add Visitor" CausesValidation="false"></asp:LinkButton>                
                <asp:LinkButton ID="lbAddFamily" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddFamily_Click" Text="Add Family" CausesValidation="false"></asp:LinkButton>
            </div>
        </div>
    </div>

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
                    ClientValidationFunction="cvDOBValidator_ClientValidate" OnServerValidate="cvDOBValidator_ServerValidate" ControlToValidate="dtpDOBSearch" />
            </div>
            <div class="span3">
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

    <asp:Panel ID="AddFamilyPanel" runat="server" CssClass="add-family">
        <div class="row-fluid attended-checkin-header">
            <div class="span3 attended-checkin-actions">
                <asp:LinkButton ID="lbAddFamilyCancel" CssClass="btn btn-large btn-primary" runat="server" OnClick="lbAddFamilyCancel_Click" Text="Cancel" CausesValidation="false"/>
            </div>

            <div class="span6">
                <h1>Add Family</h1>
            </div>

            <div class="span3 attended-checkin-actions">
                <asp:LinkButton ID="lbAddFamilySave" CssClass="btn btn-large last btn-primary" runat="server" OnClick="lbAddFamilySave_Click" Text="Save" />
            </div>
        </div>
        <div class="row-fluid attended-checkin-body">
            <div class="span3 ">
                <h3>First Name</h3>
            </div>
            <div class="span3">
                <h3>Last Name</h3>
            </div>
            <div class="span2">
                <h3>DOB</h3>
            </div>
            <div class="span2">
                <h3>Gender</h3>
            </div>
            <div class="span2">
                <h3>Attribute</h3>
            </div>
        </div>
        <div id="addFamilyNamesPage1" runat="server">
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="tbFirstName1" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbLastName1" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span2">
                    <Rock:DatePicker ID="dpDOB1" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBValidator1" runat="server" ErrorMessage="The first DOB field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBValidator_ClientValidate" OnServerValidate="cvDOBValidator_ServerValidate" ControlToValidate="dpDOB1" />
                </div>
                <div class="span2">
                    <Rock:DataTextBox ID="tbGender1" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span2">
                    <Rock:DataTextBox ID="tbGrade1" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="tbFirstName2" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbLastName2" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span2">
                    <Rock:DatePicker ID="dpDOB2" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBValidator2" runat="server" ErrorMessage="The second DOB field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBValidator_ClientValidate" OnServerValidate="cvDOBValidator_ServerValidate" ControlToValidate="dpDOB2" />
                </div>
                <div class="span2">
                    <Rock:DataTextBox ID="tbGender2" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span2">
                    <Rock:DataTextBox ID="tbGrade2" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="tbFirstName3" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbLastName3" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span2">
                    <Rock:DatePicker ID="dpDOB3" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBValidator3" runat="server" ErrorMessage="The third DOB field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBValidator_ClientValidate" OnServerValidate="cvDOBValidator_ServerValidate" ControlToValidate="dpDOB3" />
                </div>
                <div class="span2">
                    <Rock:DataTextBox ID="tbGender3" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span2">
                    <Rock:DataTextBox ID="tbGrade3" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="tbFirstName4" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbLastName4" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span2">
                    <Rock:DatePicker ID="dpDOB4" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBValidator4" runat="server" ErrorMessage="The fourth DOB field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBValidator_ClientValidate" OnServerValidate="cvDOBValidator_ServerValidate" ControlToValidate="dpDOB4" />
                </div>
                <div class="span2">
                    <Rock:DataTextBox ID="tbGender4" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span2">
                    <Rock:DataTextBox ID="tbGrade4" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
        </div>
        <div id="addFamilyNamesPage2" runat="server" visible="false">
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="tbFirstName5" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbLastName5" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DatePicker ID="dpDOB5" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBValidator5" runat="server" ErrorMessage="The first DOB field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBValidator_ClientValidate" OnServerValidate="cvDOBValidator_ServerValidate" ControlToValidate="dpDOB5" />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbGrade5" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="tbFirstName6" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbLastName6" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DatePicker ID="dpDOB6" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBValidator6" runat="server" ErrorMessage="The second DOB field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBValidator_ClientValidate" OnServerValidate="cvDOBValidator_ServerValidate" ControlToValidate="dpDOB6" />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbGrade6" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="tbFirstName7" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbLastName7" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DatePicker ID="dpDOB7" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBValidator7" runat="server" ErrorMessage="The third DOB field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBValidator_ClientValidate" OnServerValidate="cvDOBValidator_ServerValidate" ControlToValidate="dpDOB7" />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbGrade7" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="tbFirstName8" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbLastName8" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DatePicker ID="dpDOB8" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBValidator8" runat="server" ErrorMessage="The fourth DOB field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBValidator_ClientValidate" OnServerValidate="cvDOBValidator_ServerValidate" ControlToValidate="dpDOB8" />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbGrade8" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
        </div>
        <div id="addFamilyNamesPage3" runat="server" visible="false">
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="tbFirstName9" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbLastName9" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DatePicker ID="dpDOB9" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBValidator9" runat="server" ErrorMessage="The first DOB field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBValidator_ClientValidate" OnServerValidate="cvDOBValidator_ServerValidate" ControlToValidate="dpDOB9" />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbGrade9" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="tbFirstName10" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbLastName10" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DatePicker ID="dpDOB10" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBValidator10" runat="server" ErrorMessage="The second DOB field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBValidator_ClientValidate" OnServerValidate="cvDOBValidator_ServerValidate" ControlToValidate="dpDOB10" />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbGrade10" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="tbFirstName11" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbLastName11" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DatePicker ID="dpDOB11" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBValidator11" runat="server" ErrorMessage="The third DOB field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBValidator_ClientValidate" OnServerValidate="cvDOBValidator_ServerValidate" ControlToValidate="dpDOB11" />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbGrade11" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="tbFirstName12" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbLastName12" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DatePicker ID="dpDOB12" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBValidator12" runat="server" ErrorMessage="The fourth DOB field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBValidator_ClientValidate" OnServerValidate="cvDOBValidator_ServerValidate" ControlToValidate="dpDOB12" />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbGrade12" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
        </div>
        <div class="row-fluid attended-checkin-body buttons">
            <asp:LinkButton ID="PreviousButton" CssClass="btn btn-large btn-primary left-button" runat="server" OnClick="PreviousButton_Click" Text="Previous" Visible="false" />
            <asp:LinkButton ID="MoreButton" CssClass="btn btn-large btn-primary right-button" runat="server" OnClick="MoreButton_Click" Text="More" />
        </div>
        <asp:ValidationSummary ID="valSummaryBottom" runat="server" HeaderText="Please Correct the Following:" CssClass="alert alert-error block-message error alert error-modal" />
    </asp:Panel>
    <asp:ModalPopupExtender ID="mpeFamily" runat="server" TargetControlID="lbOpenFamilyPanel" PopupControlID="AddFamilyPanel" CancelControlID="lbAddFamilyCancel" BackgroundCssClass="modalBackground"></asp:ModalPopupExtender>
    <asp:LinkButton ID="lbOpenFamilyPanel" runat="server" CausesValidation="false"></asp:LinkButton>

</ContentTemplate>
</asp:UpdatePanel>
