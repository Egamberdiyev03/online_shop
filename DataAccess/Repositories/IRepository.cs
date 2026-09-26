using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public interface IRepository<Entity> where Entity : class
    {
        Task<Entity> AddAsync(Entity entity);
        Task<Entity> UpdateAsync(Entity entity);
        Task<Entity> GetByIdAsync(int id);
        Task<List<Entity>> GetAllAsync();
        IQueryable<Entity> GetAsQueryable();
        Task<bool> DeleteAsync(int id);
        Task SaveChangesAsync();
    }
}
