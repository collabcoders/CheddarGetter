using System;
using System.Text;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace CheddarGetter.Helpers
{
    /// <summary>
    /// Public interaface for all functions in the service
    /// </summary>
    public interface IHttpService
    {
        string getRequest(string urlPath);
        Task<string> postRequest(string urlPath, string postParams);
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
            string result = string.Empty;
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
            catch (Exception ex)
            {
                throw ex;
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
            string result = string.Empty;
            try
            {
                //Create request
                WebRequest request = WebRequest.Create(baseUrl + urlPath);
                request.Credentials = new NetworkCredential(_username, _password);
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";

                //Set data in request
                var finalParams = removeFirstAnd(postParams);
                byte[] bytes = Encoding.UTF8.GetBytes(finalParams);
                request.ContentLength = bytes.Length;

                using (Stream dataStream = await request.GetRequestStreamAsync())
                {
                    dataStream.Write(bytes, 0, bytes.Length);

                    //Get the response
                    using (WebResponse wr = await request.GetResponseAsync())
                    {
                        using (StreamReader reader = new StreamReader(wr.GetResponseStream(), Encoding.UTF8))
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
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
