using BTL_WEB.Helpers;
using BTL_WEB.Models;
using BTL_WEB.Tests.TestInfrastructure;

namespace BTL_WEB.Tests.Helpers;

public class PaginatedListTests
{
    [Fact]
    public async Task CreateAsync_ReturnsExpectedPageData()
    {
        await using var context = TestDbFactory.CreateContext();
        context.Users.AddRange(Enumerable.Range(1, 10).Select(i => new User
        {
            UserId = i,
            Username = $"user{i}",
            PasswordHash = "hash",
            FullName = $"User {i}",
            Email = $"user{i}@test.local",
            RoleId = 1,
            Status = "Active",
            CreatedAt = DateTime.Now
        }));
        await context.SaveChangesAsync();

        var source = context.Users.OrderBy(x => x.UserId).Select(x => x.UserId);
        var result = await PaginatedList<int>.CreateAsync(source, pageIndex: 2, pageSize: 3);

        Assert.Equal(new[] { 4, 5, 6 }, result.Items);
        Assert.Equal(2, result.PageIndex);
        Assert.Equal(4, result.TotalPages);
        Assert.Equal(10, result.TotalItems);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(1, 0)]
    public async Task CreateAsync_ThrowsArgumentOutOfRange_ForInvalidPagingInput(int pageIndex, int pageSize)
    {
        await using var context = TestDbFactory.CreateContext();
        context.Users.Add(new User
        {
            UserId = 1,
            Username = "user1",
            PasswordHash = "hash",
            FullName = "User 1",
            Email = "user1@test.local",
            RoleId = 1,
            Status = "Active",
            CreatedAt = DateTime.Now
        });
        await context.SaveChangesAsync();

        var source = context.Users.Select(x => x.UserId);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => PaginatedList<int>.CreateAsync(source, pageIndex, pageSize));
    }
}
