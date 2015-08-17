## Corvallis Bus Server

The backend that powers the best apps for the Corvallis Transit System.

## Prerequisites for running

- Visual Studio 2015

## Purpose

To have a more convenient way to get real-time information about the free buses in Corvallis.  Data from CTS is mapped into some more easily-digestable JSON for different use cases.

## Disclaimer

We assume no liability for any missed buses.  Buses may be erratic in their arrival behavior, and we cannot control that.

## API Routes

###/transit/static

Input: None

Output:

   Returns a JSON dictionary containing static route and stop information.  Recommended usage is a one-time download and then local storage on the device.  This will allow for simpler and far less data-intensive calls later.

Url: http://corvallisbus.azurewebsites.net/transit/static

```
{
   "routes":
   {
      "1":
      {
        "path": [
          14244,
          13265,
          ...
        ]
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
        "routes": [
          "C3"
        ]
      },
      ...
    }
```

###/transit/eta/

Input:
   - **Required** one or more Stop IDs

Output:

   Returns a JSON dictionary, where they keys are the supplied Stop IDs, and the values are dictionaries.  These dested dictionaries are such that the keys are route numbers, and the values are integers corresponding to the ETA for that route to that stop.  For example, ``"2":21"`` means that Route 2 is arriving at the given stop in 21 minutes.

Sample Url: http://corvallisbus.azurewebsites.net/transit/eta/14244,13265

```
{
  "14244": {
    
  },
  "13265": {
    "1": 6,
    "2":21,
    "5":22,
    "8":22
  }
}
```

###/transit/schedule/

Input: 
   - **Required** one or more stop IDs

Output:

   A JSON dictionary where the keys are Stop IDs and the values are dictionaries of ``{ Route No : schedule }``.

Sample Url: http://corvallisbus.azurewebsites.net/transit/schedule/14244,13265

```
{
  "14244": {
    "1": [
      
    ],
    "2": [
      "2015-07-29 19:25"
    ],
    "4": [
      
    ]
  },
  "13265": {
    "1": [
      
    ],
    "2": [
      "2015-07-29 19:25"
    ],
    "3": [
      
    ],
    "5": [
      "2015-07-29 19:25",
      "2015-07-29 19:55",
      "2015-07-29 20:25",
      "2015-07-29 20:55"
    ],
    "7": [
      "2015-07-29 19:30"
    ],
    "8": [
      
    ]
  }
}
```
### /transit/favorites

Input:
   - **Required** one or more stop IDs, comma-separated
   - **Optional** latitude and longitude, comma-separated

Output:

   A JSON array of stop information for "favorite stops" features.  See sample for details.
   
Sample Url: http://corvallisbus.azurewebsites.net/transit/favorites?stops=11776,10308&location=44.5645659,-123.2620435
   
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
