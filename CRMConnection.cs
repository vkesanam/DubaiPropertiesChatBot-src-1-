using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace CRMPropertyChatbot
{
    public class CRMConnection
    {
        internal static void CreateLeadReg(string customerName, string email)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

                CrmServiceClient crmConn = new CrmServiceClient("venkata@intertecd365forops.onmicrosoft.com", CrmServiceClient.MakeSecureString("Welcome@123"), "EMEA", "org61bce9a2", useUniqueInstance: false, useSsl: true, isOffice365: true);
                IOrganizationService service = crmConn.OrganizationServiceProxy;

                Entity Lead = new Entity("lead");
                Lead["subject"] = "Property Request from - " + customerName;
                Lead["lastname"] = customerName;
                Lead["emailaddress1"] = email;
                Guid LeadGuid = service.Create(Lead);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static void CreateCase(string complaint,string customerName, string phone, string email)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

                CrmServiceClient crmConn = new CrmServiceClient("venkata@intertecd365forops.onmicrosoft.com", CrmServiceClient.MakeSecureString("Welcome@123"), "EMEA", "org61bce9a2", useUniqueInstance: false, useSsl: true, isOffice365: true);
                IOrganizationService service = crmConn.OrganizationServiceProxy;

                Microsoft.Xrm.Sdk.Entity Case = new Microsoft.Xrm.Sdk.Entity("incident");
                Case["title"] = complaint;

                Microsoft.Xrm.Sdk.Entity Account = new Microsoft.Xrm.Sdk.Entity("account");
                Account["customerName"] = customerName;
                Account["telephone1"] = phone;
                Account["emailaddress1"] = email;
                Guid AccountId = service.Create(Account);

                Case["customerid"] = new EntityReference("account", AccountId);
                Guid CaseId = service.Create(Case);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}