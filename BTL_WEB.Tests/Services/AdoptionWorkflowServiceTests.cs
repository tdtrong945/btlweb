using BTL_WEB.Models;
using BTL_WEB.Services;
using BTL_WEB.Tests.TestInfrastructure;
using BTL_WEB.ViewModels.Adoption;

namespace BTL_WEB.Tests.Services;

public class AdoptionWorkflowServiceTests
{
    [Fact]
    public async Task CreateRequestAsync_ReturnsError_WhenPetNotFound()
    {
        await using var context = TestDbFactory.CreateContext();
        var service = new AdoptionWorkflowService(context);

        var result = await service.CreateRequestAsync(1, new CreateAdoptionRequestViewModel { PetId = 999 });

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateRequestAsync_ReturnsError_WhenDuplicatePendingRequestExists()
    {
        await using var context = TestDbFactory.CreateContext();
        SeedForAdoption(context);
        context.AdoptionRequests.Add(new AdoptionRequest
        {
            RequestId = 10,
            PetId = 1,
            UserId = 2,
            RequestDate = DateTime.Now,
            Status = "Pending"
        });
        await context.SaveChangesAsync();

        var service = new AdoptionWorkflowService(context);
        var result = await service.CreateRequestAsync(3, new CreateAdoptionRequestViewModel { PetId = 1 });

        Assert.False(result.Success);
        Assert.Contains("dang cho xu ly", result.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ReviewRequestAsync_Approve_UpdatesStatusesAndCreatesContract()
    {
        await using var context = TestDbFactory.CreateContext();
        SeedForAdoption(context);

        context.AdoptionRequests.Add(new AdoptionRequest
        {
            RequestId = 20,
            PetId = 1,
            UserId = 3,
            RequestDate = DateTime.Now,
            Status = "Pending"
        });

        context.Staff.Add(new Staff
        {
            StaffId = 99,
            UserId = 2,
            BranchId = 1,
            Position = "Manager",
            HireDate = DateOnly.FromDateTime(DateTime.Today),
            Status = "Active"
        });

        await context.SaveChangesAsync();

        var service = new AdoptionWorkflowService(context);
        var result = await service.ReviewRequestAsync(2, new ReviewAdoptionRequestViewModel
        {
            RequestId = 20,
            Action = "Approve",
            AdoptionFee = 150,
            Terms = "Sample terms"
        });

        Assert.True(result.Success);

        var request = await context.AdoptionRequests.FindAsync(20);
        var pet = await context.Pets.FindAsync(1);
        var contract = context.AdoptionContracts.Single(x => x.RequestId == 20);

        Assert.Equal("Approved", request!.Status);
        Assert.Equal(99, request.ReviewedByStaffId);
        Assert.Equal("Adopted", pet!.AdoptionStatus);
        Assert.Equal(3, pet.OwnerId);
        Assert.Equal(150, contract.AdoptionFee);
        Assert.Equal("Active", contract.Status);
    }

    [Fact]
    public async Task ReviewRequestAsync_Reject_ReturnsPetToAvailable()
    {
        await using var context = TestDbFactory.CreateContext();
        SeedForAdoption(context);

        context.AdoptionRequests.Add(new AdoptionRequest
        {
            RequestId = 21,
            PetId = 1,
            UserId = 3,
            RequestDate = DateTime.Now,
            Status = "Pending"
        });

        await context.SaveChangesAsync();

        var service = new AdoptionWorkflowService(context);
        var result = await service.ReviewRequestAsync(2, new ReviewAdoptionRequestViewModel
        {
            RequestId = 21,
            Action = "Reject"
        });

        Assert.True(result.Success);
        var pet = await context.Pets.FindAsync(1);
        Assert.Equal("Available", pet!.AdoptionStatus);
    }

    private static void SeedForAdoption(PetCareHubContext context)
    {
        context.Users.AddRange(
            new User
            {
                UserId = 2,
                Username = "staff.user",
                PasswordHash = "hash",
                FullName = "Staff User",
                Email = "staff@test.local",
                RoleId = 1,
                Status = "Active",
                CreatedAt = DateTime.Now
            },
            new User
            {
                UserId = 3,
                Username = "adopter.user",
                PasswordHash = "hash",
                FullName = "Adopter User",
                Email = "adopter@test.local",
                RoleId = 1,
                Status = "Active",
                CreatedAt = DateTime.Now
            });

        context.Branches.Add(new Branch
        {
            BranchId = 1,
            BranchName = "Main",
            Address = "Addr",
            Status = "Active"
        });

        context.Pets.Add(new Pet
        {
            PetId = 1,
            Name = "Lucky",
            Species = "Dog",
            BranchId = 1,
            AdoptionStatus = "Available",
            Status = "Active",
            CreatedAt = DateTime.Now
        });

        context.SaveChanges();
    }
}
