Lotus 4.1 port
================

Target:
- SPT 4.1.5
- .NET 10.0
- WTT-ServerCommonLib 3.0.3

Main changes:
- AbstractModMetadata -> IModMetadata
- OnLoad() -> OnLoadAsync(CancellationToken)
- DatabaseService/ConfigServer removed in favor of injected tables/configs
- GetTables().Traders -> TradersTable.Traders
- GetTables().Locales.Global -> LocaleTable.Global
- GetLocations() -> LocationTable
- ISptLogger namespace moved to SPTarkov.Common.Models.Logging
- ModHelper moved to SPTarkov.Server.Core.Helpers.Server
- 4.1 load order uses Preload for database modifications
- WTT dependency updated to 3.0
