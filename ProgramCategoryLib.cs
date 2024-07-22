using CookComputing.XmlRpc;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OdooXmlRpcLibrary
{
    public class OdooXmlRpcClient
    {
        private readonly string _url;
        private readonly string _db;
        private readonly string _username;
        private readonly string _password;
        private readonly int _uid;

        public OdooXmlRpcClient(string url, string db, string username, string password)
        {
            _url = url;
            _db = db;
            _username = username;
            _password = password;

            // Authenticate and get UID
            _uid = Authenticate();
        }

        private int Authenticate()
        {
            var client = XmlRpcProxyGen.Create<IOdooCommonRpcClient>();
            //updated a line of code here client.url...
            client.Url = $"{_url}/xmlrpc/2/common";
            var uid = client.Authenticate(_db, _username, _password, new object[] { });
            return uid;
        }

        public int CreateCategory(Dictionary<string, object> fields)
        {
            var client = XmlRpcProxyGen.Create<IOdooObjectRpcClient>();
            //updated a line of code here client.url...
            client.Url = $"{_url}/xmlrpc/2/object";

            var xmlRpcStruct = new XmlRpcStruct();
            foreach (var field in fields)
            {
                xmlRpcStruct.Add(field.Key, field.Value);
            }

            var categoryId = client.Create(_db, _uid, _password, "product.template", "create", new object[]
            {
                xmlRpcStruct
            });
            return categoryId;
        }

        public int[] CreateCategories(Dictionary<string, object>[] fieldSets)
        {
            var client = XmlRpcProxyGen.Create<IOdooObjectRpcClient>();
            //Update a line of code here...
            client.Url = $"{_url}/xmlrpc/2/object";

            var categoryIds = new int[fieldSets.Length];

            for (int i = 0; i < fieldSets.Length; i++)
            {
                var xmlRpcStruct = new XmlRpcStruct();
                foreach (var field in fieldSets[i])
                {
                    xmlRpcStruct.Add(field.Key, field.Value);
                }

                categoryIds[i] = client.Create(_db, _uid, _password, "product.template", "create", new object[]
                {
                    xmlRpcStruct
                });
            }
            return categoryIds;
        }

        public bool UpdateCategory(int categoryId, Dictionary<string, object> fields)
        {
            var client = XmlRpcProxyGen.Create<IOdooObjectRpcClient>();
            //Update a line of code here...
            client.Url = $"{_url}/xmlrpc/2/object";

            var xmlRpcStruct = new XmlRpcStruct();
            foreach (var field in fields)
            {
                xmlRpcStruct.Add(field.Key, field.Value);
            }

            var result = client.Update(_db, _uid, _password, "product.template", "write", new object[]
            {
                new int[] { categoryId },
                xmlRpcStruct
            });
            return result;
        }

        public bool DeleteCategory(int categoryId)
        {
            var client = XmlRpcProxyGen.Create<IOdooObjectRpcClient>();
            //Updated a line of code here...
            client.Url = $"{_url}/xmlrpc/2/object";

            var result = client.Delete(_db, _uid, _password, "product.template", "unlink", new object[]
            {
                new int[] { categoryId }
            });
            return result;
        }

        public Dictionary<string, object> GetCategoryById(int categoryId, bool includeChildCategories = false)
        {
            try
            {
                var client = XmlRpcProxyGen.Create<IOdooObjectRpcClient>();
                var fields = new object[] { "id" }; // Removed "name" field

                var filterDomain = new List<object> { new object[] { "id", "=", categoryId } };

                var categoryResponse = client.SearchRead(_db, _uid, _password, "product.category", "search_read", new object[]
                {
            filterDomain.ToArray(),
            fields
                });

                // Log the categoryResponse received from Odoo for debugging
                Console.WriteLine($"Received categoryResponse: {Newtonsoft.Json.JsonConvert.SerializeObject(categoryResponse)}");

                if (categoryResponse != null && categoryResponse.Length > 0)
                {
                    var categoryDetails = ParseCategoryResponse(categoryResponse[0]);

                    if (includeChildCategories)
                    {
                        var childCategories = GetChildCategories(categoryId);
                        categoryDetails.Add("child_categories", childCategories);
                    }

                    return categoryDetails;
                }
                else
                {
                    Console.WriteLine($"Category with ID {categoryId} not found.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching category {categoryId} details: {ex.Message}");
                return null; // Return null or throw an exception as needed
            }
        }


        private Dictionary<string, object> ParseCategoryResponse(object categoryResponse)
        {
            var categoryDetails = new Dictionary<string, object>();
            var responseItem = categoryResponse as IDictionary<string, object>;

            if (responseItem != null)
            {
                foreach (var entry in responseItem)
                {
                    categoryDetails.Add(entry.Key, entry.Value);
                }
            }

            return categoryDetails;
        }

        private Dictionary<string, object>[] GetChildCategories(int categoryId)
        {
            try
            {
                var client = XmlRpcProxyGen.Create<IOdooObjectRpcClient>();
                var fields = new object[] { "id", "name" }; // Add more fields as needed

                var filterDomain = new List<object> { new object[] { "parent_id", "=", categoryId } };

                var categoryResponse = client.SearchRead(_db, _uid, _password, "product.category", "search_read", new object[]
                {
                    filterDomain.ToArray(),
                    fields
                });

                if (categoryResponse != null && categoryResponse.Length > 0)
                {
                    var childCategories = new List<Dictionary<string, object>>();
                    foreach (var item in categoryResponse)
                    {
                        childCategories.Add(ParseCategoryResponse(item));
                    }
                    return childCategories.ToArray();
                }
                else
                {
                    Console.WriteLine($"No child categories found for category with ID {categoryId}.");
                    return new Dictionary<string, object>[0];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching child categories for category ID {categoryId}: {ex.Message}");
                return null; // Return null or throw an exception as needed
            }
        }

        public object[] GetCategoriesByParentName(int parentName)
        {
            try
            {
                var client = XmlRpcProxyGen.Create<IOdooObjectRpcClient>();
                var parentFields = new object[] { "name" }; // Add more fields as needed // removed the name field here

                var parentFilterDomain = new List<object> { new object[] { "name", "=", parentName } };

                var categoryResponse = client.SearchRead(_db, _uid, _password, "product.category", "search_read", new object[]
                {
                    parentFilterDomain.ToArray(),
                    parentFields
                });

                return categoryResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching categories by parent ID {parentName}: {ex.Message}");
                return null; // Return null or throw an exception as needed
            }
        }

        public object[] GetMasterCategories(bool isMaster)
        {
            try
            {
                var client = XmlRpcProxyGen.Create<IOdooObjectRpcClient>();
                var fields = new object[] { "id", "name" }; // Add more fields as needed

                var filterDomain = new List<object> { new object[] { "is_master", "=", isMaster } };

                var categoryResponse = client.Read(_db, _uid, _password, "product.category", "search_read", new object[]
                {
            filterDomain.ToArray(),
            fields
                });

                return categoryResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching master categories: {ex.Message}");
                return null; // Return null or throw an exception as needed
            }
        }

        public object[] GetCategoriesByUpdatedDateRange(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var client = XmlRpcProxyGen.Create<IOdooObjectRpcClient>();
                var fields = new object[] { "id", "name" }; // Add more fields as needed

                List<object> filterDomain = new List<object>();

                if (fromDate.HasValue)
                {
                    filterDomain.Add(new object[] { "write_date", ">=", fromDate.Value.ToString("dd-MM-yyyy 00:00:00") });
                }

                if (toDate.HasValue)
                {
                    filterDomain.Add(new object[] { "write_date", "<=", toDate.Value.ToString("dd-MM-yyyy 23:59:59") });
                }

                var categoryResponse = client.Read(_db, _uid, _password, "product.category", "search_read", new object[]
                {
            filterDomain.ToArray(),
            fields
                });

                return categoryResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching categories by updated date range: {ex.Message}");
                return null; // Return null or throw an exception as needed
            }
        }

        public object[] ReadCategories(DateTime? fromdate = null, DateTime? todate = null)
        {
            var client = XmlRpcProxyGen.Create<IOdooObjectRpcClient>();

            try
            {
                List<object> filterdomain = new List<object>();

                if (fromdate.HasValue)
                {
                    filterdomain.Add(new object[]
                    {
                        "write_date", ">=", fromdate.Value.ToString("dd-MM-yyyy 00:00:00")
                    });
                }

                if (todate.HasValue)
                {
                    filterdomain.Add(new object[]
                    {
                        "write_date", "<=", todate.Value.ToString("dd-MM-yyyy 23:59:59")
                    });
                }

                // fetch the category information
                var categoryresponse = client.SearchRead(_db, _uid, _password, "product.category", "search_read", new object[]
                {
                    filterdomain.ToArray(),
                new object[] { "id", "name" } // specify the fields you want to fetch
                });

                if (categoryresponse != null && categoryresponse.Length > 0)
                {
                    return categoryresponse;
                }
                else
                {
                    Console.WriteLine($"no categories found for the given date range.");
                    return new Dictionary<string, object>[0]; // return an empty array if no categories found
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error fetching category details: {ex.Message}");
                return null; // return null to indicate an error occurred
            }
        }

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
            int Create(string db, int uid, string password, string model, string method, object[] args);

            [XmlRpcMethod("execute_kw")]
            bool Update(string db, int uid, string password, string model, string method, object[] args);

            [XmlRpcMethod("execute_kw")]
            bool Delete(string db, int uid, string password, string model, string method, object[] args);

            [XmlRpcMethod("execute_kw")]
            int[] Search(string db, int uid, string password, string model, object[] domain, object options);

            [XmlRpcMethod("execute_kw")]
            object[] Read(string db, int uid, string password, string model, string method, object[] args);

            [XmlRpcMethod("execute_kw")]
            object[] SearchRead(string db, int uid, string password, string model, string method, object[] args);
        }
    }
}
