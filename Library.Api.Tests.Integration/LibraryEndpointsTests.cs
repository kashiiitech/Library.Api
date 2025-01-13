using System.Net;
using FluentAssertions;
using Library.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Library.Api.Tests.Integration
{
    public class LibraryEndpointsTests : IClassFixture<WebApplicationFactory<IApiMarker>>, IAsyncLifetime
    {
        private readonly WebApplicationFactory<IApiMarker> _factory;

        private readonly List<string> _createdIsbns = new();

        public LibraryEndpointsTests(WebApplicationFactory<IApiMarker> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreateBook_CreatesBook_WhenDataIsCorrect()
        {
            // Arrage
            var httpClient = _factory.CreateClient();
            var book = GenerateBook();

            // Act
            var result = await httpClient.PostAsJsonAsync("/books", book);
            _createdIsbns.Add(book.Isbn);
            var createdBook = await result.Content.ReadFromJsonAsync<Book>();

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.Created);
            createdBook.Should().BeEquivalentTo(book);
            result.Headers.Location.Should().Be($"/books/{book.Isbn}");
        }

        [Fact]
        public async Task CreateBook_Fails_WhenIsbnIsInvalid()
        {
            // Arrage
            var httpClient = _factory.CreateClient();
            var book = GenerateBook();
            book.Isbn = "INVALID";

            // Act
            var result = await httpClient.PostAsJsonAsync("/books", book);
            _createdIsbns.Add(book.Isbn);
            var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
            var error = errors!.Single();

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.PropertyName.Should().Be("Isbn");
            error.ErrorMessage.Should().Be("Value was not a valid ISBN-13");
        }


        [Fact]
        public async Task CreateBook_Fails_WhenBookExists()
        {
            // Arrage
            var httpClient = _factory.CreateClient();
            var book = GenerateBook();

            // Act
            await httpClient.PostAsJsonAsync("/books", book);
            _createdIsbns.Add(book.Isbn);
            var result = await httpClient.PostAsJsonAsync("/books", book);
            var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
            var error = errors!.Single();

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.PropertyName.Should().Be("Isbn");
            error.ErrorMessage.Should().Be("A book with this ISBN-13 already exists!");
        }

        [Fact]
        public async Task GetBook_ReturnsBook_WhenBookExists()
        {
            //Arrage
            var httpClient = _factory.CreateClient();
            var book = GenerateBook();
            await httpClient.PostAsJsonAsync("/books", book);
            _createdIsbns.Add(book.Isbn);

            // Act
            var result = await httpClient.GetAsync($"/books/{book.Isbn}");
            var existingBook = await result.Content.ReadFromJsonAsync<Book>();

            // Assert
            existingBook.Should().BeEquivalentTo(book);
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetBook_ReturnsNotFound_WhenBookDoesNotExists()
        {
            //Arrage
            var httpClient = _factory.CreateClient();
            var isbn = GenerateIsbn();

            // Act
            var result = await httpClient.GetAsync($"/books/{isbn}");

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetAllBook_ReturnsAllBooks_WhenBooksExists()
        {
            //Arrange
            var httpClient = _factory.CreateClient();
            var book = GenerateBook();
            await httpClient.PostAsJsonAsync("/books", book);
            _createdIsbns.Add(book.Isbn);
            var books = new List<Book> { book };

            // Act
            var result = await httpClient.GetAsync("/books");
            var returnedBooks = await result.Content.ReadFromJsonAsync<List<Book>>();

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            returnedBooks.Should().BeEquivalentTo(books);
        }

        [Fact]
        public async Task GetAllBook_ReturnsNoBooks_WhenNoBooksExists()
        {
            // Arrange
            var httpClient = _factory.CreateClient();

            // Act
            var result = await httpClient.GetAsync("/books");
            var returnedBooks = await result.Content.ReadFromJsonAsync<List<Book>>();

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            returnedBooks.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchBook_ReturnsBooks_WhenTitleMatches()
        {
            //Arrage
            var httpClient = _factory.CreateClient();
            var book = GenerateBook();
            await httpClient.PostAsJsonAsync("/books", book);
            _createdIsbns.Add(book.Isbn);
            var books = new List<Book> { book };

            // Act
            var result = await httpClient.GetAsync("/books?searchTerm=oder");
            var returnedBooks = await result.Content.ReadFromJsonAsync<List<Book>>();

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            returnedBooks.Should().BeEquivalentTo(books);
        }

        private Book GenerateBook(string title = "The Dirty Coder")
        {
            return new Book
            {
                Isbn = GenerateIsbn(),
                Title = title,
                Author = "Kashif Ali",
                PageCount = 420,
                ShortDescription = "All my tricks to learn minimal api in one book",
                ReleaseDate = new DateTime(2023, 1, 1),
            };
        }

        private string GenerateIsbn()
        {
            return $"{Random.Shared.Next(100, 999)}-" +
                   $"{Random.Shared.Next(1000000000, 2100999999)}";
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            var httpClient = _factory.CreateClient();
            foreach (var createdIsbn in _createdIsbns)
            {
                await httpClient.DeleteAsync($"/books/{createdIsbn}");
            }
        }
    }
}
