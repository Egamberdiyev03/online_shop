using DataAccess.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class Repository<Entity> : IRepository<Entity> where Entity : class 
    {
        private readonly AppDbContext _context;
        public Repository(AppDbContext context)
        {
            _context = context;
            context.Set<Entity>();
        }
        public  async Task<List<Entity>> GetAllAsync()
        {
            var entities = await _context.Set<Entity>().ToListAsync();
            return entities;   
        }
        public async Task<Entity> GetByIdAsync(int id)
        { 
            return await _context.Set<Entity>().FindAsync(id);
        }
        public async Task<Entity> AddAsync(Entity entity)
        {
            await _context.Set<Entity>().AddAsync(entity);
            return entity;
        }
        public async Task<Entity> UpdateAsync(Entity entity)
        {
            _context.Set<Entity>().Update(entity);
            return entity;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Set<Entity>().FindAsync(id);
            if (entity == null)
                return false;

            _context.Set<Entity>().Remove(entity);
            _context.SaveChanges();
            return true;
        }

        public IQueryable<Entity> GetAsQueryable()
        {
            return _context.Set<Entity>().AsQueryable();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        
    }
}
