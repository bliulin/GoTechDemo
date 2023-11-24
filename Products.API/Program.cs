
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data.Common;

namespace Products.API
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

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapGet("/product-stock", (HttpContext httpContext) =>
            {
                var products = DataLayer.GetStock();
                return Results.Ok(products);
            })
            .WithName("GetStock")
            .WithOpenApi();

            app.MapGet("/product-stock/{id}", (HttpContext httpContext, string id) =>
            {
                var product = DataLayer.GetStock().First(x => x.Id == id);
                if (product == null)
                {
                    return Results.NotFound($"did not find product with id {id}");
                }

                return Results.Ok(product);
            })
            .WithName("GetProductStock")
            .WithOpenApi();

            app.MapDelete("/product-stock/{id}/{count}", (HttpContext httpContext, string id, int count) =>
            {
                int code = DataLayer.RemoveFromStock(id, count);
                if (code != 0)
                {
                    return Results.BadRequest(code);
                }
                return Results.Ok();
            })
            .WithName("RemoveFromStock")
            .WithOpenApi();

            app.MapPost("/break", (HttpContext httpContext) => 
            {
                DataLayer.Break();
            }).WithName("BreakAPI").WithOpenApi();

            app.MapPost("/repair", (HttpContext httpContext) =>
            {
                DataLayer.Repair();
            }).WithName("RepairAPI").WithOpenApi();

            app.Run();
        }
    }
}