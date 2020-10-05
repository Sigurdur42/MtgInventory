# Introduction

Write something fancy here

# Open Tasks - Card Lookup

- Display card image either on click or on hover
- Add option to select print option (per card, per printing)
- Add option to add card to inventory

# Required packages / infrastructure

- .net core 3.1
- MatBlazor (nuget)
- LiteDb (nuget)

# MKM API

## Open tasks
- Use task scheduler for parallel work (See https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskscheduler?view=netcore-3.1)
- Check Price download on stock
- Auto download sets after startup
- Auto update sets if 
  * outdated
  * cards are missing
  * when downloading price data?
- Add UI for MKM tokens
- Handle MKM tokens (no typeline issue)
- Handle duplicate lands with different arts
  * handle difference in old cards between MKM and Scryfall
  * Handle different set codes for old stuff
- Display price in deck list view (USD / EUR from Scryfall, add MKM EUR as separate column)

- view model for stock items
- Display price in stock items list