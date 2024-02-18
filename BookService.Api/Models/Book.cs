using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BookService.Api.Models;

public class Book
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; }

    [BsonElement("author")]
    public string Author { get; set; }

    [BsonElement("year")]
    public int Year { get; set; }
     
    [BsonElement("price")]
    public decimal Price { get; set; }

    [BsonElement("category")]
    public string Category { get; set; }
}