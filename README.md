## Corvallis Bus Server

The backend that powers the best apps for the Corvallis Transit System.

## Prerequisites for running

- Visual Studio 2015

## Purpose

To have a more convenient way to get real-time information about the free buses in Corvallis.  Data from CTS is merged with data from Google Transit, with some convenient projections applied, and mapped into some easily-digestable JSON for different use cases.

## Disclaimer

We assume no liability for any missed buses.  Buses may be erratic in their arrival behavior, and we cannot control that.

## API Routes

###/static

Input: None

Output:

   Returns a JSON dictionary containing static route and stop information.  Recommended usage is a one-time download and then local storage on the device.  This will allow for simpler and far less data-intensive calls later.

Url: https://corvallisb.us/api/static

```
{
   "routes":
   {
      "1":
      {
        "routeNo": "1",
        "path": [
          14244,
          13265,
          ...
        ],
        "color": "00ADEE",
        "url": "http://www.corvallisoregon.gov/index.aspx?page=822",
        "polyline": "[a long string encoded with the Google polyline format]"
      }
      "2":
      {
        ...
      }
    }
    "stops":
    {
      "10019": 
      {
        "id": 10019,
        "name": "Benton Oaks RV Park",
        "lat": 44.5701824,
        "lng": -123.3122203,
        "routeNames": ["C3"]},
      },
      ...
    }
```

###/eta/

Queries the city API for arrival estimates, which are encoded as integers.

Input:
   - **Required** one or more Stop IDs

Output:

Returns a JSON dictionary, where they keys are the supplied Stop IDs, and the values are dictionaries.  These nested dictionaries are such that the keys are route numbers, and the values are lists of integers corresponding to the ETAs for that route to that stop. For example, ``"6":[1, 21]"`` means that Route 6 is arriving at the given stop in 1 minute, and again in 21 minutes. ETAs are limited to 30 minutes in the future by the city.

Sample Url: https://corvallisb.us/api/eta/14244,13265

```
{
  "14244": {
    
  },
  "13265": {
    "1": [6],
    "2": [21],
    "5": [22],
    "8": [22]
  }
}
```

###/schedule/

Returns an interleaved list of arrival times for each route for each stop ID provided.
Most of the stops in the Corvallis Transit System don't have a schedule. This app fabricates schedules for them by interpolating between those stops that have a schedule. The time between two officially scheduled stops is divided by the number of unscheduled stops between them. This turns out to be a reasonably accurate method.

Since buses can run behind by 15 minutes or more, or have runs cancelled outright, some interpretation is necessary to communicate the schedule and the estimates in the most informative way possible for users.

For instance, in the case of a bus running late, scheduled arrival times can be shown only at least 20 minutes in advance. If they instead were shown only at least 30 minutes in advance, there would be gaps in time where a bus's likely arrival wouldn't be apparent to the user. In other words, the API allows the schedule a 10-minute grace period to "pass" as an estimate, but when the city starts putting out an estimate for that same bus's arrival, the scheduled time gets replaced by the estimated time.

Input: 
   - **Required** one or more stop IDs

Output:

   A JSON dictionary where the keys are Stop IDs and the values are dictionaries of ``{ Route No : schedule }``.
   The schedule is a list of integers where each integer is "minutes from now." Integers are used because ETAs are interleaved with scheduled arrival times. This avoids a problem where an ETA appears to go up by a minute at the same time the minute on the system clock increments. It introduces a problem where the scheduled times vary by a minute if the server has a different minute value at the time it creates the payload than the client has at the time it consumes the payload. For the time being, it's recommended to use the endpoints which interpret these times and produce user-friendly descriptions for you.

Sample Url: https://corvallisb.us/api/schedule/14244,13265

```
{
  "14244": {
    "1": [
      
    ],
    "2": [
      40
    ],
    "4": [
      
    ]
  },
  "13265": {
    "1": [
      
    ],
    "2": [
      37
    ],
    "3": [
      
    ],
    "5": [
      35,
      65,
      95,
      125
    ],
    "7": [
      12
    ],
    "8": [
      
    ]
  }
}
```
###/favorites

Input:
One or more of the following is required.
   - one or more stop IDs, comma-separated
   - latitude and longitude, comma-separated

Output:

   A JSON array of stop information for "favorite stops" features. This allows a developer to easily create a widget to show the user's favorite stops. It shows arrivals summary information for the nearest 2 routes that will arrive at each favorite stop. If the user consents to provide a location, it will determine the stop's distance from the user and sort ascending by this quantity. The widget merely needs to wake up, download less than 1 KB of data, and display it.
   
Sample Url: https://corvallisb.us/api/favorites?stops=11776,10308&location=44.5645659,-123.2620435
   
```
[
  {
    "stopName": "Downtown Transit Center",
    "stopID": 14237,
    "distanceFromUser": "0.1 miles",
    "isNearestStop": true,
    "firstRouteColor": "034DA1",
    "firstRouteName": "6",
    "firstRouteArrivals": "26 minutes, 3:14 PM",
    "secondRouteColor": "",
    "secondRouteName": "",
    "secondRouteArrivals": ""
  },
  {
    "stopName": "NW Monroe Ave & NW 7th St",
    "stopID": 10308,
    "distanceFromUser": "0.2 miles",
    "isNearestStop": false,
    "firstRouteColor": "F26521",
    "firstRouteName": "3",
    "firstRouteArrivals": "2 minutes, 3:08 PM",
    "secondRouteColor": "D7181F",
    "secondRouteName": "7",
    "secondRouteArrivals": "2 minutes, 3:23 PM"
  },
  {
    "stopName": "SW Western Blvd & SW Hillside Dr",
    "stopID": 11776,
    "distanceFromUser": "1.7 miles",
    "isNearestStop": false,
    "firstRouteColor": "F26521",
    "firstRouteName": "3",
    "firstRouteArrivals": "2:57 PM, 3:57 PM",
    "secondRouteColor": "EC0C6D",
    "secondRouteName": "C3",
    "secondRouteArrivals": "3:19 PM, 5:34 PM"
  }
]
```

###/arrivals-summary/

Input:
   - **Required** one or more stop IDs

Output:
   A dictionary where the keys are stop IDs and the values are a list of nice, user-friendly descriptions of the arrival times for each route at that stop, sorted by descending arrival time. The server tries to determine if the route arrives at the stop "pretty much" hourly or half-hourly. Most routes arrive hourly, with a 10-minute break in the middle of the day. Thus if all the scheduled times left in the day are between 50-70 minutes from each other, it's considered to be an hourly schedule. Similarly with all being 20-40 minutes apart to be considered half-hourly.
   
Sample URL: https://corvallisb.us/api/arrivals-summary/10308,14237

```
{  
  "10308":[  
    {  
      "routeName":"2",
      "arrivalsSummary":"1 minute, 07:48 PM",
      "scheduleSummary":""
    },
    {  
      "routeName":"1",
      "arrivalsSummary":"10 minutes",
      "scheduleSummary":""
    },
    {  
      "routeName":"5",
      "arrivalsSummary":"11 minutes, 07:48 PM",
      "scheduleSummary":"Last arrival at 09:18 PM"
    },
    ...,
    {  
      "routeName":"8",
      "arrivalsSummary":"No arrivals!",
      "scheduleSummary":""
    },
    {  
      "routeName":"C1",
      "arrivalsSummary":"No arrivals!",
      "scheduleSummary":""
    },
    {  
      "routeName":"C3",
      "arrivalsSummary":"No arrivals!",
      "scheduleSummary":""
    },
    {  
      "routeName":"CVA",
      "arrivalsSummary":"No arrivals!",
      "scheduleSummary":""
    }
  ],
  "14237":[  
    {  
      "routeName":"6",
      "arrivalsSummary":"17 minutes, 07:54 PM",
      "scheduleSummary":"Last arrival at 08:24 PM"
    }
  ]
}
```
