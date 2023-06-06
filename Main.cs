using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.API;
using System.Reflection;
//using LSPD_First_Response.Mod.Callouts;

namespace Code_Blue_Calls
{
    public class Main : Plugin
    {
        public override void Initialize()
        {
            // Subscribe to the OnOnDutyStateChanged event
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;

            // Log a message to indicate that the callouts have been initialized
            Game.LogTrivial("Code Blue callouts initialized.");

            // Register an event handler for resolving LSPDFR-related assemblies
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LSPDFRResolveEventHandler);
        }

        public override void Finally()
        {
            throw new NotImplementedException();
        }

        // Event handler for the OnOnDutyStateChanged event
        public static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            if (OnDuty)
            {
                // If the player goes on duty, register the callouts
                RegisterCallouts();

                // Display a notification to indicate that the callouts have been loaded
                Game.DisplayNotification("Code Blue Callouts loaded.");
            }
        }

        // Register the callouts
        private static void RegisterCallouts()
        {
            Functions.RegisterCallout(typeof(Callouts.DomesticViolence));
        }

        // Event handler for resolving LSPDFR-related assemblies
        // I stole this code from a tutorial i have no idea how it works but i just know that it checks for lspdfr version. Btw comments are made by chat gpt
        public static Assembly LSPDFRResolveEventHandler(object sender, ResolveEventArgs args)
        {
            // Iterate through all user plugins to find the requested assembly
            foreach (Assembly assembly in Functions.GetAllUserPlugins())
            {
                // Check if the assembly name matches the requested name
                if (args.Name.ToLower().Contains(assembly.GetName().Name.ToLower()))
                {
                    return assembly; // Return the assembly if found
                }
            }
            return null; // Return null if the assembly is not found
        }

        // Check if an LSPDFR plugin is running
        // I stole this code from a tutorial i have no idea how it works but i just know that it checks for lspdfr version. Btw comments are made by chat gpt
        public static bool IsLSPDFRPluginRunning(string Plugin, Version minversion = null)
        {
            
            foreach (Assembly assembly in Functions.GetAllUserPlugins())
            {
                AssemblyName an = assembly.GetName();

                // Check if the plugin name matches the requested name
                if (an.Name.ToLower() == Plugin.ToLower())
                {
                    // Check the version if a minimum version is provided
                    if (minversion == null || an.Version.CompareTo(minversion) >= 0)
                    {
                        return true; // Return true if the plugin is running and meets the minimum version requirement
                    }
                }
            }
            return false; // Return false if the plugin is not found or does not meet the minimum version requirement
        }
    }
}
