using System;
using System.Web;
using System.Collections.Specialized;
using System.Net;

namespace CheddarGetter.Helpers
{
    /// <summary>
    /// Interaface for all string functions used in the services
    /// </summary>
    public class FormatFunctions
    {
        /// <summary>
        /// Will format the string into a acceptable month date format for CG
        /// </summary>
        /// <param name="ccMonth">The month in a string to be parsed</param>
        /// <returns>A formatted month string</returns>
        public static string formatMonth(string ccMonth)
        {
            string returnMonth = ccMonth;

            if (ccMonth.Length == 1)
            {
                returnMonth = "0" + ccMonth;
            }

            return returnMonth;
        }

        /// <summary>
        /// Will loop through a string all additional meta data parameters and create a properly formated QueryString
        /// </summary>
        /// <param name="query">string value from the AdditionalMetaDataParams in QueryString format</param>
        /// <returns>A URL Encoded QueryString of additional meta data parametes</returns>
        public static string addMetaDataParams(string query)
        {
            var queryString = string.Empty;

            if (!string.IsNullOrEmpty(query))
            {
                var paramCount = 0;
                NameValueCollection queryStringParams = HttpUtility.ParseQueryString(query);
                foreach (String key in queryStringParams)
                {
                    paramCount += 1;
                    queryString += "&" + key + "=" + WebUtility.UrlEncode(queryStringParams[key]);
                }
            }

            return queryString;
        }

        /// <summary>
        /// Will generate the QueryString format for each parameter and value 
        /// </summary>
        /// <param name="key">The parameter key/name to be passed to CheddarGetter</param>
        /// <param name="value">The parameter value</param>
        /// <returns>A formatted month string</returns>
        public static string addParam(string key, string value)
        {
            return (!string.IsNullOrEmpty(value)) ? $"&{key}={WebUtility.UrlEncode(value)}" : string.Empty;
        }
    }
}
