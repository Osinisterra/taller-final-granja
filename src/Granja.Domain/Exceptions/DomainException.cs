namespace Granja.Domain.Exceptions;

/// <summary>Error de validación del dominio (datos inválidos). Se traduce a HTTP 400.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

/// <summary>Violación de una regla de negocio por el estado actual. Se traduce a HTTP 409.</summary>
public abstract class ReglaNegocioException : DomainException
{
    protected ReglaNegocioException(string message) : base(message) { }
}

/// <summary>El recurso solicitado no existe. Se traduce a HTTP 404.</summary>
public class EntidadNoEncontradaException : DomainException
{
    public EntidadNoEncontradaException(string entidad, object clave)
        : base($"No se encontró {entidad} con identificador '{clave}'.") { }
}
