using PIM.Models.Taxonomy.Facets;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;

namespace PIM.Services.Entities.Taxonomy.Abstractions;

public interface IFacetRepository : IFacetService, IEntityRepository<Facet, FacetSearchObject, EntitySortBy, FacetIncludes>;
