using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using todozulu.common.Models;
using todozulu.common.Responses;
using todozulu.Function.Entities;

namespace todozulu.Function.Functions
{
    public static class TodoApi
    {
        [FunctionName(nameof(CreateTodo))]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation("Recieved a new todo");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Todo todo = JsonConvert.DeserializeObject<Todo>(requestBody);

            if (string.IsNullOrEmpty(todo?.TaskDescription))
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must have a TaskDescription."
                });
            }
            //Create new input to table

            TodoEntity todoEntity = new TodoEntity
            {
                CreatedTime = DateTime.UtcNow, //London Time "Utc"
                ETag = "*", //Horizontal Partition
                IsCompleted = false,
                PartitionKey = "TODO", //Table where to save
                RowKey = Guid.NewGuid().ToString(), //Table index, Guid "alphanumeric password that does not repeat"
                TaskDescription = todo.TaskDescription

            };

            TableOperation addOperation = TableOperation.Insert(todoEntity); //Save entity
            await todoTable.ExecuteAsync(addOperation);

            string message = "New todo stored in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = todoEntity
            });
        }
    }
}
