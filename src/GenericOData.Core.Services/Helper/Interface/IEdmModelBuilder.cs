using Microsoft.OData.Edm;

namespace GenericOData.Core.Services.Helper.Interface
{
    public interface IEdmModelBuilder
    {
        /// <summary>
        /// BuildTableModel.
        /// </summary>
        /// <param name="tableName">tableName.</param>
        /// <returns>Tuple.</returns>
        (IEdmModel, IEdmEntityType, IEdmEntitySet) BuildTableModel(string tableName);
    }
}
