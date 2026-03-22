using System;
using System.Collections.Generic;
using System.Reflection;

namespace Playground.EFCore.XML
{
    public class XmlRecursiveProcessor
    {
        private readonly Dictionary<Type, IXmlRepository> _repositories;
        private readonly XmlContainerHelper _helper;

        public XmlRecursiveProcessor(Dictionary<Type, IXmlRepository> repositories)
        {
            _repositories = repositories;
            _helper = new XmlContainerHelper(_repositories);
        }
        public void ResetVisited() => _helper.ResetVisited();
        public void ProcessInclude<T>(T entity) where T : class, IEntity
        {
            if (_helper.IsRecursive(entity)) return;

            foreach (var (navigationPropperte, ForeignKeyPropperte, ForeignKeyId, repository) in _helper.GetNavigationContext(entity))
            {
                if (ForeignKeyId != 0)
                {
                    var related = repository.GetById(ForeignKeyId);
                    if (related != null)
                    {
                        ForeignKeyPropperte?.SetValue(entity, ForeignKeyId);
                        navigationPropperte.SetValue(entity, related);

                        if (related is IEntity relatedEntity)
                            ProcessInclude(relatedEntity);
                    }
                }
            }
        }
        public void ProcessAddInclude<T>(T entity) where T : class, IEntity
        {
            if (_helper.IsRecursive(entity)) return;

            foreach (var (navigationPropperte, ForeignKeyPropperte, ForeignKeyId, repository) in _helper.GetNavigationContext(entity))
            {
                var related = navigationPropperte.GetValue(entity);

                if (related != null && related is IEntity relatedEntity)
                {
                    if (ForeignKeyId == 0 || relatedEntity.Id == 0)
                    {
                        ProcessAddInclude((dynamic)relatedEntity);

                        var savedRelated = (IEntity)repository.Add(relatedEntity);
                        ForeignKeyPropperte?.SetValue(entity, savedRelated.Id);
                        navigationPropperte.SetValue(entity, savedRelated);
                    }
                }
            }
        }
        public void ProcessUpdateInclude<T>(T entity) where T : class, IEntity
        {
            if (_helper.IsRecursive(entity)) return;

            foreach (var (navigationPropperte, ForeignKeyPropperte, ForeignKeyId, repository) in _helper.GetNavigationContext(entity))
            {
                var related = navigationPropperte.GetValue(entity);

                if (related != null && related is IEntity relatedEntity)
                {
                    if (relatedEntity.Id != 0)
                    {
                        ProcessUpdateInclude((dynamic)relatedEntity);
                        var updatedRelated = (IEntity)repository.Update(relatedEntity)!;
                        if (updatedRelated != null)
                        {
                            ForeignKeyPropperte?.SetValue(entity, updatedRelated.Id);
                            navigationPropperte.SetValue(entity, updatedRelated);
                        }
                    }
                    else if (ForeignKeyId == 0)
                    {
                        ProcessAddInclude((dynamic)relatedEntity);
                        var addedRelated = (IEntity)repository.Add(relatedEntity);
                        ForeignKeyPropperte?.SetValue(entity, addedRelated.Id);
                        navigationPropperte.SetValue(entity, addedRelated);
                    }
                }
            }
        }
        public IEnumerable<(Type EntityType, int Id)> ProcessDeleteInclude<T>(T? entity) where T : class, IEntity
        {
            if (entity == null) yield break;

            if (_helper.IsRecursive(entity)) yield break;

            foreach (var (navigationPropperte, _, _, _) in _helper.GetNavigationContext(entity))
            {
                var related = navigationPropperte.GetValue(entity);

                if (related != null && related is IEntity relatedEntity && relatedEntity.Id != 0)
                {
                    var deleteMethod = typeof(XmlRecursiveProcessor).GetMethod(nameof(ProcessDeleteInclude),
                        BindingFlags.NonPublic | BindingFlags.Instance)
                        ?.MakeGenericMethod(relatedEntity.GetType());

                    if (deleteMethod != null)
                    {
                        var results = deleteMethod.Invoke(this, new object[] { relatedEntity })
                            as IEnumerable<(Type, int)>;

                        if (results != null)
                        {
                            foreach (var item in results)
                                yield return item;
                        }
                    }
                }
            }
            yield return (entity.GetType(), entity.Id);
        }
    }
}