<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BarcodeAttendance.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.BarcodeAttendance.BarcodeAttendance" %>
<asp:UpdatePanel ID="pnlContent" runat="server">


    <ContentTemplate>
                        <asp:HiddenField ID="listMarkedAttendance" EnableViewState="true" runat="server" />


        <div class="panel panel-block">

            <div class="panel-heading clearfix">
                <h1 class="panel-title pull-left">
                    <i class="fa fa-check-square-o"></i>
                    <asp:Literal ID="lHeading" runat="server" Text="Scan Attendance" />
                </h1>
            </div>

            <div class="panel-body">
                <asp:Panel ID="pnlLeftPanel" runat="server"  >

                    
                    
                    <div class="well col-sm-5">
                        <p><b>Scan or Type ID</b></p>

                        <asp:Panel ID="pnlBarcode" runat="server" CssClass="row" DefaultButton="btnBarcode">
                            <div class="col-sm-10">
                                <asp:Textbox ID="tbBarcode" runat="server" CssClass="form-control" SourceTypeName=""/>
                            </div>

                            <div class="col-sm-2">
                                <asp:LinkButton ID="btnBarcode" runat="server" OnClientClick="return false;" CssClass="btn btn-default btn-sm" UseSubmitBehavior="false"><i class='fa fa-check-square'></i></asp:LinkButton>
                            </div>
                            
                            
                        </asp:Panel>
                        <asp:Panel ID="pnlAlert" runat="server" CssClass="row">
                            <div class="col-sm-12">
                                <Rock:NotificationBox ID="nbBarcode" runat="server" NotificationBoxType="Danger" Visible="false"/>
                            </div>
                        </asp:Panel>

                        <hr />
                        <asp:Panel ID="pnlFilters" runat="server">
                        <div class="row">
                            <asp:Panel ID="pnlGroupType" runat="server" CssClass="col-sm-12">
                                <Rock:RockDropDownList ID="bddlGroupType" runat="server" Label="Group Type" Required="true" AutoPostBack="true" OnSelectedIndexChanged="bddlGroupType_SelectionChanged" EnableViewState="true" DataTextField="Name" DataValueField="Id" Visible="false" />
                            </asp:Panel>
                            <asp:panel ID="pnlCampus" runat="server" CssClass="col-sm-6">
                                <Rock:RockDropDownList ID="bddlCampus" runat="server" Label="Campus" Required="false" AutoPostBack="true" OnSelectedIndexChanged="bddlCampus_Changed" EnableViewState="true" DataTextField="Name" DataValueField="Id"/>
                            </asp:panel>
                            <asp:panel ID="pnlParentGroup" runat="server" CssClass="col-sm-6">
                                <Rock:RockDropDownList ID="bddlParentGroup" runat="server" Label="Parent Group" Required="false" AutoPostBack="true" EnableViewState="true" DataTextField="Name" DataValueField="Id"/>
                            </asp:panel>
                            <asp:panel ID="pnlServiceDate" runat="server" CssClass="col-sm-6">
                                <Rock:DatePicker ID="datepServiceDate" runat="server" Label="Date" Required="true" EnableViewState="true" AutoPostBack="true" />
                            </asp:panel>
                            <asp:panel ID="pnlServiceTime" runat="server" CssClass="col-sm-6">
                                <Rock:RockDropDownList ID="bddlServiceTime" runat="server" Label="Schedule" Required="false" AutoPostBack="true" OnSelectedIndexChanged="cblAreas_Changed" EnableViewState="true" DataTextField="Name" DataValueField="Id"/>
                            </asp:panel>
                        </div>
                        
                        <Rock:RockListBox ID="rlb_Rooms" runat="server" Label="Locations" OnSelectedIndexChanged="rlb_Rooms_Changed" EnableViewState="true" AutoPostBack="true" DataTextField="Name" DataValueField="Id" SelectionMode="Multiple" />
                        

                        <Rock:BootstrapButton ID="btnRooms" runat="server" Text="Load Members" DataLoadingText="Loading..." CssClass="btn btn-primary" OnClick="cblRooms_Changed" />

                        <br />
                        <br />
                        <div class="row">
                            <asp:Panel ID="pnlSort" runat="server" CssClass="col-sm-12">
                            
                                <Rock:RockRadioButtonList ID="rblSort" runat="server" Label="Sort By" AutoPostBack="true" Required="true" EnableViewState="true" RepeatDirection="Horizontal" RepeatColumns="2" RepeatLayout="Flow" Visible="false">
                                    <asp:ListItem Text="FirstName LastName" Value="FirstName" Selected="true" ></asp:ListItem>
                                    <asp:ListItem Text="LastName, FirstName" Value="LastName" ></asp:ListItem>
                                    
                                </Rock:RockRadioButtonList>
                            
                            </asp:Panel>

                        </div>
                            
                        <hr />
                        
                        </asp:Panel>
                        
                        <asp:LinkButton ID="lbSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Commit Attendance" CssClass="btn btn-primary" OnClick="lbSave_Click" CausesValidation="false" />
                        <asp:LinkButton ID="lbCancel" runat="server" AccessKey="q" ToolTip="Alt+q" Text="Cancel" CssClass="btn btn-default" OnClick="lbCancel_Click" CausesValidation="false" />
                        
                        
                    
                    </div>
                    
                </asp:Panel>

                

                    <div class="col-sm-7 text-center">
                        
                            <Rock:NotificationBox ID="nbCapacity" runat="server" Title="<font size='30'>0/0</font><br />" Text="Attended / Capacity" NotificationBoxType="Success" />
                                   
                      <asp:Panel ID="pnlDetails" runat="server">  
                        <div >
                            
                            <asp:Repeater ID="tableAttendees" runat="server" >

                                <HeaderTemplate>
                                    <table id="tableAttendees" class="grid-table table table-bordered table-striped table-hover">
                                        <tr>
                                            <th scope="col">Attended</th>
                                            <th scope="col"></th>
                                            <th scope="col">ID</th>
                                            <th scope="col">Name</th>
                                            <th scope="col">Group Name</th>
                                        </tr>
                                    
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <tr id='<%# "attendeeRow_" + Eval("PersonId") %>'>
                                        <td>
                                            <asp:Label ID="labelAttended" runat="server" Text='<%# Eval("Attended") %>' />
                                        </td>
                                        <td>
                                            <asp:LinkButton ID="linkRemoveAttendance" runat="server" Enabled="false" CssClass="btn btn-danger btn-sm"><i class="fa fa-times"></i></asp:LinkButton>
                                        </td>
                                        <td>
                                            <asp:Label ID="labelID" runat="server" Text='<%# Eval("PersonId") %>' />
                                        </td>
                                        <td>
                                            <asp:Label ID="labelName" runat="server" Text='<%# Eval("Name") %>' />
                                        </td>
                                        <td>
                                            <asp:Label ID="labelGroupName" runat="server" Text='<%# Eval("GroupName") %>' />
                                        </td>
                                    </tr>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </table>
                                </FooterTemplate>
                            </asp:Repeater>
                        </div>

                        
                    
                     </asp:Panel>
                 </div>


                
            </div>

        </div>

        <!-- Delete Attendance Warning -->
        <Rock:ModalAlert ID="maDeleteAttendanceWarning" runat="server" />

        <style>
            .grid {
                height: 100%;
                min-height: 350px;
                overflow-y: scroll;
                overflow-x: hidden;
            }
        </style>

    </ContentTemplate>
    

</asp:UpdatePanel>


<script type="text/javascript">

    function pageLoad() {
        //Hopefully this gets called...
        
        //Set Event Handlers for textfield and button
        $('#<%=btnBarcode.ClientID%>').click(function () {
            var text = $('#<%=tbBarcode.ClientID%>').val();
            //console.log("Found Text: " + text);
            text = text.trim();
            text = text.replace("%", "");
            

            if (text != "") {
                var personId = Number(text);

                if ( !isNaN( personId ) ) {

                    //If not already marked attended
                    if ($('#<%=listMarkedAttendance.ClientID%>').val().indexOf("|" + personId + "|") == -1) {

                        //Add To the listMarkedAttendance Hidden Field (initial pipe should already be there)
                        $('#<%=listMarkedAttendance.ClientID%>').val($('#<%=listMarkedAttendance.ClientID%>').val() + personId + "|")

                        //Search Through Table For Rows with PersonId and Mark As Attended

                        $("#tableAttendees").find('#attendeeRow_' + personId).find("a[id$=linkRemoveAttendance]").removeClass("aspNetDisabled");
                        $("#tableAttendees").find('#attendeeRow_' + personId).find("span[id$=labelAttended]").text("-Marked-");

                        //Add 1 to Attendance Totals
                        
                        var capacityTotal = $("#tableAttendees").find("span[id$=labelAttended]").length;
                        var attendedTotal = $('#<%=listMarkedAttendance.ClientID%>').val().split("|").length - 2;

                        $('#<%=nbCapacity.ClientID%>').find("font").text(attendedTotal + "/" + capacityTotal);

                    }
                    
                }
                else {
                    //throw error NaN
                    console.log("NaN Error: " + personId);
                }
            }
            
            $('#<%=tbBarcode.ClientID%>').val("");

            //set focus and scroll back
            setTimeout(function () {
                var x = window.scrollX, y = window.scrollY;
                $('#<%=tbBarcode.ClientID%>').focus();
                window.scrollTo(x, y);
            }, 300);
        });

        //Create row click to attendance function 
        $("#tableAttendees").find("tr").click(function () {
            var personId = $(this).find("span[id$=labelID]").text();
            //console.log("PersonId selected: " + personId);

            //prefill scan text and trigger button
            $('#<%=tbBarcode.ClientID%>').val(personId);
            $('#<%=btnBarcode.ClientID%>').trigger("click");
        });
        
        //Create remove click for each attended row
        $("#tableAttendees").find("a[id$=linkRemoveAttendance]").click(function () {
            //stop tr click event from triggering
            event.stopPropagation();

            var personId = $(this).closest('tr').find("span[id$=labelID]").text();
            //console.log("PersonId to remove: " + personId);

            //Remove from the hidden 
            var newText = $('#<%=listMarkedAttendance.ClientID%>').val().split("|" + personId + "|").join("|");
            $('#<%=listMarkedAttendance.ClientID%>').val(newText);
            //console.log(newText);

            //remove Attended text and disable button
            $("#tableAttendees").find('#attendeeRow_' + personId).find("a[id$=linkRemoveAttendance]").addClass("aspNetDisabled");
            $("#tableAttendees").find('#attendeeRow_' + personId).find("span[id$=labelAttended]").text("");

            //recalculate header
            var capacityTotal = $("#tableAttendees").find("span[id$=labelAttended]").length;
            var attendedTotal = $('#<%=listMarkedAttendance.ClientID%>').val().split("|").length - 2;

            $('#<%=nbCapacity.ClientID%>').find("font").text(attendedTotal + "/" + capacityTotal);

            //set focus on barcode scanner
            
            setTimeout(function () {
                var x = window.scrollX, y = window.scrollY;
                $('#<%=tbBarcode.ClientID%>').focus();
                window.scrollTo(x, y);
            }, 300);
        });

        
    }
    
</script>