using System;
using System.Collections.Generic;
using System.Linq;

using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;

namespace Crypto
{
    public class CloudWatch
    {
        public static string PushMetrics(string ns, List<MetricDatum> metrics)
        {
            var request = new PutMetricDataRequest
            {
                Namespace = ns,
                MetricData = metrics
            };
            //
            Amazon.Runtime.AWSCredentials credentials = null;
            if (Configuration.IsLinux())
            {
                string AccessKey = "xxx";
                string SecretKey = "xxx";
                credentials = new Amazon.Runtime.BasicAWSCredentials(AccessKey, SecretKey);
            }
            else
            {
                credentials = new Amazon.Runtime.StoredProfileAWSCredentials("cloudwatch");
            }
            //
            var client = new AmazonCloudWatchClient(credentials, Amazon.RegionEndpoint.APSoutheast1);
            System.Threading.Tasks.Task<PutMetricDataResponse> task = client.PutMetricDataAsync(request);
            PutMetricDataResponse response = task.Result;
            //
            System.Net.HttpStatusCode statusCode = response.HttpStatusCode;
            Console.WriteLine(DateTime.Now.ToString("HH:mm ") + statusCode.ToString());
            return statusCode.ToString();
        }
    }
}
