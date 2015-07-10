var CCV = CCV || {}

CCV.findCampusById = function (campusId) {
  return CCV.locations.filter(function (campus) { return campus.id == campusId })[0]
}

CCV.locations = [




  {
    id: '8',
    name: 'Anthem',

    geo: {
      lat: 33.84683,
      lng: -112.13706,
    },

    phone: '',
    street: '39905 N Gavilan Peak Pkwy',
    city: 'Anthem',
    state: 'AZ',
    zip: '85086-2521',
    photo: 'http://rock.ccv.church:80/GetImage.ashx?guid=f093e8c5-20c1-4678-9465-c81489b0a8c4',
    serviceTimes: {


    },
  },




  {
    id: '9',
    name: 'Avondale',

    geo: {
      lat: 33.4636054,
      lng: -112.301477,
    },

    phone: '',
    street: '1565 N 113th Ave',
    city: 'Avondale',
    state: 'AZ',
    zip: '85392-3935',
    photo: 'http://rock.ccv.church:80/GetImage.ashx?guid=47fbd2ae-42b5-4569-967e-090fe5013014',
    serviceTimes: {


    },
  },




  {
    id: '7',
    name: 'East Valley',

    geo: {
      lat: 33.3912,
      lng: -111.61549,
    },

    phone: '',
    street: '1330 S Crismon Rd',
    city: 'Mesa',
    state: 'AZ',
    zip: '85209-3767',
    photo: 'http://rock.ccv.church:80/GetImage.ashx?guid=abfd309d-cc0f-4803-9ca7-80e9aaa47a7d',
    serviceTimes: {


    },
  },




  {
    id: '1',
    name: 'Peoria',

    geo: {
      lat: 33.7110943,
      lng: -112.2088517,
    },

    phone: '',
    street: '7007 W Happy Valley Rd',
    city: 'Peoria',
    state: 'AZ',
    zip: '85383-3223',
    photo: 'http://rock.ccv.church:80/GetImage.ashx?guid=358262c2-c286-4535-9f04-aed54d96fd49',
    serviceTimes: {

      saturday: ['4:30 pm','6:00 pm',],sunday: ['9:00 am','10:30 am','12:00 pm',],
    },
  },




  {
    id: '6',
    name: 'Scottsdale',

    geo: {
      lat: 33.65789,
      lng: -111.88851,
    },

    phone: '',
    street: '19030 N Pima Rd',
    city: 'Scottsdale',
    state: 'AZ',
    zip: '85255-5392',
    photo: 'http://rock.ccv.church:80/GetImage.ashx?guid=99fe4a6f-e102-4b29-982d-a93ce1c7b348',
    serviceTimes: {


    },
  },




  {
    id: '5',
    name: 'Surprise',

    geo: {
      lat: 33.5875394,
      lng: -112.3785948,
    },

    phone: '',
    street: '14787 W Cholla St',
    city: 'Surprise',
    state: 'AZ',
    zip: '85379-4418',
    photo: 'http://rock.ccv.church:80/GetImage.ashx?guid=76f4ce45-0446-4103-b4ed-cf84d8c509c4',
    serviceTimes: {


    },
  }

]
