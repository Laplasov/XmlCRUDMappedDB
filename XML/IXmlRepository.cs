using System;
using System.Collections.Generic;
using System.Text;

namespace Playground.EFCore.XML
{
    public interface IXmlRepository
    {
        object GetAll();
        object? GetById(int id);
        object Add(object entity);
        object? Update(object entity);
        bool Delete(int id);
    }
    public interface IXmlRepository<T> : IXmlRepository where T : class, IEntity
    {
        new List<T> GetAll();
        new T? GetById(int id);
        T Add(T entity);
        T? Update(T entity);
        new bool Delete(int id); 
    }
}
