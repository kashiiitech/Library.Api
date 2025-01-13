﻿using FluentValidation.Results;
using FluentValidation;
using Library.Api.Models;
using Library.Api.Services;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Library.Api.Endpoints
{
    public static class LibraryEndpoints
    {
        public static void AddLibraryEndpoints(this IServiceCollection services)
        {
            services.AddSingleton<IBookService, BookService>();
        }

        public static void UseLibraryEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("books", CreateBookAsync)
           .WithName("CreateBook")
            .Accepts<Book>("application/json")
            .Produces<Book>(201)
            .Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags("Books");

            app.MapGet("books", async (IBookService bookService, string? searchTerm) =>
            {
                if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
                {
                    var matchedBooks = await bookService.SearchByTitleAsync(searchTerm);
                    return Results.Ok(matchedBooks);
                }

                var books = await bookService.GetAllAsync();
                return Results.Ok(books);
            }).WithName("GetBooks")
              .Produces<IEnumerable<Book>>(200)
              .WithTags("Books");

            app.MapGet("books/{isbn}", async (string isbn, IBookService bookService) =>
            {
                var book = await bookService.GetByIsbnAsync(isbn);
                return book is not null ? Results.Ok(book) : Results.NotFound();
            }).WithName("GetBook")
              .Produces<Book>(200)
              .Produces(404)
              .WithTags("Books");

            // Updating book
            app.MapPut("books/{isbn}", async (string isbn, Book book, IBookService bookService, IValidator<Book> validator) =>
            {

                var validationResult = await validator.ValidateAsync(book);
                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(validationResult.Errors);
                }

                var updated = await bookService.UpdateAsync(book);
                return updated ? Results.Ok(book) : Results.NotFound();
            }).WithName("UpdateBook")
              .Accepts<Book>("application/json")
              .Produces<Book>(200)
              .Produces<IEnumerable<ValidationFailure>>(400)
              .WithTags("Books");

            // Delete book
            app.MapDelete("books/{isbn}", async (string isbn, IBookService bookService) =>
            {
                var deleted = await bookService.DeleteAsync(isbn);
                return deleted ? Results.NoContent() : Results.NotFound();
            }).WithName("DeleteBook")
              .Produces(204)
              .Produces(404)
              .WithTags("Books");

            app.MapGet("status",
            () =>
                {
                    return Results.Extensions.Html(@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Dreamy Adventures</title>
</head>
<body>
    <h1>Welcome to the Land of Imagination</h1>
    <p>
        Step into a world where every corner holds a story waiting to be told, and every moment is filled with wonder.
        Let your dreams take flight!
    </p>
</body>
</html>");
                });//.ExcludeFromDescription(); used to hide any endpoints from swagger
        }


    private static async Task<IResult> CreateBookAsync(Book book, IBookService bookService, IValidator<Book> validator)
    {
            {

                var validationResult = await validator.ValidateAsync(book);
                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(validationResult.Errors);
                }

                var created = await bookService.CreateAsync(book);
                if (!created)
                {
                    return Results.BadRequest(new List<ValidationFailure>
        {
            new ("Isbn", "A book with this ISBN-13 already exists!")
                    });
                }

                //var path = linker.GetPathByName("GetBook", new { isbn = book.Isbn });
                // get the other uri before endpoint
                //var locationUri = linker.GetUriByName(context, "GetBook", new { isbn = book.Isbn });
                //return Results.Created(locationUri, book);

                //return Results.CreatedAtRoute("GetBook", new { isbn = book.Isbn }, book);
                return Results.Created($"/books/{book.Isbn}", book);
            }
        }
    }
}
