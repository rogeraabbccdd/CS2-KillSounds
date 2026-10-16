using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using Clientprefs.API;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API.Modules.Entities;

namespace CS2KillSounds;

public class Config : BasePluginConfig
{
    [JsonPropertyName("ConfigVersion")]
    public override int Version { get; set; } = 1;
    [JsonPropertyName("SoundEventFiles")]
    public string SoundEventFiles { get; set; } = "soundevents/killsounds.vsndevts";
    [JsonPropertyName("DefaultVolume")]
    public float DefaultVolume { get; set; } = 0.4f;
    [JsonPropertyName("KnifeSound")]
    public string KnifeSound { get; set; } = "Kill.mcdonald";
    [JsonPropertyName("FireSound")]
    public string FireSound { get; set; } = "Kill.burn";
    [JsonPropertyName("HeGrenadeSound")]
    public string HeGrenadeSound { get; set; } = "Kill.kobe";
}

public class Plugin : BasePlugin
{
    public override string ModuleName => "CS2 Kill Sounds";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Kento";
    public override string ModuleDescription => "Kill Sound";
    public Config Config { get; set; } = new();
    public int VolumeCookie = -1;
    public readonly PluginCapability<IClientprefsApi> cookieCapabilty = new("Clientprefs");
    public IClientprefsApi? CLIENT_PREFS_API;

    public override void Load(bool hotReload)
    {
        RegisterListener<Listeners.OnServerPrecacheResources>(OnPrecacheResources);
    }
    public override void Unload(bool hotReload)
    {
        RemoveListener<Listeners.OnServerPrecacheResources>(OnPrecacheResources);
    }
    public void OnConfigParsed(Config config)
    {
        Config = config;
    }
    public override void OnAllPluginsLoaded(bool hotReload)
    {
        LoadClientPrefs();

        if (hotReload)
        {
            ReloadClientprefs();
        }
    }
    public void LoadClientPrefs()
    {
        try
        {
            CLIENT_PREFS_API = cookieCapabilty.Get() ?? throw new Exception("Clientprefs api not found");
            CLIENT_PREFS_API.OnDatabaseLoaded += OnClientprefDatabaseReady;
            CLIENT_PREFS_API.OnPlayerCookiesCached += OnPlayerCookiesCached;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to load CleintprefsApi! | {ex.Message}");
        }
    }
    public void UnloadClientprefis()
    {
        if (CLIENT_PREFS_API == null)
            return;

        CLIENT_PREFS_API.OnDatabaseLoaded -= OnClientprefDatabaseReady;
        CLIENT_PREFS_API.OnPlayerCookiesCached -= OnPlayerCookiesCached;
    }
    public void ReloadClientprefs()
    {
    }
    public async void OnClientprefDatabaseReady()
    {
        if (CLIENT_PREFS_API == null) return;

        try
        {
            VolumeCookie = await CLIENT_PREFS_API.RegPlayerCookieAsync("Volume", "Player volume preference", CookieAccess.CookieAccess_Public);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to register Volume cookie | {ex.Message}");
            return;
        }
        if (VolumeCookie == -1)
        {
            Logger.LogError("Failed to register Volume cookie");
        }
    }

    public void OnPlayerCookiesCached(CCSPlayerController player)
    {
    }
    public void OnPrecacheResources(ResourceManifest resource)
    {
        resource.AddResource(Config.SoundEventFiles);
    }
    [GameEventHandler]
    public HookResult OnEventPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        // Server.PrintToChatAll(@event.Weapon);
        if (@event.Weapon.Contains("knife") || @event.Weapon.Contains("bayonet"))
        {
            PlaySound(Config.KnifeSound);
        }
        else if (@event.Weapon == "inferno")
        {
            PlaySound(Config.FireSound);
        }
        else if (@event.Weapon == "hegrenade")
        {
            PlaySound(Config.HeGrenadeSound);
        }

        return HookResult.Continue;
    }
    public void PlaySound(string sound)
    {
        foreach (var player in Utilities.GetPlayers().Where(p => !p.IsBot && !p.IsHLTV))
        {
            float volume = GetPlayerVolume(player);
            // player.PrintToChat($"PlaySound Volume: {volume}");

            if (volume > 0)
            {
                RecipientFilter filter = [player];
                Server.NextFrame(() => player.EmitSound(sound, filter, volume));
            }
        }
    }
    public float GetPlayerVolume(CCSPlayerController player)
    {
        float volume = Config.DefaultVolume;

        if (CLIENT_PREFS_API != null && VolumeCookie != -1)
        {
            try
            {
                string volStr = CLIENT_PREFS_API.GetPlayerCookie(player, VolumeCookie);
                // player.PrintToChat($"GetPlayerVolume Volume: {volStr}");
                if (!string.IsNullOrEmpty(volStr) && float.TryParse(volStr, out float parsed))
                    volume = parsed;
            }
            catch (Exception)
            {
                // Clientprefs throws when the player is invalid or cookies are not cached yet
            }
        }
        return volume;
    }
}

