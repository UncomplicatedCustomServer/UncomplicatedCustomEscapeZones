using System.ComponentModel;

namespace UncomplicatedEscapeZones;

internal class Config
{
    [Description("Do enable the developer (debug) mode?")]
    public bool Debug { get; set; } = false;

    [Description(
        "If false the UCS credit tag system won't be activated. PLEASE DON'T DEACTIVATE IT as LOTS OF PEOPLE WORKED ON THIS PLUGIN completly for FREE!")]
    public bool EnableCreditTags { get; set; } = true;
}