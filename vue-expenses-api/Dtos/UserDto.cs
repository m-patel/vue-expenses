using System.Globalization;
using Newtonsoft.Json;

namespace vue_expenses_api.Dtos
{
    public class UserDto
    {
        protected UserDto()
        {
        }

        public UserDto(
            string firstName,
            string lastName,
            string fullName,
            string systemName,
            string email,
            string token,
            string refreshToken,
            string currencyRegionName,
            bool useDarkMode,
            string role,
            string region)
        {
            FirstName = firstName;
            LastName = lastName;
            FullName = fullName;
            SystemName = systemName;
            Email = email;
            Token = token;
            RefreshToken = refreshToken;
            CurrencyRegionName = currencyRegionName;
            UseDarkMode = useDarkMode;
            Theme = useDarkMode ? "dark" : "light";
            DisplayCurrency = new RegionInfo(currencyRegionName).CurrencySymbol;
            Role = role;
            Region = region;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string SystemName { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string CurrencyRegionName { get; }
        public bool UseDarkMode { get; set; }
        public string Theme { get; set; }
        public string DisplayCurrency { get; set; }
        public string Role { get; set; }
        public string Region { get; set; }

        [JsonIgnore]
        public byte[] Hash { get; set; }

        [JsonIgnore]
        public byte[] Salt { get; set; }
    }
}