<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilySelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.FamilySelect" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>

<script type="text/javascript">

    function setControlEvents() {
        // give user instant feedback, server side object needs to set class though
        $('.family').unbind('click').on('click', function () {
            $('.family').removeClass('active');
            $(this).toggleClass('active');
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
            } else {
                $('#hfSelectedVisitor').val(buttonId + selectedIds);
            }
            return false;
        });

        function cvBirthDateValidator_ClientValidate(sender, args) {
            // check the birthdate against a date regex
            alert('checking date');
            args.IsValid = args.Value.match(/^(\d{1,2})[- /.](\d{1,2})[- /.](\d{2,4})$/);        
        };
    };

    $(document).ready(function () { setControlEvents(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(setControlEvents);

</script>


<asp:UpdatePanel ID="upContent" runat="server" UpdateMode="Conditional">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />
    <asp:HiddenField ID="personVisitorType" runat="server" />
    <asp:HiddenField ID="hfSelectedPerson" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hfSelectedVisitor" runat="server" ClientIDMode="Static" />
        
    <asp:Panel ID="pnlFamilySelect" runat="server">

        <div class="row-fluid attended-checkin-header">
            <div class="span3 attended-checkin-actions">
                <asp:LinkButton ID="lbBack" CssClass="btn btn-large btn-primary" runat="server" OnClick="lbBack_Click" Text="Back" CausesValidation="false"/>
            </div>

            <div class="span6">
                <h1 id="familyTitle" runat="server">Search Results</h1>
            </div>

            <div class="span3 attended-checkin-actions">
                <asp:LinkButton ID="lbNext" CssClass="btn btn-large last btn-primary" runat="server" OnClick="lbNext_Click" Text="Next" CausesValidation="false" />
            </div>
        </div>
                
        <div class="row-fluid attended-checkin-body">
            
            <asp:UpdatePanel ID="pnlSelectFamily" runat="server" UpdateMode="Conditional" class="span3 family-div">
            <ContentTemplate>
                
                <div class="attended-checkin-body-container">
                    <h3>Families</h3>
                    <asp:ListView ID="lvFamily" runat="server" OnPagePropertiesChanging="lvFamily_PagePropertiesChanging" OnItemCommand="lvFamily_ItemCommand" OnItemDataBound="lvFamily_ItemDataBound" >
                    <ItemTemplate>                            
                            <asp:LinkButton ID="lbSelectFamily" runat="server" data-selected='<%# Eval("Selected") %>' CommandArgument='<%# Eval("Group.Id") %>'
                                CssClass="btn btn-primary btn-large btn-block btn-checkin-select family" CausesValidation="false">
                                <%# Eval("Caption") %><br /><span class='checkin-sub-title'><%# Eval("SubCaption") %></span>
                            </asp:LinkButton>
                    </ItemTemplate>
                    </asp:ListView>
                    <asp:DataPager ID="dpPager" runat="server" PageSize="5" PagedControlID="lvFamily">
                        <Fields>
                            <asp:NextPreviousPagerField ButtonType="Button" ButtonCssClass="btn btn-primary" />
                        </Fields>
                    </asp:DataPager>
                </div>
                
            </ContentTemplate>
            </asp:UpdatePanel>

            <asp:UpdatePanel ID="pnlSelectPerson" runat="server" UpdateMode="Conditional" class="span3 person-div">
            <ContentTemplate>
                
                <div class="attended-checkin-body-container">
                    <h3>People</h3>
                    <asp:Repeater ID="repPerson" runat="server" OnItemDataBound="repPerson_ItemDataBound">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbSelectPerson" runat="server" Text='<%# Container.DataItem.ToString() %>' data-id='<%# Eval("Person.Id") %>' CommandArgument='<%# Eval("Person.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select person" CausesValidation="false" />
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                
            </ContentTemplate>
            </asp:UpdatePanel>

            <asp:UpdatePanel ID="pnlSelectVisitor" runat="server" UpdateMode="Conditional" class="span3 visitor-div">
            <ContentTemplate>
                
                <div class="attended-checkin-body-container">
                    <h3>Visitors</h3>
                    <asp:Repeater ID="repVisitors" runat="server" OnItemDataBound="repVisitors_ItemDataBound">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbSelectVisitor" runat="server" Text='<%# Container.DataItem.ToString() %>' data-id='<%# Eval("Person.Id") %>' CommandArgument='<%# Eval("Person.Id") %>' CssClass="btn btn-primary btn-large btn-block btn-checkin-select visitor" CausesValidation="false" />
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                
            </ContentTemplate>
            </asp:UpdatePanel>

            <div ID="divNothingFound" runat="server" class="span9 nothing-found-message" visible="false">
                <asp:Label ID="lblNothingFound" runat="server" Text="Please add them using one of the buttons on the right:" />
            </div>
            
            <div class="span3 add-someone">
                <div class="attended-checkin-body-container last">
                    <h3 id="actions" runat="server">Actions</h3>
                    <asp:LinkButton ID="lbAddPerson" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddPerson_Click" Text="Add Person" CausesValidation="false"></asp:LinkButton>
                    <asp:LinkButton ID="lbAddVisitor" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddVisitor_Click" Text="Add Visitor" CausesValidation="false"></asp:LinkButton>                
                    <asp:LinkButton ID="lbAddFamily" runat="server" CssClass="btn btn-primary btn-large btn-block btn-checkin-select" OnClick="lbAddFamily_Click" Text="Add Family" CausesValidation="false" />
                </div>
            </div>
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlAddPerson" runat="server" CssClass="add-person" DefaultButton="lbAddPersonSearch">

        <Rock:ModalAlert ID="maAddPerson" runat="server" />
        <div class="row-fluid attended-checkin-header">
            <div class="span3 attended-checkin-actions">
                <asp:LinkButton ID="lbAddPersonCancel" CssClass="btn btn-large btn-primary" runat="server" OnClick="lbAddPersonCancel_Click" Text="Cancel" CausesValidation="false"/>
            </div>

            <div class="span6">
                <h1><asp:Label ID="lblAddPersonHeader" runat="server"></asp:Label></h1>
            </div>

            <div class="span3 attended-checkin-actions">
                <asp:LinkButton ID="lbAddPersonSearch" CssClass="btn btn-large last btn-primary" runat="server" OnClick="lbAddPersonSearch_Click" Text="Search" />
            </div>
        </div>
		
        <div class="row-fluid attended-checkin-body searchperson">
            <div class="span3">
                <Rock:LabeledTextBox ID="tbFirstNameSearch" runat="server" CssClass="fullBlock" LabelText="First Name" />
            </div>
            <div class="span3">
                <Rock:LabeledTextBox ID="tbLastNameSearch" runat="server" CssClass="fullBlock" LabelText="Last Name" />
            </div>
            <div class="span2">
                <Rock:DatePicker ID="dpDOBSearch" runat="server" CssClass="datePicker" LabelText="DOB" />                
            </div>
            <div class="span2">
                <Rock:DataDropDownList ID="ddlGenderSearch" runat="server" CssClass="fullBlock" LabelText="Gender" />
            </div>
            <div class="span2">
                <Rock:DataDropDownList ID="ddlAbilitySearch" runat="server" CssClass="fullBlock" LabelText="Ability/Grade" />
            </div>
        </div>
        <br />
        <div class="row-fluid attended-checkin-body searchperson">
            <Rock:Grid ID="rGridPersonResults" runat="server" OnRowCommand="rGridPersonResults_AddExistingPerson" ShowActionRow="false"
                PageSize="3" DataKeyNames="Id" AllowPaging="true" AllowSorting="true">
                <Columns>
                    <asp:BoundField DataField="Id" Visible="false" />
                    <asp:BoundField DataField="FirstName" HeaderText="First Name" SortExpression="FirstName"/>
                    <asp:BoundField DataField="LastName" HeaderText="Last Name" SortExpression="LastName" />
                    <asp:BoundField DataField="BirthDate" HeaderText="DOB" SortExpression="BirthDate" DataFormatString="{0:d}" HtmlEncode="false" />
                    <asp:BoundField DataField="Gender" HeaderText="Gender" SortExpression="Gender" />
                    <asp:BoundField DataField="Attribute" HeaderText="Ability/Grade" SortExpression="Attribute" />
                    <asp:TemplateField HeaderText="Add">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbAdd" runat="server" CssClass="btn btn-large btn-primary" CommandName="Add" 
                                Text="Add" CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"><i class="icon-plus"></i>
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </Rock:Grid>
        </div>

        <div class="row-fluid attended-checkin-body searchperson">
            <asp:LinkButton ID="lbAddNewPerson" runat="server" Text="None of these, add a new person."
                CssClass="btn btn-large btn-primary" Visible="false" OnClick="lbAddNewPerson_Click">
            </asp:LinkButton>
        </div>
    </asp:Panel>
    <asp:ModalPopupExtender ID="mpeAddPerson" runat="server" TargetControlID="hfOpenPersonPanel" PopupControlID="pnlAddPerson" 
        CancelControlID="lbAddPersonCancel" BackgroundCssClass="modalBackground" />
    <asp:HiddenField ID="hfOpenPersonPanel" runat="server" />    

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
    
        <asp:ListView ID="lvAddFamily" runat="server" OnPagePropertiesChanging="lvAddFamily_PagePropertiesChanging">
        <LayoutTemplate>
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
                    <h3>Gender</h3>
                </div>
                <div class="span2">
                    <h3>Ability/Grade</h3>
                </div>                
            </div>
            <asp:PlaceHolder ID="itemPlaceholder" runat="server" />
        </LayoutTemplate>
        <ItemTemplate>
            <div class="row-fluid attended-checkin-body searchperson">
                <div class="span3">
                    <Rock:DataTextBox ID="tbFirstName" runat="server" CssClass="fullBlock" Text='<%# Eval("FirstName") %>'
                        SourceTypeName="Rock.Model.Person, Rock"  />
                </div>
                <div class="span3">
                    <Rock:DataTextBox ID="tbLastName" runat="server" CssClass="fullBlock" Text='<%# Eval("LastName") %>'
                        SourceTypeName="Rock.Model.Person, Rock"  />
                </div>
                <div class="span2">
                    <Rock:DatePicker ID="dpBirthDate" runat="server" />                    
                </div>
                <div class="span2">
                    <Rock:LabeledDropDownList ID="ddlGender" runat="server" CssClass="fullBlock" AppendDataBoundItems="True"
                        OnDataBinding="ddlGender_DataBinding" />                    
                </div>
                <div class="span2">
                    <Rock:LabeledDropDownList ID="ddlAbilityGrade" runat="server" CssClass="fullBlock" AppendDataBoundItems="True"
                        OnDataBinding="ddlAbilityGrade_DataBinding" />                    
                </div>
            </div>
        </ItemTemplate>        
        </asp:ListView>

        <div class="row-fluid attended-checkin-body searchperson">
            <asp:DataPager ID="dpAddFamily" runat="server" PageSize="2" PagedControlID="lvAddFamily">
                <Fields>
                    <asp:NextPreviousPagerField ButtonType="Button" ButtonCssClass="btn btn-primary" />
                </Fields>
            </asp:DataPager>
        </div>

    </asp:Panel>
    <asp:ModalPopupExtender ID="mpeAddFamily" runat="server" TargetControlID="hfOpenFamilyPanel" PopupControlID="pnlAddFamily"
        CancelControlID="lbAddFamilyCancel" BackgroundCssClass="modalBackground" />
    <asp:HiddenField ID="hfOpenFamilyPanel" runat="server" />

</ContentTemplate>
</asp:UpdatePanel>
