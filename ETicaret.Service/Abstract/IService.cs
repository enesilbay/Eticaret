using ETicaret.Core.Entities;
using System.Linq.Expressions;

namespace ETicaret.Service.Abstract
{
    public interface IService<T> where T : class, IEntity, new()
    {
        List<T> GetAll();
        List<T> GetAll(Expression<Func<T,bool>> expression);
        IQueryable<T> GetQueryable();
        T Get(Expression<Func<T, bool>> expression);
        T Find(int id);
        void Add(T entity);
        void Delete(T entity);
        void Update(T entity);
        int SavaChanges();

        //Asenkron metotlar
        Task<T> FindAsync(int id);
        Task<T> GetAsync(Expression<Func<T, bool>> expression); //x=>x.name gibi sorgular yapabilmeyi sağlayan metot
        Task<List<T>> GetAllAsync();
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>> expression);
        Task AddAsync(T entity);
        Task<int> SavaChangesAsync();




    }
}
