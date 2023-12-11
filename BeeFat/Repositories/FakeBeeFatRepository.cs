using BeeFat.Data;
using BeeFat.Domain.Infrastructure;
using BeeFat.Interfaces;

namespace BeeFat.Repositories;

public class FakeBeeFatRepository: IBaseRepository
{
    public ICollection<ApplicationUser> BeeFatUsers { get; }
    public ICollection<Food> Foods { get; }
    public ICollection<FoodProduct> FoodProducts { get; }
    
    private Track FakeTrack;
    private ApplicationUser FakeUser;
    private IEnumerable<Track> FakeTracks;

    public FakeBeeFatRepository()
    {
        FakeUser = FakeData.OlegRasin;
        FakeTrack = FakeUser.Track;
        FoodProducts = FakeData.FoodProducts;
        FakeTracks = FakeData.FakeTracks;
    }

    public ApplicationUser GetUser(Guid id=new())
    {
        return FakeUser;
    }

    public Track GetTrackByUser(ApplicationUser user)
    {
        return FakeTrack;
    }

    public IEnumerable<Track> GetTracksByCondition(Func<Track, bool> condition)
    {
        return FakeTracks.Where(condition);
    }

    public void UpdateUserInfo(ApplicationUser user)
    {
        FakeUser.CloneFrom(user);
    }

    public void UpdatePortionSize(FoodProduct updatedFp)
    {
        var fpList = (List<FoodProduct>)FoodProducts;
        var foodProduct = FoodProducts.FirstOrDefault(fp => fp.Id == updatedFp.Id);

        var indexOldFp = fpList.IndexOf(foodProduct);
        fpList[indexOldFp] = updatedFp;
    }
}