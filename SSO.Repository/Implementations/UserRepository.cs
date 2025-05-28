using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using SSO.Repository.Database;
using SSO.Repository.Entities;
using SSO.Repository.Interfaces;

namespace SSO.Repository.Implementations;

public class UserRepository : IUserRepository
{
    private readonly AppDBContext _dbContext;

    public UserRepository(AppDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<User> GetUser(string email)
    {
        User? user = await _dbContext.Users.Where(p => p.Email == email).FirstOrDefaultAsync();
        if (user == null)
        {
            user = new User
            {
                Email = email
            };
             _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }
        return user;
    }
}