using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Playground.EFCore.XML
{
    [AttributeUsage(AttributeTargets.Property)]
    public class XmlForeignKeyAttribute : XmlIgnoreAttribute
    {
        public string ForeignKeyProperty { get; }
        public XmlForeignKeyAttribute(string foreignKeyProperty) => 
            ForeignKeyProperty = foreignKeyProperty;
    }
}
