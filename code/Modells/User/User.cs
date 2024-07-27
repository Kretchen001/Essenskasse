using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Modells.User {

    /// <summary></summary>
    public class User {

        /// <summary></summary>
        [JsonInclude, JsonPropertyName("Guid"), JsonProperty(nameof(Guid))]
        public Guid Guid { get; set; } = new Guid();
        /// <summary>Internal Name, should not be changed.</summary>
        [JsonInclude, JsonPropertyName("Name"), JsonProperty(nameof(Name))]
        public string Name { get; set; } = string.Empty;
        /// <summary>Displayed Name.</summary>
        [JsonInclude, JsonPropertyName("UserName"), JsonProperty(nameof(UserName))]
        public string UserName { get; set; } = string.Empty;
        /// <summary></summary>
        [JsonInclude, JsonPropertyName("Pwd"), JsonProperty(nameof(Pwd))]
        public string Pwd { get; set; } = string.Empty;
        /// <summary></summary>
        [JsonInclude, JsonPropertyName("Email"), JsonProperty(nameof(Email))]
        public string Email { get; set; } = string.Empty;
        /// <summary></summary>
        [JsonInclude, JsonPropertyName("Role"), JsonProperty(nameof(Role))]
        public int Role { get; set; } = 0;

        /// <summary></summary>
        public User() { }

        /// <summary></summary>
        /// <param name="guid"></param>
        /// <param name="name"></param>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        /// <param name="email"></param>
        /// <param name="role"></param>
        [System.Text.Json.Serialization.JsonConstructor]
        public User(Guid guid, string name, string userName, string pwd, string email, int role) {
            Guid = guid;
            Name = name;
            UserName = userName;
            Pwd = pwd;
            Email = email;
            Role = role;
        }

        /// <summary></summary>
        /// <param name="name"></param>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        /// <param name="email"></param>
        /// <param name="role"></param>
        public User(string name, string userName, string pwd, string email, int role) {
            Guid = Guid.NewGuid();
            Name = name;
            UserName = userName;
            Pwd = pwd;
            Email = email;
            Role = role;
        }
    }
}
