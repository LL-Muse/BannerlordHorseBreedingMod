using System;
using System.Linq;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using MCM.Abstractions.Settings.Base.Global;

namespace HorseBreeding
{
    
    internal class HorseBreedingBehavior : CampaignBehaviorBase
    {
        private CampaignTime _startTime;
        private float _Duration;
        private HorseBreedingSettings instance = GlobalSettings<HorseBreedingSettings>.Instance;
        private ItemObject[] tiers = new ItemObject[6];

        Random rng;

        public override void RegisterEvents()
        {
            rng = new Random();

            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(MenuItems));
        }

        private void MenuItems(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("village", "breed_horses", "Breed Horse", (GameMenuOption.OnConditionDelegate)(args =>
            {
                if (!Settlement.CurrentSettlement.Village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>("aserai_horse")) && !Settlement.CurrentSettlement.Village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>("battania_horse")) && !Settlement.CurrentSettlement.Village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>("empire_horse")) && !Settlement.CurrentSettlement.Village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>("khuzait_horse")) && !Settlement.CurrentSettlement.Village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>("sturgia_horse")) && !Settlement.CurrentSettlement.Village.IsProducing(MBObjectManager.Instance.GetObject<ItemObject>("vlandia_horse")))
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Continue;
                    args.Tooltip = new TextObject("The land around this village is not suited to horse breeding (Hint: go to a village that produces horse to use this action)");
                args.IsEnabled = true;
                    return false;
                }
                 
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args => ShowHorseBreedList()), index: 1);

            campaignGameStarter.AddWaitGameMenu("breeding_wait", "You are breeding horses", (OnInitDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(_Duration, 0.0f);
                args.MenuContext.GameMenu.AllowWaitingAutomatically();
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConditionDelegate)(args =>
            {
                args.MenuContext.GameMenu.AllowWaitingAutomatically();
                args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                return true;
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConsequenceDelegate)(args => ProduceEnd()), (OnTickDelegate)((args, dt) => args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_startTime.ElapsedHoursUntilNow / _Duration)), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);
            campaignGameStarter.AddWaitGameMenu("breeding_wait2", "You are breeding horses", (OnInitDelegate)(args =>
            {
                args.MenuContext.GameMenu.SetTargetedWaitingTimeAndInitialProgress(_Duration, 0.0f);
                args.MenuContext.GameMenu.AllowWaitingAutomatically();
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConditionDelegate)(args =>
            {
                args.MenuContext.GameMenu.AllowWaitingAutomatically();
                args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                return true;
            }), (TaleWorlds.CampaignSystem.GameMenus.OnConsequenceDelegate)(args => Produce2End()), (OnTickDelegate)((args, dt) => args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_startTime.ElapsedHoursUntilNow / _Duration)), GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption);

            campaignGameStarter.AddGameMenuOption("breeding_wait", "breeding_wait_leave", "Stop and Leave", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                GameMenu.SwitchToMenu("village");
            }));

            campaignGameStarter.AddGameMenuOption("breeding_wait2", "breeding_wait2_leave", "Stop and Leave", (GameMenuOption.OnConditionDelegate)(args =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }), (GameMenuOption.OnConsequenceDelegate)(args =>
            {
                GameMenu.SwitchToMenu("village");
            }));
        }

        private void ShowHorseBreedList()
        {
            List<InquiryElement> inquiryElements = new List<InquiryElement>();
            inquiryElements.Add(new InquiryElement((object)MBObjectManager.Instance.GetObject<ItemObject>("aserai_horse"), "Aserai Horses", new ImageIdentifier(MBObjectManager.Instance.GetObject<ItemObject>("aserai_horse"))));
            inquiryElements.Add(new InquiryElement((object)MBObjectManager.Instance.GetObject<ItemObject>("battania_horse"), "Battanian Horses", new ImageIdentifier(MBObjectManager.Instance.GetObject<ItemObject>("battania_horse"))));
            inquiryElements.Add(new InquiryElement((object)MBObjectManager.Instance.GetObject<ItemObject>("empire_horse"), "Imperial Horses", new ImageIdentifier(MBObjectManager.Instance.GetObject<ItemObject>("empire_horse"))));
            inquiryElements.Add(new InquiryElement((object)MBObjectManager.Instance.GetObject<ItemObject>("khuzait_horse"), "Khuzait Horses", new ImageIdentifier(MBObjectManager.Instance.GetObject<ItemObject>("khuzait_horse"))));
            inquiryElements.Add(new InquiryElement((object)MBObjectManager.Instance.GetObject<ItemObject>("sturgia_horse"), "Sturgian Horses", new ImageIdentifier(MBObjectManager.Instance.GetObject<ItemObject>("sturgia_horse"))));
            inquiryElements.Add(new InquiryElement((object)MBObjectManager.Instance.GetObject<ItemObject>("vlandia_horse"), "Vlandian Horses", new ImageIdentifier(MBObjectManager.Instance.GetObject<ItemObject>("vlandia_horse"))));


            InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData("Possible Horse Breeds", "Select the type of horse you want to breed", inquiryElements, true, 1, "Continue", (string)null, (Action<List<InquiryElement>>)(args =>
             {
                 List<InquiryElement> source = args;
                 if (source != null && !source.Any<InquiryElement>())
                 {
                     return;
                 }
                 InformationManager.HideInquiry();
                 SubModule.ExecuteActionOnNextTick((Action)(() => HorseSelected(args.Select<InquiryElement, ItemObject>((Func<InquiryElement, ItemObject>)(element => element.Identifier as ItemObject)))));
             }), (Action<List<InquiryElement>>)null));
        }

        private void HorseSelected(IEnumerable<ItemObject> horses)
        {
            ItemObject breed = horses.First<ItemObject>();
            if(breed == MBObjectManager.Instance.GetObject<ItemObject>("empire_horse"))
            {
                tiers[0] = MBObjectManager.Instance.GetObject<ItemObject>("sumpter_horse");
                tiers[1] = MBObjectManager.Instance.GetObject<ItemObject>("empire_horse");
                tiers[2] = MBObjectManager.Instance.GetObject<ItemObject>("t2_empire_horse");
                tiers[3] = MBObjectManager.Instance.GetObject<ItemObject>("t3_empire_horse");
                tiers[4] = MBObjectManager.Instance.GetObject<ItemObject>("noble_horse_imperial");
                tiers[5] = MBObjectManager.Instance.GetObject<ItemObject>("t5_empire_horse");    
            }
            else if(breed == MBObjectManager.Instance.GetObject<ItemObject>("aserai_horse"))
            {
                tiers[0] = MBObjectManager.Instance.GetObject<ItemObject>("sumpter_horse");
                tiers[1] = MBObjectManager.Instance.GetObject<ItemObject>("noble_horse_southern");
                tiers[2] = MBObjectManager.Instance.GetObject<ItemObject>("t2_aserai_horse");
                tiers[3] = MBObjectManager.Instance.GetObject<ItemObject>("t3_aserai_horse");
                tiers[4] = MBObjectManager.Instance.GetObject<ItemObject>("noble_horse_imperial");
                tiers[5] = MBObjectManager.Instance.GetObject<ItemObject>("t5_aserai_horse");
            }
            else if (breed == MBObjectManager.Instance.GetObject<ItemObject>("battania_horse"))
            {
                tiers[0] = MBObjectManager.Instance.GetObject<ItemObject>("sumpter_horse");
                tiers[1] = MBObjectManager.Instance.GetObject<ItemObject>("battania_horse");
                tiers[2] = MBObjectManager.Instance.GetObject<ItemObject>("t2_battania_horse");
                tiers[3] = MBObjectManager.Instance.GetObject<ItemObject>("t3_battania_horse");
                tiers[4] = MBObjectManager.Instance.GetObject<ItemObject>("noble_horse_battania");
                tiers[5] = MBObjectManager.Instance.GetObject<ItemObject>("t5_battania_horse");
            }
            else if (breed == MBObjectManager.Instance.GetObject<ItemObject>("khuzait_horse"))
            {
                tiers[0] = MBObjectManager.Instance.GetObject<ItemObject>("sumpter_horse");
                tiers[1] = MBObjectManager.Instance.GetObject<ItemObject>("khuzait_horse");
                tiers[2] = MBObjectManager.Instance.GetObject<ItemObject>("t2_khuzait_horse");
                tiers[3] = MBObjectManager.Instance.GetObject<ItemObject>("t3_khuzait_horse");
                tiers[4] = MBObjectManager.Instance.GetObject<ItemObject>("noble_horse_eastern");
                tiers[5] = MBObjectManager.Instance.GetObject<ItemObject>("t5_khuzait_horse");
            }
            else if (breed == MBObjectManager.Instance.GetObject<ItemObject>("sturgia_horse"))
            {
                tiers[0] = MBObjectManager.Instance.GetObject<ItemObject>("sumpter_horse");
                tiers[1] = MBObjectManager.Instance.GetObject<ItemObject>("sturgia_horse");
                tiers[2] = MBObjectManager.Instance.GetObject<ItemObject>("t2_sturgia_horse");
                tiers[3] = MBObjectManager.Instance.GetObject<ItemObject>("t3_sturgia_horse");
                tiers[4] = MBObjectManager.Instance.GetObject<ItemObject>("noble_horse_northern");
                tiers[5] = MBObjectManager.Instance.GetObject<ItemObject>("t5_sturgia_horse");
            }
            else if (breed == MBObjectManager.Instance.GetObject<ItemObject>("vlandia_horse"))
            {
                tiers[0] = MBObjectManager.Instance.GetObject<ItemObject>("sumpter_horse");
                tiers[1] = MBObjectManager.Instance.GetObject<ItemObject>("vlandia_horse");
                tiers[2] = MBObjectManager.Instance.GetObject<ItemObject>("t2_vlandia_horse");
                tiers[3] = MBObjectManager.Instance.GetObject<ItemObject>("t3_vlandia_horse");
                tiers[4] = MBObjectManager.Instance.GetObject<ItemObject>("noble_horse_western");
                tiers[5] = MBObjectManager.Instance.GetObject<ItemObject>("t5_vlandia_horse");
            }
            Produce();
        }

        private void Produce()
        {
            if (PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("grain")) < instance.BreedingGrain)
            {
                InformationManager.ShowInquiry(new InquiryData("Insufficient Grain", "You need " + instance.BreedingGrain + "grain to breed horses", true, false, "OK", "", (Action)null, (Action)null), true);
                return;
            }

            int RidingSkill = Hero.MainHero.GetSkillValue(DefaultSkills.Riding);
            _startTime = CampaignTime.Now;
            _Duration = instance.BreedingTime / ((100 + RidingSkill) / 100);
            GameMenu.SwitchToMenu("breeding_wait");
        }

        private void Produce2()
        {
            int RidingSkill = Hero.MainHero.GetSkillValue(DefaultSkills.Riding);
            _startTime = CampaignTime.Now;
            _Duration = instance.BreedingTime / ((100 + RidingSkill) / 100);
            GameMenu.SwitchToMenu("breeding_wait2");
        }

        private void ProduceEnd()
        {
            int max_tier = Math.Min(6, Hero.MainHero.GetSkillValue(DefaultSkills.Riding)/50);
            int current_tier = 1;
            int next_tier_chance = 50;
            if (Hero.MainHero.GetPerkValue(DefaultPerks.Riding.Breeder))
            {
                next_tier_chance += 10;
            }
            while (current_tier < max_tier)
            {
                if(rng.Next(0, 100) <= next_tier_chance)
                {
                    current_tier++;
                }
                else
                {
                    break;
                }
            }

            Hero.MainHero.AddSkillXp(DefaultSkills.Riding, instance.RidingXPPerHorseTier * current_tier);
            
            PartyBase.MainParty.ItemRoster.AddToCounts(tiers[current_tier-1], 1);
            InformationManager.DisplayMessage(new InformationMessage("You have managed to breed a " + tiers[current_tier - 1].Name.ToString()));
            PartyBase.MainParty.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("grain"), -1 * instance.BreedingGrain);

            if(PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("grain")) < instance.BreedingGrain)
            {
                InformationManager.ShowInquiry(new InquiryData("Insufficient Grain", "You do not have enough grain to breed horses", true, false, "OK", "", (Action)null, (Action)null), true);
                return;
            }
            else
            {
                Produce2();
            }
        }

        private void Produce2End()
        {
            int max_tier = Math.Min(6, Hero.MainHero.GetSkillValue(DefaultSkills.Riding) / 50);
            int current_tier = 1;
            int next_tier_chance = 50;
            if (Hero.MainHero.GetPerkValue(DefaultPerks.Riding.Breeder))
            {
                next_tier_chance += 10;
            }
            while (current_tier < max_tier)
            {
                if (rng.Next(0, 100) <= next_tier_chance)
                {
                    current_tier++;
                }
                else
                {
                    break;
                }
            }

            Hero.MainHero.AddSkillXp(DefaultSkills.Riding, instance.RidingXPPerHorseTier * current_tier);

            PartyBase.MainParty.ItemRoster.AddToCounts(tiers[current_tier - 1], 1);
            InformationManager.DisplayMessage(new InformationMessage("You have managed to breed a " + tiers[current_tier - 1].Name.ToString()));
            PartyBase.MainParty.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("grain"), -1 * instance.BreedingGrain);

            if (PartyBase.MainParty.ItemRoster.GetItemNumber(MBObjectManager.Instance.GetObject<ItemObject>("grain")) < instance.BreedingGrain)
            {
                InformationManager.ShowInquiry(new InquiryData("Insufficient Grain", "You do not have enough grain to breed horses", true, false, "OK", "", (Action)null, (Action)null), true);
                return;
            }
            else
            {
                Produce();
            }
        }

        private void TakeMenuAction()
        {
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}