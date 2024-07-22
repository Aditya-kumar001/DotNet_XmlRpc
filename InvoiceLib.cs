using System;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using OdooRpc.CoreCLR.Client.Interfaces;
using OdooRpc.CoreCLR.Client.Models;
using OdooRpc.CoreCLR.Client;
using OdooRpc.CoreCLR.Client.Models.Parameters;
using System.ComponentModel;
using CookComputing.XmlRpc;
using System.Collections.Generic;

namespace OdooXmlRpcLibrary
{

    [XmlRpcUrl("http://localhost:8010/xmlrpc/2/common")]
    public interface IOdooCommonRpcClient : IXmlRpcProxy
    {
        [XmlRpcMethod("authenticate")]
        int Authenticate(string db, string username, string password, object[] args);
    }

    [XmlRpcUrl("http://localhost:8010/xmlrpc/2/object")]
    public interface IOdooObjectRpcClient : IXmlRpcProxy
    {
        [XmlRpcMethod("execute_kw")]
        dynamic Custom(string db, long? uid, string password, string model, string method, object[] args);

        [XmlRpcMethod("execute_kw")]
        int Create(string db, long? uid, string password, string model, string method, object[] args, dynamic kwrgs);

    }
    public class OdooXmlRpcInvoiceMain
    {
        private OdooConnectionInfo OdooConnection;
        private IOdooRpcClient OdooRpcClient;

        public OdooXmlRpcInvoiceMain(string host, int port, string database, string username, string password)
        {
            string jsonString = $@"{{
            'OdooConnection': {{
                'Host': '{host}',
                'Port': {port},
                'Database': '{database}',
                'Username': '{username}',
                'Password': '{password}'
            }}
        }}";
            var settings = JsonConvert.DeserializeObject<JObject>(jsonString);
            this.OdooConnection = settings["OdooConnection"].ToObject<OdooConnectionInfo>();

        }

        public async Task<bool> LoginToOdoo()
        {
            try
            {
                this.OdooRpcClient = new OdooRpcClient(this.OdooConnection);

                var odooVersion = await this.OdooRpcClient.GetOdooVersion();

                Console.WriteLine("Odoo Version: {0} - {1}", odooVersion.ServerVersion, odooVersion.ProtocolVersion);

                await this.OdooRpcClient.Authenticate();

                if (this.OdooRpcClient.SessionInfo.IsLoggedIn)
                {
                    Console.WriteLine("Login successful => User Id: {0}", this.OdooRpcClient.SessionInfo.UserId);
                    return true;
                }
                else
                {
                    Console.WriteLine("Login failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting to Odoo: {0}", ex.Message);
                return false;
            }
        }



        //=======================================Ref-Code for Creating an Invoice======================================//
        public async Task<long> CreateInvoice(Dictionary<string, object> data)
        {
            try
            {
                if (data["name"] == null)
                {
                    throw new Exception("No name passed. need name to check this invoice is already exixt or not!");
                }
                // check if exist
                var reqParams = new OdooSearchParameters(
                    "account.move",
                    new OdooDomainFilter().Filter("name", "like", data["name"])
                );

                var invoices = await this.OdooRpcClient.Search<long[]>(reqParams, new OdooPaginationParameters(0, 1));

                if (invoices.Length != 0)
                {
                    Console.WriteLine("Invoice is already exist with name: {0}", data["name"]);
                    return -1;
                }


                var id = await this.OdooRpcClient.Create<dynamic>("account.move", data);

                if (id == null)
                {
                    Console.WriteLine("Unable to create invoice", data["name"]);
                    return -1;

                }

                Console.WriteLine("Invoice created with ID: {0}", id);

                bool posted = await this.PostInvoiceToPost(id);

                return id;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating invoice in Odoo: {0}", ex.Message);
                return -1;
            }
        }


        public async Task<bool> PostInvoiceToPost(long invoiceId)
        {
            try
            {
                var client = XmlRpcProxyGen.Create<IOdooObjectRpcClient>();

                client.Url = $"http://{this.OdooConnection.Host}:{this.OdooConnection.Port}/xmlrpc/2/object";

                var result = client.Custom(this.OdooConnection.Database, this.OdooRpcClient.SessionInfo.UserId, this.OdooConnection.Password, "account.move", "action_post", new object[] { invoiceId });

                if (result != null)
                {
                    Console.WriteLine("Invoice with ID {0} posted successfully.", invoiceId);
                    return true;
                }
                else
                {
                    Console.WriteLine("Failed to post invoice with ID {0}.", invoiceId);
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking or posting invoice with ID {0}: {1}", invoiceId, ex.Message);
                return false;
            }
        }





        public async Task<bool> CheckInvoicePaidStatus(long invoiceId)
        {
            try
            {
                // check if exist
                var reqParams = new OdooSearchParameters(
                    "account.move",
                    new OdooDomainFilter().Filter("id", "=", invoiceId)
                );
                var fieldParams = new OdooFieldParameters();
                fieldParams.Add("payment_state");

                var invoice = await this.OdooRpcClient.Get<JArray>(reqParams, fieldParams);

                if (invoice == null)
                {
                    return false;
                }
                string state = invoice[0]["payment_state"].ToObject<string>();
                if (state == "paid")
                {
                    Console.WriteLine("Invoice ID {0} is {1}.", invoiceId, state);
                    return true;
                }
                else
                {
                    Console.WriteLine("Invoice ID {0} is {1}.", invoiceId, state);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking paid status of Invoice ID {0}: {1}", invoiceId, ex.Message);
                return false;
            }
        }

        public async Task<bool> ApplyPayment(long invoiceId, long journalId, long companyId)
        {
            try
            {

                var isPaid = await CheckInvoicePaidStatus(invoiceId);
                if (isPaid)
                {
                    Console.WriteLine("Invoice ID {0} is already paid. No further payments can be applied.", invoiceId);
                    return false;
                }


                var client = XmlRpcProxyGen.Create<IOdooObjectRpcClient>();

                client.Url = $"http://{this.OdooConnection.Host}:{this.OdooConnection.Port}/xmlrpc/2/object";


                var vals = new
                {
                    journal_id = journalId, // bank journal id
                    company_id = companyId // company id
                };
                var context = new
                {
                    context = new
                    {
                        active_ids = new object[] { invoiceId },
                        active_model = "account.move"
                    }
                };

                var res = client.Create(this.OdooConnection.Database, this.OdooRpcClient.SessionInfo.UserId, this.OdooConnection.Password, "account.payment.register", "create", new object[] { vals }, context);
                Console.WriteLine(res);

                var result = client.Custom(this.OdooConnection.Database, this.OdooRpcClient.SessionInfo.UserId, this.OdooConnection.Password, "account.payment.register", "action_create_payments", new object[] { res });
                if (result != null)
                {
                    Console.WriteLine("Payment applied to Invoice ID {0}.", invoiceId);

                    return true;
                }
                else
                {
                    Console.WriteLine("Failed to apply payment to Invoice ID {0}.", invoiceId);
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error applying payment to Invoice ID {0}: {1}", invoiceId, ex.Message);
                return false;
            }
        }


        private async Task CheckAndSetInvoicePaid(long invoiceId)
        {
            try
            {

                var fieldParams = new OdooFieldParameters();
                fieldParams.Add("amount_total");
                fieldParams.Add("amount_residual");

                var invoice = await OdooRpcClient.GetAll<JObject>("account.move", fieldParams, new OdooPaginationParameters().OrderByDescending("name"));

                decimal totalAmount = invoice["amount_total"].ToObject<decimal>();
                decimal residualAmount = invoice["amount_residual"].ToObject<decimal>();

                if (residualAmount <= 0 && totalAmount > 0)
                {

                    await UpdateInvoiceField(invoiceId, "state", "paid");
                    Console.WriteLine("Invoice ID {0} marked as paid.", invoiceId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking and setting invoice paid status for Invoice ID {0}: {1}", invoiceId, ex.Message);
            }

        }


        public async Task<bool> UpdateInvoiceField(long invoiceId, string fieldName, object newValue)
        {
            try
            {
                dynamic fieldsToUpdate = new object();
                fieldsToUpdate[fieldName] = newValue;

                var result = await OdooRpcClient.Update<dynamic>("account.move", invoiceId, fieldsToUpdate);

                if (result)
                {
                    Console.WriteLine("Invoice field '{0}' updated successfully.", fieldName);
                    return result;
                }
                else
                {
                    Console.WriteLine("Failed to update invoice field '{0}'.", fieldName);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating invoice field '{0}': {1}", fieldName, ex.Message);
                return false;
            }
        }

    }
}