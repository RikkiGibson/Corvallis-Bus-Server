using CorvallisBus.Core.Models.Connexionz;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Xml.Serialization;
using Xunit;

namespace MyTestProject;

public class MyTests
{
    [Fact]
    public void RoutePositionET()
    {
        var xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <RoutePositionET>
              <Content Expires="2023-04-08T16:23:31-07:00" MaxArrivalScope="60" />
              <Platform PlatformTag="332" Name="NW Kings Blvd &amp; NW Hayes Ave" />
            </RoutePositionET>
            """;
        var serializer = new XmlSerializer(typeof(RoutePositionET));
        var entity = (RoutePositionET)serializer.Deserialize(new StringReader(xml))!;
        var items = entity.Items;
        Assert.Equal(2, items.Length);

        var content = (RoutePositionContent)items[0];
        Assert.Equal("04/08/2023 16:23:31", content.Expires.ToString(CultureInfo.InvariantCulture));
        Assert.Equal(60, content.MaxArrivalScope);

        var platform = (RoutePositionPlatform)items[1];
        Assert.Equal("332", platform.PlatformTag);

        Assert.Null(platform.Route);
    }

    [Fact]
    public void RouteProject()
    {
        var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("CorvallisBus.Test.RoutePattern.xml") ?? throw new Exception();
        var reader = new StreamReader(resource);
        var content = reader.ReadToEnd();

        var serializer = new XmlSerializer(typeof(RoutePattern));
        var entity = (RoutePattern)serializer.Deserialize(new StringReader(content))!;
        var project = (RoutePatternProject)entity.Items[1];
        Assert.NotEmpty(project.Route);
    }
}
