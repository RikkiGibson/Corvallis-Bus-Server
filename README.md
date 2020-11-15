## Corvallis Bus Server

[![Build Status](https://travis-ci.org/RikkiGibson/Corvallis-Bus-Server.svg?branch=NetCore)](https://travis-ci.org/RikkiGibson/Corvallis-Bus-Server)

The backend that powers the best apps for the Corvallis Transit System.
- [iOS client](https://github.com/RikkiGibson/Corvallis-Bus-iOS)
- [Android client](https://github.com/OSU-App-Club/Corvallis-Bus-Android)
- [Web client](https://github.com/RikkiGibson/corvallis-bus-web)

## Prerequisites for running

The [.NET SDK](https://dotnet.microsoft.com/download) must be installed. The precise version that you should install can be found in [global.json](global.json). Then you can run the following commands in the repo root directory:
```sh
# If you want to run tests
$ dotnet test CorvallisBus.Test

# Run the web app
$ cd CorvallisBus.Web
$ dotnet run

# Run the data init job locally by sending a POST request
$ curl -d {} localhost:57855/api/job/init
```

## Purpose

To have a more convenient way to get real-time information about the free buses in Corvallis.  Data from CTS is merged with data from Google Transit, with some convenient projections applied, and mapped into some easily-digestable JSON for different use cases.

## Disclaimer

We assume no liability for any missed buses.  Buses may be erratic in their arrival behavior, and we cannot control that.

## API Documentation

Visit https://corvallisb.us/api for documentation.
