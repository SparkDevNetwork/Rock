<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkFlowRegistration.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.Workflow.WorkFlowRegistration" %>
<style>
    .checkbox {
        padding-top: 14px;
    }

</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbNotice" runat="server" Visible="false" NotificationBoxType="Danger"/>
        <Rock:NotificationBox ID="nbWarning" runat="server" Visible="false" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlView" runat="server">

            <asp:Literal ID="lLavaOverview" runat="server" Visible="false" />
            <asp:Literal ID="lLavaOutputDebug" runat="server" Visible="false" />

            <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

            <div class="row">
                <asp:Panel id="pnlwell" runat="server">
                    <div class="panel panel-default">
                         <div class="panel-heading">
                            <h3 class="panel-title">Visit Information</h3>
                          </div>
                    <div class="panel-body">
                    <div class="col-sm-12">
                        <div class="col-sm-3">
                            <Rock:CampusPicker ID="cpCampus" runat="server" Required="true" OnSelectedIndexChanged="cpCampus_SelectedIndexChanged" AutoPostBack="true"/>
                        </div>
                        <div class="col-sm-3">
                             <Rock:RockDropDownList ID="ddlVisitInformation" runat="server" Label="Visit Information">
                                 <asp:listitem value="0" Text=""></asp:listitem>
                                 <asp:listitem value="1">Joining Class Today</asp:listitem>
                                 <asp:listitem value="2">Guest For The 1st Time</asp:listitem>
                                 <asp:listitem value="3">Guest For The 2nd Time or more</asp:listitem>
                                 <asp:listitem value="4">Interested in Houston's First Membership</asp:listitem>
                             </Rock:RockDropDownList>
                        </div>
                        <div class="col-sm-3">
                             <Rock:RockDropDownList ID="ddlLifeBibleStudy" runat="server" Label="Life Bible Study">
                                 <asp:listitem value="0" Text=""></asp:listitem>
                             </Rock:RockDropDownList>
                        </div>
                        <div class="col-sm-3">
                             <Rock:RockDropDownList ID="ddlWorshipHour" runat="server" Label="Worship Hour">
                                 <asp:listitem value="0" Text=""></asp:listitem>
                             </Rock:RockDropDownList>
                        </div>
						<div class="col-sm-12">
                             <Rock:RockTextBox runat="server" ID="tbClass" MaxLength="60"  Required="true" Label="Class you attended" /> 
                        </div>
                    </div>
                    </div>
                </div>
                </asp:Panel>
            </div>

             <div class="row">
            <div class="panel panel-default">
                 <div class="panel-heading">
                            <h3 class="panel-title">Person Information</h3>
                          </div>
              <div class="panel-body">
                                        
                <asp:Panel id="pnlCol1" runat="server">
                <div class="row">
                     <div class="col-sm-6">
                        <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" Required="true" ></Rock:RockTextBox>
                     </div>
                     <div class="col-sm-6">
                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" Required="true" ></Rock:RockTextBox>
                     </div>
                 </div>
                 <div class="row">
                    <asp:Panel ID="pnlHomePhone" runat="server" >
                        <div class="col-sm-6">
                            <Rock:PhoneNumberBox ID="pnHome" runat="server" Label="Home Phone" />
                        </div>
                        
                 </asp:Panel>
                    <asp:Panel ID="pnlCellPhone" runat="server" >
                        <div class="col-sm-4">
                            <Rock:PhoneNumberBox ID="pnCell" runat="server" Label="Cell Phone" />
                        </div>
                        <div class="col-sm-1">
                            <Rock:RockCheckBox ID="cbSms"  runat="server" Label="" Text="Enable SMS" />
                        </div>
                    </asp:Panel>
                 </div>

                 <div class="row">
                    <div class="col-sm-6">
                        <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" Required="true" ></Rock:EmailBox>
                    </div>
                    <div class="col-sm-6">
                        <Rock:RockDropDownList ID="ddlMaritalStatus" runat="server" Label="Marital Status" OnSelectedIndexChanged="ddlRelationship_SelectedIndexChanged" AutoPostBack="true" Required="true"  />
                    </div>
                 </div>
				
			
                 <Rock:AddressControl ID="acAddress" runat="server" Label="Address" />
				
					
					 <div class="row">
                        <div class="col-sm-6">
                           <Rock:DatePicker runat="server" ID="dpBirthdate" Required="true" Label="Birthdate" />
						</div>
                         <div class="col-sm-6">
                           <Rock:DatePicker runat="server" ID="dpAnniversaryDate" Required="true" Label="Anniversary Date" Visible="false" />
                           <Rock:DatePicker runat="server" ID="dpWeddingDate" Required="true" Label="Wedding Date" Visible="false" />
						</div>
					 </div>
					 
                </asp:Panel>
                   </div>
                 </div>
                </div>

                <asp:Panel ID="pnlCol2" runat="server"  Visible="false" CssClass="" >
                 <div class="row">   
                <div class="panel panel-default">
                  <div class="panel-heading">
                    <asp:Literal ID="ltrMarital" runat="server"></asp:Literal>
                  </div>
                    
                  <div class="panel-body">
                     <div class="row">
                        <div class="col-sm-6">
                            <Rock:RockTextBox ID="tbSpouseFirstName" runat="server" Label="First Name" ></Rock:RockTextBox>
                        </div>
                        <div class="col-sm-6">
                            <Rock:RockTextBox ID="tbSpouseLastName" runat="server" Label="Last Name" ></Rock:RockTextBox>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-sm-7">
                            <Rock:PhoneNumberBox ID="pnSpouseCell" runat="server" Label="Cell Phone" />
                        </div>
                        <div class="col-sm-5">
                            <Rock:RockCheckBox ID="cbSpouseSms"  runat="server" Label="" Text="Enable SMS" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-sm-6">
                        <Rock:EmailBox ID="tbSpouseEmail" runat="server" Label="Email" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <Rock:AddressControl ID="acFianceAddress" runat="server" Label="Address" />
                        </div>
                        
                         </div>
                    </div>
                      </div>
                </div>
                </asp:Panel>

            <div class="actions">
                <asp:LinkButton ID="btnRegister" runat="server" CssClass="btn btn-primary btn-lg" OnClick="btnRegister_Click" />
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlResult" runat="server" Visible="false">
            <asp:Literal ID="lResult" runat="server" />
            <asp:Literal ID="lResultDebug" runat="server" />
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
