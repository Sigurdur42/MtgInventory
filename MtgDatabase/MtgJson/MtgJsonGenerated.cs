using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MtgDatabase.MtgJson
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class Meta
    {
        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }
    }

    public class Foil
    {
        [JsonPropertyName("2020-12-15")]
        public double _20201215 { get; set; }

        [JsonPropertyName("2020-12-16")]
        public double _20201216 { get; set; }

        [JsonPropertyName("2020-12-17")]
        public double _20201217 { get; set; }

        [JsonPropertyName("2020-12-18")]
        public double _20201218 { get; set; }

        [JsonPropertyName("2020-12-19")]
        public double _20201219 { get; set; }

        [JsonPropertyName("2020-12-20")]
        public double _20201220 { get; set; }

        [JsonPropertyName("2021-01-04")]
        public double _20210104 { get; set; }

        [JsonPropertyName("2021-01-05")]
        public double _20210105 { get; set; }

        [JsonPropertyName("2021-01-06")]
        public double _20210106 { get; set; }

        [JsonPropertyName("2021-01-08")]
        public double _20210108 { get; set; }

        [JsonPropertyName("2021-01-09")]
        public double _20210109 { get; set; }

        [JsonPropertyName("2021-01-10")]
        public double _20210110 { get; set; }

        [JsonPropertyName("2021-01-11")]
        public double _20210111 { get; set; }

        [JsonPropertyName("2021-01-12")]
        public double _20210112 { get; set; }

        [JsonPropertyName("2021-01-13")]
        public double _20210113 { get; set; }

        [JsonPropertyName("2021-01-15")]
        public double _20210115 { get; set; }

        [JsonPropertyName("2021-01-16")]
        public double _20210116 { get; set; }

        [JsonPropertyName("2021-01-17")]
        public double _20210117 { get; set; }

        [JsonPropertyName("2021-01-18")]
        public double _20210118 { get; set; }

        [JsonPropertyName("2021-01-20")]
        public double _20210120 { get; set; }

        [JsonPropertyName("2021-01-22")]
        public double _20210122 { get; set; }

        [JsonPropertyName("2021-01-23")]
        public double _20210123 { get; set; }

        [JsonPropertyName("2021-01-24")]
        public double _20210124 { get; set; }

        [JsonPropertyName("2021-01-25")]
        public double _20210125 { get; set; }

        [JsonPropertyName("2021-01-26")]
        public double _20210126 { get; set; }

        [JsonPropertyName("2021-01-27")]
        public double _20210127 { get; set; }

        [JsonPropertyName("2021-01-28")]
        public double _20210128 { get; set; }

        [JsonPropertyName("2021-01-29")]
        public double _20210129 { get; set; }

        [JsonPropertyName("2021-01-30")]
        public double _20210130 { get; set; }

        [JsonPropertyName("2021-01-31")]
        public double _20210131 { get; set; }

        [JsonPropertyName("2021-02-01")]
        public double _20210201 { get; set; }

        [JsonPropertyName("2021-02-02")]
        public double _20210202 { get; set; }

        [JsonPropertyName("2021-02-03")]
        public double _20210203 { get; set; }

        [JsonPropertyName("2021-02-04")]
        public double _20210204 { get; set; }

        [JsonPropertyName("2021-02-05")]
        public double _20210205 { get; set; }

        [JsonPropertyName("2021-02-06")]
        public double _20210206 { get; set; }

        [JsonPropertyName("2021-02-07")]
        public double _20210207 { get; set; }

        [JsonPropertyName("2021-02-09")]
        public double _20210209 { get; set; }
    }

    public class Buylist
    {
        [JsonPropertyName("foil")]
        public Foil Foil { get; set; }

        [JsonPropertyName("normal")]
        public Normal Normal { get; set; }
    }

    public class Retail
    {
        [JsonPropertyName("foil")]
        public Foil Foil { get; set; }

        [JsonPropertyName("normal")]
        public Normal Normal { get; set; }
    }

    public class Cardkingdom
    {
        [JsonPropertyName("buylist")]
        public Buylist Buylist { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("retail")]
        public Retail Retail { get; set; }
    }

    public class Normal
    {
        [JsonPropertyName("2020-12-15")]
        public double _20201215 { get; set; }

        [JsonPropertyName("2020-12-16")]
        public double _20201216 { get; set; }

        [JsonPropertyName("2020-12-17")]
        public double _20201217 { get; set; }

        [JsonPropertyName("2020-12-18")]
        public double _20201218 { get; set; }

        [JsonPropertyName("2020-12-19")]
        public double _20201219 { get; set; }

        [JsonPropertyName("2020-12-20")]
        public double _20201220 { get; set; }

        [JsonPropertyName("2021-01-04")]
        public double _20210104 { get; set; }

        [JsonPropertyName("2021-01-05")]
        public double _20210105 { get; set; }

        [JsonPropertyName("2021-01-06")]
        public double _20210106 { get; set; }

        [JsonPropertyName("2021-01-08")]
        public double _20210108 { get; set; }

        [JsonPropertyName("2021-01-09")]
        public double _20210109 { get; set; }

        [JsonPropertyName("2021-01-10")]
        public double _20210110 { get; set; }

        [JsonPropertyName("2021-01-11")]
        public double _20210111 { get; set; }

        [JsonPropertyName("2021-01-12")]
        public double _20210112 { get; set; }

        [JsonPropertyName("2021-01-13")]
        public double _20210113 { get; set; }

        [JsonPropertyName("2021-01-15")]
        public double _20210115 { get; set; }

        [JsonPropertyName("2021-01-16")]
        public double _20210116 { get; set; }

        [JsonPropertyName("2021-01-17")]
        public double _20210117 { get; set; }

        [JsonPropertyName("2021-01-18")]
        public double _20210118 { get; set; }

        [JsonPropertyName("2021-01-20")]
        public double _20210120 { get; set; }

        [JsonPropertyName("2021-01-22")]
        public double _20210122 { get; set; }

        [JsonPropertyName("2021-01-23")]
        public double _20210123 { get; set; }

        [JsonPropertyName("2021-01-24")]
        public double _20210124 { get; set; }

        [JsonPropertyName("2021-01-25")]
        public double _20210125 { get; set; }

        [JsonPropertyName("2021-01-26")]
        public double _20210126 { get; set; }

        [JsonPropertyName("2021-01-27")]
        public double _20210127 { get; set; }

        [JsonPropertyName("2021-01-28")]
        public double _20210128 { get; set; }

        [JsonPropertyName("2021-01-29")]
        public double _20210129 { get; set; }

        [JsonPropertyName("2021-01-30")]
        public double _20210130 { get; set; }

        [JsonPropertyName("2021-01-31")]
        public double _20210131 { get; set; }

        [JsonPropertyName("2021-02-02")]
        public double _20210202 { get; set; }

        [JsonPropertyName("2021-02-03")]
        public double _20210203 { get; set; }

        [JsonPropertyName("2021-02-04")]
        public double _20210204 { get; set; }

        [JsonPropertyName("2021-02-05")]
        public double _20210205 { get; set; }

        [JsonPropertyName("2021-02-06")]
        public double _20210206 { get; set; }

        [JsonPropertyName("2021-02-07")]
        public double _20210207 { get; set; }

        [JsonPropertyName("2021-02-09")]
        public double _20210209 { get; set; }

        [JsonPropertyName("2021-02-01")]
        public double _20210201 { get; set; }
    }

    public class Cardmarket
    {
        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("retail")]
        public Retail Retail { get; set; }
    }

    public class Tcgplayer
    {
        [JsonPropertyName("buylist")]
        public Buylist Buylist { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("retail")]
        public Retail Retail { get; set; }
    }

    public class Paper
    {
        [JsonPropertyName("cardkingdom")]
        public Cardkingdom Cardkingdom { get; set; }

        [JsonPropertyName("cardmarket")]
        public Cardmarket Cardmarket { get; set; }

        [JsonPropertyName("tcgplayer")]
        public Tcgplayer Tcgplayer { get; set; }
    }

    public class _00010d56Fe385e358aed518019aa36a5
    {
        [JsonPropertyName("paper")]
        public Paper Paper { get; set; }
    }

    public class _0001e0d02dcd5640AadcA84765cf5fc9
    {
        [JsonPropertyName("paper")]
        public Paper Paper { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("00010d56-fe38-5e35-8aed-518019aa36a5")]
        public _00010d56Fe385e358aed518019aa36a5 _00010d56Fe385e358aed518019aa36a5 { get; set; }

        [JsonPropertyName("0001e0d0-2dcd-5640-aadc-a84765cf5fc9")]
        public _0001e0d02dcd5640AadcA84765cf5fc9 _0001e0d02dcd5640AadcA84765cf5fc9 { get; set; }
    }

    public class Root
    {
        [JsonPropertyName("meta")]
        public Meta Meta { get; set; }

        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }


}
