using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace exam_api.Services;

public class MinioService
{
        private readonly IMinioClient minio;
        private readonly ILogger logger;
        private readonly string bucket_name;

        public MinioService(IConfiguration config, ILogger<MinioService> logger)
        {
            this.logger = logger;
            string endpoint, accessKey, secretKey;
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                endpoint = config["Minio:Endpoint"];
                accessKey = config["Minio:AccessKey"];
                secretKey = config["Minio:SecretKey"];
                bucket_name = config["Minio:BucketName"];
            }
            else
            {
                endpoint = "127.0.0.1:9000";
                accessKey = Environment.GetEnvironmentVariable("MinioRootUser");
                secretKey = Environment.GetEnvironmentVariable("MinioRootPassword");
                bucket_name = Environment.GetEnvironmentVariable("MinioBucketName");
            }

            minio = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(false)
                .Build(); 
        }

        public async Task UploadFileAsync(string object_name, Stream data, string content_type)
        {
            try
            {
                logger.LogInformation($"Looking for bucket with name {bucket_name}");
                
                bool found = await minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket_name));
                if (!found)
                {
                    logger.LogWarning($"Bucket not found with name {bucket_name}, creating it");
                    await minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket_name));
                    logger.LogInformation($"Bucket with name {bucket_name} created");
                }

                logger.LogInformation($"Putting object in bucket {bucket_name} with name {object_name}, " +
                                      $"content type - {content_type}, size - {data.Length}");
                await minio.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(bucket_name)
                    .WithObject(object_name)
                    .WithStreamData(data)
                    .WithObjectSize(data.Length)
                    .WithContentType(content_type));
                
                logger.LogInformation($"Object in bucket {bucket_name} with name {object_name} created");
            }
            catch (MinioException e)
            {
                logger.LogError($"MinIO ERROR {e.Message}");
            }
        }

        public async Task<string> GetFileUrlAsync(string object_name)
        {
            try
            {
                logger.LogInformation($"Getting object in bucket {bucket_name} with name {object_name}");
                var args = new PresignedGetObjectArgs()
                    .WithBucket(bucket_name)
                    .WithObject(object_name)
                    .WithExpiry(3600); // 1 hour
                logger.LogInformation($"Created link for object with name {object_name} in bucket {bucket_name}, expiry - 1h");
                return await minio.PresignedGetObjectAsync(args);
            }
            catch (MinioException e)
            {
                logger.LogError($"[MinIO ERROR] {e.Message}");
                return null;
            }
        }

        public async Task DeleteFileAsync(string objectName)
        {
            try
            {
                logger.LogInformation($"Deleting object in bucket {bucket_name} with name {objectName}");
                await minio.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(bucket_name).WithObject(objectName));
                logger.LogInformation($"Deleted object in bucket {bucket_name} with name {objectName}");
            }
            catch (MinioException e)
            {
                logger.LogError($"[MinIO ERROR] {e.Message}");
            }
        }
    }
