using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace exam_frontend.Services
{
    public class MinioService
    {
        private readonly IMinioClient minio;
        private readonly string bucket_name;

        public MinioService(IConfiguration config)
        {
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

            minio = new Minio.MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(false)
                .Build();
        }

        public async Task UploadFileAsync(string object_name, Stream data, string content_type)
        {
            try
            {
                bool found = await minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket_name));
                if (!found)
                {
                    await minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket_name));
                }

                await minio.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(bucket_name)
                    .WithObject(object_name)
                    .WithStreamData(data)
                    .WithObjectSize(data.Length)
                    .WithContentType(content_type));
            }
            catch (MinioException e)
            {
                Console.WriteLine($"[MinIO ERROR] {e.Message}");
            }
        }

        public async Task<string> GetFileUrlAsync(string object_name)
        {
            try
            {
                var args = new PresignedGetObjectArgs()
                    .WithBucket(bucket_name)
                    .WithObject(object_name)
                    .WithExpiry(3600); // 1 hour

                return await minio.PresignedGetObjectAsync(args);
            }
            catch (MinioException e)
            {
                Console.WriteLine($"[MinIO ERROR] {e.Message}");
                return null;
            }
        }

        public async Task DeleteFileAsync(string objectName)
        {
            try
            {
                await minio.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(bucket_name).WithObject(objectName));
            }
            catch (MinioException e)
            {
                Console.WriteLine($"[MinIO ERROR] {e.Message}");
            }
        }
    }

}
