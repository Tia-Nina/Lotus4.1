using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Utils;
using System.Reflection;
using Path = System.IO.Path;

namespace LunnayalunaLotus;

public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "com.Luna.LunnayalunaLotus";
    public string Name { get; init; } = "Lotus";
    public string Author { get; init; } = "LunnayalunaLotus";
    public List<string>? Contributors { get; init; } = ["LycorisOni"];
    public SemanticVersioning.Version Version { get; init; } = new("1.7.4");
    public SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.0");
    public List<string>? Incompatibilities { get; init; } = null;
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = new()
    {
        { "com.wtt.commonlib", new SemanticVersioning.Range("~3.0") }
    };
    public string? Url { get; init; } = null;
    public string? License { get; init; } = "MIT";
    public bool HasPrepatcher { get; init; } = false;
}

[Injectable(TypePriority = OnLoadOrder.Preload + 1)]
public class LunaLotusJsonLoad(
    ISptLogger<LunaLotusJsonLoad> logger,
    ModHelper modHelper,
    ImageRouter imageRouter,
    TraderConfig traderConfig,
    RagfairConfig ragfairConfig,
    TimeUtil timeUtil,
    AddCustomTraderHelper addCustomTraderHelper
) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("Make sure to check the Lotus modpage for gunsmith task solutions");
        Console.ResetColor();

        cancellationToken.ThrowIfCancellationRequested();

        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var traderImagePath = Path.Combine(pathToMod, "res/Lotus.jpg");
        var traderBase = modHelper.GetJsonDataFromFile<TraderBase>(pathToMod, "data/base.json");

        imageRouter.AddRoute(
            traderBase.Avatar.Replace(".jpg", ""),
            traderImagePath);

        addCustomTraderHelper.SetTraderUpdateTime(
            traderConfig,
            traderBase,
            timeUtil.GetHoursAsSeconds(1),
            timeUtil.GetHoursAsSeconds(2));

        ragfairConfig.Traders.TryAdd(traderBase.Id, true);

        addCustomTraderHelper.AddTraderWithEmptyAssortToDb(traderBase);

        addCustomTraderHelper.AddTraderToLocales(
            traderBase,
            "Lotus",
            "A businesswoman who travels around conflict zones around the world.");

        var lotusAssort =
            modHelper.GetJsonDataFromFile<TraderAssort>(
                pathToMod,
                "data/assort.json");

        addCustomTraderHelper.OverwriteTraderAssort(
            traderBase.Id,
            lotusAssort);

        return Task.CompletedTask;
    }
}

[Injectable(TypePriority = OnLoadOrder.Preload + 1)]
public class EditDatabaseValues(
    LocationTable locationTable
) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var lab = locationTable.Laboratory;

        lab.Base.AccessKeys =
            lab.Base.AccessKeys.Append("6747b519aa6cb78b189e6081");

        lab.Base.AccessKeysPvE =
            lab.Base.AccessKeysPvE.Append("6747b519aa6cb78b189e6081");

        return Task.CompletedTask;
    }
}

[Injectable(TypePriority = OnLoadOrder.Preload + 2)]
public class Oni(
    WTTServerCommonLib.WTTServerCommonLib wttCommon
) : IOnLoad
{
    public async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var assembly = Assembly.GetExecutingAssembly();

        await wttCommon.CustomAssortSchemeService.CreateCustomAssortSchemes(assembly);
        await wttCommon.CustomQuestService.CreateCustomQuests(assembly);
        await wttCommon.CustomQuestZoneService.CreateCustomQuestZones(assembly);
        await wttCommon.CustomItemServiceExtended.CreateCustomItems(assembly);
        await wttCommon.CustomDialogueService.CreateCustomDialogues(assembly);
    }
}
