### These are sample backend that the vue app expects.
#### Current Person
  The standard response from the current person api is expected. The following fields are used:
  ```json 
  [{
    "NickName": "Bema Developer",
    "LastName": "Admin",
    "Email": "kevin.rutledge@bemaservices.com",
    "PrimaryCampusId": 1,
    "Id": 14968,
  }]
  ```
#### Campuses
  An abbreviated response from the campuses list is expected, but the full response can be used using Id and Name.
```json
[{
  "Id": 2,
  "Name": "New Haven"
}, {
  "Id": 3,
  "Name": "Greater Bridgeport"
}, {
  "Id": 4,
  "Name": "Hartford"
}, {
  "Id": 5,
  "Name": "Middletown"
}, {
  "Id": 6,
  "Name": "North"
}, {
  "Id": 7,
  "Name": "Springfield"
}, {
  "Id": 8,
  "Name": "Stamford"
}, {
  "Id": 9,
  "Name": "New Britain"
}, {
  "Id": 10,
  "Name": "Worcester"
}]
```
#### Mission Categories
  A Defined value list of Id and Description.
  ```json
[{
  "Id": 1932,
  "Value": "Family Friendly"
}, {
  "Id": 1933,
  "Value": "Construction"
}, {
  "Id": 1934,
  "Value": "Youth"
}, {
  "Id": 1935,
  "Value": "Organization/Packing"
}, {
  "Id": 1936,
  "Value": "Food Prep/Meal Services"
}, {
  "Id": 1937,
  "Value": "Food Delivery"
}, {
  "Id": 1938,
  "Value": "Special Events"
}]
 ```

#### Calendar Response
  ```json
[{
		"EventId": 23,
		"EventPhoto": "",
		"EventDescription": "<p>Sorting clothing and handing out groceries to those in need (over 300 people each weekend)</p>",
		"EventSummary": "Sorting clothing and handing out groceries to those in need (over 300 people each weekend)",
		"SocialMediaLinks": {
			"Facebook": "",
			"Twitter": ""
		},
		"Campus": {
			"Name": "New Haven",
			"Id": 2
		},
		"ContactPerson": {
			"PersonAliasId": "1304",
			"FirstName": "Lauren",
			"LastName": "Roy",
			"Email": "lcorso@ourcitychurch.org",
			"PhoneNumber": ""
		},
		"Location": "57 Olive Street, New Haven",
		"Upcomingdates": [{
				"StartDateTime": "11/21/2020 7:45:00 AM",
				"EndDateTime": "11/21/2020 11:45:00 AM"
			},
			{
				"StartDateTime": "12/19/2020 7:45:00 AM",
				"EndDateTime": "12/19/2020 11:45:00 AM"
			},
			{
				"StartDateTime": "1/16/2021 7:45:00 AM",
				"EndDateTime": "1/16/2021 11:45:00 AM"
			}
		],
		"OccurrenceNote": "",
		"RegistrationInformation": []
	},
	{
		"EventId": 43,
		"EventPhoto": "",
		"EventDescription": "<p>Serve dinner to 20 women living in one of New Reach's shelters.  Sign up today!<br></p>",
		"EventSummary": "Serve dinner to 20 women living in one of New Reach's shelters.",
		"SocialMediaLinks": {
			"Facebook": "",
			"Twitter": ""
		},
		"Campus": {
			"Name": "New Haven",
			"Id": 2
		},
		"ContactPerson": {
			"PersonAliasId": "1304",
			"FirstName": "Lauren",
			"LastName": "Roy",
			"Email": "lcorso@voxchurch.org",
			"PhoneNumber": ""
		},
		"Location": "New Reach Dinner",
		"Upcomingdates": [{
				"StartDateTime": "12/9/2020 5:00:00 PM",
				"EndDateTime": "12/9/2020 7:00:00 PM"
			},
			{
				"StartDateTime": "1/13/2021 5:00:00 PM",
				"EndDateTime": "1/13/2021 7:00:00 PM"
			},
			{
				"StartDateTime": "2/10/2021 5:00:00 PM",
				"EndDateTime": "2/10/2021 7:00:00 PM"
			}
		],
		"OccurrenceNote": "",
		"RegistrationInformation": []
	},
	{
		"EventId": 45,
		"EventPhoto": "",
		"EventDescription": "<p>Serve dinner to those in need in the downtown area.<br></p>",
		"EventSummary": "Serve dinner to those in need in the downtown area.",
		"SocialMediaLinks": {
			"Facebook": "",
			"Twitter": ""
		},
		"Campus": {
			"Name": "New Haven",
			"Id": 2
		},
		"ContactPerson": {
			"PersonAliasId": "1304",
			"FirstName": "Lauren",
			"LastName": "Roy",
			"Email": "lcorso@ourcitychurch.org",
			"PhoneNumber": ""
		},
		"Location": "311 Temple Street, New Haven",
		"Upcomingdates": [{
				"StartDateTime": "11/25/2020 5:00:00 PM",
				"EndDateTime": "11/25/2020 7:00:00 PM"
			},
			{
				"StartDateTime": "12/30/2020 5:00:00 PM",
				"EndDateTime": "12/30/2020 7:00:00 PM"
			},
			{
				"StartDateTime": "1/27/2021 5:00:00 PM",
				"EndDateTime": "1/27/2021 7:00:00 PM"
			}
		],
		"OccurrenceNote": "",
		"RegistrationInformation": []
	},
	{
		"EventId": 101,
		"EventPhoto": "",
		"EventDescription": "<p style=\"text-align: left;\">The Encounter is Vox Church's weekend retreat that happens several times each year.&nbsp;&nbsp;</p><p style=\"text-align: left;\">Check-in begins at 3:30pm on Friday, and dinner will be served before the evening session.&nbsp; Friday night's session will begin at 7pm.&nbsp; The retreat weekend will last until 11:30am on Sunday.</p><p>Cost: $225<br></p><p>Includes lodging and meals.</p><p><title></title>      Our Encounter retreat weekends are a time to unplug from the busyness of life, seek God and encounter Him in a new and fresh way. &nbsp;If you are in need of financial assistance for Encounter, please fill out <a href=\"/Content/Documents/Encounter Scholarship Request Form.docx\" target=\"_blank\">this application</a>&nbsp;and email it to <a href=\"mailto:kkish@voxchurch.org\">Karen Kish</a>. &nbsp;All requests will be confidentially reviewed and you will be notified via email.</p>",
		"EventSummary": "Bi-annual Vox Church weekend retreat.",
		"SocialMediaLinks": {
			"Facebook": "",
			"Twitter": ""
		},
		"Campus": {
			"Name": "",
			"Id": null
		},
		"ContactPerson": {
			"PersonAliasId": "2832",
			"FirstName": "Karen",
			"LastName": "Kish",
			"Email": "kkish@voxchurch.org",
			"PhoneNumber": ""
		},
		"Location": "Mercy by the Sea: 167 Neck Rd, Madison, CT",
		"Upcomingdates": [{
			"StartDateTime": "11/20/2020 12:00:00 AM",
			"EndDateTime": "11/20/2020 12:00:00 AM"
		}],
		"OccurrenceNote": "",
		"RegistrationInformation": [{
				"RegistrationInstanceId": 163,
				"RegistrationStartDate": "11/14/2019 9:00:00 AM",
				"RegistrationEndDate": "11/20/2020 9:00:00 PM",
				"RegistrationPublicName": "Nov 2020 Shoreline Encounter",
				"RegistrationPublicSlug": "",
				"RegistrationMaxAttendees": "45",
				"FegistrationTotalRegistrants": "1"
			}

		]
	}
]
  ```