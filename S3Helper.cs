using Amazon.S3;
using Amazon.S3.Model;

namespace KnightMove2
{
    public class S3Helper
    { 
        private readonly AmazonS3Client _s3Client;

        /// <summary>
        /// AWS class used to write to S3.
        /// </summary>
        /// <param name="accessKeyId"></param>
        /// <param name="secretAccessKey"></param>
        /// <param name="region"></param>
        public S3Helper(string accessKeyId, string secretAccessKey, string region)
        {
            var config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
            };
            _s3Client = new AmazonS3Client(accessKeyId, secretAccessKey, config);
        }

        /// <summary>
        /// Write to S3 bucket.
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="key"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task WriteObjectToS3Async(string bucketName, string key, string body)
        {
            try
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    ContentBody = body
                };

                PutObjectResponse response = await _s3Client.PutObjectAsync(putRequest);
                Console.WriteLine("Object written to S3 successfully");
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
        }
    }
}
