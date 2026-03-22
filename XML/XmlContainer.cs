using System.Reflection;

namespace Playground.EFCore.XML
{
    public class XmlContainer : IXmlContainer
    {
        private readonly Dictionary<Type, IXmlRepository> _repositories;
        private readonly XmlRecursiveProcessor _processor;
        public XmlContainer(params (Type entityType, string filePath)[] configurations)
        {
            _repositories = new Dictionary<Type, IXmlRepository>();
            _processor = new XmlRecursiveProcessor(_repositories);

            foreach (var (entityType, filePath) in configurations) 
            {
                var repoType = typeof(XmlRepository<>).MakeGenericType(entityType);
                var repo = (IXmlRepository)Activator.CreateInstance(repoType, filePath)!;
                _repositories[entityType] = repo;
            }
        }
        private IXmlRepository<T> GetRepo<T>() where T : class, IEntity
        {
            if (_repositories.TryGetValue(typeof(T), out var repo))
                return (IXmlRepository<T>)repo;

            throw new NotSupportedException($"Type {typeof(T).Name} not registered");
        }
        public List<T> GetAll<T>() where T : class, IEntity => GetRepo<T>().GetAll();
        public T? GetById<T>(int id) where T : class, IEntity => GetRepo<T>().GetById(id);
        public T Add<T>(T entity) where T : class, IEntity => GetRepo<T>().Add(entity);
        public T? Update<T>(T entity) where T : class, IEntity => GetRepo<T>().Update(entity);
        public bool Delete<T>(int id) where T : class, IEntity => GetRepo<T>().Delete(id);
        public List<T> GetAllInclude<T>() where T : class, IEntity
        {
            var entities = GetRepo<T>().GetAll();
            _processor.ResetVisited();
            foreach (var entity in entities)
                if (entity != null) _processor.ProcessInclude(entity);
            return entities;
        }
        public T? GetByIdInclude<T>(int id) where T : class, IEntity
        {
            var entity = GetRepo<T>().GetById(id);
            _processor.ResetVisited();
            if (entity != null) _processor.ProcessInclude(entity);
            return entity;
        }

        public T AddInclude<T>(T entity) where T : class, IEntity
        {
            _processor.ResetVisited();
            _processor.ProcessAddInclude(entity);
            return GetRepo<T>().Add(entity);
        }

        public T? UpdateInclude<T>(T entity) where T : class, IEntity
        {
            _processor.ResetVisited();
            _processor.ProcessUpdateInclude(entity);
            return GetRepo<T>().Update(entity);
        }

        public bool DeleteInclude<T>(int id) where T : class, IEntity
        {
            _processor.ResetVisited();
            var entity = GetRepo<T>().GetById(id);
            if (entity == null) return false;

            foreach (var (entityType, entityId) in _processor.ProcessDeleteInclude(entity))
            {
                if (_repositories.TryGetValue(entityType, out var repository))
                    repository.Delete(entityId);
            }
            return true;
        }
    }
}
