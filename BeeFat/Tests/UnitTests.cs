using BeeFat.Data;
using BeeFat.Domain.Infrastructure;
using BeeFat.Helpers;
using BeeFat.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace BeeFat.Tests;

[TestFixture]
public class UnitTests
{
    private IConfiguration configuration;
    private DbContextOptions<ApplicationDbContext> options;

    public UserRepository UserRepository;
    public TrackRepository TrackRepository;
    public JournalRepository JournalRepository;
    public FoodProductRepository FoodProductRepository;
    
    private DbContextOptions<ApplicationDbContext> GetOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            .Options;
    }
    
    public UnitTests()
    {
        configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        
        options = GetOptions();
        UserRepository = new UserRepository(configuration, options);
        TrackRepository = new TrackRepository(configuration, options);
        JournalRepository = new JournalRepository(configuration, options);
        FoodProductRepository = new FoodProductRepository(configuration, options);
    }

    [Test]
    public void TestUserInfoSaving()
    {
        var hh = new HomeHelper(UserRepository, JournalRepository, FoodProductRepository);
        var trackPicker = new TrackPickHelper(UserRepository, TrackRepository, JournalRepository);
        
        var newTrack = TrackRepository.GetCollection(t => t.Id != hh.User.TrackId).First();
        trackPicker.ChangeSelectedTrack(newTrack);
        trackPicker.Save();
        
        var userProfileHelper = new UserProfileHelper(UserRepository);
        var newLastName = GenerateRandomString(10);
        userProfileHelper.UserModel.PersonName.LastName = newLastName;
        userProfileHelper.SaveProfile();
        
        var hh1 = new HomeHelper(UserRepository, JournalRepository, FoodProductRepository);
        hh1.User.TrackId.Should().Be(newTrack.Id);
        hh1.User.PersonName.LastName.Should().Be(newLastName);
    }

    [Test]
    public void TestTransferFoodProductsFromTrackToJournal()
    {
        var hh = new HomeHelper(UserRepository, JournalRepository, FoodProductRepository);
        var track = hh.User.Track;

        Track otherTrack;
        using (var context = new ApplicationDbContext(options, configuration))
        {
            otherTrack = context.Tracks.First(t => t.Id != track.Id);
        }
        
        var tp = new TrackPickHelper(UserRepository, TrackRepository, JournalRepository);
        tp.ChangeSelectedTrack(otherTrack);
        tp.Save();
        
        var journal = hh.User.Journal;
        var ids1 = journal.FoodProducts.Select(fp => fp.Id).OrderBy(id => id).ToList();
        var ids2 = track.FoodProducts.Select(fp => fp.Id).OrderBy(id => id).ToList();

        ids1.SequenceEqual(ids2).Should().BeTrue();
    }
    
    [Test]
    public void TestSetEatenFoodProducts()
    {
        var hh = new HomeHelper(UserRepository, JournalRepository, FoodProductRepository);
        var journal = hh.User.Journal;
        var eatenProduct = journal.FoodProducts.First(fp => !fp.IsEaten);
        hh.SelectedFoodProduct = eatenProduct;
        eatenProduct.PortionSize += 10;
        hh.PortionSize = eatenProduct.PortionSize;
        
        hh.ChangeFoodProductInfoAndSave(true);
        journal = hh.JournalRepository.GetById(journal.Id);
        var foundEatenProduct = journal.FoodProducts.First(fp => fp.Id == eatenProduct.Id);
        foundEatenProduct.IsEaten.Should().BeTrue();
        foundEatenProduct.PortionSize.Should().Be(hh.PortionSize);
    }



    private string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();

        var randomString = new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        return randomString;
    }
}