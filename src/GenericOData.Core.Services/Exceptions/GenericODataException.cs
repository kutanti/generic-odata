using System.Net;

namespace GenericOData.Core.Services.Exceptions
{
    public class GenericODataException : HttpRequestException 
    {

        public GenericODataException(string message, HttpStatusCode statusCode) : base(message, null, statusCode) 
        {

        }
    }
}
