using TmsApi.Infrastructure.Caching;

namespace TmsApi.Tests.Caching;

public class CacheKeysTests
{
    [Fact]
    public void Course_UsesSchemaVersionAndNormalizedCode()
    {
        var key = CacheKeys.Course("CSE-101");

        Assert.Equal("V2:course:cse-101", key);
    }
}
