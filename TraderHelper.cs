using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Utils.Cloners;

namespace LunnayalunaLotus
{
    [Injectable(TypePriority = OnLoadOrder.Preload + 1)]
    public class AddCustomTraderHelper(
        ISptLogger<AddCustomTraderHelper> logger,
        ICloner cloner,
        TradersTable tradersTable,
        LocaleTable localeTable)
    {
        public void SetTraderUpdateTime(
            TraderConfig traderConfig,
            TraderBase baseJson,
            int refreshTimeSecondsMin,
            int refreshTimeSecondsMax)
        {
            var traderRefreshRecord = new UpdateTime
            {
                TraderId = baseJson.Id,
                Seconds = new MinMax<int>(
                    refreshTimeSecondsMin,
                    refreshTimeSecondsMax)
            };

            traderConfig.UpdateTime.Add(traderRefreshRecord);
        }

        public void AddTraderWithEmptyAssortToDb(TraderBase traderDetailsToAdd)
        {
            var emptyTraderItemAssortObject = new TraderAssort
            {
                Items = [],
                BarterScheme = new Dictionary<MongoId, List<List<BarterScheme>>>(),
                LoyalLevelItems = new Dictionary<MongoId, int>()
            };

            var traderDataToAdd = new Trader
            {
                Assort = emptyTraderItemAssortObject,
                Base = cloner.Clone(traderDetailsToAdd),
                QuestAssort = new()
                {
                    { "Started", new() },
                    { "Success", new() },
                    { "Fail", new() }
                },
                Dialogue = []
            };

            if (tradersTable.ContainsKey(traderDetailsToAdd.Id))
            {
                logger.Warning($"Unable to add trader: {traderDetailsToAdd.Id}, it already exists.");
                return;
            }

            tradersTable.Add(traderDetailsToAdd.Id, traderDataToAdd);
        }

        public void AddTraderToLocales(
            TraderBase baseJson,
            string firstName,
            string description)
        {
            var locales = localeTable.Global;
            var newTraderId = baseJson.Id;
            var fullName = baseJson.Name;
            var nickName = baseJson.Nickname;
            var location = baseJson.Location;

            foreach (var (_, localeKvP) in locales)
            {
                localeKvP.AddTransformer(lazyloadedLocaleData =>
                {
                    lazyloadedLocaleData.Add(
                        $"{newTraderId} FullName",
                        fullName);

                    lazyloadedLocaleData.Add(
                        $"{newTraderId} FirstName",
                        firstName);

                    lazyloadedLocaleData.Add(
                        $"{newTraderId} Nickname",
                        nickName);

                    lazyloadedLocaleData.Add(
                        $"{newTraderId} Location",
                        location);

                    lazyloadedLocaleData.Add(
                        $"{newTraderId} Description",
                        description);

                    return lazyloadedLocaleData;
                });
            }
        }

        public void OverwriteTraderAssort(
            string traderId,
            TraderAssort newAssorts)
        {
            var traderToEdit = tradersTable.GetTrader(traderId);

            if (traderToEdit == null)
            {
                logger.Warning($"Unable to update assorts for trader: {traderId}, they couldn't be found on the server");
                return;
            }

            traderToEdit.Assort = newAssorts;
        }
    }
}
