
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace VNNSIS.Interface
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetById(int id);
        IEnumerable<T> GetAll();
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        Task Add(T entity);
        Task AddRange(IEnumerable<T> entities);
        Task Remove(int id);
        void RemoveRange(IEnumerable<T> entities);
        void Update(T entity);
      
    }
}