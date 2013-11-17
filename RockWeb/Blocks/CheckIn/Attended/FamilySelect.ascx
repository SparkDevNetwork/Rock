<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilySelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.FamilySelect" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>

<asp:UpdatePanel ID="pnlContent" runat="server" UpdateMode="Conditional">
<ContentTemplate>

    <asp:Panel ID="pnlSelections" runat="server" CssClass="attended" >

        <Rock:ModalAlert ID="maWarning" runat="server" />
        <asp:HiddenField ID="personVisitorType" runat="server" />
        
        <div class="row checkin-header">
            <div class="col-sm-3 checkin-actions">
                <Rock:BootstrapButton ID="lbBack" CssClass="btn btn-lg btn-primary" runat="server" OnClick="lbBack_Click" 
                    EnableViewState="false" CausesValidation="False" Text="Back" />
            </div>

            <div class="col-sm-6">                
                <h1 id="lblFamilyTitle" runat="server">Search Results</h1>
            </div>

            <div class="col-sm-3 checkin-actions">
                <Rock:BootstrapButton ID="lbNext" CssClass="btn btn-lg btn-primary" runat="server" OnClick="lbNext_Click" 
                    EnableViewState="false" CausesValidation="False" Text="Next" />
            </div>
        </div>
                
        <div class="row checkin-body">
            <div class="col-md-3">
                <asp:UpdatePanel ID="pnlFamily" runat="server" UpdateMode="Conditional">
                <ContentTemplate>                
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
                </ContentTemplate>
                </asp:UpdatePanel>
            </div>

            <div class="col-md-3">
                <asp:UpdatePanel ID="pnlPerson" runat="server" UpdateMode="Conditional"> 
                <ContentTemplate>
			        <asp:HiddenField ID="hfSelectedPerson" runat="server" ClientIDMode="Static" />

			        <h3>People</h3>                    
			        <asp:ListView ID="lvPerson" runat="server" OnItemDataBound="lvPerson_ItemDataBound" OnPagePropertiesChanging="lvPerson_PagePropertiesChanging" >
				        <ItemTemplate>                            
					        <asp:LinkButton ID="lbSelectPerson" runat="server" data-id='<%# Eval("Person.Id") %>'
						        CssClass="btn btn-primary btn-lg btn-block btn-checkin-select person">
						        <%# Eval("Person.FullName") %><br />
						        <span class='checkin-sub-title'>
							        Birthday: <%# Eval("Person.BirthMonth") + "/" + Eval("Person.BirthDay") + " " ?? "N/A " %> 
                                    <%# Convert.ToInt32( Eval( "Person.Age" ) ) <= 18 ? "Age: " + Eval( "Person.Age" ) : string.Empty %>
						        </span>
					        </asp:LinkButton>
				        </ItemTemplate>    
				        <EmptyDataTemplate>
					        <div runat="server" class="nothing-eligible">
						        <asp:Literal ID="lblPersonTitle" runat="server" Text="No one in this family is eligible to check-in." />
					        </div>                            
				        </EmptyDataTemplate>              
			        </asp:ListView>
			        <asp:DataPager ID="dpPersonPager" runat="server" PageSize="5" PagedControlID="lvPerson">
				        <Fields>
					        <asp:NextPreviousPagerField ButtonType="Button" ButtonCssClass="btn btn-lg btn-primary btn-checkin-select" />
				        </Fields>
			        </asp:DataPager>
                </ContentTemplate>
                </asp:UpdatePanel>
            </div>

            <div class="col-md-3">
                <asp:UpdatePanel ID="pnlVisitor" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:HiddenField ID="hfSelectedVisitor" runat="server" ClientIDMode="Static" />

			        <h3>Visitors</h3>
			        <asp:ListView ID="lvVisitor" runat="server" OnItemDataBound="lvVisitor_ItemDataBound" OnPagePropertiesChanging="lvVisitor_PagePropertiesChanging">
				        <ItemTemplate>
					        <asp:LinkButton ID="lbSelectVisitor" runat="server" data-id='<%# Eval("Person.Id") %>' 
						        CssClass="btn btn-primary btn-lg btn-block btn-checkin-select visitor">
						        <%# Eval("Person.FullName") %><br />
						        <span class='checkin-sub-title'>Birthday: <%# Eval("Person.BirthMonth") + "/" + Eval("Person.BirthDay") ?? "N/A" %></span>
					        </asp:LinkButton>
				        </ItemTemplate>
			        </asp:ListView>
			        <asp:DataPager ID="dpVisitorPager" runat="server" PageSize="5" PagedControlID="lvVisitor">
				        <Fields>
					        <asp:NextPreviousPagerField ButtonType="Button" ButtonCssClass="btn btn-lg btn-primary btn-checkin-select" />
				        </Fields>
			        </asp:DataPager>
                </ContentTemplate>
                </asp:UpdatePanel>
            </div>

            <div ID="divNothingFound" runat="server" class="col-md-9" visible="false">
                <div class="col-md-3"></div>
                <div class="col-md-9 nothing-eligible">
                    <asp:Literal ID="lblNothingFound" runat="server" Text="Please add them using one of the buttons on the right:" EnableViewState="false" />
                </div>
            </div>

            <div class="col-md-3">
                <h3 id="actions" runat="server">Actions</h3>
                <asp:LinkButton ID="lbAddPerson" runat="server" CssClass="btn btn-primary btn-lg btn-block btn-checkin-select" OnClick="lbAddPerson_Click" Text="Add Person" CausesValidation="false" EnableViewState="false" />
                <asp:LinkButton ID="lbAddVisitor" runat="server" CssClass="btn btn-primary btn-lg btn-block btn-checkin-select" OnClick="lbAddVisitor_Click" Text="Add Visitor" CausesValidation="false" EnableViewState="false" />
                <asp:LinkButton ID="lbAddFamily" runat="server" CssClass="btn btn-primary btn-lg btn-block btn-checkin-select" OnClick="lbAddFamily_Click" Text="Add Family" CausesValidation="false" EnableViewState="false" />
            </div>

        </div>

    </asp:Panel>

    <asp:Panel ID="pnlAddPerson" runat="server" CssClass="attended modal-foreground" DefaultButton="lbPersonSearch" style="display:none">
        <asp:ModalPopupExtender ID="mpeAddPerson" runat="server" BehaviorID="mpeAddPerson" TargetControlID="hfOpenPersonPanel" PopupControlID="pnlAddPerson" CancelControlID="lbAddPersonCancel" BackgroundCssClass="attended modal-background" />
        <asp:HiddenField ID="hfOpenPersonPanel" runat="server" />   

        <div class="row checkin-header">
            <div class="checkin-actions">
                <div class="col-sm-3">
                    <Rock:BootstrapButton ID="lbAddPersonCancel" runat="server" CssClass="btn btn-lg btn-primary cancel" Text="Cancel" EnableViewState="false" />
                </div>

                <div class="col-sm-6">
                    <h2><asp:Literal ID="lblAddPersonHeader" runat="server" EnableViewState="false" /></h2>
                </div>

                <div class="col-sm-3">
                    <Rock:BootstrapButton ID="lbPersonSearch" runat="server" CssClass="btn btn-lg btn-primary" OnClick="lbPersonSearch_Click" Text="Search" EnableViewState="false" />
                </div>
            </div>
        </div>
		
        <div class="checkin-body">
            <div class="row">               
                <div class="col-md-2">
                    <Rock:RockTextBox ID="tbFirstNameSearch" runat="server" CssClass="col-md-12" Label="First Name" ValidationGroup="Person" Text="" />
                </div>
                <div class="col-md-3">
                    <Rock:RockTextBox ID="tbLastNameSearch" runat="server" CssClass="col-md-12" Label="Last Name" ValidationGroup="Person" Text="" />
                </div>
                <div class="col-md-2">
                    <Rock:DatePicker ID="dpDOBSearch" runat="server" Label="DOB" ValidationGroup="Person" CssClass="col-md-12" />
                </div>
                <div class="col-md-2">
                    <Rock:RockDropDownList ID="ddlGenderSearch" runat="server" ValidationGroup="Person" CssClass="col-md-12" Label="Gender" />
                </div>
                <div class="col-md-3">
                    <Rock:RockDropDownList ID="ddlAbilitySearch" runat="server" CssClass="col-md-12" Label="Ability/Grade" />
                </div>
            </div>
            
            <div class="row">
                <asp:UpdatePanel ID="pnlPersonSearch" runat="server">
                <ContentTemplate>
                    <Rock:Grid ID="rGridPersonResults" runat="server" OnRowCommand="rGridPersonResults_AddExistingPerson"
                        OnGridRebind="rGridPersonResults_GridRebind" ShowActionRow="false" PageSize="4" DataKeyNames="Id" AllowSorting="true">
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
                                        Text="Add" CommandArgument="<%# ((GridViewRow) Container).RowIndex %>" CausesValidation="false"><i class="fa fa-plus"></i>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </Rock:Grid>
                </ContentTemplate>
                </asp:UpdatePanel>
            </div>

            <div class="row">
                <asp:LinkButton ID="lbSavePerson" runat="server" Text="None of these, add a new person." CssClass="btn btn-lg btn-primary btn-checkin-select" ValidationGroup="Person" CausesValidation="true" OnClick="lbSavePerson_Click" />
            </div>
            
        </div>
    </asp:Panel>
    
    <asp:Panel ID="pnlAddFamily" runat="server" CssClass="attended modal-foreground" style="display:none">
        <asp:ModalPopupExtender ID="mpeAddFamily" runat="server" BehaviorID="mpeAddFamily" TargetControlID="hfOpenFamilyPanel" PopupControlID="pnlAddFamily" CancelControlID="lbAddFamilyCancel" BackgroundCssClass="attended modal-background" />
        <asp:HiddenField ID="hfOpenFamilyPanel" runat="server" />

        <div class="row checkin-header">
            <div class="col-sm-3 checkin-actions">
                <Rock:BootstrapButton ID="lbAddFamilyCancel" CssClass="btn btn-lg btn-primary cancel" runat="server" Text="Cancel" CausesValidation="false" EnableViewState="false" />
            </div>

            <div class="col-sm-6">
                <h2>Add Family</h2>
            </div>

            <div class="col-sm-3 checkin-actions">
                <Rock:BootstrapButton ID="lbSaveFamily" CssClass="btn btn-lg btn-primary" runat="server" OnClick="lbSaveFamily_Click" Text="Save" EnableViewState="false" ValidationGroup="Family" />
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
                        <Rock:RockTextBox ID="tbFirstName" runat="server" RequiredErrorMessage="First Name is Required"  Text='<%# ((NewPerson)Container.DataItem).FirstName %>' ValidationGroup="Family" />
                    </div>
                    <div class="col-md-3">
                        <Rock:RockTextBox ID="tbLastName" runat="server" RequiredErrorMessage="Last Name is Required" Text='<%# ((NewPerson)Container.DataItem).LastName %>' ValidationGroup="Family" />
                    </div>
                    <div class="col-md-2">
                        <Rock:DatePicker ID="dpBirthDate" runat="server" RequiredErrorMessage="DOB is Required" SelectedDate='<%# ((NewPerson)Container.DataItem).BirthDate %>' ValidationGroup="Family"  />
                    </div>
                    <div class="col-md-2">
                        <Rock:RockDropDownList ID="ddlGender" runat="server" RequiredErrorMessage="Gender is Required" ValidationGroup="Family" />
                    </div>
                    <div class="col-md-3">
                        <Rock:RockDropDownList ID="ddlAbilityGrade" runat="server" />
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

</ContentTemplate>
</asp:UpdatePanel>

<script type="text/javascript">

    function setControlEvents() {

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

        $('#<%= pnlFamily.ClientID %>').on('click', 'a', function () {
            $('.nothing-eligible').html("<i class='fa fa-spinner fa-spin fa-2x'></i>");
        });
                
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