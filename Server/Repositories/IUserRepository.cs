using DemoForum.Models.Entities;

namespace DemoForum.Repositories;

public interface IUserRepository : ICrudRepository<User, string>
{
    Task<(bool, User?)> CheckLoginValid(User user);
}