using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace MWI
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class MWI_SessionCore : MySessionComponentBase
    {
        #region Do Not Change
            private bool setup;
            private bool itemAdded;
            private IMyCubeBlock cubeBlock;
            private readonly HashSet<IMyEntity> entities = new HashSet<IMyEntity>();

            //private List<MyDefinitionId> ammoMagazineList = new List<MyDefinitionId>();
            //private List<IMyTerminalControl> weaponControls; // store weapon controls
            //private static Dictionary<long, MyTerminalControlComboBoxItem> m_ammoSelectionItems;
            //public static Dictionary<long, MyTerminalControlComboBoxItem> AmmoSelectionItems
            //{
            //    get
            //    {
            //        if (m_ammoSelectionItems == null)
            //            m_ammoSelectionItems = new Dictionary<long, MyTerminalControlComboBoxItem>();

            //        return m_ammoSelectionItems;
            //    }
            //}
        #endregion

        public override void UpdateBeforeSimulation()
        {

            if (!MyAPIGateway.Multiplayer.IsServer)
                return;

            if (MyAPIGateway.Session == null)
                return;

            if (!setup)
            {
                setup = true;
                SessionSetup();
            }
        }

        private void SessionSetup()
        {
            try
            {
                MyAPIGateway.Entities.GetEntities(entities); // iterate existing entities in the world
                foreach (var entity in entities)
                {
                    IsEntityGrid(entity); // apply check to see if entity is grid and subscribe to events
                }

                MyAPIGateway.Entities.OnEntityAdd += IsEntityGrid; // subscribe new grids to grid check and further subscription

                #region Weapon Terminal Setup
                //var turretAmmoSelect = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyLargeTurretBase>("TurretAmmoSelect");
                //var launcherAmmoSelect = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMySmallMissileLauncher>("LauncherAmmoSelect");
                //var reloadableAmmoSelect = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMySmallMissileLauncherReload>("ReloadableAmmoSelect");

                //turretAmmoSelect.Title = MyStringId.GetOrCompute("Ammo Select");
                //turretAmmoSelect.Tooltip = MyStringId.GetOrCompute("Select the priority ammo for this turret");

                //launcherAmmoSelect.Title = MyStringId.GetOrCompute("Ammo Select");
                //launcherAmmoSelect.Tooltip = MyStringId.GetOrCompute("Select the priority ammo for this launcher");

                //reloadableAmmoSelect.Title = MyStringId.GetOrCompute("Ammo Select");
                //reloadableAmmoSelect.Tooltip = MyStringId.GetOrCompute("Select the priority ammo for this launcher");

                //MyAPIGateway.TerminalControls.AddControl<IMyLargeTurretBase>(turretAmmoSelect);
                //MyAPIGateway.TerminalControls.AddControl<IMySmallMissileLauncher>(launcherAmmoSelect);
                //MyAPIGateway.TerminalControls.AddControl<IMySmallMissileLauncherReload>(reloadableAmmoSelect);

                //MyAPIGateway.TerminalControls.GetControls<IMyUserControllableGun>(out weaponControls);

                //turretAmmoSelect.ComboBoxContent = AmmoComboBox;
                //launcherAmmoSelect.ComboBoxContent = AmmoComboBox;
                //reloadableAmmoSelect.ComboBoxContent = AmmoComboBox;

                #endregion
            }
            catch (Exception e)
            {
                //MyAPIGateway.Utilities.ShowNotification("[ Error in " + GetType().FullName + ": " + e.Message + " ]", 10000, MyFontEnum.Red);
                MyLog.Default.WriteLine(e);
            }
        }

        private void IsEntityGrid(IMyEntity entity)
        {
            try
            {
                var cubeGrid = entity as MyCubeGrid;
                if (cubeGrid != null)
                {
                    var blockCount = cubeGrid.BlocksCount;
                    var grid = (IMyCubeGrid) cubeGrid;
                    var firstBlock = grid.GetCubeBlock(Vector3I.Zero); // get the starting block

                    if (blockCount == 1 && firstBlock != null)
                    {
                        SlimBlockAdded(firstBlock); // makes sure the starting block of a new grid gets event treatment
                    }

                    grid.OnBlockAdded += SlimBlockAdded; // subscribes to block placement event on collected grid
                    grid.OnClose += CubeGridRemoved;
                }
            }
            catch (Exception e)
            {
                //MyAPIGateway.Utilities.ShowNotification("[ Error in " + GetType().FullName + ": " + e.Message + " ]", 10000, MyFontEnum.Red);
                MyLog.Default.WriteLine(e);
            }
        }

        // event handler that checks for newly placed turrets
        private void SlimBlockAdded(IMySlimBlock placedBlock)
        {
            try
            {
                var turret = placedBlock.FatBlock?.GameLogic.GetAs<MWI_Core>();
                if (turret == null) return;

                turret.justPlaced = true;
            }
            catch (Exception e)
            {
                //MyAPIGateway.Utilities.ShowNotification("[ Error in " + GetType().FullName + ": " + e.Message + " ]", 10000, MyFontEnum.Red);
                MyLog.Default.WriteLine(e);
            }
        }

        #region Experimenting with terminal controls for ammo selection
        //private void AmmoComboBox(List<MyTerminalControlComboBoxItem> ammoMagazine)
        //{
        //    try
        //    {
        //        var block = cubeBlock?.GameLogic.GetAs<Core>();

        //        if (block != null)
        //        {
        //            var gun = (IMyGunObject<MyGunBase>) block.Entity;
        //            var gunDefinition = gun.DefinitionId;

        //            var weapon = new MyWeaponDefinition();
        //            var ammoItem = new MyTerminalControlComboBoxItem();
        //            var ammoIndex = weapon.GetAmmoMagazineIdArrayIndex(gunDefinition);

        //            foreach (var mag in weapon.AmmoMagazinesId)
        //            {
        //                var magName = mag.SubtypeName;

        //                for (var i = 0; i < ammoIndex; i++)
        //                {
        //                    ammoMagazine.Add(ammoItem);
        //                }
        //            }

        //            var terminalBlock = cubeBlock as IMyTerminalBlock;
        //            if (terminalBlock != null)
        //            {
        //                if (!AmmoSelectionItems.ContainsKey(terminalBlock.EntityId))
        //                    AmmoSelectionItems.Add(terminalBlock.EntityId, new MagazineList());
        //            }

        //            var defaultAmmoId = gun.GunBase.CurrentAmmoMagazineId;
        //            var defaultAmmoName = gun.GunBase.CurrentAmmoMagazineDefinition.DisplayNameText;
        //            var defaultAmmoSubtype = defaultAmmoId.SubtypeName;

        //            var itemName = MyStringId.GetOrCompute(defaultAmmoName + "[ " + defaultAmmoSubtype + " ]");
        //            ammoItem.Value = itemName;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        MyAPIGateway.Utilities.ShowNotification("[ Error in " + GetType().FullName + ": " + e.Message + " ]", 10000, MyFontEnum.Red);
        //        MyLog.Default.WriteLine(e);
        //    }
        //}
        #endregion

        #region Session and Grid Close
        public void CubeGridRemoved(IMyEntity entity)
        {
            var cubeGrid = entity as MyCubeGrid;
            if (cubeGrid != null)
            {
                var grid = (IMyCubeGrid) cubeGrid;

                grid.OnBlockAdded -= SlimBlockAdded;
                grid.OnClose -= CubeGridRemoved;
            }
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Entities.OnEntityAdd -= IsEntityGrid;

            entities.Clear();
            //weaponControls.Clear();

            MissileManager.PhysCache = null;
        }
        #endregion

    }
}
