using SSO.Repository.Entities;

namespace SSO.Repository.Interfaces;

public interface IUserRepository
{
    public Task<User> GetUser(string email);
    
}