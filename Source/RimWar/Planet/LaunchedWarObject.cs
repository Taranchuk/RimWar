﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;

namespace RimWar.Planet
{
    [StaticConstructorOnStartup]
    public class LaunchedWarObject : WorldObject
    {

        private int uniqueId = -1;
        private string nameInt;
        private int warPointsInt = -1;

        public int destinationTile = -1;
        private bool arrived;
        private int initialTile = -1;
        private float traveledPct;
        private const float TravelSpeed = 0.00025f;

        public Faction FactionOwner => base.Faction;
        public bool IsPlayerControlled => base.Faction == Faction.OfPlayer;

        public override bool AppendFactionToInspectString => true;

        public RimWarData rimwarData => WorldUtility.GetRimWarDataForFaction(this.Faction);

        private Vector3 Start => Find.WorldGrid.GetTileCenter(initialTile);
        private Vector3 End => Find.WorldGrid.GetTileCenter(destinationTile);

        private Settlement parentSettlement = null;
        private WorldObject targetWorldObject = null;

        public override Vector3 DrawPos => Vector3.Slerp(Start, End, traveledPct);
        private static readonly Color WarObjectDefaultColor = new Color(1f, 1f, 1f);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref uniqueId, "uniqueId", 0);
            Scribe_Values.Look(ref nameInt, "name");
            Scribe_Values.Look<int>(ref this.warPointsInt, "warPointsInt", -1, false);
            Scribe_Values.Look(ref destinationTile, "destinationTile", 0);
            Scribe_Values.Look(ref arrived, "arrived", defaultValue: false);
            Scribe_Values.Look(ref initialTile, "initialTile", 0);
            Scribe_Values.Look(ref traveledPct, "traveledPct", 0f);
            Scribe_References.Look<Settlement>(ref this.parentSettlement, "parentSettlement");
            Scribe_References.Look<WorldObject>(ref this.targetWorldObject, "targetWorldObject");
        }

        private float TraveledPctStepPerTick
        {
            get
            {
                Vector3 start = Start;
                Vector3 end = End;
                if (start == end)
                {
                    return 1f;
                }
                float num = GenMath.SphericalDistance(start.normalized, end.normalized);
                if (num == 0f)
                {
                    return 1f;
                }
                return 0.00025f / num;
            }
        }

        public override void PostAdd()
        {
            base.PostAdd();
            initialTile = base.Tile;
        }        

        public Settlement ParentSettlement
        {
            get
            {
                WorldObject wo = Find.World.worldObjects.WorldObjectAt(this.parentSettlement.Tile, WorldObjectDefOf.Settlement);
                if (wo != null && wo.Faction == this.Faction)
                {
                    return this.parentSettlement;
                }
                else
                {
                    this.parentSettlement = null;
                    return this.parentSettlement;
                }

            }
            set
            {
                this.parentSettlement = value;
            }
        }

        public WorldObject DestinationTarget
        {
            get
            {
                return this.targetWorldObject;
            }
            set
            {
                this.targetWorldObject = value;
            }
        }

        public bool DestinationReached
        {
            get
            {
                return traveledPct >= 1f;
            }
        }

        public virtual int RimWarPoints
        {
            get
            {               
                return this.warPointsInt;
            }
            set
            {
                this.warPointsInt = value;
            }
        }    

        public string Name
        {
            get
            {
                return nameInt;
            }
            set
            {
                nameInt = value;
            }
        }

        public override bool HasName => !nameInt.NullOrEmpty();

        public override string Label
        {
            get
            {
                if(!HasName)
                {
                    return "";
                }
                if (nameInt != null)
                {
                    return nameInt;
                }
                return base.Label;
            }
        }       

        public LaunchedWarObject()
        {

        }

        public void SetUniqueId(int newId)
        {
            if (uniqueId != -1 || newId < 0)
            {
                Log.Error("Tried to set warobject with uniqueId " + uniqueId + " to have uniqueId " + newId);
            }
            uniqueId = newId;
        }

        public override void Tick()
        {
            base.Tick();
            traveledPct += TraveledPctStepPerTick;
            if (traveledPct >= 1f)
            {
                traveledPct = 1f;
                ArrivalAction();
            }
        }        

        public override void SpawnSetup()
        {
            base.SpawnSetup();
        }

        public virtual void ImmediateAction(WorldObject wo)
        {
            Find.WorldObjects.Remove(this);
        }

        public virtual void ArrivalAction()
        {
            Find.WorldObjects.Remove(this);
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }
            return stringBuilder.ToString();
        }
    }
}
