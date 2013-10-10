<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilySelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.FamilySelect" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<link rel="stylesheet" type="text/css"  href="../../Styles/Kendo/kendo.common.min.css" />
<%--<link rel="stylesheet" type="text/css"  href="../../Styles/Kendo/kendo.flat.less" />--%>
<script type="text/javascript">

    function setControlEvents() {

        $('.family').unbind('click').on('click', function () {
            $('.family').removeClass('active');
            $(this).addClass('active');
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

        $find("mpeAddPerson").add_shown(function () {
            $find("mpeAddPerson")._backgroundElement.onclick = function () {
                $find("mpeAddPerson").hide();
            }
        });

        $find("mpeAddFamily").add_shown(function () {
            $find("mpeAddFamily")._backgroundElement.onclick = function () {
                $find("mpeAddFamily").hide();
            }
        });

    };

    $(document).ready(function () { setControlEvents(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(setControlEvents);

</script>

<asp:UpdatePanel ID="upContent" runat="server" UpdateMode="Conditional">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />
    <asp:HiddenField ID="personVisitorType" runat="server" />
        
    <asp:Panel ID="pnlFamilySelect" runat="server" CssClass="attended">

        <div class="row checkin-header">
            <div class="col-md-3 checkin-actions">
                <asp:LinkButton ID="lbBack" CssClass="btn btn-lg btn-primary" runat="server" OnClick="lbBack_Click" Text="Back" CausesValidation="false"/>
            </div>

            <div class="col-md-6">                
                <h1 id="lblFamilyTitle" runat="server">Search Results</h1>
            </div>

            <div class="col-md-3 checkin-actions">
                <asp:LinkButton ID="lbNext" CssClass="btn btn-lg btn-primary" runat="server" OnClick="lbNext_Click" Text="Next" CausesValidation="false" />
            </div>
        </div>
                
        <div class="row checkin-body">
            
            <asp:UpdatePanel ID="pnlSelectFamily" runat="server" UpdateMode="Conditional" class="col-md-3">
            <ContentTemplate>
                
                <div class="">
                    <h3>Families</h3>
                    <asp:ListView ID="lvFamily" runat="server" OnPagePropertiesChanging="lvFamily_PagePropertiesChanging" 
                        OnItemCommand="lvFamily_ItemCommand" OnItemDataBound="lvFamily_ItemDataBound" >
                    <ItemTemplate>                            
                            <asp:LinkButton ID="lbSelectFamily" runat="server" CommandArgument='<%# Eval("Group.Id") %>'
                                CssClass="btn btn-primary btn-lg btn-block btn-checkin-select family" CausesValidation="false">
                                <%# Eval("Caption") %><br /><span class='checkin-sub-title'><%# Eval("SubCaption") %></span>
                            </asp:LinkButton>
                    </ItemTemplate>                    
                    </asp:ListView>
                    <asp:DataPager ID="dpFamilyPager" runat="server" PageSize="5" PagedControlID="lvFamily">
                        <Fields>
                            <asp:NextPreviousPagerField ButtonType="Button" ButtonCssClass="btn btn-lg btn-primary btn-checkin-select" />
                        </Fields>
                    </asp:DataPager>
                </div>
                
            </ContentTemplate>
            </asp:UpdatePanel>

            <asp:UpdatePanel ID="pnlSelectPerson" runat="server" UpdateMode="Conditional" class="col-md-3">
            <ContentTemplate>
                <asp:HiddenField ID="hfSelectedPerson" runat="server" ClientIDMode="Static" />

                <div class="">
                    <h3>People</h3>                    
                    <asp:ListView ID="lvPerson" runat="server" OnItemDataBound="lvPerson_ItemDataBound" OnPagePropertiesChanging="lvPerson_PagePropertiesChanging" >
                        <ItemTemplate>                            
                            <asp:LinkButton ID="lbSelectPerson" runat="server" data-id='<%# Eval("Person.Id") %>'
                                CssClass="btn btn-primary btn-lg btn-block btn-checkin-select person">
                                <%# Eval("Person.FullName") %><br />
                                <span class='checkin-sub-title'>
                                    Birthday: <%# Eval("Person.BirthMonth") + "/" + Eval("Person.BirthDay") ?? "N/A" %> 
                                </span>
                            </asp:LinkButton>
                        </ItemTemplate>    
                        <EmptyDataTemplate>
                            <div runat="server" class="nothing-eligible">
                                <asp:Label ID="lblPersonTitle" runat="server" Text="No one in this family is eligible to check-in." />
                            </div>                            
                        </EmptyDataTemplate>              
                    </asp:ListView>
                    <asp:DataPager ID="dpPersonPager" runat="server" PageSize="5" PagedControlID="lvPerson">
                        <Fields>
                            <asp:NextPreviousPagerField ButtonType="Button" ButtonCssClass="btn btn-lg btn-primary btn-checkin-select" />
                        </Fields>
                    </asp:DataPager>
                </div>
                
            </ContentTemplate>
            </asp:UpdatePanel>

            <asp:UpdatePanel ID="pnlSelectVisitor" runat="server" UpdateMode="Conditional" class="col-md-3">
            <ContentTemplate>
                <asp:HiddenField ID="hfSelectedVisitor" runat="server" ClientIDMode="Static" />

                <div class="">
                    <h3>Visitors</h3>
                    <asp:ListView ID="lvVisitor" runat="server" OnItemDataBound="lvVisitor_ItemDataBound" OnPagePropertiesChanging="lvVisitor_PagePropertiesChanging">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbSelectVisitor" runat="server" data-id='<%# Eval("Person.Id") %>' 
                                CssClass="btn btn-primary btn-lg btn-block btn-checkin-select visitor">
                                <%# Eval("Person.FullName") %><br />
                                <span class='checkin-sub-title'>Birthday: <%# Eval("Person.BirthMonth") + "/" + Eval("Person.BirthDay") ?? "Not entered" %></span>
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:ListView>
                    <asp:DataPager ID="dpVisitorPager" runat="server" PageSize="5" PagedControlID="lvVisitor">
                        <Fields>
                            <asp:NextPreviousPagerField ButtonType="Button" ButtonCssClass="btn btn-lg btn-primary btn-checkin-select" />
                        </Fields>
                    </asp:DataPager>
                </div>
                
            </ContentTemplate>
            </asp:UpdatePanel>

            <div ID="divNothingFound" runat="server" class="col-md-9" visible="false">
                <div class="col-md-3"></div>
                <div class="col-md-9 nothing-eligible">
                    <asp:Label ID="lblNothingFound" runat="server" Text="Please add them using one of the buttons on the right:" />
                </div>
            </div>
            <div class="col-md-3">
                <h3 id="actions" runat="server">Actions</h3>
                <asp:LinkButton ID="lbAddPerson" runat="server" CssClass="btn btn-primary btn-lg btn-block btn-checkin-select" OnClick="lbAddPerson_Click" Text="Add Person" CausesValidation="false"></asp:LinkButton>
                <asp:LinkButton ID="lbAddVisitor" runat="server" CssClass="btn btn-primary btn-lg btn-block btn-checkin-select" OnClick="lbAddVisitor_Click" Text="Add Visitor" CausesValidation="false"></asp:LinkButton>                
                <asp:LinkButton ID="lbAddFamily" runat="server" CssClass="btn btn-primary btn-lg btn-block btn-checkin-select" OnClick="lbAddFamily_Click" Text="Add Family" CausesValidation="false" />
            </div>
        </div>

    </asp:Panel>

    <asp:Panel ID="pnlAddPerson" runat="server" CssClass="attended modal-foreground" DefaultButton="lbAddPersonSearch">
        
        <Rock:ModalAlert ID="maAddPerson" runat="server" />
        <div class="row checkin-header">
            <div class="checkin-actions">
                <div class="col-md-3">
                    <asp:LinkButton ID="lbAddPersonCancel" CssClass="btn btn-lg btn-primary" runat="server" Text="Cancel" CausesValidation="false"/>
                </div>

                <div class="col-md-6">
                    <h2><asp:Label ID="lblAddPersonHeader" runat="server"></asp:Label></h2>
                </div>

                <div class="col-md-3">
                    <asp:LinkButton ID="lbAddPersonSearch" CssClass="btn btn-lg btn-primary" runat="server" OnClick="lbAddPersonSearch_Click" Text="Search" CausesValidation="false" />
                </div>
            </div>
        </div>
		
        <div class="checkin-body">
            <div class="row">
                <div class="col-md-2">
                    <Rock:RockTextBox ID="tbFirstNameSearch" runat="server" CssClass="col-md-12" Label="First Name" Required="true" RequiredErrorMessage="Required" ValidationGroup="test" />
                </div>
                <div class="col-md-3">
                    <Rock:RockTextBox ID="tbLastNameSearch" runat="server" CssClass="col-md-12" Label="Last Name" Required="true" RequiredErrorMessage="Required" ValidationGroup="test" />
                </div>
                <div class="col-md-2">
                    <Rock:DatePicker ID="dpDOBSearch" runat="server" Label="DOB" CssClass="col-md-12" Required="true" RequiredErrorMessage="Required" ValidationGroup="test" />
                </div>
                <div class="col-md-2">
                    <Rock:DataDropDownList ID="ddlGenderSearch" runat="server" CssClass="col-md-12" Label="Gender" Required="true" RequiredErrorMessage="Required" ValidationGroup="test" />
                </div>
                <div class="col-md-3">
                    <Rock:DataDropDownList ID="ddlAbilitySearch" runat="server" CssClass="col-md-12" Label="Ability/Grade" />
                </div>
            </div>
            
            <div class="row">
                <Rock:Grid ID="rGridPersonResults" runat="server" OnRowCommand="rGridPersonResults_AddExistingPerson"
                    ShowActionRow="false" PageSize="3" DataKeyNames="Id" AllowSorting="true">
                    <Columns>
                        <asp:BoundField DataField="Id" Visible="false" />
                        <asp:BoundField DataField="FirstName" HeaderText="First Name" SortExpression="FirstName"/>
                        <asp:BoundField DataField="LastName" HeaderText="Last Name" SortExpression="LastName" />
                        <asp:BoundField DataField="BirthDate" HeaderText="DOB" SortExpression="BirthDate" DataFormatString="{0:MM/dd/yy}" HtmlEncode="false" />
                        <asp:BoundField DataField="Age" HeaderText="Age" SortExpression="Age" />
                        <asp:BoundField DataField="Gender" HeaderText="Gender" SortExpression="Gender" />
                        <asp:BoundField DataField="Attribute" HeaderText="Ability/Grade" SortExpression="Attribute" />
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:LinkButton ID="lbAdd" runat="server" CssClass="btn btn-lg btn-primary" CommandName="Add" 
                                    Text="Add" CommandArgument="<%# ((GridViewRow) Container).RowIndex %>" CausesValidation="false"><i class="icon-plus"></i>
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </Rock:Grid>
            </div>

            <div class="row">
                <asp:LinkButton ID="lbAddNewPerson" runat="server" Text="None of these, add a new person."
                    CssClass="btn btn-lg btn-primary btn-checkin-select" Visible="false" OnClick="lbAddNewPerson_Click" ValidationGroup="test">
                </asp:LinkButton>
            </div>
        </div>
    </asp:Panel>
    <asp:ModalPopupExtender ID="mpeAddPerson" runat="server" BehaviorID="mpeAddPerson" TargetControlID="hfOpenPersonPanel" PopupControlID="pnlAddPerson" 
        CancelControlID="lbAddPersonCancel" BackgroundCssClass="attended modal-background" />
    <asp:HiddenField ID="hfOpenPersonPanel" runat="server" />    

    <asp:Panel ID="pnlAddFamily" runat="server" CssClass="attended modal-foreground">

        <div class="row checkin-header">
            <div class="col-md-3 checkin-actions">
                <asp:LinkButton ID="lbAddFamilyCancel" CssClass="btn btn-lg btn-primary" runat="server" Text="Cancel" CausesValidation="false" />
            </div>

            <div class="col-md-6">
                <h2>Add Family</h2>
            </div>

            <div class="col-md-3 checkin-actions">
                <asp:LinkButton ID="lbAddFamilySave" CssClass="btn btn-lg btn-primary" runat="server" OnClick="lbAddFamilySave_Click" Text="Save" />
            </div>
        </div>        
    
        <div class="checkin-body">
            <asp:ListView ID="lvAddFamily" runat="server" OnPagePropertiesChanging="lvAddFamily_PagePropertiesChanging" OnItemDataBound="lvAddFamily_ItemDataBound" >
            <LayoutTemplate>
                <div class="row">
                    <div class="col-md-2">
                        <h4>First Name</h4>
                    </div>
                    <div class="col-md-3">
                        <h4>Last Name</h4>
                    </div>
                    <div class="col-md-2">
                        <h4>DOB</h4>
                    </div>
                    <div class="col-md-2">
                        <h4>Gender</h4>
                    </div>
                    <div class="col-md-3">
                        <h4>Ability/Grade</h4>
                    </div>                
                </div>
                <asp:PlaceHolder ID="itemPlaceholder" runat="server" />
            </LayoutTemplate>
            <ItemTemplate>
                <div class="row">
                    <div class="col-md-2">
                        <Rock:RockTextBox ID="tbFirstName" runat="server" CssClass="col-md-12" Text='<%# ((NewPerson)Container.DataItem).FirstName %>' />
                    </div>
                    <div class="col-md-3">
                        <Rock:RockTextBox ID="tbLastName" runat="server" CssClass="col-md-12" Text='<%# ((NewPerson)Container.DataItem).LastName %>' />
                    </div>
                    <div class="col-md-2">
                        <Rock:DatePicker ID="dpBirthDate" runat="server" CssClass="col-md-12" SelectedDate='<%# ((NewPerson)Container.DataItem).BirthDate %>' />
                    </div>
                    <div class="col-md-2">
                        <Rock:DataDropDownList ID="ddlGender" runat="server" CssClass="col-md-12" />
                    </div>
                    <div class="col-md-3">
                        <Rock:DataDropDownList ID="ddlAbilityGrade" runat="server" CssClass="col-md-12" />
                    </div>                
                </div>
            </ItemTemplate>        
            </asp:ListView>        

            <div class="row">
                <asp:DataPager ID="dpAddFamily" runat="server" PageSize="5" PagedControlID="lvAddFamily">
                    <Fields>
                        <asp:NextPreviousPagerField ButtonType="Button" ButtonCssClass="btn btn-lg btn-primary btn-checkin-select" />
                    </Fields>
                </asp:DataPager>
            </div>
        </div>

    </asp:Panel>
    <asp:ModalPopupExtender ID="mpeAddFamily" runat="server" BehaviorID="mpeAddFamily" TargetControlID="hfOpenFamilyPanel" PopupControlID="pnlAddFamily"
        CancelControlID="lbAddFamilyCancel" BackgroundCssClass="attended modal-background" />
    <asp:HiddenField ID="hfOpenFamilyPanel" runat="server" />

</ContentTemplate>
</asp:UpdatePanel>
