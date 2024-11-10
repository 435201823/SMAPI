using System;
using Pathoschild.Http.Client;

namespace StardewModdingAPI.Toolkit.Framework.Clients
{
    /// <summary>The HTTP cache headers set by a remote server.</summary>
    public record ApiCacheHeaders
    {
        /*********
        ** Fields
        *********/
        /// <summary>When the server's data was last updated.</summary>
        public readonly DateTimeOffset LastModified;

        /// <summary>The entity tag which represents the current version of the server's data.</summary>
        public readonly string EntityTag;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="lastModified">When the server's data was last updated.</param>
        /// <param name="entityTag">The entity tag which represents the current version of the server's data.</param>
        public ApiCacheHeaders(DateTimeOffset lastModified, string entityTag)
        {
            this.LastModified = lastModified;
            this.EntityTag = entityTag;
        }

        /// <summary>Read the required cache headers from an API response.</summary>
        /// <param name="response">The API response whose headers to read.</param>
        /// <exception cref="InvalidOperationException">The response is missing one or more of the required HTTP headers (<c>ETag</c> and <c>Last-Modified</c>).</exception>
        public static ApiCacheHeaders FromResponse(IResponse response)
        {
            return new ApiCacheHeaders(
                lastModified: response.Message.Content.Headers.LastModified ?? throw new InvalidOperationException("The API response doesn't include the required Last-Modified header."),
                entityTag: response.Message.Headers.ETag?.Tag ?? throw new InvalidOperationException("The API response doesn't include the required ETag header.")
            );
        }
    }
}
