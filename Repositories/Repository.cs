using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VNNSIS.Interface;

namespace VNNSIS.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DbSet<T> _entities;

        public Repository(DbContext context)
        {
            try{
                _entities = context.Set<T>();
            }
            catch(Exception)
            {

            }
           
        }
        public async Task<T> GetById(int id)
        {
            return await _entities.FindAsync(id);
        }
        public IEnumerable<T> GetAll()
        {
            return _entities.ToList();
        }
        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _entities.Where(predicate);
        }
        public async Task Add(T entity)
        {
            await _entities.AddAsync(entity);
        }
        public async Task AddRange(IEnumerable<T> entities)
        {
            await _entities.AddRangeAsync(entities);
        }
        public async Task Remove(int id)
        {
            var entity = await _entities.FindAsync(id);
            _entities.Remove(entity);
        }
        public void RemoveRange(IEnumerable<T> entities)
        {
            _entities.RemoveRange(entities);
        }
        public void Update(T entity)
        {
            _entities.Update(entity);
        }
       
    }
}