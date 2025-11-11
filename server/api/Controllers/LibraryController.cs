using api.DTOs.Requests;
using api.Services;
using dataccess;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace api;

public class LibraryController(ILibraryService libraryService, IAuthService authService) : ControllerBase
{
    [HttpPost(nameof(GetAuthors))]
    public async Task<List<Author>> GetAuthors([FromBody] SieveModel sieveModel)
    {
        var jwtClaims = await authService.VerifyAndDecodeToken(Request.Headers.Authorization.FirstOrDefault());
        return await libraryService.GetAuthors(sieveModel, jwtClaims);
    }


    [HttpPost(nameof(GetBooks))]
    public async Task<List<Book>> GetBooks([FromBody] SieveModel sieveModel)
    {
        var jwtClaims = await authService.VerifyAndDecodeToken(Request.Headers.Authorization.FirstOrDefault());

        return await libraryService.GetBooks(sieveModel, jwtClaims);
    }

    [HttpPost(nameof(GetGenres))]
    public async Task<List<Genre>> GetGenres([FromBody] SieveModel sieveModel)
    {
        var jwtClaims = await authService.VerifyAndDecodeToken(Request.Headers.Authorization.FirstOrDefault());

        return await libraryService.GetGenres(sieveModel, jwtClaims);
    }

    [HttpPost(nameof(CreateBook))]
    public async Task<Book> CreateBook([FromBody] CreateBookRequestDto dto)
    {
        var jwtClaims = await authService.VerifyAndDecodeToken(Request.Headers.Authorization.FirstOrDefault());

        return await libraryService.CreateBook(dto, jwtClaims);
    }

    [HttpPut(nameof(UpdateBook))]
    public async Task<Book> UpdateBook([FromBody] UpdateBookRequestDto dto)
    {
        var jwtClaims = await authService.VerifyAndDecodeToken(Request.Headers.Authorization.FirstOrDefault());

        return await libraryService.UpdateBook(dto, jwtClaims);
    }

    [HttpDelete(nameof(DeleteBook))]
    public async Task<Book> DeleteBook([FromQuery] string bookId)
    {
        var jwtClaims = await authService.VerifyAndDecodeToken(Request.Headers.Authorization.FirstOrDefault());

        return await libraryService.DeleteBook(bookId, jwtClaims);
    }

    [HttpPost(nameof(CreateAuthor))]
    public async Task<Author> CreateAuthor([FromBody] CreateAuthorRequestDto dto)
    {
        var jwtClaims = await authService.VerifyAndDecodeToken(Request.Headers.Authorization.FirstOrDefault());

        return await libraryService.CreateAuthor(dto, jwtClaims);
    }

    [HttpPut(nameof(UpdateAuthor))]
    public async Task<Author> UpdateAuthor([FromBody] UpdateAuthorRequestDto dto)
    {
        var jwtClaims = await authService.VerifyAndDecodeToken(Request.Headers.Authorization.FirstOrDefault());

        return await libraryService.UpdateAuthor(dto, jwtClaims);
    }

    [HttpDelete(nameof(DeleteAuthor))]
    public async Task<Author> DeleteAuthor([FromQuery] string authorId)
    {
        var jwtClaims = await authService.VerifyAndDecodeToken(Request.Headers.Authorization.FirstOrDefault());

        return await libraryService.DeleteAuthor(authorId, jwtClaims);
    }

    [HttpPost(nameof(CreateGenre))]
    public async Task<Genre> CreateGenre([FromBody] CreateGenreDto dto)
    {
        var jwtClaims = await authService.VerifyAndDecodeToken(Request.Headers.Authorization.FirstOrDefault());

        return await libraryService.CreateGenre(dto, jwtClaims);
    }

    [HttpDelete(nameof(DeleteGenre))]
    public async Task<Genre> DeleteGenre([FromQuery] string genreId)
    {
        var jwtClaims = await authService.VerifyAndDecodeToken(Request.Headers.Authorization.FirstOrDefault());

        return await libraryService.DeleteGenre(genreId, jwtClaims);
    }

    [HttpPut(nameof(UpdateGenre))]
    public async Task<Genre> UpdateGenre([FromBody] UpdateGenreRequestDto dto)
    {
        var jwtClaims = await authService.VerifyAndDecodeToken(Request.Headers.Authorization.FirstOrDefault());

        return await libraryService.UpdateGenre(dto, jwtClaims);
    }
}