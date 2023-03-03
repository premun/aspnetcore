// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Http.Generators;

internal static class RequestDelegateGeneratorSources
{
    private const string SourceHeader = """
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable
""";

    public static string GeneratedCodeAttribute => $@"[System.CodeDom.Compiler.GeneratedCodeAttribute(""{typeof(RequestDelegateGeneratorSources).Assembly.FullName}"", ""{typeof(RequestDelegateGeneratorSources).Assembly.GetName().Version}"")]";

    public static string GetGeneratedRouteBuilderExtensionsSource(string genericThunks, string thunks, string endpoints) => $$"""
{{SourceHeader}}

namespace Microsoft.AspNetCore.Builder
{
    {{GeneratedCodeAttribute}}
    internal class SourceKey
    {
        public string Path { get; init; }
        public int Line { get; init; }

        public SourceKey(string path, int line)
        {
            Path = path;
            Line = line;
        }
    }

{{GetEndpoints(endpoints)}}
}

namespace Microsoft.AspNetCore.Http.Generated
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.IO;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.Routing.Patterns;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Metadata;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Primitives;

    using MetadataPopulator = System.Func<System.Reflection.MethodInfo, Microsoft.AspNetCore.Http.RequestDelegateFactoryOptions?, Microsoft.AspNetCore.Http.RequestDelegateMetadataResult>;
    using RequestDelegateFactoryFunc = System.Func<System.Delegate, Microsoft.AspNetCore.Http.RequestDelegateFactoryOptions, Microsoft.AspNetCore.Http.RequestDelegateMetadataResult?, Microsoft.AspNetCore.Http.RequestDelegateResult>;

    file static class GeneratedRouteBuilderExtensionsCore
    {
{{GetGenericThunks(genericThunks)}}
{{GetThunks(thunks)}}

        private static EndpointFilterDelegate BuildFilterDelegate(EndpointFilterDelegate filteredInvocation, EndpointBuilder builder, MethodInfo mi)
        {
            var routeHandlerFilters =  builder.FilterFactories;
            var context0 = new EndpointFilterFactoryContext
            {
                MethodInfo = mi,
                ApplicationServices = builder.ApplicationServices,
            };
            var initialFilteredInvocation = filteredInvocation;
            for (var i = routeHandlerFilters.Count - 1; i >= 0; i--)
            {
                var filterFactory = routeHandlerFilters[i];
                filteredInvocation = filterFactory(context0, filteredInvocation);
            }
            return filteredInvocation;
        }

        private static Task ExecuteObjectResult(object? obj, HttpContext httpContext)
        {
            if (obj is IResult r)
            {
                return r.ExecuteAsync(httpContext);
            }
            else if (obj is string s)
            {
                return httpContext.Response.WriteAsync(s);
            }
            else
            {
                return httpContext.Response.WriteAsJsonAsync(obj);
            }
        }

        private static Func<HttpContext, StringValues> ResolveFromRouteOrQuery(string parameterName, IEnumerable<string>? routeParameterNames)
        {
            return routeParameterNames?.Contains(parameterName, StringComparer.OrdinalIgnoreCase) == true
                ? (httpContext) => new StringValues((string?)httpContext.Request.RouteValues[parameterName])
                : (httpContext) => httpContext.Request.Query[parameterName];
        }

        private static async ValueTask<(bool, T?)> TryResolveBody<T>(HttpContext httpContext, bool allowEmpty)
        {
            var feature = httpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpRequestBodyDetectionFeature>();

            if (feature?.CanHaveBody == true)
            {
                if (!httpContext.Request.HasJsonContentType())
                {
                    httpContext.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                    return (false, default);
                }
                try
                {
                    var bodyValue = await httpContext.Request.ReadFromJsonAsync<T>();
                    if (!allowEmpty && bodyValue == null)
                    {
                        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                        return (false, bodyValue);
                    }
                    return (true, bodyValue);
                }
                catch (IOException)
                {
                    return (false, default);
                }
                catch (System.Text.Json.JsonException)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return (false, default);
                }
            }
            return (false, default);
        }
    }
}
""";
    private static string GetGenericThunks(string genericThunks) => genericThunks != string.Empty ? $$"""
        private static class GenericThunks<T>
        {
            public static readonly Dictionary<(string, int), (MetadataPopulator, RequestDelegateFactoryFunc)> map = new()
            {
                {{genericThunks}}
            };
        }

        internal static RouteHandlerBuilder MapCore<T>(
            this IEndpointRouteBuilder routes,
            string pattern,
            Delegate handler,
            IEnumerable<string> httpMethods,
            string filePath,
            int lineNumber)
        {
            var (populateMetadata, createRequestDelegate) = GenericThunks<T>.map[(filePath, lineNumber)];
            return RouteHandlerServices.Map(routes, pattern, handler, httpMethods, populateMetadata, createRequestDelegate);
        }
""" : string.Empty;

    private static string GetThunks(string thunks) => thunks != string.Empty ? $$"""
        private static readonly Dictionary<(string, int), (MetadataPopulator, RequestDelegateFactoryFunc)> map = new()
        {
{{thunks}}
        };

        internal static RouteHandlerBuilder MapCore(
            this IEndpointRouteBuilder routes,
            string pattern,
            Delegate handler,
            IEnumerable<string> httpMethods,
            string filePath,
            int lineNumber)
        {
            var (populateMetadata, createRequestDelegate) = map[(filePath, lineNumber)];
            return RouteHandlerServices.Map(routes, pattern, handler, httpMethods, populateMetadata, createRequestDelegate);
        }
""" : string.Empty;

    private static string GetEndpoints(string endpoints) => endpoints != string.Empty ? $$"""
    // This class needs to be internal so that the compiled application
    // has access to the strongly-typed endpoint definitions that are
    // generated by the compiler so that they will be favored by
    // overload resolution and opt the runtime in to the code generated
    // implementation produced here.
    {{GeneratedCodeAttribute}}
    internal static class GenerateRouteBuilderEndpoints
    {
        private static readonly string[] GetVerb = new[] { global::Microsoft.AspNetCore.Http.HttpMethods.Get };
        private static readonly string[] PostVerb = new[] { global::Microsoft.AspNetCore.Http.HttpMethods.Post };
        private static readonly string[] PutVerb = new[]  { global::Microsoft.AspNetCore.Http.HttpMethods.Put };
        private static readonly string[] DeleteVerb = new[] { global::Microsoft.AspNetCore.Http.HttpMethods.Delete };
        private static readonly string[] PatchVerb = new[] { global::Microsoft.AspNetCore.Http.HttpMethods.Patch };

        {{endpoints}}
    }
""" : string.Empty;
}
