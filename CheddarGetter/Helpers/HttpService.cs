using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using CheddarGetter.Models;
using System.Linq;

namespace CheddarGetter.Helpers
{
    /// <summary>
    /// Public interaface for all functions in the service
    /// </summary>
    public interface IHttpService
    {
        string getRequest(string urlPath);
        Task<string> postRequest(string urlPath, string postParams);
        Task<XmlDocument> XmlPostRequest(string urlPath, string postParams);
    }

    public class HttpService : IHttpService
    {
        private readonly string _username;
        private readonly string _password;
        private readonly string baseUrl = "https://www.getcheddar.com/xml";

        /// <summary>
        /// Constructor
        /// </summary>
        public HttpService(string username, string password)
        {
            _username = username;
            _password = password;
        }

        /// <summary>
        /// Handles the GET requests
        /// </summary>
        /// <param name="urlPath">The rest of the url for the request</param>
        /// <returns>A string of XML data that is returned for the request</returns>
        public string getRequest(string urlPath)
        {
            string result = "";

            try
            {
                HttpWebRequest request = WebRequest.Create(baseUrl + urlPath) as HttpWebRequest;

                //Add authentication
                request.Credentials = new NetworkCredential(_username, _password);

                // Get response  

                using (WebResponse response = request.GetResponseAsync().Result)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    result = reader.ReadToEnd();
                }

            }
            catch (WebException wex)
            {
                HttpWebResponse response = wex.Response as HttpWebResponse;

                XDocument xDoc = XDocument.Load(response.GetResponseStream());
                List<CGError> errorList = new List<CGError>();
                errorList = (from e in xDoc.Descendants("errors")
                             select new CGError
                             {
                                 ID = (string)e.Attribute("id"),
                                 Code = (string)e.Attribute("code"),
                                 AuxCode = (string)e.Attribute("auxCode"),
                                 Message = (string)e.Element("error")
                             }).ToList();

                foreach (CGError e in errorList)
                {
                    result = "Error:" + e.Message.ToString();
                }

                //throw wex;
            }

            return result;
        }

        /// <summary>
        /// Handles the POST request
        /// </summary>
        /// <param name="urlPath">The rest of the URL for the request</param>
        /// <param name="postParams">Any additional parameters for the POST are added here</param>
        /// <returns>A string of XML data that is returned for the request</returns>
        public async Task<string> postRequest(string urlPath, string postParams)
        {
            string result = "";
            try
            {
                //Create request
                WebRequest request = WebRequest.Create(baseUrl + urlPath);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Credentials = new NetworkCredential(_username, _password);

                //Set data in request
                Stream dataStream = await request.GetRequestStreamAsync();
                byte[] bytes = Encoding.UTF8.GetBytes(removeFirstAnd(postParams));
                dataStream.Write(bytes, 0, bytes.Length);

                //Get the response
                WebResponse wr = await request.GetResponseAsync();

                Stream receiveStream = wr.GetResponseStream();
                StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
                result = reader.ReadToEnd();
            }
            catch (WebException wex)
            {
                HttpWebResponse response = wex.Response as HttpWebResponse;

                XDocument xDoc = XDocument.Load(response.GetResponseStream());
                List<CGError> errorList = new List<CGError>();
                errorList = (from e in xDoc.Descendants("errors")
                             select new CGError
                             {
                                 ID = (string)e.Attribute("id"),
                                 Code = (string)e.Attribute("code"),
                                 AuxCode = (string)e.Attribute("auxCode"),
                                 Message = (string)e.Element("error")
                             }).ToList();

                foreach (CGError e in errorList)
                {
                    result = "Error:" + e.Message.ToString();
                }

                //throw wex;
            }
            return result;
        }

        /// <summary>
        /// Handles the POST request
        /// </summary>
        /// <param name="urlPath">The rest of the URL for the request</param>
        /// <param name="postParams">Any additional parameters for the POST are added here</param>
        /// <returns>XML data/document tyupe that is returned for the request</returns>
        public async Task<XmlDocument> XmlPostRequest(string urlPath, string postParams)
        {
            try
            {
                //Create request
                WebRequest request = WebRequest.Create(baseUrl + urlPath);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Credentials = new NetworkCredential(_username, _password);

                //Set data in request
                Stream dataStream = await request.GetRequestStreamAsync();
                byte[] bytes = Encoding.UTF8.GetBytes(removeFirstAnd(postParams));
                dataStream.Write(bytes, 0, bytes.Length);

                //Get the response
                WebResponse wr = await request.GetResponseAsync();
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(wr.GetResponseStream());
                return (xmlDoc);
            }
            catch (WebException wex)
            {
                HttpWebResponse response = wex.Response as HttpWebResponse;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(response.GetResponseStream());
                return (xmlDoc);

                //throw wex;
            }
        }

        /// <summary>
        /// Will check and format remove the first "and" symbol at the begining of a QueryString
        /// </summary>
        /// <param name="queryString">The complete QueryString with all parameters</param>
        /// <returns>A formatted QueryString with the first "and" symbol removed</returns>
        private string removeFirstAnd(string queryString)
        {
            if (queryString.StartsWith("&"))
            {
                return queryString.TrimStart('&');
            }
            return queryString;
        }
    }
}
