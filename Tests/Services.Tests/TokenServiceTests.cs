namespace Tests;

using DAL;
using Domain.entity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Services.Implemt;

public class TokenServiceTests
{
    private TokenService _tokenService;
    public TokenServiceTests()
    {
        var MoqDbContext = new Mock<AppDbContext>();

        MoqDbContext.Setup(x => x.Users).ReturnsDbSet(new List<User>() { new User() {Id = 1, Email="art@art.com"}});
        
        _tokenService = new TokenService(MoqDbContext.Object);
    }

    [Fact]
    public async void RemoveRefreshTokenAsync_userId0_false()
    {
        bool result = await _tokenService.RemoveRefreshTokenAsync(new User() {Id = 0});

        Assert.False(result);
    } 
    [Fact]
    public async void RemoveRefreshTokenAsync_userIdIsNotInDB_false()
    {
        bool result = await _tokenService.RemoveRefreshTokenAsync(new User() {Id = 123123});

        Assert.False(result);
    }   
}