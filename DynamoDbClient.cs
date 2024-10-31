using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using MovieApp.Models;
using System.Configuration;
using Amazon;
using ConfigurationManager = System.Configuration.ConfigurationManager;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;

namespace MovieApp
{
    public class DynamoDbClient
    {
        private readonly AmazonDynamoDBClient _dynamoDbClient;
        private readonly DynamoDBContext _context;

        public DynamoDbClient()
        {
            string accessKey = ConfigurationManager.AppSettings["AWSAccessKeyId"];
            string secretKey = ConfigurationManager.AppSettings["AWSSecretAccessKey"];
            string region = ConfigurationManager.AppSettings["AWSRegion"];
            var regionEndpoint = RegionEndpoint.GetBySystemName(region);

            _dynamoDbClient = new AmazonDynamoDBClient(accessKey, secretKey, regionEndpoint);
            _context = new DynamoDBContext(_dynamoDbClient);
        }

        public async Task<List<Movie>> GetAllMoviesAsync()
        {
            var conditions = new List<ScanCondition>();
            return await _context.ScanAsync<Movie>(conditions).GetRemainingAsync();
        }

        public async Task<Movie> GetMovieByIdAsync(string id, string movieName)
        {
            return await _context.LoadAsync<Movie>(id, movieName);
        }

        public async Task AddMovieAsync(Movie movie)
        {
            await _context.SaveAsync(movie);
        }

        public async Task UpdateMovieAsync(Movie movie)
        {
            await _context.SaveAsync(movie);
        }

        public async Task DeleteMovieAsync(string id, string movieName)
        {
            await _context.DeleteAsync<Movie>(id, movieName);
        }

        public async Task<List<Movie>> GetMoviesByRatingAsync(int minRating)
        {
            var config = new DynamoDBOperationConfig
            {
                IndexName = "rating-genre-index",
            };

            var scanConditions = new List<ScanCondition>
            {
                new ScanCondition("Rating", ScanOperator.GreaterThan, minRating)
            };

            return await _context.ScanAsync<Movie>(scanConditions, config).GetRemainingAsync();
        }

        public async Task<List<Movie>> GetMoviesByGenreAsync(string genre)
        {
            var config = new DynamoDBOperationConfig
            {
                IndexName = "genre-rating-index",
            };

            var scanConditions = new List<ScanCondition>
            {
                new ScanCondition("Genre", ScanOperator.Equal, genre)
            };

            return await _context.ScanAsync<Movie>(scanConditions, config).GetRemainingAsync();
        }
    }
}
