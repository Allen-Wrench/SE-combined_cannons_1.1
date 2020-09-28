using System;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Engine.Physics;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace MWI
{
    // Missile particle manager v0.1.1 - by PhoenixTheSage
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Missile), false)]
    public class MissileManager : MyGameLogicComponent
    {
        private static readonly string MyMissile = "Sandbox.Game.Weapons.MyMissile";
        private static string missileType;
        private static string typeCache;
        private IMyEntity topEntity;
        private MyObjectBuilder_Missile missile;
        private MatrixD missileWorldMatrix;
        private Vector3D missilePosition;
        private MyParticleEffect missileTrail;
        private readonly float missileTrailOffsetMultiplier = -2.8f; // Trail position fix, Keen default: 0.4
        internal static MyPhysicsComponentBase PhysCache { get; set; }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
        }

        #region Actions On Missile Spawn
        public override void OnAddedToScene()
        {
            try
            {
                // spawn missile trail emmitter on client only
                if (MyAPIGateway.Session.IsServer && MyAPIGateway.Utilities.IsDedicated)
                    return;

                topEntity = Entity;

                if (topEntity == null)
                    return;

                if (topEntity?.GetType().ToString() == MyMissile)
                {
                    // if client on a server, temporarily add physics before getting objectbuilder of missile
                    if (!MyAPIGateway.Session.IsServer)
                    {
                        MyAPIGateway.Utilities.InvokeOnGameThread(GetOB);
                    }
                    else
                    {
                        missile = topEntity.GetObjectBuilder() as MyObjectBuilder_Missile;
                    }



                    //issue with this cache and separate missile type situations

                    //if (missile != null && typeCache == null)
                    //{
                    //    missileType = missile.AmmoMagazineId.SubtypeId;
                    //    typeCache = missileType;
                    //}
                    //else
                    //{
                    //    missileType = typeCache;
                    //}

                    //temporary
                    if (missile != null)
                    {
                        missileType = missile.AmmoMagazineId.SubtypeId;
                    }


                    //add identical smoke particle back to vanilla 200mm missiles (original sbc library nulled)
                    if (missileType == "Missile200mm")
                    {
                        typeCache = null;

                        // make sure to get initial entity location, then spawn emitter
                        UpdateMissileLocation();
                        MyParticlesManager.TryCreateParticleEffect("Rocket_Fume", ref missileWorldMatrix, ref missilePosition, topEntity.Render.ParentIDs[0], out missileTrail);
                    }
                }
            }
            catch (Exception e)
            {
                //MyAPIGateway.Utilities.ShowNotification("[ Error in " + GetType().FullName + ": " + e.Message + " ]", 10000, MyFontEnum.Red);
                MyLog.Default.WriteLine(e);
            }
        }
        #endregion

        public override void UpdateBeforeSimulation()
        {
            try
            {
                // update missile trail emitter position on client only
                if (MyAPIGateway.Session.IsServer && MyAPIGateway.Utilities.IsDedicated)
                    return;

                if (topEntity == null)
                    return;

                if (missile != null)
                {
                    UpdateMissileLocation();

                    if (missileTrail != null)
                    {
                        missileTrail.WorldMatrix = missileWorldMatrix;
                        missileTrail.SetTranslation(missilePosition);
                    }
                }
            }
            catch (Exception e)
            {
                //MyAPIGateway.Utilities.ShowNotification("[ Error in " + GetType().FullName + ": " + e.Message + " ]", 10000, MyFontEnum.Red);
                MyLog.Default.WriteLine(e);
            }
        }

        #region Actions On Missile Despawn
        public override void OnRemovedFromScene()
        {
            try
            {
                // make sure emitter is removed with parent entity
                if (missileTrail != null)
                {
                    missileTrail.Stop(false);
                    missileTrail = null;
                }
            }
            catch (Exception e)
            {
                //MyAPIGateway.Utilities.ShowNotification("[ Error in " + GetType().FullName + ": " + e.Message + " ]", 10000, MyFontEnum.Red);
                MyLog.Default.WriteLine(e);
            }
        }
        #endregion

        private void UpdateMissileLocation()
        {
            try
            {
                missileWorldMatrix = topEntity.WorldMatrix;
                missileWorldMatrix.Translation -= missileWorldMatrix.Forward * missileTrailOffsetMultiplier;
                missilePosition = missileWorldMatrix.Translation;
            }
            catch (Exception e)
            {
                //MyAPIGateway.Utilities.ShowNotification("[ Error in " + GetType().FullName + ": " + e.Message + " ]", 10000, MyFontEnum.Red);
                MyLog.Default.WriteLine(e);
            }
        }

        private void GetOB()
        {
            try
            {
                if (!MyAPIGateway.Session.IsServer)
                {
                    if (PhysCache == null)
                    {
                        topEntity.InitBoxPhysics(MyStringHash.NullOrEmpty, Vector3.Zero, Vector3.One, 0, 0, 0, 0, RigidBodyFlag.RBF_STATIC);
                        PhysCache = topEntity.Physics;

                        missile = topEntity.GetObjectBuilder() as MyObjectBuilder_Missile;
                        topEntity.Physics = null;
                    }
                    else
                    {
                        topEntity.Physics = PhysCache;

                        missile = topEntity.GetObjectBuilder() as MyObjectBuilder_Missile;
                        topEntity.Physics = null;
                    }
                }
            }
            catch (Exception e)
            {
                //MyAPIGateway.Utilities.ShowNotification("[ Error in " + GetType().FullName + ": " + e.Message + " ]", 10000, MyFontEnum.Red);
                MyLog.Default.WriteLine(e);
            }
        }
    }
}
