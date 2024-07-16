using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Modells.User {

    public class SessionToken {

        /// <summary></summary>
        [JsonInclude, JsonPropertyName("Guid"), JsonProperty(nameof(Guid))]
        public Guid Guid { get; set; } = new Guid();
        /// <summary></summary>
        [JsonInclude, JsonPropertyName("UserGuidGuid"), JsonProperty(nameof(UserGuid))]
        public Guid UserGuid { get; set; } = new Guid();
        /// <summary></summary>
        [JsonInclude, JsonPropertyName("ExpaireDate"), JsonProperty(nameof(ExpaireDate))]
        public long ExpaireDate { get; set; } = 0;

        /// <summary></summary>
        public SessionToken() { }

        /// <summary></summary>
        /// <param name="guid"></param>
        /// <param name="userGuid"></param>
        /// <param name="exaireDate"></param>
        public SessionToken(Guid guid, Guid userGuid, long exaireDate) {
            Guid = guid;
            UserGuid = userGuid;
            ExpaireDate = exaireDate;
        }
    }
}
