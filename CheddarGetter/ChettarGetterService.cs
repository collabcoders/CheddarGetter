using CheddarGetter.Helpers;
using CheddarGetter.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CheddarGetter
{
    /// <summary>
    /// Public interaface for all functions in the service
    /// </summary>
    public interface ICheddarGetterService
    {
        List<SubscriptionPlan> GetSubscriptionPlans();
        List<Customer> GetCustomers();
        List<Invoice> GetInvoices(string customerID);
        Customer GetCustomer(string customerCode);
        Task<Customer> CreateCustomer(Customer customer);
        Task<Customer> CreateCustomerWithCreditCard(CustomerPost customer);
        Task<Customer> CreateCustomerWithPayPal(CustomerPost customer, string returnUrl, string cancelUrl);
        Task<Customer> UpdateCustomerAndSubscription(CustomerPost customer);
        Task<Customer> UpdateCustomer(Customer customer);
        Task<Customer> UpdateSubscription(CustomerPost customer);
        Task<Customer> UpdateSubscriptionPlanOnly(string customerCode, string newPlan);
        bool CancelSubscription(string customerCode);
        bool DeleteCustomer(string customerCode);
        Task<Customer> AddItem(string customerCode, string itemCode, int quantityToAdd);
        Task<Customer> RemoveItem(string customerCode, string itemCode, int quantityToRemove);
        Task<Customer> SetItem(string customerCode, ProductItemCode itemCode, int quantityToSet);
        Task<Customer> RefundCharge(RefundChargePost refund);
        Task<Customer> IssueVoid(IssueVoidPost voidinvoice);
        Task<Customer> AddCustomCharge(CustomChargePost customCharge);
    }

    public class CheddarGetterService : ICheddarGetterService
    {

        private readonly IHttpService _httpService;
        private CheddarGetterConfig _config = new CheddarGetterConfig();

        /// <summary>
        /// Constructor
        /// </summary>
        public CheddarGetterService(IOptions<CheddarGetterConfig> options)
        {
            _config.productCode = options.Value.productCode;
            _config.username = options.Value.username;
            _config.password = options.Value.password;
            _httpService = new HttpService(options.Value.username, options.Value.password);
        }

        /// <summary>
        /// Get all of the subscription plans for your product code
        /// </summary>
        /// <returns>A list of SubscriptionPlan objects</returns>
        public List<SubscriptionPlan> GetSubscriptionPlans()
        {
            List<SubscriptionPlan> subscriptionPlansList = new List<SubscriptionPlan>();

            try
            {
                string urlPath = $"/plans/get/productCode/{_config.productCode}";

                string result = _httpService.getRequest(urlPath);
                XDocument plansXML = XDocument.Parse(result);

                subscriptionPlansList = (from p in plansXML.Descendants("plan")
                                         select new SubscriptionPlan
                                         {
                                             ID = (Guid)p.Attribute("id"),
                                             Code = (string)p.Attribute("code"),
                                             Name = (string)p.Element("name"),
                                             Description = (string)p.Element("description"),
                                             IsActive = (bool)p.Element("isActive"),
                                             TrialDays = (int)p.Element("trialDays"),
                                             BillingFrequency = (string)p.Element("billingFrequency"),
                                             BillingFrequencyPer = (string)p.Element("billingFrequencyPer"),
                                             BillingFrequencyUnit = (string)p.Element("billingFrequencyUnit"),
                                             BillingFrequencyQuantity = (string)p.Element("billingFrequencyQuantity"),
                                             SetupChargeCode = (string)p.Element("setupChargeCode"),
                                             SetupChargeAmount = (float)p.Element("setupChargeAmount"),
                                             RecurringChargeCode = (string)p.Element("recurringChargeCode"),
                                             RecurringChargeAmount = (float)p.Element("recurringChargeAmount"),
                                             CreatedDateTime = (DateTime)p.Element("createdDatetime"),
                                             PlanItems = (from i in p.Element("items").Descendants("item")
                                                          select new PlanItem
                                                          {
                                                              ID = (Guid)i.Attribute("id"),
                                                              Code = (string)i.Attribute("code"),
                                                              Name = (string)i.Element("name"),
                                                              QuantityIncluded = (double)i.Element("quantityIncluded"),
                                                              IsPeriodic = (bool)i.Element("isPeriodic"),
                                                              OverageAmount = (float)i.Element("overageAmount"),
                                                              CreatedDateTime = (DateTime)i.Element("createdDatetime")
                                                          }).ToList()
                                         }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return subscriptionPlansList;
        }

        /// <summary>
        /// Get a list of all customers for your product code
        /// </summary>
        /// <returns>A list of Customer objects</returns>
        public List<Customer> GetCustomers()
        {
            Customers customers = new Customers();

            try
            {
                string urlPath = $"/customers/get/productCode/{_config.productCode}";

                string result = _httpService.getRequest(urlPath);
                XDocument customersXML = XDocument.Parse(result);

                customers = getCustomerList(customersXML);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return customers.CustomerList;
        }

        public List<Invoice> GetInvoices(string customerID)
        {
            Invoices invoices = new Invoices();

            string urlPath = $"/customers/get/productCode/{_config.productCode}/id/{customerID}";

            try
            {
                string result = _httpService.getRequest(urlPath);

                XDocument invoicesXML = XDocument.Parse(result);

                invoices = getInvoiceList(invoicesXML);
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (HttpWebResponse)ex.Response;
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return invoices.InvoiceList;
        }

        /// <summary>
        /// Get a particular customer based on a passed in customer code and your product code
        /// </summary>
        /// <param name="customerCode">A string representing a customer's code in CG </param>
        /// <returns>A associated Customer object for the passed in customer code</returns>
        public Customer GetCustomer(string customerCode)
        {
            Customers customers = new Customers();
            Customer customer = new Customer();

            //use id for CG ID or code for unique customer code that we create
            string urlPath = $"/customers/get/productCode/{_config.productCode}/code/{customerCode}";
            try
            {
                string result = _httpService.getRequest(urlPath);

                XDocument customersXML = XDocument.Parse(result);

                customers = getCustomerList(customersXML);

                if (customers.CustomerList.Count > 0)
                {
                    customer = customers.CustomerList[0];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return customer;
        }

        /// <summary>
        /// Create a new customer based on the passed in CustomerPost object 
        /// </summary>
        /// <param name="customer">A CustomerPost object that represents a customer to be created</param>
        /// <returns>A newly created Customer object</returns>
        public async Task<Customer> CreateCustomerWithCreditCard(CustomerPost customer)
        {
            Customers customers = new Customers();
            Customer newCustomer = new Customer();
            try
            {
                string urlPath = $"/customers/new/productCode/{_config.productCode}";
                string postParams = FormatFunctions.addParam("code", customer.Code) +
                    FormatFunctions.addParam("firstName", customer.FirstName) +
                    FormatFunctions.addParam("lastName", customer.LastName) +
                    FormatFunctions.addParam("email", customer.Email) +
                    FormatFunctions.addParam("company", customer.Company) +
                    FormatFunctions.addParam("notes", customer.Notes) +
                    FormatFunctions.addParam("subscription[planCode]", customer.PlanCode) +
                    FormatFunctions.addParam("subscription[ccFirstName]", customer.CCFirstName) +
                    FormatFunctions.addParam("subscription[ccLastName]", customer.CCLastName) +
                    FormatFunctions.addParam("subscription[ccNumber]", customer.CCNumber) +
                    FormatFunctions.addParam("subscription[ccExpiration]", customer.CCExpiration) +
                    FormatFunctions.addParam("subscription[ccCardCode]", customer.CCCardCode) +
                    FormatFunctions.addParam("subscription[ccZip]", customer.CCZip) +
                    FormatFunctions.addMetaDataParams(customer.AdditionalMetaData);

                string result = await _httpService.postRequest(urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);
                customers = getCustomerList(newCustomerXML);
                if (customers.CustomerList.Count > 0)
                {
                    newCustomer = customers.CustomerList[0];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return newCustomer;
        }

        /// <summary>
        /// Create a new customer based on the passed in CustomerPost object 
        /// </summary>
        /// <param name="customer">A CustomerPost object that represents a customer to be created</param>
        /// <param name="returnUrl">The return url for PayPal transactions</param>
        /// <param name="cancelUrl">The cancel url for PayPal transactions</param>
        /// <returns>A newly created Customer object</returns>
        public async Task<Customer> CreateCustomerWithPayPal(CustomerPost customer, string returnUrl, string cancelUrl)
        {
            Customers customers = new Customers();
            Customer newCustomer = new Customer();
            try
            {
                string urlPath = $"/customers/new/productCode/{_config.productCode}";
                string postParams = "subscription[method]=paypal" +
                    FormatFunctions.addParam("code", customer.Code) +
                    FormatFunctions.addParam("firstName", customer.FirstName) +
                    FormatFunctions.addParam("lastName", customer.LastName) +
                    FormatFunctions.addParam("email", customer.Email) +
                    FormatFunctions.addParam("subscription[planCode]", customer.PlanCode) +
                    FormatFunctions.addParam("subscription[ccFirstName]", customer.CCFirstName) +
                    FormatFunctions.addParam("subscription[ccLastName]", customer.CCLastName) +
                    FormatFunctions.addParam("subscription[returnUrl]", returnUrl) +
                    FormatFunctions.addParam("subscription[cancelUrl]", cancelUrl) +
                    FormatFunctions.addMetaDataParams(customer.AdditionalMetaData);

                string result = await _httpService.postRequest(urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);
                customers = getCustomerList(newCustomerXML);
                if (customers.CustomerList.Count > 0)
                {
                    newCustomer = customers.CustomerList[0];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return newCustomer;
        }


        /// <summary>
        /// Create a new customer without credit card info based on the passed in Customer object
        /// </summary>
        /// <param name="customer">A Customer object that represents a customer to be created</param>
        /// <returns>A newly created Customer object</returns>
        public async Task<Customer> CreateCustomer(Customer customer)
        {
            Customers customers = new Customers();
            Customer newCustomer = new Customer();
            try
            {
                string urlPath = string.Format("/customers/new/productCode/{0}", _config.productCode);
                var postParams = FormatFunctions.addParam("code", customer.Code) +
                                 FormatFunctions.addParam("firstName", customer.FirstName) +
                                 FormatFunctions.addParam("lastName", customer.LastName) +
                                 FormatFunctions.addParam("email", customer.Email) +
                                 FormatFunctions.addParam("company", customer.Company) +
                                 FormatFunctions.addParam("notes", customer.Notes) +
                                 FormatFunctions.addParam("subscription[planCode]", customer.PlanCode) +
                                 FormatFunctions.addParam("remoteAddress", customer.RemoteAddress) +
                                 FormatFunctions.addMetaDataParams(customer.AdditionalMetaData);

                string result = await _httpService.postRequest(urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);
                customers = getCustomerList(newCustomerXML);
                if (customers.CustomerList.Count > 0)
                {
                    newCustomer = customers.CustomerList[0];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return newCustomer;
        }

        /// <summary>
        /// Update a customer
        /// </summary>
        /// <param name="customer">A CustomerPost object that represents the changes to be updated</param>
        /// <returns>An updated Customer object with the changes applied</returns>
        public async Task<Customer> UpdateCustomer(Customer customer)
        {
            Customers customers = new Customers();
            Customer updatedCustomer = new Customer();

            try
            {
                // Create the web request  
                string urlPath = string.Format("/customers/edit-customer/productCode/{0}/code/{1}", _config.productCode, customer.Code);
                string postParams = FormatFunctions.addParam("firstName", customer.FirstName) +
                FormatFunctions.addParam("lastName", customer.LastName) +
                FormatFunctions.addParam("email", customer.Email) +
                FormatFunctions.addParam("company", customer.Company) +
                FormatFunctions.addParam("notes", customer.Notes) +
                FormatFunctions.addParam("remoteAddress", customer.RemoteAddress) +
                FormatFunctions.addMetaDataParams(customer.AdditionalMetaData);

                string result = await _httpService.postRequest(urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);
                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    updatedCustomer = customers.CustomerList[0];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return updatedCustomer;
        }

        /// <summary>
        /// Update a customer and their subscription
        /// </summary>
        /// <param name="customer">A CustomerPost object that represents the changes to be updated</param>
        /// <returns>An updated Customer object with the changes applied</returns>
        public async Task<Customer> UpdateCustomerAndSubscription(CustomerPost customer)
        {
            Customers customers = new Customers();
            Customer updatedCustomer = new Customer();

            try
            {
                // Create the web request  
                string urlPath = $"/customers/edit/productCode/{_config.productCode}/code/{customer.Code}";
                string postParams = FormatFunctions.addParam("firstName", customer.FirstName) +
                    FormatFunctions.addParam("lastName", customer.LastName) +
                    FormatFunctions.addParam("email", customer.Email) +
                    FormatFunctions.addParam("company", customer.Company) +
                    FormatFunctions.addParam("notes", customer.Notes) +
                    FormatFunctions.addParam("subscription[planCode]", customer.PlanCode.ToString().ToUpper()) +
                    FormatFunctions.addParam("subscription[ccFirstName]", customer.CCFirstName) +
                    FormatFunctions.addParam("subscription[ccLastName]", customer.CCLastName) +
                    FormatFunctions.addParam("subscription[ccNumber]", customer.CCNumber) +
                    FormatFunctions.addParam("subscription[ccExpiration]", FormatFunctions.formatMonth(customer.CCExpMonth) + @"/" + customer.CCExpYear) +
                    FormatFunctions.addParam("subscription[ccCardCode]", customer.CCCardCode) +
                    FormatFunctions.addParam("subscription[ccZip]", customer.CCZip) +
                    FormatFunctions.addParam("remoteAddress", customer.RemoteAddress) +
                    FormatFunctions.addMetaDataParams(customer.AdditionalMetaData);

                string result = await _httpService.postRequest(urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);
                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    updatedCustomer = customers.CustomerList[0];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return updatedCustomer;
        }

        /// <summary>
        /// Update a customer's subscription
        /// </summary>
        /// <param name="customer">A CustomerPost object with the subscription details to update</param>
        /// <returns>A Customer object with the applied changes</returns>
        public async Task<Customer> UpdateSubscription(CustomerPost customer)
        {
            Customers customers = new Customers();
            Customer updatedCustomer = new Customer();
            try
            {
                // Create the web request  
                string urlPath = $"/customers/edit-subscription/productCode/{_config.productCode}/code/{customer.Code}";

                string postParams = FormatFunctions.addParam("planCode", customer.PlanCode) +
                FormatFunctions.addParam("ccFirstName", customer.CCFirstName) +
                FormatFunctions.addParam("ccLastName", customer.CCLastName) +
                FormatFunctions.addParam("ccNumber", customer.CCNumber) +
                FormatFunctions.addParam("ccExpiration", FormatFunctions.formatMonth(customer.CCExpMonth) + @"/" + customer.CCExpYear) +
                FormatFunctions.addParam("ccCardCode", customer.CCCardCode) +
                FormatFunctions.addParam("ccZip", customer.CCZip) +
                FormatFunctions.addParam("remoteAddress", customer.RemoteAddress) +
                FormatFunctions.addMetaDataParams(customer.AdditionalMetaData);

                string result = await _httpService.postRequest(urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);
                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    updatedCustomer = customers.CustomerList[0];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return updatedCustomer;
        }

        /// <summary>
        /// Update a customer's subscription plan
        /// </summary>
        /// <param name="customerCode">The customer's code of the customer to be updated</param>
        /// <param name="newPlan">The plan to set the customer to</param>
        /// <returns>A Customer object with the updated changes applied</returns>
        public async Task<Customer> UpdateSubscriptionPlanOnly(string customerCode, string newPlan)
        {
            Customers customers = new Customers();
            Customer updatedCustomer = new Customer();

            try
            {
                // Create the web request  
                string urlPath = $"/customers/edit-subscription/productCode/{_config.productCode}/code/{customerCode}";

                string postParams = $"planCode={WebUtility.UrlEncode(newPlan.ToString().ToUpper())}";

                string result = await _httpService.postRequest(urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);

                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    updatedCustomer = customers.CustomerList[0];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return updatedCustomer;
        }

        /// <summary>
        /// Cancel a customer's subscription
        /// </summary>
        /// <param name="customerCode">The customer code of the customer to cancel</param>
        /// <returns>A bool representing the success of the cancel</returns>
        public bool CancelSubscription(string customerCode)
        {
            Customers customers = new Customers();
            Customer editCustomer = new Customer();
            bool canceled = false;

            try
            {
                string urlPath = $"/customers/cancel/productCode/{_config.productCode}/code/{customerCode}";

                string result = _httpService.getRequest(urlPath);
                XDocument newCustomerXML = XDocument.Parse(result);

                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    editCustomer = customers.CustomerList[0];
                }

                canceled = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return canceled;
        }

        /// <summary>
        /// Delete customer's subscription
        /// </summary>
        /// <param name="customerCode">The customer code of the customer to delete</param>
        /// <returns>A bool representing the success of the deletion</returns>
        public bool DeleteCustomer(string customerCode)
        {
            Customers customers = new Customers();
            Customer deleteCustomer = new Customer();
            bool deleted = false;

            try
            {
                string urlPath = $"/customers/delete/productCode/{_config.productCode}/code/{customerCode}";

                string result = _httpService.getRequest(urlPath);
                XDocument newCustomerXML = XDocument.Parse(result);

                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    deleteCustomer = customers.CustomerList[0];
                }

                deleted = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return deleted;
        }

        /// <summary>
        /// Add an item and set the quantity for a customer
        /// Note: if no quantity is specified then it will increment by 1 by default
        /// </summary>
        /// <param name="customerCode">The customer's code to associate the item with</param>
        /// <param name="itemCode">The item code of the item which we are adding</param>
        /// <param name="quantityToAdd">The number of units to add of this item</param>
        /// <returns>A Customer object reflecting the updated item and quantity</returns>
        public async Task<Customer> AddItem(string customerCode, string itemCode, int quantityToAdd)
        {
            Customers customers = new Customers();
            Customer editCustomer = new Customer();

            try
            {
                string urlPath = $"/customers/add-item-quantity/productCode/{_config.productCode}/code/{customerCode}/itemCode/{itemCode}";
                string postParams = "";

                if (quantityToAdd > 1)
                {
                    postParams = $"quantity={WebUtility.UrlEncode(quantityToAdd.ToString())}";
                }

                string result = await _httpService.postRequest(urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);

                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    editCustomer = customers.CustomerList[0];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return editCustomer;
        }

        /// <summary>
        /// Remove an item and set the quantity for a customer
        /// Note: if no quantity is specified then it will decrement by 1 by default
        /// </summary>
        /// <param name="customerCode">The customer's code to associate the item with</param>
        /// <param name="itemCode">The item code of the item which we are removing</param>
        /// <param name="quantityToRemove">The number of units to remove of this item</param>
        /// <returns>A Customer object reflecting the updated item and quantity</returns>
        public async Task<Customer> RemoveItem(string customerCode, string itemCode, int quantityToRemove)
        {
            Customers customers = new Customers();
            Customer editCustomer = new Customer();

            try
            {
                string urlPath = $"/customers/remove-item-quantity/productCode/{_config.productCode}/code/{customerCode}/itemCode/{itemCode}";
                string postParams = "";
                if (quantityToRemove > 1)
                {
                    postParams = $"quantity={WebUtility.UrlEncode(quantityToRemove.ToString())}";
                }

                string result = await _httpService.postRequest(urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);

                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    editCustomer = customers.CustomerList[0];
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return editCustomer;
        }

        /// <summary>
        /// Set an item count to a specific quantity
        /// </summary>
        /// <param name="customerCode">The customer's code of the customer that will be updated </param>
        /// <param name="itemCode">The code of the item that will be updated</param>
        /// <param name="quantityToSet">The quantity to set for the item</param>
        /// <returns>A Customer object reflecting the updated item and quantity count</returns>
        public async Task<Customer> SetItem(string customerCode, ProductItemCode itemCode, int quantityToSet)
        {
            Customers customers = new Customers();
            Customer editCustomer = new Customer();

            try
            {
                string urlPath = $"/customers/set-item-quantity/productCode/{_config.productCode}/code/{customerCode}/itemCode/{itemCode.ToString()}";
                string postParams = $"quantity={WebUtility.UrlEncode(quantityToSet.ToString())}";

                string result = await _httpService.postRequest(urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);

                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    editCustomer = customers.CustomerList[0];
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return editCustomer;
        }

        /// <summary>
        /// Refunds the charge.
        /// </summary>
        /// <param name="refund">The refund.</param>
        /// <returns></returns>
        public async Task<Customer> RefundCharge(RefundChargePost refund)
        {
            Customers customers = new Customers();
            Customer refundCustomer = new Customer();

            try
            {
                string urlPath = $"/invoices/refund/productCode/{_config.productCode}/";
                string postParams = $"number={WebUtility.UrlEncode(refund.InvoiceNumber)}" +
                    $"&amount={WebUtility.UrlEncode(refund.RefundAmount.ToString())}";

                string result = await _httpService.postRequest(urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);

                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    refundCustomer = customers.CustomerList[0];
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return refundCustomer;
        }

        /// <summary>
        /// Issues the void.
        /// </summary>
        /// <param name="voidinvoice">The voidinvoice.</param>
        /// <returns></returns>
        public async Task<Customer> IssueVoid(IssueVoidPost voidinvoice)
        {
            Customers customers = new Customers();
            Customer voidCustomer = new Customer();

            try
            {
                string urlPath = $"/invoices/void/productCode/{_config.productCode}/";
                string postParams = $"number={WebUtility.UrlEncode(voidinvoice.InvoiceNumber)}";

                string result = await _httpService.postRequest(urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);

                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    voidCustomer = customers.CustomerList[0];
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return voidCustomer;
        }

        /// <summary>
        /// Add a customer charge for a customer
        /// </summary>
        /// <param name="customCharge">A CustomerChargePost object with the customer charge and customer code</param>
        /// <returns>A Customer object with the reflected custom charge</returns>
        public async Task<Customer> AddCustomCharge(CustomChargePost customCharge)
        {
            Customers customers = new Customers();
            Customer editCustomer = new Customer();

            try
            {
                string urlPath = $"/customers/set-item-quantity/productCode/{_config.productCode}/code/{customCharge.CustomerCode}/itemCode/{customCharge.ItemCode}";
                string postParams = FormatFunctions.addParam("chargeCode", customCharge.ChargeCode) +
                    FormatFunctions.addParam("quantity", customCharge.Quantity.ToString()) +
                    FormatFunctions.addParam("eachAmount", customCharge.EachAmount.ToString()) +
                    FormatFunctions.addParam("description", customCharge.Description);

                string result = await _httpService.postRequest(urlPath, postParams);
                XDocument newCustomerXML = XDocument.Parse(result);
                customers = getCustomerList(newCustomerXML);

                if (customers.CustomerList.Count > 0)
                {
                    editCustomer = customers.CustomerList[0];
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return editCustomer;
        }

        /// <summary>
        /// Get the customer list and any associated CG errors from a XDocument (customer XML)
        /// </summary>
        /// <param name="customersXML">A XDocument that contains customer XML data</param>
        /// <returns>A Customers object (which is a list of customers and any associated GC errors) 
        /// that is built from the parsed XDocument</returns>
        private Customers getCustomerList(XDocument customersXML)
        {
            Customers customers = new Customers();
            List<Customer> customerList = new List<Customer>();
            List<CGError> errorList = new List<CGError>();

            try
            {
                customerList = (from c in customersXML.Descendants("customer")
                                select new Customer
                                {
                                    ID = (Guid)c.Attribute("id"),
                                    Code = (string)c.Attribute("code"),
                                    FirstName = (string)c.Element("firstName"),
                                    LastName = (string)c.Element("lastName"),
                                    Company = (string)c.Element("company"),
                                    Notes = (string)c.Element("notes"),
                                    Email = (string)c.Element("email"),
                                    GatewayToken = (string)c.Element("gatewayToken"),
                                    //CreatedDateTime = (DateTime)c.Element("createdDatetime"),
                                    //ModifiedDateTime = (DateTime)c.Element("modifiedDatetime"),
                                    //Subscriptions = getSubscriptionList(c.Element("subscriptions")),
                                }).ToList();

                errorList = (from e in customersXML.Descendants("errors")
                             select new CGError
                             {
                                 ID = (string)e.Attribute("id"),
                                 Code = (string)e.Attribute("code"),
                                 AuxCode = (string)e.Attribute("auxCode"),
                                 Message = (string)e.Element("error")
                             }).ToList();

                customers.CustomerList = customerList;
                customers.ErrorList = errorList;

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return customers;
        }

        /// <summary>
        /// Gets the invoice list.
        /// </summary>
        /// <param name="invoicesXML">The invoices XML.</param>
        /// <returns></returns>
        private Invoices getInvoiceList(XDocument invoicesXML)
        {
            Invoices invoices = new Invoices();
            List<Invoice> invoiceList = new List<Invoice>();
            List<CGError> errorList = new List<CGError>();

            try
            {
                invoiceList = (from i in invoicesXML.Descendants("invoice")
                               select new Invoice
                               {
                                   ID = (Guid)i.Attribute("id"),
                                   Number = (int)i.Element("number"),
                                   Type = (string)i.Element("type"),
                                   CreatedDateTime = (DateTime)i.Element("createdDatetime"),
                                   BillingDateTime = (DateTime)i.Element("billingDatetime"),
                                   Transactions = getTransactions(i.Element("transactions")),
                                   Charges = getCharges(i.Element("charges")),
                               }).ToList();

                errorList = (from e in invoicesXML.Descendants("errors")
                             select new CGError
                             {
                                 ID = (string)e.Attribute("id"),
                                 Code = (string)e.Attribute("code"),
                                 AuxCode = (string)e.Attribute("auxCode"),
                                 Message = (string)e.Element("error")
                             }).ToList();

                invoices.InvoiceList = invoiceList;
                invoices.ErrorList = errorList;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return invoices;

        }

        /// <summary>
        /// Get a list of SubscriptionItem objects based on an item XML node in a XElement object
        /// </summary>
        /// <param name="item">A XElement object representing a node of XML items</param>
        /// <returns>A list of SubscriptionItem objects</returns>
        private List<SubscriptionItem> getSubscriptionItems(XElement item)
        {
            List<SubscriptionItem> subscriptionItemList = new List<SubscriptionItem>();

            try
            {
                if (item != null && item.Descendants("item") != null)
                {
                    subscriptionItemList = (from si in item.Descendants("item")
                                            select new SubscriptionItem
                                            {
                                                ID = (Guid)si.Attribute("id"),
                                                Code = (string)si.Attribute("code"),
                                                Name = (string)si.Element("name"),
                                                Quantity = (int)si.Element("quantity"),
                                                CreatedDateTime = si.Element("createdDatetime") == null ? (DateTime?)null : (DateTime?)si.Element("createdDatetime"),
                                                ModifiedDateTime = (DateTime?)si.Element("modifiedDatetime")
                                            }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return subscriptionItemList;
        }

        /// <summary>
        /// Get a list of charges based on a charge XML node in a XElement object
        /// </summary>
        /// <param name="charges">A XElement object representing a node of XML charges</param>
        /// <returns>A list of Charge objects</returns>
        private List<Charge> getCharges(XElement charges)
        {
            List<Charge> chargeList = new List<Charge>();

            try
            {
                if (charges != null && charges.Descendants("charge") != null)
                {
                    chargeList = (from ch in charges.Descendants("charge")
                                  select new Charge
                                  {
                                      ID = string.IsNullOrEmpty(ch.Attribute("id").Value) ? (Guid?)null : (Guid?)ch.Attribute("id"),
                                      Code = (string)ch.Attribute("code"),
                                      Type = (string)ch.Element("type"),
                                      Quantity = (int)ch.Element("quantity"),
                                      EachAmount = (float)ch.Element("eachAmount"),
                                      Description = (string)ch.Element("description"),
                                      CreatedDateTime = (DateTime)ch.Element("createdDatetime")
                                  }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return chargeList;
        }

        private List<Transaction> getTransactions(XElement transactions)
        {
            List<Transaction> transList = new List<Transaction>();

            try
            {
                if (transactions != null && transactions.Descendants("transaction") != null)
                {
                    transList = (from tr in transactions.Descendants("transaction")
                                 select new Transaction
                                 {
                                     ID = string.IsNullOrEmpty(tr.Attribute("id").Value) ? (Guid?)null : (Guid?)tr.Attribute("id"),
                                     Response = (string)tr.Element("response")
                                 }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return transList;
        }

        /// <summary>
        /// Get a list of invoices based on an invoice XML node in a XElement object
        /// </summary>
        /// <param name="invoices">A XElement object representing a node of XML invoices</param>
        /// <returns>A list of Invoice objects</returns>
        private List<Invoice> getInvoiceList(XElement invoices)
        {
            List<Invoice> invoiceList = new List<Invoice>();

            try
            {
                if (invoices != null && invoices.Descendants("invoice") != null)
                {
                    invoiceList = (from i in invoices.Descendants("invoice")
                                   select new Invoice
                                   {
                                       ID = (Guid)i.Attribute("id"),
                                       Number = (int)i.Element("number"),
                                       Type = (string)i.Element("type"),
                                       BillingDateTime = (DateTime)i.Element("billingDatetime"),
                                       PaidTransactionId = string.IsNullOrEmpty(i.Element("paidTransactionId").Value) ? (Guid?)null : (Guid?)i.Element("paidTransactionId"),
                                       CreatedDateTime = (DateTime)i.Element("createdDatetime"),
                                       Charges = getCharges(i.Element("charges")),
                                       Transactions = getTransactions(i.Element("transactions"))
                                   }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return invoiceList;
        }

        /// <summary>
        /// Get a list of plan items based on an items XML node in a XElement object
        /// </summary>
        /// <param name="items">A XElement object representing a node of XML items</param>
        /// <returns>A list of PlanItem objects</returns>
        private List<PlanItem> getPlanItemsList(XElement items)
        {
            List<PlanItem> planItemList = new List<PlanItem>();

            try
            {
                if (items != null && items.Descendants("item") != null)
                {
                    planItemList = (from pi in items.Descendants("item")
                                    select new PlanItem
                                    {
                                        ID = (Guid)pi.Attribute("id"),
                                        Code = (string)pi.Attribute("code"),
                                        Name = (string)pi.Element("name"),
                                        QuantityIncluded = (int)pi.Element("quantityIncluded"),
                                        IsPeriodic = (bool)pi.Element("isPeriodic"),
                                        OverageAmount = (float)pi.Element("overageAmount"),
                                        CreatedDateTime = (DateTime)pi.Element("createdDatetime")
                                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return planItemList;
        }

        /// <summary>
        /// Get a list of subscription plans based on a plan XML node in a XElement object
        /// </summary>
        /// <param name="plans">A XElement object representing a node of XML plans</param>
        /// <returns>A list of SubscriptionPlan objects</returns>
        private List<SubscriptionPlan> getSubscriptionPlanList(XElement plans)
        {
            List<SubscriptionPlan> subscriptionPlanList = new List<SubscriptionPlan>();

            try
            {
                if (plans != null && plans.Descendants("plan") != null)
                {
                    subscriptionPlanList = (from sp in plans.Descendants("plan")
                                            select new SubscriptionPlan
                                            {
                                                ID = (Guid)sp.Attribute("id"),
                                                Code = (string)sp.Attribute("code"),
                                                Name = (string)sp.Element("name"),
                                                Description = (string)sp.Element("description"),
                                                IsActive = (bool)sp.Element("isActive"),
                                                TrialDays = (int)sp.Element("trialDays"),
                                                BillingFrequency = (string)sp.Element("billingFrequency"),
                                                BillingFrequencyPer = (string)sp.Element("billingFrequencyPer"),
                                                BillingFrequencyUnit = (string)sp.Element("billingFrequencyUnit"),
                                                BillingFrequencyQuantity = (string)sp.Element("billingFrequencyQuantity"),
                                                SetupChargeCode = (string)sp.Element("setupChargeCode"),
                                                SetupChargeAmount = (float)sp.Element("setupChargeAmount"),
                                                RecurringChargeCode = (string)sp.Element("recurringChargeCode"),
                                                RecurringChargeAmount = (float)sp.Element("recurringChargeAmount"),
                                                CreatedDateTime = (DateTime)sp.Element("createdDatetime"),
                                                PlanItems = getPlanItemsList(sp.Element("items"))
                                            }).ToList();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return subscriptionPlanList;
        }

        /// <summary>
        /// Get a list of subscriptions based on a subscriptions XML node in a XElement object
        /// </summary>
        /// <param name="subscriptions">A XElement object representing a node of XML subscriptions</param>
        /// <returns>A list of Subscription objects</returns>
        private List<Subscription> getSubscriptionList(XElement subscriptions)
        {
            List<Subscription> subscriptionList = new List<Subscription>();

            try
            {
                if (subscriptions != null && subscriptions.Descendants("subscriptions") != null)
                {
                    subscriptionList = (from s in subscriptions.Descendants("subscription")
                                        select new Subscription
                                        {
                                            ID = (Guid)s.Attribute("id"),
                                            SubscriptionsPlans = getSubscriptionPlanList(s.Element("plans")),
                                            GatewayToken = (string)s.Element("gatewayToken"),
                                            CCFirstName = (string)s.Element("ccFirstName"),
                                            CCLastName = (string)s.Element("ccLastName"),
                                            CCZip = (string)s.Element("ccZip"),
                                            CCType = (string)s.Element("ccType"),
                                            CCLastFour = string.IsNullOrEmpty(s.Element("ccLastFour").Value) ? (int?)null : (int?)s.Element("ccLastFour"),
                                            CCExpirationDate = string.IsNullOrEmpty(s.Element("ccExpirationDate").Value) ? (DateTime?)null : (DateTime?)s.Element("ccExpirationDate"),
                                            CanceledDateTime = string.IsNullOrEmpty(s.Element("canceledDatetime").Value) ? (DateTime?)null : (DateTime?)s.Element("canceledDatetime"),
                                            CreatedDateTime = (DateTime)s.Element("createdDatetime"),
                                            SubscriptionItems = getSubscriptionItems(s.Element("items")),
                                            Invoices = getInvoiceList(s.Element("invoices")),
                                        }).ToList();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return subscriptionList;
        }
    }
}
