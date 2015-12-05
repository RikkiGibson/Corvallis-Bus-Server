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
   "Routes":
   {
      "1":
      {
        "RouteNo": "1",
        "Path": [
          14244,
          13265,
          ...
        ],
        "Color": "00ADEE",
        "Url": "http://www.corvallisoregon.gov/index.aspx?page=822",
        "Polyline": "[a long string encoded with the Google polyline format]"
      }
      "2":
      {
        ...
      }
    }
    "Stops":
    {
      "10019": 
      {
        "ID": 10019,
        "Name": "Benton Oaks RV Park",
        "Lat": 44.5701824,
        "Long": -123.3122203,
        "RouteNames": ["C3"]},
      },
      ...
    }
```

###/eta/

Queries the city API for arrival estimates, which are encoded as integers.

Input:
   - **Required** one or more Stop IDs

Output:

Returns a JSON dictionary, where they keys are the supplied Stop IDs, and the values are dictionaries.  These nested dictionaries are such that the keys are route numbers, and the values are lists of integers corresponding to the ETAs for that route to that stop. For example, ``"6":[1, 21]"`` means that Route 6 is arriving at the given stop in 1 minute, and again in 21 minutes. ETAs are limited to 30 minutes in the future.

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

Since buses can run behind by 15 minutes or more, or have runs cancelled outright, some interpretation is necessary to communicate the schedule and the estimates in the most informative way possible for users. Check out the source for TransitClient if you want to know the details on what is done for this.

Input: 
   - **Required** one or more stop IDs

Output:

   A JSON dictionary where the keys are Stop IDs and the values are dictionaries of ``{ Route No : schedule }``.
   The schedule is a list of integers where each integer is "minutes from now." Integers are used because ETAs are interleaved with scheduled arrival times. This avoids a problem where an ETA appears to go up by a minute at the same time the minute on the system clock increments. It introduces a problem where the scheduled times vary by a minute if the server has a different minute value at the time it creates the payload than the client has at the time it consumes the payload. This is a case of settling for "good enough."

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

   A JSON array of stop information for "favorite stops" features.  See sample for details.
   
Sample Url: https://corvallisb.us/api/favorites?stops=11776,10308&location=44.5645659,-123.2620435
   
```
[
  {
    "StopName": "Downtown Transit Center",
    "StopId": 14237,
    "DistanceFromUser": "0.1 miles",
    "IsNearestStop": true,
    "FirstRouteColor": "034DA1",
    "FirstRouteName": "6",
    "FirstRouteArrivals": "26 minutes, 3:14 PM",
    "SecondRouteColor": "",
    "SecondRouteName": "",
    "SecondRouteArrivals": ""
  },
  {
    "StopName": "NW Monroe Ave & NW 7th St",
    "StopId": 10308,
    "DistanceFromUser": "0.2 miles",
    "IsNearestStop": false,
    "FirstRouteColor": "F26521",
    "FirstRouteName": "3",
    "FirstRouteArrivals": "2 minutes, 3:08 PM",
    "SecondRouteColor": "D7181F",
    "SecondRouteName": "7",
    "SecondRouteArrivals": "2 minutes, 3:23 PM"
  },
  {
    "StopName": "SW Western Blvd & SW Hillside Dr",
    "StopId": 11776,
    "DistanceFromUser": "1.7 miles",
    "IsNearestStop": false,
    "FirstRouteColor": "F26521",
    "FirstRouteName": "3",
    "FirstRouteArrivals": "2:57 PM, 3:57 PM",
    "SecondRouteColor": "EC0C6D",
    "SecondRouteName": "C3",
    "SecondRouteArrivals": "3:19 PM, 5:34 PM"
  }
]
```

###/arrivalsSummary/

Input:
   - **Required** one or more stop IDs

Output:
   A dictionary where the keys are stop IDs and the values are a list of nice, sorted view models with user-friendly descriptions of the arrival times for each route at that stop.
   
Sample URL: https://corvallisb.us/api/arrivals-summary/10308,14237

```
{  
  "10308":[  
    {  
      "RouteName":"2",
      "RouteColor":"882790",
      "ArrivalsSummary":"1 minute, 07:48 PM",
      "ScheduleSummary":""
    },
    {  
      "RouteName":"1",
      "RouteColor":"00ADEE",
      "ArrivalsSummary":"10 minutes",
      "ScheduleSummary":""
    },
    {  
      "RouteName":"5",
      "RouteColor":"BD559F",
      "ArrivalsSummary":"11 minutes, 07:48 PM",
      "ScheduleSummary":"Last arrival at 09:18 PM"
    },
    ...,
    {  
      "RouteName":"8",
      "RouteColor":"008540",
      "ArrivalsSummary":"No arrivals!",
      "ScheduleSummary":""
    },
    {  
      "RouteName":"C1",
      "RouteColor":"614630",
      "ArrivalsSummary":"No arrivals!",
      "ScheduleSummary":""
    },
    {  
      "RouteName":"C3",
      "RouteColor":"EC0C6D",
      "ArrivalsSummary":"No arrivals!",
      "ScheduleSummary":""
    },
    {  
      "RouteName":"CVA",
      "RouteColor":"3F2885",
      "ArrivalsSummary":"No arrivals!",
      "ScheduleSummary":""
    }
  ],
  "14237":[  
    {  
      "RouteName":"6",
      "RouteColor":"034DA1",
      "ArrivalsSummary":"17 minutes, 07:54 PM",
      "ScheduleSummary":"Last arrival at 08:24 PM"
    }
  ]
}
```
