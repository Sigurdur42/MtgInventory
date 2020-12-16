# Introduction

Write something fancy here

#Links
- https://github.com/AdrienTorris/awesome-blazor
- https://mudblazor.com/components/alert (MudBlazor components)

# Open Tasks - Card Lookup

- Display card image either on click or on hover
- Add option to select print option (per card, per printing)
- Add option to add card to inventory

# Required packages / infrastructure

- .net core 3.1
- LiteDb (nuget)

# MKM API

## Open tasks
- Use task scheduler for parallel work (See https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskscheduler?view=netcore-3.1)

- remove online only sets


- Specific message when MKM API is not configured. Also avoid accessing the API in that case. This must be centralized
- Check Price download on stock
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

- get unmapped cards by looking for empty mkm ID