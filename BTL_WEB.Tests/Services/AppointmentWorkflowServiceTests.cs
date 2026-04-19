using BTL_WEB.Models;
using BTL_WEB.Services;
using BTL_WEB.Tests.TestInfrastructure;
using BTL_WEB.ViewModels.Appointments;

namespace BTL_WEB.Tests.Services;

public class AppointmentWorkflowServiceTests
{
    [Fact]
    public async Task CreateAppointmentAsync_ReturnsError_ForPastDate()
    {
        await using var context = TestDbFactory.CreateContext();
        var service = new AppointmentWorkflowService(context);

        var model = new AppointmentCreateViewModel
        {
            AppointmentDateTime = DateTime.Now.AddMinutes(-1),
            SelectedServiceIds = [1]
        };

        var result = await service.CreateAppointmentAsync(currentUserId: 1, isCustomer: true, model);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task CreateAppointmentAsync_CreatesAppointmentAndServices_WhenInputValid()
    {
        await using var context = TestDbFactory.CreateContext();
        SeedForAppointment(context);
        var service = new AppointmentWorkflowService(context);

        var model = new AppointmentCreateViewModel
        {
            UserId = 10,
            PetId = 100,
            BranchId = 1,
            AppointmentDateTime = DateTime.Now.AddDays(1),
            SelectedServiceIds = [201, 202],
            Notes = "Test"
        };

        var result = await service.CreateAppointmentAsync(currentUserId: 10, isCustomer: true, model);

        Assert.True(result.Success);
        Assert.NotNull(result.AppointmentId);

        var created = await context.Appointments.FindAsync(result.AppointmentId);
        Assert.NotNull(created);
        Assert.Equal("Pending", created!.Status);

        var lines = context.AppointmentServices.Where(x => x.AppointmentId == result.AppointmentId).ToList();
        Assert.Equal(2, lines.Count);
        Assert.All(lines, x => Assert.Equal(1, x.Quantity));
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsError_ForInvalidStatus()
    {
        await using var context = TestDbFactory.CreateContext();
        var service = new AppointmentWorkflowService(context);

        var result = await service.UpdateStatusAsync(appointmentId: 1, status: "Invalid");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CalculateTotalAsync_ReturnsSumOfAppointmentServices()
    {
        await using var context = TestDbFactory.CreateContext();
        context.AppointmentServices.AddRange(
            new AppointmentService { AppointmentId = 1, ServiceId = 1, Quantity = 2, UnitPrice = 50 },
            new AppointmentService { AppointmentId = 1, ServiceId = 2, Quantity = 1, UnitPrice = 75 });
        await context.SaveChangesAsync();

        var service = new AppointmentWorkflowService(context);
        var total = await service.CalculateTotalAsync(1);

        Assert.Equal(175, total);
    }

    private static void SeedForAppointment(PetCareHubContext context)
    {
        context.Branches.Add(new Branch
        {
            BranchId = 1,
            BranchName = "Main",
            Address = "Addr",
            Status = "Active"
        });

        context.Users.Add(new User
        {
            UserId = 10,
            Username = "u10",
            PasswordHash = "hash",
            FullName = "User 10",
            Email = "u10@test.local",
            RoleId = 1,
            Status = "Active",
            CreatedAt = DateTime.Now
        });

        context.Pets.Add(new Pet
        {
            PetId = 100,
            Name = "Pet 100",
            Species = "Dog",
            AdoptionStatus = "Available",
            BranchId = 1,
            OwnerId = 10,
            Status = "Active",
            CreatedAt = DateTime.Now
        });

        context.Services.AddRange(
            new Service { ServiceId = 201, CategoryId = 1, ServiceName = "S1", Price = 100, DurationMinutes = 30, Status = "Active" },
            new Service { ServiceId = 202, CategoryId = 1, ServiceName = "S2", Price = 200, DurationMinutes = 45, Status = "Active" });

        context.SaveChanges();
    }
}
