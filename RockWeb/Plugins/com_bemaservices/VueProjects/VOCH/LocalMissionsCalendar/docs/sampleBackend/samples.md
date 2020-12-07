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

  ```