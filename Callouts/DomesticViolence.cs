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
                    Victims = new List<Vector3>
                    {
                        new Vector3(344.8771f, -998.5061f, -99.1962f)
                    },
                    Suspects = new List<Vector3>
                    {
                        new Vector3(339.5396f, -997.5002f, -99.1962f)
                    },
                    Position = new Vector3(-1356.278f, -1129.217f, 3.779083f)
                },
                new House
                {
                    Name = "West eclipse towers",
                    Victims = new List<Vector3>
                    {
                        new Vector3(-782.3491f, 329.5226f, 217.0381f),
                        new Vector3(-785.1342f, 342.9378f, 216.8519f),
                        new Vector3(-796.3958f, 338.1474f, 220.4384f),
                        new Vector3(-795.549f, 323.2416f, 217.0381f),
                        new Vector3(-777.6931f, 316.4848f, 85.66267f)
                    },
                    Suspects = new List<Vector3>
                    {
                        new Vector3(-787.7764f, 330.2213f, 217.0383f),
                        new Vector3(-785.4869f, 337.4933f, 216.8385f),
                        new Vector3(-800.6288f, 332.9929f, 220.4384f),
                        new Vector3(-797.6311f, 326.5521f, 217.0381f),
                        new Vector3(-769.764f, 316.3391f, 85.66267f)
                    },
                    Position = new Vector3(-775.9423f, 291.741f, 85.38015f)
                },
                new House
                {
                    Name = "Janitors house",
                    Victims = new List<Vector3>
                    {
                        new Vector3(-111.19f, -11f, 70.52f)
                    },
                    Suspects = new List<Vector3>
                    {
                        new Vector3(-111.19f, -8.28f, 70.52f)
                    },
                    Position = new Vector3(-131.3567f, -32.02588f, 57.84212f)
                },
                new House
                {
                    Name = "4 Integrity way apt",
                    Victims = new List<Vector3>
                    {
                        new Vector3(-8.287368f, -588.9766f, 98.83028f),
                        new Vector3(-20.05867f, -590.1524f, 98.83028f),
                        new Vector3(-7.449115f, -591.1342f, 94.02557f),
                        new Vector3(-22.00957f, -593.6589f, 94.10957f),
                        new Vector3(-22.16828f, -601.7687f, 100.2328f)
                    },
                    Suspects = new List<Vector3>
                    {
                        new Vector3(-8.015527f, -585.1836f, 98.85117f),
                        new Vector3(-16.91024f, -590.4242f, 98.83028f),
                        new Vector3(-13.52914f, -589.8003f, 94.02557f),
                        new Vector3(-18.81809f, -595.7461f, 94.03452f),
                        new Vector3(-24.06141f, -599.5947f, 100.2388f)
                    },
                    Position = new Vector3(-66.24619f, -574.7697f, 36.95814f)
                },
                new House
                {
                    Name = "StrangeWays",
                    Victims = new List<Vector3>
                    {
                        new Vector3(-608.4471f, 46.57162f, 97.40007f),
                        new Vector3(-617.1703f, 51.9048f, 97.59996f),
                        new Vector3(-623.8194f, 52.13329f, 97.59952f),
                        new Vector3(-616.989f, 60.88132f, 98.2f),
                        new Vector3(-594.8115f, 50.29039f, 96.99963f)
                    },
                    Suspects = new List<Vector3>
                    {
                        new Vector3(-609.6661f, 47.69266f, 97.40007f),
                        new Vector3(-611.9868f, 51.139f, 97.63506f),
                        new Vector3(-622.6797f, 55.9524f, 97.5995f),
                        new Vector3(-615.5163f, 57.98928f, 98.19996f),
                        new Vector3(-597.5438f, 49.36379f, 97.03481f)
                    },
                    Position = new Vector3(-615.7661f, 24.44156f, 41.59992f)
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
