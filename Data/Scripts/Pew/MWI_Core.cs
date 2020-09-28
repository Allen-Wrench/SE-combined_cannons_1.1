using Sandbox.Game.Entities;
using Sandbox.Game.Gui;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;



//Muzzle Flash and Recoil Animations by Digi. Updated by PhoenixTheSage.
namespace MWI
{
    public class MWI_Core : MyGameLogicComponent
    {
        #region Settings [Override Through Init Casting]
            //default fx settings
            protected bool useMuzzleLogic       = true;  // set false for weapons with gatling ammo types
            protected bool useCustomMuzzle      = true;  // set false if there's no recoil dummy for a little extra optimization

            protected bool isAnimated           = false; // set true if weapon is rigged for recoil

            protected float BarrelTravelDist    = 2.5f;
            protected float BarrelPunchSpeed    = 1.25f;
            protected float BarrelRestoreSpeed  = 0.1f;

            protected string ParticleType       = "Explosion_Missile";
            protected float ParticleScale       = 0.475f;

            // default AI filters
            protected string TargetMeteors      = "TargetMeteors_On";
            protected string TargetMissiles     = "TargetMissiles_Off";
            protected string TargetSmallShips   = "TargetSmallShips_On";
            protected string TargetLargeShips   = "TargetLargeShips_On";
            protected string TargetCharacters   = "TargetCharacters_Off";
            protected string TargetStations     = "TargetStations_On";
            protected string TargetNeutrals     = "TargetNeutrals_On";

            //protected MySoundPair SoundPair = null; // new MySoundPair("SoundName without Arc or Real prefix");

            public virtual void Setup() { }
        #endregion Settings

        #region Do Not Change
            public bool first = true;
            public bool justPlaced;
            
            private long lastShotTime;
            private readonly List<AnimatedBarrel> animatingBarrels = new List<AnimatedBarrel>();
            private MatrixD muzzleLocalMatrix;
            private MatrixD muzzleWorldMatrix;
            private Vector3D muzzleWorldPosition;
            private uint closestSubpartID = 4294967295u;
            private IMyCubeBlock block;
            private IMyGunObject<MyGunBase> gun;
  
        #endregion

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void UpdateBeforeSimulation()
        {
            try
            {

                if (MyAPIGateway.Session.IsServer && MyAPIGateway.Utilities.IsDedicated)
                    return;

                block = (IMyCubeBlock)Entity;

                if (block?.CubeGrid?.Physics == null)
                    return; // ignore projected grids

                gun = (IMyGunObject<MyGunBase>)Entity;

                if (first)
                {
                    first = false;

                    lastShotTime = gun.GunBase.LastShootTime.Ticks;
                    muzzleLocalMatrix = gun.GunBase.GetMuzzleLocalMatrix();

                    Setup(); // call the overwriteable method for other guns to change this class' variables

                    if (justPlaced)
                    {
                        justPlaced = false;

                        var placed = block as IMyLargeTurretBase;
                        if (placed != null)
                        {
                            //MyAPIGateway.Utilities.ShowNotification("[ Valid turret, attempting to set filters ]", 4000, MyFontEnum.Blue);
                            placed.ApplyAction(TargetMeteors);
                            placed.ApplyAction(TargetMissiles);
                            placed.ApplyAction(TargetSmallShips);
                            placed.ApplyAction(TargetLargeShips);
                            placed.ApplyAction(TargetCharacters);
                            placed.ApplyAction(TargetStations);
                            placed.ApplyAction(TargetNeutrals);
                        }
                    }
                }

                if (!block.IsFunctional) // block broken, pause everything (even ongoing animations)
                    return;

                if (!useMuzzleLogic)
                    return;

                // only animate if custom recoil dummy is setup with appropriate barrel subparts - default is False
                if (isAnimated)
                {
                    ApplyRecoil();
                }


            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        #region Animating barrels individually
        private void ApplyRecoil()
        {
            for (var i = animatingBarrels.Count - 1; i >= 0; i--) // looping backwards to allow removing mid-loop
            {
                var data = animatingBarrels[i];

                if (data.subpart.Closed) // subpart for some reason no longer exists
                {
                    animatingBarrels.RemoveAt(i);
                    continue;
                }

                var m = data.subpart.PositionComp.LocalMatrix;

                if (!data.restoring) // recoiling/moving backwards
                {
                    data.travel += Math.Max(2f - data.travel / 2.5f, 0.001f); // a damping effect near the end
                    m.Translation = data.initialTranslation + m.Down * Math.Min(data.travel, BarrelTravelDist);

                    if (data.travel >= BarrelTravelDist)
                    {
                        data.travel = BarrelTravelDist;
                        data.restoring = true;
                    }
                }
                else // restoring/moving forwards
                {
                    if (data.travel > BarrelTravelDist / 3)
                        data.travel -= Math.Max(0.1f - data.travel / 65, 0.0005f); // first part of retraction, accelerating
                    else
                        data.travel *= 0.96f; // second part, deccelerating

                    if (data.travel <= 0.001f)
                    {
                        m.Translation = data.initialTranslation;
                        animatingBarrels.RemoveAt(i);
                    }
                    else
                    {
                        m.Translation = data.initialTranslation + m.Down * data.travel;
                    }
                }

                data.subpart.PositionComp.SetLocalMatrix(ref m, Entity, true);
            }
        }
        #endregion Animating barrels individually

        private void LogError(Exception e)
        {
            MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");

            //if (MyAPIGateway.Session?.Player != null)
            //    MyAPIGateway.Utilities.ShowNotification($"[ ERROR: {GetType().FullName}: {e.Message} | Send SpaceEngineers.Log to mod author ]", 10000, MyFontEnum.Red);
        }
    }
}