using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace MWI
{
    // Fix provided by Digi <3
    // https://github.com/THDigi/SE-ModScript-Examples/blob/master/Data/Scripts/Examples/MissileTurretProjectileSoundFix.cs
    // Will cause "Possible entity type script logic collision" warnings
    // safe to ignore, just due to double casting
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_LargeMissileTurret), false, "", "SmallMissileTurret", "HeavyDefenseTurret", "BattleshipCannon", "BattleshipCannonMK2", "BattleshipCannonMK22", "BattleshipCannonMK3", "BFG_M", "TelionAF", "TelionAF_small", "BFTriCannon", "TelionAFGen2")]
    public class SoundManager : MyGameLogicComponent
    {
        private IMyFunctionalBlock block;
        private IMyGunObject<MyGunBase> gun;
        private MyEntity3DSoundEmitter soundEmitter;
        private MySoundPair soundPair;
        private long lastShotTime;
        private readonly bool allMisTurrets = true;
        private readonly bool debug = false;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            try
            {
                if (MyAPIGateway.Session.IsServer && MyAPIGateway.Utilities.IsDedicated)
                    return; // DS doesn't need to play sounds

                block = (IMyFunctionalBlock)Entity;

                if (block?.CubeGrid?.Physics == null)
                    return; // ignore projected grids

                gun = (IMyGunObject<MyGunBase>)Entity;

                if (!gun.GunBase.HasProjectileAmmoDefined && !allMisTurrets)
                    return; // ignore missile turrets that don't have projectile ammo

                var def = (MyWeaponBlockDefinition)block.SlimBlock.BlockDefinition;
                var weaponDef = MyDefinitionManager.Static.GetWeaponDefinition(def.WeaponDefinitionId);
                if (gun.GunBase.IsAmmoProjectile)
                    soundPair = weaponDef.WeaponAmmoDatas[0].ShootSound;
                else
                    soundPair = weaponDef.WeaponAmmoDatas[1].ShootSound;

                lastShotTime = gun.GunBase.LastShootTime.Ticks;

                soundEmitter = new MyEntity3DSoundEmitter((MyEntity)Entity); // create a sound emitter following this block entity

                block.IsWorkingChanged += BlockWorkingChanged;
                BlockWorkingChanged(block);
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        public override void Close()
        {
            soundEmitter?.StopSound(true, true);
            soundEmitter = null;
        }

        private void BlockWorkingChanged(IMyCubeBlock block)
        {
            if (block.IsWorking)
                NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            else
                NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void UpdateBeforeSimulation()
        {
            try
            {
                var shotTime = gun.GunBase.LastShootTime.Ticks;

                if (shotTime > lastShotTime)
                {
                    lastShotTime = shotTime;

                    var muzzleLocalMatrix = gun.GunBase.GetMuzzleLocalMatrix();
                    var localWorldMatrix = muzzleLocalMatrix * gun.GunBase.WorldMatrix;
                    soundEmitter.SetPosition(localWorldMatrix.Translation);
                    soundEmitter.PlaySound(soundPair);

                    if (!debug)
                        return;

                    if (gun.GunBase.IsAmmoProjectile)
                        MyAPIGateway.Utilities.ShowNotification("[DEBUG] shot projectile", 500);
                    else
                        MyAPIGateway.Utilities.ShowNotification("[DEBUG] shot missile", 500);
                }
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        private void LogError(Exception e)
        {
            MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");

            if (MyAPIGateway.Session?.Player != null)
                MyAPIGateway.Utilities.ShowNotification($"[ ERROR: {GetType().FullName}: {e.Message} | Send SpaceEngineers.Log to mod author ]", 10000, MyFontEnum.Red);
        }
    }
}
