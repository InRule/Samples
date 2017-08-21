using System;
using System.Net;
using System.Web.Mvc;
using InRule.RuleServices.Common.Connectivity;
using InRule.RuleServices.Common.DataObjects;

namespace InRule.RuleServices.ASPMVCTestUI.Controllers
{
    [ValidateInput(false)]
    [HandleError]
    public class RuleExecutionController : Controller
    {
        public ActionResult ExecuteRuleRequest()
        {
            return View(new RuleExecutionRequest());
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ExecuteRuleRequest(RuleExecutionRequest request, FormCollection form)
        {
            try
            {
                // assign ruleset parameters from form fields
                this.PopulateParametersFromForm(request, form, Literals.RuleSetParamPrefix);
                this.PopulateEndPointOverridesFromForm(form, request, Literals.EndPointOverridePrefix);

                var client = new RuleClient();
                var response = client.ExecuteRuleRequest(request);

                if (!String.IsNullOrWhiteSpace(response.Error))
                {
                    return PartialView("_AjaxError", response.Error);
                }
                return PartialView("_ExecuteRuleResponse", response);
            }
            catch (WebException ex)
            {
                return PartialView("_AjaxError", this.DigestWebErrorBody(ex));
            }
            catch (Exception ex)
            {
                return PartialView("_AjaxError", ex.ToString());
            }
        }

        public ActionResult ApplyRules()
        {
            return View(new RuleExecutionRequest());
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ApplyRules(FormCollection form)
        {
            try
            {
                var client = new RuleClient();
                var results = client.GetApplyRulesResponse(form["RuleApp"], form["Entity"], form["EntityXml"], form["ReturnEntity"], form["ResponseType"]);
                return PartialView("_RulesGetResponse", results);
            }
            catch (WebException ex)
            {
                return PartialView("_AjaxError", this.DigestWebErrorBody(ex));
            }
            catch (Exception ex)
            {
                return PartialView("_AjaxError", ex.ToString());
            }
        }

        public ActionResult ExecuteRuleSet()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ExecuteRuleSet(FormCollection form)
        {
            try
            {
                var parameters = this.PopulateRuleSetParameterQueryString(form, Literals.RuleSetParamPrefix);
                var client = new RuleClient();
                var results = client.GetExecuteRuleSetResponse(form["RuleApp"], form["RuleSet"], form["Entity"], form["EntityXml"], form["ReturnEntity"], form["ResponseType"], parameters);
                return PartialView("_RulesGetResponse", results);
            }
            catch (WebException ex)
            {
                return PartialView("_AjaxError", this.DigestWebErrorBody(ex));
            }
            catch (Exception ex)
            {
                return PartialView("_AjaxError", ex.ToString());
            }
        }
    }
}
