# Open Food Facts importer

Bulk-loads the [Open Food Facts](https://world.openfoodfacts.org/data) product dump into the PIM catalog on
SQL Server, as an alternative to the hand-built recipe seeder in `../Infrastructure`.

At full scale: ~4.7M packaged products, tens of millions of component and facet rows.

## Licensing

The Open Food Facts database is published under the **Open Database License (ODbL)**; its contents under the
Database Contents License. ODbL is share-alike: a database derived from it — including a seeded `.bak` or
`.db` file — stays ODbL, must credit Open Food Facts, and must be kept open if published. Local development
and a hosted demo are fine with attribution. Shipping the seeded database as a download carries the licence
with it.

## Before running

> **Check which server you are pointed at.** `Program.cs` calls `AddUserSecrets` *after*
> `Host.CreateApplicationBuilder`, so a connection string in user secrets overrides both `appsettings.json`
> and environment variables. The importer prints `Import target: <server> / <database>` before it writes
> anything, and refuses to run against a database that already holds products unless
> `TruncateBeforeImport` is explicitly set to `true`.

Requires a SQL Server connection string at `ConnectionStrings:SqlServer:PIM`. With only the SQLite string
set, the importer throws rather than falling back.

## Running

```bash
dotnet run --project app/PIM.DataGenerator -- --off
```

or set `"DataSource": "OpenFoodFacts"` in `appsettings.json`.

The first run downloads ~13 GB into `OpenFoodFacts:DataDir` (the products dump is ~12.6 GB compressed).
Downloads resume if interrupted; a complete file is left alone. Set `SkipDownload: true` to use a dump
staged by other means.

### Rate limiting

Open Food Facts limits requests per IP and asks every client to identify itself with a
`AppName/Version (ContactEmail)` User-Agent — **put a real contact address in `OpenFoodFacts:UserAgent`**.
Anonymous clients are throttled quickly, and repeat offenders can have the IP banned.

The downloader stays inside the limit by spacing requests `RequestDelaySeconds` apart, using one ranged GET
per file rather than a HEAD/GET pair, and skipping cached taxonomies younger than `TaxonomyMaxAgeHours`
without contacting the server. A 429 or 503 is retried with backoff, honouring `Retry-After`.

If you have just been throttled, wait a few minutes before retrying — the limit is per IP and applies
regardless of the client.

## Configuration (`OpenFoodFacts` section)

| Setting | Default | Purpose |
|---|---|---|
| `DataDir` | `./off-data` | Download cache. Needs ~13 GB free. |
| `MaxProducts` | *(all)* | Cap the number of packaged products **imported**. Start small. |
| `Language` | *(all)* | Keep only products written in this language, e.g. `"en"`. |
| `BatchSize` | 50000 | Rows per `SqlBulkCopy` batch. |
| `SkipDownload` | false | Use the files already in `DataDir`. |
| `UserAgent` | *(placeholder)* | Required by OFF. Set a real contact address. |
| `RequestDelaySeconds` | 4 | Minimum gap between HTTP requests. |
| `MaxRetries` | 6 | Attempts per file on 429/503, with backoff. |
| `TaxonomyMaxAgeHours` | 24 | Reuse a cached taxonomy this fresh without any request. |
| `TruncateBeforeImport` | **false** | Delete the existing catalog and taxonomy first. |
| `ImportBrands` | true | Import OFF brands as `Organization` parties + `ProductSupplier` links. |
| `TaxonomyIngredientsOnly` | true | Skip ingredient nodes OFF could not resolve against its taxonomy. |
| `RequireIngredients` | false | Skip products with no parsed ingredients. |
| `MaxComponentsPerIngredient` | 10 | Cap on a shared ingredient's sub-components. |
| `MaxIngredientDepth` | 3 | Cap on the depth of the shared ingredient graph. |

## How it maps

| Open Food Facts | PIM |
|---|---|
| product record | `Product` |
| nested `ingredients[]` tree | `Product` + `ProductComponent` (assembly → component) |
| `percent_estimate` | `ProductComponent.Quantity` |
| `percent_min` of 0 | `ProductComponent.IsOmittable` |
| taxonomy file (`categories`, `labels`, …) | `FacetGroup` |
| taxonomy entry | `Facet` |
| entry `parents` | `FacetLink` (many-to-many, so multi-parent DAGs survive) |
| `categories_tags`, `labels_tags`, … | `ProductFacet` |
| `brands_tags` | `Organization` + `ProductSupplier` |
| `product_quantity_unit` | `UnitType` |
| `created_t` / `last_modified_t` | `Created` / `LastModified` (UTC) |

## Where the recursion comes from

About 70% of OFF products carry at least one compound ingredient, nesting up to four levels — a filling that
contains ricotta and milk, a chocolate that contains cocoa and sugar. Each compound ingredient becomes a
`Product` in its own right whose components are its sub-ingredients, which is the same shape the recipe
seeder built by hand from dish → partial dish → ingredient.

Ingredient products are **shared** across the catalog: every product mentioning `en:ricotta` points at one
row, mirroring the recipe seeder's canonical ingredients. Two consequences follow, and both are handled:

- **Cycles.** Two products can disagree about whether dough contains flour or flour contains dough. Since
  `GetProductOffspring` is a recursive CTE, a loop would make it churn to its level cap on every call, so a
  candidate edge is rejected when the child can already reach the parent.
- **Fan-out.** A generic node like "Flour" would otherwise collect a child from every product that ever
  spelled out what its flour contains, and because the tree functions enumerate paths rather than nodes,
  that inflates every offspring query. `MaxComponentsPerIngredient` and `MaxIngredientDepth` bound it. With
  the defaults, a product's full component tree averages ~100 edges; without them it was ~960.

The counts of links dropped for each reason are logged at the end of a run.

## Filtering by language

`Language: "en"` keeps only products whose own language is English, which is what makes `product_name`
an English name.

The per-language `product_name_en` field looks like the more direct test, and is not: contributors
routinely copy the local name into it, so a French product can carry `"Huile d'olive Monini"` as its
English name. Matching on the record's `lang`/`lc` avoids that. A product also needs a non-empty name to
qualify — a record marked English with no name has no English name.

Measured over the first 3,000 products of the dump: 3,000 imported unfiltered with 52 accented titles,
versus 2,615 imported with `"en"` and 2 accented titles (both English records containing an accented word).

`MaxProducts` counts products **imported**, not records read, so it still means what it says when a
language filter is rejecting most of the dump — the scan simply reads further into the file to reach it.

## Indexes

Nothing index-related lives here — the indexes a catalog at this scale needs are declared in the model
(`PimDbContext.OnModelCreating`), because they belong to the schema rather than to whatever seeded it.
New databases get them from `EnsureCreated()`. An existing database does not, since `EnsureCreated()` is a
no-op once the database exists — run `src/PIM.Data/PerformanceIndexes.sql` against those.

## Notes on the implementation

- `EfBulkWriter<T>` derives the target table, column names and value mapping from the **EF Core model**, so
  a mapping change in `PimDbContext` carries through without touching the importer. It also applies the
  model's `MaxLength` and decimal `Scale` to each value — which is what keeps over-long OFF titles and
  128-character taxonomy keys from failing a batch against `Facet.Code`'s `MaxLength(32)`.
- Ids are assigned by the importer and written with `KEEPIDENTITY`, from separate ranges for packaged
  products, ingredient products, brands and ad-hoc facets, so foreign keys resolve without a round-trip per
  row. Identity seeds are reset afterwards, or the application's next insert would collide.
- Foreign keys are disabled during the load, because component rows are written before the ingredient
  products they reference, and re-enabled with `WITH CHECK` — which re-validates every row and is the
  import's own integrity test.
- `NormalizedTitle` / `NormalizedContent` are written through Regira's `INormalizer`, since bulk copy
  bypasses the normalizing interceptor. Without them `?q=` search would silently match nothing.
