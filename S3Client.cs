using Amazon.S3.Model;
using Amazon.S3;
using Amazon;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace MovieApp
{
    public class S3Client
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3Client()
        {
            string accessKey = ConfigurationManager.AppSettings["AWSAccessKeyId"];
            string secretKey = ConfigurationManager.AppSettings["AWSSecretAccessKey"];
            string region = ConfigurationManager.AppSettings["AWSRegion"];

            _bucketName = "movies306";
            _s3Client = new AmazonS3Client(accessKey, secretKey, RegionEndpoint.GetBySystemName("ca-central-1"));
        }

        public async Task<string> GetMovieDownloadUrlAsync(string movieId)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = movieId + ".mp4",
                Expires = DateTime.UtcNow.AddMinutes(10)
            };

            return _s3Client.GetPreSignedURL(request);
        }

        public async Task UploadMovieFileAsync(string movieId, Stream fileStream)
        {
            var uploadRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = $"{movieId}.mp4",
                InputStream = fileStream,
                ContentType = "video/mp4"
            };

            await _s3Client.PutObjectAsync(uploadRequest);
        }

        public async Task DeleteMovieFileAsync(string movieId)
        {
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = $"{movieId}.mp4"
            };

            await _s3Client.DeleteObjectAsync(deleteObjectRequest);
        }
    }
}
