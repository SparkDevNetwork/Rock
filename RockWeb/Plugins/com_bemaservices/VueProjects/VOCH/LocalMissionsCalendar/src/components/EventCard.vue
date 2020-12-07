<template>
    <div

      :class="ShowDetails ? 'col-md-6' : 'col-md-3 col-lg-4'"
      v-if="!calculateSpotsRemaining.LimitedSpots || calculateSpotsRemaining.SpotsRemaining > 0 "
    >
    <v-card
    class="mx-auto"
    max-width="100%"
  >
    <v-img
      class="white--text align-end"
      :height="ShowDetails ? '400px' : '200px'"
      width="100%"
      :src="Event.EventPhoto.Url"
    >
      <v-card-title>{{Event.EventName}}</v-card-title>
    </v-img>

    <v-card-subtitle class="pb-0">
      <div style="display:flex; flex-direction:row; flex-wrap:nowrap; justify-content:space-between; align-items:top;">
        <div>
          {{ formatDate(Event.EventNextStartDate.StartDateTime) }} at {{ formatTime(Event.EventNextStartDate.StartDateTime) }}
          <div>{{ Event.OccurrenceLocation }}</div>
          <div v-if="calculateSpotsRemaining.LimitedSpots && calculateSpotsRemaining.SpotsRemaining > 0 ">Spots Remaining: {{ calculateSpotsRemaining.SpotsRemaining }}</div><br/>
        </div>
        <div>
            {{Event.CampusName}}
        </div>
      </div>
    </v-card-subtitle>

    <v-card-text class="text--primary">
      <div v-if="!ShowDetails">
        <div class="font-weight-bold">Summary:</div>
        <div>{{ Event.EventSummary}}</div>
      </div>
      <div v-else >
         <div class="font-weight-bold">Description:</div>
        <div v-html="Event.EventDescription"></div>
        <div class="font-weight-bold">Upcoming Dates:</div>
        <ul>
          <li v-for="date in Event.UpcomingDates" :key="date">{{formatDate(date.StartDateTime) }} at {{ formatTime(date.StartDateTime) }} to {{formatDate(date.EndDateTime) != formatDate(date.StartDateTime) ? `${formatDate(date.EndDateTime)} at` : ''}} {{ formatTime(date.EndDateTime) }}</li>
        </ul>
        <div v-if="Event.RegistrationInformation.length > 0">
            <div class="font-weight-bold">Registration Information:</div>
            <ul>
              <li v-for="registration in Event.RegistrationInformation" :key="registration.RegistrationInstanceId">
                <span>{{registration.RegistrationPublicName}} - Register Between {{formatDate(registration.RegistrationStartDate) }} and {{formatDate(registration.RegistrationEndDate) }}</span><br />
                <span vif="!registrationSpotsRemaining(registration).LimitedSpots || registrationSpotsRemaining(registration).SpotsRemaining > 0">
                  <a :href="registration.RegistrationPublicSlug != '' ? `https://voxchurch.org/registration/${registration.RegistrationPublicSlug}` : `https://voxchurch.org/registration?RegistrationInstance=${registration.RegistrationInstanceId}`" class="btn btn-primary">Register</a>
                </span>
              </li>
            </ul>
         </div>
      </div>
    </v-card-text>
    <v-card-actions>
      <v-btn
        color="orange"
        text
        @click="ShowDetails = !ShowDetails"
      >
        <span v-if="!ShowDetails">View Details</span>
        <span v-else>Hide Details</span>
      </v-btn>

      <v-btn
        color="orange"
        text
        v-if="Event.RegistrationInformation.length > 0 && calculateSpotsRemaining.LimitedSpots && calculateSpotsRemaining.SpotsRemaining > 0"
      >
        Register
      </v-btn>
    </v-card-actions>
    
  </v-card>

  </div>
</template>

<script>

export default {
  data: () => ({
    ShowDetails: false,
    Event: {
      OccurrenceId: 54,
      OccurrenceLocation: '57 Olive Street, New Haven',
      OccurrenceNote: '',
      ContactPersonAliasId: 1304,
      ContactFirstName: 'Lauren',
      ContactLastName: 'Roy',
      ContactEmail: 'lcorso@ourcitychurch.org',
      ContactPhone: '',
      EventItemId: 23,
      EventName: 'Loaves & Fishes',
      EventDescription: '<p>Sorting clothing and handing out groceries to those in need (over 300 people each weekend)</p>',
      EventSummary: 'Sorting clothing and handing out groceries to those in need (over 300 people each weekend)',
      EventDetailsUrl: '/community-groups/group-details?GroupId=25477',
      EventPhoto: {
        BinaryFileType: {
          StorageEntityType: {
            Name: 'Rock.Storage.Provider.Database',
            AssemblyName: 'Rock.Storage.Provider.Database, Rock, Version=1.10.3.7, Culture=neutral, PublicKeyToken=null',
            FriendlyName: 'Database',
            IsEntity: false,
            IsSecured: true,
            IsCommon: false,
            SingleValueFieldTypeId: null,
            MultiValueFieldTypeId: null,
            IsIndexingEnabled: false,
            AttributesSupportPrePostHtml: false,
            AttributesSupportShowOnBulk: false,
            Id: 51,
            Guid: '0aa42802-04fd-4aec-b011-feb127fc85cd',
            ForeignId: null,
            ForeignGuid: null,
            ForeignKey: null,
          },
          IsSystem: true,
          Name: 'Unsecured',
          Description: 'The default file type',
          IconCssClass: null,
          StorageEntityTypeId: 51,
          AllowCaching: false,
          RequiresViewSecurity: false,
          MaxWidth: null,
          MaxHeight: null,
          PreferredFormat: -1,
          PreferredResolution: -1,
          PreferredColorDepth: -1,
          PreferredRequired: false,
          CreatedDateTime: null,
          ModifiedDateTime: null,
          CreatedByPersonAliasId: null,
          ModifiedByPersonAliasId: null,
          ModifiedAuditValuesAlreadyUpdated: false,
          Attributes: null,
          AttributeValues: null,
          Id: 3,
          Guid: 'c1142570-8cd6-4a20-83b1-acb47c1cd377',
          ForeignId: null,
          ForeignGuid: null,
          ForeignKey: null,
        },
        Document: null,
        Url: 'https://rock.voxchurch.org:443/GetImage.ashx?guid=f113a617-1790-449b-81fb-cd090dd39747',
        IsTemporary: true,
        IsSystem: false,
        BinaryFileTypeId: 3,
        FileName: 'Loaves&fishes_button.jpg',
        FileSize: 343086,
        MimeType: 'image/jpeg',
        Description: null,
        StorageEntityTypeId: 51,
        Path: '~/GetImage.ashx?guid=f113a617-1790-449b-81fb-cd090dd39747',
        Width: null,
        Height: null,
        ContentLastModified: '2017-12-07T11:03:40.433',
        CreatedDateTime: '2017-12-07T11:03:40.433',
        ModifiedDateTime: '2017-12-07T11:03:40.433',
        CreatedByPersonAliasId: null,
        ModifiedByPersonAliasId: null,
        ModifiedAuditValuesAlreadyUpdated: false,
        Attributes: null,
        AttributeValues: null,
        Id: 430,
        Guid: 'f113a617-1790-449b-81fb-cd090dd39747',
        ForeignId: null,
        ForeignGuid: null,
        ForeignKey: null,
      },
      Categories: [],
      CampusId: 2,
      CampusName: 'New Haven',
      EventNextStartDate: {
        StartDateTime: '2020-12-19T07:45:00',
        EndDateTime: '2020-12-19T11:45:00',
      },
      UpcomingDates: [
        {
          StartDateTime: '2020-12-19T07:45:00',
          EndDateTime: '2020-12-19T11:45:00',
        },
      ],
      SocialMediaLinks: [],
      RegistrationInformation: [
        {
          RegistrationInstanceId: 272,
          RegistrationStartDate: '2020-08-28T09:00:00',
          RegistrationEndDate: '2020-12-28T12:00:00',
          RegistrationPublicName: 'Virtual VoxKids',
          RegistrationPublicSlug: '',
          RegistrationMaxAttendees: null,
          RegistrationTotalRegistrants: 87,
        },
      ],
    },

  }),
  methods: {
    formatDate(dateItem) {
      const options = {
        year: 'numeric', month: 'long', day: 'numeric',
      };

      const newDate = new Date(dateItem);
      return newDate.toLocaleDateString('en-US', options);
    },
    formatTime(dateItem) {
      const options = {
        hour: 'numeric',
        minute: '2-digit',
        hour12: true,
      };
      const newDate = new Date(dateItem);
      return newDate.toLocaleTimeString('en-US', options);
    },
    registrationSpotsRemaining(registration) {
      let SpotsRemaining = null;
      let LimitedSpots = false;

      SpotsRemaining = registration.RegistrationMaxAttendees - registration.RegistrationTotalRegistrants;
      LimitedSpots = registration.RegistrationMaxAttendees > 0 ? registration.RegistrationMaxAttendees : false;

      return {
        LimitedSpots,
        SpotsRemaining,
      };
    },
  },
  computed: {
    calculateSpotsRemaining() {
      let SpotsRemaining = null;
      let LimitedSpots = false;

      if (this.Event.RegistrationInformation[0]) {
        SpotsRemaining = this.Event.RegistrationInformation[0].RegistrationMaxAttendees - this.Event.RegistrationInformation[0].RegistrationTotalRegistrants;
        LimitedSpots = this.Event.RegistrationInformation[0].RegistrationMaxAttendees > 0 ? this.Event.RegistrationInformation[0].RegistrationMaxAttendees : false;
      }
      return {
        LimitedSpots,
        SpotsRemaining,
      };
    },
  },
};
</script>
<style >

</style>
