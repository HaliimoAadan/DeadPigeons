using api.DTOs.Requests;
using dataccess;
using Sieve.Models;

namespace api.Services;

public interface ILibraryService
{
    Task<Book> CreateBook(CreateBookRequestDto dto, JwtClaims requester);
    Task<Book> UpdateBook(UpdateBookRequestDto dto, JwtClaims requester);
    Task<Book> DeleteBook(string id, JwtClaims requester);
    Task<Author> CreateAuthor(CreateAuthorRequestDto dto, JwtClaims requester);
    Task<Author> UpdateAuthor(UpdateAuthorRequestDto dto, JwtClaims requester);
    Task<Author> DeleteAuthor(string authorId, JwtClaims requester);
    Task<Genre> CreateGenre(CreateGenreDto dto, JwtClaims requester);
    Task<Genre> DeleteGenre(string genreId, JwtClaims requester);
    Task<Genre> UpdateGenre(UpdateGenreRequestDto dto, JwtClaims requester);

    Task<List<Author>> GetAuthors(SieveModel sieveModel, JwtClaims requester);
    Task<List<Book>> GetBooks(SieveModel sieveModel, JwtClaims requester);
    Task<List<Genre>> GetGenres(SieveModel sieveModel, JwtClaims requester);
}