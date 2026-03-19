using Regira.DAL.Paging;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using Regira.Globalization;
using Regira.Utilities;
using Webshop.Core.Abstractions;
using Country = Webshop.Models.Countries.Country;
using CountryEntity = Webshop.Models.Countries.Country;

namespace Webshop.Services.Entities.Countries;

public class CountryRepository(ICultureContext cultureContext) : IEntityService<Country, string, SearchObject<string>>
{
    public Task<CountryEntity?> Details(string id)
    {
        var country = CountryUtility.GetCountry(id);
        var item = country != null ? Convert(country) : null;
        return Task.FromResult(item);
    }

    public Task<IList<CountryEntity>> List(SearchObject<string>? so = null, PagingInfo? pagingInfo = null)
    {
        var query = CountryUtility.GetAllCountries();
        if (!string.IsNullOrWhiteSpace(so?.Q))
        {
            query = query
                .Select(x => new { country = x, weight = CalculateWeight(x, so) })
                .Where(x => x.weight > 0)
                .OrderByDescending(x => x.weight)
                .ThenBy(x => x.country.Title)
                .Select(x => x.country);
        }
        else
        {
            query = query
                .OrderBy(x => x.Iso2Code == cultureContext.CountryCode ? 0 : 1)
                .ThenBy(x => x.Title);
        }

        var itemQuery = query.Select(Convert);
        if (pagingInfo?.PageSize > 0)
        {
            itemQuery = itemQuery.PageItems(pagingInfo.PageSize, pagingInfo.Page - 1);
        }
        var items = itemQuery.ToList();
        return Task.FromResult(items as IList<CountryEntity>);
    }


    Task<IList<CountryEntity>> IEntityReadService<CountryEntity, string>.List(object? so, PagingInfo? pagingInfo)
        => List(Convert(so), pagingInfo);
    public Task<long> Count(object? so)
        => Count(Convert(so));
    public async Task<long> Count(SearchObject<string>? so)
        => (await List(so)).Count;


    public int CalculateWeight(Regira.Globalization.Country item, SearchObject<string>? so)
    {
        var weight = 0;
        if (!string.IsNullOrWhiteSpace(so?.Q))
        {
            // title in current language
            var itemTitle = (item.NamesByLanguage?.ContainsKey(cultureContext.Culture.Name) == true
                                ? item.NamesByLanguage![cultureContext.Culture.Name]
                                : null)
                            ?? item.Title;
            if (item.Iso2Code.Equals(so.Q, StringComparison.InvariantCultureIgnoreCase))
            {
                weight += 20;
            }
            var titles = item.NamesByLanguage!.Values;
            if (titles.Any(title => title.Equals(so.Q, StringComparison.InvariantCultureIgnoreCase)))
            {
                weight += 25;
            }
            else if (so.Q.Length > 1 && titles.Any(title => so.Q.Equals(GetInitials(title), StringComparison.InvariantCultureIgnoreCase)))
            {
                weight += 15;
            }
            if (itemTitle.StartsWith(so.Q, StringComparison.InvariantCultureIgnoreCase))
            {
                weight += 20;
            }
            else if (item.NamesByLanguage!.Values.Any(title => title.StartsWith(so.Q, StringComparison.InvariantCultureIgnoreCase)))
            {
                weight += 10;
            }
            else if (titles.Any(title => title.Contains(so.Q, StringComparison.InvariantCultureIgnoreCase)))
            {
                weight += 5;
            }
        }
        // Bonus points for default item
        if (weight > 0 && item.Iso2Code.Equals(cultureContext.CountryCode, StringComparison.InvariantCultureIgnoreCase))
        {
            weight += 5;
        }

        return weight;
    }
    public CountryEntity Convert(Regira.Globalization.Country item)
    {
        return new CountryEntity
        {
            Id = item.Iso2Code,
            Code = item.Iso2Code,
            Title = (item.NamesByLanguage?.TryGetValue(cultureContext.Culture.Name, out var value) is true ? value : null) ?? item.Title,
            IsDefault = item.Iso2Code == cultureContext.CountryCode
        };
    }
    protected virtual SearchObject<string>? Convert(object? so)
    {
        if (so == null)
        {
            return null;
        }
        return so as SearchObject<string> ?? ObjectUtility.Create<SearchObject<string>>(so);
    }
    static string? GetInitials(string? input)
        => string.IsNullOrWhiteSpace(input) ? input : string.Join("", input.Split(' ').Select(s => s.First()));


    #region NotSupported
    public Task Add(CountryEntity item)
    {
        throw new NotImplementedException();
    }
    public Task<CountryEntity?> Modify(CountryEntity item)
    {
        throw new NotImplementedException();
    }
    public Task Save(CountryEntity item)
    {
        throw new NotImplementedException();
    }
    public Task Remove(CountryEntity item)
    {
        throw new NotImplementedException();
    }
    public Task<int> SaveChanges(CancellationToken token = new())
    {
        throw new NotImplementedException();
    }
    #endregion
}