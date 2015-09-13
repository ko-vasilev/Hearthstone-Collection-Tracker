using Hearthstone_Collection_Tracker.ViewModels;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hearthstone_Collection_Tracker.Internal
{
    public class AchievementHandler
    {
        private IList<BasicSetCollectionInfo> _setsInfo;
        private Regex _regex = new Regex(@".*cardId=(?<cardId>\w*).*\[CardFlair: Premium=(?<quality>\w*)\] (?<amount>\d+)");

        private readonly string standard = "STANDARD";
        private readonly string golden = "GOLDEN";

        public AchievementHandler(IList<BasicSetCollectionInfo> setsInfo)
        {
            this._setsInfo = setsInfo;
        }

        public void Handle(IEnumerable<LogLineItem> logLines)
        {
            foreach (LogLineItem logLine in logLines)
            {
                Match match = _regex.Match(logLine.Line);
                if (match.Success)
                {
                    int amount = int.Parse(match.Groups["amount"].Value);
                    if (amount <= 2)
                    {
                        string cardId = match.Groups["cardId"].Value;
                        string quality = match.Groups["quality"].Value;


                        Card card = Database.GetCardFromId(cardId);
                        BasicSetCollectionInfo setInfo = _setsInfo.FirstOrDefault(set => card.Set == set.SetName);
                        if (setInfo != null)
                        {
                            CardInCollection cardInCollection = setInfo.Cards.FirstOrDefault(item => item.Card.Id == card.Id);
                            if (cardInCollection != null)
                            {
                                if (amount <= cardInCollection.MaxAmountInCollection)
                                {
                                    if (quality == standard)
                                    {
                                        if (amount < cardInCollection.AmountNonGolden)
                                        {
                                            int oldAmount = cardInCollection.AmountNonGolden;
                                            cardInCollection.AmountNonGolden = amount;
                                            Logger.WriteLine(string.Format("Hearthstone Collection Tracker: set amount of {0} ({1}) from {2} to {3}", card.Name, quality.ToLower(), oldAmount, amount));
                                        }
                                    }
                                    else if (quality == golden)
                                    {
                                        if (amount < cardInCollection.AmountGolden)
                                        {
                                            int oldAmount = cardInCollection.AmountGolden;
                                            cardInCollection.AmountGolden = amount;
                                            Logger.WriteLine(string.Format("Hearthstone Collection Tracker: set amount of {0} ({1}) from {2} to {3}", card.Name, quality.ToLower(), oldAmount, amount));
                                        }
                                    }
                                    else
                                    {
                                        Logger.WriteLine(string.Format("Hearthstone Collection Tracker: unknown card quality: {0}", quality));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
