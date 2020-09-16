using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Batch.Common;
using System.Collections.Generic;
using System.Diagnostics;

namespace AzFunctionApp1
{
    public class AzFunction
    {
        private readonly IConfiguration config;
        private readonly ILogger<AzFunction> logger;

        private string BatchAccountUrl;
        private string BatchAccountName;
        private string BatchAccountKey;

        public AzFunction(IConfiguration config, ILogger<AzFunction> logger)
        {
            this.config = config;
            this.logger = logger;

            BatchAccountUrl = config.GetValue<string>("BatchAccountUrl");
            BatchAccountName = config.GetValue<string>("BatchAccountName");
            BatchAccountKey = config.GetValue<string>("BatchAccountKey");
        }

        [FunctionName("Function1")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, 
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(BatchAccountUrl, BatchAccountName, BatchAccountKey);

            using (BatchClient batchClient = BatchClient.Open(cred))
            {
                try
                {
                    Console.WriteLine("Sample start: {0}", DateTime.Now);
                    Stopwatch timer = new Stopwatch();
                    timer.Start();

                    string jobId = "Python-" + Guid.NewGuid().ToString();

                    CloudJob job = batchClient.JobOperations.CreateJob();
                    job.Id = jobId;
                    job.PoolInformation = new PoolInformation { PoolId = "mypool" };

                    job.Commit();

                    CloudTask task = new CloudTask("BatchCmd", "cmd /c %AZ_BATCH_NODE_STARTUP_DIR%\\wd\\test.bat");
                    task.UserIdentity = new UserIdentity(new AutoUserSpecification(elevationLevel: ElevationLevel.Admin, scope: AutoUserScope.Task));

                    batchClient.JobOperations.AddTask(jobId, task);

                    TimeSpan timeout = TimeSpan.FromMinutes(30);
                    Console.WriteLine("Monitoring all tasks for 'Completed' state, timeout in {0}...", timeout);

                    IEnumerable<CloudTask> addedTasks = batchClient.JobOperations.ListTasks(jobId);

                    batchClient.Utilities.CreateTaskStateMonitor().WaitAll(addedTasks, TaskState.Completed, timeout);

                    Console.WriteLine("All tasks reached state Completed.");

                    IEnumerable<CloudTask> completedtasks = batchClient.JobOperations.ListTasks(jobId);

                    foreach (CloudTask ct in completedtasks)
                    {
                        Console.WriteLine(ct.ToString());
                    }

                    // Print out some timing info
                    timer.Stop();
                    Console.WriteLine("Sample end: {0}", DateTime.Now);
                    Console.WriteLine("Elapsed time: {0}", timer.Elapsed);

                    // add code to check the output to verify if the result is good, otherwise throw exception 
                    // if (not good result) {
                    //   throw new Exception("something bad happened...");
                    // }
                }
                catch (Exception e)
                {
                    logger.LogError("Error: " + e.ToString());
                }
            }

            return new OkObjectResult("OK");
        }
    }
}
