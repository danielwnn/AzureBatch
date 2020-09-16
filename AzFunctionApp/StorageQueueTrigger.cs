using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzFunctionApp1
{
    public static class StorageQueueTrigger
    {
        [FunctionName("StorageQueueTrigger")]
        public static void Run(
            [QueueTrigger("image-message-queue", Connection = "StorageQueueConnection")]string myQueueItem, 
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            Dictionary<string, Object> values = JsonConvert.DeserializeObject<Dictionary<string, Object>>(myQueueItem);
            string dataStr = values["data"].ToString();
            Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataStr);
            
            string blobUrl = data["url"].ToString();
            log.LogInformation(blobUrl);

            string batchAccountUrl = "https://azbatchml.westus2.batch.azure.com";
            string batchAccountName = "azbatchml";
            string batchAccountKey = "YOQwit+pV8rYm0hrU9sXwNilZd00JJWO46TK6p1uPjqzgRYdE4MBtbvvMp0hx0Ue80dCmezk9dzXYzwrAptPrg==";

            BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(batchAccountUrl, batchAccountName, batchAccountKey);

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

                    CloudTask task = new CloudTask("BatchCmd", "cmd /c %AZ_BATCH_NODE_STARTUP_DIR%\\wd\\test.bat " + blobUrl);
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
                    log.LogError("Error: " + e.ToString());
                }
            }
        }
    }
}
