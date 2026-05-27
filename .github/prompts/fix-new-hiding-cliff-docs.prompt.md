---
mode: agent
description: Add <remarks> warnings to the new-hiding members on ISpecification<T,TResult> documenting the dispatch cliff.
tools: [changes, codebase, editFiles, problems]
---

Add XML documentation `<remarks>` blocks to the three `new`-hiding members on
`ISpecification<T, TResult>` in
`src/Specification/Ferreimavi.Specification/ISpecification.cs`.

## Background

`ISpecification<T, TResult>` hides three members from `ISpecification<T>` using the
`new` keyword:

```csharp
public interface ISpecification<T, TResult> : ISpecification<T>
{
    new ISpecificationBuilder<T, TResult> Query { get; }
    new Func<IEnumerable<TResult>, IEnumerable<TResult>>? PostProcessingAction { get; }
    new IEnumerable<TResult> Evaluate(IEnumerable<T> entities);
}
```

`new`-hiding means that if a caller holds the object via an `ISpecification<T>`
variable, the base member is dispatched — returning `ISpecificationBuilder<T>` instead
of `ISpecificationBuilder<T, TResult>`, a `Func<IEnumerable<T>, ...>` instead of the
result-typed action, and unevaluated raw-entity results instead of the projected ones.
This is a silent behavioral cliff that is hard to spot in code review.

Currently all three members have only `<summary>` tags.

## What to change

In `src/Specification/Ferreimavi.Specification/ISpecification.cs`, add a `<remarks>`
block to each of the three hiding members.  The remarks must:

1. State that this member hides the base `ISpecification<T>` member via `new`.
2. Warn that dispatching through an `ISpecification<T>` variable invokes the base
   version and silently loses the `TResult`-specific behaviour.
3. Include a short concrete misuse example as a `<code>` block, e.g.:

```xml
/// <remarks>
/// This member hides <see cref="ISpecification{T}.Evaluate(IEnumerable{T})"/> via
/// <see langword="new"/>.  If the instance is held as <c>ISpecification&lt;T&gt;</c>
/// the base method is dispatched, returning <c>IEnumerable&lt;T&gt;</c> (unprojected)
/// instead of <c>IEnumerable&lt;TResult&gt;</c>.
/// <code>
/// ISpecification&lt;Order, OrderDto&gt; typed   = new MySpec();
/// ISpecification&lt;Order&gt;           untyped = typed;
///
/// typed.Evaluate(orders)   // IEnumerable&lt;OrderDto&gt; — correct
/// untyped.Evaluate(orders) // IEnumerable&lt;Order&gt;   — silent wrong type
/// </code>
/// Always reference projectable specifications through
/// <c>ISpecification&lt;T, TResult&gt;</c>.
/// </remarks>
```

Adapt the example text appropriately for `Query` and `PostProcessingAction`.

Do **not** change any logic.  Only add XML doc `<remarks>` to the three hiding members.
After editing, run `dotnet build src/Specification/Ferreimavi.Specification/Mango.Specifications.csproj`
and confirm zero errors and zero new warnings.
