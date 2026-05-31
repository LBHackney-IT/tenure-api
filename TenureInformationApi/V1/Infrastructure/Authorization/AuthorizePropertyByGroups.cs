using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Hackney.Core.JWT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.IO;
using System.Text;
using Hackney.Core;

namespace TenureInformationApi.V1.Infrastructure.Authorization
{
    public class AuthorizePropertyByGroups : TypeFilterAttribute
    {
        /// <summary>
        /// Authorise this endpoint using permitted groups
        /// </summary>
        /// <param name="propertyName">
        /// The property to check for existence
        /// </param>
        /// <param name="permittedGroupsVariable">
        /// The name of the environment variable that stores the permitted groups for the property
        /// </param>
        public AuthorizePropertyByGroups(string propertyName, string permittedGroupsVariable) : base(typeof(TokenGroupsPropertyFilter))
        {
            Arguments = new object[] { propertyName, permittedGroupsVariable };
        }
    }

    public class TokenGroupsPropertyFilter : IAsyncAuthorizationFilter
    {
        private readonly ISet<string> _requiredGoogleGroups;
        private readonly ITokenFactory _tokenFactory;
        private readonly string _jsonPropertyName;

        public TokenGroupsPropertyFilter(ITokenFactory tokenFactory, string propertyName, string permittedGroupsVariable)
        {
            _tokenFactory = tokenFactory;
            _jsonPropertyName = propertyName?.ToCamelCase();

            var requiredGooglepermittedGroupsVariable = Environment.GetEnvironmentVariable(permittedGroupsVariable);
            if (requiredGooglepermittedGroupsVariable is null) throw new Exception($"Environment variable cannot be null: {nameof(permittedGroupsVariable)}");

            _requiredGoogleGroups = requiredGooglepermittedGroupsVariable
                .Split(',', StringSplitOptions.RemoveEmptyEntries) // Note: Env variable must not have spaces after commas
                .ToHashSet();
        }

        private JsonSerializerOptions CreateJsonOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        private bool CanEditProperty(string[] groups) => groups?.Any(g => _requiredGoogleGroups.Contains(g)) ?? false;

        private async Task<string> ReadStreamToEndAndKeepOpen(Stream stream)
        {
            string returnValue;

            using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
            {
                returnValue = await reader.ReadToEndAsync();
            }
            stream.Position = 0;

            return returnValue;
        }

        private async Task<bool> HasProperty(AuthorizationFilterContext context)
        {
            var updateJson = await ReadStreamToEndAndKeepOpen(context.HttpContext.Request.Body);

            var returnValue = false;
            if (!string.IsNullOrWhiteSpace(updateJson))
            {
                var updateDic = JsonSerializer.Deserialize<Dictionary<string, object>>(updateJson, CreateJsonOptions());
                returnValue = updateDic.ContainsKey(_jsonPropertyName);
            }
            return returnValue;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var token = _tokenFactory.Create(context.HttpContext.Request.Headers);
            if (token?.Groups is null ||
                (!CanEditProperty(token.Groups) && await HasProperty(context)))
            {
                context.Result =
                    new UnauthorizedObjectResult($"User {token?.Name} is not authorized to access the {_jsonPropertyName} property on this endpoint.");
            }
        }

    }
}
