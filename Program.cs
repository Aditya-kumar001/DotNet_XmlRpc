/*
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OdooXmlRpcLibrary;
//using System.Globalization;
//using static System.Runtime.InteropServices.JavaScript.JSType;


namespace ConsoleApp1
{


    class Program
    {
        static void Main(string[] args)
        {
            string host = "localhost";
            int port = 8010;
            string db = "invoice_payment1";
            string username = "admin";
            string password = "admin";

            OdooXmlRpcMain odooClient = new OdooXmlRpcMain(host, port, db, username, password);

            Program p = new Program();
            p.test(odooClient).Wait();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();


        }
        public async Task test(OdooXmlRpcMain client)
        {
            try
            {
                // Attempt to login to Odoo
                bool isLoginSuccessful = await client.LoginToOdoo();
                Console.WriteLine("Login successful => {0}", isLoginSuccessful);

                if (isLoginSuccessful)
                {

                    //=====================This console app code is here is used for the invoiceing purposes===================//
                    /*
                    string name = "INV/2024/10099";
                    DateTime invoice_date = DateTime.UtcNow.Date;
                    string state = "draft";

                    var invoice_lines = new object[] { new object[] { 0, 0, new { product_id = 2, quantity = 8, price_unit = 10 } } };
                    var data = new Dictionary<string, object>();
                    data["name"] = name;
                    data["state"] = state;
                    data["invoice_date"] = invoice_date;
                    data["move_type"] = "out_invoice";
                    data["partner_id"] = 3;
                    data["invoice_line_ids"] = invoice_lines;

                    long id = await client.CreateInvoice(data);
                    bool posted = await client.PostInvoiceToPost(id);

                    Console.WriteLine("Invoice creation successful => Invoice ID: {0}", id);
                    
                    /*
                    if (id == -1)
                    {
                        Console.WriteLine("Invoice already exist so no need to register payment");
                        return;
                    }
                    long journalId = 6; //this is bank journal id
                    long companyId = 1;

                    var result = await client.ApplyPayment(id, journalId, companyId);
                    Console.WriteLine($"Payment registered result: {result}");

                    
                    //=====================This above console app code is here is used for the invoiceing purposes===================//

                   

                }
                else
                {
                    Console.WriteLine("Login to Odoo failed. Cannot create invoice.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);

            }

        }


    }
}

*/


//****************************For the product category class library******************************//

/*
using CookComputing.XmlRpc;
using OdooXmlRpcLibrary;
using System;
using System.Security.Cryptography;
using static OdooXmlRpcLibrary.OdooXmlRpcClient;
    

namespace ConsoleApp1
{

    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;


    public partial class CategoryDataClass
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        // Add any other properties as needed

        public static CategoryDataClass FromJson(string json)
        {
            try
            {
                if (string.IsNullOrEmpty(json))
                {
                    throw new ArgumentException("JSON string is null or empty.");
                }

                Console.WriteLine($"JSON string received: {json}"); // Log JSON string received
                return JsonConvert.DeserializeObject<CategoryDataClass>(json, Converter.Settings);
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                throw; // Rethrow the exception or handle it as appropriate
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"JSON string is null or empty: {ex.Message}");
                throw; // Rethrow the exception or handle it as appropriate
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw; // Rethrow the exception or handle it as appropriate
            }
        }

    }

    public static class Serialize
    {
        public static string ToJson(this CategoryDataClass self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
        {
            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
        },
        };
    }
    class Program
    {
        static void Main(string[] args)
        {
            string url = "http://localhost:8010";
            string db = "invoice_payment1";
            string username = "admin";
            string password = "admin";

            OdooXmlRpcClient odooClient = new OdooXmlRpcClient(url, db, username, password);



            //==============================SEARCH BY PARENT NAME======================================================//
            //Console.Write("Enter parent category name: ");
            //string parentName = Console.ReadLine();

            //var categories = odooClient.GetCategoriesByParentName(parentName);

            //if (categories != null)
            //{
            //    Console.WriteLine($"Categories found for parent name '{parentName}':");
            //    foreach (var category in categories)
            //    {
            //        Console.WriteLine($"ID: {category.Id}, Name: {category.Name}");
            //    }
            //}
            //else
            //{
            //    Console.WriteLine($"No categories found for parent name '{parentName}'.");
            //}


            //==============================UPDATED DATE RANGE WHEN BOTH DATE ARE THERE ===============================//
            /*
            DateTime? fromDate = new DateTime(2023, 1, 1);
            DateTime? toDate = new DateTime(2023, 12, 31);
            var categories = odooClient.GetCategoriesByUpdatedDateRange(fromDate, toDate);

            if (categories != null)
            {
                Console.WriteLine($"Categories updated between {fromDate} and {toDate}:");
                foreach (var item in categories)
                {
                    var data = CategoryDataClass.FromJson(item.ToString());
                    Console.WriteLine($"{data.Name}");
                }
            }
            else
            {
                Console.WriteLine("No categories found in the specified date range.");
            }

            Console.ReadLine();
            */
//==============================UPDATED DATE RANGE WHEN BOTH DATE ARE THERE ===============================//



//int parentId = 1; // Replace with the actual parent ID
//var categories = odooClient.GetCategoriesByParentId(parentId);

//if (categories != null)
//{
//    Console.WriteLine($"Categories under parent ID {parentId}:");
//    foreach (var item in categories)
//    {
//        var data = CategoryDataClass.FromJson(item.ToString());
//        Console.WriteLine($"{data.Name}");
//    }
//}
//else
//{
//    Console.WriteLine($"No categories found under parent ID {parentId}.");
//}

//Console.ReadLine();
//int categoryId = 2;

//// Fetch category details including child categories if needed
//bool includeChildCategories = true;
//var categoryDetails = odooClient.GetCategoryById(categoryId, includeChildCategories);

//if (categoryDetails != null)
//{
//    Console.WriteLine($"Category Details for ID {categoryId}:");
//    foreach (var kvp in categoryDetails)
//    {
//        Console.WriteLine($"{kvp.Key}: {kvp.Value}");
//    }

//    if (includeChildCategories && categoryDetails.ContainsKey("child_categories"))
//    {
//        Console.WriteLine("Child Categories:");
//        foreach (var childCategory in (Dictionary<string, object>[])categoryDetails["child_categories"])
//        {
//            Console.WriteLine($"- {childCategory["id"]} : {childCategory["name"]}");
//        }
//    }
//}
//else
//{
//    Console.WriteLine($"Category with ID {categoryId} not found.");
//}

//Console.WriteLine("\nPress any key to exit...");
//Console.ReadKey();


//try
//{
//===========Create single category==========================

//var fields = new Dictionary<string, object>
//{
//    { "name", "Mobile phone" },
//    { "parent_id", 1 }
//};
//int createdcategoryid = odooClient.CreateCategory(fields);

//===========Create single category==========================



// ===============Create Multiple categories=================

//var fieldSets = new Dictionary<string, object>[]
//{
//    new Dictionary<string, object>
//    {
//        { "name", "Books" },
//        { "description", "All kinds of books" }
//    },
//    new Dictionary<string, object>
//    {
//        { "name", "Clothing" },
//        { "description", "Men and women clothing" }
//    },
//    new Dictionary<string, object>
//    {
//        { "name", "Toys" },
//        { "description", "Toys for children" }
//    }
//};
//int[] createdCategoryIds = odooClient.CreateCategories(fieldSets);

// ===============Create Multiple categories=================



//========Update a category by categoryID and giving field name and corresponding value==========

//int categoryid = 13;
//var fields = new dictionary<string, object>
//{
//    { "name", "updated electronics" },
//    //{ "parent_id", 3 }
//};
//bool updateresult = odooclient.updatecategory(categoryid, fields);

//========Update a category by categoryID and giving field name and corresponding value==========


//============Delete a category by categoryID===================

//int categoryId = 13;
//bool deleteResult = odooClient.DeleteCategory(categoryId);

//============Delete a category by categoryID===================


//=================================Get a category information==================================
// int categoryid = 3; // replace with the actual category id you want to fetch
//object[] categorydetails = odooClient.SearchRead();

//if (categorydetails != null)
//{
//    foreach(var item in categorydetails)
//    {
//        var item = categorydetails[i];
//        var data= CategoryDataClass.FromJson(item.ToString());
//        Console.WriteLine($"{data.Name}");
//    }


//}
//else
//{
//    Console.WriteLine($"category with id not found.");
//}

//Console.ReadLine();
//===================Update using date and time =========================

// Scenario 1: From Date IS NOT NULL and To Date IS NULL
//DateTime fromDate1 = new DateTime(2024, 06, 14);  // Example From Date
//var categories1 = odooClient.ReadCategories(fromDate: fromDate1);
//PrintCategories(categories1, "Categories updated on or after " + fromDate1.ToString("yyyy-MM-dd"));


//static void PrintCategories(Dictionary<string, object>[] categories, string header)
//{
//    Console.WriteLine(header);
//    if (categories != null)
//    {
//        foreach (var category in categories)
//        {
//            Console.WriteLine($"Category ID: {category["id"]}, Name: {category["name"]}");
//        }
//    }
//    else
//    {
//        Console.WriteLine("No categories found.");
//    }
//    Console.WriteLine();
//}


//}
//catch (Exception ex)
//{
//    Console.WriteLine("An error occurred: " + ex.Message);
//}
//============================updateddaterange=============================================================//
//DateTime fromDate = new DateTime(2023, 1, 1); // Example start date
//DateTime toDate = DateTime.Now; // Example end date (current date)

//// Fetch categories updated within the specified date range
//Dictionary<string, object>[] categories = odooClient.GetCategoriesByUpdatedDateRange(fromDate, toDate);

//// Print the fetched categories
//Console.WriteLine(categories);
//if (categories != null && categories.Length > 0)
//{
//    foreach (var category in categories)
//    {
//        Console.WriteLine("Category:");
//        foreach (var kvp in category)
//        {
//            Console.WriteLine($"{fields.Key}: {.Value}");
//        }
//        Console.WriteLine();
//    }
//}
//else
//{
//    Console.WriteLine("No categories found for the given date range.");
//}
//        }
//    }
//}



//*************************************For Quottion Library***********************************//
/*
using System;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OdooXmlRpcLibrary;
//using static System.Runtime.InteropServices.JavaScript.JSType;


namespace ConsoleApp1
{


    class Program
    {
        static void Main(string[] args)
        {
            string host = "localhost";
            int port = 8010;
            string db = "invoice_payment1";
            string username = "admin";
            string password = "admin";

            OdooXmlRpcMain odooClient = new OdooXmlRpcMain(host, port, db, username, password);

            Program p = new Program();
            p.test(odooClient).Wait();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

        }
        public async Task test(OdooXmlRpcMain client)
        {
            try
            {
                bool isLoginSuccessful = await client.LoginToOdoo();
                Console.WriteLine("Login successful => {0}", isLoginSuccessful);

                if (isLoginSuccessful)
                {

                    /*
                    string name = "P00102";

                    DateTime date_order = DateTime.UtcNow;
                    string formatted_date_order = date_order.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    string state = "draft";

                    var lines_ids = new object[]
                    {
                        new object[]
                        {
                            0, 0, new
                            {
                                product_id = 3,
                                product_qty = 220,
                                price_unit = 2340
                            }
                        }
                    };
                    var data = new Dictionary<string, object>
                    {
                        { "name", name },
                        { "state", state },
                        { "date_order", formatted_date_order },
                        { "partner_id", 1 },
                        { "order_line", lines_ids }
                    };
                    long id = await client.CreateRFQ(data);
                    Console.WriteLine("Quotation creation successful => Invoice ID: {0}", id);
                    */

                    /*
                    bool confirm = await client.ConfirmRfq(14);
                    Console.WriteLine($"Quotation is in confirm state: {confirm}");
                    */

                    /*
                    bool confirm = await client.ReceiveProduct(14);
                    Console.WriteLine($"Quotation is in receive product state: {confirm}");
                    */


                    /*
                    bool confirm = await client.ValidateProduct(16);
                    Console.WriteLine($"Quotation is in validate state: {confirm}");
                    */


                    //Not working -------XXXXXXX

                    /*
                    bool confirm = await client.CreateBill(14);
                    Console.WriteLine($"Quotation is in confirm state: {confirm}");
                    */

                    /*
                    bool confirm = await client.ConformBill(4);
                    Console.WriteLine($"Quotation is in conform bill: {confirm}");
                    */

                    /*
                    long journalId = 5; //this is bank journal id
                    long companyId = 1;
                    long invoiceId = 5;

                    bool confirm = await client.ReceivePayment(journalId, companyId, invoiceId );
                    Console.WriteLine($"Quotation is in conform bill: {confirm}");
                    */

                    /*
                    string name = "P00012";

                    DateTime date_order = DateTime.UtcNow;
                    string formatted_date_order = date_order.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    string state = "draft";

                    var lines_ids = new object[]
                    {
                        new object[]
                        {
                            0, 0, new
                            {
                                product_id = 3,
                                product_uom_qty = 220,
                                price_unit = 2340,
                                //tax_id = 10
                            }
                        }
                    };
                    var data = new Dictionary<string, object>
                    {
                        { "name", name },
                        { "state", state },
                        { "date_order", formatted_date_order },
                        { "partner_id", 1 },
                        { "order_line", lines_ids }
                    };
                    long id = await client.CreateQuotation(data);
                    Console.WriteLine("Quotation creation successful => Invoice ID: {0}", id);
                    */


                    //--------------------------XXXXXXXXX------------------------//
                    /*
                     bool confirm = await client.ChangeQuotationState(3);
                     Console.WriteLine($"Quotation is in confirm state: {confirm}");
                    */

                    /*
                    bool confirm = await client.CreateSaleOrderInvoice(6);
                    Console.WriteLine($"Quotation is in create sales order invoice: {confirm}");
                    


                }

                else
                {
                    Console.WriteLine("Login to Odoo failed. Cannot create invoice.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
        }
    }
}
*/