using AzureFunctionSwagger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using ProductWebAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProductWebAPI
{
    public static class ProductFunction
    {
        // Get all products
        [FunctionName("GetAllProducts")]
        [OpenApiOperation(operationId: "GetAllProducts", tags: new[] { "Products" })]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<Product>))]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Product not found")]
        public static async Task<IActionResult> GetAllProducts(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "products")] HttpRequestMessage req)
        {
            var products = InMemoryProductService.GetAll();
            return new OkObjectResult(products);
        }

        // Get a product by ID
        [FunctionName("GetProductById")]
        [OpenApiOperation(operationId: "GetProductById", tags: new[] { "Products" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = false, Type = typeof(int), Description = "Product ID")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Product))]
        public static async Task<IActionResult> GetProductById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "products/{id:int}")] HttpRequestMessage req,
            int id)
        {
            var product = InMemoryProductService.GetById(id);
            return product != null ? new OkObjectResult(product) : new NotFoundResult();
        }

        // Create a new product
        [FunctionName("CreateProduct")]
        [OpenApiOperation(operationId: "CreateProduct", tags: new[] { "Products" })]
        [OpenApiRequestBody("application/json", typeof(Product))]
        [OpenApiResponseWithBody(HttpStatusCode.Created, "application/json", typeof(Product))]
        public static async Task<IActionResult> AddProduct(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "products")] HttpRequestMessage req)
        {
            var content = await req.Content.ReadAsStringAsync();
            var newProduct = JsonConvert.DeserializeObject<Product>(content);

            if (newProduct == null)
            {
                return new BadRequestResult();
            }

            InMemoryProductService.Add(newProduct);
            return new CreatedAtRouteResult("GetProductById", new { id = newProduct.ProductID }, newProduct);
        }

        [FunctionName("UpdateProduct")]
        [OpenApiOperation(operationId: "UpdateProduct", tags: new[] { "Products" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "Product ID")]
        [OpenApiRequestBody("application/json", typeof(Product))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Product))]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Product not found")]
        public static async Task<IActionResult> UpdateProduct(
                [HttpTrigger(AuthorizationLevel.Function, "put", Route = "products/{id:int}")] HttpRequestMessage req, int id)
        {
            var content = await req.Content.ReadAsStringAsync();
            var updatedProduct = JsonConvert.DeserializeObject<Product>(content);

            if (updatedProduct == null)
            {
                return new BadRequestObjectResult("Invalid product data.");
            }

            updatedProduct.ProductID = id;

            var success = InMemoryProductService.Update(id, updatedProduct);

            return success ? new OkObjectResult(updatedProduct) : new NotFoundResult();
        }


        // Delete a product
        [FunctionName("DeleteProduct")]
        [OpenApiOperation(operationId: "DeleteProduct", tags: new[] { "Products" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "Product ID")]

        public static async Task<IActionResult> DeleteProduct(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "products/{id:int}")] HttpRequestMessage req,
            int id)
        {
            var success = InMemoryProductService.Delete(id);
            return success ? new NoContentResult() : new NotFoundResult();
        }
    }
}
