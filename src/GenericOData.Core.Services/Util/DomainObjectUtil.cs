using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GenericOData.Core.Services.Util
{
    public static class DomainObjectUtil
    {
        public static HashSet<TypeInfo> GetAllDomainObjectsTypeInfo()
        {
            var domainObjectTypes = GetAllDomainObjectTypes();

            return new HashSet<TypeInfo>(domainObjectTypes.Select(x => x.GetTypeInfo()));
        }


        public static IEnumerable<Type> GetAllDomainObjectTypes()
        {
            var entityTypes = AppDomain.CurrentDomain.GetAssemblies()
                          .SelectMany(t => t.GetTypes())
                          .Where(t => t != null && t.Namespace != null && t.IsClass && t.Namespace.Contains("Entities.DomainObjects") 
                          && !t.Namespace.Contains(".InternalClass") && !t.Name.EndsWith("BaseEntity"));

            return entityTypes;
        }
    }
}