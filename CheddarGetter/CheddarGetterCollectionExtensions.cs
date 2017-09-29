using CheddarGetter.Models;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CheddarGetter
{
    /// <summary>
    /// Interaface for set CheddarGetterConfig properties for Dependancy Injection on the Startup.cs of the Application
    /// </summary>
    public static class CheddarGetterCollectionExtensions
    {

        /// <summary>
        /// Handles the GET requests
        /// </summary>
        /// <param name="productCode">The rest of the url for the request</param>
        /// <param name="username">The User ChettarGetter User Name (typically the the user email)</param>
        /// <param name="password">The User ChettarGetter Password</param>

        public static IServiceCollection AddCheddarGetterService(this IServiceCollection collection,
    Action<CheddarGetterConfig> setupAction)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (setupAction == null) throw new ArgumentNullException(nameof(setupAction));

            collection.Configure(setupAction);
            return collection.AddTransient<ICheddarGetterService, CheddarGetterService>();
        }
    }
}
