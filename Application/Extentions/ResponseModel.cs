using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Extentions
{
    public class ResponseModel<T>
    {
        public string Message { get; }
        public HttpStatusCode StatusCode { get; }
        public T result { get; }

        public ResponseModel(T value)
        {
            result = value;
        }

        public ResponseModel(string message, HttpStatusCode statusCode)
        {
            Message = message;
            StatusCode = statusCode;
        }

        public ResponseModel(string message, HttpStatusCode statusCode,T value)
        {
            Message = message;
            StatusCode = statusCode;
            result = value;
        }
    }
}
