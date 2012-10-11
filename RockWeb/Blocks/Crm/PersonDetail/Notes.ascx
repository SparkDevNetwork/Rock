<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Notes.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.Notes" %>

<div class="span8 person-notes-container">
    <section id="person-notes" class="person-notes scroll-container">
        <header class="group">
            <h4>Timeline</h4>
            <a id="note-add" class="note-add btn"><i class="icon-plus"></i></a>

            <script>

                $(document).ready(function ()     

                    $('#note-add').click(function ()     
                        $('#note-entry').slideToggle("slow");
                    });

                });

            </script>

        </header>
                    
        <div id="note-entry" style="display: none;">
            <label>Note</label>
            <textarea></textarea>
 
            <div class="row-fluid">
                <div class="span4">
                    <label class="checkbox">
                        <input type="checkbox" value="">
                        Alert
                    </label>
                </div>
                <div class="span4">
                    <label class="checkbox">
                        <input type="checkbox" value="">
                        Private
                    </label>
                </div>
                <div class="span4">
                    <button class="btn btn-mini" type="button"><i class="icon-lock"></i> Security</button>
                </div>
            </div>
        </div>

        <div class="person-notes-details">
            <div class="scrollbar" style="height: 150px;">
                <div class="track" style="height: 150px;">
                    <div class="thumb" style="top: 0px; height: 126.949px;">
                        <div class="end"></div>
                    </div>
                </div>
            </div>
            <div class="viewport">
                <div class="note-container-top"></div>
                <div class="note-container overview">
                    <article class="group alert">
                        <i class="icon-comment"></i>
                        <div class="details">
                            <h5>7/14/2012 - Bob Johnson</h5>
                            Talk to security before allowing to serve in any ministry area.
                        </div>
                    </article>
                    <article class="group">
                        <i class="icon-calendar"></i>
                        <div class="details">
                            <h5>8/1/2012 - Event Registration</h5>
                            Register for Feed My Staving Puppies (Jon, Heidi, Alex and Adam)
                        </div>
                    </article>
                    <article class="group personal">
                        <i class="icon-comment"></i>
                        <div class="details">
                            <h5>7/14/2012 - Mike McClain</h5>
                            Had Lunch with Jon today to talk about using new neighborhood map.
                        </div>
                    </article>
                    <article class="group">
                        <i class="icon-envelope"></i>
                        <div class="details">
                            <h5>7/7/2012 - Email from: Dustin Tappan</h5>
                            An email was sent to Jon from Dustin Tappan on 10/31 @9:35am.
                        </div>
                    </article>
                    <article class="group">
                        <i class="icon-phone"></i>
                        <div class="details">
                            <h5>7/1/2012 - Call To Scott Merlin</h5>
                            Jon called Scott Merlin's phone, 2999, on Monday October 12th at 10:51am and talked for 10mins.
                        </div>
                    </article>
                    <article class="group">
                        <i class="icon-comment"></i>
                        <div class="details">
                            <h5>7/1/2012 - Bob Johnson</h5>
                            Talked to Jon about joining the Security Team and gave him the forms needed to apply.
                        </div>
                    </article>
                    <article class="group">
                        <i class="icon-calendar"></i>
                        <div class="details">
                            <h5>8/1/2012 - Event Registration</h5>
                            Register for Feed My Staving Puppies (Jon, Heidi, Alex and Adam)
                        </div>
                    </article>
                    <article class="group">
                        <i class="icon-comment"></i>
                        <div class="details">
                            <h5>7/14/2012 - Mike McClain</h5>
                            Had Lunch with Jon today to talk about using new neighborhood map.
                        </div>
                    </article>
                    <article class="group">
                        <i class="icon-envelope"></i>
                        <div class="details">
                            <h5>7/7/2012 - Email from: Dustin Tappan</h5>
                            An email was sent to Jon from Dustin Tappan on 10/31 @9:35am.
                        </div>
                    </article>
                    <article class="group">
                        <i class="icon-phone"></i>
                        <div class="details">
                            <h5>7/1/2012 - Call To Scott Merlin</h5>
                            Jon called Scott Merlin's phone, 2999, on Monday October 12th at 10:51am and talked for 10mins.
                        </div>
                    </article>
                    <article class="group">
                        <i class="icon-comment"></i>
                        <div class="details">
                            <h5>7/1/2012 - Bob Johnson</h5>
                            Talked to Jon about joining the Security Team and gave him the forms needed to apply.
                        </div>
                    </article>
                </div>
                <div class="note-container-bottom"></div>
            </div>
        </div>
    </section>
        
    <script>
        $(document).ready(function ()     
            $('#person-notes').tinyscrollbar(     size: 150 });
        });
    </script>
        
</div>
