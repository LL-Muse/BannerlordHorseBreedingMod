using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using System.Collections.Generic;
using System;

namespace HorseBreeding
{
    public class SubModule : MBSubModuleBase
    {
        private static readonly List<Action> ActionsToExecuteNextTick = new List<Action>();
        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (!(game.GameType is Campaign))
                return;
            ((CampaignGameStarter)gameStarterObject).AddBehavior((CampaignBehaviorBase)new HorseBreedingBehavior());
        }

        public static void ExecuteActionOnNextTick(Action action)
        {
            if (action == null)
            {
                return;
            }
            SubModule.ActionsToExecuteNextTick.Add(action);
        }

        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);
            foreach (Action action in SubModule.ActionsToExecuteNextTick)
                action();
            SubModule.ActionsToExecuteNextTick.Clear();
        }
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
        }
    }
}
