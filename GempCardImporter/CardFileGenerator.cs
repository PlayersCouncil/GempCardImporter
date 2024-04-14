using GeneratorCSVHandler;

using Microsoft.VisualBasic.CompilerServices;

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GempCardImporter
{
	public class CardFileGenerator
	{
		public static bool Errata = false;
		public static bool Playtest = false;

		private static string SiteBlockConvert(string site)
		{
			if (String.IsNullOrWhiteSpace(site))
				return "SHADOWS";

			site = site.ToUpper();

			if (site.Contains("T"))
				return "TWO_TOWERS";

			if (site.Contains("K"))
				return "KING";

			return "FELLOWSHIP";
        }

		private static String Enumify(String input)
		{
			if (String.IsNullOrWhiteSpace(input))
				return "";

			return input.ToUpper().Replace(" ", "_").Replace("-", "_");
		}

		private static String ApplyDynamicArray(string json, string fieldName, string defaultItem, List<string> items)
		{
            if (items.Count() == 1)
            {
                json += $@"
		{fieldName}: {items.First()}
";
            }
            else if (items.Count() > 0)
            {
                json += $@"
		{fieldName}: [
			{String.Join("\n\t\t\t", items)}
		]
";
            }
            else if(!String.IsNullOrWhiteSpace(defaultItem))
            {
                json += $@"
		#{fieldName}: {defaultItem}
";
            }

			return json;
        }
		public static string GetJsonObject(CardRow card)
		{
			string json = $@"
	XXGEMPIDXX: {{
		cardInfo: {{
			imagePath: XXIMAGEPATHXX
			javaClass: {(Errata || Playtest ? "false" : "true")}
";
			if(Errata || Playtest)
			{
				json += $@"
			parentId: XXPARENTIDXX
			parentPath: {(Errata ? "errata/pc" : "promos/placeholder")}
";
			}
			
			json += $@"
			version: XXVERSIONXX
			collInfo: {card.CollectorsInfo}
			rarity: {card.rarity}
			setNum: ""{card.set_num}""
			cardNum: {card.card_num}
			style: Standard
		}}

		title: {card.title.Replace("<br>", " ").Replace(@"\", " ").Replace("  ", " ")}
{(!String.IsNullOrWhiteSpace(card.subtitle) ? $"\t\tsubtitle: {card.subtitle.Replace(@"\", " ").Replace("  ", " ")}" : "")}
		unique: {(card.IsUnique ? "true" : "false")}
{(card.Side != null ? $"\t\tside: {card.Side}" : "")}
{(card.HasCulture() ? $"\t\tculture: {card.SanitizedCulture}" : "")}
		twilight: {(card.twilight.HasValue ? card.twilight : 0)}
		type: {(String.IsNullOrWhiteSpace(card.card_type) ? card.template.ToLower().Replace("onering", "The One Ring").FirstCharUpper() : card.card_type.ToLower().Replace("sanctuary", "site")).FirstCharUpper()}";

			switch (String.IsNullOrWhiteSpace(card.card_type) ? card.template.ToLower() : card.card_type.ToLower())
			{
				case "possession":
				case "artifact":
					json += $@"
{(card.strength.HasValue ? $"\t\tstrength: {card.strength}" : "")}
{(card.vitality.HasValue ? $"\t\tvitality: {card.vitality}" : "")}
{(card.resistance.HasValue ? $"\t\tresistance: {card.resistance}" : "")}
{(!String.IsNullOrWhiteSpace(card.site) ? $"\t\tsite: {card.site}" : "")}
{(!String.IsNullOrWhiteSpace(card.card_subtype) && !card.card_subtype.ToLower().Contains("support") ? $"\t\titemclass: {card.card_subtype.ToLower().FirstCharUpper()}" : "")}
";
					var target = card.Target();
					if (!String.IsNullOrWhiteSpace(target))
					{
						json += $@"
		#target: {target}
";
					}
					break;

				case "onering":
				case "follower":
				case "condition":
					json += $@"
{(card.strength.HasValue ? $"\t\tstrength: {card.strength}" : "")}
{(card.vitality.HasValue ? $"\t\tvitality: {card.vitality}" : "")}
{(card.resistance.HasValue ? $"\t\tresistance: {card.resistance}" : "")}
{(!String.IsNullOrWhiteSpace(card.site) ? $"\t\tsite: {card.site}" : "")}
";
					break;

				case "ally":
					json += $@"
{(!String.IsNullOrWhiteSpace(card.site) ? $"\t\tallyHome: {card.AllyHome()}" : "")}
		race: {card.card_subtype.ToLower().FirstCharUpper()}
{(card.strength.HasValue ? $"\t\tstrength: {card.strength}" : "")}
{(card.vitality.HasValue ? $"\t\tvitality: {card.vitality}" : "")}
";
					break;

				case "companion":
					json += $@"
		race: {card.card_subtype.ToLower().FirstCharUpper()}
{(card.strength.HasValue ? $"\t\tstrength: {card.strength}" : "")}
{(card.vitality.HasValue ? $"\t\tvitality: {card.vitality}" : "")}
{(!String.IsNullOrWhiteSpace(card.signet) ? $"\t\tsignet: {card.signet.ToLower().FirstCharUpper()}" : "")}
{(card.resistance.HasValue ? $"\t\tresistance: {card.resistance}" : $"\t\tresistance: 6")}
";
					break;

				case "minion":
					json += $@"
		race: {card.card_subtype.ToLower().FirstCharUpper()}
{(card.strength.HasValue ? $"\t\tstrength: {card.strength}" : "")}
{(card.vitality.HasValue ? $"\t\tvitality: {card.vitality}" : "")}
{(!String.IsNullOrWhiteSpace(card.site) ? $"\t\tsite: {card.site}" : "")}
"; 
					break;

				case "event":
					var eventKeywords = card.EventKeywords();
					json = ApplyDynamicArray(json, "keywords", "Regroup", eventKeywords);
					break;

				case "site":
				case "sanctuary":
					json += $@"
{(!String.IsNullOrWhiteSpace(card.site) ? $"\t\tsite: {card.site}\n" : "")}";

					if(String.IsNullOrWhiteSpace(card.site))
                    {
						json += $"\t\tblock: Shadows\n";
					}
					else if (card.site.Contains("T"))
					{
						json += $"\t\tblock: Towers\n";
					}
					else if (card.site.Contains("K"))
					{
						json += $"\t\tblock: King\n";
					}
					else 
					{
						json += $"\t\tblock: Fellowship\n";
					}
				
					json += $@"
		direction: {(card.tags.Contains("right_arrow") ? "Right" : "Left")}
";
					break;

			}

			if(card.card_type != null && card.card_type != "event") //Event keywords are handled above
			{
				var keywords = card.SupportKeywords();
                json = ApplyDynamicArray(json, "keywords", null, keywords);
			}

			json += @"
		/*requires: {
			
		}
";

			if (card.card_type.ToLower() == "event")
			{
				if(card.card_subtype.ToLower().Contains("response"))
				{
					json += @"
		effects: {
			type: responseEvent
			trigger: {
				
			}
			cost: {
				
			}
			effect: [
				{
					
				}
			]
		]*/
";
				}
				else
				{
					json += @"
		effects: {
			type: event
			cost: {
				
			},
			effect: [
				{
					
				}
			]
		}*/
";
				}
				
			}
			else
			{

				json += @"
		effects: [
			{
				
			}
			{
				
			}
		]*/
";
			}

			json += @$"
		gametext: {(!String.IsNullOrWhiteSpace(card.game_text) ? CardRow.SanitizeNanDECKString(card.game_text) : "\"\"")}
		lore: {(!String.IsNullOrWhiteSpace(card.lore) ? CardRow.SanitizeNanDECKString(card.lore) : "\"\"")}
		promotext: {(!String.IsNullOrWhiteSpace(card.promo_text) ? CardRow.SanitizeNanDECKString(card.promo_text) : "\"\"")}
		alts: {{
			promos: {{
			}}
			errata: {{
			}}
		}}
	}}";

			json = json.Replace("\r", "");
			json = Regex.Replace(json, @"\n\n+", "\n");
			return json;
		}

		public static string GetJavaTestFile(CardRow card)
		{
			string java = "";

			if(Errata)
            {
				java += $"package com.gempukku.lotro.cards.unofficial.pc.errata.set{CardRow.GetSet(card.set_num, false, false)};\n\n";
			}
			else
            {
                java += $"package com.gempukku.lotro.cards.official.set{CardRow.GetSet(card.set_num, false, false)};\n\n";
                //java += $"package com.gempukku.lotro.cards.unofficial.pc.vsets.set_v0{card.set_num.ToLower().Replace("v", "")};\n\n";
			}
			java += $@"
import com.gempukku.lotro.cards.GenericCardTestHelper;
import com.gempukku.lotro.common.*;
import com.gempukku.lotro.game.CardNotFoundException;
import com.gempukku.lotro.game.PhysicalCardImpl;
import com.gempukku.lotro.logic.decisions.DecisionResultInvalidException;
import com.gempukku.lotro.logic.modifiers.MoveLimitModifier;
import org.junit.Test;

import java.util.HashMap;

import static org.junit.Assert.*;

public class Card_{CardRow.GetSet(card.set_num, Errata, Playtest)}_{card.card_num.Value.ToString("000")}_{(Errata ? "Errata" : "")}Tests
{{

	protected GenericCardTestHelper GetScenario() throws CardNotFoundException, DecisionResultInvalidException {{
		return new GenericCardTestHelper(
				new HashMap<>()
				{{{{
					put(""card"", ""{card.GetGempID(Errata, Playtest)}"");
					// put other cards in here as needed for the test case
				}}}},
				GenericCardTestHelper.FellowshipSites,
				GenericCardTestHelper.FOTRFrodo,
				GenericCardTestHelper.RulingRing
		);
	}}

	@Test
	public void {Regex.Replace(card.title, @"[^A-Za-z]", "")}StatsAndKeywordsAreCorrect() throws DecisionResultInvalidException, CardNotFoundException {{

		/**
		* Set: {card.set_num}
		* Title: {card.FullName}
		* Unique: {(card.IsUnique ? "True" : "False")}
		* Side: {GetSideEnum(card)}
		* Culture: {card.SanitizedCulture}
		* Twilight Cost: {card.twilight}
		* Type: {(String.IsNullOrWhiteSpace(card.card_type) ? card.template.ToLower() : card.card_type.ToLower())}
		* Subtype: {card.card_subtype}
{(card.strength.HasValue ? $"\t\t* Strength: " + card.strength.Value.ToString() : "")}
{(card.vitality.HasValue ? $"\t\t* Vitality: " + card.vitality.Value.ToString() : "")}
{(card.resistance.HasValue ? $"\t\t* Resistance: " + card.resistance.Value.ToString() : "")}
{(!string.IsNullOrWhiteSpace(card.signet) ? $"\t\t* Signet: " + card.signet : "")}
{(!string.IsNullOrWhiteSpace(card.site) ? $"\t\t* Site Number: " + card.site : "")}
		* Game Text: {card.game_text.Replace("\n", "\n\t\t* ").Replace("\\", "\n\t\t* \t")}
		*/

		var scn = GetScenario();

		var card = scn.GetFreepsCard(""card"");

		assertEquals(""{card.title.Replace("<br>", " ").Replace(@"\", " ").Replace("  ", " ")}"", card.getBlueprint().getTitle());
		assertEquals(""{card.subtitle}"", card.getBlueprint().getSubtitle());
		assert{(card.IsUnique ? "True" : "False")}(card.getBlueprint().isUnique());
		assertEquals(CardType.{(String.IsNullOrWhiteSpace(card.card_type) ? card.template.ToLower() : card.card_type.ToLower()).Replace("onering", "THE_ONE_RING").Replace(" ", "_").ToUpper().Replace("SANCTUARY", "SITE")}, card.getBlueprint().getCardType());";

            if (card.HasCulture())
			{
				java += $@"
		assertEquals(Side.{GetSideEnum(card)}, card.getBlueprint().getSide());
		assertEquals(Culture.{Enumify(card.SanitizedCulture)}, card.getBlueprint().getCulture());";
			}



			if (card.IsCharacter && !String.IsNullOrWhiteSpace(card.card_subtype))
			{
				java += $@"
		assertEquals(Race.{Enumify(card.card_subtype).Replace("Û", "U")}, card.getBlueprint().getRace());";
			}

			if(card.IsItem && (!String.IsNullOrWhiteSpace(card.card_subtype) || card.SupportKeywords().Any(x => x.ToLower().Contains("support"))))
			{
                java += $@"
		assertTrue(card.getBlueprint().getPossessionClasses().contains(PossessionClass.{Enumify(card.card_subtype)}));";
            }

            var keywords = card.SupportKeywords();
            if (card.IsEvent)
			{
				keywords = card.EventKeywords();
			}

            if (keywords.Count > 0)
			{
				foreach (var keyword in keywords)
				{
                    java += $@"
		assertTrue(scn.HasKeyword(card, Keyword.{Enumify(keyword)}));";
                }
			}

            if (card.twilight.HasValue)
            {
                java += $@"
		assertEquals({card.twilight}, card.getBlueprint().getTwilightCost());";
            }

            if (card.strength.HasValue)
            {
                java += $@"
		assertEquals({card.strength}, card.getBlueprint().getStrength());";
            }

            if (card.vitality.HasValue)
            {
                java += $@"
		assertEquals({card.vitality}, card.getBlueprint().getVitality());";
            }

            if (card.resistance.HasValue)
            {
                java += $@"
		assertEquals({card.resistance}, card.getBlueprint().getResistance());";
            }
            
            if(!string.IsNullOrWhiteSpace(card.signet))
			{
                java += $@"
		assertEquals(Signet.{Enumify(card.signet)}, card.getBlueprint().getSignet()); ";
            }

			if(!string.IsNullOrWhiteSpace(card.site))
			{
				if(card.IsAlly)
				{
					string homes = card.AllyHome();
					var parts = homes.Split(",");
                    if (parts.Count() > 0)
                    {
                        java += $@"
		assertEquals({card.SiteNum}, card.getBlueprint().getAllyHomeSiteNumbers()[0]);
		assertEquals(SitesBlock.{SiteBlockConvert(card.site)}, card.getBlueprint().getAllyHomeSiteBlock());";
                    }
                    if (parts.Count() > 2)
                    {
                        java += $@"
		assertEquals({card.SiteNum}, card.getBlueprint().getAllyHomeSiteNumbers()[1]);";
                    }
                }
				else
				{
                    java += $@"
		assertEquals({card.SiteNum}, card.getBlueprint().getSiteNumber());";
                }
                
            }
            

			if (card.card_type.ToLower() == "site")
			{
                java += $@"
		assertEquals(SitesBlock.{SiteBlockConvert(card.site)}, card.getBlueprint().getSiteBlock());";
			}

			java += $@"
	}}

	// Uncomment any @Test markers below once this is ready to be used
	//@Test
	public void {Regex.Replace(card.title, @"[^A-Za-z]", "")}Test1() throws DecisionResultInvalidException, CardNotFoundException {{
		//Pre-game setup
		var scn = GetScenario();

		var card = scn.GetFreepsCard(""card"");
		scn.FreepsMoveCardToHand(card);

		scn.StartGame();
		scn.FreepsPlayCard(card);

		assertEquals({card.twilight ?? 0}, scn.GetTwilight());
	}}
}}
";
			java = java.Replace("\r", "");
			return Regex.Replace(java, "\n\n+", "\n\n").Replace("\n\n\t\t*", "\n\t\t*");
		}

		public static string FormatWikiString(string sheetStr)
        {
			string output = sheetStr;

			output = output.Replace("<b>", "'''");
			output = output.Replace("</b>", "'''");
			output = output.Replace("<i>", "''");
			output = output.Replace("</i>", "''");
			output = output.Replace("<br>", "\n\n");
			output = output.Replace("\\", "\n\n");
			output = Regex.Replace(output, @"\((\d+)\)", "{{T|$1}}");
			output = Regex.Replace(output, @"\[(\w+)\]", "{{CI|$1}}");

			return output;

		}
		public static string GetWikiErrataFile(CardRow card)
        {
			string errata = "";

			errata += $@"
{{{{Errata
|ID={card.id.Replace("_card", "").Replace("E", "S").Replace("SN", "EN")}
|BaseCardID={Regex.Replace(card.id, @"\.\d+_card", "").Replace("E", "S").Replace("SN", "EN")}.0
|Subset=S
|Revision={Regex.Replace(card.id, @"LOTR-EN.*\.(\d+).*", "$1")}
|ReleaseDate={card.errata_date.Replace(".", "-")}
|ReleaseNotes=
|ImageFilename={card.id}.jpg
|IsPhysical=no
|IsPlayable=yes
|IsUnique={(card.unique.Contains("T") ? "yes" : "no")}
|Title={card.title.Replace("<br>", " ").Replace(@"\", " ").Replace("  ", " ")}
|Subtitle={card.subtitle}
|Subtypes={card.card_subtype}
|TwilightCost={card.twilight}
|StrengthMod={card.strength}
|VitalityMod={card.vitality}
|SiteNumMod={card.site}
|ResistanceMod={card.resistance}
|Strength={card.strength}
|Vitality={card.vitality}
|SiteNum={card.site}
|Resistance={card.resistance}
|FormattedGameText={FormatWikiString(card.game_text)}
|Notes=
}}}}

";
			return errata;
        }

		public static string GetJSFileLine(string gempID, string imagePath, CardRow card)
		{
			string output = "";
			string baseURL = "https://i.lotrtcgpc.net/";
			string leftside = $"'{gempID}'".PadRight(9);

			// //Gimli, Son of Gloin (Errata)
			output += $"\n\t// {card.FullName} ({card.CollectorsInfo})";
            if (Errata)
            {
                output += " [Errata]";
            }

            if (Playtest)
            {
                output += " [Playtest]";
            }

            //'151_39': 'https://i.lotrtcgpc.net/errata/LOTR-ENV1E039.1_card.jpg',
            output += $"\n\t{leftside}: '{baseURL}{imagePath}',";

			return output;
        }

        private static string GetSideEnum(CardRow card)
        {
			if (card.Side == null)
				return ""; 

			//So we have "FREE_PEOPLE" and not "FREE_PEOPLES" for esoteric gemp reasons
			return card.Side.ToUpper().Replace(" ", "_").Replace("LES", "LE");
        }

		public static void GenerateFilesForCards(IEnumerable<CardRow> cards, string path)
		{
			Directory.CreateDirectory(Path.Combine(path, "hjson"));
			Directory.CreateDirectory(Path.Combine(path, "tests"));
			Directory.CreateDirectory(Path.Combine(path, "wiki"));

			var cardsByCulture = new Dictionary<string, Dictionary<string, List<string>>>();

			
			string suffix = "";
			if(Errata)
            {
				suffix += "-errata";
			}
			if(Playtest)
            {
				suffix += "-playtest";
			}
			string extraPath = ""; //end in / if it's not blank

			string jsfile = "{";
            
			foreach (var card in cards)
			{
				if (String.IsNullOrWhiteSpace(card.title))
					continue;

				string setNum = CardRow.GetSet(card.set_num, false, false);
                string set = $"set{setNum}";
				string java = GetJavaTestFile(card);
				File.WriteAllText(Path.Combine(path, "tests",  $"Card_{setNum}_{card.card_num.Value.ToString("000")}_{(Errata ? "Errata" : "")}Tests.java"), java);

				string wiki = GetWikiErrataFile(card);
				File.WriteAllText(Path.Combine(path, "wiki", $"{card.id}.txt"), wiki);

				string culture = card.culture;
				if (card.template.ToLower() == "site")
				{
					culture = "Sites";
				}
				if (card.template.ToLower() == "onering")
				{
					culture = "OneRing";
				}
                if (culture.ToLower().Contains("gollum"))
                {
                    culture = "Gollum";
                }
                if (culture.ToLower().Contains("balrog"))
                {
                    culture = "Moria";
                }

                culture = culture.ToLower().FirstCharUpper();

				if (!cardsByCulture.ContainsKey(culture))
				{
					cardsByCulture[culture] = new Dictionary<string, List<string>>();
				}
				if (!cardsByCulture[culture].ContainsKey(set))
				{
                    cardsByCulture[culture][set] = new List<string>();
				}
				string json = GetJsonObject(card);
				string imageName = card.GetImagePath(Errata, Playtest);
				string gempID = card.GetGempID(Errata, Playtest);

                if (Errata)
                {
					imageName = imageName.Replace("errata/", $"errata/{extraPath}");
					json = json.Replace("XXVERSIONXX", "1?");
                }
				else
                {
					imageName = imageName.Replace("sets/", $"sets/{extraPath}");
					json = json.Replace("XXVERSIONXX", "0");
				}

				json = json.Replace("XXIMAGEPATHXX", imageName);
				json = json.Replace("XXGEMPIDXX", gempID);
				json = json.Replace("XXPARENTIDXX", card.GetGempID(false, false));

				cardsByCulture[culture][set].Add(json);

				jsfile += GetJSFileLine(gempID, imageName, card);
            }

			jsfile += "\n}";
            File.WriteAllText(Path.Combine(path,  $"PC_cards_addenda.js"), jsfile);

            foreach (string culture in cardsByCulture.Keys)
			{
				foreach (string set in cardsByCulture[culture].Keys)
				{
					File.WriteAllText(Path.Combine(path, "hjson", $"{set}-{culture}{suffix}.hjson"), "{\n" + string.Join("\n", cardsByCulture[culture][set]) + "\n}\n");
				}
			}
		}
	}
}
