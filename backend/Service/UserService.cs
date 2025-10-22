using Microsoft.EntityFrameworkCore;

using backend.Exceptions;
using backend.Interfaces;
using backend.Models;
using backend.Resources;

namespace backend.Services
{
    public class UserService : IUserService
    {
        private readonly AppDatabaseContext _context;

        public UserService(AppDatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new NotFoundException($"User with the id {id} is not found");
            }
            return user;
        }

        public async Task<bool> UpdateUserAsync(int id, User updatedUser)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                throw new NotFoundException($"User with the id {id} is not found");
            }

            existingUser.Username = updatedUser.Username;
            existingUser.Password = updatedUser.Password;
            existingUser.Usertype = updatedUser.Usertype;
            existingUser.Name = updatedUser.Name;
            existingUser.Email = updatedUser.Email;
            existingUser.Address = updatedUser.Address;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new NotFoundException($"User with the id {id} is not found");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}