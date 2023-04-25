using Robust.Shared.Configuration;

namespace Content.Shared.White;

/*
 * PUT YOUR CUSTOM VARS HERE
 * DO IT OR I WILL KILL YOU
 * with love, by Hail-Rakes
 */


[CVarDefs]
public sealed class WhiteCVars
{
    /*
 * Slang
    */

    public static readonly CVarDef<bool> ChatSlangFilter =
        CVarDef.Create("ic.slang_filter", true, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    /*
 * Sponsors
    */

    public static readonly CVarDef<string> SponsorsApiUrl =
        CVarDef.Create("sponsor.api_url", "", CVar.SERVERONLY);

    /*
 * Queue
    */

    public static readonly CVarDef<bool>
        QueueEnabled = CVarDef.Create("queue.enabled", false, CVar.SERVERONLY);


}
