<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilySelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.FamilySelect" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>

<script type="text/javascript">
    function cvDOBAgeValidator_ClientValidate(sender, args) {
        args.IsValid = false;
        if (args.Value.length <= 3 && !isNaN(args.Value)) {
            args.IsValid = true;
            return;
        }
        else if (isValidDate(args.Value)) {
            args.IsValid = true;
            return;
        }
    };

    function isValidDate(value, userFormat) {
        userFormat = userFormat || 'mm/dd/yyyy';
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
            return (m > 0 && m < 13 && y && y.length === 4 && d > 0 && d <= (new Date(y, m, 0)).getDate());
        }
        return isDate(theDate, theFormat);
    }
</script>

<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />

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
                        <asp:LinkButton ID="lbSelectFamily" runat="server" CommandArgument='<%# Eval("Group.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" ><%# Eval("Caption") %><br /><span class="checkin-sub-title"><%# Eval("SubCaption") %></span></asp:LinkButton>
                    </ItemTemplate>
                </asp:ListView>
                <asp:DataPager ID="Pager" runat="server" PageSize="4" PagedControlID="lvFamily">
                    <Fields>
                        <asp:NextPreviousPagerField ButtonType="Button" ButtonCssClass="btn btn-primary" />
                    </Fields>
                </asp:DataPager>
            </div>
        </div>

        <div id="personDiv" class="span3 person-div" runat="server">
            <div class="attended-checkin-body-container">
                <h3>People</h3>
                <asp:Repeater ID="rPerson" runat="server" OnItemCommand="rPerson_ItemCommand">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbSelectPerson" runat="server" Text='<%# Container.DataItem.ToString() %>' CommandArgument='<%# Eval("Person.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select" />
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <div id="emptyDiv" class="span3 empty-div" runat="server">
            <div class="attended-checkin-body-container">
                <h3>Visitors</h3>
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
                <asp:LinkButton ID="lbAddPerson" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddPerson_Click" Text="Add Person"></asp:LinkButton>
                <asp:LinkButton ID="lbAddVisitor" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddVisitor_Click" Text="Add Visitor"></asp:LinkButton>                
                <asp:LinkButton ID="lbAddFamily" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddFamily_Click" Text="Add Family"></asp:LinkButton>
            </div>
        </div>
    </div>

    <asp:Panel ID="AddFamilyPanel" runat="server" CssClass="AddFamily">
        <div class="row-fluid attended-checkin-header">
            <div class="span3 attended-checkin-actions">
                <asp:LinkButton ID="lbAddFamilyCancel" CssClass="btn btn-large btn-primary" runat="server" OnClick="lbAddFamilyCancel_Click" Text="Cancel" CausesValidation="false"/>
            </div>

            <div class="span6">
                <h1 id="H1" runat="server">Add Family</h1>
            </div>

            <div class="span3 attended-checkin-actions">
                <asp:LinkButton ID="lbAddFamilyAdd" CssClass="btn btn-large last btn-primary" runat="server" OnClick="lbAddFamilyAdd_Click" Text="Add" />
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
<%--        <div id="peopleDiv" runat="server">
        <div class="row-fluid attended-checkin-body person">
            <div class="span3">
                <Rock:DataTextBox ID="tbFirstName1" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
            </div>
            <div class="span3">
                <Rock:DataTextBox ID="tbLastName1" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
            </div>
            <div class="span3">
                <Rock:DatePicker ID="DatePicker1" runat="server" CssClass="datePickerClass"></Rock:DatePicker>
            </div>
            <div class="span3">
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
            <div class="span3">
                <Rock:DatePicker ID="DatePicker2" runat="server"></Rock:DatePicker>
            </div>
            <div class="span3">
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
            <div class="span3">
                <Rock:DatePicker ID="DatePicker3" runat="server"></Rock:DatePicker>
            </div>
            <div class="span3">
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
            <div class="span3">
                <Rock:DatePicker ID="DatePicker4" runat="server"></Rock:DatePicker>
            </div>
            <div class="span3">
                <Rock:DataTextBox ID="tbGrade4" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
            </div>
        </div>
        </div>
        <asp:Button ID="AddButton" runat="server" Text="Add Row" OnClick="AddButton_Click" />--%>
        <div id="div1" runat="server">
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox1" runat="server" CssClass="fullBlock" Text="1"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox2" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DatePicker ID="DatePicker1" runat="server" CssClass="datePickerClass"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBAgeValidator1" runat="server" ErrorMessage="The first DOB/Age field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBAgeValidator_ClientValidate" OnServerValidate="cvDOBAgeValidator_ServerValidate" ControlToValidate="DatePicker1" />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox3" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox4" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox5" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DatePicker ID="DatePicker2" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBAgeValidator2" runat="server" ErrorMessage="The second DOB/Age field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBAgeValidator_ClientValidate" OnServerValidate="cvDOBAgeValidator_ServerValidate" ControlToValidate="DatePicker2" />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox6" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox7" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox8" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DatePicker ID="DatePicker3" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBAgeValidator3" runat="server" ErrorMessage="The third DOB/Age field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBAgeValidator_ClientValidate" OnServerValidate="cvDOBAgeValidator_ServerValidate" ControlToValidate="DatePicker3" />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox9" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox10" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox11" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DatePicker ID="DatePicker4" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBAgeValidator4" runat="server" ErrorMessage="The fourth DOB/Age field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBAgeValidator_ClientValidate" OnServerValidate="cvDOBAgeValidator_ServerValidate" ControlToValidate="DatePicker4" />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox12" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
        </div>
        <div id="div2" runat="server" visible="false">
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox13" runat="server" CssClass="fullBlock" Text="2"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox14" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DatePicker ID="DatePicker5" runat="server" CssClass="datePickerClass"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBAgeValidator5" runat="server" ErrorMessage="The first DOB/Age field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBAgeValidator_ClientValidate" OnServerValidate="cvDOBAgeValidator_ServerValidate" ControlToValidate="DatePicker5" />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox15" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox16" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox17" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DatePicker ID="DatePicker6" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBAgeValidator6" runat="server" ErrorMessage="The second DOB/Age field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBAgeValidator_ClientValidate" OnServerValidate="cvDOBAgeValidator_ServerValidate" ControlToValidate="DatePicker6" />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox18" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox19" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox20" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DatePicker ID="DatePicker7" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBAgeValidator7" runat="server" ErrorMessage="The third DOB/Age field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBAgeValidator_ClientValidate" OnServerValidate="cvDOBAgeValidator_ServerValidate" ControlToValidate="DatePicker7" />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox21" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox22" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox23" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DatePicker ID="DatePicker8" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBAgeValidator8" runat="server" ErrorMessage="The fourth DOB/Age field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBAgeValidator_ClientValidate" OnServerValidate="cvDOBAgeValidator_ServerValidate" ControlToValidate="DatePicker8" />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox24" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
        </div>
        <div id="div3" runat="server" visible="false">
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox25" runat="server" CssClass="fullBlock" Text="3"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox26" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DatePicker ID="DatePicker9" runat="server" CssClass="datePickerClass"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBAgeValidator9" runat="server" ErrorMessage="The first DOB/Age field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBAgeValidator_ClientValidate" OnServerValidate="cvDOBAgeValidator_ServerValidate" ControlToValidate="DatePicker9" />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox27" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox28" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox29" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DatePicker ID="DatePicker10" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBAgeValidator10" runat="server" ErrorMessage="The second DOB/Age field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBAgeValidator_ClientValidate" OnServerValidate="cvDOBAgeValidator_ServerValidate" ControlToValidate="DatePicker10" />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox30" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox31" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox32" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DatePicker ID="DatePicker11" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBAgeValidator11" runat="server" ErrorMessage="The third DOB/Age field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBAgeValidator_ClientValidate" OnServerValidate="cvDOBAgeValidator_ServerValidate" ControlToValidate="DatePicker11" />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox33" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
            <div class="row-fluid attended-checkin-body person">
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox34" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox35" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
                <div class="span3">
                    <Rock:DatePicker ID="DatePicker12" runat="server"></Rock:DatePicker>
                    <asp:CustomValidator ID="cvDOBAgeValidator12" runat="server" ErrorMessage="The fourth DOB/Age field is incorrect."
                        CssClass="align-middle" EnableClientScript="true" Display="None" 
                        ClientValidationFunction="cvDOBAgeValidator_ClientValidate" OnServerValidate="cvDOBAgeValidator_ServerValidate" ControlToValidate="DatePicker12" />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="DataTextBox36" runat="server" CssClass="fullBlock"></Rock:DataTextBox>
                </div>
            </div>
        </div>
        <div class="row-fluid attended-checkin-body buttons">
            <asp:LinkButton ID="PreviousButton" CssClass="btn btn-large btn-primary left-button" runat="server" OnClick="PreviousButton_Click" Text="Previous" Visible="false" />
            <asp:LinkButton ID="MoreButton" CssClass="btn btn-large btn-primary right-button" runat="server" OnClick="MoreButton_Click" Text="More" />
        </div>
        <asp:ValidationSummary ID="valSummaryBottom" runat="server" HeaderText="Please Correct the Following:" CssClass="alert alert-error block-message error alert error-modal" />
    </asp:Panel>
    <%--<asp:ModalPopupExtender ID="mpe" runat="server" TargetControlID="lbAddFamily" PopupControlID="AddFamilyPanel" CancelControlID="lbAddFamilyCancel" OkControlID="lbAddFamilyAdd" BackgroundCssClass="modalBackground"></asp:ModalPopupExtender>--%>
    <asp:ModalPopupExtender ID="mpe" runat="server" TargetControlID="lbAddFamily" PopupControlID="AddFamilyPanel" CancelControlID="lbAddFamilyCancel" BackgroundCssClass="modalBackground"></asp:ModalPopupExtender>
<%--            
<div class="bored"><Rock:DataTextBox ID="tbDOB1" runat="server" CssClass="textInline"></Rock:DataTextBox><span class="or">or</span><Rock:DataTextBox ID="tbAge1" runat="server" CssClass="textInlineLast"></Rock:DataTextBox></div>
<div class="bored"><Rock:DataTextBox ID="tbDOB2" runat="server" CssClass="textInline"></Rock:DataTextBox><span class="or">or</span><Rock:DataTextBox ID="tbAge2" runat="server" CssClass="textInlineLast"></Rock:DataTextBox></div>
<div class="bored"><Rock:DataTextBox ID="tbDOB3" runat="server" CssClass="textInline"></Rock:DataTextBox><span class="or">or</span><Rock:DataTextBox ID="tbAge3" runat="server" CssClass="textInline"></Rock:DataTextBox></div>
<div class="bored"><Rock:DataTextBox ID="tbDOB4" runat="server" CssClass="textInline"></Rock:DataTextBox><span class="or">or</span><Rock:DataTextBox ID="tbAge4" runat="server" CssClass="textInline"></Rock:DataTextBox></div>
--%>

</ContentTemplate>
</asp:UpdatePanel>
