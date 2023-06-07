using System;
using System.Linq;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using CalloutInterfaceAPI;
using System.Collections.Generic;
using DialogueSystem;
using CalloutInterface;
using LSPD_First_Response.Engine;

namespace Code_Blue_Calls.Callouts
{

    [CalloutInterface("Domestic Violence", CalloutProbability.Medium, "Code 3", "LSPD")]
    public class DomesticViolence : Callout
    {
        private Ped victim, suspect;
        private Blip blip_suspect, blip_victim;
        private Vector3 location_suspect, location_victim;

        private Vector3 GetClosestHouse(List<Vector3> houses)
        {
            Vector3 playerPosition = Game.LocalPlayer.Character.Position;
            Vector3 closestHouse = Vector3.Zero;
            float closestDistance = float.MaxValue;

            foreach (Vector3 house in houses)
            {
                float distance = Vector3.Distance(playerPosition, house);
                if (distance < closestDistance)
                {
                    closestHouse = house;
                    closestDistance = distance;
                }
            }
            return closestHouse;
        }

        // I don't know what im doing
        private class House
        {
            public Vector3 Position;
            public List<Vector3> Victims { get; set; }
            public List<Vector3> Suspects { get; set; }
            public string Name;
        }

        public override bool OnBeforeCalloutDisplayed()
        {
            // This is stupid and will get replaced with a list containing a list with the suspect and the victim and its gonna be per house
            List<Vector3> location_suspect = new List<Vector3> { 
                new Vector3()
            };

            

            //This is a list of all the houses/interiors
            //This is used for getting the closest house to the player so its not random and he/she doesn't have to go to the other side of the map
            
            List<Vector3> houses = new List<Vector3> 
            {
                new Vector3(1276.979f, -1725.641f, 54.65f), //Lester's place
                new Vector3(249.1075f, -1720.578f, 29.16469f), //Generic house
                new Vector3(-775.9423f, 291.741f, 85.38015f), //West eclips towers
                new Vector3(-131.3567f, -32.02588f, 57.84212f), //Janitors house
                new Vector3(-66.24619f, -574.7697f, 36.95814f), //4 integrity way (players apt.)
                new Vector3(-13.93472f, -1455.081f, 30.4537f), //Franklin
                new Vector3(-616.9177f, 22.83247f, 41.47743f), //Strangeways
                new Vector3(-178.65f, 507.2562f, 136.0046f), //Wild oats
                new Vector3(-553.8226f, 666.5585f, 144.6216f), //Normandy
                new Vector3(1122.733f, 2647.359f, 37.99636f), //Vespuci aka sandy motel
            };

            List<House> houses1 = new List<House>
            {
                new House
                {
                    Name = "Lesterhouse",
                    Victims = new List<Vector3>
                    {
                        new Vector3(1273.748f, -1709.831f, 54.77149f)
                    },
                    Suspects = new List<Vector3>
                    {
                        new Vector3(1274.567f, -1713.383f, 54.77149f)
                    },
                    Position = new Vector3(1276.979f, -1725.641f, 54.65f)
                },
                new House
                {
                    Name = "Generic House",
                    Victims = new List<Vector3>
                    {
                        new Vector3(264.6913f, -997.2188f, -99.00867f)
                    },
                    Suspects = new List<Vector3>
                    {
                        new Vector3(261.1124f, -998.8026f, -99.00865f)
                    },
                    Position = new Vector3(249.1075f, -1720.578f, 29.16469f)
                },
                new House
                {
                    Name = "Frankiln house old",
                    Victims = new List<Vector3>
                    {
                        new Vector3(-11.18673f, -1429.365f, 31.10147f)
                    },
                    Suspects = new List<Vector3>
                    {
                        new Vector3(-10.82392f, -1438.442f, 31.10153f)
                    },
                    Position = new Vector3(-13.93472f, -1455.081f, 30.4537f)
                },
                new House
                {
                    Name = "Med rich apt.",

                    
                }
            };

            // Choose the closest house to the player. This isn't really optimal as if the player isn't patrolling and doing radar or some other thing wich involves being in one place
            // Then the player will get the same house again and again
            Vector3 SpawnPoint = GetClosestHouse(houses);


            

            

            // Not needed as My code isn't goofy and doesn't set the callout position to the other side of the map
            //AddMaximumDistanceCheck(600f, SpawnPoint); //Player must be 600m or closer 


            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
            CalloutMessage = "Domestic Violence";
            CalloutPosition = SpawnPoint;
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_RESISTING_ARREST_02 IN_OR_ON_POSITION", SpawnPoint);


            


            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            new RelationshipGroup("Attacker");
            new RelationshipGroup("Victims");

            suspect = new Ped(location_suspect);
            suspect.IsPersistent = true;
            suspect.BlockPermanentEvents = true;
            suspect.RelationshipGroup = "Attacker";
            suspect.Inventory.GiveNewWeapon("WEAPON_NAVYREVOLVER", 1, true);



            victim = new Ped(location_victim);
            victim.IsPersistent = true;
            victim.BlockPermanentEvents = true;
            victim.RelationshipGroup = "Victims";

            //Game.LocalPlayer.Character.RelationshipGroup = "Victims";

            suspect.Tasks.StandStill(-1);

            Game.SetRelationshipBetweenRelationshipGroups("Attacker", "Victim", Relationship.Hate);

            blip_suspect = suspect.AttachBlip();
            blip_suspect.Color = System.Drawing.Color.Red;
            blip_suspect.IsRouteEnabled = true;

            blip_victim = victim.AttachBlip();
            blip_victim.Color = System.Drawing.Color.Green;
            blip_victim.IsRouteEnabled = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (Game.LocalPlayer.Character.DistanceTo(suspect) < 40f && !ShotAtVictim && !SuspectKillingPlayer && !victim.IsDead)
            {
                suspect.Tasks.FightAgainst(victim);
                ShotAtVictim = true;
            }

            if (Game.LocalPlayer.Character.DistanceTo(suspect) < 15f && !SuspectKillingPlayer)
            {
                //Give the suspect a weapon and make him attack the player
                suspect.Tasks.Clear();
                suspect.Inventory.EquippedWeapon.Ammo = 6;
                SuspectKillingPlayer = true;
                suspect.Tasks.FightAgainst(Game.LocalPlayer.Character);

            }

            if (suspect.IsDead || suspect.IsCuffed)
            {
                End();
            }

            if (suspect.Exists() && victim == null)
            {
                victim = new Ped(location_victim);
                victim.Tasks.FightAgainst(suspect);
                Game.DisplaySubtitle("Investigate the domestic violence report and neutralize the threat.", 5000);
            }

            if (victim != null && victim.IsDead)
            {
                Game.DisplaySubtitle("The victim is dead. Neutralize the suspect.", 5000);
            }
        }

        public override void End()
        {
            base.End();
            if (blip_suspect.Exists()) blip_suspect.Delete();
            if (blip_victim.Exists()) blip_victim.Delete();
            if (suspect.Exists())
            {
                suspect.Dismiss();
            }
        }
    }
}
