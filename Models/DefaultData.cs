namespace Projektverktyg.Models;

public static class DefaultData
{
    public static List<Risk> GetDefaultRisks() => new()
    {
        new()
        {
            Id = 1, Title = "Kompetensförsörjning/Bemanning", Selected = true,
            Description = "Risk att projektet saknar rätt systemspecifik kompetens och/eller tillräcklig bemanning.",
            Cause = "Otillräcklig kapacitetsplanering, beroende av nyckelpersoner, otydlig kompetensprofil per aktivitet.",
            Consequence = "Förseningar, ökade kostnader, lägre kvalitet, fler omtag.",
            Category = "Resurser", Probability = 1, Impact = 5,
            Owner = "Projektledare", Strategy = "Minska",
            Preventive = "Definiera kompetensbehov per leverans, säkra seniora nyckelroller, gör resursestimat per aktivitet.",
            Fallback = "Skala ned scope till MVP, omplanera milstolpar, ta in extern systemspecialist.",
            Trigger = "Bemanningsluckor i plan, låg tillgänglighet, blockerade tasks pga kunskapsbrist."
        },
        new()
        {
            Id = 2, Title = "Tight tidsplan", Selected = true,
            Description = "Risk att tidsplanen är för tajt för att hinna analys, bygg, test och införande.",
            Cause = "Underskattade aktiviteter/beroenden, för lite buffert, sen start på beslut.",
            Consequence = "Försening av milstolpar/go-live, ökade kostnader, kvalitetsbrister.",
            Category = "Tid", Probability = 1, Impact = 4,
            Owner = "Projektledare", Strategy = "Minska",
            Preventive = "Baselina planen med realistiska estimat, lägg in buffert och tydliga beslutspunkter.",
            Fallback = "Replanera go-live, fasa leverans i etapper, skala ned scope till MVP.",
            Trigger = "Upprepade förseningar i delaktiviteter, många blockerade tasks."
        },
        new()
        {
            Id = 3, Title = "Budget", Selected = true,
            Description = "Risk att projektets budget inte räcker för att leverera överenskommen scope och kvalitet.",
            Cause = "Underskattade estimat, otydlig scope/krav, för låg buffert.",
            Consequence = "Budgetöverskridande, behov av omprioritering/scope-reduktion.",
            Category = "Kostnad", Probability = 2, Impact = 4,
            Owner = "Projektledare", Strategy = "Minska",
            Preventive = "Baselina budget mot tydlig scope och WBS, följ upp burn rate veckovis.",
            Fallback = "Skala ned scope till MVP, flytta delar till senare fas.",
            Trigger = "Utfall > plan 2 veckor i rad, fler change requests utan finansiering."
        },
        new()
        {
            Id = 4, Title = "Otydligt scope", Selected = true,
            Description = "Risk att projektets omfattning (scope) är otydlig eller tolkas olika.",
            Cause = "Bristfälliga krav/acceptanskriterier, otydliga mål och avgränsningar.",
            Consequence = "Scope creep, fler ändringar och omtag, förseningar.",
            Category = "Scope", Probability = 3, Impact = 5,
            Owner = "Projektledare", Strategy = "Minska",
            Preventive = "Definiera mål, avgränsningar, kravlista med prioritet (MoSCoW).",
            Fallback = "Frysa scope för kommande release, leverera MVP och flytta resterande till fas 2.",
            Trigger = "Många 'vi trodde'-diskussioner, hög andel öppna frågor."
        },
        new()
        {
            Id = 5, Title = "Beroenden till tredje part", Selected = false,
            Description = "Risk att externa parter inte levererar i tid eller enligt förväntan.",
            Cause = "Otydliga krav/ansvar, långa ledtider, begränsad supportkapacitet hos tredje part.",
            Consequence = "Förseningar, blockerade aktiviteter, omplanering och ökade kostnader.",
            Category = "Leverantör", Probability = 2, Impact = 4,
            Owner = "Projektledare", Strategy = "Minska",
            Preventive = "Kartlägg alla beroenden och ägare, säkra kontaktvägar och SLA.",
            Fallback = "Aktivera workaround (mock/stub), fasa leverans, byt leverantör om möjligt.",
            Trigger = "Väntan på åtkomst/miljö >3 arbetsdagar, uteblivna svar."
        },
        new()
        {
            Id = 6, Title = "Integration mot nya system", Selected = false,
            Description = "Risk att integration mot nya eller okända system innebär tekniska osäkerheter.",
            Cause = "Bristande dokumentation, omogen API-version, okända begränsningar.",
            Consequence = "Förseningar pga prototyp/omtag, ökad utvecklings- och testinsats.",
            Category = "Teknik", Probability = 2, Impact = 5,
            Owner = "Teknisk lead", Strategy = "Minska",
            Preventive = "Gör tidig teknisk förstudie/spike (PoC), verifiera autentisering och nyckelflöden.",
            Fallback = "Bygg fallback med manuell import/export, fasa integrationen.",
            Trigger = "PoC fallerar, sena API-ändringar, återkommande fel i test."
        },
        new()
        {
            Id = 7, Title = "Samarbetet med kunden", Selected = true,
            Description = "Risk att kundens nyckelpersoner inte har avsatt tid och att kommunikationen brister.",
            Cause = "Kunden upptagen i linjeverksamhet, otydliga roller/mandat.",
            Consequence = "Förseningar, felaktiga antaganden och omtag, blockerad utveckling/test.",
            Category = "Intressenter", Probability = 2, Impact = 4,
            Owner = "Projektledare", Strategy = "Minska",
            Preventive = "Säkra sponsor och tydliga roller, boka återkommande forum.",
            Fallback = "Eskalera via sponsor/styrgrupp, pausa arbete som kräver kundinput.",
            Trigger = "Uteblivna svar >3 arbetsdagar, inställda möten."
        },
        new()
        {
            Id = 8, Title = "Okända beroenden & hårda deadlines", Selected = false,
            Description = "Risk att projektets största beroenden och hårda deadlines inte är kartlagda.",
            Cause = "Ingen gemensam beroendeanalys, otydlig ansvarsfördelning.",
            Consequence = "Oväntade blockerare, missade deadlines, omfattande omplanering.",
            Category = "Planering", Probability = 1, Impact = 4,
            Owner = "Projektledare", Strategy = "Minska",
            Preventive = "Genomför beroende-workshop, skapa beroendekarta med ägare och datum.",
            Fallback = "Replanera milstolpar, fasa leverans, skapa temporära workarounds.",
            Trigger = "Oklara eller saknade datum för beroenden, återkommande blockerare."
        },
        new()
        {
            Id = 9, Title = "Informationssäkerhet & åtkomst", Selected = false,
            Description = "Risk att GDPR-krav och behörighets-/åtkomstfrågor inte hanteras tidigt.",
            Cause = "Otydliga dataklassningar, sen involvering av IT-säkerhet/DPO.",
            Consequence = "Blockerad utveckling/test, försenad go-live, efterlevnadsrisk.",
            Category = "Säkerhet", Probability = 1, Impact = 4,
            Owner = "Teknisk lead", Strategy = "Minska",
            Preventive = "Identifiera datatyper och dataklassning tidigt, säkra nödvändiga avtal.",
            Fallback = "Pausa dataflöden tills krav uppfyllts, anonymisera testdata.",
            Trigger = "Fördröjd åtkomst >5 arbetsdagar, oklar dataägare."
        },
        new()
        {
            Id = 10, Title = "Test/UAT & Go-live readiness", Selected = true,
            Description = "Risk att testning, acceptans, utbildning och driftsättning inte är tillräckligt planerat.",
            Cause = "För lite tid avsatt för test/UAT, otydliga acceptanskriterier.",
            Consequence = "Försenad go-live, fler buggar i produktion, incidenter.",
            Category = "Kvalitet", Probability = 3, Impact = 5,
            Owner = "Testansvarig", Strategy = "Minska",
            Preventive = "Definiera acceptanskriterier och teststrategi tidigt, planera UAT-fönster.",
            Fallback = "Skjuta go-live eller fasa releasen, begränsa scope till kritiska flöden.",
            Trigger = "UAT startar sent, många blockerade testfall, ökande antal kritiska buggar."
        }
    };

    public static DevOpsStructure GetDefaultDevOpsStructure() => new()
    {
        Epics = new()
        {
            new()
            {
                Title = "Utveckling",
                Features = new()
                {
                    new()
                    {
                        Title = "Uppstart", Effort = 24,
                        Requirements = new()
                        {
                            new() { Title = "Grunduppsättning", Estimate = 8 }
                        }
                    },
                    new()
                    {
                        Title = "Integrationsflöden", Effort = 16,
                        Requirements = new()
                    }
                }
            },
            new()
            {
                Title = "Test, verifiering och produktionssättning",
                Features = new()
                {
                    new()
                    {
                        Title = "Driftsättning", Effort = 16,
                        Requirements = new()
                        {
                            new() { Title = "Driftsättning produktion", Estimate = 8 },
                            new() { Title = "Driftsättning test", Estimate = 8 }
                        }
                    },
                    new()
                    {
                        Title = "System- och flödestester, rättningar därefter", Effort = 16,
                        Requirements = new()
                    }
                }
            },
            new()
            {
                Title = "Projektadmin",
                Features = new()
                {
                    new()
                    {
                        Title = "Projektadmin", Effort = 40,
                        Requirements = new()
                        {
                            new() { Title = "Projektinitiering", Estimate = 8 },
                            new() { Title = "Övergripande flödes- och lösningsarbete", Estimate = 8 },
                            new() { Title = "Dokumentation", Estimate = 4 },
                            new() { Title = "Projektledning", Estimate = 12 },
                            new() { Title = "Veckomöten", Estimate = 8 }
                        }
                    }
                }
            },
            new()
            {
                Title = "Framtida utveckling",
                Features = new()
                {
                    new()
                    {
                        Title = "Out of scope", Effort = 0,
                        Requirements = new()
                    }
                }
            }
        }
    };

    public static List<ChecklistItem> GetDefaultChecklist() => new()
    {
        new("Överlämning från sälj", "- Gå igenom avtalet/avtalen.\n- Sammanställ vad som är kommunicerat med kunden.\n- Kartlägg säljarens kunddialog.\n- Klargör hur fakturering ska ske.\n- Säkerställ att det finns avtal på plats gällande hosting.", "1. Uppstart"),
        new("Konfiguration Azure Devops", "- Finns kund i Azure Devops innan? Har den korrekt process, CMMI@exsitec?\n- Guide för att konfigurera Azure Devops", "1. Uppstart"),
        new("Inventering av kund", "- Kartlägg vad vi har gjort tidigare hos kunden.\n- Undersök historik och underlag: tidrapporter i Qlik, HubSpot och Azure DevOps.", "1. Uppstart"),
        new("Skapa projektmapp i drive", "- Hitta kund, om den inte finns skapa upp en ny mapp, med mapparna AvtalProjekt och Systemdokumentation.", "1. Uppstart"),
        new("Scope och estimat", "- Vilket scope och vilka estimat finns definierade för projektet?", "1. Uppstart"),
        new("Projektplan och planering", "- Hur ser projektplanen ut, och vilket verktyg ska användas?\n- Skapa projektverktyget enligt mallen.", "1. Uppstart"),
        new("Fakturering och tidrapportering", "- Säkerställ att ordern är upplagd i Visma.\n- Klargör hur kunden vill få sina fakturor.", "1. Uppstart"),
        new("Ta fram och utvärdera projektgruppen", "- Hur ser kompetensfördelningen ut?\n- Är projektet bemannat med rätt kompetens?\n- Behöver en systemexpert tas in?", "1. Uppstart"),
        new("Intern projektstart", "", "1. Uppstart"),
        new("Extern projektstart", "Följande bör gås igenom:\n- Projektbeskrivning och lösningsförslag\n- Scope, avgränsningar och tidsplan\n- Arbetsprocess och projektverktyg\n- Projektgrupp och eventuell styrgrupp\n- Förväntningar\n- Riskanalys\n- Kommunikationsvägar\n- ÄTA-hantering", "1. Uppstart"),
        new("Frekventa projektavstämningar", "- Genomför både interna och externa avstämningar.\n- Boka återkommande tider.\n\nExempelagenda:\n1. Takt och budget\n2. Projektplan\n3. Status aktivitetslista\n4. Aktiviteter nästkommande vecka\n5. Eventuella ÄTA\n6. Riskanalys", "1. Uppstart"),
        new("Identifiera mottagare av lösningen", "- Vilka ska testa lösningen?\n- Vem äger processen?\n- Vilka är slutanvändarna?\n- Vem godkänner leveransen?\n- Behövs utbildning?\n- Vem ansvarar för förvaltning och support?", "1. Uppstart"),
        new("Etablera process för ÄTA-hantering", "- Vilka är beslutsfattare?\n- Är ni och kunden överens om scope?\n- Hur hanterar vi sådant som uppstår utanför scope?", "1. Uppstart"),
        new("Uppföljning av tid", "- Hur tänker ni följa upp tid?\n- Kan vi stödja det utifrån hur vi tidrapporterar idag?\n- Behöver vi justera något i faktureringen?", "1. Uppstart"),
        new("Projektcoachning", "- Genomför projektcoachning", "1. Uppstart"),
        new("Riskanalys", "- Identifiera, bedöm och hantera projektets risker löpande.\n- Exempel: kompetens, tidsplan, budget, scope, beroenden, integration, kundsamarbete.", "1. Uppstart"),
        new("Uppbokning av eventuell workshop", "- Boka tid för workshop.\n- Klargör syfte och förväntat utfall.\n- Definiera deltagare och roller.\n- Förbered agenda.\n- Säkerställ uppföljning.", "1. Uppstart"),
        new("Uppbokning av styrgruppsmöten", "- Boka in styrgruppsmöten vid behov.\n- Klargör syfte och mandat.\n- Säkerställ rätt deltagare.\n- Förbered standardagenda.\n- Bestäm hur beslut dokumenteras.", "1. Uppstart"),
        new("Definition av effektmål", "- Vad ska projektet uppnå i kundens organisation?\n- Finns det mätbara mål?\n- Vilka KPI:er är relevanta?", "1. Uppstart"),
        new("Arkitekturcoachning", "- Genomför arkitekturcoachning vid behov", "1. Uppstart"),
        new("Kommunikation", "- Är projektmöten inbokade?\n- Har ni kommit överens om kommunikationskanaler?\n- Är alla berörda involverade?\n- Hur dokumenteras beslut?", "2. Början"),
        new("Lösningsarbete och processkartläggning", "- Har vi en tydlig bild av lösningen?\n- Finns processbeskrivningar?\n- Är workshops genomförda?", "2. Början"),
        new("Scope, estimat, tidsplan", "- Hur ligger vi till i förhållande till estimat?\n- Finns ett tydligt scope godkänt av kund?\n- Håller vi tillräckligt hög takt?", "2. Början"),
        new("Riskhantering", "- Är det några uppenbara risker identifierade?\n- Har de kommunicerats till kund?", "2. Början"),
        new("Hosting och installation", "- Hur ska lösningen hostas?\n- Finns beroenden till tredje part?\n- Finns avtal för Azure?", "2. Början"),
        new("Testning och testmiljö", "- Hur ska lösningen testas?\n- Har vi tillgång till testmiljö?\n- Har kunden avsatt tid för testning?", "2. Början"),
        new("Dokumentation", "- Är dokumentationen påbörjad?\n- Finns ett samlat ställe för dokumentation?", "3. Mitten"),
        new("Reflektion kring arbetssätt", "- Vad har fungerat bra/mindre bra?\n- Vilka utmaningar finns?\n- Behöver något förändras?", "3. Mitten"),
        new("Riskhantering (mitten)", "- Är det några nya risker identifierade?\n- Har de kommunicerats till kund?", "3. Mitten"),
        new("Scope, estimat, tidsplan (mitten)", "- Hur ligger vi till?\n- Finns etablerat arbetssätt att hantera ÄTA?", "3. Mitten"),
        new("Felhantering", "- Är fel synliga för kunden?\n- Hur hanteras fel i koden?\n- Behövs underhållsavtal?", "4. Slutet"),
        new("Driftsättningsplan", "- Har ni pratat om driftsättning och risker?\n- Finns datum spikat?\n- Möjlighet till mjuk driftsättning?", "4. Slutet"),
        new("Projektutvärdering", "- Har en projektutvärdering skickats ut?\n- Vad har gått bra/mindre bra?\n- Vilka lärdomar?", "4. Slutet"),
        new("Projektavslut", "- Finns projektavslut inbokat med kund?\n- Är det tydligt kommunicerat?\n- Finns något för steg 2?", "4. Slutet"),
        new("Acceptanstest och godkännande", "- Har ni etablerat hur kunden ska acceptanstesta lösningen?", "4. Slutet"),
        new("Förvaltning och underhåll", "- Vilka kommer förvalta lösningen?\n- Vilken intervall på förvaltningsmöten?\n- Har vi synkat med andra LO:n?", "4. Slutet"),
    };
}

public record ChecklistItem(string Title, string Description, string Phase, string Status = "Ej klar");
