using ETICARET.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ETICARET.DataAccess.Abstract
{
    // Repository pattern için temel arayüz 
    // Ortak CRUD operasyonlarını tanımlamak için kullanılabilir
    public interface IRepository<T>
    {
        T GetById(int id);
        T GetOne(Expression<Func<T,bool>> filter = null);
        List<T> GetAll(Expression<Func<T, bool>> filter = null);
        void Create(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
