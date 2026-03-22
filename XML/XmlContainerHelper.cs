using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;

namespace Playground.EFCore.XML
{
    public class XmlContainerHelper
    {
        private readonly HashSet<(Type, int)> _visited = new();

        private readonly Dictionary<Type, IXmlRepository> _repositories;

        public XmlContainerHelper(Dictionary<Type, IXmlRepository> repositories) => 
            _repositories = repositories;

        public bool IsRecursive(IEntity entity)
        {
            var entityKey = (entity.GetType(), entity.Id);
            if (!_visited.Add(entityKey)) 
                return true;
            else 
                return false;
        }
        public void ResetVisited() => _visited.Clear();

        // Find navigation properties with [XmlForeignKey]
        public IEnumerable<(PropertyInfo navigationPropperte, XmlForeignKeyAttribute ForeignKeyAttribute)>
            GetNavigationProppertes(object entity)
        {
            return entity.GetType().GetProperties()
                .Select(p => new { Propperte = p, Attribute = p.GetCustomAttribute<XmlForeignKeyAttribute>() })
                .Where(x => x.Attribute != null)
                .Select(x => (x.Propperte, x.Attribute!));
        }

        // Get foreign key property info
        public (PropertyInfo ForeignKeyPropperte, int ForeignKeyId)
            GetForeignKeyPropperteInfo(object entity, string ForeignKeyPropertyName)
        {
            var ForeignKeyPropperte = entity.GetType().GetProperty(ForeignKeyPropertyName);
            var ForeignKeyId = ForeignKeyPropperte?.GetValue(entity) as int? ?? 0;

            if (ForeignKeyPropperte == null)
                throw new ArgumentException("ForeignKeyPropperte is null");

            return (ForeignKeyPropperte, ForeignKeyId);
        }

        public IEnumerable<(
            PropertyInfo navigationPropperte,
            PropertyInfo ForeignKeyPropperte,
            int ForeignKeyId,
            IXmlRepository repository)>
        GetNavigationContext(object entity)
        {
            foreach (var (navigationPropperte, ForeignKeyAttribute) in GetNavigationProppertes(entity))
            {
                var (ForeignKeyPropperte, ForeignKeyId) = GetForeignKeyPropperteInfo(entity, ForeignKeyAttribute.ForeignKeyProperty);

                var navigationType = navigationPropperte.PropertyType;

                if (navigationType.IsGenericType && navigationType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    navigationType = navigationType.GetGenericArguments()[0];
                }
                if (_repositories.TryGetValue(navigationType, out var repository))
                {
                    yield return (navigationPropperte, ForeignKeyPropperte, ForeignKeyId, repository);
                }
            }
        }
    }
}
