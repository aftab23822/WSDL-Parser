using System.Web.Services.Description;

namespace Parsing_WSDL.WSDLParser
{
    public class ContentTypeHandler
    {
        public void SetInputOutputContentType(OperationWSDL operationWsdl, OperationBinding operationbinding)
        {
            SetInputAndContentType(operationWsdl, operationbinding);
            SetOutputAndContentType(operationWsdl, operationbinding);
        }
        private void SetInputAndContentType(OperationWSDL operationWsdl, OperationBinding operationbinding)
        {
            if (operationbinding.Input is null)
            {
                operationWsdl.SendInputBodyAs = OperationBody.None;
                return;
            }

            operationWsdl.InputContentType = ApiConstants.XmlContentType;
            SetInput(operationWsdl, operationbinding.Input);
        }

        private void SetOutputAndContentType(OperationWSDL operationWsdl, OperationBinding operationbinding)
        {
            if (operationbinding.Output is null)
            {
                operationWsdl.ResponseOutputAs = OperationBody.None;
                return;
            }
            SetOutput(operationWsdl, operationbinding.Output);
        }

        private void SetOutput(OperationWSDL operationWsdl, OutputBinding outputBinding)
        {
            operationWsdl.OutputContentType = ApiConstants.XmlContentType;
            operationWsdl.ResponseOutputAs = OperationBody.XmlBody;
            if (outputBinding.Extensions.Find(typeof(SoapBodyBinding)) != null)
                operationWsdl.ResponseOutputAs = OperationBody.XmlBody;
            else
                SetMimeContentBinding(outputBinding, operationWsdl);
        }

        private void SetMimeContentBinding(OutputBinding outputBinding, OperationWSDL operationWsdl)
        {
            if (outputBinding.Extensions.Find(typeof(MimeContentBinding)) is not MimeContentBinding mimeContentBinding)
                return;
            operationWsdl.OutputContentType = mimeContentBinding.Type;
        }



        private void SetInput(OperationWSDL operationWsdl, InputBinding inputBinding)
        {
            SoapBodyBinding soap = inputBinding.Extensions.Find(typeof(SoapBodyBinding)) as SoapBodyBinding;
            SoapHeaderBinding soapHeader = inputBinding.Extensions.Find(typeof(SoapHeaderBinding)) as SoapHeaderBinding;
            MimeContentBinding mimeContentBinding = inputBinding.Extensions.Find(typeof(MimeContentBinding)) as MimeContentBinding;

            if (soap != null)
                SetSoapBodyProperties(operationWsdl, soap);

            else if (soapHeader != null)
                operationWsdl.SendInputBodyAs = OperationBody.HeaderParameter;

            else if (mimeContentBinding != null)
                SetMimeContentProperties(operationWsdl, mimeContentBinding);

            else
                SetDefaultProperties(operationWsdl);
        }

        private void SetSoapBodyProperties(OperationWSDL operationWsdl, SoapBodyBinding soap)
        {
            operationWsdl.BodeRequired = soap.Required;
            operationWsdl.BodyEncoding = soap.Encoding;
            operationWsdl.BodyNamespace = soap.Namespace;
            operationWsdl.BodyUse = soap.Use;
            operationWsdl.SendInputBodyAs = OperationBody.XmlBody;
        }

        private void SetMimeContentProperties(OperationWSDL operationWsdl, MimeContentBinding mimeContentBinding)
        {
            operationWsdl.InputContentType = mimeContentBinding.Type;
            operationWsdl.SendInputBodyAs = OperationBody.XmlBody;
        }

        private void SetDefaultProperties(OperationWSDL operationWsdl)
        {
            if (operationWsdl.HttpVerb?.ToLower() == "get")
                operationWsdl.SendInputBodyAs = OperationBody.QueryParameter;
            else
                operationWsdl.SendInputBodyAs = OperationBody.UrlEncodedParametersBody;
        }

    }
}
