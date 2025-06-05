using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;


namespace RekordRest
{
  public partial class RekordRestClient
  {
    static partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
      settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
      settings.Converters.Add(new IsoDateTimeConverter
      {
        DateTimeStyles = DateTimeStyles.AdjustToUniversal,
        // "yyyy-MM-ddTHH:mm:ss.fffK" for 3 decimal places and 'Z'
        // If the endpoint wants NO milliseconds when they are zero,
        // this might be too much, but for ".650z" it should be fine.
        DateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ",
        Culture = CultureInfo.InvariantCulture // Always use invariant culture
      });
    }
  }

}
