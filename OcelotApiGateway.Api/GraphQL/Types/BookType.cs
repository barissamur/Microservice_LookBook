using OcelotApiGateway.Api.GraphQL.Models;

namespace OcelotApiGateway.Api.GraphQL.Types;

// GraphQL/Types/BookType.cs
public class BookType : ObjectType<Book>
{
    protected override void Configure(IObjectTypeDescriptor<Book> descriptor)
    {
        descriptor.Field(t => t.Id).Type<NonNullType<IdType>>();
        descriptor.Field(t => t.Title).Type<NonNullType<StringType>>();
        descriptor.Field(t => t.Author).Type<NonNullType<StringType>>();
        descriptor.Field(t => t.Year).Type<NonNullType<IntType>>();
        descriptor.Field(t => t.Price).Type<NonNullType<DecimalType>>();
        descriptor.Field(t => t.Category).Type<NonNullType<StringType>>();
    }
}
