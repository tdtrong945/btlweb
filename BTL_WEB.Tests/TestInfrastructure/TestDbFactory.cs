using BTL_WEB.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_WEB.Tests.TestInfrastructure;

internal static class TestDbFactory
{
    public static PetCareHubContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PetCareHubContext>()
            .UseInMemoryDatabase(databaseName: $"btlweb-tests-{Guid.NewGuid():N}")
            .Options;

        return new PetCareHubContext(options);
    }
}
