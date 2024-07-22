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
using System.Linq.Expressions;
using System.Linq;
using System.Security.Cryptography;
using System.Collections;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Linq;

namespace OdooXmlRpcLibrary
{

    [XmlRpcUrl("http://localhost:8010/xmlrpc/2/common")]
    public interface IOdooComRpcClient : IXmlRpcProxy
    {
        [XmlRpcMethod("authenticate")]
        int Authenticate(string db, string username, string password, object[] args);
    }

    [XmlRpcUrl("http://localhost:8010/xmlrpc/2/object")]
    public interface IOdooObjRpcClient : IXmlRpcProxy
    {

        [XmlRpcMethod("execute_kw")]
        int Crea(string db, long? uid, string password, string model, string method, object[] args, dynamic kwrgs);

        [XmlRpcMethod("execute_kw")]
        dynamic Cust(string db, long? uid, string password, string model, string method, object[] args);

    }

    public class OdooXmlRpcMain
    {
        private OdooConnectionInfo OdooConnection;
        private IOdooRpcClient OdooRpcClient;

        public OdooXmlRpcMain(string host, int port, string database, string username, string password)
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

        public async Task<long> CreateRFQ(Dictionary<string, object> data)
        {
            try
            {
                var id = await this.OdooRpcClient.Create<dynamic>("purchase.order", data);

                if (id == null)
                {
                    Console.WriteLine("Unable to create Request for quote.");
                    return -1;
                }
                Console.WriteLine("Request for Quotes created with ID: {0}", id);

                return id;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating RFQ in Odoo: {0}", ex.Message);
                return -1;
            }
        }
        //===========================Confirm RFQ to update state to purchase order=============================//

        public async Task<bool> ConfirmRfq(long invoiceId)
        {
            try
            {
                var client = XmlRpcProxyGen.Create<IOdooObjRpcClient>();

                client.Url = $"http://{this.OdooConnection.Host}:{this.OdooConnection.Port}/xmlrpc/2/object";


                var result = client.Cust(this.OdooConnection.Database, this.OdooRpcClient.SessionInfo.UserId, this.OdooConnection.Password, "purchase.order", "button_confirm", new object[] { invoiceId });

                if (result != null)
                {
                    Console.WriteLine("Quotation with ID {0} confirm successfully.", invoiceId);
                    return true;
                }
                else
                {
                    Console.WriteLine("Failed to confirm invoice with ID {0}.", invoiceId);
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking or posting invoice with ID {0}: {1}", invoiceId, ex.Message);
                return false;
            }
        }

        //===========================Receive Product=================================//
        public async Task<bool> ReceiveProduct(long invoiceId)
        {
            try
            {
                var client = XmlRpcProxyGen.Create<IOdooObjRpcClient>();

                client.Url = $"http://{this.OdooConnection.Host}:{this.OdooConnection.Port}/xmlrpc/2/object";


                var result = client.Cust(this.OdooConnection.Database, this.OdooRpcClient.SessionInfo.UserId, this.OdooConnection.Password, "purchase.order", "action_view_picking", new object[] { invoiceId });

                if (result != null)
                {
                    Console.WriteLine("Quotation with ID {0} change state successfully.", invoiceId);
                    return true;
                }
                else
                {
                    Console.WriteLine("Failed to change state of quotation with ID {0}.", invoiceId);
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking or posting invoice with ID {0}: {1}", invoiceId, ex.Message);
                return false;
            }
        }

        //==========================================Validate==========================================//

        public async Task<bool> ValidateProduct(long invoiceId)
        {
            try
            {
                var client = XmlRpcProxyGen.Create<IOdooObjRpcClient>();

                client.Url = $"http://{this.OdooConnection.Host}:{this.OdooConnection.Port}/xmlrpc/2/object";


                var result = client.Cust(this.OdooConnection.Database, this.OdooRpcClient.SessionInfo.UserId, this.OdooConnection.Password, "stock.picking", "button_validate", new object[] { invoiceId });

                if (result != null)
                {
                    Console.WriteLine("Quotation with ID {0} change state successfully.", invoiceId);
                    return true;
                }
                else
                {
                    Console.WriteLine("Failed to change state of quotation with ID {0}.", invoiceId);
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking or posting invoice with ID {0}: {1}", invoiceId, ex.Message);
                return false;
            }
        }

        //=============================Conform after create bill=================================//

        public async Task<bool> ConformBill(long invoiceId)
        {
            try
            {
                var client = XmlRpcProxyGen.Create<IOdooObjRpcClient>();

                client.Url = $"http://{this.OdooConnection.Host}:{this.OdooConnection.Port}/xmlrpc/2/object";


                var result = client.Cust(this.OdooConnection.Database, this.OdooRpcClient.SessionInfo.UserId, this.OdooConnection.Password, "account.move", "action_post", new object[] { invoiceId });

                if (result != null)
                {
                    Console.WriteLine("Quotation with ID {0} change state successfully.", invoiceId);
                    return true;
                }
                else
                {
                    Console.WriteLine("Failed to change state of quotation with ID {0}.", invoiceId);
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking or posting invoice with ID {0}: {1}", invoiceId, ex.Message);
                return false;
            }
        }


        //================================Create bill================================//

        public async Task<bool> CreateBill(long invoiceId)
        {
            try
            {
                var client = XmlRpcProxyGen.Create<IOdooObjRpcClient>();

                client.Url = $"http://{this.OdooConnection.Host}:{this.OdooConnection.Port}/xmlrpc/2/object";


                var result = client.Cust(this.OdooConnection.Database, this.OdooRpcClient.SessionInfo.UserId, this.OdooConnection.Password, "purchase.order", "action_create_invoice", new object[] { invoiceId });

                if (result != null)
                {
                    Console.WriteLine("Quotation with ID {0} creating bill successfully.", invoiceId);
                    return true;
                }
                else
                {
                    Console.WriteLine("Failed to create bill invoice with ID {0}.", invoiceId);
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking or posting invoice with ID {0}: {1}", invoiceId, ex.Message);
                return false;
            }
        }

        //============================receive bill payment================================//

        /*
        public async Task<bool> ReceivePayment(long invoiceId, long journalId, long companyId)
        {
            try
            {
                var client = XmlRpcProxyGen.Create<IOdooObjectRpcClient>();

                client.Url = $"http://{this.OdooConnection.Host}:{this.OdooConnection.Port}/xmlrpc/2/object";


                var result = client.Custom(this.OdooConnection.Database, this.OdooRpcClient.SessionInfo.UserId, this.OdooConnection.Password, "account.move", "action_register_payment", new object[] { invoiceId });

                if (result != null)
                {
                    Console.WriteLine("Quotation with ID {0} creating bill successfully.", invoiceId);
                    return true;
                }
                else
                {
                    Console.WriteLine("Failed to create bill invoice with ID {0}.", invoiceId);
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking or posting invoice with ID {0}: {1}", invoiceId, ex.Message);
                return false;
            }
        }
        */


        //==================================Sales Orders==============================//

        public async Task<long> CreateQuotation(Dictionary<string, object> data)
        {
            try
            {
                var id = await this.OdooRpcClient.Create<dynamic>("sale.order", data);

                if (id == null)
                {
                    Console.WriteLine("Unable to create Request for quote.");
                    return -1;
                }
                Console.WriteLine("Request for Quotes created with ID: {0}", id);

                return id;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating RFQ in Odoo: {0}", ex.Message);
                return -1;
            }
        }

        //======================================Change state of Quotation to sales order=========================================//

        public async Task<bool> ReceivePayment(long invoiceId, long journalId, long companyId)
        {
            try
            {

                var client = XmlRpcProxyGen.Create<IOdooObjRpcClient>();

                client.Url = $"http://{this.OdooConnection.Host}:{this.OdooConnection.Port}/xmlrpc/2/object";

                Console.WriteLine("this is =============3");

                var vals = new
                {
                    journal_id = journalId,
                    company_id = companyId,
                };
                Console.WriteLine("This is =============1");
                var context = new
                {
                    context = new
                    {
                        active_ids = new object[] { invoiceId },
                        active_model = "account.move"
                    }
                };
                Console.WriteLine("this is ===============2");

                var res = client.Crea(this.OdooConnection.Database, this.OdooRpcClient.SessionInfo.UserId, this.OdooConnection.Password, "action.register.payment", "create", new object[] { vals }, context);
                Console.WriteLine(res);

                Console.WriteLine("this is ==========4");

                var result = client.Cust(this.OdooConnection.Database, this.OdooRpcClient.SessionInfo.UserId, this.OdooConnection.Password, "account.payment.register", "action_create_payments", new object[] { res });

                Console.WriteLine("This is ========5");

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

        public async Task<bool> ChangeQuotationState(long invoiceId)
        {
            try
            {
                var client = XmlRpcProxyGen.Create<IOdooObjRpcClient>();

                client.Url = $"http://{this.OdooConnection.Host}:{this.OdooConnection.Port}/xmlrpc/2/object";


                var result = client.Cust(this.OdooConnection.Database, this.OdooRpcClient.SessionInfo.UserId, this.OdooConnection.Password, "sale.order", "action_confirm", new object[] { invoiceId });

                if (result != null)
                {
                    Console.WriteLine("Quotation with ID {0} change state successfully.", invoiceId);
                    return true;
                }
                else
                {
                    Console.WriteLine("Failed to change state of quotation with ID {0}.", invoiceId);
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking or posting invoice with ID {0}: {1}", invoiceId, ex.Message);
                return false;
            }
        }

        //===========================create invoice for sale order======================================//
        public async Task<bool> CreateSaleOrderInvoice(long invoiceId)
        {
            try
            {
                var client = XmlRpcProxyGen.Create<IOdooObjRpcClient>();

                client.Url = $"http://{this.OdooConnection.Host}:{this.OdooConnection.Port}/xmlrpc/2/object";


                var result = client.Cust(this.OdooConnection.Database, this.OdooRpcClient.SessionInfo.UserId, this.OdooConnection.Password, "sale.order", "action_invoice_create", new object[] { invoiceId });

                if (result != null)
                {
                    Console.WriteLine("Sale order invoice with ID {0} created successfully.", invoiceId);
                    return true;
                }
                else
                {
                    Console.WriteLine("Failed to create Sale order invoice quotation with ID {0}.", invoiceId);
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking or posting invoice with ID {0}: {1}", invoiceId, ex.Message);
                return false;
            }
        }
    }
}