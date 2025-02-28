﻿using Dapper;
using Library.Api.Data;
using Library.Api.Models;

namespace Library.Api.Services
{
    public class BookService : IBookService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public BookService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<bool> CreateAsync(Book book)
        {
            var existingBook = await GetByIsbnAsync(book.Isbn);
            if (existingBook is not null)
            {
                return false;
            }

            using var connection = await _connectionFactory.CreateConnectionAsync();
            await connection.ExecuteAsync(
                @"INSERT INTO Books (Isbn, Title, Author, ShortDescription, PageCount, ReleaseDate) 
                  VALUES (@Isbn, @Title, @Author, @ShortDescription, @PageCount, @ReleaseDate)",
                book);
            return true;
        }

        public async Task<bool> DeleteAsync(string isbn)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var result = await connection.ExecuteAsync(
                @"DELETE FROM Books Where Isbn = @Isbn", new {Isbn = isbn});

            return result > 0;
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            return await connection.QueryAsync<Book>("SELECT * FROM Books");
        }

        public async Task<Book?> GetByIsbnAsync(string isbn)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            return connection.QuerySingleOrDefault<Book>("SELECT * FROM Books WHERE Isbn = @Isbn LIMIT 1", new {Isbn = isbn});
        }

        public async Task<IEnumerable<Book>> SearchByTitleAsync(string searchTerm)
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            return await connection.QueryAsync<Book>(
                "SELECT * FROM Books WHERE Title LIKE @SearchTerm",
                new { SearchTerm = $"%{searchTerm}%" });
        }

        public async Task<bool> UpdateAsync(Book book)
        {
            var existingBook = await GetByIsbnAsync(book.Isbn);
            if(existingBook is null)
            {
                return false;
            }
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var results = await connection.ExecuteAsync(
                @"UPDATE Books SET Title = @Title, Author = @Author, ShortDescription = @ShortDescription, PageCount = @PageCount, ReleaseDate = @ReleaseDate WHERE Isbn = @Isbn", book);

            return results > 0;

        }
    }
}
