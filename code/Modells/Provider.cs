using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Modells {

    /// <summary>Represent a (food) provider.</summary>
    public class Provider {

        /// <summary>Id of the provider</summary>
        [JsonInclude, JsonPropertyName("Guid"), JsonProperty(nameof(Guid))]
        public Guid Guid { get; set; } = new Guid();
        /// <summary>Name of the provider</summary>
        [JsonInclude, JsonPropertyName("Designation"), JsonProperty(nameof(Designation))]
        public string Designation { get; set; } = default!;

        /// <summary></summary>
        public Provider() { }

        /// <summary></summary>
        /// <param name="Guid"></param>
        /// <param name="designation"></param>
        [System.Text.Json.Serialization.JsonConstructor]
        public Provider(Guid Guid, string designation) {
            this.Guid = Guid;
            this.Designation = designation;
        }

        /// <summary></summary>
        /// <param name="designation"></param>
        public Provider(string designation) {
            Guid = Guid.NewGuid();
            this.Designation = designation;
        }

        /// <summary></summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool Equals(Provider b) {
            if (this.Guid.Equals(b.Guid)) {
                if (this.Designation.Equals(b.Designation)) {
                    return true;
                }
            }
            return false;
        }
    }
}
