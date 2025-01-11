using FluentValidation;
using FluentValidation.Results;
using Library.Api.Data;
using Library.Api.Models;
using Library.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDbConnectionFactory>(_ => 
            new SqliteConnectionFactory(
                builder.Configuration.GetValue<string>("Database:ConnectionString")));
builder.Services.AddSingleton<IBookService, BookService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddSingleton<DatabaseInitializer>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("books", async (Book book, IBookService bookService, IValidator<Book> validator) =>
{

    var validationResult = await validator.ValidateAsync(book);
    if(!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors);
    }

    var created = await bookService.CreateAsync(book);
    if(!created)
    {
        return Results.BadRequest(new List<ValidationFailure>
        {
            new ("Isbn", "A book with this ISBN-13 already exists!")
        });
    }

    return Results.Created($"/books/{book.Isbn}", book);
});

// Db init here
var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();


app.Run();
