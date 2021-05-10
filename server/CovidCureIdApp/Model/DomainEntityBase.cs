using System.Reflection;
using System.Text.Json.Serialization;
using CovidCureIdApp.DataAccess.Support;

namespace CovidCureIdApp.Model
{
    /// <summary>
    ///     Abstract base class for domain entities which are persisted to the database.
    /// </summary>
    public abstract class DomainEntityBase
    {
        /// <summary>
        ///     A system assigned ID of the entity.
        /// </summary>
        [JsonPropertyName("id")]
        public virtual string Id { get; set; }

        /// <summary>
        ///     A name associated with the entity.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        ///     The partition key for this type.
        /// </summary>
        public abstract string PartitionKey { get; }

        /// <summary>
        ///     Gets the container name based on either the type name or an explicit container name using the Container attribute.
        /// </summary>
        [JsonIgnore]
        public virtual string ContainerName
        {
            get
            {
                ContainerAttribute containerAttribute = GetType().GetCustomAttribute<ContainerAttribute>();

                return string.IsNullOrEmpty(containerAttribute?.Name) ? GetType().Name : containerAttribute.Name;
            }
        }

        /// <summary>
        ///     Returns the type name.
        /// </summary>
        public virtual string TypeName {
            get => GetType().Name;
            set { /* Needed for serialization */ }
        }
    }
}
