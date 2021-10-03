using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCorePollyExamples.Configurations;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Serilog;
using System;
using System.Net.Http;

namespace NetCorePollyExamples.Extensions
{
    public static class HttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddPolicyRetryAndCircuitBreaker(this IHttpClientBuilder builder, IConfiguration configuration, string name)
        {
            var resilienceConfiguration = new ResilienceConfiguration()
            {
                Retry = configuration.GetSection($"PolicyRetry:{name}").Get<RetryPolicyConfiguration>(),
                CircuitBreaker = configuration.GetSection($"PolicyCircuitBreaker:{name}").Get<CircuitBreakerPolicyConfiguration>()
            };
            return builder
               .AddRetryHandler(resilienceConfiguration?.Retry)
               .AddCircuitBreakerHandler(resilienceConfiguration?.CircuitBreaker)
               .AddTimeoutHandler(resilienceConfiguration?.Retry);
        }


        public static IHttpClientBuilder AddPolicyRetryAndCircuitBreaker(this IHttpClientBuilder builder, ResilienceConfiguration resilienceConfiguration)
           => builder
               .AddRetryHandler(resilienceConfiguration?.Retry)
               .AddCircuitBreakerHandler(resilienceConfiguration?.CircuitBreaker)
               .AddTimeoutHandler(resilienceConfiguration?.Retry);

        private static IHttpClientBuilder AddCircuitBreakerHandler(this IHttpClientBuilder builder, CircuitBreakerPolicyConfiguration circuitBreakerPolicyConfiguration)
        {
            if (circuitBreakerPolicyConfiguration == null || !circuitBreakerPolicyConfiguration.Enabled)
                return builder;

            Log.Logger.Information("AddCircuitBreakerHandler - {@CircuitBreakerPolicySettings} {@Service}", circuitBreakerPolicyConfiguration, builder.Name);

            var addAdvancedCircuit = circuitBreakerPolicyConfiguration.Advanced?.FailureThreshold > 0;
            var addBasicCircuit = circuitBreakerPolicyConfiguration.Basic?.NumberOfExceptionAllowedBeforeOpenCircuit > 0;

            if (addAdvancedCircuit)
                builder.AddPolicyHandler(GetAdvancedCircuitBreakerPolicy(circuitBreakerPolicyConfiguration.Advanced));
            else if (addBasicCircuit)
                builder.AddPolicyHandler(GetBasicCircuitBreakerPolicy(circuitBreakerPolicyConfiguration.Basic));

            return builder;
        }

        private static IHttpClientBuilder AddRetryHandler(this IHttpClientBuilder builder, RetryPolicyConfiguration policyRetry)
        {
            if (policyRetry == null || policyRetry.RetryCount == 0)
                return builder;

            Log.Logger.Information("AddRetryHandler - {@PolicyRetry} {@Service}", policyRetry, builder.Name);

            return builder.AddPolicyHandler(GetRetryPolicy(policyRetry));
        }

        private static IHttpClientBuilder AddTimeoutHandler(this IHttpClientBuilder builder, RetryPolicyConfiguration policyRetry)
        {
            if (policyRetry == null || policyRetry.TimeoutMilliseconds == 0)
                return builder;

            Log.Logger.Information("AddTimeoutHandler - {@TimeoutMilliseconds} {@Service}", policyRetry.TimeoutMilliseconds, builder.Name);

            return builder.AddPolicyHandler(GetTimeoutPolicy(policyRetry));
        }

        private static IAsyncPolicy<HttpResponseMessage> GetAdvancedCircuitBreakerPolicy(AdvancedCircuitBreakerPolicyConfiguration circuitBreakerPolicy)
        {
            return HttpPolicyExtensions
                          .HandleTransientHttpError()
                          .Or<Exception>()
                          .AdvancedCircuitBreakerAsync(circuitBreakerPolicy.FailureThreshold,
                                                      TimeSpan.FromSeconds(circuitBreakerPolicy.SamplingDurationInSeconds),
                                                      circuitBreakerPolicy.MinimumThroughput,
                                                      TimeSpan.FromSeconds(circuitBreakerPolicy.DurationOfBreakInSeconds),
                                                      onBreak: (res, ts, ctx) =>
                                                      {
                                                          GerarLogPolly("CircuitBreaker onBreak", ctx, res?.Exception);
                                                      },
                                                      onReset: (ctx) =>
                                                      {
                                                          GerarLogPolly("CircuitBreaker onReset", ctx);
                                                      },
                                                      onHalfOpen: () => { });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetBasicCircuitBreakerPolicy(BasicCircuitBreakerPolicyConfiguration circuitBreakerPolicy)
        {
            return HttpPolicyExtensions
                          .HandleTransientHttpError()
                          .Or<Exception>()
                          .CircuitBreakerAsync(circuitBreakerPolicy.NumberOfExceptionAllowedBeforeOpenCircuit,
                                                TimeSpan.FromSeconds(circuitBreakerPolicy.DurationOfBreakInSeconds),
                                                onBreak: (res, ts, ctx) =>
                                                {
                                                    GerarLogPolly("CircuitBreaker onBreak", ctx, res?.Exception);
                                                },
                                                onReset: (ctx) =>
                                                {
                                                    GerarLogPolly("CircuitBreaker onReset", ctx);
                                                },
                                                onHalfOpen: () => { });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(RetryPolicyConfiguration policyRetryConfiguration)
            => HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(policyRetryConfiguration.RetryCount,
                                    _ => TimeSpan.FromMilliseconds(policyRetryConfiguration.RetryDelayMilliseconds),
                                    onRetry: (res, ts, ctx) =>
                                    {
                                        GerarLogPolly("Retry onRetry", ctx, res?.Exception);
                                    });

        private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(RetryPolicyConfiguration policyRetryConfiguration)
            => Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(policyRetryConfiguration.TimeoutMilliseconds));

        private static void GerarLogPolly(string template, Context ctx, Exception exception = null)
        {
            //var fullTemplate =  $"{template}  {{Host}} : {{Url}} : {{Method}}";
            //var uri = (Uri) ctx.GetValue(PollyContextProperties.Uri);
            //var method = (string) ctx.GetValue(PollyContextProperties.Method);

            //if (exception == null)
            //    Log.Logger.Information(fullTemplate, uri.Host, uri.AbsoluteUri, method);
            //else
            //    Log.Logger.Error(exception, fullTemplate, uri.Host, uri.AbsoluteUri, method);
        }

        private static object GetValue(this Context context, string key)
        {
            if (context.TryGetValue(key, out object valor))
                return valor;

            return string.Empty;
        }
    }
}
