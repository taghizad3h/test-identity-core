using TestIdentityCore.Domain;
using TestIdentityCore.Repository.Base;

namespace TestIdentityCore.Repository;

public interface IUserRepository : IRepository<ApplicationUser>
{
    public Task<ApplicationUser?> FindByEmailAsync(string email);
    public Task<ApplicationUser?> FindByEmailAsyncRawSqlSafe(string email);
    public Task<ApplicationUser?> FindByEmailAsyncRawSqlUnsafe(string email);

}