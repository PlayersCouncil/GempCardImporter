using FileHelpers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace GeneratorCSVHandler
{
	[DelimitedRecord("\t")]
	[IgnoreFirst]
	public class CardRow
	{
		public static Dictionary<string, string> SetMap = new Dictionary<string, string>()
		{
			["V0"] = "100",
			["V1"] = "101",
			["V2"] = "102",
			["0"] = "00",
			["1"] = "01",
			["2"] = "02",
			["3"] = "03",
			["4"] = "04",
			["5"] = "05",
			["6"] = "06",
			["7"] = "07",
			["8"] = "08",
			["9"] = "09",
			["10"] = "10",
			["11"] = "11",
			["12"] = "12",
			["13"] = "13",
			["14"] = "14",
			["15"] = "15",
			["16"] = "16",
			["17"] = "17",
			["18"] = "18",
			["19"] = "19",
		};

		public static Dictionary<string, string> ErrataSetMap = new Dictionary<string, string>()
		{
			["V0"] = "100",
			["V1"] = "101",
			["V2"] = "102",
			["0"] = "50",
			["1"] = "51",
			["2"] = "52",
			["3"] = "53",
			["4"] = "54",
			["5"] = "55",
			["6"] = "56",
			["7"] = "57",
			["8"] = "58",
			["9"] = "59",
			["10"] = "60",
			["11"] = "61",
			["12"] = "62",
			["13"] = "63",
			["14"] = "64",
			["15"] = "65",
			["16"] = "66",
			["17"] = "67",
			["18"] = "68",
			["19"] = "69",
		};

		public static Dictionary<string, string> PlaytestSetMap = new Dictionary<string, string>()
		{
			["V0"] = "150",
			["V1"] = "151",
			["V2"] = "152",
			["0"] = "70",
			["1"] = "71",
			["2"] = "72",
			["3"] = "73",
			["4"] = "74",
			["5"] = "75",
			["6"] = "76",
			["7"] = "77",
			["8"] = "78",
			["9"] = "79",
			["10"] = "80",
			["11"] = "81",
			["12"] = "82",
			["13"] = "83",
			["14"] = "84",
			["15"] = "85",
			["16"] = "86",
			["17"] = "87",
			["18"] = "88",
			["19"] = "89",

		};

		public static Dictionary<string, string> PathSetMap = new Dictionary<string, string>()
		{
			["V0"] = "vset0",
			["V1"] = "vset1",
			["V2"] = "V2",
		};

		public static string GetSet(string set, bool errata, bool playtest)
		{
			if (errata && playtest)
			{
				if (PlaytestSetMap.ContainsKey(set))
				{
					return $"{PlaytestSetMap[set]}";
				}

				return $"{set}";
			}

			if (errata) // !playtest
			{
				if (ErrataSetMap.ContainsKey(set))
				{
					return $"{ErrataSetMap[set]}";
				}

				return $"{set}";
			}

			if (SetMap.ContainsKey(set))
			{
				return $"{SetMap[set]}";
			}

			return $"{set}";
		}

		public string GetGempID(bool errata, bool playtest)
		{
			String set = GetSet(set_num, errata, playtest);

			if (set == "00")
				return $"0_{card_num}";

			return $"{Regex.Replace(set, @"^0*", "")}_{card_num}";
		}

		public static Dictionary<string, string> CultureSides = new Dictionary<string, string>()
		{
			["dwarven"] = "Free Peoples",
			["elven"] = "Free Peoples",
			["gandalf"] = "Free Peoples",
			["gondor"] = "Free Peoples",
			["rohan"] = "Free Peoples",
			["shire"] = "Free Peoples",
			["isengard"] = "Shadow",
			["dunland"] = "Shadow",
			["moria"] = "Shadow",
			["moria_balrog"] = "Shadow",
			["raider"] = "Shadow",
			["sauron"] = "Shadow",
			["ringwraith"] = "Shadow",
			["wraith"] = "Shadow",
			["orc"] = "Shadow",
			["man"] = "Shadow",
			["men"] = "Shadow",
			["evil man"] = "Shadow",
			["evil men"] = "Shadow",
			["uruk"] = "Shadow",
			["urukhai"] = "Shadow",
			["uruk-hai"] = "Shadow",
			["uruk hai"] = "Shadow",
		};


		public string GetImagePath(bool errata, bool playtest)
		{
			string mappedSet = SetMap[set_num];
			if (mappedSet.Length == 2)
			{
				if (errata)
				{
					return $"errata/{id}.jpg";
				}
				else
				{
					if (notes.ToLower().Contains("tengwar"))
					{
						return $"decipher/LOTR{mappedSet}{card_num.Value.ToString("000")}T.jpg";
					}
					switch (rarity)
					{
						case "RF":
							return $"decipher/LOTR{mappedSet}F{card_num.Value.ToString("00")}.jpg";
						case "O":
							return $"decipher/LOTR{mappedSet}O{card_num.Value.ToString("00")}.jpg";
						default:
							return $"decipher/LOTR{mappedSet}{card_num.Value.ToString("000")}.jpg";
					}
				}
			}

			return $"sets/{PathSetMap[set_num]}/{id}.jpg";
		}

		public string id { get; set; }
		public string image_name { get; set; }
		public string unique { get; set; }
		public bool IsUnique => unique == "T" || unique == "Y";
        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }

        public string FullName
		{
			get
			{
				var strippedTitle = title.Replace("<br>", " ").Replace(@"\", " ").Replace("  ", " ");

                if (!String.IsNullOrWhiteSpace(subtitle))
				{
					return $"{strippedTitle}, {subtitle}";
				}

				return strippedTitle;
			}
		}
		public string title { get; set; }
		public string NormalizedTitle => Regex.Replace(RemoveDiacritics(title), @"[^A-Za-z]", "");

        public string subtitle { get; set; }
        public string culture { get; set; }
		public string Side
		{
			get
			{
				if (culture.ToLower().Contains("gollum"))
				{
					if (tags.ToLower().Contains("shadow") || culture.ToLower().Contains("shadow"))
						return "Shadow";

					return "Free Peoples";
				}
				if (template.ToLower() == "site" || card_type.ToLower() == "onering" || template.ToLower() == "onering")
					return null;

				return CultureSides[culture.ToLower()];
			}
		}

		public string SanitizedCulture
		{
			get
			{
				if (culture.ToLower().Contains("gollum"))
					return "Gollum";

				if (culture.ToLower().Contains("moria"))
					return "Moria";

				return culture.ToLower().Replace("ringwraith", "wraith").FirstCharUpper();
			}
		}
		public string template { get; set; }
		public string card_type { get; set; }
		public bool IsCharacter => card_type.ToLower() == "minion" || card_type.ToLower() == "companion" || IsAlly;
        public bool IsAlly => card_type.ToLower() == "ally";
        public bool IsMinion => card_type.ToLower() == "minion";
        public bool IsItem => card_type.ToLower() == "possession" || card_type.ToLower() == "artifact";
		public bool IsEvent => card_type.ToLower() == "event";
		public bool IsSite => template.ToLower() == "site" || card_type.ToLower() == "sanctuary" || card_type.ToLower() == "site";
		public bool IsOneRing => template.ToLower() == "onering";
		public string display_type { get; set; }
        public string card_subtype { get; set; }
		public string EventSubtype {
			get
			{
				if (!String.IsNullOrWhiteSpace(card_subtype))
					return card_subtype.ToLower().FirstCharUpper();

                var text = game_text.Replace("<b>", "").Replace("</b>", "");
                var matches = Regex.Matches(text, @"(Fellowship|Shadow|Maneuver|Archery|Assignment|Skirmish|Regroup):");

				return String.Join(", ", matches.Select(x => x.Groups[1].Value).ToList());
            }
		} 
        public int? twilight { get; set; }
		public int? strength { get; set; }
		public int? vitality { get; set; }
		public int? resistance { get; set; }
		public string signet { get; set; }
		public string site { get; set; }

		public string SiteNum => site.Replace("F", "").Replace("T", "").Replace("K", "");

        public string AllyHome()
		{
			var homes = new List<string>();
			foreach(string home in site.Split('&')) 
			{
				var match = Regex.Match(home.Trim(), @"(\d)(\w)?");
				if(!match.Success) 
					continue;

				string sitenum = match.Groups[1].Value;
				string blockAbbr = match.Groups[2].Value.ToUpper();
				string block = "";

                switch (blockAbbr)
                {
                    case "K":
                        block = "king";
                        break;
                    case "T":
                        block = "towers";
                        break;
                    case "F":
                    case "":
					default:
                        block = "fellowship";
                        break;
                }

				homes.Add($"{block},{sitenum}");
			}

			return String.Join(",", homes);
		}
		public string CollectorsInfo => $"{set_num}{rarity}{card_num}";

		public string set_num { get; set; }
		public string rarity { get; set; }
		public int? card_num { get; set; }
		public string game_text { get; set; }

		public List<string> Keywords()
		{
			var text = game_text.Replace("<b>", "").Replace("</b>", "");
			var matches = Regex.Matches(text, @"(?<![\w>\]\)'] )(?<=( |^))(Hunter \d+|Ambush \(\d+\)|Damage \+\d+|Defender \+\d+|Toil \d+|[\w-]+)\.");
			if (matches.Count == 0)
				return new List<string>();

			//Numeric keywords are reduced to Ambush+3, Toil+3, Hunter+3, etc.
			return matches.Select(x => x.Groups[2].Value.Replace(" +", "+").Replace("(", "").Replace(")", "")).ToList();
		}

		public List<string> EventKeywords()
		{
			var keywords = Keywords();
			if (card_type.ToLower() != "event")
				return keywords;

			if(!String.IsNullOrWhiteSpace(card_subtype))
			{
				keywords.Add(card_subtype);
			}

            var text = game_text.Replace("<b>", "").Replace("</b>", "");
            var matches = Regex.Matches(text, @"(Fellowship|Shadow|Maneuver|Archery|Assignment|Skirmish|Regroup|Response):");
            if (matches.Count == 0)
                return keywords.Distinct().ToList();

			keywords.AddRange(matches.Select(x => x.Groups[1].Value));
            return keywords.Distinct().ToList();
        }

		public List<string> SupportKeywords()
		{
			var list = EventKeywords();
			if(card_subtype.ToLower().Contains("support") || Regex.IsMatch(game_text.ToLower(), @"(plays|play|place) to your support area\."))
			{
				list.Add("Support Area");
			}

			if(tags.ToLower().Contains("ring_bearer"))
			{
				list.Add("CAN_START_WITH_RING");
			}

			return list;
		}

		public String Target()
		{
            var text = game_text.Replace("<b>", "").Replace("</b>", "");
            var matches = Regex.Matches(text, @"(Bearer must be|Plays on) (.*?)\.");
            if (matches.Count == 0)
                return null;

			var target = matches[0].Groups[2].Value;
			if(target.ToLower().StartsWith("a site you control"))
			{
				target = "ControlledSite";
            }
			else if (target.ToLower().StartsWith("a site"))
			{
				target = "site";				
			}
            else if (target.ToLower().StartsWith("a ") || target.ToLower().StartsWith("the ") || target.ToLower().StartsWith("that "))
            {
				//Do nothing--leave target to be the exact matched text.
            }
			else
			{
				//Assume it's a name if it made it this far
				target = $"title({target})";
			}

			return target;
        }
		public string lore { get; set; }

		public static string SanitizeNanDECKString(string original)
        {
			string output = original;
			output = output.Replace(@"\", "<br>");
			output = output.Replace("/_", " ");
			output = output.Replace("---", "—");
			output = output.Replace("--", "–");
			return output;
        }
		public string promo_text { get; set; }
		public string errata_date { get; set; }
		public string custom_icon { get; set; }
		public string tags { get; set; }
		public double? game_text_width { get; set; }
		public string game_text_color { get; set; }
		public double? game_text_scale { get; set; }
		public double? game_text_spacing { get; set; }
		public double? lore_gap { get; set; }
		public double? lore_spacing { get; set; }
		public double? title_area_scale { get; set; }
		public double? title_text_scale { get; set; }
		public double? subtitle_text_scale { get; set; }
		public string title_color_override { get; set; }
		public string type_color_override { get; set; }
		public double? type_text_scale { get; set; }
		public string icon_text_color { get; set; }
		public string twilight_color { get; set; }
		public string border_color { get; set; }
		public string notes { get; set; }

        [FieldOptional]
		public string Origin { get; set; }

        public bool HasCulture()
        {
            return template.ToLower() != "site" && card_type.ToLower() != "sanctuary" && template.ToLower() != "onering" && !String.IsNullOrWhiteSpace(card_type);
        }
    }
}
