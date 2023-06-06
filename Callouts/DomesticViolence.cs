using System;
using System.Linq;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using CalloutInterfaceAPI;

namespace Code_Blue_Calls.Callouts
{
    [CalloutInfo("Domestic Violence", CalloutProbability.High)]
    [CalloutInterface("Domestic Violence", CalloutProbability.Medium, "Code 3", "LSPD")]
    public class DomesticViolence : Callout
    {
        private Ped victim, suspect;
        private Blip blip_suspect;
        private Blip blip_victim;
        private Vector3 location_suspect;
        private Vector3 location_victim;
        private bool victimFleeing;
        private bool suspectKillingPlayer;
        private bool shotAtVictim;

        public override bool OnBeforeCalloutDisplayed()
        {
            // Set the locations for the suspect and victim
            location_suspect = new Vector3(-111.19f, -8.28f, 70.52f);
            location_victim = new Vector3(-111.19f, -11f, 70.52f);

            // Show the callout area blip before accepting the callout
            ShowCalloutAreaBlipBeforeAccepting(location_suspect, 30f);

            // Set the callout message and position
            CalloutMessage = "Domestic Violence";
            CalloutPosition = location_suspect;

            // Play scanner audio to announce the callout
            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_RESISTING_ARREST_02 IN_OR_ON_POSITION", location_suspect);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            // Create relationship groups for the suspect and victim
            new RelationshipGroup("Attacker");
            new RelationshipGroup("Victims");

            // Create the suspect and set properties
            suspect = new Ped(location_suspect);
            suspect.IsPersistent = true;
            suspect.BlockPermanentEvents = true;
            suspect.RelationshipGroup = "Attacker";
            suspect.Inventory.GiveNewWeapon("WEAPON_NAVYREVOLVER", 1, true);

            // Create the victim and set properties
            victim = new Ped(location_victim);
            victim.IsPersistent = true;
            victim.BlockPermanentEvents = true;
            victim.RelationshipGroup = "Victims";

            // Set the relationship between the suspect and victim to hate
            Game.SetRelationshipBetweenRelationshipGroups("Attacker", "Victims", Relationship.Hate);

            // Attach blips to the suspect and victim
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

            // If the player is near the suspect and hasn't shot at the victim yet, make the suspect fight the victim
            if (Game.LocalPlayer.Character.DistanceTo(suspect) < 40f && !shotAtVictim && !suspectKillingPlayer && !victim.IsDead)
            {
                suspect.Tasks.FightAgainst(victim);
                shotAtVictim = true;
            }

            // If the player is near the suspect and hasn't been attacked by the suspect yet, make the suspect attack the player
            if (Game.LocalPlayer.Character.DistanceTo(suspect) < 15f && !suspectKillingPlayer)
            {
                suspect.Tasks.Clear();
                suspect.Inventory.EquippedWeapon.Ammo = 6;
                suspectKillingPlayer = true;
                suspect.Tasks.FightAgainst(Game.LocalPlayer.Character);
            }

            // End the callout if the suspect is dead or cuffed
            if (suspect.IsDead || suspect.IsCuffed)
            {
                End();
            }

            // If the suspect is still alive and the victim is null, create a new victim and make them fight the suspect
            if (suspect.Exists() && victim == null)
            {
                victim = new Ped(location_victim);
                victim.Tasks.FightAgainst(suspect);
                Game.DisplaySubtitle("Investigate the domestic violence report and neutralize the threat.", 5000);
            }

            // If the victim is dead, display a message to neutralize the suspect and end the callout
            if (victim != null && victim.IsDead)
            {
                Game.DisplaySubtitle("The victim is dead. Neutralize the suspect.", 5000);
                End();
            }

            // Randomly determine the outcome of the callout
            Random random = new Random();
            int outcome = random.Next(1, 101);

            if (outcome <= 20)
            {
                // AG kills VI
                if (!victim.IsDead)
                {
                    suspect.Tasks.FightAgainst(victim);
                }
            }
            else if (outcome <= 30)
            {
                // AG kills VI and PL
                if (!victim.IsDead)
                {
                    suspect.Tasks.FightAgainst(victim);
                }
                if (!Game.LocalPlayer.Character.IsDead)
                {
                    suspect.Tasks.FightAgainst(Game.LocalPlayer.Character);
                }
            }
            else if (outcome <= 35)
            {
                // AG kills VI and commits suicide
                if (!victim.IsDead)
                {
                    suspect.Tasks.FightAgainst(victim);
                }
                if (!suspect.IsDead)
                {
                    suspect.Kill();
                }
            }
            else if (outcome <= 50)
            {
                // AG kills PL
                if (!Game.LocalPlayer.Character.IsDead)
                {
                    suspect.Tasks.FightAgainst(Game.LocalPlayer.Character);
                }
            }
            else if (outcome <= 60)
            {
                // AG takes VI hostage
                if (!victim.IsDead)
                {
                    suspect.Tasks.AimWeaponAt(victim, 999999);
                }
            }
            else if (outcome <= 80)
            {
                // AG flees
                if (!suspect.IsDead)
                {
                    suspect.Tasks.ReactAndFlee(Game.LocalPlayer.Character);
                }
            }
            else
            {
                // AG is drunk and noncompliant, eventually flees on foot
                if (!suspect.IsDead)
                {
                    suspect.Tasks.Wander();
                }
            }
        }

        public override void End()
        {
            base.End();

            // Delete the suspect and victim blips if they exist
            if (blip_suspect.Exists())
                blip_suspect.Delete();
            if (blip_victim.Exists())
                blip_victim.Delete();

            // Dismiss the suspect if they exist
            if (suspect.Exists())
            {
                suspect.Dismiss();
            }
        }
    }
}
