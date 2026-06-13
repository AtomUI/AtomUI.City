using AtomUI.City.Routing;

namespace AtomUI.City.Routing.Tests;

public sealed class RouteTemplateTests
{
    [Theory]
    [InlineData("")]
    [InlineData("/")]
    public void ParseSupportsRootTemplate(string pattern)
    {
        var template = RouteTemplate.Parse(pattern);

        Assert.Equal(string.Empty, template.Pattern);
        Assert.Empty(template.Segments);
        Assert.True(template.TryMatch("/", out var values));
        Assert.Empty(values);
    }

    [Fact]
    public void ParseSupportsAspNetStyleTemplateSegments()
    {
        var template = RouteTemplate.Parse("items/{id:int}/{slug=overview}/files/{*path}");

        Assert.Equal("items/{id:int}/{slug=overview}/files/{*path}", template.Pattern);
        Assert.Collection(
            template.Segments,
            segment => Assert.Equal(RouteTemplateSegmentKind.Literal, segment.Kind),
            segment =>
            {
                Assert.Equal(RouteTemplateSegmentKind.Parameter, segment.Kind);
                Assert.Equal("id", segment.Name);
                Assert.Equal(["int"], segment.Constraints);
            },
            segment =>
            {
                Assert.Equal(RouteTemplateSegmentKind.Parameter, segment.Kind);
                Assert.Equal("slug", segment.Name);
                Assert.Equal("overview", segment.DefaultValue);
            },
            segment => Assert.Equal(RouteTemplateSegmentKind.Literal, segment.Kind),
            segment =>
            {
                Assert.Equal(RouteTemplateSegmentKind.CatchAll, segment.Kind);
                Assert.Equal("path", segment.Name);
            });
    }

    [Fact]
    public void TemplateCollectionsRejectExternalListMutation()
    {
        var template = RouteTemplate.Parse("items/{id:int}");
        var replacement = RouteTemplate.Parse("replacement").Segments[0];
        var segments = Assert.IsAssignableFrom<IList<RouteTemplateSegment>>(template.Segments);
        var constraints = Assert.IsAssignableFrom<IList<string>>(template.Segments[1].Constraints);

        Assert.Throws<NotSupportedException>(() => segments[0] = replacement);
        Assert.Throws<NotSupportedException>(() => constraints[0] = "guid");
        Assert.Equal(RouteTemplateSegmentKind.Literal, template.Segments[0].Kind);
        Assert.Equal("int", template.Segments[1].Constraints[0]);
    }

    [Fact]
    public void TryMatchExtractsParametersAndAppliesConstraints()
    {
        var template = RouteTemplate.Parse("orders/{id:int}/items/{itemId:guid}");
        var matched = template.TryMatch("orders/42/items/6f9619ff-8b86-d011-b42d-00cf4fc964ff", out var values);
        var rejected = template.TryMatch("orders/not-an-int/items/6f9619ff-8b86-d011-b42d-00cf4fc964ff", out _);

        Assert.True(matched);
        Assert.False(rejected);
        Assert.Equal("42", values["id"]);
        Assert.Equal("6f9619ff-8b86-d011-b42d-00cf4fc964ff", values["itemId"]);
    }

    [Fact]
    public void TryMatchSupportsOptionalDefaultAndCatchAllSegments()
    {
        var template = RouteTemplate.Parse("docs/{lang=en}/{*path}");

        Assert.True(template.TryMatch("docs", out var defaultValues));
        Assert.Equal("en", defaultValues["lang"]);

        Assert.True(template.TryMatch("docs/zh-CN/guides/getting-started", out var values));
        Assert.Equal("zh-CN", values["lang"]);
        Assert.Equal("guides/getting-started", values["path"]);
    }
}
