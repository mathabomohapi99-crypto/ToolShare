namespace ToolShare.Api;

// WHY: one base type lets the central handler recognise "our" business
// errors with a single check, and anything else is treated as a real 500.
public abstract class DomainException(string message) : Exception(message);

// WHY: maps to 404. The thing the caller referred to doesn't exist.
public sealed class NotFoundException(string message) : DomainException(message);

// WHY: maps to 409. The request is well-formed, but the current state
// of the world doesn't allow it (tool already checked out).
public sealed class ConflictException(string message) : DomainException(message);

// WHY: maps to 422. Same Idempotency-Key reused with a DIFFERENT payload.
// The request is understood but can't be honoured without ambiguity.
public sealed class IdempotencyKeyReuseException(string message) : DomainException(message);