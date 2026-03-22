namespace Playground.EFCore.XML
{
    public interface IXmlContainer
    {
        public List<T> GetAll<T>() where T : class, IEntity;
        public T? GetById<T>(int id) where T : class, IEntity;
        public T Add<T>(T entity) where T : class, IEntity;
        public T? Update<T>(T entity) where T : class, IEntity;
        public bool Delete<T>(int id) where T : class, IEntity;
    }
}
