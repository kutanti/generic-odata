using GenericOData.Core.Services.Helper.Interface;
using Microsoft.OData.Edm;
using System;

namespace GenericOData.Core.Services.Helper
{
    public class EdmModelBuilder : IEdmModelBuilder
    {
        private const string DEFAULTNAMESPACE = "Entities.DomainObjects";

        /// <inheritdoc/>
        public (IEdmModel, IEdmEntityType, IEdmEntitySet) BuildTableModel(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            var model = new EdmModel();
            var entityType = new EdmEntityType(DEFAULTNAMESPACE, tableName, null, false, true);
            model.AddElement(entityType);

            var defaultContainer = new EdmEntityContainer(DEFAULTNAMESPACE, "DefaultContainer");
            model.AddElement(defaultContainer);
            var entitySet = defaultContainer.AddEntitySet(tableName, entityType);

            return (model, entityType, entitySet);
        }
    }
}
