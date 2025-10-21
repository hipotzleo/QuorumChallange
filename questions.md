# Quorum Challenge – Questions

---

### 1️ : Discuss your strategy and decisions implementing the application

The main goal was to create a **clean, reliable, and extensible** system to process CSV data, handle edge cases, and display aggregated legislative insights.

**Key decisions**
- **CSV reading:**  
  Implemented a custom `CsvReaderService<T>` using `CsvHelper` in streaming mode (`ReadAsync` + `GetRecord<T>`).  
  Invalid records are **logged and skipped** instead of throwing exceptions.
- **Header normalization:**  
  Uses case-insensitive matching (`PrepareHeaderForMatch`) to support `snake_case` headers.
- **Caching:**  
  `IMemoryCache` ensures data is loaded once and reused across requests, improving performance.
- **Aggregation:**  
  Performed entirely in-memory with `Dictionary` lookups for O(n) performance.  
  Sponsors resolved via `GetValueOrDefault` to prevent `KeyNotFoundException`.
- **UI & APIs:**  
  Minimal APIs + Razor Pages = small, focused endpoints and a clean user interface.
- **Error handling:**  
  Designed to fail softly. Any malformed data only affects that record, not the whole dataset.

**Complexity:**  
- O(n) reading and aggregation time  
- Constant memory usage (streaming + cache)  
- Separation of concerns through `Domain`, `Infra`, and `App` layers

---

### 2️ : How would you adapt your solution for new columns like “Bill Voted On Date” or “Co-Sponsors”?

- Add optional fields to models (`DateTime? VotedOn`, `List<int>? CoSponsors`)
- Extend existing `ClassMaps` with new column names (e.g., `Map(m => m.VotedOn).Name("voted_on")`)
- Keep the reader tolerant — unknown columns are ignored automatically
- Add new aggregation logic if needed (e.g., grouping by date or counting co-sponsors)
- Extend UI/DTOs to display or expose the new information

This approach ensures backward compatibility with older datasets.

---

### 3️ : What if you needed to generate CSVs instead of reading them?

I would add an `ICsvWriter<T>` service using the same mapping classes.  
It would:
- Serialize objects to CSV using `CsvWriter`
- Write directly to a `MemoryStream`
- Return a downloadable file in endpoints like:
  ```csharp
  return Results.File(stream, "text/csv", "bills_summary.csv");
  ```

This approach keeps the architecture symmetric, the same maps define both reading and writing.

---

### 4️ : How long did you spend on the assignment?

Roughly **7–8 hours total**:
| Task | Time |
|------|------:|
| Project setup (Razor Pages, Swagger, DI) | ~1.5h |
| CSV infrastructure (maps, reader, error handling) | ~2.0h |
| Aggregation logic and endpoints | ~1.5h |
| Memory cache implementation | ~0.5h |
| Testing, documentation, cleanup | ~1.5h |

I Used github Copilot to speed up boilerplate code and focused on core logic and architecture
About unit tests, I wrote key tests for CSV reading and aggregation, but more coverage could be added with more time.

---

**Author:** Leonardo Feitosa  
**Date:** October 2025  
**Tech Stack:** .NET 8, C#, CsvHelper, IMemoryCache, Razor Pages, xUnit
