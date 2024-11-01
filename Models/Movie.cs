using Amazon.DynamoDBv2.DataModel;
using System;

namespace MovieApp.Models
{
    [DynamoDBTable("Movies")]
    public class Movie
    {
        [DynamoDBHashKey("movieId")]
        public string MovieId { get; set; }

        [DynamoDBRangeKey("movieName")]
        public string MovieName { get; set; }

        [DynamoDBProperty("addedBy")]
        public string AddedBy { get; set; }

        [DynamoDBProperty("comments")]
        public List<Comment> Comments { get; set; } = new List<Comment>();

        [DynamoDBProperty("director")]
        public string Director { get; set; }

        [DynamoDBProperty("genre")]
        public string Genre { get; set; }

        [DynamoDBProperty("rating")]
        public double? Rating { get; set; }

        [DynamoDBProperty("releaseDate")]
        public DateTime ReleaseDate { get; set; }
    }

    public class Comment
    {
        [DynamoDBProperty("commentId")]
        public string CommentId { get; set; }

        [DynamoDBProperty("content")]
        public string Content { get; set; }

        [DynamoDBProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [DynamoDBProperty("userId")]
        public string UserId { get; set; }

        [DynamoDBProperty("rating")]
        public double? Rating { get; set; }
    }
}
