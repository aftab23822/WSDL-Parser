using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Parsing_WSDL.WSDLParser
{
    public class ApiConstants
    {
        public const char Slash = '/';
        public const string POST = "POST";
        public const string PUT = "PUT";
        public const string DELETE = "DELETE";
        public const string GET = "GET";
        public const string GraphQLContentType = "GraphQL";
        public const string JsonContentType = "application/json";
        public const string RawContentType = "Raw";
        public const string MultipartContentType = "multipart/form-data";
        public const string JsonFormat = "application/json";
        public const string TextContentType = "application/text";
        public const string TextPlainContentType = "text/plain";
        public const string Tls12SecurityProtocol = "Tls12";
        public const string XmlFormat = "text/xml";
        public const string XmlContentType = "text/xml;charset=utf-8";
        public const string JavaScriptContentType = "application/javascript";
        public const string HTMLContentType = "text/html";
        public const string number = "number";
        public const string AllContentType = "*/*";
        public const string ApplicationFormContentType = "application/x-www-form-urlencoded";
        public const string ApplicationGraphQLContentType = "application/graphql";
    }
}
