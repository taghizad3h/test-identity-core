using Microsoft.EntityFrameworkCore;
using TestIdentityCore.Domain;
using TestIdentityCore.Repository.Base;

namespace TestIdentityCore.Repository;

public class UserRepository(ApplicationDbContext dbContext) : Repository<ApplicationUser>(dbContext), IUserRepository
{
    
    public async Task<ApplicationUser?> FindByEmailAsync(string email)
    {
        // EF Core LINQ by default uses parameterized query which prevents SQL injection
        return await dbContext.Set<ApplicationUser>().FirstOrDefaultAsync(x => x.Email == email);
    }
    
    public async Task<ApplicationUser?> FindByEmailAsyncRawSqlSafe(string email)
    {
        // Using parameterized query which prevents SQL injection
        return await dbContext.Set<ApplicationUser>()
            .FromSqlRaw("SELECT * FROM ApplicationUsers WHERE Email = {0}", email)
            .FirstOrDefaultAsync();
    }

    public async Task<ApplicationUser?> FindByEmailAsyncRawSqlUnsafe(string email)
    {
        // Using string interpolation which does not prevent SQL injection
        return await dbContext.Set<ApplicationUser>()
            .FromSqlRaw($"SELECT * FROM AspNetUsers WHERE Email = '{email}'")
            .FirstOrDefaultAsync();
    }
}