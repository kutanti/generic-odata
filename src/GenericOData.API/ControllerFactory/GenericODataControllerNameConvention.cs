using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;

using GenericODataAPI.Controllers;

namespace GenericODataAPI.ControllerFactory
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class GenericODataControllerNameConvention : Attribute, IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (!controller.ControllerType.IsGenericType || controller.ControllerType.GetGenericTypeDefinition() != typeof(GenericOdataController<>))
            {
                return;
            }
            var entityType = controller.ControllerType.GenericTypeArguments[0];

            string extractedDatasetFullName = entityType.FullName.Replace("Entities.DomainObjects.", "");

            string[] nameParts = extractedDatasetFullName.Split('.');
            controller.ControllerName = entityType.Name;
            controller.RouteValues["version"] = nameParts[1].ToLowerInvariant();
            controller.RouteValues["dataBase"] = nameParts[2].ToLowerInvariant();
            controller.RouteValues["entity"] = entityType.Name.ToLowerInvariant();
        }
    }
}