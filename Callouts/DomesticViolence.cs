using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using CalloutInterfaceAPI;
using System.Collections.Generic;
using DialogueSystem;
using CalloutInterface;
using LSPD_First_Response.Engine;
using System.Drawing.Text;
using System.Runtime.ExceptionServices;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Drawing;
using System.Threading;

namespace Code_Blue_Calls.Callouts
{

    [CalloutInterface("Domestic Violence", CalloutProbability.Medium, "Code 3", "LSPD")]
    public class DomesticViolence : Callout
    {
        private House theHouse;
        private Ped victim, suspect;
        //private Blip blip_suspect, blip_victim;
        private List<House> Houses;
        private Random rand = new Random();
        private Vector3 loc_victim, loc_suspect;
        IniFile ini = new IniFile("test.ini");
        int index;
        

        private House GetClosestHouse(List<House> houses)
        {
            Vector3 playerPosition = Game.LocalPlayer.Character.Position;
            House closestHouse = null;
            float closestDistance = float.MaxValue;
                
            foreach (House house in houses)
            {
                float distance = Vector3.Distance(playerPosition, house.Position);
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

        private static int PercentageMagicFuckery(int[] dict)
        {
            int totalChances = dict.Sum();
            Random random = new Random(); // Create a single instance of Random
            int randomNumber = random.Next(1, totalChances + 1);
            int cumulativeSum = 0;
            int selectedIdx = -1;

            for (int i = 0; i < dict.Length; i++)
            {
                cumulativeSum += dict[i];
                if (randomNumber <= cumulativeSum)
                {
                    selectedIdx = i;
                    break;
                }
            }
            return selectedIdx;
        }

        private void CheckAllPedsAndCreateIfMissing()
        {
            if (!victim)
            {
                // Spawn the victim ped
                victim = new Ped(loc_victim);

                // Make the victim persistent to prevent despawning
                victim.IsPersistent = true;

                // Set BlockPermanentEvents to true (behavior may be affected)
                victim.BlockPermanentEvents = true;

                // Make the victim stand still indefinitely
                victim.Tasks.StandStill(-1);
            }
            if (!suspect)
            {
                // Spawn the suspect ped
                suspect = new Ped(loc_suspect);

                // Make the suspect persistent to prevent despawning
                suspect.IsPersistent = true;

                // Set BlockPermanentEvents to true (behavior may be affected)
                suspect.BlockPermanentEvents = true;

                // Make the suspect stand still indefinitely
                suspect.Tasks.StandStill(-1);
            }
        }

        private void killViIfClose()
        {
            // If the player can't hear the gunshot properly just kill the victim
            if (Game.LocalPlayer.Character.DistanceTo(loc_victim) > 500) { suspect.Tasks.Clear(); victim.Kill(); return; }

            // If the player can hear the gunshot make the suspect kill the victim
            suspect.Tasks.FightAgainst(victim);
        }

        private void hateCops()
        {
            suspect.RelationshipGroup.SetRelationshipWith(RelationshipGroup.Cop, Relationship.Hate);
            suspect.RelationshipGroup.SetRelationshipWith(Game.LocalPlayer.Character.RelationshipGroup, Relationship.Hate);
        }


        void suicide()
        {
            int rInt = rand.Next(0, 1);
            if (rInt == 0)
            {
                suspect.Tasks.PlayAnimation(new AnimationDictionary("mp_suicide"), "pill", 5f, AnimationFlags.None); //Animations are scary to me
                                                                                                                     // It will take 30-60s for the pill to take effect
                Thread.Sleep(rand.Next(30000, 60000));
                suspect.Kill();
            }
            else
            {
                if (!suspect.Inventory.HasLoadedWeapon) { suspect.Inventory.GiveNewWeapon("weapon_dbshotgun", 2, true); }

                suspect.Tasks.PlayAnimation(new AnimationDictionary("mp_suicide"), "pistol", 5f, AnimationFlags.None); //Animations are scary to me
                suspect.Kill();
            }
        }

        public override bool OnBeforeCalloutDisplayed()
        { 
            // OMG I just realised i can do this all in a xml. Well too late now
            Houses = new List<House>
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
                },
                new House
                {
                    Name = "Wild oats",
                    Victims = new List<Vector3>
                    {
                        new Vector3(-167.024f, 482.7889f, 137.2653f),
                        new Vector3(-168.7479f, 489.9296f, 137.4435f),
                        new Vector3(-169.5214f, 494.82f, 137.6535f),
                        new Vector3(-174.3784f, 492.1631f, 133.8438f),
                        new Vector3(-165.0503f, 484.7615f, 133.8696f),
                        new Vector3(-171.1398f, 497.537f, 130.0437f),
                        new Vector3(-171.7018f, 491.266f, 130.0436f),
                        new Vector3(-168.6209f, 489.5579f, 133.8721f)
                    },
                    Suspects = new List<Vector3>
                    {
                        new Vector3(-169.5247f, 480.2822f, 137.2442f),
                        new Vector3(-168.0818f, 487.8052f, 137.4435f),
                        new Vector3(-170.4885f, 493.2755f, 137.6535f),
                        new Vector3(-174.7031f, 497.1105f, 133.8438f),
                        new Vector3(-163.6721f, 485.0209f, 133.8696f),
                        new Vector3(-174.7959f, 498.5963f, 130.0383f),
                        new Vector3(-175.4943f, 492.6709f, 130.0436f),
                        new Vector3(-166.7517f, 492.885f, 133.8437f)
                    },
                    Position = new Vector3(-183.7358f, 510.1255f, 135.2205f)
                },
                new House
                {
                    Name = "Normandy drv",
                    Victims = new List<Vector3>
                    {
                        new Vector3(-565.8858f, 656.9946f, 145.8321f),
                        new Vector3(-572.941f, 653.3565f, 145.632f),
                        new Vector3(-570.8762f, 644.0323f, 145.4596f),
                        new Vector3(-572.1926f, 658.8855f, 142.0321f),
                        new Vector3(-573.1517f, 643.7465f, 142.0321f),
                        new Vector3(-571.5641f, 651.9454f, 142.0601f),
                        new Vector3(-571.7731f, 667.8361f, 138.2321f)
                    },
                    Suspects = new List<Vector3>
                    {
                        new Vector3(-569.3209f, 656.2357f, 145.8321f),
                        new Vector3(-573.0806f, 652.2025f, 145.632f),
                        new Vector3(-573.103f, 644.9861f, 145.4596f),
                        new Vector3(-571.3017f, 661.8961f, 142.0321f),
                        new Vector3(-573.0618f, 647.8514f, 142.0321f),
                        new Vector3(-567.8376f, 650.1216f, 142.0323f),
                        new Vector3(-571.7289f, 671.4425f, 138.2321f)
                    },
                    Position = new Vector3(-553.8226f, 666.5585f, 144.6216f)
                },
                new House
                {
                    Name = "Sandy motel",
                    Victims = new List<Vector3>
                    {
                        new Vector3(151.9337f, -1004.217f, -98.99999f)
                    },
                    Suspects = new List<Vector3>
                    {
                        new Vector3(153.0823f, -1006.528f, -98.99999f)
                    },
                    Position = new Vector3(1122.733f, 2647.359f, 37.99636f)
                }
            };

            // Choose the closest house to the player. This isn't really optimal as if the player isn't patrolling and doing radar or some other thing wich involves being in one place
            // Then the player will get the same house again and again
            theHouse = GetClosestHouse(Houses);

            Vector3 SpawnPoint = theHouse.Position;

            AddMaximumDistanceCheck(700f, SpawnPoint);

            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
            CalloutMessage = "Domestic Violence";
            CalloutPosition = SpawnPoint;
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_RESISTING_ARREST_02 IN_OR_ON_POSITION", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        void killVictim()
        {
            if (!suspect.Inventory.HasLoadedWeapon)
            {
                suspect.Inventory.GiveNewWeapon("weapon_snspistol", 15, true);
                suspect.Tasks.Clear();
                suspect.Tasks.FightAgainst(victim);
            }
            else
            {
                suspect.Tasks.Clear();
                suspect.Tasks.FightAgainst(victim);
            }
        }

        public override bool OnCalloutAccepted()
        {
            //In this stage we want to choose the positions of the suspsect and the victim, Also in this stage we might guns to the peds inventories 

            int PositionNumber;
            int MaxNumber;

            bool realistic = bool.Parse(ini.IniReadValue("Callouts", "RealisticChances"));

            //Get the max number of positions
            MaxNumber = theHouse.Suspects.Count;
            //Get zie number in range
            PositionNumber = rand.Next(0, MaxNumber);

            //Get the spawn points of the suspect and victim
            loc_suspect = theHouse.Suspects[PositionNumber];
            loc_victim = theHouse.Victims[PositionNumber];

            // Create the suspect and the victim
            CheckAllPedsAndCreateIfMissing();
            
            int[] chances_unrealistic = { 20, 10, 5, 15, 5, 20, 20, 5 };
            int[] chances_realistic = { 5, 17, 7, 5, 15, 25, 10, 6, 10 };

            // You must be level 100+ to read this.
            // Basically its a if statement inside ()
            index = PercentageMagicFuckery(realistic ? chances_realistic : chances_unrealistic);

            



                // Prepare stuff
                switch (index)
            {
                // Aggressor (AG) kills Victim (VI)
                case 0:
                    killViIfClose();
                    break;
                // AG kills VI and Player (PL)
                case 1:
                    killViIfClose();

                    hateCops();

                    break;
                // AG takes VI hostage
                case 2:
                    suspect.Tasks.Clear();
                    suspect.Tasks.AimWeaponAt(victim, -1);
                    break;
                // AG kills PL
                case 3:
                    hateCops();
                    break;
                //AG kills VI and commits suicide
                case 4:
                    killViIfClose();
                    if (Game.LocalPlayer.Character.DistanceTo(suspect) > 150) { suspect.Kill(); }
                    break;
                //AG flees
                case 5:
                    break;
                //AG is just drunk and noncompliant and eventually flees on foot
                case 6:
                    // Make the suspect drunk
                    suspect.Tasks.PlayAnimation(new AnimationDictionary("move_m@drunk@verydrunk"), "walk", 5f, AnimationFlags.Loop); //Animations are scary to me
                    break;
                //AG Doesnt care about the victim and sets up booby traps
                case 7:
                    break;
                //It was a missunderstanding
                case 8:
                    break;
                //Not a crime
                case 9:
                    break;
                //AG Said he will stop
                case 10:
                    break;
            }




            return base.OnCalloutAccepted();
            
        }

        //All the process related crap

        public override void Process()
        {
            base.Process();

            int chance = rand.Next(0, 100);
            bool realistic = bool.Parse(ini.IniReadValue("Callouts", "RealisticChances"));

            

            switch (index)
            {
                // Aggressor (AG) kills Victim (VI)
                case 0:
                    break;
                // AG kills VI and Player (PL)
                case 1:
                    break;
                // AG takes VI hostage
                case 2:
                    break;
                // AG kills PL
                case 3:
                    break;
                //AG kills VI and commits suicide
                case 4:
                    break;
                //AG flees
                case 5:
                    break;
                //AG is just drunk and noncompliant and eventually flees on foot
                case 6:
                    break;
                //AG Doesnt care about the victim and sets up booby traps
                case 7:
                    break;
                //It was a missunderstanding
                case 8:
                    break;
                //Not a crime
                case 9:
                    break;
                //AG Said he will stop
                case 10:
                    break;
            }






            
        }

        public override void End()
        {
            base.End();

        }
    }
}
