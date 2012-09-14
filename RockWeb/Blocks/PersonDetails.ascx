<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonDetails.ascx.cs" Inherits="RockWeb.Blocks.PersonDetails" %>

<script src="/RockWeb/Scripts/jquery.tagsinput.js"></script>
<link href="/RockWeb/CSS/jquery.tagsinput.css" rel="stylesheet">

<link href="/RockWeb/CSS/PersonDetailsCore.css" rel="stylesheet">
<script src="/RockWeb/Scripts/tinyscrollbar.min.js"></script>



<div id="person-profile" class="row-fluid">
    <div class="span3">
        <div class="bio-wrap group">
            <aside class="bio">             
                <div class="content">
                    <img src="/RockWeb/Assets/Mockup/jon.jpg" alt="Jon Edmiston" />

                    <section class="group">
                        <span class="member-status">Member</span>
                        <span class="record-status inactive">Inactive</span>

                        <span class="campus">Peoria Campus</span>
                        <span class="area">West Wing</span>
                    </section>

                    <section class="group">
                        <span class="age">39 yrs old <em>(2/10)</em></span>
                        <span class="gender">Male</span>
                        <span class="marital-status">Married 17yrs <em>(12/23)</em></span>
                    </section>
                </div>
                <footer>
                    <div class="left"></div>
                    <div class="center"></div>
                    <div class="right"></div>
                </footer>
            </aside>

            <aside class="bio-details">
                <section class="family group">
                    <header>Edmiston Family <a href="#" class="edit"><i class="icon-edit"></i></a></header>

                    <ul class="group">
                        <li>
                            <a href="">
                            <img src="/RockWeb/Assets/Mockup/heidi.jpg" />
                            <h4>Heidi</h4>
                            <small>Wife</small>
                            </a>
                        </li>
                        <li>
                            <a href="">
                            <img src="/RockWeb/Assets/Mockup/alex.jpg" />
                            <h4>Alex</h4>
                            <small>Son</small>
                            </a>
                        </li>
                        <li>
                            <a href="">
                            <img src="/RockWeb/Assets/Mockup/adam.jpg" />
                            <h4>Adam</h4>
                            <small>Son</small>
                            </a>
                        </li>
                   
                        <li>
                            <a href="">
                            <img src="/RockWeb/Assets/Mockup/rachael-sue.jpg" />
                            <h4>Rachael-Sue</h4>
                            <small>Pet</small>
                            </a>
                        </li>
                        <li>
                            <a href="">
                            <img src="/RockWeb/Assets/Mockup/monica.jpg" />
                            <h4>Monica</h4>
                            <small>Pet</small>
                            </a>
                        </li>
                    
                    </ul>

                </section>
                <section class="contact-info group">
                    <header>Contact Information <a href="#" class="edit"><i class="icon-edit"></i></a></header>
                
                    <ul class="phone-numbers">
                        <li><a href="#" class="highlight"><i class="icon-phone"></i><span class="phone-unlisted" data-value="623.780.0135">Unlisted</span> <small>Home</small></a></li>
                        <li><a href="#" class="highlight"><i class="icon-phone"></i>623.298.2911 <small>Internal</small></a></li>
                        <li><a href="#" class="highlight"><i class="icon-phone"></i>623.866.2792 <small>Cell</small></a></li>
                        <li><a href="#" class="highlight"><i class="icon-phone"></i>623.376.2444 <small>Work</small></a></li>
                    </ul>

                    <script>
                        $('ul.phone-numbers li a').live({
                            mouseenter:
                               function () {
                                   var spanItem = $('span.phone-unlisted', this);
                                   if (spanItem.length > 0) {
                                       $(spanItem).text($(spanItem).attr('data-value'));
                                   }

                                   var phoneIcon = $('i', this);
                                   if (phoneIcon.length > 0) {
                                       $(phoneIcon).fadeToggle();
                                   }
                               },
                            mouseleave:
                               function () {
                                   var spanItem = $('span.phone-unlisted', this);
                                   if (spanItem.length > 0) {
                                       $(spanItem).text('Unlisted');
                                   }

                                   var phoneIcon = $('i', this);
                                   if (phoneIcon.length > 0) {
                                       $(phoneIcon).hide();
                                   }
                               }
                            });                
                    </script>

                    <ul class="emails">
                        <li>jonathan.edmiston@gmail.com</li>
                    </ul>

                    <ul class="addresses">
                        <li class="group">
                            <h4>Home Address</h4>
                            <a href="" class="map"><i class="icon-map-marker"></i></a>
                            <div class="address">
                                <span>9039 W Molly Ln</span>
                                <span>Peoria, AZ 85383</span>
                            </div>
                            <div class="actions">
                                <a href="" title="GPS: 33.7281 -112.2546"><i class="icon-globe"></i></a>
                                <a href="" title="Address Standardized"><i class="icon-magic"></i></a>
                            </div>
                        </li>
                    </ul>
                    <script>
                        $('ul.addresses li').live({
                            mouseenter:
                               function () {
                                   var actionsDiv = $('div.actions', this);
                                   if (actionsDiv.length > 0) {
                                       $(actionsDiv).fadeToggle();
                                   }
                               },
                            mouseleave:
                               function () {
                                   var actionsDiv = $('div.actions', this);
                                   if (actionsDiv.length > 0) {
                                       $(actionsDiv).fadeToggle();
                                   }
                            }
                        });
                    </script>
                </section>

                <section class="dates">
                    <header>Dates <a href="#" class="edit"><i class="icon-edit"></i></a></header>

                    <ul>
                        <li>1/1/2012 <small>First Visit</small></li>
                        <li>6/17/2012 <small>Date Joined</small></li>
                        <li>6/17/2012 <small>Baptism Date</small></li>
                    </ul>
                </section>

                <section class="documents">
                    <header>Documents <a href="#" class="edit"><i class="icon-edit"></i></a></header>

                    <ul>
                        <li><i class="icon-file"></i> Birth Certificate</li>
                        <li><i class="icon-group"></i> Membership Covenant</li>
                    </ul>
                </section>

            </aside>
        </div>
    </div>

    <div class="span9 tags-notes-attributes">
        <div class="row-fluid tag-row">
            <div class="span12 tag-span">
                <h2>Tags</h2>            
                <div class="tag-wrap">
                    <input name="person-tags" id="person-tags" value="foo^personal,bar,baz" />
                </div>
            </div>
        </div>

        <div class="row-fluid note-attribute-column">

            <div class="span8 person-notes-container">
                <section id="person-notes" class="person-notes scroll-container">
                    <header class="group">
                        <h4>Timeline</h4>
                        <a id="note-add" class="note-add btn"><i class="icon-plus"></i></a>

                        <script>

                            $(document).ready(function () {
                                
                                $('#note-add').click(function () {
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
                    $(document).ready(function () {
                        $('#person-notes').tinyscrollbar({ size: 150 });
                        $('ul.ui-autocomplete').css('width', '300px');
                        $('#person-tags').tagsInput({
                            autocomplete_url: '/rockweb/autocomplete-sample.html',
                            autoCompleteAppendTo: 'div.tag-wrap',
                            'height':'auto',
                            'width': '100%',
                            'interactive': true,
                            'defaultText': 'add tag',
                            'removeWithBackspace': false,
                            'onAddTag': AddTag,
                            'onRemoveTag': RemoveTag,
                            'enableDelete': true
                        });
                    });

                    function AddTag(tagName) {
                        // save tag to server (if tag does not exist don't save and trigger event below)

                        // simulate a tag that does not already exist, this check should be done on the server
                        if (tagName.toLowerCase() == 'does not exist') {
                            var r = confirm("A tag called '" + tagName + "' does not exist. Do you want to create a new personal tag?");
                            if (r == true) {
                                // call server control to add tag
                                // save person to tag
                            }
                            else {
                                // remove tag
                                $('#person-tags').removeTag(tagName);
                            } 
                        }
                    }

                    function RemoveTag(tagName) {
                        // call server to remove tag
                    }
                </script>
        
            </div>


            <div class="span4">
                <aside class="supplemental-info">
            
                    <section class="personal-key-attributes">
                        <header>Personal Key Attributes <a href="#" class="edit"><i class="icon-edit"></i></a></header>

                        <ul>
                            <li>Baptism Date: <span class="value">12/23/2012 (8yrs)</span></li>
                            <li>How Joined: <span class="value">Baptized</span>
                            <li>Last Gave: <span class="value">8/1/2012</span>
                            <li>T-Shirt Size: <span class="value"l>L</span>
                            <li>Favorite Movie: <span class="value">Star Wars</span></li>
                        </ul>
                    </section>

                    <section class="known-relationships">
                        <header>Known Relationships <a href="#" class="edit"><i class="icon-chevron-right"></i></a></header>

                        <ul>
                            <li><a href="#" class="highlight"><i class="icon-user photo"></i> Bret Norman <small>Invited</small></a></li>
                            <li><a href="#" class="highlight"><i class="icon-user photo"></i> Diane Richardson <small>Invited</small></a></li>
                            <li><a href="#" class="highlight"><i class="icon-blank photo"></i> Trica Peters <small>Invited</small></a></li>
                            <li><a href="#" class="highlight"><i class="icon-user photo"></i> Jen Davis <small>Invited By</small></a></li>
                            <li><a href="#" class="highlight"><i class="icon-user photo"></i> Tom Hope <small>Can Check In</small></a></li>
                            <li><a href="#" class="highlight"><i class="icon-blank photo"></i> Holly Hope <small>Sister</small></a></li>
                        </ul>
                    </section>

                    <section class="implied-relationships">
                        <header>Implied Relationships <a href="#" class="edit"><i class="icon-chevron-right"></i></a></header>

                        <ul>
                            <li><a href="#" class="highlight"><i class="icon-user photo"></i> David Roberts</a></li>
                            <li><a href="#" class="highlight"><i class="icon-user photo"></i> Erik Pitts</a></li>
                            <li><a href="#" class="highlight"><i class="icon-blank photo"></i> Jeff Rhodes</a></li>
                            <li><a href="#" class="highlight"><i class="icon-user photo"></i> Cameron Jones</a></li>
                            <li><a href="#" class="highlight"><i class="icon-user photo"></i> Kyle Ownes</a></li>
                            <li><a href="#" class="highlight"><i class="icon-blank photo"></i> Jeff Wyatt</a></li>
                            <li><a href="#" class="highlight"><i class="icon-blank photo"></i> Becky Aders</a></li>
                            <li><a href="#" class="highlight"><i class="icon-blank photo"></i> Robin Smith</a></li>
                            <li><a href="#" class="highlight"><i class="icon-blank photo"></i> Shami Brown</a></li>
                        </ul>
                    </section>

                </aside>
            </div>

        </div>

    </div>
</div>

    <-- Button to trigger modal -->
    <a href="#myModal" role="button" class="btn" data-toggle="modal">Launch demo modal</a>
     
    <-- Modal -->
    <div class="modal" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
    <div class="modal-header">
    <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
    <h3 id="myModalLabel">Modal header</h3>
    </div>
    <div class="modal-body">
    <p>One fine body…</p>
    </div>
    <div class="modal-footer">
    <button class="btn" data-dismiss="modal" aria-hidden="true">Close</button>
    <button class="btn btn-primary">Save changes</button>
    </div>
    </div>