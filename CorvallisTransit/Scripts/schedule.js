// compares our stop objects we're generating
// the tables from based on the stops eta, or the route's number
// if the stops eta is equal. This is used to sort our new tables
// data before generating the tables.
function etaComparer(a, b) {
    if (a.stop.eta > b.stop.eta) {
        return 1;
    }
    else if (a.stop.eta < b.stop.eta) {
        return -1;
    }
    else if (a.route.num > b.route.num) {
        return 1;
    }
    else if (a.route.num < b.route.num) {
        return -1;
    }
    return 0;
}

// search handler
function searchStop(stopNo) {

    // targetStopNum will form the basis of our regexp
    // to match addresses, if it isn't a stop number
    var targetStopNum = [];

    // if we have a stop number, then we need to just use that for
    // our regular expression otherwise check to see if we have 
    // anything at all in the search box
    if (stopNo != undefined) {
        targetStopNum = [stopNo];
        $("#stopSearch").val(targetStopNum[0]);
    }
    else if ($("#stopSearch").val().length > 0) {
        targetStopNum = $("#stopSearch").val().split(' ');
    }
    // if there isn't anything in the box and they haven't clicked
    // then just break and don't do anything
    if (targetStopNum.length == 0) {
        return;
    }

    // check the dropdown for the selected search type
    // and build the regular expression
    // the logic will be, find our substrings in order, or in reverse order
    // the reverse order covers when someone enters crossstreet street instead
    // of street crossstreet,  among other things.
    // the second type will be an or search
    // TODO: make this expression better.
    var addressRegExp;
    var searchType = $("#searchType").val();

    if (searchType == "and") {
        addressRegExp = new RegExp(targetStopNum.join('.*') + "|" + targetStopNum.reverse().join('.*'), "i");;
    }
    else {
        addressRegExp = new RegExp(targetStopNum.join('|'), "i");
    }

    var foundRoutes = {};
    var foundRouteCounter = 0;

    // for each route, check the stops and if we match either as the target stop
    // number or match our regular expression, we'll increment our counter
    // and add to our associative array based on the stop address for each
    // route's qualifying stops. the associative array will be used for labelling etc
    // TODO: change associative array to regular array, now that the matching stops
    // also match streets and are organized by route there is no need to have
    // any associative array here
    for (var i = 0; i < routes.length; ++i) {
        var foundStops = {};
        var foundStopCounter = 0;

        for (var j = 0; j < routes[i].stops.length; ++j) {
            var stopNum = routes[i].stops[j].model.StopNumber;
            var stopAddress = routes[i].stops[j].model.Address;

            // either it's our target stop or it matches our regular expression
            if (stopNum == targetStopNum                             
                || (stopAddress.match(addressRegExp) != null
                    && stopAddress.match(addressRegExp).length > 0)) {

                // check to see if we have any times for the given stop address
                // if we don't yet then create the new array so we can push onto it
                if (foundStops[stopAddress] == undefined) {
                    foundStops[stopAddress] = [];
                }

                // associate the stop and time in our array and increase the counter
                // for this stop 
                // TODO: fix this as specified above, 
                // originally used when stops were grouped by address intead of route
                // though we could still change that to enable either view
                foundStops[stopAddress].push({ stop: routes[i].stops[j], route: routes[i] });
                foundStopCounter++;
            }
        }
        // only assign if we have some
        if (foundStopCounter > 0) {
            foundRoutes[routes[i].num] = foundStops;
            foundRouteCounter += foundStopCounter;
        }
    }

    // remove all our previous results from the page
    $(".foundRoute").remove();

    // go over our keys in the associative arrays, generating the route container
    // and then iterating over the matched stops
    for (var foundRouteKey in foundRoutes) {
        var foundRouteWrapper = $("<div class='foundRoute'><h3>Route " + foundRouteKey + "</h3></div>");
        var foundStops = foundRoutes[foundRouteKey];

        // TODO: fix code here referring to the keys for their label purposes
        for (var foundStopKey in foundStops) {
            var foundStop = foundStops[foundStopKey];
            var container = $("<div class='targetStopContainer container'></div>");
            var table = $("<table class='targetStopTable'></table>");

            var newHeader = $("<div class='h4 searchAddress'></div>");

            newHeader.text(foundStopKey);
            newHeader = newHeader.appendTo(container);
            table = table.appendTo(container);

            foundStop = foundStop.sort(etaComparer);

            // we only need the ETA row now that we aren't grouping by street address
            // TODO: allow either view
            // also, this row grows out horizontally, it doesn't grow the table vertically
            var etaRow = $("<tr><td>ETA</td></tr>");

            for (var j = 0; j < foundStop.length; ++j) {
                // only show the details if 
                if (foundStop[j].stop.eta > 0) {
                    // make our unique id for this in case we want to update it later
                    etaRow.append($("<td>" + foundStop[j].stop.eta + "m</td>"));
                }
            }
            // insert the rows, make the table visible
            table.append(etaRow);

            container.appendTo(foundRouteWrapper);
        }
        foundRouteWrapper.appendTo("#searchBox");
    };

    // display the number of results found
    $("#searchResultHeader").text(foundRouteCounter + " results found.")
    $("#searchResultHeader").removeClass('hidden');

}


// document.ready handler
$(function () {
    // since our search button isn't a submit type, we have to listen
    // for the enter key if someone presses it.
    $("#stopSearch").keydown(function (e) {
        if (e.which === 13) {
            $('#searchButton').trigger('click');
        }
    });

    // bind the headers toggle for + and - tags
    $('.accordion-toggle').on('click', function () {
        var isCollapsed = $(this).parent().find(".panel-collapse").hasClass('collapse');
        var icon = $(this).find(".panelIcon");
        if (isCollapsed) {
            icon.html("&ndash;");
        }
        else {
            icon.text("+");
        }
    });

    // this is called by the server when we have new route info
    // it updates tables for the routes we are provided info for
    function updateRoute(route) {
        var routeIndex = -1;

        // figure out which route we're dealing with in our routes list
        for (var i = 0; i < routes.length; ++i) {
            if (routes[i].num == route.num) {
                routeIndex = i;
                break;
            }
        }

        // if we don't have that route in our list yet, create it
        // otherwise just insert it to the end of the list
        if (routeIndex > -1) {
            routes[routeIndex] = route;
        }
        else {
            routes.push(route);
        }

        // grab the previous table to replace and generate a new table skeleton
        var routeTable = $("#Route" + route.num + "Panel table");
        var newTable = $("<table class='table-responsive table'><tr><th class='stop-address'>Stop Location</th><th class='eta'>ETA</th></tr></table>");

        // we're going to say m at the end, but if the stop time has a warning
        // then we want to properly display the *
        var timeUnit = "m" + (route.warningClasses != " " ? "*" : "");

        // insert all of the stops
        for (var i = 0; i < route.stops.length; ++i) {
            var stop = route.stops[i];
            newTable.append("<tr class='" + route.warningClasses + "'><td class='stop-address'><a href='#searchBox' onclick='searchStop(" + stop.model.StopNumber + ");'>" + stop.model.Address + " (" + stop.model.StopNumber + ")</a></td><td class='eta'>" + stop.eta + timeUnit + "</td></tr>");
        }

        // replace the previous route's table
        routeTable.replaceWith(newTable);

        // if we don't have any data, then hide this route
        if (newTable.find("td").length == 0) {
            $("#Route" + route.num).addClass("hidden");
        }
        else {
            $("#Route" + route.num).removeClass("hidden");
        }

        // update the search tables based on our new information
        searchStop($("#searchStop").val());
    }

    // bind the hubs for SignalR
    var routeHub = $.connection.routeHub;
    routeHub.client.updateRoute = updateRoute;

    $.connection.hub.start();
});