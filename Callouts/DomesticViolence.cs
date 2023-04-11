using System;
using System.Linq;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;

namespace Code_Blue_Calls.Callouts
{
    [CalloutInfo("Domestic Violence", CalloutProbability.High)]
    public class DomesticViolence : Callout
    {
        private Ped victim, suspect;
        private Blip blip_suspect;
        private Blip blip_victim;
        private Vector3 location_suspect;
        private Vector3 location_victim;
        private bool Victimfleeing;
        private bool SuspectKillingPlayer;
        private bool ShotAtVictim;
        

        public override bool OnBeforeCalloutDisplayed()
        {
            location_suspect = new Vector3(-111.19f, -8.28f, 70.52f);
            location_victim = new Vector3(-111.19f, -11f, 70.52f);

            ShowCalloutAreaBlipBeforeAccepting(location_suspect, 30f);
            CalloutMessage = "Domestic Violence";
            CalloutPosition = location_suspect;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_RESISTING_ARREST_02 IN_OR_ON_POSITION", location_suspect);
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
