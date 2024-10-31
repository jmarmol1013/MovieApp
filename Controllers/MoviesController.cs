using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using MovieApp.Models;

namespace MovieApp.Controllers
{
    public class MoviesController : Controller
    {
        private readonly DynamoDbClient _dynamoDbClient;
        private readonly S3Client _s3Client;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(ILogger<MoviesController> logger)
        {
            _dynamoDbClient = new DynamoDbClient();
            _s3Client = new S3Client();
            _logger = logger;
        }

        
        public async Task<IActionResult> Index(string genre, double? rating)
        {
            var movies = await _dynamoDbClient.GetAllMoviesAsync();

            if (!string.IsNullOrWhiteSpace(genre))
            {
                movies = movies.Where(m => m.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (rating.HasValue)
            {
                movies = movies.Where(m => m.Rating >= rating.Value).ToList();
            }

            return View(movies);
        }


        public async Task<IActionResult> Download(string movieId)
        {
            var url = await _s3Client.GetMovieDownloadUrlAsync(movieId);
            return Redirect(url);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Movie movie, IFormFile movieFile)
        {
            if (ModelState.IsValid)
            {
                // Add movie to DB
                await _dynamoDbClient.AddMovieAsync(movie);

                // Add movie .mp4 to bucket
                if (movieFile != null && movieFile.Length > 0)
                {
                    using (var stream = movieFile.OpenReadStream())
                    {
                        await _s3Client.UploadMovieFileAsync(movie.MovieId, stream);
                    }
                }

                return RedirectToAction("Index");
            }

            return View(movie);
        }

        // Movies/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(string id, string movieName)
        {
            var movie = await _dynamoDbClient.GetMovieByIdAsync(id, movieName);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Edit/{id}
        [HttpPost]
        public async Task<IActionResult> Edit(string id, Movie movie, IFormFile? newVideoFile = null)
        {

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError(error.ErrorMessage);
                }
                return View(movie);
            }

            movie.MovieId = id;

            // Update DynamoDB
            await _dynamoDbClient.UpdateMovieAsync(movie);

            // Update S3 if new video file is provided
            if (newVideoFile != null && newVideoFile.Length > 0)
            {
                using (var stream = newVideoFile.OpenReadStream())
                {
                    await _s3Client.UploadMovieFileAsync(movie.MovieId, stream);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(string movieId, string movieName, string content, string userId)
        {
            if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(movieName))
            {
                return BadRequest("Comment content, User ID, and Movie Name are required.");
            }

            var movie = await _dynamoDbClient.GetMovieByIdAsync(movieId, movieName);
            if (movie == null)
            {
                return NotFound();
            }

            // Create a new comment
            var comment = new Comment
            {
                CommentId = Guid.NewGuid().ToString(),
                Content = content,
                Timestamp = DateTime.UtcNow,
                UserId = userId
            };

            movie.Comments.Add(comment);

            // Update the movie in DynamoDB
            await _dynamoDbClient.UpdateMovieAsync(movie);

            return RedirectToAction("Index");
        }


    }
}
