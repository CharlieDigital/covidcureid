namespace CovidCureIdApp.DataAccess.Support;

/// <summary>
///     Attribute which defines the container that the entity belongs to.  For subclasses, this attribute must be
///     explicitly applied to determine whether it should use its own container or the base class container.
/// </summary>
/// <remarks>
///     Request units are provisioned by containers and various other aspects of the database are constrained by the
///     container such as stored procedures.  It is possible to allow RUs to cross containers, but this limits throughput
///     versus a single container.  In general, most of the data should reside in a single container.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class ContainerAttribute : Attribute
{
    /// <summary>
    ///     The name of the container.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    ///     Creates an instance of the attribute using an implicit name (the type name).
    /// </summary>
    public ContainerAttribute()
    {

    }

    /// <summary>
    ///     Creates an instance of the attribute using the specified container name.
    /// </summary>
    /// <param name="explicitName">The explicit name of the container</param>
    public ContainerAttribute(string explicitName)
    {
        Name = explicitName;
    }
}
