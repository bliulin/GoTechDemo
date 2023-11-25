
using System;
using Microsoft.Extensions.Resilience;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using System.Net;
using Products.DataModels;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Reflection.Metadata.Ecma335;

namespace Orders.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            IHttpClientBuilder httpClientBuilder = builder.Services
                .AddHttpClient<IProductsClient, ProductsClient>();

            //httpClientBuilder.AddStandardHedgingHandler();
            //httpClientBuilder.AddStandardResilienceHandler();

            //httpClientBuilder.AddStandardHedgingHandler(static (IRoutingStrategyBuilder builder) =>
            //{
            //    // Hedging allows sending multiple concurrent requests
            //    builder.ConfigureOrderedGroups(static options =>
            //    {
            //        options.Groups.Add(new UriEndpointGroup()
            //        {
            //            Endpoints =
            //{
            //    // Imagine a scenario where 3% of the requests are 
            //    // sent to the experimental endpoint.
            //    new() { Uri = new("https://example.net/api/experimental"), Weight = 3 },
            //    new() { Uri = new("https://example.net/api/stable"), Weight = 97 }
            //}
            //        });
            //    });
            //});

            httpClientBuilder.AddResilienceHandler(
            "CustomPipeline",
            static builder =>
            {
                // See: https://www.pollydocs.org/strategies/retry.html
                builder
                //.AddRetry(new HttpRetryStrategyOptions
                //{
                //    // Customize and configure the retry logic.
                //    BackoffType = DelayBackoffType.Exponential,
                //    MaxRetryAttempts = 3,
                //    UseJitter = true
                //})
                // See: https://www.pollydocs.org/strategies/circuit-breaker.html
                .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    // Customize and configure the circuit breaker logic.
                    SamplingDuration = TimeSpan.FromSeconds(10),
                    FailureRatio = 0.5,
                    MinimumThroughput = 3,
                    BreakDuration = TimeSpan.FromSeconds(10),
                    ShouldHandle = static args =>
                    {
                        if (args.Outcome.Exception is HttpRequestException)
                        {
                            return ValueTask.FromResult(true);
                        }
                        return ValueTask.FromResult(args is
                        {
                            Outcome.Result.StatusCode:
                                HttpStatusCode.RequestTimeout or
                                    HttpStatusCode.TooManyRequests or
                                    HttpStatusCode.InternalServerError
                        });
                    },
                    OnOpened = (context) =>
                    {
                        Console.WriteLine("circuit opened");
                        return ValueTask.CompletedTask;
                    },
                    OnClosed = (context) =>
                    {
                        Console.WriteLine("circuit closed");
                        return ValueTask.CompletedTask;
                    },
                    OnHalfOpened = (context) =>
                    {
                        Console.WriteLine("circuit half opened");
                        return ValueTask.CompletedTask;
                    }

                })
                // See: https://www.pollydocs.org/strategies/timeout.html
                .AddTimeout(TimeSpan.FromSeconds(5))
                .AddFallback(new Polly.Fallback.FallbackStrategyOptions<HttpResponseMessage>()
                {
                    //ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    //.Handle<Exception>()
                    //.HandleResult(r => r is null),
                    ShouldHandle = new Func<Polly.Fallback.FallbackPredicateArguments<HttpResponseMessage>, ValueTask<bool>>(
                        (arg) => 
                        {
                            bool ok = true;
                            if (arg.Outcome.Result != null)
                            {
                                ok = arg.Outcome.Result.IsSuccessStatusCode;
                            }
                            else if (arg.Outcome.Exception != null)
                            {
                                ok = false;
                            }

                            return ValueTask.FromResult(!ok);
                        }),
                    FallbackAction = static args =>
                    {
                        var products = new Product[]
                        {
                            new Product("332", "value from cache", 22, 10)
                        };
                        HttpResponseMessage httpResponseMessage = new(HttpStatusCode.Accepted);
                        httpResponseMessage.Content = new StringContent(JsonSerializer.Serialize(products));
                        return Outcome.FromResultAsValueTask(httpResponseMessage);
                    }
                });;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapPost("order/{productId}/{count}", async (HttpContext context, string productId, int count, IProductsClient productsClient) =>
            {
                await productsClient.RemoveFromStock(productId, count);
                return Results.Ok();
            })
            .WithName("OrderProducts")
            .WithOpenApi();

            app.MapGet("order/available-products", async (HttpContext httpContext, IProductsClient productsClient) =>
            {
                var list = await productsClient.GetAvailableProducts();
                return Results.Ok(list);
            })
            .WithName("products-stock")
            .WithOpenApi();

            app.Run();
        }
    }
}