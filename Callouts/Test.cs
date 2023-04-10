using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using System.Drawing;

namespace Code_Blue_Calls.Callouts
{
    [CalloutInfo("Test", CalloutProbability.High)]
    public class Test : Callout
    {
        private Ped suspect;
        private Vehicle vehicle;
        private Blip blip;
        private LHandle Pursuit;
        private Vector3 spawn;
        private bool created;
        public override bool OnBeforeCalloutDisplayed()
        {
            spawn = World.GetRandomPositionOnStreet();
            ShowCalloutAreaBlipBeforeAccepting(spawn, 30f);
            //AddMaximumDistanceCheck(40, spawn);
            CalloutMessage = "Test";
            CalloutPosition = spawn;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_RESISTING_ARREST_02 IN_OR_ON_POSITION", spawn);
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            vehicle = new Vehicle("BLISTA", spawn);
            vehicle.IsPersistent = true;
            suspect = new Ped(vehicle.GetOffsetPositionFront(5f));
            suspect.IsPersistent = true;
            suspect.BlockPermanentEvents = true;
            suspect.WarpIntoVehicle(vehicle, -1);

            blip = suspect.AttachBlip();
            blip.Color = System.Drawing.Color.Yellow;
            blip.IsRouteEnabled = true;

            created = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!created && Game.LocalPlayer.Character.DistanceTo(vehicle) <= 20f)
            {
                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, suspect);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                created = true;
            }
            if (created && !Functions.IsPursuitStillRunning(Pursuit))
            {
                End();
            }

        }
        public override void End()
        {
            base.End();

            if (suspect.Exists())
            {
                suspect.Dismiss();
            }
        }
    }
}
