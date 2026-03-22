using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Playground.EFCore.XML
{
    public class XmlRepository<T> : IXmlRepository<T> where T : class, IEntity
    {
        private readonly string _filePath;
        private readonly XmlSerializer _serializer;
        public XmlRepository(string filePath)
        {
            _filePath = Path.GetFullPath(filePath);
            _serializer = new XmlSerializer(typeof(List<T>));

            if (!File.Exists(filePath))
                Save(new List<T>());
        }
        private List<T> Load()
        {
            using var reader = new StreamReader(_filePath);
            return _serializer.Deserialize(reader) as List<T> ?? new();
        }
        private void Save(List<T> data)
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            using var writer = new StreamWriter(_filePath);
            _serializer.Serialize(writer, data);
        }
        public List<T> GetAll() => Load();
        public T? GetById(int id) => Load().FirstOrDefault(e => e.Id == id);
        public T Add(T entity)
        {
            var data = Load();

            if (entity.Id == 0)
            {
                var maxId = data.Max(e => e.Id); 
                entity.Id = maxId + 1;
            }

            data.Add(entity);
            Save(data);
            return entity;
        }

        public T? Update(T entity)
        {
            var data = Load();

            if (entity.Id == 0)
                throw new ArgumentException("Entity must have a valid Id");

            var existing = data.FirstOrDefault(e => e.Id == entity.Id); 
            if (existing == null) return null;

            var index = data.IndexOf(existing);
            data[index] = entity;
            Save(data);
            return entity;
        }


        public bool Delete(int id)
        {
            var data = Load();
            var entity = data.FirstOrDefault(e => e.Id == id);
            if (entity == null) return false;
            data.Remove(entity);
            Save(data);
            return true;
        }
        object IXmlRepository.GetAll() => GetAll();
        object? IXmlRepository.GetById(int id) => GetById(id);
        object IXmlRepository.Add(object entity) => Add((T)entity);
        object? IXmlRepository.Update(object entity) => Update((T)entity);
    }
}
