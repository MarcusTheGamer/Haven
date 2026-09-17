using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using System.Text.Json;

namespace Haven.Services;

public class SupabaseSessionHandler : IGotrueSessionPersistence<Session>
{
    private const string SessionKey = "supabase_session";

    public void SaveSession(Session session)
    {
        var json = JsonSerializer.Serialize(session);
        Preferences.Set(SessionKey, json);
    }

    public Session? LoadSession()
    {
        var json = Preferences.Get(SessionKey, string.Empty);

        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Session>(json);
        }
        catch
        {
            Preferences.Remove(SessionKey);
            return null;
        }
    }

    public void DestroySession()
    {
        Preferences.Remove(SessionKey);
    }
}