using MicroLendingSystem.Database.AppDbContext;
using MicroLendingSystem.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace MicroLendingSystem.Backend.Authorization;

public static class RbacPermissionBootstrap
{
    public static async Task EnsureAsync(AppDbContext context, CancellationToken ct = default)
    {
        foreach (var (oldName, newName) in StandardPermissions.LegacyRenames)
        {
            var row = await context.Permissions.FirstOrDefaultAsync(p => p.Name == oldName, ct);
            if (row is null)
                continue;

            var taken = await context.Permissions.AnyAsync(p => p.Name == newName && p.Id != row.Id, ct);
            if (!taken)
                row.Name = newName;
        }

        await context.SaveChangesAsync(ct);

        var nameSet = (await context.Permissions.Select(p => p.Name).ToListAsync(ct)).ToHashSet();
        foreach (var name in StandardPermissions.All)
        {
            if (!nameSet.Contains(name))
                context.Permissions.Add(new Permission { Name = name });
        }

        await context.SaveChangesAsync(ct);

        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == PermissionNames.AdminRoleName, ct);
        if (adminRole is not null)
        {
            var allIds = await context.Permissions.Select(p => p.Id).ToListAsync(ct);
            var existing = await context.RolePermissions
                .Where(rp => rp.RoleId == adminRole.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync(ct);

            foreach (var pid in allIds.Where(pid => !existing.Contains(pid)))
                context.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = pid });

            await context.SaveChangesAsync(ct);
        }

        var borrowerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Borrower", ct);
        if (borrowerRole is null)
        {
            borrowerRole = new Role { Name = "Borrower" };
            context.Roles.Add(borrowerRole);
            await context.SaveChangesAsync(ct);
        }

        var borrowerPermNames = new[]
        {
            PermissionNames.Borrower_Read,
            PermissionNames.LoanRequest_List,
            PermissionNames.Loan_Read,
            PermissionNames.Loan_Create,
            PermissionNames.Loan_Repay,
            PermissionNames.Transaction_List
        };

        var borrowerPermIds = await context.Permissions
            .Where(p => borrowerPermNames.Contains(p.Name))
            .Select(p => p.Id)
            .ToListAsync(ct);

        var brExisting = await context.RolePermissions
            .Where(rp => rp.RoleId == borrowerRole.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync(ct);

        foreach (var pid in borrowerPermIds.Where(pid => !brExisting.Contains(pid)))
            context.RolePermissions.Add(new RolePermission { RoleId = borrowerRole.Id, PermissionId = pid });

        await context.SaveChangesAsync(ct);
    }
}
