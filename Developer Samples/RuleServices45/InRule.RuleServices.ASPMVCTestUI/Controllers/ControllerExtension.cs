using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using InRule.RuleServices.Common;
using InRule.RuleServices.Common.DataObjects;

namespace InRule.RuleServices.ASPMVCTestUI.Controllers
{
    public static class ControllerExtension
    {
        public static void PopulateParametersFromForm(this Controller controller, RuleExecutionRequest request, FormCollection form, string controlPrefix)
        {
            if (request == null || form == null)
            {
                throw new Exception("Cannot populate Rule Set Parameters.  Request is null.");
            }

            request.Parameters = GetParameterListFromForm(form, controlPrefix);
        }

        public static string PopulateRuleSetParameterQueryString(this Controller controller, FormCollection form, string controlPrefix)
        {
            if (form == null)
            {
                throw new Exception("Cannot populate Rule Set Parameters. Form was null.");
            }

            var parameters = GetParameterListFromForm(form, controlPrefix);
            var queryString = new StringBuilder();
            foreach (var parameter in parameters)
            {
                queryString.AppendFormat(Constants.QueryStringItemTemplate, parameter.Name, parameter.Value);
            }
            return queryString.ToString();
        }

        private static List<RuleExecutionParameter> GetParameterListFromForm(NameValueCollection form, string controlPrefix)
        {
            if (form == null)
            {
                throw new Exception("Cannot populate Rule Set Parameters. Form was null.");
            }

            var tempContainer = new List<RuleExecutionParameter>();
            foreach (var fieldName in form.AllKeys)
            {
                if (fieldName.StartsWith(controlPrefix + Literals.ParamNameKeyPart))
                {
                    var paramName = form[fieldName];
                    if (!String.IsNullOrWhiteSpace(paramName))
                    {
                        var parameter = new RuleExecutionParameter { Name = paramName.Trim() };
                        var suffixText = Regex.Match(fieldName, @"\d+$", RegexOptions.RightToLeft);
                        if (suffixText.Success)
                        {
                            int suffixCount;
                            if (int.TryParse(suffixText.Value, out suffixCount))
                            {
                                parameter.Value = form[controlPrefix + Literals.ParamValueKeyPart + suffixCount];
                            }
                        }
                        tempContainer.Add(parameter);
                    }
                }
            }
            return tempContainer;
        }

        public static void PopulateEndPointOverridesFromForm(this Controller controller, FormCollection form, RuleExecutionRequest request, string controlPrefix)
        {
            if (form == null || request == null)
            {
                throw new Exception("Cannot populate Rule Set Parameters. Form or Request was null.");
            }

            var tempContainer = new List<RuleExecutionEndPointOverride>();

            foreach (var fieldName in form.AllKeys)
            {
                if (fieldName.StartsWith(controlPrefix) && fieldName.EndsWith(Literals.EndPointOverrideKeyNamePart))
                {
                    var endPointName = form[fieldName];
                    if (!String.IsNullOrWhiteSpace(endPointName))
                    {
                        var endPointOverride = new RuleExecutionEndPointOverride { EndPointName = endPointName };
                        var paramContainer = new List<RuleExecutionEndPointProperty>();
                        var prefixText = Regex.Match(fieldName, "^" + controlPrefix + @"\d+", RegexOptions.RightToLeft);
                        foreach (var childFieldName in form.AllKeys)
                        {
                            if (childFieldName.StartsWith(prefixText + Literals.ParamNameKeyPart))
                            {
                                var paramName = form[childFieldName];
                                if (!String.IsNullOrWhiteSpace(paramName))
                                {
                                    var parameter = new RuleExecutionEndPointProperty { Name = paramName.Trim() };
                                    var suffixText = Regex.Match(childFieldName, @"\d+$", RegexOptions.RightToLeft);
                                    if (suffixText.Success)
                                    {
                                        int suffixCount;
                                        if (int.TryParse(suffixText.Value, out suffixCount))
                                        {
                                            parameter.Value = form[prefixText + Literals.ParamValueKeyPart + suffixCount];
                                        }
                                    }
                                    paramContainer.Add(parameter);
                                }
                            }
                        }
                        endPointOverride.Properties = paramContainer;
                        tempContainer.Add(endPointOverride);
                    }
                }
            }

            request.EndPointOverrides = tempContainer;
        }

        public static string DigestWebErrorBody(this Controller controller, WebException ex)
        {
            var errorResponse = String.Empty;
            if (ex.Response != null && ex.Response.ContentLength > 0)
            {
                using (var stream = ex.Response.GetResponseStream())
                {
                    if (stream != null)
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            errorResponse = reader.ReadToEnd().Trim();
                            if (!String.IsNullOrWhiteSpace(errorResponse) && errorResponse.ToLower().Contains("<body>"))
                            {
                                var startIndex = errorResponse.ToLower().IndexOf("<body>", StringComparison.Ordinal) + 6;
                                errorResponse = errorResponse.Substring(startIndex, errorResponse.IndexOf("</body>", StringComparison.Ordinal) - startIndex - 1);
                            }
                        }
                    }
                }
            }

            if (String.IsNullOrWhiteSpace(errorResponse))
            {
                errorResponse = ex.Message;
            }

            return errorResponse;
        }
    }
}
