using GenericODataAPI.Controllers;
using GenericOData.Core.Services.Util;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GenericODataAPI.ControllerFactory
{
    public class GenericODataControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            var model_types = DomainObjectUtil.GetAllDomainObjectsTypeInfo();
            foreach (var model_type in model_types)
            {
                Type[] typeArgs = { model_type };
                var controller_type = typeof(GenericOdataController<>).MakeGenericType(typeArgs).GetTypeInfo();
                feature.Controllers.Add(controller_type);
            }
        }
    }
}
